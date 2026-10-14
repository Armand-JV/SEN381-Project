import { Link } from "react-router-dom";

export default function SignupPage() {
  return (
    <section className="auth-card">
      <h1>Create account</h1>
      <p>Account registration is not available yet. Backend identity and signup endpoints are not implemented.</p>
      <p className="muted">Track this work before enabling account creation in the frontend.</p>
      <Link to="/login">Back to sign in</Link>
    </section>
  );
}
