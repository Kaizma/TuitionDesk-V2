import { BookOpen } from "lucide-react";
import { EmptyState } from "../../shared/components/EmptyState";
import { PageHeader } from "../../shared/components/PageHeader";

export function ClassesPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Classes"
        description="Every recurring group, its schedule, fee, and roster."
      />
      <EmptyState
        icon={BookOpen}
        title="The class registry is coming next"
        description="Saturday Maths, Wednesday Physics, and everything else will live here with a live enrolled count."
      />
    </div>
  );
}
