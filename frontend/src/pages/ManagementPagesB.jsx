import { useEffect, useMemo, useState } from "react";
import api, { apiRequest } from "../api/client";
import { DashboardLayout } from "../components/layout/AppShell";
import { DataTable, ErrorState, LoadingState, PasswordField, SectionHeading, StatCard } from "../components/common/Ui";
import { useToast } from "../context/ToastContext";
import { useAuth } from "../context/AuthContext";
import { useLiveData } from "../hooks/useLiveData";
import { formatCurrency, formatDate } from "../utils/format";
import { notifyDataChanged } from "../utils/liveData";
import { orderStatuses, paymentMethods, roles } from "../utils/constants";

function Panel({ title, subtitle, children }) {
  return (
    <div className="glass-panel p-4 h-100">
      <SectionHeading title={title} subtitle={subtitle} />
      {children}
    </div>
  );
}

export function OrdersManagementPage() {
  const { user } = useAuth();
  const [draftItem, setDraftItem] = useState({ menuItemId: "", quantity: 1 });
  const [orderDraft, setOrderDraft] = useState({ tableId: "", orderType: "DineIn", notes: "", items: [] });
  const [paymentDraft, setPaymentDraft] = useState({ orderId: "", amount: "", paymentMethod: "Cash", transactionId: "" });
  const { showToast } = useToast();

  const {
    data,
    loading,
    error,
    reload
  } = useLiveData(
    async () => {
      const [ordersData, tablesData, menuData] = await Promise.all([
        apiRequest(api.get("/api/orders?pageSize=50"), "Failed to load orders"),
        apiRequest(api.get("/api/tables"), "Failed to load tables"),
        apiRequest(api.get("/api/menuitems?pageSize=100"), "Failed to load menu items")
      ]);

      return {
        orders: ordersData.items ?? [],
        tables: tablesData,
        menuItems: menuData.items ?? []
      };
    },
    [user?.role],
    {
      initialData: {
        orders: [],
        tables: [],
        menuItems: []
      }
    }
  );

  const orders = data?.orders ?? [];
  const tables = data?.tables ?? [];
  const menuItems = data?.menuItems ?? [];

  useEffect(() => {
    if (menuItems.length) {
      setDraftItem((current) => ({
        ...current,
        menuItemId: current.menuItemId || menuItems[0].id
      }));
    }
  }, [menuItems]);

  const draftTotal = useMemo(() => {
    return orderDraft.items.reduce((sum, item) => {
      const menuItem = menuItems.find((candidate) => candidate.id === item.menuItemId);
      return sum + (menuItem?.finalPrice ?? 0) * item.quantity;
    }, 0);
  }, [menuItems, orderDraft.items]);

  const payableOrders = useMemo(
    () => orders.filter((order) => !["Paid", "Cancelled"].includes(order.status)),
    [orders]
  );

  useEffect(() => {
    if (!paymentDraft.orderId) {
      return;
    }

    const selectedOrder = payableOrders.find((order) => order.id === paymentDraft.orderId);
    if (!selectedOrder) {
      return;
    }

    setPaymentDraft((current) => ({
      ...current,
      amount: current.amount || String(selectedOrder.totalAmount)
    }));
  }, [paymentDraft.orderId, payableOrders]);

  const addDraftItem = () => {
    if (!draftItem.menuItemId) {
      return;
    }

    setOrderDraft((current) => ({
      ...current,
      items: [...current.items, { menuItemId: draftItem.menuItemId, quantity: Number(draftItem.quantity), notes: "" }]
    }));
  };

  const submitOrder = async (event) => {
    event.preventDefault();

    if (orderDraft.items.length === 0) {
      showToast("Add at least one item before creating an order.", "error");
      return;
    }

    try {
      await apiRequest(
        api.post("/api/orders", {
          ...orderDraft,
          tableId: orderDraft.tableId || null
        }),
        "Failed to create order"
      );
      showToast("Order created.");
      setOrderDraft({ tableId: "", orderType: "DineIn", notes: "", items: [] });
      await reload({ silent: true });
      notifyDataChanged({ source: "staff-order" });
    } catch (error) {
      showToast(error.message, "error");
    }
  };

  const updateStatus = async (id, status) => {
    try {
      await apiRequest(api.put(`/api/orders/${id}/status`, { status }), "Failed to update order status");
      showToast(`Order moved to ${status}.`);
      await reload({ silent: true });
      notifyDataChanged({ source: "order-status" });
    } catch (error) {
      showToast(error.message, "error");
    }
  };

  const submitPayment = async (event) => {
    event.preventDefault();

    if (!paymentDraft.orderId) {
      showToast("Select an order before saving payment.", "error");
      return;
    }

    if (Number(paymentDraft.amount) <= 0) {
      showToast("Enter a valid payment amount.", "error");
      return;
    }

    try {
      await apiRequest(
        api.post("/api/payments", {
          ...paymentDraft,
          amount: Number(paymentDraft.amount)
        }),
        "Failed to create payment"
      );
      showToast("Payment recorded.");
      setPaymentDraft({ orderId: "", amount: "", paymentMethod: "Cash", transactionId: "" });
      await reload({ silent: true });
      notifyDataChanged({ source: "payment" });
    } catch (error) {
      showToast(error.message, "error");
    }
  };

  return (
    <DashboardLayout title="Orders Management" subtitle="Create new orders, move status across the kitchen flow, and record payments.">
      <div className="row g-4 mb-4">
        {user?.role !== roles.chef ? (
          <div className="col-xl-6">
            <Panel title="Create Order" subtitle="Use this panel for cashier or waiter assisted order entry.">
              <form className="row g-3" onSubmit={submitOrder}>
                <div className="col-md-6">
                  <label className="form-label">Order Type</label>
                  <select className="form-select" value={orderDraft.orderType} onChange={(event) => setOrderDraft((current) => ({ ...current, orderType: event.target.value }))}>
                    <option>DineIn</option>
                    <option>TakeAway</option>
                    <option>Delivery</option>
                  </select>
                </div>
                <div className="col-md-6">
                  <label className="form-label">Table</label>
                  <select className="form-select" value={orderDraft.tableId} onChange={(event) => setOrderDraft((current) => ({ ...current, tableId: event.target.value }))}>
                    <option value="">No table</option>
                    {tables.map((table) => (
                      <option key={table.id} value={table.id}>
                        {table.tableNumber} ({table.status})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="col-md-7">
                  <label className="form-label">Menu Item</label>
                  <select className="form-select" value={draftItem.menuItemId} onChange={(event) => setDraftItem((current) => ({ ...current, menuItemId: event.target.value }))}>
                    {menuItems.map((item) => (
                      <option key={item.id} value={item.id}>
                        {item.name} - {formatCurrency(item.finalPrice)}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="col-md-3">
                  <label className="form-label">Qty</label>
                  <input className="form-control" type="number" min="1" value={draftItem.quantity} onChange={(event) => setDraftItem((current) => ({ ...current, quantity: event.target.value }))} />
                </div>
                <div className="col-md-2 d-grid">
                  <label className="form-label invisible">Add</label>
                  <button type="button" className="btn btn-outline-dark" onClick={addDraftItem}>
                    Add
                  </button>
                </div>
                <div className="col-12">
                  <label className="form-label">Notes</label>
                  <textarea className="form-control" rows="2" value={orderDraft.notes} onChange={(event) => setOrderDraft((current) => ({ ...current, notes: event.target.value }))} />
                </div>
                <div className="col-12">
                  <div className="small text-secondary mb-2">Draft Items</div>
                  <div className="d-flex flex-column gap-2">
                    {orderDraft.items.map((item, index) => {
                      const menuItem = menuItems.find((candidate) => candidate.id === item.menuItemId);
                      return (
                        <div key={`${item.menuItemId}-${index}`} className="d-flex justify-content-between align-items-center bg-light rounded-4 px-3 py-2">
                          <span>
                            {menuItem?.name} x {item.quantity}
                          </span>
                          <button type="button" className="btn btn-sm btn-outline-danger" onClick={() => setOrderDraft((current) => ({ ...current, items: current.items.filter((_, currentIndex) => currentIndex !== index) }))}>
                            Remove
                          </button>
                        </div>
                      );
                    })}
                  </div>
                </div>
                <div className="col-12 d-flex justify-content-between align-items-center">
                  <strong>Estimated Total</strong>
                  <strong>{formatCurrency(draftTotal)}</strong>
                </div>
                <div className="col-12 d-grid">
                  <button className="btn btn-warning" disabled={orderDraft.items.length === 0}>
                    Create Order
                  </button>
                </div>
              </form>
            </Panel>
          </div>
        ) : null}
        {[roles.admin, roles.manager, roles.cashier].includes(user?.role) ? (
          <div className="col-xl-6">
            <Panel title="Record Payment" subtitle="Collect cash, card, or mobile payments and close invoices.">
              <form className="row g-3" onSubmit={submitPayment}>
                <div className="col-12">
                    <label className="form-label">Order</label>
                    <select className="form-select" value={paymentDraft.orderId} onChange={(event) => setPaymentDraft((current) => ({ ...current, orderId: event.target.value }))}>
                      <option value="">Select an order</option>
                      {payableOrders.map((order) => (
                        <option key={order.id} value={order.id}>
                          {order.orderNumber} - {formatCurrency(order.totalAmount)} - {order.status}
                        </option>
                    ))}
                  </select>
                </div>
                <div className="col-md-6">
                  <label className="form-label">Amount</label>
                  <input className="form-control" type="number" step="0.01" value={paymentDraft.amount} onChange={(event) => setPaymentDraft((current) => ({ ...current, amount: event.target.value }))} />
                </div>
                <div className="col-md-6">
                  <label className="form-label">Method</label>
                  <select className="form-select" value={paymentDraft.paymentMethod} onChange={(event) => setPaymentDraft((current) => ({ ...current, paymentMethod: event.target.value }))}>
                    {paymentMethods.map((method) => (
                      <option key={method}>{method}</option>
                    ))}
                  </select>
                </div>
                <div className="col-12">
                  <label className="form-label">Transaction Id</label>
                  <input className="form-control" value={paymentDraft.transactionId} onChange={(event) => setPaymentDraft((current) => ({ ...current, transactionId: event.target.value }))} />
                </div>
                <div className="col-12 d-grid">
                  <button className="btn btn-warning">Save Payment</button>
                </div>
              </form>
            </Panel>
          </div>
        ) : null}
      </div>

      <Panel title="Orders Queue" subtitle="Update statuses as dishes move through service.">
        {loading && orders.length === 0 ? (
          <LoadingState />
        ) : error && orders.length === 0 ? (
          <ErrorState
            title="Orders workspace unavailable"
            description={error.message}
            action={<button className="btn btn-warning" onClick={() => void reload()}>Try Again</button>}
          />
        ) : (
          <>
            {error ? <div className="alert alert-warning">{error.message}. Showing the most recent loaded orders.</div> : null}
            <DataTable
              columns={[
                { key: "orderNumber", label: "Order" },
                { key: "status", label: "Status" },
                { key: "orderType", label: "Type" },
                { key: "tableNumber", label: "Table" },
                { key: "totalAmount", label: "Total", render: (row) => formatCurrency(row.totalAmount) },
                { key: "createdAtUtc", label: "Created", render: (row) => formatDate(row.createdAtUtc) },
                {
                  key: "actions",
                  label: "Move To",
                  render: (row) => (
                    <select className="form-select form-select-sm" defaultValue={row.status} onChange={(event) => updateStatus(row.id, event.target.value)}>
                      {orderStatuses.map((status) => (
                        <option key={status}>{status}</option>
                      ))}
                    </select>
                  )
                }
              ]}
              rows={orders}
            />
          </>
        )}
      </Panel>
    </DashboardLayout>
  );
}

export function EmployeeManagementPage() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState({ fullName: "", email: "", password: "", role: roles.manager, phoneNumber: "", address: "", isActive: true });
  const { showToast } = useToast();

  const loadUsers = () => apiRequest(api.get("/api/users?pageSize=100"), "Failed to load users").then((data) => setUsers(data.items ?? []));

  useEffect(() => {
    loadUsers().finally(() => setLoading(false));
  }, []);

  const submit = async (event) => {
    event.preventDefault();
    try {
      if (editingId) {
        const { email, ...payload } = form;
        await apiRequest(api.put(`/api/users/${editingId}`, payload), "Failed to update employee");
        showToast("Employee updated.");
      } else {
        await apiRequest(api.post("/api/users", form), "Failed to create employee");
        showToast("Employee created.");
      }

      setEditingId(null);
      setForm({ fullName: "", email: "", password: "", role: roles.manager, phoneNumber: "", address: "", isActive: true });
      await loadUsers();
    } catch (error) {
      showToast(error.message, "error");
    }
  };

  return (
    <DashboardLayout title="Employee Management" subtitle="Create staff accounts, assign roles, update profiles, and deactivate access.">
      <div className="row g-4">
        <div className="col-xl-5">
          <Panel title={editingId ? "Edit Employee" : "Add Employee"}>
            <form className="row g-3" onSubmit={submit}>
              <div className="col-12">
                <label className="form-label">Full Name</label>
                <input className="form-control" required value={form.fullName} onChange={(event) => setForm((current) => ({ ...current, fullName: event.target.value }))} />
              </div>
              <div className="col-12">
                <label className="form-label">Email</label>
                <input className="form-control" type="email" required disabled={Boolean(editingId)} value={form.email} onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))} />
              </div>
              <div className="col-md-6">
                <PasswordField
                  label={editingId ? "New Password" : "Password"}
                  autoComplete={editingId ? "new-password" : "current-password"}
                  value={form.password}
                  onChange={(event) => setForm((current) => ({ ...current, password: event.target.value }))}
                />
              </div>
              <div className="col-md-6">
                <label className="form-label">Role</label>
                <select className="form-select" value={form.role} onChange={(event) => setForm((current) => ({ ...current, role: event.target.value }))}>
                  {Object.values(roles)
                    .filter((role) => role !== roles.customer)
                    .map((role) => (
                      <option key={role}>{role}</option>
                    ))}
                </select>
              </div>
              <div className="col-md-6">
                <label className="form-label">Phone</label>
                <input className="form-control" value={form.phoneNumber} onChange={(event) => setForm((current) => ({ ...current, phoneNumber: event.target.value }))} />
              </div>
              <div className="col-md-6">
                <label className="form-label">Active</label>
                <select className="form-select" value={String(form.isActive)} onChange={(event) => setForm((current) => ({ ...current, isActive: event.target.value === "true" }))}>
                  <option value="true">Active</option>
                  <option value="false">Inactive</option>
                </select>
              </div>
              <div className="col-12">
                <label className="form-label">Address</label>
                <input className="form-control" value={form.address} onChange={(event) => setForm((current) => ({ ...current, address: event.target.value }))} />
              </div>
              <div className="col-12 d-grid">
                <button className="btn btn-warning">{editingId ? "Update Employee" : "Create Employee"}</button>
              </div>
            </form>
          </Panel>
        </div>
        <div className="col-xl-7">
          <Panel title="Employees">
            {loading ? (
              <LoadingState />
            ) : (
              <DataTable
                columns={[
                  { key: "fullName", label: "Name" },
                  { key: "email", label: "Email" },
                  { key: "role", label: "Role" },
                  { key: "phoneNumber", label: "Phone" },
                  { key: "isActive", label: "Status", render: (row) => (row.isActive ? "Active" : "Inactive") },
                  {
                    key: "actions",
                    label: "Actions",
                    render: (row) => (
                      <button
                        className="btn btn-sm btn-outline-dark"
                        onClick={() => {
                          setEditingId(row.id);
                          setForm((current) => ({ ...current, fullName: row.fullName, email: row.email, password: "", role: row.role, phoneNumber: row.phoneNumber ?? "", address: "", isActive: row.isActive }));
                        }}
                      >
                        Edit
                      </button>
                    )
                  }
                ]}
                rows={users}
              />
            )}
          </Panel>
        </div>
      </div>
    </DashboardLayout>
  );
}

export function InventoryManagementPage() {
  const [items, setItems] = useState([]);
  const [alerts, setAlerts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState({ name: "", unit: "kg", quantityInStock: 0, reorderLevel: 0, costPerUnit: 0, supplierName: "", notes: "" });
  const { showToast } = useToast();

  const loadData = async () => {
    const [inventoryData, lowStockData] = await Promise.all([
      apiRequest(api.get("/api/inventory?pageSize=100"), "Failed to load inventory"),
      apiRequest(api.get("/api/inventory/low-stock"), "Failed to load low stock alerts")
    ]);

    setItems(inventoryData.items ?? []);
    setAlerts(lowStockData);
  };

  useEffect(() => {
    loadData().finally(() => setLoading(false));
  }, []);

  const submit = async (event) => {
    event.preventDefault();
    try {
      const payload = {
        ...form,
        quantityInStock: Number(form.quantityInStock),
        reorderLevel: Number(form.reorderLevel),
        costPerUnit: Number(form.costPerUnit)
      };

      if (editingId) {
        await apiRequest(api.put(`/api/inventory/${editingId}`, payload), "Failed to update inventory");
        showToast("Inventory item updated.");
      } else {
        await apiRequest(api.post("/api/inventory", payload), "Failed to create inventory item");
        showToast("Inventory item created.");
      }

      setEditingId(null);
      setForm({ name: "", unit: "kg", quantityInStock: 0, reorderLevel: 0, costPerUnit: 0, supplierName: "", notes: "" });
      await loadData();
    } catch (error) {
      showToast(error.message, "error");
    }
  };

  return (
    <DashboardLayout title="Inventory Management" subtitle="Track stock, update quantities, and catch low-stock alerts early.">
      <div className="row g-4 mb-4">
        <div className="col-xl-5">
          <Panel title={editingId ? "Edit Inventory Item" : "Add Inventory Item"}>
            <form className="row g-3" onSubmit={submit}>
              <div className="col-md-8">
                <label className="form-label">Name</label>
                <input className="form-control" required value={form.name} onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))} />
              </div>
              <div className="col-md-4">
                <label className="form-label">Unit</label>
                <input className="form-control" value={form.unit} onChange={(event) => setForm((current) => ({ ...current, unit: event.target.value }))} />
              </div>
              <div className="col-md-4">
                <label className="form-label">Quantity</label>
                <input className="form-control" type="number" step="0.01" value={form.quantityInStock} onChange={(event) => setForm((current) => ({ ...current, quantityInStock: event.target.value }))} />
              </div>
              <div className="col-md-4">
                <label className="form-label">Reorder Level</label>
                <input className="form-control" type="number" step="0.01" value={form.reorderLevel} onChange={(event) => setForm((current) => ({ ...current, reorderLevel: event.target.value }))} />
              </div>
              <div className="col-md-4">
                <label className="form-label">Cost / Unit</label>
                <input className="form-control" type="number" step="0.01" value={form.costPerUnit} onChange={(event) => setForm((current) => ({ ...current, costPerUnit: event.target.value }))} />
              </div>
              <div className="col-12">
                <label className="form-label">Supplier</label>
                <input className="form-control" value={form.supplierName} onChange={(event) => setForm((current) => ({ ...current, supplierName: event.target.value }))} />
              </div>
              <div className="col-12">
                <label className="form-label">Notes</label>
                <textarea className="form-control" rows="2" value={form.notes} onChange={(event) => setForm((current) => ({ ...current, notes: event.target.value }))} />
              </div>
              <div className="col-12 d-grid">
                <button className="btn btn-warning">{editingId ? "Update Item" : "Create Item"}</button>
              </div>
            </form>
          </Panel>
        </div>
        <div className="col-xl-7">
          <div className="row g-4">
            <div className="col-12">
              <Panel title="Low Stock Alerts">
                <div className="row g-3">
                  {alerts.map((alert) => (
                    <div className="col-md-6" key={alert.id}>
                      <StatCard label={alert.name} value={`${alert.quantityInStock} ${alert.unit}`} accent="gold" hint={`Reorder at ${alert.reorderLevel} ${alert.unit}`} />
                    </div>
                  ))}
                </div>
              </Panel>
            </div>
          </div>
        </div>
      </div>

      <Panel title="Inventory Table">
        {loading ? (
          <LoadingState />
        ) : (
          <DataTable
            columns={[
              { key: "name", label: "Name" },
              { key: "quantityInStock", label: "Qty" },
              { key: "unit", label: "Unit" },
              { key: "reorderLevel", label: "Reorder" },
              { key: "costPerUnit", label: "Cost", render: (row) => formatCurrency(row.costPerUnit) },
              { key: "lastUpdatedAtUtc", label: "Updated", render: (row) => formatDate(row.lastUpdatedAtUtc) },
              {
                key: "actions",
                label: "Actions",
                render: (row) => (
                  <button className="btn btn-sm btn-outline-dark" onClick={() => { setEditingId(row.id); setForm({ name: row.name, unit: row.unit, quantityInStock: row.quantityInStock, reorderLevel: row.reorderLevel, costPerUnit: row.costPerUnit, supplierName: row.supplierName ?? "", notes: row.notes ?? "" }); }}>
                    Edit
                  </button>
                )
              }
            ]}
            rows={items}
          />
        )}
      </Panel>
    </DashboardLayout>
  );
}

export function ReportsPage() {
  const [period, setPeriod] = useState("daily");
  const {
    data,
    loading,
    error,
    reload
  } = useLiveData(
    async () => {
      const [salesData, topItemsData, employeeData] = await Promise.all([
        apiRequest(api.get(`/api/reports/sales-summary?period=${period}`), "Failed to load sales report"),
        apiRequest(api.get("/api/reports/top-menu-items?take=5"), "Failed to load top menu items"),
        apiRequest(api.get("/api/reports/employee-performance"), "Failed to load employee performance")
      ]);

      return {
        report: salesData,
        topItems: topItemsData,
        employees: employeeData
      };
    },
    [period],
    {
      initialData: {
        report: null,
        topItems: [],
        employees: []
      }
    }
  );

  const report = data?.report;
  const topItems = data?.topItems ?? [];
  const employees = data?.employees ?? [];

  return (
    <DashboardLayout title="Reports" subtitle="Daily, weekly, and monthly sales visibility with item and employee analytics.">
      <div className="d-flex justify-content-end mb-4">
        <select className="form-select w-auto" value={period} onChange={(event) => setPeriod(event.target.value)}>
          <option value="daily">Daily</option>
          <option value="weekly">Weekly</option>
          <option value="monthly">Monthly</option>
        </select>
      </div>
      {loading && !report ? (
        <LoadingState />
      ) : error && !report ? (
        <ErrorState
          title="Reports unavailable"
          description={error.message}
          action={<button className="btn btn-warning" onClick={() => void reload()}>Try Again</button>}
        />
      ) : (
        <>
          {error ? <div className="alert alert-warning">{error.message}. Showing the most recent loaded report data.</div> : null}
          <div className="row g-3 mb-4">
            <div className="col-md-4">
              <StatCard label="Total Revenue" value={formatCurrency(report.totalRevenue)} accent="gold" />
            </div>
            <div className="col-md-4">
              <StatCard label="Total Orders" value={report.totalOrders} accent="blue" />
            </div>
            <div className="col-md-4">
              <StatCard label="Average Order Value" value={formatCurrency(report.averageOrderValue)} accent="green" />
            </div>
          </div>
          <div className="row g-4">
            <div className="col-xl-6">
              <Panel title="Trend">
                <DataTable columns={[{ key: "label", label: "Period" }, { key: "amount", label: "Revenue", render: (row) => formatCurrency(row.amount) }]} rows={report.trend} />
              </Panel>
            </div>
            <div className="col-xl-6">
              <Panel title="Top Menu Items">
                <DataTable columns={[{ key: "name", label: "Item" }, { key: "quantitySold", label: "Qty" }, { key: "revenue", label: "Revenue", render: (row) => formatCurrency(row.revenue) }]} rows={topItems} />
              </Panel>
            </div>
          </div>
          <div className="mt-4">
            <Panel title="Employee Performance">
              <DataTable columns={[{ key: "employeeName", label: "Employee" }, { key: "role", label: "Role" }, { key: "ordersHandled", label: "Orders Handled" }, { key: "revenueInfluenced", label: "Revenue", render: (row) => formatCurrency(row.revenueInfluenced) }]} rows={employees} />
            </Panel>
          </div>
        </>
      )}
    </DashboardLayout>
  );
}
