export type ClassStatus = "active" | "archived";

export interface ClassListItem {
  id: string;
  code: string;
  name: string;
  subject: string;
  gradeLevel: string | null;
  scheduleText: string | null;
  activeEnrollmentCount: number;
  capacity: number | null;
  monthlyFeeMinor: number;
  currency: string;
  status: ClassStatus;
}

export interface ClassDetail {
  id: string;
  code: string;
  name: string;
  subject: string;
  gradeLevel: string | null;
  scheduleText: string | null;
  startDate: string;
  endDate: string | null;
  monthlyFeeMinor: number;
  currency: string;
  capacity: number | null;
  notes: string | null;
  status: ClassStatus;
  activeEnrollmentCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface ClassListParams {
  search?: string;
  status?: "active" | "archived" | "all";
  sort?: "name" | "code" | "recent";
  page?: number;
  pageSize?: number;
  excludeStudentId?: string;
}

export interface SaveClassPayload {
  name: string;
  subject: string;
  gradeLevel: string | null;
  scheduleText: string | null;
  startDate: string;
  endDate: string | null;
  monthlyFeeMinor: number;
  currency: string;
  capacity: number | null;
  notes: string | null;
  acknowledgeDuplicate?: boolean;
}

export interface Lookups {
  subjects: string[];
  gradeLevels: string[];
}
