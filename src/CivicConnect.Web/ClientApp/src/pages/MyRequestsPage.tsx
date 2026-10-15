import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/client";
import { requestStateName } from "../api/types";
import type { PagedResultDto, RequesterRequestListItemDto } from "../api/types";

export default function MyRequestsPage() {
  const [items, setItems] = useState<RequesterRequestListItemDto[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    setLoading(true); setError("");
    try {
      const response = await api.get<PagedResultDto<RequesterRequestListItemDto>>("/requests/mine", { params: { page, pageSize: 20 } });
      setItems(response.data.items); setTotalPages(response.data.totalPages); setTotalCount(response.data.totalCount);
    } catch { setItems([]); setError("Could not load your requests."); }
    finally { setLoading(false); }
  }, [page]);

  useEffect(() => { void load(); }, [load]);

  return <section className="page">
    <div className="page-heading"><div><h1>My Requests</h1><p className="muted">{totalCount} request(s) submitted by you</p></div><Link className="btn-primary" to="/submit">New request</Link></div>
    {error && <p className="error" role="alert">{error}</p>}
    {loading ? <p className="loading">Loading…</p> : items.length === 0 ? <div className="card"><p className="empty">You have not submitted any requests.</p><Link to="/submit">Submit your first request</Link></div> : <div className="table-scroll"><table className="table">
      <thead><tr><th>Reference</th><th>Title</th><th>Category</th><th>State</th><th>Submitted</th><th>Actions</th></tr></thead>
      <tbody>{items.map((item) => { const state = requestStateName(item.state); return <tr key={item.id}><td>#{item.id}</td><td>{item.title}</td><td>{item.categoryName}</td><td><span className={`badge badge-${state.toLowerCase()}`}>{item.stateName || state}</span></td><td>{new Date(item.createdAt).toLocaleDateString()}</td><td><Link to={`/requests/${item.id}`}>View</Link> · <Link to={`/requests/${item.id}/history`}>History</Link>{(state === "Resolved" || state === "Closed") && <>&nbsp;· <Link to={`/requests/${item.id}/feedback`}>Feedback</Link></>}</td></tr>; })}</tbody>
    </table></div>}
    {totalPages > 1 && <nav className="pager" aria-label="Request pagination"><button disabled={page === 1} onClick={() => setPage((value) => value - 1)}>Previous</button><span>Page {page} of {totalPages}</span><button disabled={page === totalPages} onClick={() => setPage((value) => value + 1)}>Next</button></nav>}
  </section>;
}
