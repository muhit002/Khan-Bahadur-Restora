import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import api, { apiRequest } from "../api/client";
import { useAuth } from "../context/AuthContext";
import { useCart } from "../context/CartContext";
import { useToast } from "../context/ToastContext";
import {
  EmptyState,
  ErrorState,
  ImageWithFallback,
  LoadingState,
  PasswordField,
  SectionHeading
} from "../components/common/Ui";
import { useLiveData } from "../hooks/useLiveData";
import { getDashboardPathForRole } from "../utils/auth";
import { brand, dashboardBanners, getCategoryFallback, getMenuFallback } from "../utils/branding";
import { formatCurrency, resolveImageUrl } from "../utils/format";

function QuantityStepper({ value, onChange, disabled = false }) {
  return (
    <div className="quantity-stepper">
      <button type="button" onClick={() => onChange(Math.max(value - 1, 1))} disabled={disabled}>
        -
      </button>
      <span>{value}</span>
      <button type="button" onClick={() => onChange(value + 1)} disabled={disabled}>
        +
      </button>
    </div>
  );
}

function MenuShowcaseCard({ item, quantity = 1, onQuantityChange, onAddToCart, user }) {
  const fallbackSrc = resolveImageUrl(getMenuFallback(item));
  const displaySrc = item.imageUrl ? resolveImageUrl(item.imageUrl) : fallbackSrc;
  const buttonLabel = !user
    ? "Login to Add to Cart"
    : user.role !== "Customer"
      ? "Customer Accounts Only"
      : `Add to Cart - ${formatCurrency((item.finalPrice ?? 0) * quantity)}`;

  return (
    <article className="menu-card">
      <ImageWithFallback
        className="menu-card-image"
        src={displaySrc}
        fallbackSrc={fallbackSrc}
        alt={item.name}
      />
      <div className="menu-card-body">
        <div className="d-flex justify-content-between align-items-start gap-3">
          <div>
            <h5 className="mb-1">{item.name}</h5>
            <div className="small text-secondary">{item.categoryName}</div>
          </div>
          <div className="text-end">
            <strong>{formatCurrency(item.finalPrice)}</strong>
            {item.discountPercentage > 0 ? (
              <div className="small text-decoration-line-through text-secondary">
                {formatCurrency(item.price)}
              </div>
            ) : null}
          </div>
        </div>
        <div className="menu-meta">
          <span className="catalog-chip">{item.preparationTimeMinutes} min</span>
          <span className={`catalog-chip ${item.isAvailable ? "" : "text-secondary"}`}>
            {item.isAvailable ? "Available" : "Unavailable"}
          </span>
        </div>
        <p className="text-secondary small mb-0">{item.description}</p>
        {typeof onAddToCart === "function" ? (
          <div className="menu-actions">
            <QuantityStepper
              value={quantity}
              onChange={onQuantityChange}
              disabled={!item.isAvailable}
            />
            <button
              type="button"
              className="add-cart-button"
              onClick={() => onAddToCart(item)}
              disabled={!item.isAvailable || (user && user.role !== "Customer")}
            >
              {buttonLabel}
            </button>
          </div>
        ) : null}
      </div>
    </article>
  );
}

function CategoryShowcaseCard({ category }) {
  const fallbackSrc = resolveImageUrl(getCategoryFallback(category.name));
  const displaySrc = category.imageUrl ? resolveImageUrl(category.imageUrl) : fallbackSrc;

  return (
    <div className="glass-panel p-4 h-100 category-showcase">
      <ImageWithFallback
        src={displaySrc}
        fallbackSrc={fallbackSrc}
        alt={category.name}
        className="category-showcase-image"
      />
      <small className="text-uppercase text-secondary">Category</small>
      <h4 className="mt-2">{category.name}</h4>
      <p className="text-secondary mb-3">
        {category.description || "A curated station from our daily kitchen line."}
      </p>
      <span className="catalog-chip">{category.menuItemsCount} dishes</span>
    </div>
  );
}

export function HomePage() {
  const { data, loading, error, reload } = useLiveData(
    async () => {
      const [categories, menuData] = await Promise.all([
        apiRequest(api.get("/api/categories/active"), "Failed to load categories"),
        apiRequest(api.get("/api/menuitems?pageSize=6"), "Failed to load menu items")
      ]);

      return {
        categories,
        featuredItems: menuData.items ?? []
      };
    },
    [],
    { initialData: { categories: [], featuredItems: [] } }
  );

  const categories = data?.categories ?? [];
  const featuredItems = data?.featuredItems ?? [];

  return (
    <>
      <section className="hero-banner">
        <div className="container">
          <div className="row g-4 align-items-center">
            <div className="col-lg-6">
              <div className="hero-card">
                <span className="hero-badge mb-3">{brand.label}</span>
                <h1 className="hero-title mb-3">{brand.name}</h1>
                <p className="lead text-secondary section-copy mb-4">
                  {brand.promise} Order intake, table control, payment tracking, reporting, and customer
                  ordering now stay in one cleaner flow.
                </p>
                <div className="d-flex flex-wrap gap-3">
                  <Link to="/menu" className="btn btn-warning btn-lg">
                    Browse Today&apos;s Menu
                  </Link>
                  <Link to="/register" className="btn btn-outline-dark btn-lg">
                    Open Customer Account
                  </Link>
                </div>
              </div>
            </div>
            <div className="col-lg-6">
              <div className="glass-panel p-4 hero-visual">
                <ImageWithFallback
                  src={resolveImageUrl(dashboardBanners.home)}
                  fallbackSrc={resolveImageUrl(dashboardBanners.home)}
                  alt={brand.name}
                  className="hero-panel-image"
                />
                <div className="feature-stack mt-4">
                  {[
                    "Customer cart and ordering with live totals",
                    "Manager, admin, cashier, waiter, and chef dashboards",
                    "Payment recording, sales reports, and order tracking"
                  ].map((feature) => (
                    <div key={feature} className="feature-chip">
                      {feature}
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="pb-5">
        <div className="container">
          <SectionHeading
            title="Kitchen Categories"
            subtitle="Every menu group is organized for both service staff and customers."
          />
          {loading && categories.length === 0 ? (
            <LoadingState text="Loading featured kitchen categories..." />
          ) : error && categories.length === 0 ? (
            <ErrorState
              title="Categories unavailable"
              description={error.message}
              action={<button className="btn btn-warning" onClick={() => void reload()}>Try Again</button>}
            />
          ) : (
            <>
              {error ? (
                <div className="alert alert-warning">
                  {error.message}. Showing the most recent loaded categories.
                </div>
              ) : null}
              <div className="row g-4">
                {categories.map((category) => (
                  <div className="col-md-6 col-xl-3" key={category.id}>
                    <CategoryShowcaseCard category={category} />
                  </div>
                ))}
              </div>
            </>
          )}
        </div>
      </section>

      <section className="pb-5">
        <div className="container">
          <SectionHeading
            title="Featured Dishes"
            subtitle="A quick look at the dishes customers can order right now."
          />
          {loading && featuredItems.length === 0 ? (
            <LoadingState />
          ) : error && featuredItems.length === 0 ? (
            <ErrorState
              title="Menu unavailable"
              description={error.message}
              action={<button className="btn btn-warning" onClick={() => void reload()}>Try Again</button>}
            />
          ) : featuredItems.length === 0 ? (
            <EmptyState
              title="No featured dishes yet"
              description="Add menu items from the management workspace and they will appear here."
            />
          ) : (
            <div className="menu-grid">
              {featuredItems.map((item) => (
                <MenuShowcaseCard key={item.id} item={item} user={null} />
              ))}
            </div>
          )}
        </div>
      </section>
    </>
  );
}

export function MenuPage() {
  const [filters, setFilters] = useState({ search: "", categoryId: "" });
  const [quantities, setQuantities] = useState({});
  const { addToCart } = useCart();
  const { user } = useAuth();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const {
    data: categories = [],
    loading: categoryLoading,
    error: categoryError
  } = useLiveData(
    () => apiRequest(api.get("/api/categories/active"), "Failed to load categories"),
    [],
    { initialData: [] }
  );

  const {
    data: menuItems = [],
    loading,
    error,
    reload
  } = useLiveData(
    async () => {
      const params = new URLSearchParams({ pageSize: "24" });
      if (filters.search) {
        params.set("search", filters.search);
      }
      if (filters.categoryId) {
        params.set("categoryId", filters.categoryId);
      }

      const data = await apiRequest(api.get(`/api/menuitems?${params.toString()}`), "Failed to load menu items");
      return data.items ?? [];
    },
    [filters.search, filters.categoryId],
    { initialData: [] }
  );

  const handleAddToCart = (item) => {
    if (!user) {
      showToast("Please sign in with a customer account to add dishes to your cart.", "error");
      navigate("/login", { state: { from: "/menu" } });
      return;
    }

    if (user.role !== "Customer") {
      showToast("Only customer accounts can place online menu orders.", "error");
      return;
    }

    const quantity = Math.max(quantities[item.id] ?? 1, 1);
    addToCart(item, quantity);
    setQuantities((current) => ({ ...current, [item.id]: 1 }));
    showToast(`${quantity} x ${item.name} added to cart.`);
  };

  const updateQuantity = (itemId, nextQuantity) => {
    setQuantities((current) => ({
      ...current,
      [itemId]: Math.max(Number(nextQuantity) || 1, 1)
    }));
  };

  return (
    <section className="py-5">
      <div className="container">
        <SectionHeading
          title="Order From the Menu"
          subtitle="Pick dishes, adjust quantity, and send everything to your cart in one clean step."
        />

        <div className="glass-panel p-4 mb-4">
          <div className="row g-3">
            <div className="col-lg-8">
              <input
                className="form-control"
                placeholder="Search by dish name or description"
                value={filters.search}
                onChange={(event) =>
                  setFilters((current) => ({ ...current, search: event.target.value }))
                }
              />
            </div>
            <div className="col-lg-4">
              <select
                className="form-select"
                value={filters.categoryId}
                onChange={(event) =>
                  setFilters((current) => ({ ...current, categoryId: event.target.value }))
                }
                disabled={categoryLoading}
              >
                <option value="">All categories</option>
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
              </select>
            </div>
          </div>
          {categoryError ? <div className="small text-danger mt-3">{categoryError.message}</div> : null}
        </div>

        {loading && menuItems.length === 0 ? (
          <LoadingState />
        ) : error && menuItems.length === 0 ? (
          <ErrorState
            title="Menu unavailable"
            description={error.message}
            action={<button className="btn btn-warning" onClick={() => void reload()}>Try Again</button>}
          />
        ) : menuItems.length === 0 ? (
          <EmptyState
            title="No dishes found"
            description="Try a different search or add menu items from the management workspace."
          />
        ) : (
          <>
            {error ? (
              <div className="alert alert-warning">
                {error.message}. Showing the most recent loaded menu data.
              </div>
            ) : null}
            <div className="menu-grid">
              {menuItems.map((item) => (
                <MenuShowcaseCard
                  key={item.id}
                  item={item}
                  quantity={quantities[item.id] ?? 1}
                  onQuantityChange={(value) => updateQuantity(item.id, value)}
                  onAddToCart={handleAddToCart}
                  user={user}
                />
              ))}
            </div>
          </>
        )}
      </div>
    </section>
  );
}

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { login } = useAuth();
  const { showToast } = useToast();
  const [form, setForm] = useState({ email: "admin@restaurant.com", password: "Admin@123" });
  const [loading, setLoading] = useState(false);
  const redirectTo = location.state?.from ?? "/dashboard-router";

  const handleSubmit = async (event) => {
    event.preventDefault();
    setLoading(true);
    try {
      const result = await login(form);
      const nextPath =
        redirectTo === "/dashboard-router"
          ? getDashboardPathForRole(result.user?.role) ?? redirectTo
          : redirectTo;

      showToast(`Welcome back to ${brand.name}, ${result.user.fullName}.`);
      navigate(nextPath, { replace: true });
    } catch (error) {
      showToast(error.message, "error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="auth-shell">
      <div className="auth-card">
        <span className="hero-badge mb-3">{brand.label}</span>
        <h1 className="section-title mb-3">Sign in to {brand.name}</h1>
        <p className="text-secondary mb-4">
          Access dashboards, orders, payments, and customer service tools from one connected {brand.name} workspace.
        </p>
        <form className="row g-3" onSubmit={handleSubmit}>
          <div className="col-12">
            <label className="form-label">Email</label>
            <input
              className="form-control"
              type="email"
              required
              value={form.email}
              onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))}
            />
          </div>
          <div className="col-12">
            <PasswordField
              label="Password"
              required
              autoComplete="current-password"
              value={form.password}
              onChange={(event) =>
                setForm((current) => ({ ...current, password: event.target.value }))
              }
            />
          </div>
          <div className="col-12 d-grid">
            <button className="btn btn-warning btn-lg" disabled={loading}>
              {loading ? "Signing in..." : "Sign In"}
            </button>
          </div>
        </form>
        <div className="mt-4 small text-secondary">
          Default admin: <strong>admin@restaurant.com</strong> / <strong>Admin@123</strong>
        </div>
        <div className="mt-2">
          <Link to="/register">Need a customer account? Register here.</Link>
        </div>
      </div>
    </section>
  );
}

export function RegisterPage() {
  const navigate = useNavigate();
  const { register } = useAuth();
  const { showToast } = useToast();
  const [form, setForm] = useState({
    fullName: "",
    email: "",
    password: "",
    phoneNumber: "",
    address: ""
  });
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (form.password.length < 8) {
      showToast("Password must be at least 8 characters long.", "error");
      return;
    }

    setLoading(true);
    try {
      const result = await register(form);
      showToast(`Registration complete. Welcome to ${brand.name}.`);
      navigate(getDashboardPathForRole(result.user?.role) ?? "/dashboard/customer", { replace: true });
    } catch (error) {
      showToast(error.message, "error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="auth-shell">
      <div className="auth-card">
        <span className="hero-badge mb-3">{brand.label}</span>
        <h1 className="section-title mb-3">Create Your {brand.name} Account</h1>
        <p className="text-secondary mb-4">
          Customers can browse the menu, build a cart, place orders, and review their order history.
        </p>
        <form className="row g-3" onSubmit={handleSubmit}>
          <div className="col-md-6">
            <label className="form-label">Full Name</label>
            <input
              className="form-control"
              required
              value={form.fullName}
              onChange={(event) =>
                setForm((current) => ({ ...current, fullName: event.target.value }))
              }
            />
          </div>
          <div className="col-md-6">
            <label className="form-label">Email</label>
            <input
              className="form-control"
              type="email"
              required
              value={form.email}
              onChange={(event) =>
                setForm((current) => ({ ...current, email: event.target.value }))
              }
            />
          </div>
          <div className="col-md-6">
            <PasswordField
              label="Password"
              required
              autoComplete="new-password"
              value={form.password}
              onChange={(event) =>
                setForm((current) => ({ ...current, password: event.target.value }))
              }
            />
          </div>
          <div className="col-md-6">
            <label className="form-label">Phone Number</label>
            <input
              className="form-control"
              value={form.phoneNumber}
              onChange={(event) =>
                setForm((current) => ({ ...current, phoneNumber: event.target.value }))
              }
            />
          </div>
          <div className="col-12">
            <label className="form-label">Address</label>
            <input
              className="form-control"
              value={form.address}
              onChange={(event) =>
                setForm((current) => ({ ...current, address: event.target.value }))
              }
            />
          </div>
          <div className="col-12 d-grid">
            <button className="btn btn-warning btn-lg" disabled={loading}>
              {loading ? "Creating account..." : "Create Account"}
            </button>
          </div>
        </form>
      </div>
    </section>
  );
}
