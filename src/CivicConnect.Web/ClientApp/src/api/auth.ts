import type { AuthState, UserRole } from "./types";

const STORAGE_KEY = "civicconnect_auth";

type Session = { userId: string; role: string };

export function getAuth(): AuthState | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AuthState;
  } catch {
    localStorage.removeItem(STORAGE_KEY);
    return null;
  }
}

async function getSession(headers: HeadersInit): Promise<Session | null> {
  const response = await fetch("/api/auth/session", { headers });
  return response.ok ? await response.json() as Session : null;
}

async function tryJwt(userId: string, role: UserRole): Promise<AuthState | null> {
  const response = await fetch(`/api/TestAuth/token/${encodeURIComponent(userId)}?role=${role.toUpperCase()}`);
  // Only a missing debug endpoint permits header fallback; rejected or failed JWT login does not.
  if (response.status === 404) return null;
  if (!response.ok) throw new Error("Token request failed.");

  const { token } = await response.json() as { token?: string };
  if (typeof token !== "string" || !token.trim()) throw new Error("Invalid token response.");

  const session = await getSession({ Authorization: `Bearer ${token}` });
  if (!sessionMatches(session, userId, role)) throw new Error("Token verification failed.");
  return { userId, role, token };
}

function sessionMatches(session: Session | null, userId: string, role: UserRole): boolean {
  return session?.userId === userId && session.role.toUpperCase() === role.toUpperCase();
}

// ponytail: remove demo login when a real identity provider supplies credentials and tokens.
export async function login(userId: string, role: UserRole, allowTestFallback: boolean): Promise<AuthState | null> {
  logout();
  userId = userId.trim();
  if (!userId || !["Requester", "Staff", "Manager"].includes(role)) return null;

  const auth = await tryJwt(userId, role);
  const verifiedAuth = auth ?? (allowTestFallback ? await tryTestHeaders(userId, role) : null);
  if (verifiedAuth) localStorage.setItem(STORAGE_KEY, JSON.stringify(verifiedAuth));
  return verifiedAuth;
}

async function tryTestHeaders(userId: string, role: UserRole): Promise<AuthState | null> {
  const session = await getSession({ "X-Test-Role": role.toUpperCase(), "X-Test-User": userId });
  return sessionMatches(session, userId, role) ? { userId, role, token: null } : null;
}

export function logout(): void {
  localStorage.removeItem(STORAGE_KEY);
  window.dispatchEvent(new Event("auth:change"));
}
