import { Search, UserPlus } from "lucide-react";
import { useEffect, useState } from "react";
import { ApiError } from "../../../shared/api/client";
import { Button } from "../../../shared/components/Button";
import { Dialog } from "../../../shared/components/Dialog";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { useStudentsQuery } from "../../students/queries";
import { useBulkEnrollStudents } from "../queries";

interface AddStudentsDialogProps {
  open: boolean;
  classId: string;
  className: string;
  capacity: number | null;
  enrolledCount: number;
  onClose: () => void;
}

export function AddStudentsDialog({
  open,
  classId,
  className,
  capacity,
  enrolledCount,
  onClose,
}: AddStudentsDialogProps) {
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search);
  const [selected, setSelected] = useState<string[]>([]);
  const [error, setError] = useState<string | null>(null);
  const bulkEnroll = useBulkEnrollStudents(classId);
  const students = useStudentsQuery({
    search: debouncedSearch,
    status: "active",
    page: 1,
    pageSize: 50,
    excludeClassId: classId,
  });

  useEffect(() => {
    if (open) {
      setSearch("");
      setSelected([]);
      setError(null);
    }
  }, [open]);

  function toggle(studentId: string) {
    setSelected((current) =>
      current.includes(studentId)
        ? current.filter((id) => id !== studentId)
        : [...current, studentId],
    );
  }

  async function submit() {
    if (selected.length === 0) {
      return;
    }

    if (
      capacity !== null &&
      enrolledCount + selected.length > capacity &&
      !window.confirm(
        `This class has ${capacity} seats and ${enrolledCount} enrolled. Adding ${selected.length} more will exceed capacity. Continue?`,
      )
    ) {
      return;
    }

    setError(null);
    try {
      await bulkEnroll.mutateAsync(selected);
      onClose();
    } catch (submitError) {
      setError(
        submitError instanceof ApiError
          ? submitError.message
          : "The students could not be enrolled.",
      );
    }
  }

  return (
    <Dialog
      open={open}
      title={`Add students to ${className}`}
      description="Search for active students who are not already enrolled."
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button
            onClick={() => void submit()}
            disabled={selected.length === 0 || bulkEnroll.isPending}
          >
            {bulkEnroll.isPending
              ? "Adding…"
              : `Add ${selected.length || ""} student${selected.length === 1 ? "" : "s"}`.trim()}
          </Button>
        </>
      }
    >
      <div className="space-y-4">
        {error ? (
          <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-800" role="alert">
            {error}
          </div>
        ) : null}

        <div className="relative">
          <Search
            className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400"
            aria-hidden="true"
          />
          <input
            type="search"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search by name, code, or phone"
            aria-label="Search students"
            className="h-10 w-full rounded-lg border border-line bg-surface pl-9 pr-3 text-sm outline-none transition focus:border-brand focus:ring-2 focus:ring-brand/20"
          />
        </div>

        <div className="max-h-72 overflow-y-auto rounded-lg border border-line">
          {students.isPending ? (
            <p className="px-4 py-8 text-center text-sm text-muted">Loading students…</p>
          ) : students.isError ? (
            <p className="px-4 py-8 text-center text-sm text-red-700">
              Students could not be loaded.
            </p>
          ) : students.data?.items.length === 0 ? (
            <p className="px-4 py-8 text-center text-sm text-muted">
              No eligible students match this search.
            </p>
          ) : (
            <ul className="divide-y divide-line">
              {students.data?.items.map((student) => (
                <li key={student.id}>
                  <label className="flex cursor-pointer items-center gap-3 px-4 py-3 hover:bg-slate-50">
                    <input
                      type="checkbox"
                      checked={selected.includes(student.id)}
                      onChange={() => toggle(student.id)}
                      className="h-4 w-4 rounded border-line text-brand focus:ring-brand"
                    />
                    <span className="min-w-0 flex-1">
                      <span className="block text-sm font-medium text-ink">
                        {student.fullName}
                      </span>
                      <span className="block text-xs text-muted">
                        {student.code} · {student.phone}
                        {student.gradeLevel ? ` · ${student.gradeLevel}` : ""}
                      </span>
                    </span>
                  </label>
                </li>
              ))}
            </ul>
          )}
        </div>

        <p className="text-xs text-muted">
          <UserPlus className="mr-1 inline h-3.5 w-3.5 align-text-bottom" aria-hidden="true" />
          {selected.length} selected
          {capacity !== null ? ` · ${Math.max(0, capacity - enrolledCount)} seats available` : " · unlimited capacity"}
        </p>
      </div>
    </Dialog>
  );
}
