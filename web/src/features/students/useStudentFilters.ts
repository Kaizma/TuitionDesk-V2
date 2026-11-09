import { useCallback, useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import type { StudentListParams } from "./types";

const statuses = ["active", "archived", "all"] as const;
const sorts = ["name", "code", "recent"] as const;

function parsePage(value: string | null): number {
  const page = Number(value ?? "1");
  return Number.isFinite(page) && page >= 1 ? Math.floor(page) : 1;
}

export function useStudentFilters() {
  const [params, setParams] = useSearchParams();

  const filters = useMemo<StudentListParams>(() => {
    const status = params.get("status");
    const sort = params.get("sort");

    return {
      search: params.get("search") ?? "",
      status: statuses.includes(status as (typeof statuses)[number])
        ? (status as (typeof statuses)[number])
        : "active",
      sort: sorts.includes(sort as (typeof sorts)[number])
        ? (sort as (typeof sorts)[number])
        : "name",
      page: parsePage(params.get("page")),
      pageSize: 25,
    };
  }, [params]);

  const update = useCallback(
    (patch: Partial<StudentListParams>) => {
      const next = new URLSearchParams(params);
      const search = patch.search ?? filters.search ?? "";
      const status = patch.status ?? filters.status ?? "active";
      const sort = patch.sort ?? filters.sort ?? "name";
      const page = patch.page ?? 1;

      if (search.trim()) {
        next.set("search", search.trim());
      } else {
        next.delete("search");
      }

      if (status && status !== "active") {
        next.set("status", status);
      } else {
        next.delete("status");
      }

      if (sort && sort !== "name") {
        next.set("sort", sort);
      } else {
        next.delete("sort");
      }

      if (page > 1) {
        next.set("page", String(page));
      } else {
        next.delete("page");
      }

      setParams(next, { replace: true });
    },
    [filters.search, filters.sort, filters.status, params, setParams],
  );

  return { filters, update };
}
