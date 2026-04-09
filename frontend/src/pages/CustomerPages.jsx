import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api, { apiRequest } from "../api/client";
import { DashboardLayout } from "../components/layout/AppShell";
import {
  DataTable,
  EmptyState,
  ErrorState,
  ImageWithFallback,
  LoadingState,
  SectionHeading
} from "../components/common/Ui";
import { useCart } from "../context/CartContext";
import { useToast } from "../context/ToastContext";
import { useLiveData } from "../hooks/useLiveData";
import { brand, getMenuFallback } from "../utils/branding";
import { formatCurrency, formatDate, resolveImageUrl } from "../utils/format";
import { notifyDataChanged } from "../utils/liveData";

function QuantityStepper({ quantity, onDecrease, onIncrease }) {
  return (
    <div className="quantity-stepper">
      <button type="button" onClick={onDecrease}>
        -
      </button>
      <span>{quantity}</span>
      <button type="button" onClick={onIncrease}>
        +
      </button>
    </div>
  );
}

export function CartPage() {
  const {
    items,
    totalAmount,
    totalItems,
    incrementQuantity,
    decrementQuantity,
    removeFromCart,
    clearCart
  } = useCart();

  return (
    <DashboardLayout
      title="Cart"
      subtitle="Review every selected dish before sending your order to the kitchen."
    >
      {items.length === 0 ? (
        <EmptyState
          title="Your cart is empty"
          description="Add menu items from the menu page before placing an order."
        />
      ) : (
        <div className="glass-panel p-4">
          <SectionHeading
            title="Cart Items"
            subtitle={`${totalItems} item${totalItems > 1 ? "s" : ""} selected`}
            action={
              <button className="btn btn-outline-dark" onClick={clearCart}>
                Clear Cart
              </button>
            }
          />
          <div className="cart-list">
            {items.map((item) => {
              const fallbackSrc = resolveImageUrl(getMenuFallback(item));
              const displaySrc = item.imageUrl ? resolveImageUrl(item.imageUrl) : fallbackSrc;

              return (
                <div key={item.id} className="cart-line">
                  <ImageWithFallback
                    src={displaySrc}
                    fallbackSrc={fallbackSrc}
                    alt={item.name}
                    className="cart-line-image"
                  />
                  <div className="cart-line-main">
                    <div className="cart-line-title">
                      <div>
                        <h5 className="mb-1">{item.name}</h5>
                        <div className="small text-secondary">{item.categoryName || brand.label}</div>
                      </div>
                      <strong>{formatCurrency(item.price * item.quantity)}</strong>
                    </div>
                    <div className="small text-secondary">
                      Unit price: {formatCurrency(item.price)}
                    </div>
                    <div className="cart-line-footer">
                      <QuantityStepper
                        quantity={item.quantity}
                        onDecrease={() => decrementQuantity(item.id)}
                        onIncrease={() => incrementQuantity(item.id)}
                      />
                      <button
                        type="button"
                        className="btn btn-sm btn-outline-danger"
                        onClick={() => removeFromCart(item.id)}
                      >
                        Remove
                      </button>
                    </div>
                  </div>
                  <div className="d-flex flex-column gap-2 align-items-end">
                    <span className="catalog-chip">Ready in cart</span>
                    <Link to="/place-order" className="btn btn-warning">
                      Continue
                    </Link>
                  </div>
                </div>
              );
            })}
          </div>
          <div className="cart-summary">
            <div>
              <div className="small text-secondary">Automatic total</div>
              <strong>{formatCurrency(totalAmount)}</strong>
            </div>
            <div className="d-flex gap-2">
              <Link to="/menu" className="btn btn-outline-dark">
                Add More Items
              </Link>
              <Link to="/place-order" className="btn btn-warning">
                Place Order
              </Link>
            </div>
          </div>
        </div>
      )}
    </DashboardLayout>
  );
}

export function PlaceOrderPage() {
  const { items, totalAmount, clearCart } = useCart();
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({ tableId: "", orderType: "DineIn", notes: "" });
  const { showToast } = useToast();
  const navigate = useNavigate();

  const {
    data: tables = [],
    loading: tablesLoading,
    error: tablesError,
    reload: reloadTables
  } = useLiveData(
    () => apiRequest(api.get("/api/tables/available?guests=1"), "Failed to load tables"),
    [],
    { initialData: [] }
  );

  useEffect(() => {
    if (form.orderType !== "DineIn" && form.tableId) {
      setForm((current) => ({ ...current, tableId: "" }));
    }
  }, [form.orderType, form.tableId]);

  const submit = async (event) => {
    event.preventDefault();
    if (!items.length) {
      showToast("Your cart is empty.", "error");
      return;
    }

    setSubmitting(true);
    try {
      await apiRequest(
        api.post("/api/orders", {
          tableId: form.orderType === "DineIn" ? form.tableId || null : null,
          orderType: form.orderType,
          notes: form.notes,
          items: items.map((item) => ({ menuItemId: item.id, quantity: item.quantity }))
        }),
        "Failed to place order"
      );

      clearCart();
      notifyDataChanged({ source: "customer-order" });
      showToast("Order placed successfully.");
      navigate("/dashboard/customer", { replace: true });
    } catch (error) {
      showToast(error.message, "error");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <DashboardLayout
      title="Place Order"
      subtitle="Confirm the order type, pick a table if needed, and send your cart."
    >
      <div className="row g-4">
        <div className="col-xl-5">
          <div className="glass-panel p-4">
            <SectionHeading title="Order Details" />
            <form className="row g-3" onSubmit={submit}>
              <div className="col-md-6">
                <label className="form-label">Order Type</label>
                <select
                  className="form-select"
                  value={form.orderType}
                  onChange={(event) =>
                    setForm((current) => ({ ...current, orderType: event.target.value }))
                  }
                >
                  <option>DineIn</option>
                  <option>TakeAway</option>
                  <option>Delivery</option>
                </select>
              </div>
              <div className="col-md-6">
                <label className="form-label">Table</label>
                {tablesError && form.orderType === "DineIn" ? (
                  <div className="d-grid gap-2">
                    <div className="small text-danger">{tablesError.message}</div>
                    <button
                      type="button"
                      className="btn btn-sm btn-outline-dark"
                      onClick={() => void reloadTables()}
                    >
                      Reload Tables
                    </button>
                  </div>
                ) : (
                  <select
                    className="form-select"
                    value={form.tableId}
                    onChange={(event) =>
                      setForm((current) => ({ ...current, tableId: event.target.value }))
                    }
                    disabled={form.orderType !== "DineIn" || tablesLoading}
                  >
                    <option value="">{tablesLoading ? "Loading tables..." : "No table"}</option>
                    {tables.map((table) => (
                      <option key={table.id} value={table.id}>
                        {table.tableNumber} ({table.seats} seats)
                      </option>
                    ))}
                  </select>
                )}
              </div>
              {form.orderType !== "DineIn" ? (
                <div className="col-12 small text-secondary">
                  Table assignment is only needed for dine-in orders.
                </div>
              ) : null}
              <div className="col-12">
                <label className="form-label">Notes</label>
                <textarea
                  className="form-control"
                  rows="3"
                  value={form.notes}
                  onChange={(event) =>
                    setForm((current) => ({ ...current, notes: event.target.value }))
                  }
                />
              </div>
              <div className="col-12 d-grid">
                <button className="btn btn-warning" disabled={submitting || !items.length}>
                  {submitting ? "Placing Order..." : "Confirm Order"}
                </button>
              </div>
            </form>
          </div>
        </div>
        <div className="col-xl-7">
          <div className="glass-panel p-4 h-100">
            <SectionHeading
              title="Order Summary"
              subtitle={`Estimated total: ${formatCurrency(totalAmount)}`}
            />
            {items.length ? (
              <DataTable
                columns={[
                  { key: "name", label: "Item" },
                  { key: "quantity", label: "Qty" },
                  { key: "price", label: "Unit Price", render: (row) => formatCurrency(row.price) },
                  {
                    key: "total",
                    label: "Line Total",
                    render: (row) => formatCurrency(row.price * row.quantity)
                  }
                ]}
                rows={items}
              />
            ) : (
              <EmptyState
                title="No items selected"
                description="Add dishes to the cart from the menu page first."
              />
            )}
          </div>
        </div>
      </div>
    </DashboardLayout>
  );
}

export function OrderHistoryPage() {
  const { data: orders = [], loading, error, reload } = useLiveData(
    () =>
      apiRequest(api.get("/api/orders?pageSize=50"), "Failed to load order history").then(
        (data) => data.items ?? []
      ),
    [],
    { initialData: [] }
  );

  return (
    <DashboardLayout
      title="Order History"
      subtitle="Review every current and past order tied to your customer account."
    >
      {loading && orders.length === 0 ? (
        <LoadingState />
      ) : error && orders.length === 0 ? (
        <ErrorState
          title="Order history unavailable"
          description={error.message}
          action={<button className="btn btn-warning" onClick={() => void reload()}>Try Again</button>}
        />
      ) : (
        <div className="glass-panel p-4">
          <SectionHeading title="Orders Timeline" />
          {error ? (
            <div className="alert alert-warning">
              {error.message}. Showing the most recent loaded order history.
            </div>
          ) : null}
          <DataTable
            columns={[
              { key: "orderNumber", label: "Order" },
              { key: "status", label: "Status" },
              { key: "orderType", label: "Type" },
              { key: "tableNumber", label: "Table" },
              { key: "totalAmount", label: "Total", render: (row) => formatCurrency(row.totalAmount) },
              { key: "createdAtUtc", label: "Created", render: (row) => formatDate(row.createdAtUtc) }
            ]}
            rows={orders}
          />
        </div>
      )}
    </DashboardLayout>
  );
}
