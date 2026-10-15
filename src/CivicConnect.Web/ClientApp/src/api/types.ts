export type RequestStateName =
  | "Open"
  | "Assigned"
  | "InProgress"
  | "Resolved"
  | "Closed"
  | "Cancelled"
  | "Rejected";

// ASP.NET Core serializes enums as integers unless JsonStringEnumConverter is configured.
export type RequestState = RequestStateName | number;

export const requestStateName = (state: RequestState): RequestStateName =>
  typeof state === "number"
    ? (["Open", "Assigned", "InProgress", "Resolved", "Closed", "Cancelled", "Rejected"] as const)[state] ?? "Open"
    : state;

// Numeric enum value for JSON bodies (ASP.NET DTOs accept the default number form).
export const requestStateValue = (state: RequestStateName): number =>
  ["Open", "Assigned", "InProgress", "Resolved", "Closed", "Cancelled", "Rejected"].indexOf(state);

export type UserRole = "Requester" | "Staff" | "Manager";

export interface AuthState {
  role: UserRole;
  userId: string;
  token: string | null;
}

export interface RequestComment {
  id: number;
  requestId: number;
  text: string;
  createdAt: string;
  createdBy: string;
  isInternal: boolean;
}

export interface StatusHistory {
  id: number;
  requestId: number;
  previousState: RequestState;
  newState: RequestState;
  changedAt: string;
  changedBy: string;
  reason?: string | null;
}

export interface Request {
  id: number;
  title: string;
  description: string;
  categoryId: number;
  state: RequestState;
  createdAt: string;
  requesterId: string;
  assigneeId?: string | null;
  history: StatusHistory[];
  comments: RequestComment[];
}

export interface StaffRequestListItemDto {
  id: number;
  title: string;
  description: string;
  categoryId: number;
  categoryName: string;
  state: RequestState;
  createdAt: string;
  requesterId: string;
  assigneeId?: string | null;
}

export interface PagedResultDto<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
