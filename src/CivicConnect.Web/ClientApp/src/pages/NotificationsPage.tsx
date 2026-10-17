import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/client";
import type { NotificationDto, PagedResultDto } from "../api/types";

export default function NotificationsPage() {
  const [items, setItems] = useState<NotificationDto[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [unreadOnly, setUnreadOnly] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    setLoading(true); setError("");
    try {
      const response = await api.get<PagedResultDto<NotificationDto>>("/notifications", {
        params: { page, pageSize: 20, unreadOnly },
      });
      setItems(response.data.items);
      setTotalPages(response.data.totalPages);
      setTotalCount(response.data.totalCount);
    } catch {
      setItems([]); setError("Could not load notifications.");
    } finally { setLoading(false); }
  }, [page, unreadOnly]);

  useEffect(() => { void load(); }, [load]);

  const markRead = async (notification: NotificationDto) => {
    if (notification.readAt) return;
    try {
      await api.put(`/notifications/${notification.id}/read`);
      window.dispatchEvent(new Event("notifications:change"));
      await load();
    } catch { setError("Could not mark notification as read."); }
  };

  const changeFilter = (nextUnreadOnly: boolean) => {
    setUnreadOnly(nextUnreadOnly); setPage(1);
  };

  return <section className="page">
    <div className="page-heading">
      <div><h1>Notifications</h1><p className="muted">{totalCount} notification(s){unreadOnly ? " unread" : ""}</p></div>
      <label className="check"><input type="checkbox" checked={unreadOnly} onChange={(event) => changeFilter(event.target.checked)} /> Show unread only</label>
    </div>
    {error && <p className="error" role="alert">{error}</p>}
    {loading ? <p className="loading">Loading…</p> : items.length === 0 ? <div className="card"><p className="empty">No notifications found.</p></div> : <div className="notification-list">
      {items.map((notification) => <article className={`notification-card${notification.readAt ? "" : " notification-unread"}`} key={notification.id}>
        <div>
          <p className="notification-message">{notification.message}</p>
          <p className="muted">{new Date(notification.createdAt).toLocaleString()}</p>
        </div>
        <div className="notification-actions">
          <Link to={`/requests/${notification.requestId}`}>View request</Link>
          {!notification.readAt && <button className="btn-link" onClick={() => void markRead(notification)}>Mark as read</button>}
        </div>
      </article>)}
    </div>}
    {totalPages > 1 && <nav className="pager" aria-label="Notification pagination"><button disabled={page === 1} onClick={() => setPage((value) => value - 1)}>Previous</button><span>Page {page} of {totalPages}</span><button disabled={page === totalPages} onClick={() => setPage((value) => value + 1)}>Next</button></nav>}
  </section>;
}
