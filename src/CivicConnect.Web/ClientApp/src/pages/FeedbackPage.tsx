import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../api/client";
import { requestStateName } from "../api/types";
import type { Feedback, ProblemDetails, Request } from "../api/types";

export default function FeedbackPage() {
  const { requestId } = useParams();
  const [request, setRequest] = useState<Request>();
  const [feedback, setFeedback] = useState<Feedback>();
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState("");
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!requestId) return;
    let active = true;
    api.get<Request>(`/requests/${requestId}`)
      .then((response) => { if (active) { setRequest(response.data); setFeedback(response.data.feedback ?? undefined); } })
      .catch(() => { if (active) setError("Could not load this request."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [requestId]);

  const submit = async (event: React.FormEvent) => {
    event.preventDefault(); setBusy(true); setError("");
    try {
      const response = await api.post<Feedback>(`/requests/${requestId}/feedback`, { rating, comment: comment.trim() });
      setFeedback(response.data);
    } catch (cause) {
      const response = (cause as { response?: { data?: ProblemDetails | string } }).response;
      const data = response?.data;
      if (typeof data === "string") setError(data);
      else setError(data?.errors?.rating?.[0] ?? data?.errors?.comment?.[0] ?? data?.detail ?? data?.title ?? "Could not submit feedback.");
    } finally { setBusy(false); }
  };

  const state = request ? requestStateName(request.state) : "Open";
  const eligible = state === "Resolved" || state === "Closed";

  return <section className="page">
    <Link className="back" to={`/requests/${requestId}`}>← Back to request</Link>
    <h1>Feedback for Request #{requestId}</h1>
    {error && <p className="error" role="alert">{error}</p>}
    {loading ? <p className="loading">Loading…</p> : feedback ? <article className="card"><h2>Feedback submitted</h2><p><strong>{feedback.rating} / 5</strong></p>{feedback.comment && <p>{feedback.comment}</p>}<p className="muted">Submitted {new Date(feedback.createdAt).toLocaleString()}</p></article> : !eligible ? <p className="empty">Feedback becomes available after request is resolved or closed.</p> : <form className="card" onSubmit={submit}><label className="field">Rating<select value={rating} onChange={(event) => setRating(Number(event.target.value))}>{[5, 4, 3, 2, 1].map((value) => <option key={value} value={value}>{value} / 5</option>)}</select></label><label className="field">Comment <span className="muted">(optional, 2,000 characters maximum)</span><textarea value={comment} maxLength={2000} rows={5} onChange={(event) => setComment(event.target.value)} /></label><button className="btn-primary" disabled={busy}>{busy ? "Submitting…" : "Submit feedback"}</button></form>}
  </section>;
}
