import { BookOpen, Search } from "lucide-react";
import { useEffect, useState } from "react";
import { ApiError } from "../../../shared/api/client";
import { Button } from "../../../shared/components/Button";
import { Dialog } from "../../../shared/components/Dialog";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { useClassesQuery } from "../../classes/queries";
import { useEnrollStudent } from "../queries";

interface AddToClassDialogProps {
  open: boolean;
  studentId: string;
  studentName: string;
  onClose: () => void;
}

export function AddToClassDialog({
  open,
  studentId,
  studentName,
  onClose,
}: AddToClassDialogProps) {
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebouncedValue(search);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const classes = useClassesQuery({
    search: debouncedSearch,
    status: "active",
    page: 1,
    pageSize: 50,
    excludeStudentId: studentId,
  });
  const enroll = useEnrollStudent(selectedId ?? "");

  useEffect(() => {
    if (open) {
      setSearch("");
      setSelectedId(null);
      setError(null);
    }
  }, [open]);

  async function submit() {
    if (!selectedId) {
      return;
    }

    const selected = classes.data?.items.find((item) => item.id === selectedId);
    if (
      selected &&
      selected.capacity !== null &&
      selected.activeEnrollmentCount >= selected.capacity &&
      !window.confirm(
        `${selected.name} is at capacity (${selected.activeEnrollmentCount} / ${selected.capacity}). Add ${studentName} anyway?`,
      )
    ) {
      return;
    }

    setError(null);
    try {
      await enroll.mutateAsync(studentId);
      onClose();
    } catch (submitError) {
      setError(
        submitError instanceof ApiError
          ? submitError.message
          : "The student could not be enrolled.",
      );
    }
  }

  return (
    <Dialog
      open={open}
      title={`Add ${studentName} to a class`}
      description="Only active classes the student is not already enrolled in are shown."
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button onClick={() => void submit()} disabled={!selectedId || enroll.isPending}>
            {enroll.isPending ? "Adding…" : "Add to class"}
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
            placeholder="Search by class, subject, or code"
            aria-label="Search classes"
            className="h-10 w-full rounded-lg border border-line bg-surface pl-9 pr-3 text-sm outline-none transition focus:border-brand focus:ring-2 focus:ring-brand/20"
          />
        </div>

        <div className="max-h-72 overflow-y-auto rounded-lg border border-line">
          {classes.isPending ? (
            <p className="px-4 py-8 text-center text-sm text-muted">Loading classes…</p>
          ) : classes.isError ? (
            <p className="px-4 py-8 text-center text-sm text-red-700">
              Classes could not be loaded.
            </p>
          ) : classes.data?.items.length === 0 ? (
            <p className="px-4 py-8 text-center text-sm text-muted">
              No eligible classes match this search.
            </p>
          ) : (
            <ul className="divide-y divide-line">
              {classes.data?.items.map((classEntity) => {
                const full =
                  classEntity.capacity !== null &&
                  classEntity.activeEnrollmentCount >= classEntity.capacity;

                return (
                  <li key={classEntity.id}>
                    <label className="flex cursor-pointer items-center gap-3 px-4 py-3 hover:bg-slate-50">
                      <input
                        type="radio"
                        name="class"
                        checked={selectedId === classEntity.id}
                        onChange={() => setSelectedId(classEntity.id)}
                        className="h-4 w-4 border-line text-brand focus:ring-brand"
                      />
                      <span className="min-w-0 flex-1">
                        <span className="block text-sm font-medium text-ink">
                          {classEntity.name}
                        </span>
                        <span className="block text-xs text-muted">
                          {classEntity.subject}
                          {classEntity.scheduleText ? ` · ${classEntity.scheduleText}` : ""}
                        </span>
                      </span>
                      <span className="text-xs text-slate-500">
                        {classEntity.activeEnrollmentCount}
                        {classEntity.capacity !== null ? ` / ${classEntity.capacity}` : ""}
                        {full ? " · Full" : ""}
                      </span>
                    </label>
                  </li>
                );
              })}
            </ul>
          )}
        </div>

        <p className="text-xs text-muted">
          <BookOpen className="mr-1 inline h-3.5 w-3.5 align-text-bottom" aria-hidden="true" />
          Enrolment starts today; back-dating can be added later.
        </p>
      </div>
    </Dialog>
  );
}
