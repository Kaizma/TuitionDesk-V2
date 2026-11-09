import { api } from "../../shared/api/client";
import type { RosterFilters, RosterPage } from "./types";

export function getRoster(classId: string, filters: RosterFilters): Promise<RosterPage> {
  const query = new URLSearchParams();
  if (filters.search?.trim()) {
    query.set("search", filters.search.trim());
  }
  query.set("status", filters.status ?? "active");
  query.set("page", String(filters.page ?? 1));
  query.set("pageSize", String(filters.pageSize ?? 25));

  return api<RosterPage>(`/api/classes/${classId}/roster?${query.toString()}`);
}

export function enrollStudent(classId: string, studentId: string): Promise<{ id: string }> {
  return api<{ id: string }>(`/api/classes/${classId}/enrollments`, {
    method: "POST",
    body: JSON.stringify({ studentId }),
  });
}

export function bulkEnrollStudents(
  classId: string,
  studentIds: string[],
): Promise<{ added: number }> {
  return api<{ added: number }>(`/api/classes/${classId}/enrollments/bulk`, {
    method: "POST",
    body: JSON.stringify({ studentIds }),
  });
}

export function withdrawEnrollment(enrollmentId: string): Promise<void> {
  return api<void>(`/api/enrollments/${enrollmentId}/withdraw`, { method: "POST" });
}
