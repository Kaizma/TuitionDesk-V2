import { api } from "../../shared/api/client";
import type { PagedResult } from "../../shared/api/types";
import type {
  ClassDetail,
  ClassListItem,
  ClassListParams,
  Lookups,
  SaveClassPayload,
} from "./types";

function toQuery(params: ClassListParams): string {
  const query = new URLSearchParams();
  if (params.search?.trim()) {
    query.set("search", params.search.trim());
  }
  query.set("status", params.status ?? "active");
  query.set("sort", params.sort ?? "name");
  query.set("page", String(params.page ?? 1));
  query.set("pageSize", String(params.pageSize ?? 25));
  if (params.excludeStudentId) {
    query.set("excludeStudentId", params.excludeStudentId);
  }
  return query.toString();
}

export function listClasses(params: ClassListParams): Promise<PagedResult<ClassListItem>> {
  return api<PagedResult<ClassListItem>>(`/api/classes?${toQuery(params)}`);
}

export function getClass(id: string): Promise<ClassDetail> {
  return api<ClassDetail>(`/api/classes/${id}`);
}

export function createClass(payload: SaveClassPayload): Promise<ClassDetail> {
  return api<ClassDetail>("/api/classes", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function updateClass(id: string, payload: SaveClassPayload): Promise<ClassDetail> {
  return api<ClassDetail>(`/api/classes/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export function archiveClass(id: string): Promise<void> {
  return api<void>(`/api/classes/${id}/archive`, { method: "POST" });
}

export function restoreClass(id: string): Promise<void> {
  return api<void>(`/api/classes/${id}/restore`, { method: "POST" });
}

export function getLookups(): Promise<Lookups> {
  return api<Lookups>("/api/lookups");
}
