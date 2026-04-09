import { Link, NavLink } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import { useCart } from "../../context/CartContext";
import { getDashboardPathForRole, normalizeRole } from "../../utils/auth";
import { brand } from "../../utils/branding";

const roleLinks = {
  Admin: [
    { to: "/dashboard/admin", label: "Dashboard" },
    { to: "/management/menu", label: "Menu" },
    { to: "/management/categories", label: "Categories" },
    { to: "/management/orders", label: "Orders" },
    { to: "/management/employees", label: "Employees" },
    { to: "/management/inventory", label: "Inventory" },
    { to: "/management/reports", label: "Reports" }
  ],
  Manager: [
    { to: "/dashboard/manager", label: "Dashboard" },
    { to: "/management/menu", label: "Menu" },
    { to: "/management/categories", label: "Categories" },
    { to: "/management/orders", label: "Orders" },
    { to: "/management/inventory", label: "Inventory" },
    { to: "/management/reports", label: "Reports" }
  ],
  Cashier: [
    { to: "/dashboard/cashier", label: "Dashboard" },
    { to: "/management/orders", label: "Orders" }
  ],
  Chef: [
    { to: "/dashboard/chef", label: "Dashboard" },
    { to: "/management/orders", label: "Kitchen Orders" }
  ],
  Waiter: [
    { to: "/dashboard/waiter", label: "Dashboard" },
    { to: "/management/orders", label: "Orders" }
  ],
  Customer: [
    { to: "/dashboard/customer", label: "Dashboard" },
    { to: "/menu", label: "Menu" },
    { to: "/cart", label: "Cart" },
    { to: "/place-order", label: "Place Order" },
    { to: "/order-history", label: "Order History" }
  ]
};

export function PublicHeader() {
  const { user, logout } = useAuth();
  const { totalItems, clearCart } = useCart();
  const dashboardPath = getDashboardPathForRole(user?.role);

  const handleLogout = () => {
    clearCart();
    logout();
  };

  return (
    <header className="public-header">
      <nav className="navbar navbar-expand-lg">
        <div className="container py-2">
          <Link className="navbar-brand brand-lockup" to="/">
            <span className="brand-mark">{brand.name}</span>
            <small className="brand-subline">{brand.label}</small>
          </Link>
          <button className="navbar-toggler" data-bs-toggle="collapse" data-bs-target="#mainNav">
            <span className="navbar-toggler-icon" />
          </button>
          <div className="collapse navbar-collapse" id="mainNav">
            <div className="navbar-nav ms-auto align-items-lg-center gap-lg-2">
              <NavLink className="nav-link" to="/">
                Home
              </NavLink>
              <NavLink className="nav-link" to="/menu">
                Menu
              </NavLink>
              {user?.role === "Customer" ? (
                <>
                  <NavLink className="nav-link" to="/cart">
                    Cart ({totalItems})
                  </NavLink>
                  <NavLink className="nav-link" to="/dashboard/customer">
                    Dashboard
                  </NavLink>
                  <button className="btn btn-sm btn-outline-dark ms-lg-2" onClick={handleLogout}>
                    Logout
                  </button>
                </>
              ) : user ? (
                <>
                  <NavLink className="nav-link" to={dashboardPath ?? "/dashboard-router"}>
                    Operations
                  </NavLink>
                  <button className="btn btn-sm btn-outline-dark ms-lg-2" onClick={handleLogout}>
                    Logout
                  </button>
                </>
              ) : (
                <>
                  <NavLink className="nav-link" to="/login">
                    Login
                  </NavLink>
                  <NavLink className="btn btn-sm btn-warning ms-lg-2" to="/register">
                    Get Started
                  </NavLink>
                </>
              )}
            </div>
          </div>
        </div>
      </nav>
    </header>
  );
}

export function DashboardLayout({ children, title, subtitle }) {
  const { user, logout } = useAuth();
  const { clearCart } = useCart();
  const links = roleLinks[normalizeRole(user?.role)] ?? [];

  const handleLogout = () => {
    clearCart();
    logout();
  };

  return (
    <div className="dashboard-shell">
      <aside className="dashboard-sidebar">
        <Link to="/" className="brand-lockup mb-4 d-inline-flex">
          <span className="brand-mark">{brand.name}</span>
          <small className="brand-subline">{brand.label}</small>
        </Link>
        <p className="small text-secondary mb-1">{brand.tagline}</p>
        <p className="small text-secondary mb-4">Role: {user?.role}</p>
        <div className="d-flex flex-column gap-2">
          {links.map((link) => (
            <NavLink key={link.to} className="sidebar-link" to={link.to}>
              {link.label}
            </NavLink>
          ))}
        </div>
        <button className="btn btn-outline-dark mt-auto" onClick={handleLogout}>
          Logout
        </button>
      </aside>
      <main className="dashboard-main">
        <div className="dashboard-topbar">
          <div>
            <h1 className="dashboard-title mb-1">{title}</h1>
            <p className="text-secondary mb-0">{subtitle}</p>
          </div>
          <div className="text-end">
            <strong>{user?.fullName}</strong>
            <div className="small text-secondary">{user?.email}</div>
            <div className="small brand-topline">{brand.name}</div>
          </div>
        </div>
        {children}
      </main>
    </div>
  );
}
