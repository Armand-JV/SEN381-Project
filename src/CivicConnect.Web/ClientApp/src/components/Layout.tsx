import { useEffect, useState } from "react";
import { Link, Outlet, useLocation, useNavigate } from "react-router-dom";
import { getAuth, logout } from "../api/auth";
import type { UserRole } from "../api/types";

const navItems: { path: string; label: string; roles: UserRole[] }[] = [
  { path: "/staff/queue", label: "Staff Queue", roles: ["Staff", "Manager"] },
  { path: "/submit", label: "Submit Request", roles: ["Requester"] },
  { path: "/my-requests", label: "My Requests", roles: ["Requester"] },
];

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
              {item.label}
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
