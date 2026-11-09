import { api } from "../../shared/api/client";
import type { PagedResult } from "../../shared/api/types";
import type {
  SaveStudentPayload,
  StudentDetail,
  StudentListItem,
  StudentListParams,
} from "./types";

function toQuery(params: StudentListParams): string {
  const query = new URLSearchParams();
  if (params.search?.trim()) {
    query.set("search", params.search.trim());
  }
  query.set("status", params.status ?? "active");
  query.set("sort", params.sort ?? "name");
  query.set("page", String(params.page ?? 1));
  query.set("pageSize", String(params.pageSize ?? 25));
  if (params.excludeClassId) {
    query.set("excludeClassId", params.excludeClassId);
  }
  return query.toString();
}

export function listStudents(params: StudentListParams): Promise<PagedResult<StudentListItem>> {
  return api<PagedResult<StudentListItem>>(`/api/students?${toQuery(params)}`);
}

export function getStudent(id: string): Promise<StudentDetail> {
  return api<StudentDetail>(`/api/students/${id}`);
}

export function createStudent(payload: SaveStudentPayload): Promise<StudentDetail> {
  return api<StudentDetail>("/api/students", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function updateStudent(id: string, payload: SaveStudentPayload): Promise<StudentDetail> {
  return api<StudentDetail>(`/api/students/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export function archiveStudent(id: string): Promise<void> {
  return api<void>(`/api/students/${id}/archive`, { method: "POST" });
}

export function restoreStudent(id: string): Promise<void> {
  return api<void>(`/api/students/${id}/restore`, { method: "POST" });
}
