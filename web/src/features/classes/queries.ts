import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ApiError } from "../../shared/api/client";
import {
  archiveClass,
  createClass,
  getClass,
  getLookups,
  listClasses,
  restoreClass,
  updateClass,
} from "./api";
import type { ClassListParams, SaveClassPayload } from "./types";

export const classKeys = {
  all: ["classes"] as const,
  lists: () => [...classKeys.all, "list"] as const,
  list: (filters: ClassListParams) => [...classKeys.lists(), filters] as const,
  details: () => [...classKeys.all, "detail"] as const,
  detail: (id: string) => [...classKeys.details(), id] as const,
  lookups: ["lookups"] as const,
};

export function useClassesQuery(filters: ClassListParams) {
  return useQuery({
    queryKey: classKeys.list(filters),
    queryFn: () => listClasses(filters),
    placeholderData: keepPreviousData,
    retry: (attempt, error) => error instanceof ApiError && error.status >= 500 && attempt < 2,
  });
}

export function useClassQuery(id: string | undefined) {
  return useQuery({
    queryKey: classKeys.detail(id ?? ""),
    queryFn: () => getClass(id as string),
    enabled: Boolean(id),
  });
}

export function useLookupsQuery() {
  return useQuery({
    queryKey: classKeys.lookups,
    queryFn: getLookups,
    staleTime: 30_000,
  });
}

function useInvalidateClasses() {
  const queryClient = useQueryClient();
  return async () => {
    await queryClient.invalidateQueries({ queryKey: classKeys.all });
    await queryClient.invalidateQueries({ queryKey: classKeys.lookups });
  };
}

export function useCreateClass() {
  const invalidate = useInvalidateClasses();
  return useMutation({
    mutationFn: (payload: SaveClassPayload) => createClass(payload),
    onSuccess: () => void invalidate(),
  });
}

export function useUpdateClass(id: string) {
  const invalidate = useInvalidateClasses();
  return useMutation({
    mutationFn: (payload: SaveClassPayload) => updateClass(id, payload),
    onSuccess: () => void invalidate(),
  });
}

export function useArchiveClass() {
  const invalidate = useInvalidateClasses();
  return useMutation({
    mutationFn: (id: string) => archiveClass(id),
    onSuccess: () => void invalidate(),
  });
}

export function useRestoreClass() {
  const invalidate = useInvalidateClasses();
  return useMutation({
    mutationFn: (id: string) => restoreClass(id),
    onSuccess: () => void invalidate(),
  });
}
