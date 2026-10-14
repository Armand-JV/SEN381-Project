import { Link, useParams } from "react-router-dom";

export default function StatusHistoryPage() {
  const { requestId } = useParams();
  return <section className="page"><h1>Request #{requestId} History</h1><div className="stub"><h2>Coming soon</h2><p>Waiting on <a href="https://github.com/Armand-JV/SEN381-Project/issues/19">FR-005</a> for a dedicated requester history endpoint. Staff can view inline history on the detail page today.</p></div><Link to={`/requests/${requestId}`}>Back to request</Link></section>;
}
