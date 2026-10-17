export type RequestStateName =
  | "Open"
  | "Assigned"
  | "InProgress"
  | "Resolved"
  | "Closed"
  | "Cancelled"
  | "Rejected";

export type RequestState = RequestStateName | number;

export const requestStateName = (state: RequestState): RequestStateName =>
  typeof state === "number"
    ? (["Open", "Assigned", "InProgress", "Resolved", "Closed", "Cancelled", "Rejected"] as const)[state] ?? "Open"
    : state;

export const requestStateValue = (state: RequestStateName): number =>
  ["Open", "Assigned", "InProgress", "Resolved", "Closed", "Cancelled", "Rejected"].indexOf(state);

export type UserRole = "Requester" | "Staff" | "Manager";

export interface AuthState { role: UserRole; userId: string; token: string | null; }

export interface LoginRequest { username: string; password: string; }

export interface RegisterRequest { username: string; email: string; password: string; role?: UserRole; }

export interface AuthResponse { userId: string; role: UserRole; token: string; }

export interface ProvisionUserRequest { username: string; email: string; password: string; role: "Staff" | "Manager"; }
export interface ProvisionedUser { id: number; username: string; email: string; role: "Staff" | "Manager"; }


export interface Category { id: number; name: string; description: string; }

export interface RequestComment {
  id: number; requestId: number; text: string; createdAt: string; createdBy: string; isInternal: boolean;
}

export interface StatusHistory {
  id: number; requestId: number; previousState: RequestState; newState: RequestState;
  changedAt: string; changedBy: string; reason?: string | null;
}

export interface Feedback {
  id: number; requestId: number; rating: number; comment: string; createdAt: string; createdBy: string;
}

export interface Request {
  id: number; title: string; description: string; categoryId: number; categoryName: string;
  state: RequestState; createdAt: string; requesterId: string; assigneeId?: string | null;
  comments: RequestComment[]; feedback?: Feedback | null;
  allowedTransitions: RequestState[]; history: StatusHistory[];
}

export interface StaffRequestListItemDto {
  id: number; title: string; description: string; categoryId: number; categoryName: string;
  state: RequestState; createdAt: string; requesterId: string; assigneeId?: string | null;
}

export interface RequesterRequestListItemDto {
  id: number; title: string; categoryId: number; categoryName: string;
  state: RequestState; stateName: string; createdAt: string;
}

export interface NotificationDto {
  id: number; requestId: number; message: string; createdAt: string; readAt?: string | null;
}

export interface PagedResultDto<T> {
  items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number;
}

export interface ProblemDetails { title?: string; detail?: string; errors?: Record<string, string[]>; message?: string; }
