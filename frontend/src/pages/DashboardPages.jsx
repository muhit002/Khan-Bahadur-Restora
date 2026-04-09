import api, { apiRequest } from "../api/client";
import { DashboardLayout } from "../components/layout/AppShell";
import {
  DashboardBanner,
  DataTable,
  ErrorState,
  LoadingState,
  SectionHeading,
  StatCard
} from "../components/common/Ui";
import { useLiveData } from "../hooks/useLiveData";
import { brand, dashboardBanners } from "../utils/branding";
import { formatCurrency, formatDate, resolveImageUrl } from "../utils/format";

const adminDashboardDefaults = {
  totalOrders: 0,
  totalRevenue: 0,
  totalCustomers: 0,
  totalEmployees: 0,
  lowStockItems: 0,
  activeMenuItems: 0,
  topMenuItems: [],
  salesOverview: []
};

const managerDashboardDefaults = {
  todayOrders: 0,
  todayRevenue: 0,
  openOrders: 0,
  weeklySales: [],
  topItems: []
};

const cashierDashboardDefaults = {
  pendingPayments: 0,
  pendingAmount: 0,
  openOrders: 0,
  recentOrders: []
};

const chefDashboardDefaults = {
  pendingOrders: 0,
  cookingOrders: 0,
  readyOrders: 0,
  kitchenQueue: []
};

const waiterDashboardDefaults = {
  assignedOrders: 0,
  readyOrders: 0,
  servedToday: 0,
  activeOrders: []
};

const customerDashboardDefaults = {
  totalOrders: 0,
  totalSpent: 0,
  activeOrders: 0,
  recentOrders: []
};

function useDashboardData(endpoint) {
  return useLiveData(() => apiRequest(api.get(endpoint), "Failed to load dashboard"), [endpoint]);
}

function StatsRow({ items }) {
  return (
    <div className="row g-3 mb-4">
      {items.map((item) => (
        <div className="col-md-6 col-xl-3" key={item.label}>
          <StatCard {...item} />
        </div>
      ))}
    </div>
  );
}

function SalesList({ data = [] }) {
  if (data.length === 0) {
    return (
      <div className="glass-panel p-4 h-100">
        <SectionHeading title="Sales Trend" subtitle="Recent revenue movement" />
        <p className="text-secondary mb-0">Sales points will appear after completed payments are recorded.</p>
      </div>
    );
  }

  const peakAmount = Math.max(...data.map((item) => item.amount), 1);

  return (
    <div className="glass-panel p-4 h-100">
      <SectionHeading title="Sales Trend" subtitle="Recent revenue movement" />
      <div className="d-flex flex-column gap-3">
        {data.map((point) => (
          <div key={point.label}>
            <div className="d-flex justify-content-between mb-1">
              <span>{point.label}</span>
              <strong>{formatCurrency(point.amount)}</strong>
            </div>
            <div className="progress" style={{ height: 10 }}>
              <div
                className="progress-bar bg-warning"
                style={{ width: `${Math.min((point.amount / peakAmount) * 100, 100)}%` }}
              />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function TopItemsTable({ items = [] }) {
  return (
    <DataTable
      columns={[
        { key: "name", label: "Menu Item" },
        { key: "quantitySold", label: "Qty Sold" },
        { key: "revenue", label: "Revenue", render: (row) => formatCurrency(row.revenue) }
      ]}
      rows={items}
      emptyMessage="Top-selling items will appear once orders are available."
    />
  );
}

function KitchenQueueTable({ items = [] }) {
  return (
    <DataTable
      columns={[
        { key: "orderNumber", label: "Order" },
        { key: "status", label: "Status" },
        { key: "tableNumber", label: "Table" },
        { key: "createdAtUtc", label: "Created", render: (row) => formatDate(row.createdAtUtc) },
        { key: "items", label: "Items", render: (row) => (Array.isArray(row.items) ? row.items.join(", ") : "") }
      ]}
      rows={items}
      emptyMessage="Kitchen queue is clear right now."
    />
  );
}

function DashboardContent({ data, loading, error, reload, title, children }) {
  if (loading && !data) {
    return <LoadingState />;
  }

  if (error && !data) {
    return (
      <ErrorState
        title={`${title} unavailable`}
        description={error.message}
        action={
          <button className="btn btn-warning" onClick={() => void reload()}>
            Try Again
          </button>
        }
      />
    );
  }

  return (
    <>
      {error ? (
        <div className="alert alert-warning">
          {error.message}. Showing the most recent loaded dashboard data.
        </div>
      ) : null}
      {typeof children === "function" ? children(data) : children}
    </>
  );
}

export function AdminDashboardPage() {
  const { data, loading, error, reload } = useDashboardData("/api/dashboard/admin");

  return (
    <DashboardLayout
      title="Admin Dashboard"
    >
      <DashboardContent data={data} loading={loading} error={error} reload={reload} title="Admin dashboard">
        {(loadedData) => {
          const dashboard = {
            ...adminDashboardDefaults,
            ...(loadedData ?? {}),
            topMenuItems: loadedData?.topMenuItems ?? adminDashboardDefaults.topMenuItems,
            salesOverview: loadedData?.salesOverview ?? adminDashboardDefaults.salesOverview
          };

          return (
            <>
              <DashboardBanner
                eyebrow={brand.label}
                title={`${brand.name} Command Center`}
                imageSrc={resolveImageUrl(dashboardBanners.admin)}
              />
              <StatsRow
                items={[
                  { label: "Total Orders", value: dashboard.totalOrders, accent: "gold" },
                  { label: "Revenue", value: formatCurrency(dashboard.totalRevenue), accent: "blue" },
                  { label: "Customers", value: dashboard.totalCustomers, accent: "green" },
                  { label: "Employees", value: dashboard.totalEmployees, accent: "gold" },
                  { label: "Active Menu Items", value: dashboard.activeMenuItems, accent: "blue" },
                  { label: "Low Stock Items", value: dashboard.lowStockItems, accent: "green" }
                ]}
              />
              <div className="row g-4">
                <div className="col-xl-7">
                  <SalesList data={dashboard.salesOverview} />
                </div>
                <div className="col-xl-5">
                  <div className="glass-panel p-4 h-100">
                    <SectionHeading title="Top Menu Items" subtitle="Best performers by quantity and revenue." />
                    <TopItemsTable items={dashboard.topMenuItems} />
                  </div>
                </div>
              </div>
            </>
          );
        }}
      </DashboardContent>
    </DashboardLayout>
  );
}

export function ManagerDashboardPage() {
  const { data, loading, error, reload } = useDashboardData("/api/dashboard/manager");

  return (
    <DashboardLayout
      title="Manager Dashboard"
      subtitle="Keep daily order flow, revenue movement, and kitchen demand in balance."
    >
      <DashboardContent data={data} loading={loading} error={error} reload={reload} title="Manager dashboard">
        {(loadedData) => {
          const dashboard = {
            ...managerDashboardDefaults,
            ...(loadedData ?? {}),
            weeklySales: loadedData?.weeklySales ?? managerDashboardDefaults.weeklySales,
            topItems: loadedData?.topItems ?? managerDashboardDefaults.topItems
          };

          return (
            <>
              <DashboardBanner
                eyebrow="Service Snapshot"
                title={`Welcome to ${brand.name}`}
                description="This view keeps service pressure, top dishes, and revenue pace visible while the floor is active."
                imageSrc={resolveImageUrl(dashboardBanners.manager)}
              />
              <StatsRow
                items={[
                  { label: "Today's Orders", value: dashboard.todayOrders, accent: "gold" },
                  { label: "Today's Revenue", value: formatCurrency(dashboard.todayRevenue), accent: "blue" },
                  { label: "Open Orders", value: dashboard.openOrders, accent: "green" }
                ]}
              />
              <div className="row g-4">
                <div className="col-xl-7">
                  <SalesList data={dashboard.weeklySales} />
                </div>
                <div className="col-xl-5">
                  <div className="glass-panel p-4 h-100">
                    <SectionHeading title="Top Items" subtitle="Top dishes this week." />
                    <TopItemsTable items={dashboard.topItems} />
                  </div>
                </div>
              </div>
            </>
          );
        }}
      </DashboardContent>
    </DashboardLayout>
  );
}

export function CashierDashboardPage() {
  const { data, loading, error, reload } = useDashboardData("/api/dashboard/cashier");

  return (
    <DashboardLayout
      title="Cashier Dashboard"
      subtitle="Create orders, collect payments, and close invoices without losing the live queue."
    >
      <DashboardContent data={data} loading={loading} error={error} reload={reload} title="Cashier dashboard">
        {(loadedData) => {
          const dashboard = {
            ...cashierDashboardDefaults,
            ...(loadedData ?? {}),
            recentOrders: loadedData?.recentOrders ?? cashierDashboardDefaults.recentOrders
          };

          return (
            <>
              <StatsRow
                items={[
                  { label: "Pending Payments", value: dashboard.pendingPayments, accent: "gold" },
                  { label: "Pending Amount", value: formatCurrency(dashboard.pendingAmount), accent: "blue" },
                  { label: "Open Orders", value: dashboard.openOrders, accent: "green" }
                ]}
              />
              <div className="glass-panel p-4">
                <SectionHeading title="Recent Orders" subtitle="Orders waiting for cashier action." />
                <DataTable
                  columns={[
                    { key: "orderNumber", label: "Order" },
                    { key: "customerName", label: "Customer" },
                    { key: "tableNumber", label: "Table" },
                    { key: "status", label: "Status" },
                    { key: "totalAmount", label: "Total", render: (row) => formatCurrency(row.totalAmount) }
                  ]}
                  rows={dashboard.recentOrders}
                />
              </div>
            </>
          );
        }}
      </DashboardContent>
    </DashboardLayout>
  );
}

export function ChefDashboardPage() {
  const { data, loading, error, reload } = useDashboardData("/api/dashboard/chef");

  return (
    <DashboardLayout
      title="Chef Dashboard"
      subtitle="Follow the kitchen queue from pending tickets to ready plates."
    >
      <DashboardContent data={data} loading={loading} error={error} reload={reload} title="Chef dashboard">
        {(loadedData) => {
          const dashboard = {
            ...chefDashboardDefaults,
            ...(loadedData ?? {}),
            kitchenQueue: loadedData?.kitchenQueue ?? chefDashboardDefaults.kitchenQueue
          };

          return (
            <>
              <StatsRow
                items={[
                  { label: "Pending", value: dashboard.pendingOrders, accent: "gold" },
                  { label: "Cooking", value: dashboard.cookingOrders, accent: "blue" },
                  { label: "Ready", value: dashboard.readyOrders, accent: "green" }
                ]}
              />
              <div className="glass-panel p-4">
                <SectionHeading title="Kitchen Queue" subtitle="Live cooking tickets." />
                <KitchenQueueTable items={dashboard.kitchenQueue} />
              </div>
            </>
          );
        }}
      </DashboardContent>
    </DashboardLayout>
  );
}

export function WaiterDashboardPage() {
  const { data, loading, error, reload } = useDashboardData("/api/dashboard/waiter");

  return (
    <DashboardLayout
      title="Waiter Dashboard"
      subtitle="Watch active orders, pickups, and service handoff from one place."
    >
      <DashboardContent data={data} loading={loading} error={error} reload={reload} title="Waiter dashboard">
        {(loadedData) => {
          const dashboard = {
            ...waiterDashboardDefaults,
            ...(loadedData ?? {}),
            activeOrders: loadedData?.activeOrders ?? waiterDashboardDefaults.activeOrders
          };

          return (
            <>
              <StatsRow
                items={[
                  { label: "Assigned Orders", value: dashboard.assignedOrders, accent: "gold" },
                  { label: "Ready for Pickup", value: dashboard.readyOrders, accent: "blue" },
                  { label: "Served Today", value: dashboard.servedToday, accent: "green" }
                ]}
              />
              <div className="glass-panel p-4">
                <SectionHeading title="Active Orders" subtitle="Orders that need your attention right now." />
                <KitchenQueueTable items={dashboard.activeOrders} />
              </div>
            </>
          );
        }}
      </DashboardContent>
    </DashboardLayout>
  );
}

export function CustomerDashboardPage() {
  const { data, loading, error, reload } = useDashboardData("/api/dashboard/customer");

  return (
    <DashboardLayout
      title="Customer Dashboard"
      subtitle="See your active orders, recent visits, and current ordering activity."
    >
      <DashboardContent data={data} loading={loading} error={error} reload={reload} title="Customer dashboard">
        {(loadedData) => {
          const dashboard = {
            ...customerDashboardDefaults,
            ...(loadedData ?? {}),
            recentOrders: loadedData?.recentOrders ?? customerDashboardDefaults.recentOrders
          };

          return (
            <>
              <DashboardBanner
                eyebrow={brand.tagline}
                title={`Welcome back to ${brand.name}`}
                description="Your recent orders, active dining status, and spend history update here automatically."
                imageSrc={resolveImageUrl(dashboardBanners.customer)}
              />
              <StatsRow
                items={[
                  { label: "Total Orders", value: dashboard.totalOrders, accent: "gold" },
                  { label: "Total Spent", value: formatCurrency(dashboard.totalSpent), accent: "blue" },
                  { label: "Active Orders", value: dashboard.activeOrders, accent: "green" }
                ]}
              />
              <div className="glass-panel p-4">
                <SectionHeading title="Recent Orders" subtitle="Latest order activity tied to your account." />
                <DataTable
                  columns={[
                    { key: "orderNumber", label: "Order" },
                    { key: "status", label: "Status" },
                    { key: "tableNumber", label: "Table" },
                    { key: "totalAmount", label: "Total", render: (row) => formatCurrency(row.totalAmount) }
                  ]}
                  rows={dashboard.recentOrders}
                />
              </div>
            </>
          );
        }}
      </DashboardContent>
    </DashboardLayout>
  );
}
