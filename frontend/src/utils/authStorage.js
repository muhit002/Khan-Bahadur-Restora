import { normalizeAuthPayload } from "./auth";

export const AUTH_STORAGE_KEY = "restaurant_auth";
const LEGACY_TOKEN_KEY = "restaurant_token";

const EMPTY_AUTH_STATE = { token: null, user: null };

export function readStoredAuth() {
  const raw = localStorage.getItem(AUTH_STORAGE_KEY);

  if (!raw) {
    return EMPTY_AUTH_STATE;
  }

  try {
    return normalizeAuthPayload(JSON.parse(raw));
  } catch {
    clearStoredAuth();
    return EMPTY_AUTH_STATE;
  }
}

export function saveStoredAuth(authState) {
  const normalized = normalizeAuthPayload(authState);

  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(normalized));

  if (normalized.token) {
    localStorage.setItem(LEGACY_TOKEN_KEY, normalized.token);
    return;
  }

  localStorage.removeItem(LEGACY_TOKEN_KEY);
}

export function clearStoredAuth() {
  localStorage.removeItem(AUTH_STORAGE_KEY);
  localStorage.removeItem(LEGACY_TOKEN_KEY);
}

export function getStoredToken() {
  const legacyToken = localStorage.getItem(LEGACY_TOKEN_KEY);
  if (legacyToken) {
    return legacyToken;
  }

  return readStoredAuth().token;
}
