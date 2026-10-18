import { Navigate, createBrowserRouter } from "react-router-dom";
import { getAuth } from "./api/auth";
import Layout from "./components/Layout";
import FeedbackPage from "./pages/FeedbackPage";
import LoginPage from "./pages/LoginPage";
import ManageUsersPage from "./pages/ManageUsersPage";
import ManagerAuditPage from "./pages/ManagerAuditPage";
import ManagerCategoriesPage from "./pages/ManagerCategoriesPage";
import ManagerDashboardPage from "./pages/ManagerDashboardPage";
import MyRequestsPage from "./pages/MyRequestsPage";
import NotificationsPage from "./pages/NotificationsPage";
import RequestDetailPage from "./pages/RequestDetailPage";
import StaffQueuePage from "./pages/StaffQueuePage";
import StatusHistoryPage from "./pages/StatusHistoryPage";
import SubmitRequestPage from "./pages/SubmitRequestPage";

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  return getAuth() ? children : <Navigate replace to="/login" />;
}

function ManagerRoute({ children }: { children: React.ReactNode }) {
  return getAuth()?.role === "Manager" ? children : <Navigate replace to="/" />;
}

function RequesterRoute({ children }: { children: React.ReactNode }) {
  return getAuth()?.role === "Requester" ? children : <Navigate replace to="/" />;
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
      { path: "staff/queue", element: <ProtectedRoute><StaffQueuePage /></ProtectedRoute> },
      { path: "manager/dashboard", element: <ManagerRoute><ManagerDashboardPage /></ManagerRoute> },
      { path: "manager/categories", element: <ManagerRoute><ManagerCategoriesPage /></ManagerRoute> },
      { path: "manager/audit", element: <ManagerRoute><ManagerAuditPage /></ManagerRoute> },
      { path: "manager/users", element: <ManagerRoute><ManageUsersPage /></ManagerRoute> },
      { path: "requests/:requestId", element: <ProtectedRoute><RequestDetailPage /></ProtectedRoute> },
      { path: "submit", element: <ProtectedRoute><SubmitRequestPage /></ProtectedRoute> },
      { path: "my-requests", element: <ProtectedRoute><MyRequestsPage /></ProtectedRoute> },
      { path: "notifications", element: <RequesterRoute><NotificationsPage /></RequesterRoute> },
      { path: "requests/:requestId/history", element: <ProtectedRoute><StatusHistoryPage /></ProtectedRoute> },
      { path: "requests/:requestId/feedback", element: <ProtectedRoute><FeedbackPage /></ProtectedRoute> },
    ],
  },
  { path: "*", element: <Navigate replace to="/" /> },
]);
