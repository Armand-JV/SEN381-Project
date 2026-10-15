import { Link, useParams } from "react-router-dom";

export default function FeedbackPage() {
  const { requestId } = useParams();
  return <section className="page"><h1>Feedback for Request #{requestId}</h1><div className="stub"><h2>Coming soon</h2><p>Waiting on <a href="https://github.com/Armand-JV/SEN381-Project/issues/20">FR-006</a>: Feedback entity and submission API.</p></div><Link to={`/requests/${requestId}`}>Back to request</Link></section>;
}
