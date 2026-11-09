import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ApiError } from "../../shared/api/client";
import { classKeys } from "../classes/queries";
import { studentKeys } from "../students/queries";
import { bulkEnrollStudents, enrollStudent, getRoster, withdrawEnrollment } from "./api";
import type { RosterFilters } from "./types";

export const rosterKeys = {
  all: (classId: string) => ["classes", classId, "roster"] as const,
  page: (classId: string, filters: RosterFilters) =>
    [...rosterKeys.all(classId), filters] as const,
};

export function useRosterQuery(classId: string, filters: RosterFilters) {
  return useQuery({
    queryKey: rosterKeys.page(classId, filters),
    queryFn: () => getRoster(classId, filters),
    placeholderData: keepPreviousData,
    retry: (attempt, error) => error instanceof ApiError && error.status >= 500 && attempt < 2,
  });
}

function useInvalidateEnrolments() {
  const queryClient = useQueryClient();
  return () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: classKeys.all }),
      queryClient.invalidateQueries({ queryKey: studentKeys.all }),
    ]);
}

export function useEnrollStudent(classId: string) {
  const invalidate = useInvalidateEnrolments();
  return useMutation({
    mutationFn: (studentId: string) => enrollStudent(classId, studentId),
    onSuccess: () => void invalidate(),
  });
}

export function useBulkEnrollStudents(classId: string) {
  const invalidate = useInvalidateEnrolments();
  return useMutation({
    mutationFn: (studentIds: string[]) => bulkEnrollStudents(classId, studentIds),
    onSuccess: () => void invalidate(),
  });
}

export function useWithdrawEnrollment() {
  const invalidate = useInvalidateEnrolments();
  return useMutation({
    mutationFn: (enrollmentId: string) => withdrawEnrollment(enrollmentId),
    onSuccess: () => void invalidate(),
  });
}
