import type { PagedResult } from "../../shared/api/types";

export interface RosterItem {
  enrollmentId: string;
  studentId: string;
  studentCode: string;
  fullName: string;
  phone: string;
  gradeLevel: string | null;
  enrolledOn: string;
  status: "active" | "withdrawn";
  withdrawnOn: string | null;
  studentStatus: "active" | "archived";
}

export type RosterPage = PagedResult<RosterItem>;

export interface RosterFilters {
  search?: string;
  status?: "active" | "withdrawn" | "all";
  page?: number;
  pageSize?: number;
}
