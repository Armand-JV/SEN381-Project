import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../api/client";
import { requestStateName } from "../api/types";
import type { StatusHistory } from "../api/types";

export default function StatusHistoryPage() {
  const { requestId } = useParams();
  const [history, setHistory] = useState<StatusHistory[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!requestId) return;
    let active = true;
    api.get<StatusHistory[]>(`/requests/${requestId}/history`)
      .then((response) => { if (active) setHistory(response.data); })
      .catch((cause) => { if (active) setError((cause as { response?: { status?: number } }).response?.status === 403 ? "You cannot view this request's history." : "Could not load status history."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [requestId]);

  return <section className="page">
    <Link className="back" to={`/requests/${requestId}`}>← Back to request</Link>
    <h1>Request #{requestId} History</h1>
    {error && <p className="error" role="alert">{error}</p>}
    {loading ? <p className="loading">Loading…</p> : history.length === 0 ? <p className="empty">No history entries.</p> : <ol className="timeline card">{history.map((item) => <li key={item.id}><strong>{requestStateName(item.previousState)} → {requestStateName(item.newState)}</strong><div className="muted">{new Date(item.changedAt).toLocaleString()} · {item.changedBy}</div>{item.reason && <p>Reason: {item.reason}</p>}</li>)}</ol>}
  </section>;
}
