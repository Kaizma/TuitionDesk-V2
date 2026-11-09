import { ArrowLeft, Pencil, Plus } from "lucide-react";
import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import { ApiError } from "../../../shared/api/client";
import { Button, buttonStyles } from "../../../shared/components/Button";
import { PageHeader } from "../../../shared/components/PageHeader";
import { StatusBadge } from "../../../shared/components/StatusBadge";
import { AddStudentsDialog } from "../../enrollments/components/AddStudentsDialog";
import { ClassRoster } from "../../enrollments/components/ClassRoster";
import { formatDate, formatDateTime } from "../../../shared/lib/date";
import { formatMoney } from "../../../shared/lib/money";
import { useArchiveClass, useClassQuery, useRestoreClass } from "../queries";
import type { ClassDetail } from "../types";

export function ClassDetailPage() {
  const { id } = useParams<{ id: string }>();
  const classEntity = useClassQuery(id);
  const archive = useArchiveClass();
  const restore = useRestoreClass();
  const [actionError, setActionError] = useState<string | null>(null);
  const [addStudentsOpen, setAddStudentsOpen] = useState(false);

  async function toggleStatus(data: ClassDetail) {
    const archiving = data.status === "active";
    const message = archiving
      ? `Archive ${data.name}? Active enrolments will be withdrawn.`
      : `Restore ${data.name}? Enrolments are not restored automatically.`;

    if (!window.confirm(message)) {
      return;
    }

    setActionError(null);
    try {
      if (archiving) {
        await archive.mutateAsync(data.id);
      } else {
        await restore.mutateAsync(data.id);
      }
    } catch (error) {
      setActionError(error instanceof Error ? error.message : "The action could not be completed.");
    }
  }

  if (classEntity.isPending) {
    return <DetailSkeleton />;
  }

  if (classEntity.isError) {
    const notFound = classEntity.error instanceof ApiError && classEntity.error.status === 404;

    return (
      <div className="space-y-6">
        <Link to="/classes" className="inline-flex items-center gap-1 text-sm text-muted hover:text-ink">
          <ArrowLeft className="h-4 w-4" aria-hidden="true" />
          Classes
        </Link>
        <div className="rounded-xl border border-line bg-surface p-8 text-center shadow-sm">
          <h1 className="text-lg font-semibold text-ink">
            {notFound ? "Class not found" : "Class could not be loaded"}
          </h1>
          <p className="mt-2 text-sm text-muted">
            {notFound
              ? "This record may have been removed or the link is incorrect."
              : "Check that the local service is running, then try again."}
          </p>
          <div className="mt-5 flex justify-center gap-2">
            {!notFound ? (
              <Button variant="secondary" onClick={() => void classEntity.refetch()}>
                Try again
              </Button>
            ) : null}
            <Link to="/classes" className={buttonStyles("secondary")}>
              Back to classes
            </Link>
          </div>
        </div>
      </div>
    );
  }

  const data = classEntity.data;
  const capacity = data.capacity;
  const seatsLeft = capacity === null ? null : Math.max(0, capacity - data.activeEnrollmentCount);

  return (
    <div className="space-y-6">
      <Link to="/classes" className="inline-flex items-center gap-1 text-sm text-muted hover:text-ink">
        <ArrowLeft className="h-4 w-4" aria-hidden="true" />
        Classes
      </Link>

      <PageHeader
        title={data.name}
        description={`${data.code} · ${data.subject}${data.gradeLevel ? ` · ${data.gradeLevel}` : ""}`}
        actions={
          <>
            {data.status === "active" ? (
              <Button onClick={() => setAddStudentsOpen(true)}>
                <Plus className="h-4 w-4" aria-hidden="true" />
                Add students
              </Button>
            ) : null}
            <Link to={`/classes/${data.id}/edit`} className={buttonStyles("secondary")}>
              <Pencil className="h-4 w-4" aria-hidden="true" />
              Edit
            </Link>
            <Button
              variant={data.status === "active" ? "danger" : "secondary"}
              onClick={() => void toggleStatus(data)}
              disabled={archive.isPending || restore.isPending}
            >
              {data.status === "active" ? "Archive" : "Restore"}
            </Button>
          </>
        }
      />

      <div className="flex flex-wrap items-center gap-3">
        <StatusBadge status={data.status} />
        <span className="text-xs text-muted">Last updated {formatDateTime(data.updatedAt)}</span>
      </div>

      {actionError ? (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
          {actionError}
        </div>
      ) : null}

      <div className="grid gap-6 lg:grid-cols-3">
        <section className="rounded-xl border border-line bg-surface p-6 shadow-sm lg:col-span-2">
          <h2 className="text-sm font-semibold uppercase tracking-wider text-muted">Class details</h2>
          <dl className="mt-4 grid gap-x-8 gap-y-4 sm:grid-cols-2">
            <DetailItem label="Subject" value={data.subject} />
            <DetailItem label="Grade level" value={data.gradeLevel} />
            <DetailItem label="Schedule" value={data.scheduleText} />
            <DetailItem label="Monthly fee" value={formatMoney(data.monthlyFeeMinor, data.currency)} />
            <DetailItem label="Start date" value={formatDate(data.startDate)} />
            <DetailItem label="End date" value={formatDate(data.endDate)} />
          </dl>

          {data.notes ? (
            <>
              <h2 className="mt-8 text-sm font-semibold uppercase tracking-wider text-muted">Notes</h2>
              <p className="mt-3 whitespace-pre-wrap text-sm leading-relaxed text-slate-600">
                {data.notes}
              </p>
            </>
          ) : null}
        </section>

        <section className="rounded-xl border border-line bg-surface p-6 shadow-sm">
          <h2 className="text-sm font-semibold uppercase tracking-wider text-muted">At a glance</h2>
          <dl className="mt-4 space-y-4">
            <DetailItem label="Class code" value={data.code} mono />
            <DetailItem
              label="Enrolled"
              value={
                capacity === null
                  ? `${data.activeEnrollmentCount} (unlimited)`
                  : `${data.activeEnrollmentCount} / ${capacity}`
              }
            />
            <DetailItem
              label="Seats left"
              value={seatsLeft === null ? "Unlimited" : String(seatsLeft)}
            />
            <DetailItem label="Added" value={formatDateTime(data.createdAt)} />
          </dl>
        </section>
      </div>

      <ClassRoster
        classId={data.id}
        className={data.name}
        archived={data.status === "archived"}
        onAddStudents={() => setAddStudentsOpen(true)}
      />

      <AddStudentsDialog
        open={addStudentsOpen}
        classId={data.id}
        className={data.name}
        capacity={data.capacity}
        enrolledCount={data.activeEnrollmentCount}
        onClose={() => setAddStudentsOpen(false)}
      />
    </div>
  );
}

function DetailItem({ label, value, mono }: { label: string; value?: string | null; mono?: boolean }) {
  return (
    <div>
      <dt className="text-xs font-medium uppercase tracking-wide text-slate-400">{label}</dt>
      <dd className={mono ? "mt-1 font-mono text-sm text-slate-700" : "mt-1 text-sm text-slate-700"}>
        {value || "—"}
      </dd>
    </div>
  );
}

function DetailSkeleton() {
  return (
    <div className="space-y-6">
      <div className="h-4 w-24 animate-pulse rounded bg-slate-100" />
      <div className="h-10 w-72 animate-pulse rounded bg-slate-100" />
      <div className="grid gap-6 lg:grid-cols-3">
        <div className="h-64 animate-pulse rounded-xl border border-line bg-surface lg:col-span-2" />
        <div className="h-64 animate-pulse rounded-xl border border-line bg-surface" />
      </div>
      <div className="h-40 animate-pulse rounded-xl border border-line bg-surface" />
    </div>
  );
}
