import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/client";
import { requestStateName, type RequestState, type RequestStateName } from "../api/types";

interface Summary { total: number; byState: Record<string, number>; byCategory: Record<string, number>; }
interface ManagedRequest { id: number; title: string; state: RequestState; createdAt: string; assigneeId?: string | null; daysOld: number; isOverdue: boolean; }
interface Category { id: number; name: string; description: string; }

const states: RequestStateName[] = ["Open", "Assigned", "InProgress", "Resolved", "Closed", "Cancelled", "Rejected"];

export default function ManagerDashboardPage() {
  const [summary, setSummary] = useState<Summary>();
  const [stalled, setStalled] = useState<ManagedRequest[]>([]);
  const [overdue, setOverdue] = useState<ManagedRequest[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [state, setState] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [assigneeId, setAssigneeId] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  const load = async () => {
    setBusy(true); setError("");
    try {
      const query = new URLSearchParams();
      if (state) query.set("state", String(states.indexOf(state as RequestStateName)));
      if (categoryId) query.set("categoryId", categoryId);
      if (assigneeId.trim()) query.set("assigneeId", assigneeId.trim());
      if (from) query.set("from", new Date(from).toISOString());
      if (to) query.set("to", new Date(to).toISOString());
      const [summaryResponse, stalledResponse, overdueResponse, categoryResponse] = await Promise.all([
        api.get<Summary>(`/reports/requests-summary?${query}`),
        api.get<ManagedRequest[]>("/management/stalled?olderThanDays=7"),
        api.get<ManagedRequest[]>("/management/overdue"),
        api.get<Category[]>("/categories"),
      ]);
      setSummary(summaryResponse.data); setStalled(stalledResponse.data); setOverdue(overdueResponse.data); setCategories(categoryResponse.data);
    } catch { setError("Could not load management data."); }
    finally { setBusy(false); }
  };

  useEffect(() => { void load(); }, []);

  return <section className="page">
    <div className="page-heading"><div><h1>Management dashboard</h1><p className="muted">Request summary and service-age oversight.</p></div><nav aria-label="Management"><Link to="/manager/categories">Categories</Link> · <Link to="/manager/audit">Audit trail</Link> · <Link to="/manager/users">Users</Link></nav></div>
    {error && <p className="error" role="alert">{error}</p>}
    <section className="card"><h2>Summary filters</h2><form onSubmit={(event) => { event.preventDefault(); void load(); }} className="filters">
      <label>State<select value={state} onChange={(event) => setState(event.target.value)}><option value="">All states</option>{states.map((item) => <option key={item}>{item}</option>)}</select></label>
      <label>Category<select value={categoryId} onChange={(event) => setCategoryId(event.target.value)}><option value="">All categories</option>{categories.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
      <label>Assignee ID<input value={assigneeId} onChange={(event) => setAssigneeId(event.target.value)} /></label>
      <label>Created from<input type="date" value={from} onChange={(event) => setFrom(event.target.value)} /></label>
      <label>Created to<input type="date" value={to} onChange={(event) => setTo(event.target.value)} /></label>
      <button className="btn-primary" disabled={busy}>Apply filters</button>
    </form></section>
    {busy && <p className="loading" role="status">Loading management data…</p>}
    {summary && <section className="card"><h2>Request summary</h2><p><strong>{summary.total}</strong> matching requests</p><h3>By state</h3><ul>{Object.entries(summary.byState).map(([key, count]) => <li key={key}>{key}: {count}</li>)}</ul><h3>By category</h3><ul>{Object.entries(summary.byCategory).map(([key, count]) => <li key={key}>{categories.find((item) => item.id === Number(key))?.name ?? `Category ${key}`}: {count}</li>)}</ul></section>}
    <section className="card"><h2>Overdue requests <span className="badge">{overdue.length}</span></h2><p className="muted">Open, assigned, or in-progress requests older than seven UTC calendar days.</p>{overdue.length ? <RequestList items={overdue} /> : <p>No overdue requests.</p>}</section>
    <section className="card"><h2>Stalled requests <span className="badge">{stalled.length}</span></h2><p className="muted">Open or assigned requests aged at least seven days.</p>{stalled.length ? <RequestList items={stalled} /> : <p>No stalled requests.</p>}</section>
  </section>;
}

function RequestList({ items }: { items: ManagedRequest[] }) {
  return <ul>{items.map((item) => <li key={item.id}><Link to={`/requests/${item.id}`}>#{item.id} — {item.title}</Link> · {requestStateName(item.state)} · {item.daysOld} days old{item.isOverdue && " · Overdue"} · {item.assigneeId ?? "Unassigned"}</li>)}</ul>;
}
