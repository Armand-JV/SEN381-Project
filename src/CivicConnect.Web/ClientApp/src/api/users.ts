import axios from "axios";
import api from "./client";
import type { ProblemDetails, ProvisionedUser, ProvisionUserRequest } from "./types";

export async function provisionUser(body: ProvisionUserRequest): Promise<ProvisionedUser> {
  try {
    const { data } = await api.post<ProvisionedUser>("/auth/users", body);
    return data;
  } catch (reason) {
    if (axios.isAxiosError<ProblemDetails>(reason)) {
      const error = reason.response?.data;
      const validation = error?.errors ? Object.values(error.errors).flat()[0] : undefined;
      throw new Error(validation ?? error?.message ?? error?.detail ?? `Request failed (${reason.response?.status ?? "network error"}).`);
    }
    throw reason;
  }
}
