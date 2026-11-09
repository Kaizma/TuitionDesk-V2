import { Plus, Search, Users } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { ApiError } from "../../../shared/api/client";
import { Button, buttonStyles } from "../../../shared/components/Button";
import { EmptyState } from "../../../shared/components/EmptyState";
import { PageHeader } from "../../../shared/components/PageHeader";
import { Pagination } from "../../../shared/components/Pagination";
import { StatusBadge } from "../../../shared/components/StatusBadge";
import { formatDate } from "../../../shared/lib/date";
import { useArchiveStudent, useRestoreStudent, useStudentsQuery } from "../queries";
import type { StudentListItem } from "../types";
import { useStudentFilters } from "../useStudentFilters";

export function StudentsListPage() {
  const { filters, update } = useStudentFilters();
  const archive = useArchiveStudent();
  const restore = useRestoreStudent();
  const [search, setSearch] = useState(filters.search);
  const [actionError, setActionError] = useState<string | null>(null);
  const searchTimer = useRef<number | undefined>(undefined);

  useEffect(() => {
    setSearch(filters.search);
  }, [filters.search]);

  useEffect(() => () => window.clearTimeout(searchTimer.current), []);

  const students = useStudentsQuery(filters);
  const rows = students.data?.items ?? [];
  const hasFilters = Boolean(filters.search) || filters.status !== "active" || filters.sort !== "name";

  function onSearchChange(value: string) {
    setSearch(value);
    window.clearTimeout(searchTimer.current);
    searchTimer.current = window.setTimeout(() => update({ search: value, page: 1 }), 250);
  }

  async function toggleStatus(student: StudentListItem) {
    const archiving = student.status === "active";
    const message = archiving
      ? `Archive ${student.fullName}? Their active class enrolments will be withdrawn.`
      : `Restore ${student.fullName}? They will become active again but are not re-enrolled automatically.`;

    if (!window.confirm(message)) {
      return;
    }

    setActionError(null);
    try {
      if (archiving) {
        await archive.mutateAsync(student.id);
      } else {
        await restore.mutateAsync(student.id);
      }
    } catch (error) {
      setActionError(error instanceof Error ? error.message : "The action could not be completed.");
    }
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Students"
        description="Names, contact details, and who is taking what."
        actions={
          <Link to="/students/new" className={buttonStyles()}>
            <Plus className="h-4 w-4" aria-hidden="true" />
            New student
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
            <label htmlFor="student-search" className="sr-only">
              Search students
            </label>
            <div className="relative">
              <Search
                className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400"
                aria-hidden="true"
              />
              <input
                id="student-search"
                type="search"
                value={search}
                onChange={(event) => onSearchChange(event.target.value)}
                placeholder="Search by name, code, or phone"
                className="h-10 w-full rounded-lg border border-line bg-surface pl-9 pr-3 text-sm outline-none transition focus:border-brand focus:ring-2 focus:ring-brand/20"
              />
            </div>
          </div>

          <div>
            <label htmlFor="student-status" className="sr-only">
              Status
            </label>
            <select
              id="student-status"
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
            <label htmlFor="student-sort" className="sr-only">
              Sort
            </label>
            <select
              id="student-sort"
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

        {students.isPending ? (
          <TableSkeleton />
        ) : students.isError ? (
          <div className="p-8 text-center">
            <p className="text-sm font-medium text-red-800">
              {students.error instanceof ApiError
                ? students.error.message
                : "Students could not be loaded."}
            </p>
            <Button className="mt-4" variant="secondary" onClick={() => void students.refetch()}>
              Try again
            </Button>
          </div>
        ) : rows.length === 0 ? (
          <div className="p-6">
            {hasFilters ? (
              <EmptyState
                icon={Users}
                title="No students match these filters"
                description="Try a different search, or clear the filters to see everyone."
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
                icon={Users}
                title="Add your first student"
                description="Keep name, phone, guardian details, and class history in one place."
                action={
                  <Link to="/students/new" className={buttonStyles()}>
                    <Plus className="h-4 w-4" aria-hidden="true" />
                    Add student
                  </Link>
                }
              />
            )}
          </div>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full min-w-[760px] text-left text-sm">
                <thead className="border-b border-line bg-slate-50/60 text-[11px] uppercase tracking-wider text-muted">
                  <tr>
                    <th scope="col" className="px-4 py-3 font-semibold">Code</th>
                    <th scope="col" className="px-4 py-3 font-semibold">Student</th>
                    <th scope="col" className="px-4 py-3 font-semibold">Phone</th>
                    <th scope="col" className="px-4 py-3 font-semibold">Grade</th>
                    <th scope="col" className="px-4 py-3 font-semibold">Classes</th>
                    <th scope="col" className="px-4 py-3 font-semibold">Status</th>
                    <th scope="col" className="px-4 py-3 text-right font-semibold">Actions</th>
                  </tr>
                </thead>
                <tbody className={students.isFetching ? "opacity-60 transition-opacity" : undefined}>
                  {rows.map((student) => (
                    <tr key={student.id} className="border-b border-line last:border-b-0 hover:bg-slate-50/60">
                      <td className="whitespace-nowrap px-4 py-3 font-mono text-xs text-slate-500">
                        {student.code}
                      </td>
                      <td className="px-4 py-3">
                        <Link
                          to={`/students/${student.id}`}
                          className="font-medium text-ink underline-offset-2 hover:text-brand hover:underline"
                        >
                          {student.fullName}
                        </Link>
                        <p className="mt-0.5 text-xs text-slate-400">
                          Updated {formatDate(student.updatedAt)}
                        </p>
                      </td>
                      <td className="whitespace-nowrap px-4 py-3 text-slate-600">{student.phone}</td>
                      <td className="px-4 py-3 text-slate-600">{student.gradeLevel ?? "—"}</td>
                      <td className="px-4 py-3 text-slate-600">
                        {student.activeClassCount === 0
                          ? "—"
                          : `${student.activeClassCount} active`}
                      </td>
                      <td className="px-4 py-3">
                        <StatusBadge status={student.status} />
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-2">
                          <Link
                            to={`/students/${student.id}/edit`}
                            className={buttonStyles("secondary", "sm")}
                          >
                            Edit
                          </Link>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => void toggleStatus(student)}
                            disabled={archive.isPending || restore.isPending}
                          >
                            {student.status === "active" ? "Archive" : "Restore"}
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="px-4 pb-4">
              <Pagination
                page={students.data?.page ?? 1}
                totalPages={students.data?.totalPages ?? 1}
                totalCount={students.data?.totalCount ?? 0}
                pageSize={students.data?.pageSize ?? filters.pageSize ?? 25}
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
    <div className="space-y-3 p-4" aria-label="Loading students" role="status">
      {Array.from({ length: 6 }).map((_, index) => (
        <div key={index} className="grid grid-cols-[80px_1.5fr_1fr_80px_80px] gap-4">
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
