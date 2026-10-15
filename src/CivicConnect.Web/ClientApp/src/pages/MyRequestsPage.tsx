import { Link } from "react-router-dom";

export default function MyRequestsPage() {
  return <section className="page"><h1>My Requests</h1><div className="stub"><h2>Coming soon</h2><p>Waiting on <a href="https://github.com/Armand-JV/SEN381-Project/issues/18">FR-004</a>: paged <code>GET /api/requests/mine</code>.</p></div><Link to="/">Back</Link></section>;
}
