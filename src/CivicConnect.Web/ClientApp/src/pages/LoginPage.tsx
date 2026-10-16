import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { login, register } from "../api/auth";
import type { UserRole } from "../api/types";

type Mode = "login" | "register";

const strengthLabels = ["Very weak", "Weak", "Fair", "Good", "Strong"];

function passwordStrength(password: string): number {
  return [password.length >= 8, /[A-Z]/.test(password), /\d/.test(password), /[^A-Za-z0-9]/.test(password)].filter(Boolean).length;
}
export default function LoginPage() {
  const navigate = useNavigate();
  const [mode, setMode] = useState<Mode>("login");
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const role: UserRole = "Requester";
  const [acceptedTerms, setAcceptedTerms] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const strength = passwordStrength(password);

  const selectMode = (nextMode: Mode) => {
    setMode(nextMode);
    setError("");
    setPassword("");
    setConfirmPassword("");
  };

  const submit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError("");

    if (!username.trim() || !password) {
      setError("Enter your username and password.");
      return;
    }
    if (mode === "register") {
      if (!email.trim()) { setError("Enter your email address."); return; }
      if (password !== confirmPassword) { setError("Passwords do not match."); return; }
      if (!acceptedTerms) { setError("Accept terms to create an account."); return; }
    }

    setBusy(true);
    try {
      const auth = mode === "login"
        ? await login(username, password)
        : await register(username, email, password, role);
      window.dispatchEvent(new Event("auth:change"));
      navigate(auth.role === "Requester" ? "/my-requests" : "/staff/queue", { replace: true });
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Authentication failed. Try again.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="login-page">
      <section className="login-card" aria-labelledby="login-heading">
        <h1 id="login-heading">CivicConnect</h1>
        <p className="subtitle">Municipal service request management</p>

        <div className="auth-tabs" role="tablist" aria-label="Account access">
          <button className={mode === "login" ? "tab-btn active" : "tab-btn"} type="button" role="tab" aria-selected={mode === "login"} onClick={() => selectMode("login")}>Sign in</button>
          <button className={mode === "register" ? "tab-btn active" : "tab-btn"} type="button" role="tab" aria-selected={mode === "register"} onClick={() => selectMode("register")}>Create account</button>
        </div>

        <form onSubmit={submit} noValidate>
          <div className="field">
            <label htmlFor="username">Username</label>
            <input id="username" autoComplete="username" required value={username} onChange={(event) => setUsername(event.target.value)} />
          </div>

          {mode === "register" && (
            <div className="field">
              <label htmlFor="email">Email address</label>
              <input id="email" type="email" autoComplete="email" required value={email} onChange={(event) => setEmail(event.target.value)} />
            </div>
          )}

          <div className="field">
            <label htmlFor="password">Password</label>
            <div className="password-field">
              <input id="password" type={showPassword ? "text" : "password"} autoComplete={mode === "login" ? "current-password" : "new-password"} required value={password} onChange={(event) => setPassword(event.target.value)} />
              <button className="pw-toggle" type="button" aria-label={showPassword ? "Hide password" : "Show password"} onClick={() => setShowPassword((visible) => !visible)}>{showPassword ? "Hide" : "Show"}</button>
            </div>
            {mode === "register" && password && (
              <div className="strength" aria-live="polite">
                <div className="strength-bar" aria-hidden="true">{[1, 2, 3, 4].map((level) => <span className={level <= strength ? "strength-fill" : ""} key={level} />)}</div>
                <span className="strength-label">{strengthLabels[strength]}</span>
              </div>
            )}
          </div>

          {mode === "register" && (
            <>
              <div className="field">
                <label htmlFor="confirm-password">Confirm password</label>
                <input id="confirm-password" type={showPassword ? "text" : "password"} autoComplete="new-password" required value={confirmPassword} onChange={(event) => setConfirmPassword(event.target.value)} />
              </div>
              <label className="check terms"><input type="checkbox" checked={acceptedTerms} onChange={(event) => setAcceptedTerms(event.target.checked)} /> I agree to the CivicConnect terms of service.</label>
            </>
          )}

          {error && <p className="error" role="alert">{error}</p>}
          <button className="btn-primary auth-submit" disabled={busy} type="submit">{busy ? (mode === "login" ? "Signing in…" : "Creating account…") : (mode === "login" ? "Sign in" : "Create account")}</button>
        </form>
      </section>
    </div>
  );
}
