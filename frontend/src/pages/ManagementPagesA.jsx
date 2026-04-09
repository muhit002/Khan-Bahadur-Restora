import { useEffect, useState } from "react";
import api, { apiRequest } from "../api/client";
import { DashboardLayout } from "../components/layout/AppShell";
import { DataTable, ImageWithFallback, LoadingState, SectionHeading } from "../components/common/Ui";
import { useToast } from "../context/ToastContext";
import { getCategoryFallback, getMenuFallback } from "../utils/branding";
import { formatCurrency, resolveImageUrl } from "../utils/format";

function ManagementForm({ title, children, onSubmit, submitLabel }) {
  return (
    <div className="glass-panel p-4 h-100">
      <SectionHeading title={title} />
      <form className="row g-3" onSubmit={onSubmit}>
        {children}
        <div className="col-12 d-grid">
          <button className="btn btn-warning">{submitLabel}</button>
        </div>
      </form>
    </div>
  );
}

export function CategoryManagementPage() {
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState({ name: "", description: "", imageUrl: "", sortOrder: 0, isActive: true });
  const { showToast } = useToast();

  const loadCategories = () =>
    apiRequest(api.get("/api/categories?pageSize=50"), "Failed to load categories").then((data) => setCategories(data.items ?? []));

  useEffect(() => {
    loadCategories().finally(() => setLoading(false));
  }, []);

  const resetForm = () => {
    setEditingId(null);
    setForm({ name: "", description: "", imageUrl: "", sortOrder: 0, isActive: true });
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    try {
      if (editingId) {
        await apiRequest(api.put(`/api/categories/${editingId}`, form), "Failed to update category");
        showToast("Category updated.");
      } else {
        await apiRequest(api.post("/api/categories", form), "Failed to create category");
        showToast("Category created.");
      }

      resetForm();
      await loadCategories();
    } catch (error) {
      showToast(error.message, "error");
    }
  };

  const handleDelete = async (id) => {
    try {
      await apiRequest(api.delete(`/api/categories/${id}`), "Failed to delete category");
      showToast("Category deleted.");
      await loadCategories();
    } catch (error) {
      showToast(error.message, "error");
    }
  };

  return (
    <DashboardLayout title="Category Management" subtitle="Control menu groupings, sort order, and active visibility.">
      <div className="row g-4">
        <div className="col-xl-5">
          <ManagementForm title={editingId ? "Edit Category" : "Add Category"} onSubmit={handleSubmit} submitLabel={editingId ? "Update Category" : "Create Category"}>
            <div className="col-12">
              <label className="form-label">Name</label>
              <input className="form-control" required value={form.name} onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))} />
            </div>
            <div className="col-12">
              <label className="form-label">Description</label>
              <textarea className="form-control" rows="3" value={form.description} onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))} />
            </div>
            <div className="col-md-8">
              <label className="form-label">Image URL</label>
              <input className="form-control" value={form.imageUrl} onChange={(event) => setForm((current) => ({ ...current, imageUrl: event.target.value }))} />
            </div>
            <div className="col-md-4">
              <label className="form-label">Sort Order</label>
              <input className="form-control" type="number" value={form.sortOrder} onChange={(event) => setForm((current) => ({ ...current, sortOrder: Number(event.target.value) }))} />
            </div>
            <div className="col-12">
              <div className="form-check">
                <input className="form-check-input" type="checkbox" checked={form.isActive} onChange={(event) => setForm((current) => ({ ...current, isActive: event.target.checked }))} />
                <label className="form-check-label">Active category</label>
              </div>
            </div>
            {editingId ? (
              <div className="col-12 d-grid">
                <button type="button" className="btn btn-outline-dark" onClick={resetForm}>
                  Cancel Edit
                </button>
              </div>
            ) : null}
          </ManagementForm>
        </div>
        <div className="col-xl-7">
          <div className="glass-panel p-4 h-100">
            <SectionHeading title="Categories" subtitle="All menu categories with item counts." />
            {loading ? (
              <LoadingState />
            ) : (
              <DataTable
                columns={[
                  {
                    key: "image",
                    label: "Image",
                    render: (row) => (
                      <ImageWithFallback
                        src={row.imageUrl ? resolveImageUrl(row.imageUrl) : resolveImageUrl(getCategoryFallback(row.name))}
                        fallbackSrc={resolveImageUrl(getCategoryFallback(row.name))}
                        alt={row.name}
                        className="rounded-4"
                        style={{ width: 56, height: 56, objectFit: "cover" }}
                      />
                    )
                  },
                  { key: "name", label: "Name" },
                  { key: "sortOrder", label: "Sort" },
                  { key: "menuItemsCount", label: "Items" },
                  { key: "isActive", label: "Status", render: (row) => (row.isActive ? "Active" : "Hidden") },
                  {
                    key: "actions",
                    label: "Actions",
                    render: (row) => (
                      <div className="d-flex gap-2">
                        <button className="btn btn-sm btn-outline-dark" onClick={() => { setEditingId(row.id); setForm({ name: row.name, description: row.description ?? "", imageUrl: row.imageUrl ?? "", sortOrder: row.sortOrder, isActive: row.isActive }); }}>
                          Edit
                        </button>
                        <button className="btn btn-sm btn-outline-danger" onClick={() => handleDelete(row.id)}>
                          Delete
                        </button>
                      </div>
                    )
                  }
                ]}
                rows={categories}
              />
            )}
          </div>
        </div>
      </div>
    </DashboardLayout>
  );
}

export function MenuManagementPage() {
  const [menuItems, setMenuItems] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [imageFile, setImageFile] = useState(null);
  const [form, setForm] = useState({
    categoryId: "",
    name: "",
    description: "",
    price: 0,
    discountPercentage: 0,
    isAvailable: true,
    imageUrl: "",
    preparationTimeMinutes: 15
  });
  const { showToast } = useToast();

  const loadData = async () => {
    const [categoryData, menuData] = await Promise.all([
      apiRequest(api.get("/api/categories/active"), "Failed to load categories"),
      apiRequest(api.get("/api/menuitems?pageSize=50"), "Failed to load menu items")
    ]);

    setCategories(categoryData);
    setMenuItems(menuData.items ?? []);
  };

  useEffect(() => {
    loadData().finally(() => setLoading(false));
  }, []);

  const resetForm = () => {
    setEditingId(null);
    setImageFile(null);
    setForm({ categoryId: categories[0]?.id ?? "", name: "", description: "", price: 0, discountPercentage: 0, isAvailable: true, imageUrl: "", preparationTimeMinutes: 15 });
  };

  useEffect(() => {
    if (!form.categoryId && categories.length) {
      setForm((current) => ({ ...current, categoryId: categories[0].id }));
    }
  }, [categories]);

  const uploadImageIfNeeded = async () => {
    if (!imageFile) {
      return form.imageUrl;
    }

    setUploading(true);
    try {
      const body = new FormData();
      body.append("file", imageFile);
      const data = await apiRequest(api.post("/api/uploads/image", body, { headers: { "Content-Type": "multipart/form-data" } }), "Image upload failed");
      return data.path;
    } finally {
      setUploading(false);
    }
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    try {
      const imageUrl = await uploadImageIfNeeded();
      const payload = { ...form, imageUrl, price: Number(form.price), discountPercentage: Number(form.discountPercentage), preparationTimeMinutes: Number(form.preparationTimeMinutes) };
      if (editingId) {
        await apiRequest(api.put(`/api/menuitems/${editingId}`, payload), "Failed to update menu item");
        showToast("Menu item updated.");
      } else {
        await apiRequest(api.post("/api/menuitems", payload), "Failed to create menu item");
        showToast("Menu item created.");
      }

      resetForm();
      await loadData();
    } catch (error) {
      showToast(error.message, "error");
    }
  };

  const handleDelete = async (id) => {
    try {
      await apiRequest(api.delete(`/api/menuitems/${id}`), "Failed to delete menu item");
      showToast("Menu item deleted.");
      await loadData();
    } catch (error) {
      showToast(error.message, "error");
    }
  };

  return (
    <DashboardLayout title="Menu Management">
      <div className="row g-4">
        <div className="col-xl-5">
          <ManagementForm title={editingId ? "Edit Menu Item" : "Add Menu Item"} onSubmit={handleSubmit} submitLabel={uploading ? "Uploading..." : editingId ? "Update Menu Item" : "Create Menu Item"}>
            <div className="col-12">
              <label className="form-label">Category</label>
              <select className="form-select" required value={form.categoryId} onChange={(event) => setForm((current) => ({ ...current, categoryId: event.target.value }))}>
                {categories.map((category) => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="col-12">
              <label className="form-label">Name</label>
              <input className="form-control" required value={form.name} onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))} />
            </div>
            <div className="col-12">
              <label className="form-label">Description</label>
              <textarea className="form-control" rows="3" value={form.description} onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))} />
            </div>
            <div className="col-md-4">
              <label className="form-label">Price</label>
              <input className="form-control" type="number" step="0.01" value={form.price} onChange={(event) => setForm((current) => ({ ...current, price: event.target.value }))} />
            </div>
            <div className="col-md-4">
              <label className="form-label">Discount %</label>
              <input className="form-control" type="number" step="0.01" value={form.discountPercentage} onChange={(event) => setForm((current) => ({ ...current, discountPercentage: event.target.value }))} />
            </div>
            <div className="col-md-4">
              <label className="form-label">Prep Time</label>
              <input className="form-control" type="number" value={form.preparationTimeMinutes} onChange={(event) => setForm((current) => ({ ...current, preparationTimeMinutes: event.target.value }))} />
            </div>
            <div className="col-12">
              <label className="form-label">Image URL</label>
              <input className="form-control" value={form.imageUrl} onChange={(event) => setForm((current) => ({ ...current, imageUrl: event.target.value }))} />
            </div>
            <div className="col-12">
              <label className="form-label">Upload Image</label>
              <input className="form-control" type="file" accept=".jpg,.jpeg,.png,.webp" onChange={(event) => setImageFile(event.target.files?.[0] ?? null)} />
            </div>
            <div className="col-12">
              <div className="form-check">
                <input className="form-check-input" type="checkbox" checked={form.isAvailable} onChange={(event) => setForm((current) => ({ ...current, isAvailable: event.target.checked }))} />
                <label className="form-check-label">Available for ordering</label>
              </div>
            </div>
            {editingId ? (
              <div className="col-12 d-grid">
                <button type="button" className="btn btn-outline-dark" onClick={resetForm}>
                  Cancel Edit
                </button>
              </div>
            ) : null}
          </ManagementForm>
        </div>
        <div className="col-xl-7">
          <div className="glass-panel p-4 h-100">
            <SectionHeading title="Menu Items" subtitle="Search results from the live menu catalog." />
            {loading ? (
              <LoadingState />
            ) : (
              <DataTable
                columns={[
                  {
                    key: "image",
                    label: "Image",
                    render: (row) =>
                      (
                        <ImageWithFallback
                          src={row.imageUrl ? resolveImageUrl(row.imageUrl) : resolveImageUrl(getMenuFallback(row))}
                          fallbackSrc={resolveImageUrl(getMenuFallback(row))}
                          alt={row.name}
                          style={{ width: 48, height: 48, objectFit: "cover", borderRadius: 12 }}
                        />
                      )
                  },
                  { key: "name", label: "Name" },
                  { key: "categoryName", label: "Category" },
                  { key: "finalPrice", label: "Price", render: (row) => formatCurrency(row.finalPrice) },
                  { key: "isAvailable", label: "Status", render: (row) => (row.isAvailable ? "Available" : "Hidden") },
                  {
                    key: "actions",
                    label: "Actions",
                    render: (row) => (
                      <div className="d-flex gap-2">
                        <button
                          className="btn btn-sm btn-outline-dark"
                          onClick={() => {
                            setEditingId(row.id);
                            setForm({
                              categoryId: row.categoryId,
                              name: row.name,
                              description: row.description ?? "",
                              price: row.price,
                              discountPercentage: row.discountPercentage,
                              isAvailable: row.isAvailable,
                              imageUrl: row.imageUrl ?? "",
                              preparationTimeMinutes: row.preparationTimeMinutes
                            });
                          }}
                        >
                          Edit
                        </button>
                        <button className="btn btn-sm btn-outline-danger" onClick={() => handleDelete(row.id)}>
                          Delete
                        </button>
                      </div>
                    )
                  }
                ]}
                rows={menuItems}
              />
            )}
          </div>
        </div>
      </div>
    </DashboardLayout>
  );
}
