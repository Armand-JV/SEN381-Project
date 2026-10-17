import { useEffect, useState } from "react";
import { Link, Outlet, useLocation, useNavigate } from "react-router-dom";
import { getAuth, logout } from "../api/auth";
import api from "../api/client";
import type { PagedResultDto, UserRole } from "../api/types";

const navItems: { path: string; label: string; roles: UserRole[] }[] = [
  { path: "/staff/queue", label: "Staff Queue", roles: ["Staff", "Manager"] },
  { path: "/manager/users", label: "Manage Users", roles: ["Manager"] },
  { path: "/submit", label: "Submit Request", roles: ["Requester"] },
  { path: "/my-requests", label: "My Requests", roles: ["Requester"] },
  { path: "/notifications", label: "Notifications", roles: ["Requester"] },
];

function NotificationCount() {
  const { pathname } = useLocation();
  const [count, setCount] = useState<number | null>(null);

  useEffect(() => {
    let controller: AbortController | undefined;
    const load = async () => {
      controller?.abort();
      controller = new AbortController();
      const { signal } = controller;
      try {
        const response = await api.get<PagedResultDto<unknown>>("/notifications", {
          params: { unreadOnly: true, page: 1, pageSize: 1 }, signal,
        });
        if (!signal.aborted) setCount(response.data.totalCount);
      } catch { if (!signal.aborted) setCount(null); }
    };
    void load();
    window.addEventListener("notifications:change", load);
    window.addEventListener("focus", load);
    return () => {
      controller?.abort();
      window.removeEventListener("notifications:change", load);
      window.removeEventListener("focus", load);
    };
  }, [pathname]);

  return <span className="notification-count" role="status" aria-label={count === null ? "Unread count unavailable" : `${count} unread notifications`}>{count === null ? "—" : count > 99 ? "99+" : count}</span>;
}

export default function Layout() {
  const navigate = useNavigate();
  const location = useLocation();
  const [auth, setAuth] = useState(getAuth());

  useEffect(() => {
    const sync = () => setAuth(getAuth());
    window.addEventListener("auth:change", sync);
    return () => window.removeEventListener("auth:change", sync);
  }, []);

  if (!auth) return <Outlet />;

  const visibleItems = navItems.filter((item) => item.roles.includes(auth.role));
  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="layout">
      <header className="header">
        <Link className="logo" to="/">CivicConnect</Link>
        <nav className="nav" aria-label="Main navigation">
          {visibleItems.map((item) => (
            <Link className={location.pathname === item.path ? "nav-item active" : "nav-item"} key={item.path} to={item.path}>
              {item.label}{item.path === "/notifications" && <NotificationCount />}
            </Link>
          ))}
        </nav>
        <div className="user-info">
          <span>{auth.userId} ({auth.role.toLowerCase()})</span>
          <button className="btn-ghost" onClick={handleLogout}>Logout</button>
        </div>
      </header>
      <main className="main"><Outlet /></main>
    </div>
  );
}
