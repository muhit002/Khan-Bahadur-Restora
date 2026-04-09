import { createContext, useContext, useEffect, useMemo, useState } from "react";
import api, { apiRequest } from "../api/client";
import { clearStoredAuth, readStoredAuth, saveStoredAuth } from "../utils/authStorage";
import { getDashboardPathForRole, isRoleAllowed, normalizeAuthPayload, normalizeUser } from "../utils/auth";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [authState, setAuthState] = useState(() => readStoredAuth());
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = authState.token;
    if (!token) {
      setLoading(false);
      return;
    }

    apiRequest(api.get("/api/auth/me"), "Failed to restore session")
      .then((user) => {
        const normalizedUser = normalizeUser(user);
        if (!normalizedUser) {
          throw new Error("Session data is incomplete.");
        }

        setAuthState((current) => {
          const updated = { ...current, user: normalizedUser };
          saveStoredAuth(updated);
          return updated;
        });
      })
      .catch(() => {
        clearStoredAuth();
        setAuthState({ token: null, user: null });
      })
      .finally(() => setLoading(false));
  }, []);

  const persistAuth = (payload) => {
    const nextState = normalizeAuthPayload(payload);
    if (!nextState.token || !nextState.user?.role) {
      throw new Error("Authentication response is incomplete. Please try again.");
    }

    saveStoredAuth(nextState);
    setAuthState(nextState);
    return nextState;
  };

  const login = async (credentials) => {
    const data = await apiRequest(api.post("/api/auth/login", credentials), "Login failed");
    return persistAuth(data);
  };

  const register = async (payload) => {
    const data = await apiRequest(api.post("/api/auth/register", payload), "Registration failed");
    return persistAuth(data);
  };

  const logout = async () => {
    try {
      await api.post("/api/auth/logout");
    } catch {
      // Ignore logout failures because the client can still drop the token safely.
    }

    clearStoredAuth();
    setAuthState({ token: null, user: null });
  };

  const value = useMemo(
    () => ({
      token: authState.token,
      user: authState.user,
      dashboardPath: getDashboardPathForRole(authState.user?.role),
      loading,
      isAuthenticated: Boolean(authState.token && authState.user),
      login,
      register,
      logout,
      hasRole: (roles) => isRoleAllowed(authState.user?.role, roles)
    }),
    [authState, loading]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider");
  }

  return context;
}
