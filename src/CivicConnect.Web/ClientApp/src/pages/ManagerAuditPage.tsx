import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/client";
import type { PagedResultDto } from "../api/types";

interface AuditLog { id: number; actor: string; action: string; entityType: string; entityId: number; beforeSummary?: string; afterSummary?: string; timestamp: string; correlationId: string; }

export default function ManagerAuditPage() {
  const [result, setResult] = useState<PagedResultDto<AuditLog>>();
  const [entityType, setEntityType] = useState("");
  const [entityId, setEntityId] = useState("");
  const [filters, setFilters] = useState("");
  const [page, setPage] = useState(1);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    setBusy(true); setError("");
    try { setResult((await api.get<PagedResultDto<AuditLog>>(`/audit?page=${page}&pageSize=20&${filters}`)).data); }
    catch { setError("Could not load audit trail."); }
    finally { setBusy(false); }
  }, [page, filters]);
  useEffect(() => { void load(); }, [load]);

  return <section className="page">
    <Link className="back" to="/manager/dashboard">← Dashboard</Link><h1>Audit trail</h1>
    <p className="muted">Current AuditLogs coverage. Legacy assignment AuditEntries remain separate.</p>
    {error && <p className="error" role="alert">{error}</p>}
    <form className="filters card" onSubmit={(event) => { event.preventDefault(); const query = new URLSearchParams(); if (entityType.trim()) query.set("entityType", entityType.trim()); if (entityId) query.set("entityId", entityId); setFilters(query.toString()); setPage(1); }}>
      <label>Entity type<input value={entityType} onChange={(event) => setEntityType(event.target.value)} placeholder="Request" /></label>
      <label>Entity ID<input type="number" min="1" value={entityId} onChange={(event) => setEntityId(event.target.value)} /></label>
      <button className="btn-primary" disabled={busy}>Apply filters</button>
    </form>
    {busy && <p role="status">Loading audit trail…</p>}
    {result && <section className="card"><p>{result.totalCount} entries</p>{result.items.length ? <ul>{result.items.map((item) => <li key={item.id}><p><strong>{item.action}</strong> · {item.entityType} #{item.entityId} · {item.actor} · {new Date(item.timestamp).toLocaleString()}</p><p>Before: {item.beforeSummary || "—"}<br />After: {item.afterSummary || "—"}</p><small>Correlation: {item.correlationId}</small></li>)}</ul> : <p>No matching entries.</p>}
      <div className="pagination"><button disabled={busy || page <= 1} onClick={() => setPage(page - 1)}>Previous</button><span>Page {page} of {Math.max(1, result.totalPages)}</span><button disabled={busy || page >= result.totalPages} onClick={() => setPage(page + 1)}>Next</button></div>
    </section>}
  </section>;
}
