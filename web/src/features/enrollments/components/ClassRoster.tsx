import { Plus, Search, Users } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { ApiError } from "../../../shared/api/client";
import { Button } from "../../../shared/components/Button";
import { Pagination } from "../../../shared/components/Pagination";
import { StatusBadge } from "../../../shared/components/StatusBadge";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { formatDate } from "../../../shared/lib/date";
import { useRosterQuery, useWithdrawEnrollment } from "../queries";

interface ClassRosterProps {
  classId: string;
  className: string;
  archived: boolean;
  onAddStudents: () => void;
}

export function ClassRoster({ classId, className, archived, onAddStudents }: ClassRosterProps) {
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search);
  const [status, setStatus] = useState<"active" | "withdrawn" | "all">("active");
  const [page, setPage] = useState(1);
  const [actionError, setActionError] = useState<string | null>(null);
  const roster = useRosterQuery(classId, {
    search: debouncedSearch,
    status,
    page,
    pageSize: 25,
  });
  const withdraw = useWithdrawEnrollment();

  async function withdrawEnrollment(enrollmentId: string, fullName: string) {
    if (!window.confirm(`Withdraw ${fullName} from ${className}? Their history is kept.`)) {
      return;
    }

    setActionError(null);
    try {
      await withdraw.mutateAsync(enrollmentId);
    } catch (error) {
      setActionError(error instanceof Error ? error.message : "The student could not be withdrawn.");
    }
  }

  const rows = roster.data?.items ?? [];

  return (
    <section className="rounded-xl border border-line bg-surface shadow-sm">
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line px-6 py-4">
        <div>
          <h2 className="text-sm font-semibold text-ink">Roster</h2>
          <p className="mt-0.5 text-xs text-muted">
            {roster.data ? `${roster.data.totalCount} shown for this filter` : "Active and past students"}
          </p>
        </div>
        {!archived ? (
          <Button size="sm" onClick={onAddStudents}>
            <Plus className="h-4 w-4" aria-hidden="true" />
            Add students
          </Button>
        ) : null}
      </div>

      {actionError ? (
        <div className="mx-6 mt-4 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-800" role="alert">
          {actionError}
        </div>
      ) : null}

      <div className="flex flex-wrap items-end gap-3 border-b border-line px-6 py-3">
        <div className="min-w-56 flex-1">
          <label htmlFor="roster-search" className="sr-only">
            Search roster
          </label>
          <div className="relative">
            <Search
              className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400"
              aria-hidden="true"
            />
            <input
              id="roster-search"
              type="search"
              value={search}
              onChange={(event) => {
                setSearch(event.target.value);
                setPage(1);
              }}
              placeholder="Search a student in this class"
              className="h-9 w-full rounded-lg border border-line bg-surface pl-9 pr-3 text-sm outline-none transition focus:border-brand focus:ring-2 focus:ring-brand/20"
            />
          </div>
        </div>

        <div>
          <label htmlFor="roster-status" className="sr-only">
            Enrolment status
          </label>
          <select
            id="roster-status"
            value={status}
            onChange={(event) => {
              setStatus(event.target.value as "active" | "withdrawn" | "all");
              setPage(1);
            }}
            className="h-9 rounded-lg border border-line bg-surface px-3 text-sm text-slate-700 outline-none focus:border-brand focus:ring-2 focus:ring-brand/20"
          >
            <option value="active">Active</option>
            <option value="withdrawn">Withdrawn</option>
            <option value="all">All</option>
          </select>
        </div>
      </div>

      {roster.isPending ? (
        <div className="space-y-3 p-6" role="status" aria-label="Loading roster">
          {Array.from({ length: 4 }).map((_, index) => (
            <div key={index} className="h-8 animate-pulse rounded bg-slate-100" />
          ))}
        </div>
      ) : roster.isError ? (
        <div className="p-8 text-center">
          <p className="text-sm font-medium text-red-800">
            {roster.error instanceof ApiError ? roster.error.message : "The roster could not be loaded."}
          </p>
          <Button className="mt-4" variant="secondary" onClick={() => void roster.refetch()}>
            Try again
          </Button>
        </div>
      ) : rows.length === 0 ? (
        <div className="px-6 py-10 text-center">
          <Users className="mx-auto h-8 w-8 text-slate-300" aria-hidden="true" />
          <p className="mt-3 text-sm font-medium text-slate-700">
            {status === "active" ? "No active students in this class" : "No enrolments match this filter"}
          </p>
          <p className="mt-1 text-sm text-muted">
            {archived
              ? "This class is archived, so its roster is read-only."
              : "Use Add students to allocate someone from the student registry."}
          </p>
          {!archived && status === "active" ? (
            <Button className="mt-4" size="sm" onClick={onAddStudents}>
              <Plus className="h-4 w-4" aria-hidden="true" />
              Add students
            </Button>
          ) : null}
        </div>
      ) : (
        <>
          <div className="overflow-x-auto">
            <table className="w-full min-w-[720px] text-left text-sm">
              <thead className="border-b border-line bg-slate-50/60 text-[11px] uppercase tracking-wider text-muted">
                <tr>
                  <th scope="col" className="px-6 py-3 font-semibold">Student</th>
                  <th scope="col" className="px-4 py-3 font-semibold">Code</th>
                  <th scope="col" className="px-4 py-3 font-semibold">Phone</th>
                  <th scope="col" className="px-4 py-3 font-semibold">Grade</th>
                  <th scope="col" className="px-4 py-3 font-semibold">Enrolled</th>
                  <th scope="col" className="px-4 py-3 font-semibold">Status</th>
                  <th scope="col" className="px-4 py-3 text-right font-semibold">Actions</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((row) => (
                  <tr key={row.enrollmentId} className="border-b border-line last:border-b-0 hover:bg-slate-50/60">
                    <td className="px-6 py-3">
                      <Link
                        to={`/students/${row.studentId}`}
                        className="font-medium text-ink underline-offset-2 hover:text-brand hover:underline"
                      >
                        {row.fullName}
                      </Link>
                      {row.studentStatus === "archived" ? (
                        <span className="ml-2 text-[11px] font-medium text-slate-400">archived</span>
                      ) : null}
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 font-mono text-xs text-slate-500">
                      {row.studentCode}
                    </td>
                    <td className="whitespace-nowrap px-4 py-3 text-slate-600">{row.phone}</td>
                    <td className="px-4 py-3 text-slate-600">{row.gradeLevel ?? "—"}</td>
                    <td className="whitespace-nowrap px-4 py-3 text-slate-600">
                      {formatDate(row.enrolledOn)}
                      {row.withdrawnOn ? (
                        <span className="block text-xs text-slate-400">
                          Withdrawn {formatDate(row.withdrawnOn)}
                        </span>
                      ) : null}
                    </td>
                    <td className="px-4 py-3">
                      <StatusBadge status={row.status} />
                    </td>
                    <td className="px-4 py-3 text-right">
                      {row.status === "active" && !archived ? (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => void withdrawEnrollment(row.enrollmentId, row.fullName)}
                          disabled={withdraw.isPending}
                        >
                          Withdraw
                        </Button>
                      ) : null}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="px-6 pb-4">
            <Pagination
              page={roster.data?.page ?? 1}
              totalPages={roster.data?.totalPages ?? 1}
              totalCount={roster.data?.totalCount ?? 0}
              pageSize={roster.data?.pageSize ?? 25}
              onPage={setPage}
            />
          </div>
        </>
      )}
    </section>
  );
}
