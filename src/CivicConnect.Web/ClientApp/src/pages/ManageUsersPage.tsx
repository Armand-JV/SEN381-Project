import { useState } from "react";
import { provisionUser } from "../api/users";

type Role = "Staff" | "Manager";

export default function ManageUsersPage() {
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirm, setConfirm] = useState("");
  const [role, setRole] = useState<Role>("Staff");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const submit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError("");
    setSuccess("");
    if (password !== confirm) {
      setError("Passwords do not match.");
      return;
    }
    setBusy(true);

    try {
      const user = await provisionUser({ username: username.trim(), email: email.trim(), password, role });
      setSuccess(`Account created for ${user.username} (${user.role}).`);
      setUsername("");
      setEmail("");
      setPassword("");
      setConfirm("");
      setRole("Staff");
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Failed to create account.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <div>
      <h1>Manage Users</h1>
      <p className="subtitle">Create Staff or Manager accounts. Share the account details securely out-of-band.</p>

      <form className="manage-users-form" onSubmit={submit}>
        <div className="field">
          <label htmlFor="username">Username</label>
          <input id="username" autoComplete="username" value={username} onChange={(event) => setUsername(event.target.value)} required />
        </div>

        <div className="field">
          <label htmlFor="email">Email</label>
          <input id="email" type="email" autoComplete="email" value={email} onChange={(event) => setEmail(event.target.value)} required />
        </div>

        <div className="field">
          <label htmlFor="password">Password</label>
          <input id="password" type="password" autoComplete="new-password" value={password} onChange={(event) => setPassword(event.target.value)} required />
        </div>

        <div className="field">
          <label htmlFor="confirm">Confirm password</label>
          <input id="confirm" type="password" autoComplete="new-password" value={confirm} onChange={(event) => setConfirm(event.target.value)} required />
        </div>

        <div className="field">
          <label htmlFor="role">Role</label>
          <select id="role" value={role} onChange={(event) => setRole(event.target.value as Role)}>
            <option value="Staff">Staff</option>
            <option value="Manager">Manager</option>
          </select>
        </div>

        {error && <p className="error" role="alert">{error}</p>}
        {success && <p className="success" role="status">{success}</p>}

        <button type="submit" disabled={busy} className="btn-primary">
          {busy ? "Creating…" : "Create account"}
        </button>
      </form>
    </div>
  );
}
