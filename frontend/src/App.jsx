import { Routes, Route } from "react-router-dom";
import { PublicHeader } from "./components/layout/AppShell";
import ProtectedRoute from "./routes/ProtectedRoute";
import { roles } from "./utils/constants";
import { HomePage, LoginPage, MenuPage, RegisterPage } from "./pages/PublicPages";
import { AdminDashboardPage, CashierDashboardPage, ChefDashboardPage, CustomerDashboardPage, ManagerDashboardPage, WaiterDashboardPage } from "./pages/DashboardPages";
import { CategoryManagementPage, MenuManagementPage } from "./pages/ManagementPagesA";
import { EmployeeManagementPage, InventoryManagementPage, OrdersManagementPage, ReportsPage } from "./pages/ManagementPagesB";
import { CartPage, OrderHistoryPage, PlaceOrderPage } from "./pages/CustomerPages";
import { DashboardRouterPage, NotFoundPage, UnauthorizedPage } from "./pages/SystemPages";

function PublicLayout({ children }) {
  return (
    <>
      <PublicHeader />
      {children}
    </>
  );
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<PublicLayout><HomePage /></PublicLayout>} />
      <Route path="/menu" element={<PublicLayout><MenuPage /></PublicLayout>} />
      <Route path="/login" element={<PublicLayout><LoginPage /></PublicLayout>} />
      <Route path="/register" element={<PublicLayout><RegisterPage /></PublicLayout>} />
      <Route path="/dashboard-router" element={<DashboardRouterPage />} />
      <Route path="/unauthorized" element={<UnauthorizedPage />} />

      <Route element={<ProtectedRoute roles={[roles.admin]} />}>
        <Route path="/dashboard/admin" element={<AdminDashboardPage />} />
        <Route path="/management/employees" element={<EmployeeManagementPage />} />
      </Route>

      <Route element={<ProtectedRoute roles={[roles.admin, roles.manager]} />}>
        <Route path="/dashboard/manager" element={<ManagerDashboardPage />} />
        <Route path="/management/menu" element={<MenuManagementPage />} />
        <Route path="/management/categories" element={<CategoryManagementPage />} />
        <Route path="/management/inventory" element={<InventoryManagementPage />} />
        <Route path="/management/reports" element={<ReportsPage />} />
      </Route>

      <Route element={<ProtectedRoute roles={[roles.admin, roles.manager, roles.cashier]} />}>
        <Route path="/dashboard/cashier" element={<CashierDashboardPage />} />
      </Route>

      <Route element={<ProtectedRoute roles={[roles.admin, roles.manager, roles.cashier, roles.chef, roles.waiter]} />}>
        <Route path="/management/orders" element={<OrdersManagementPage />} />
      </Route>

      <Route element={<ProtectedRoute roles={[roles.admin, roles.manager, roles.chef]} />}>
        <Route path="/dashboard/chef" element={<ChefDashboardPage />} />
      </Route>

      <Route element={<ProtectedRoute roles={[roles.admin, roles.manager, roles.waiter]} />}>
        <Route path="/dashboard/waiter" element={<WaiterDashboardPage />} />
      </Route>

      <Route element={<ProtectedRoute roles={[roles.customer]} />}>
        <Route path="/dashboard/customer" element={<CustomerDashboardPage />} />
        <Route path="/cart" element={<CartPage />} />
        <Route path="/place-order" element={<PlaceOrderPage />} />
        <Route path="/order-history" element={<OrderHistoryPage />} />
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
