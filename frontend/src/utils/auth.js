import { roles } from "./constants";

const canonicalRoles = Object.values(roles);

const roleLookup = canonicalRoles.reduce((lookup, role) => {
  lookup[role.toLowerCase()] = role;
  return lookup;
}, {});

const dashboardPaths = {
  [roles.admin]: "/dashboard/admin",
  [roles.manager]: "/dashboard/manager",
  [roles.cashier]: "/dashboard/cashier",
  [roles.chef]: "/dashboard/chef",
  [roles.waiter]: "/dashboard/waiter",
  [roles.customer]: "/dashboard/customer"
};

export function normalizeRole(role) {
  if (typeof role !== "string") {
    return null;
  }

  const trimmed = role.trim().toLowerCase();
  return roleLookup[trimmed] ?? null;
}

export function normalizeUser(user) {
  if (!user || typeof user !== "object") {
    return null;
  }

  return {
    id: user.id ?? null,
    fullName: user.fullName ?? user.name ?? "",
    email: user.email ?? "",
    role: normalizeRole(user.role),
    isActive: user.isActive ?? true,
    phoneNumber: user.phoneNumber ?? "",
    address: user.address ?? ""
  };
}

export function normalizeAuthPayload(payload) {
  if (!payload || typeof payload !== "object") {
    return { token: null, user: null };
  }

  const token =
    typeof payload.token === "string" && payload.token.trim().length > 0
      ? payload.token.trim()
      : null;

  return {
    token,
    user: normalizeUser(payload.user)
  };
}

export function getDashboardPathForRole(role) {
  const normalizedRole = normalizeRole(role);
  return normalizedRole ? dashboardPaths[normalizedRole] : null;
}

export function isRoleAllowed(userRole, allowedRoles = []) {
  const normalizedUserRole = normalizeRole(userRole);
  if (!normalizedUserRole) {
    return false;
  }

  if (!allowedRoles.length) {
    return true;
  }

  return allowedRoles
    .map((role) => normalizeRole(role))
    .filter(Boolean)
    .includes(normalizedUserRole);
}
