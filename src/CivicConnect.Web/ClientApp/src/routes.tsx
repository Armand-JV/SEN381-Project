import { Navigate, createBrowserRouter } from "react-router-dom";
import { getAuth } from "./api/auth";
import Layout from "./components/Layout";
import FeedbackPage from "./pages/FeedbackPage";
import LoginPage from "./pages/LoginPage";
import MyRequestsPage from "./pages/MyRequestsPage";
import RequestDetailPage from "./pages/RequestDetailPage";
import SignupPage from "./pages/SignupPage";
import StaffQueuePage from "./pages/StaffQueuePage";
import StatusHistoryPage from "./pages/StatusHistoryPage";
import SubmitRequestPage from "./pages/SubmitRequestPage";

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  return getAuth() ? children : <Navigate replace to="/login" />;
}

function HomeRoute() {
  const auth = getAuth();
  if (!auth) return <Navigate replace to="/login" />;
  return <Navigate replace to={auth.role === "Requester" ? "/my-requests" : "/staff/queue"} />;
}

export const router = createBrowserRouter([
  {
    path: "/",
    element: <Layout />,
    children: [
      { index: true, element: <HomeRoute /> },
      { path: "login", element: <LoginPage /> },
      { path: "signup", element: <SignupPage /> },
      { path: "staff/queue", element: <ProtectedRoute><StaffQueuePage /></ProtectedRoute> },
      { path: "requests/:requestId", element: <ProtectedRoute><RequestDetailPage /></ProtectedRoute> },
      { path: "submit", element: <ProtectedRoute><SubmitRequestPage /></ProtectedRoute> },
      { path: "my-requests", element: <ProtectedRoute><MyRequestsPage /></ProtectedRoute> },
      { path: "requests/:requestId/history", element: <ProtectedRoute><StatusHistoryPage /></ProtectedRoute> },
      { path: "requests/:requestId/feedback", element: <ProtectedRoute><FeedbackPage /></ProtectedRoute> },
    ],
  },
  { path: "*", element: <Navigate replace to="/" /> },
]);
