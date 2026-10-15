import axios from "axios";
import { getAuth, logout } from "./auth";

const api = axios.create({ baseURL: "/api", timeout: 10000 });

api.interceptors.request.use((config) => {
  const auth = getAuth();
  if (auth?.token) {
    config.headers.Authorization = `Bearer ${auth.token}`;
  } else if (auth) {
    // Development fallback only. Production must use a validated JWT.
    config.headers["X-Test-Role"] = auth.role.toUpperCase();
    config.headers["X-Test-User"] = auth.userId;
  }
  return config;
});

api.interceptors.response.use(undefined, (error) => {
  if (error.response?.status === 401) {
    logout();
  }
  return Promise.reject(error);
});

export default api;
