import { BookOpen, Users } from "lucide-react";
import { EmptyState } from "../../shared/components/EmptyState";
import { PageHeader } from "../../shared/components/PageHeader";

export function StudentsPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Students"
        description="The single place for names, contact details, and who is taking what."
      />
      <EmptyState
        icon={Users}
        title="The student registry arrives in the next slice"
        description="Milestone M0 is the walking skeleton. M1 adds create, search, edit, archive, and restore for students."
      />
      <p className="text-center text-xs text-slate-400">
        <BookOpen className="mr-1 inline h-3.5 w-3.5 align-text-bottom" aria-hidden="true" />
        Next after students: the class registry and allocation.
      </p>
    </div>
  );
}
