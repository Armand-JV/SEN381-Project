import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { login } from "../api/auth";
import type { UserRole } from "../api/types";

export default function LoginPage() {
  const navigate = useNavigate();
  const [userId, setUserId] = useState("staff-1");
  const [role, setRole] = useState<UserRole>("Staff");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  // ponytail: remove this when a real identity provider replaces TestAuthController and demo auth is disabled by default server-side.
  const showDemoOptIn = typeof window !== "undefined" && window.location.hostname === "localhost";
  const [useTestHeaders, setUseTestHeaders] = useState(false);

  const submit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!userId.trim()) {
      setError("Enter a user ID.");
      return;
    }

    setBusy(true);
    setError("");
    try {
      const auth = await login(userId.trim(), role, useTestHeaders);
      if (!auth) {
        setError("Login failed. Enable local demo mode and set DemoAuth__Enabled=true, or configure backend credentials.");
        return;
      }
      window.dispatchEvent(new Event("auth:change"));
      navigate(auth.role === "Requester" ? "/my-requests" : "/staff/queue", { replace: true });
    } catch {
      setError("Login failed. Check backend connection and auth configuration.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="login-page">
      <section className="login-card" aria-labelledby="login-heading">
        <h1 id="login-heading">CivicConnect</h1>
        <p className="subtitle">Municipal service request management</p>
        <form onSubmit={submit}>
          <div className="field">
            <label htmlFor="user-id">User ID</label>
            <input id="user-id" autoComplete="username" value={userId} onChange={(event) => setUserId(event.target.value)} />
          </div>
          <div className="field">
            <label htmlFor="role">Role</label>
            <select id="role" value={role} onChange={(event) => setRole(event.target.value as UserRole)}>
              <option value="Requester">Requester</option>
              <option value="Staff">Staff</option>
              <option value="Manager">Manager</option>
            </select>
          </div>
          {showDemoOptIn && (
            <div className="field">
              <label className="check">
                <input type="checkbox" checked={useTestHeaders} onChange={(event) => setUseTestHeaders(event.target.checked)} />
                Use local demo identity (no credentials verified against backend)
              </label>
            </div>
          )}
          {error && <p className="error" role="alert">{error}</p>}
          <button className="btn-primary" disabled={busy} type="submit">{busy ? "Signing in…" : "Sign in"}</button>
        </form>
        <p className="hint">Local demo requires checkbox opt-in and <code>DemoAuth__Enabled=true</code>. Real signup awaits backend identity support.</p>
        <p><a href="/signup">Create account</a></p>
      </section>
    </div>
  );
}
