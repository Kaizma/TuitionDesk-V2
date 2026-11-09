import { ArrowLeft } from "lucide-react";
import { Link, useParams } from "react-router-dom";
import { Button } from "../../../shared/components/Button";
import { PageHeader } from "../../../shared/components/PageHeader";
import { ClassForm } from "../components/ClassForm";
import { useClassQuery } from "../queries";

export function ClassFormPage() {
  const { id } = useParams<{ id: string }>();
  const isEdit = Boolean(id);
  const classEntity = useClassQuery(id);

  if (isEdit && classEntity.isPending) {
    return (
      <div className="space-y-6">
        <div className="h-4 w-24 animate-pulse rounded bg-slate-100" />
        <div className="h-10 w-64 animate-pulse rounded bg-slate-100" />
        <div className="h-72 animate-pulse rounded-xl border border-line bg-surface" />
      </div>
    );
  }

  if (isEdit && classEntity.isError) {
    return (
      <div className="space-y-6">
        <Link to="/classes" className="inline-flex items-center gap-1 text-sm text-muted hover:text-ink">
          <ArrowLeft className="h-4 w-4" aria-hidden="true" />
          Classes
        </Link>
        <div className="rounded-xl border border-line bg-surface p-8 text-center shadow-sm">
          <p className="text-sm font-medium text-red-800">This class could not be loaded.</p>
          <Button className="mt-4" variant="secondary" onClick={() => void classEntity.refetch()}>
            Try again
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <Link
        to={isEdit ? `/classes/${id}` : "/classes"}
        className="inline-flex items-center gap-1 text-sm text-muted hover:text-ink"
      >
        <ArrowLeft className="h-4 w-4" aria-hidden="true" />
        {isEdit ? "Back to class" : "Classes"}
      </Link>

      <PageHeader
        title={isEdit ? "Edit class" : "New class"}
        description={
          isEdit
            ? "Update the class definition. Student enrolments are managed from the class page."
            : "A name, subject, start date, and fee are required. Capacity is optional."
        }
      />

      <ClassForm classEntity={classEntity.data} />
    </div>
  );
}
