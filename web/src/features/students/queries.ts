import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ApiError } from "../../shared/api/client";
import {
  archiveStudent,
  createStudent,
  getStudent,
  listStudents,
  restoreStudent,
  updateStudent,
} from "./api";
import type { SaveStudentPayload, StudentListParams } from "./types";

export const studentKeys = {
  all: ["students"] as const,
  lists: () => [...studentKeys.all, "list"] as const,
  list: (filters: StudentListParams) => [...studentKeys.lists(), filters] as const,
  details: () => [...studentKeys.all, "detail"] as const,
  detail: (id: string) => [...studentKeys.details(), id] as const,
};

export function useStudentsQuery(filters: StudentListParams) {
  return useQuery({
    queryKey: studentKeys.list(filters),
    queryFn: () => listStudents(filters),
    placeholderData: keepPreviousData,
    retry: (attempt, error) => error instanceof ApiError && error.status >= 500 && attempt < 2,
  });
}

export function useStudentQuery(id: string | undefined) {
  return useQuery({
    queryKey: studentKeys.detail(id ?? ""),
    queryFn: () => getStudent(id as string),
    enabled: Boolean(id),
  });
}

function useInvalidateStudents() {
  const queryClient = useQueryClient();
  return () =>
    queryClient.invalidateQueries({
      queryKey: studentKeys.all,
    });
}

export function useCreateStudent() {
  const invalidate = useInvalidateStudents();
  return useMutation({
    mutationFn: (payload: SaveStudentPayload) => createStudent(payload),
    onSuccess: () => void invalidate(),
  });
}

export function useUpdateStudent(id: string) {
  const invalidate = useInvalidateStudents();
  return useMutation({
    mutationFn: (payload: SaveStudentPayload) => updateStudent(id, payload),
    onSuccess: () => void invalidate(),
  });
}

export function useArchiveStudent() {
  const invalidate = useInvalidateStudents();
  return useMutation({
    mutationFn: (id: string) => archiveStudent(id),
    onSuccess: () => void invalidate(),
  });
}

export function useRestoreStudent() {
  const invalidate = useInvalidateStudents();
  return useMutation({
    mutationFn: (id: string) => restoreStudent(id),
    onSuccess: () => void invalidate(),
  });
}
