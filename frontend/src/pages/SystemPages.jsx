import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { getDashboardPathForRole } from "../utils/auth";
import { brand } from "../utils/branding";

export function DashboardRouterPage() {
  const { user, loading } = useAuth();

  if (loading) {
    return (
      <div className="centered-shell">
        <div className="spinner-border text-warning" role="status" />
      </div>
    );
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  const dashboardPath = getDashboardPathForRole(user.role);
  return <Navigate to={dashboardPath ?? "/unauthorized"} replace />;
}

export function UnauthorizedPage() {
  return (
    <div className="centered-shell px-3">
      <div className="auth-card text-center">
        <h1 className="section-title mb-3">Access Restricted</h1>
        <p className="text-secondary mb-0">Your current role does not have permission to open this {brand.name} page.</p>
      </div>
    </div>
  );
}

export function NotFoundPage() {
  return (
    <div className="centered-shell px-3">
      <div className="auth-card text-center">
        <h1 className="section-title mb-3">Page Not Found</h1>
        <p className="text-secondary mb-0">The route you requested is not part of the {brand.name} workspace.</p>
      </div>
    </div>
  );
}
