import axios from "axios";
import { getStoredToken } from "../utils/authStorage";

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000").replace(/\/$/, "");

const api = axios.create({
  baseURL: apiBaseUrl
});

api.interceptors.request.use((config) => {
  const token = getStoredToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

export async function apiRequest(promise, fallbackMessage = "Request failed") {
  try {
    const response = await promise;
    return response.data;
  } catch (error) {
    const validationErrors = error?.response?.data?.errors;
    const firstValidationMessage = validationErrors
      ? Object.values(validationErrors).flat()[0]
      : null;

    const message =
      firstValidationMessage ??
      error?.response?.data?.message ??
      error?.response?.data?.details ??
      fallbackMessage;

    throw new Error(message);
  }
}

export default api;
