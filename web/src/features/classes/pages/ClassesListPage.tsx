import { BookOpen, Plus, Search } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { ApiError } from "../../../shared/api/client";
import { Button, buttonStyles } from "../../../shared/components/Button";
import { EmptyState } from "../../../shared/components/EmptyState";
import { PageHeader } from "../../../shared/components/PageHeader";
import { Pagination } from "../../../shared/components/Pagination";
import { StatusBadge } from "../../../shared/components/StatusBadge";
import { formatMoney } from "../../../shared/lib/money";
import { useArchiveClass, useClassesQuery, useRestoreClass } from "../queries";
import type { ClassListItem } from "../types";
import { useClassFilters } from "../useClassFilters";

export function ClassesListPage() {
  const { filters, update } = useClassFilters();
  const archive = useArchiveClass();
  const restore = useRestoreClass();
  const [search, setSearch] = useState(filters.search);
  const [actionError, setActionError] = useState<string | null>(null);
  const searchTimer = useRef<number | undefined>(undefined);

  useEffect(() => {
    setSearch(filters.search);
  }, [filters.search]);

  useEffect(() => () => window.clearTimeout(searchTimer.current), []);

  const classes = useClassesQuery(filters);
  const rows = classes.data?.items ?? [];
  const hasFilters = Boolean(filters.search) || filters.status !== "active" || filters.sort !== "name";

  function onSearchChange(value: string) {
    setSearch(value);
    window.clearTimeout(searchTimer.current);
    searchTimer.current = window.setTimeout(() => update({ search: value, page: 1 }), 250);
  }

  async function toggleStatus(classEntity: ClassListItem) {
    const archiving = classEntity.status === "active";
    const message = archiving
      ? `Archive ${classEntity.name}? Active enrolments will be withdrawn.`
      : `Restore ${classEntity.name}? Enrolments are not restored automatically.`;

    if (!window.confirm(message)) {
      return;
    }

    setActionError(null);
    try {
      if (archiving) {
        await archive.mutateAsync(classEntity.id);
      } else {
        await restore.mutateAsync(classEntity.id);
      }
    } catch (error) {
      setActionError(error instanceof Error ? error.message : "The action could not be completed.");
    }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Classes"
        description="Every recurring group, its schedule, fee, and roster."
        actions={
          <Link to="/classes/new" className={buttonStyles()}>
            <Plus className="h-4 w-4" aria-hidden="true" />
            New class
          </Link>
        }
      />

      {actionError ? (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
          {actionError}
        </div>
      ) : null}

      <section className="rounded-xl border border-line bg-surface shadow-sm">
        <div className="flex flex-wrap items-end gap-3 border-b border-line p-4">
          <div className="min-w-56 flex-1">
            <label htmlFor="class-search" className="sr-only">
              Search classes
            </label>
            <div className="relative">
              <Search
                className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400"
                aria-hidden="true"
              />
              <input
                id="class-search"
                type="search"
                value={search}
                onChange={(event) => onSearchChange(event.target.value)}
                placeholder="Search by name, subject, or code"
                className="h-10 w-full rounded-lg border border-line bg-surface pl-9 pr-3 text-sm outline-none transition focus:border-brand focus:ring-2 focus:ring-brand/20"
              />
            </div>
          </div>

          <div>
            <label htmlFor="class-status" className="sr-only">Status</label>
            <select
              id="class-status"
              value={filters.status}
              onChange={(event) =>
                update({ status: event.target.value as "active" | "archived" | "all", page: 1 })
              }
              className="h-10 rounded-lg border border-line bg-surface px-3 text-sm text-slate-700 outline-none focus:border-brand focus:ring-2 focus:ring-brand/20"
            >
              <option value="active">Active</option>
              <option value="archived">Archived</option>
              <option value="all">All</option>
            </select>
          </div>

          <div>
            <label htmlFor="class-sort" className="sr-only">Sort</label>
            <select
              id="class-sort"
              value={filters.sort}
              onChange={(event) =>
                update({ sort: event.target.value as "name" | "code" | "recent", page: 1 })
              }
              className="h-10 rounded-lg border border-line bg-surface px-3 text-sm text-slate-700 outline-none focus:border-brand focus:ring-2 focus:ring-brand/20"
            >
              <option value="name">Name A–Z</option>
              <option value="code">Code</option>
              <option value="recent">Recently updated</option>
            </select>
          </div>
        </div>

        {classes.isPending ? (
          <TableSkeleton />
        ) : classes.isError ? (
          <div className="p-8 text-center">
            <p className="text-sm font-medium text-red-800">
              {classes.error instanceof ApiError ? classes.error.message : "Classes could not be loaded."}
            </p>
            <Button className="mt-4" variant="secondary" onClick={() => void classes.refetch()}>
              Try again
            </Button>
          </div>
        ) : rows.length === 0 ? (
          <div className="p-6">
            {hasFilters ? (
              <EmptyState
                icon={BookOpen}
                title="No classes match these filters"
                description="Try a different search, or clear the filters to see everything."
                action={
                  <Button
                    variant="secondary"
                    onClick={() => update({ search: "", status: "active", sort: "name", page: 1 })}
                  >
                    Clear filters
                  </Button>
                }
              />
            ) : (
              <EmptyState
                icon={BookOpen}
                title="Set up your first class"
                description="Add a subject, schedule, fee, and optional capacity, then start allocating students."
                action={
                  <Link to="/classes/new" className={buttonStyles()}>
                    <Plus className="h-4 w-4" aria-hidden="true" />
                    Add class
                  </Link>
                }
              />
            )}
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full min-w-[820px] text-left text-sm">
                <thead className="border-b border-line bg-slate-50/60 text-[11px] uppercase tracking-wider text-muted">
                  <tr>
                    <th scope="col" className="px-4 py-3 font-semibold">Code</th>
                    <th scope="col" className="px-4 py-3 font-semibold">Class</th>
                    <th scope="col" className="px-4 py-3 font-semibold">Schedule</th>
                    <th scope="col" className="px-4 py-3 font-semibold">Enrolled</th>
                    <th scope="col" className="px-4 py-3 font-semibold">Fee</th>
                    <th scope="col" className="px-4 py-3 font-semibold">Status</th>
                    <th scope="col" className="px-4 py-3 text-right font-semibold">Actions</th>
                  </tr>
                </thead>
                <tbody className={classes.isFetching ? "opacity-60 transition-opacity" : undefined}>
                  {rows.map((classEntity) => {
                    const full =
                      classEntity.capacity !== null &&
                      classEntity.activeEnrollmentCount >= classEntity.capacity;

                    return (
                      <tr
                        key={classEntity.id}
                        className="border-b border-line last:border-b-0 hover:bg-slate-50/60"
                      >
                        <td className="whitespace-nowrap px-4 py-3 font-mono text-xs text-slate-500">
                          {classEntity.code}
                        </td>
                        <td className="px-4 py-3">
                          <Link
                            to={`/classes/${classEntity.id}`}
                            className="font-medium text-ink underline-offset-2 hover:text-brand hover:underline"
                          >
                            {classEntity.name}
                          </Link>
                          <p className="mt-0.5 text-xs text-slate-400">
                            {classEntity.subject}
                            {classEntity.gradeLevel ? ` · ${classEntity.gradeLevel}` : ""}
                          </p>
                        </td>
                        <td className="px-4 py-3 text-slate-600">
                          {classEntity.scheduleText ?? "—"}
                        </td>
                        <td className="whitespace-nowrap px-4 py-3 text-slate-600">
                          <span className={full ? "font-semibold text-amber-700" : undefined}>
                            {classEntity.activeEnrollmentCount}
                            {classEntity.capacity !== null ? ` / ${classEntity.capacity}` : ""}
                          </span>
                          {full ? (
                            <span className="ml-2 rounded-full bg-amber-50 px-2 py-0.5 text-[11px] font-semibold text-amber-700">
                              Full
                            </span>
                          ) : null}
                        </td>
                        <td className="whitespace-nowrap px-4 py-3 text-slate-600">
                          {formatMoney(classEntity.monthlyFeeMinor, classEntity.currency)}
                        </td>
                        <td className="px-4 py-3">
                          <StatusBadge status={classEntity.status} />
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex items-center justify-end gap-2">
                            <Link
                              to={`/classes/${classEntity.id}/edit`}
                              className={buttonStyles("secondary", "sm")}
                            >
                              Edit
                            </Link>
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => void toggleStatus(classEntity)}
                              disabled={archive.isPending || restore.isPending}
                            >
                              {classEntity.status === "active" ? "Archive" : "Restore"}
                            </Button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>

            <div className="px-4 pb-4">
              <Pagination
                page={classes.data?.page ?? 1}
                totalPages={classes.data?.totalPages ?? 1}
                totalCount={classes.data?.totalCount ?? 0}
                pageSize={classes.data?.pageSize ?? filters.pageSize ?? 25}
                onPage={(page) => update({ page })}
              />
            </div>
          </>
        )}
      </section>
    </div>
  );
}

function TableSkeleton() {
  return (
    <div className="space-y-3 p-4" aria-label="Loading classes" role="status">
      {Array.from({ length: 5 }).map((_, index) => (
        <div key={index} className="grid grid-cols-[80px_1.5fr_1fr_80px_110px] gap-4">
          <div className="h-4 animate-pulse rounded bg-slate-100" />
          <div className="h-4 animate-pulse rounded bg-slate-100" />
          <div className="h-4 animate-pulse rounded bg-slate-100" />
          <div className="h-4 animate-pulse rounded bg-slate-100" />
          <div className="h-4 animate-pulse rounded bg-slate-100" />
        </div>
      ))}
    </div>
  );
}
