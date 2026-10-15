import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/client";
import { requestStateName } from "../api/types";
import type { PagedResultDto, RequestStateName, StaffRequestListItemDto } from "../api/types";

const states: (RequestStateName | "")[] = ["", "Open", "Assigned", "InProgress", "Resolved", "Closed", "Cancelled", "Rejected"];
const categories = ["Infrastructure", "Utilities", "Parks"];

export default function StaffQueuePage() {
  const [items, setItems] = useState<StaffRequestListItemDto[]>([]);
  const [state, setState] = useState<RequestStateName | "">("");
  const [categoryId, setCategoryId] = useState("");
  const [assigneeId, setAssigneeId] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const response = await api.get<PagedResultDto<StaffRequestListItemDto>>("/staff/requests", {
        params: {
          state: state || undefined,
          categoryId: categoryId || undefined,
          assigneeId: assigneeId.trim() || undefined,
          page,
          pageSize: 20,
        },
      });
      setItems(response.data.items);
      setTotalPages(response.data.totalPages);
      setTotalCount(response.data.totalCount);
    } catch {
      setError("Could not load staff queue.");
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, [assigneeId, categoryId, page, state]);

  useEffect(() => { void load(); }, [load]);

  return (
    <section className="page">
      <div className="page-heading">
        <div><h1>Staff Request Queue</h1><p className="muted">{totalCount} request(s)</p></div>
        <button className="btn-secondary" onClick={() => void load()}>Refresh</button>
      </div>
      <div className="filters" aria-label="Queue filters">
        <label>State<select value={state} onChange={(event) => { setState(event.target.value as RequestStateName | ""); setPage(1); }}>
          {states.map((value) => <option key={value || "all"} value={value}>{value || "All states"}</option>)}
        </select></label>
        <label>Category<select value={categoryId} onChange={(event) => { setCategoryId(event.target.value); setPage(1); }}>
          <option value="">All categories</option>
          {categories.map((category, index) => <option key={category} value={index + 1}>{category}</option>)}
        </select></label>
        <label>Assignee<input value={assigneeId} onChange={(event) => { setAssigneeId(event.target.value); setPage(1); }} placeholder="staff-1" /></label>
      </div>
      {error && <p className="error" role="alert">{error}</p>}
      {loading ? <p className="loading">Loading…</p> : items.length === 0 ? <p className="empty">No requests match these filters.</p> : (
        <div className="table-scroll"><table className="table">
          <thead><tr><th>ID</th><th>Title</th><th>Category</th><th>State</th><th>Requester</th><th>Assignee</th><th>Created</th><th></th></tr></thead>
          <tbody>{items.map((item) => <tr key={item.id}>
            <td>{item.id}</td><td>{item.title}</td><td>{item.categoryName}</td>
            <td><span className={`badge badge-${requestStateName(item.state).toLowerCase()}`}>{requestStateName(item.state)}</span></td>
            <td>{item.requesterId}</td><td>{item.assigneeId ?? "—"}</td><td>{new Date(item.createdAt).toLocaleDateString()}</td>
            <td><Link to={`/requests/${item.id}`}>View</Link></td>
          </tr>)}</tbody>
        </table></div>
      )}
      {totalPages > 1 && <nav className="pager" aria-label="Queue pagination">
        <button disabled={page === 1} onClick={() => setPage((value) => value - 1)}>Previous</button>
        <span>Page {page} of {totalPages}</span>
        <button disabled={page === totalPages} onClick={() => setPage((value) => value + 1)}>Next</button>
      </nav>}
    </section>
  );
}
