import { cn } from "../lib/cn";

const styles: Record<string, string> = {
  active: "bg-emerald-50 text-emerald-700 ring-emerald-600/20",
  archived: "bg-slate-100 text-slate-600 ring-slate-500/20",
  withdrawn: "bg-amber-50 text-amber-700 ring-amber-600/20",
};

export function StatusBadge({ status }: { status: string }) {
  const style = styles[status] ?? "bg-slate-100 text-slate-600 ring-slate-500/20";

  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2 py-0.5 text-[11px] font-semibold capitalize ring-1 ring-inset",
        style,
      )}
    >
      {status}
    </span>
  );
}
