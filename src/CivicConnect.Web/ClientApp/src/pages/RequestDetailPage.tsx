import { useCallback, useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import api from "../api/client";
import { getAuth } from "../api/auth";
import { requestStateName, requestStateValue } from "../api/types";
import type { Request, RequestStateName } from "../api/types";

const allowedTransitions: Record<RequestStateName, RequestStateName[]> = {
  Open: ["Assigned", "Rejected", "Cancelled"],
  Assigned: ["InProgress", "Rejected", "Cancelled"],
  InProgress: ["Resolved", "Cancelled"],
  Resolved: ["Closed"],
  Closed: [],
  Cancelled: [],
  Rejected: [],
};

export default function RequestDetailPage() {
  const { requestId } = useParams();
  const navigate = useNavigate();
  const [request, setRequest] = useState<Request>();
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [assigneeId, setAssigneeId] = useState("");
  const [reason, setReason] = useState("");
  const [comment, setComment] = useState("");
  const [internal, setInternal] = useState(false);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    if (!requestId) return;
    setLoading(true);
    setError("");
    try {
      const response = await api.get<Request>(`/requests/${requestId}`);
      setRequest(response.data);
    } catch {
      setError("Could not load this request.");
    } finally {
      setLoading(false);
    }
  }, [requestId]);

  useEffect(() => { void load(); }, [load]);

  const action = async (work: () => Promise<unknown>) => {
    setBusy(true);
    setError("");
    try { await work(); await load(); setReason(""); }
    catch (cause) {
      const message = typeof cause === "object" && cause && "response" in cause
        ? (cause as { response?: { data?: string } }).response?.data : null;
      setError(typeof message === "string" ? message : "Action failed. Check transition rules and input.");
    } finally { setBusy(false); }
  };

  const assign = () => {
    if (!assigneeId.trim()) { setError("Enter an assignee ID."); return; }
    void action(() => api.put(`/requests/${requestId}/assign`, { assigneeId: assigneeId.trim(), reason: reason.trim() }));
  };

  const updateState = (newState: RequestStateName) => {
    if ((newState === "Rejected" || newState === "Cancelled") && !reason.trim()) {
      setError(`A reason is required for ${newState}.`);
      return;
    }
    void action(() => api.put(`/requests/${requestId}/state`, { newState: requestStateValue(newState), reason: reason.trim() }));
  };

  const postComment = (event: React.FormEvent) => {
    event.preventDefault();
    if (!comment.trim()) return;
    void action(async () => {
      await api.post(`/requests/${requestId}/comments`, { text: comment.trim(), isInternal: internal });
      setComment("");
      setInternal(false);
    });
  };

  if (loading) return <section className="page"><p className="loading">Loading…</p></section>;
  if (!request) return <section className="page"><p className="error">{error || "Request not found."}</p><button className="btn-link" onClick={() => navigate(-1)}>Back</button></section>;

  const currentState = requestStateName(request.state);
  const canManage = getAuth()?.role === "Staff" || getAuth()?.role === "Manager";
  const nextStates = canManage ? allowedTransitions[currentState] : [];
  const comments = request.comments ?? [];
  const history = request.history ?? [];

  return <section className="page">
    <Link className="back" to="/staff/queue">← Staff queue</Link>
    <div className="page-heading"><div><h1>{request.title}</h1><p className="muted">Request #{request.id} · Submitted {new Date(request.createdAt).toLocaleString()}</p></div><span className={`badge badge-${currentState.toLowerCase()}`}>{currentState}</span></div>
    {error && <p className="error" role="alert">{error}</p>}

    <article className="card"><h2>Request details</h2><dl className="details"><div><dt>Requester</dt><dd>{request.requesterId}</dd></div><div><dt>Category</dt><dd>#{request.categoryId}</dd></div><div><dt>Assignee</dt><dd>{request.assigneeId ?? "Unassigned"}</dd></div></dl><p>{request.description}</p></article>

    {canManage && <section className="card"><h2>Assign request</h2><div className="action-row"><label>Assignee ID<input value={assigneeId} onChange={(event) => setAssigneeId(event.target.value)} placeholder="staff-1" /></label><button className="btn-primary" disabled={busy || currentState !== "Open"} onClick={assign}>Assign</button></div>{currentState !== "Open" && <p className="hint">Only Open requests can be assigned.</p>}</section>}

    {canManage && <section className="card"><h2>Update state</h2><label className="reason-field">Reason <span className="muted">(required for rejection/cancellation)</span><textarea value={reason} onChange={(event) => setReason(event.target.value)} rows={2} /></label><div className="actions">{nextStates.length ? nextStates.map((state) => <button className="btn-secondary" disabled={busy} key={state} onClick={() => updateState(state)}>Mark {state}</button>) : <p className="muted">This request is in a terminal state.</p>}</div></section>}

    <section className="card"><h2>Comments</h2>{comments.length ? <ul className="comments">{comments.map((item) => <li key={item.id}><div><strong>{item.createdBy}</strong> {item.isInternal && <span className="internal-flag">Internal</span>}<span className="muted"> · {new Date(item.createdAt).toLocaleString()}</span></div><p>{item.text}</p></li>)}</ul> : <p className="muted">No comments.</p>}<form onSubmit={postComment} className="comment-form"><label>New comment<textarea required value={comment} onChange={(event) => setComment(event.target.value)} rows={3} /></label><label className="check"><input type="checkbox" checked={internal} onChange={(event) => setInternal(event.target.checked)} /> Internal comment</label><button className="btn-primary" disabled={busy}>Post comment</button></form></section>

    <section className="card"><h2>Status history</h2>{history.length ? <ol className="timeline">{history.map((item) => <li key={item.id}><strong>{item.previousState} → {item.newState}</strong><span className="muted"> · {item.changedBy} · {new Date(item.changedAt).toLocaleString()}</span>{item.reason && <p>Reason: {item.reason}</p>}</li>)}</ol> : <p className="muted">No history entries.</p>}<Link to={`/requests/${request.id}/history`}>Open standalone timeline</Link></section>
  </section>;
}
