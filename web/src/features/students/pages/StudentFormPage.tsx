import { ArrowLeft } from "lucide-react";
import { Link, useParams } from "react-router-dom";
import { Button } from "../../../shared/components/Button";
import { PageHeader } from "../../../shared/components/PageHeader";
import { StudentForm } from "../components/StudentForm";
import { useStudentQuery } from "../queries";

export function StudentFormPage() {
  const { id } = useParams<{ id: string }>();
  const isEdit = Boolean(id);
  const student = useStudentQuery(id);

  if (isEdit && student.isPending) {
    return (
      <div className="space-y-6">
        <div className="h-4 w-24 animate-pulse rounded bg-slate-100" />
        <div className="h-10 w-64 animate-pulse rounded bg-slate-100" />
        <div className="h-64 animate-pulse rounded-xl border border-line bg-surface" />
      </div>
    );
  }

  if (isEdit && student.isError) {
    return (
      <div className="space-y-6">
        <Link to="/students" className="inline-flex items-center gap-1 text-sm text-muted hover:text-ink">
          <ArrowLeft className="h-4 w-4" aria-hidden="true" />
          Students
        </Link>
        <div className="rounded-xl border border-line bg-surface p-8 text-center shadow-sm">
          <p className="text-sm font-medium text-red-800">
            This student could not be loaded.
          </p>
          <Button className="mt-4" variant="secondary" onClick={() => void student.refetch()}>
            Try again
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <Link
        to={isEdit ? `/students/${id}` : "/students"}
        className="inline-flex items-center gap-1 text-sm text-muted hover:text-ink"
      >
        <ArrowLeft className="h-4 w-4" aria-hidden="true" />
        {isEdit ? "Back to student" : "Students"}
      </Link>

      <PageHeader
        title={isEdit ? "Edit student" : "New student"}
        description={
          isEdit
            ? "Update contact details, guardian information, or grade."
            : "Only a name and phone number are required. Everything else can be added later."
        }
      />

      <StudentForm student={student.data} />
    </div>
  );
}
