import type { AuthResponse, AuthState, LoginRequest, RegisterRequest, UserRole } from "./types";

const STORAGE_KEY = "civicconnect_auth";
const roles: UserRole[] = ["Requester", "Staff", "Manager"];

type Session = { userId: string; role: string };

type ApiError = { message?: string; title?: string; detail?: string; errors?: Record<string, string[]> };

export function getAuth(): AuthState | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;

  try {
    const auth = JSON.parse(raw) as AuthState;
    if (!auth.userId || !roles.includes(auth.role) || !auth.token) throw new Error("Invalid stored session.");
    return auth;
  } catch {
    localStorage.removeItem(STORAGE_KEY);
    return null;
  }
}

async function request<T>(path: string, body: LoginRequest | RegisterRequest): Promise<T> {
  const response = await fetch(path, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    let error: ApiError = {};
    try { error = await response.json() as ApiError; } catch { /* use status fallback */ }
    const validationMessage = error.errors ? Object.values(error.errors).flat()[0] : undefined;
    throw new Error(validationMessage ?? error.message ?? error.detail ?? error.title ?? `Request failed (${response.status}).`);
  }

  return await response.json() as T;
}

function toAuthState(response: AuthResponse): AuthState {
  if (!response.userId || !response.token || !roles.includes(response.role)) {
    throw new Error("Invalid authentication response.");
  }
  return { userId: response.userId, role: response.role, token: response.token };
}

function storeAuth(auth: AuthState): AuthState {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(auth));
  return auth;
}

export async function login(username: string, password: string): Promise<AuthState> {
  logout();
  const response = await request<AuthResponse>("/api/auth/login", { username: username.trim(), password });
  return storeAuth(toAuthState(response));
}

export async function register(username: string, email: string, password: string, role: UserRole): Promise<AuthState> {
  logout();
  const response = await request<AuthResponse>("/api/auth/register", {
    username: username.trim(),
    email: email.trim(),
    password,
    role,
  });
  return storeAuth(toAuthState(response));
}

export function logout(): void {
  localStorage.removeItem(STORAGE_KEY);
  window.dispatchEvent(new Event("auth:change"));
}

export async function getSession(headers: HeadersInit): Promise<Session | null> {
  const response = await fetch("/api/auth/session", { headers });
  return response.ok ? await response.json() as Session : null;
}
