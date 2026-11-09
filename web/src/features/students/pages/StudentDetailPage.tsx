import { ArrowLeft, Pencil, Plus } from "lucide-react";
import { useState } from "react";
import { Link, useParams } from "react-router-dom";
import { ApiError } from "../../../shared/api/client";
import { Button, buttonStyles } from "../../../shared/components/Button";
import { PageHeader } from "../../../shared/components/PageHeader";
import { StatusBadge } from "../../../shared/components/StatusBadge";
import { AddToClassDialog } from "../../enrollments/components/AddToClassDialog";
import { formatDate, formatDateTime } from "../../../shared/lib/date";
import { useArchiveStudent, useRestoreStudent, useStudentQuery } from "../queries";
import type { StudentDetail, StudentEnrollment } from "../types";

export function StudentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const student = useStudentQuery(id);
  const archive = useArchiveStudent();
  const restore = useRestoreStudent();
  const [actionError, setActionError] = useState<string | null>(null);
  const [addToClassOpen, setAddToClassOpen] = useState(false);

  async function toggleStatus(data: StudentDetail) {
    const archiving = data.status === "active";
    const message = archiving
      ? `Archive ${data.fullName}? Their active class enrolments will be withdrawn.`
      : `Restore ${data.fullName}? They will become active again but are not re-enrolled automatically.`;

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

  if (student.isPending) {
    return <DetailSkeleton />;
  }

  if (student.isError) {
    const notFound = student.error instanceof ApiError && student.error.status === 404;

    return (
      <div className="space-y-6">
        <Link to="/students" className="inline-flex items-center gap-1 text-sm text-muted hover:text-ink">
          <ArrowLeft className="h-4 w-4" aria-hidden="true" />
          Students
        </Link>
        <div className="rounded-xl border border-line bg-surface p-8 text-center shadow-sm">
          <h1 className="text-lg font-semibold text-ink">
            {notFound ? "Student not found" : "Student could not be loaded"}
          </h1>
          <p className="mt-2 text-sm text-muted">
            {notFound
              ? "This record may have been removed or the link is incorrect."
              : "Check that the local service is running, then try again."}
          </p>
          <div className="mt-5 flex justify-center gap-2">
            {!notFound ? (
              <Button variant="secondary" onClick={() => void student.refetch()}>
                Try again
              </Button>
            ) : null}
            <Link to="/students" className={buttonStyles("secondary")}>
              Back to students
            </Link>
          </div>
        </div>
      </div>
    );
  }

  const data = student.data;

  return (
    <div className="space-y-6">
      <Link to="/students" className="inline-flex items-center gap-1 text-sm text-muted hover:text-ink">
        <ArrowLeft className="h-4 w-4" aria-hidden="true" />
        Students
      </Link>

      <PageHeader
        title={data.fullName}
        description={`${data.code} · Joined ${formatDate(data.joinedOn)}`}
        actions={
          <>
            {data.status === "active" ? (
              <Button onClick={() => setAddToClassOpen(true)}>
                <Plus className="h-4 w-4" aria-hidden="true" />
                Add to class
              </Button>
            ) : null}
            <Link to={`/students/${data.id}/edit`} className={buttonStyles("secondary")}>
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

      <div className="flex items-center gap-3">
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
          <h2 className="text-sm font-semibold uppercase tracking-wider text-muted">Contact</h2>
          <dl className="mt-4 grid gap-x-8 gap-y-4 sm:grid-cols-2">
            <DetailItem label="Phone" value={data.phone} />
            <DetailItem label="Email" value={data.email} />
            <DetailItem label="Guardian" value={data.guardianName} />
            <DetailItem label="Guardian phone" value={data.guardianPhone} />
          </dl>

          <h2 className="mt-8 text-sm font-semibold uppercase tracking-wider text-muted">
            School
          </h2>
          <dl className="mt-4 grid gap-x-8 gap-y-4 sm:grid-cols-2">
            <DetailItem label="School" value={data.school} />
            <DetailItem label="Grade level" value={data.gradeLevel} />
            <DetailItem label="Date of birth" value={formatDate(data.dateOfBirth)} />
            <DetailItem label="Joined on" value={formatDate(data.joinedOn)} />
          </dl>

          {data.notes ? (
            <>
              <h2 className="mt-8 text-sm font-semibold uppercase tracking-wider text-muted">
                Notes
              </h2>
              <p className="mt-3 whitespace-pre-wrap text-sm leading-relaxed text-slate-600">
                {data.notes}
              </p>
            </>
          ) : null}
        </section>

        <section className="rounded-xl border border-line bg-surface p-6 shadow-sm">
          <h2 className="text-sm font-semibold uppercase tracking-wider text-muted">At a glance</h2>
          <dl className="mt-4 space-y-4">
            <DetailItem label="Student code" value={data.code} mono />
            <DetailItem
              label="Active classes"
              value={String(data.enrollments.filter((item) => item.status === "active").length)}
            />
            <DetailItem label="Enrolment history" value={String(data.enrollments.length)} />
            <DetailItem label="Added" value={formatDateTime(data.createdAt)} />
          </dl>
        </section>
      </div>

      <section className="rounded-xl border border-line bg-surface shadow-sm">
        <div className="flex items-center justify-between border-b border-line px-6 py-4">
          <div>
            <h2 className="text-sm font-semibold text-ink">Classes</h2>
            <p className="mt-0.5 text-xs text-muted">
              Every class this student has attended, newest first.
            </p>
          </div>
          {data.status === "active" ? (
            <Button size="sm" variant="secondary" onClick={() => setAddToClassOpen(true)}>
              <Plus className="h-4 w-4" aria-hidden="true" />
              Add to class
            </Button>
          ) : null}
        </div>

        {data.enrollments.length === 0 ? (
          <div className="px-6 py-10 text-center">
            <p className="text-sm font-medium text-slate-700">Not enrolled in any classes yet</p>
            <p className="mt-1 text-sm text-muted">
              Once classes are added, this student's roster history will appear here.
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[640px] text-left text-sm">
              <thead className="border-b border-line bg-slate-50/60 text-[11px] uppercase tracking-wider text-muted">
                <tr>
                  <th scope="col" className="px-6 py-3 font-semibold">Class</th>
                  <th scope="col" className="px-4 py-3 font-semibold">Subject</th>
                  <th scope="col" className="px-4 py-3 font-semibold">Enrolled</th>
                  <th scope="col" className="px-4 py-3 font-semibold">Status</th>
                </tr>
              </thead>
              <tbody>
                {data.enrollments.map((enrollment) => (
                  <EnrollmentRow key={enrollment.id} enrollment={enrollment} />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <AddToClassDialog
        open={addToClassOpen}
        studentId={data.id}
        studentName={data.fullName}
        onClose={() => setAddToClassOpen(false)}
      />
    </div>
  );
}

function EnrollmentRow({ enrollment }: { enrollment: StudentEnrollment }) {
  return (
    <tr className="border-b border-line last:border-b-0">
      <td className="px-6 py-3">
        <p className="font-medium text-ink">{enrollment.className}</p>
        {enrollment.scheduleText ? (
          <p className="mt-0.5 text-xs text-slate-400">{enrollment.scheduleText}</p>
        ) : null}
      </td>
      <td className="px-4 py-3 text-slate-600">{enrollment.subject}</td>
      <td className="px-4 py-3 text-slate-600">
        {formatDate(enrollment.enrolledOn)}
        {enrollment.withdrawnOn ? (
          <span className="block text-xs text-slate-400">
            Withdrawn {formatDate(enrollment.withdrawnOn)}
          </span>
        ) : null}
      </td>
      <td className="px-4 py-3">
        <StatusBadge status={enrollment.status} />
      </td>
    </tr>
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
      <div className="h-48 animate-pulse rounded-xl border border-line bg-surface" />
    </div>
  );
}
