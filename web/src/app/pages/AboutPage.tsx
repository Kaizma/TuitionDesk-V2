import { useQuery } from "@tanstack/react-query";
import { AlertCircle, CheckCircle2, Database, RefreshCw } from "lucide-react";
import { getHealth } from "../../features/system/api";
import { queryKeys } from "../../shared/lib/queryKeys";
import { PageHeader } from "../../shared/components/PageHeader";
import { cn } from "../../shared/lib/cn";

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between gap-6 border-b border-line py-3 last:border-b-0">
      <dt className="text-sm text-muted">{label}</dt>
      <dd className="max-w-[60%] truncate text-right text-sm font-medium text-ink" title={value}>
        {value}
      </dd>
    </div>
  );
}

export function AboutPage() {
  const health = useQuery({ queryKey: queryKeys.health, queryFn: getHealth });

  return (
    <div className="space-y-6">
      <PageHeader
        title="About"
        description="Version, local service health, and where your data lives."
        actions={
          <button
            type="button"
            onClick={() => void health.refetch()}
            className="inline-flex items-center gap-2 rounded-lg border border-line bg-surface px-3 py-2 text-sm font-medium text-slate-700 shadow-sm transition hover:bg-slate-50"
          >
            <RefreshCw className={cn("h-4 w-4", health.isFetching && "animate-spin")} aria-hidden="true" />
            Refresh
          </button>
        }
      />

      <div className="grid gap-6 lg:grid-cols-2">
        <section className="rounded-xl border border-line bg-surface p-6 shadow-sm">
          <div className="flex items-center justify-between">
            <h2 className="text-sm font-semibold uppercase tracking-wider text-muted">Local service</h2>
            {health.isSuccess ? (
              <span className="inline-flex items-center gap-1.5 rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-700">
                <CheckCircle2 className="h-3.5 w-3.5" aria-hidden="true" />
                {health.data.status}
              </span>
            ) : health.isError ? (
              <span className="inline-flex items-center gap-1.5 rounded-full bg-red-50 px-2.5 py-1 text-xs font-semibold text-red-700">
                <AlertCircle className="h-3.5 w-3.5" aria-hidden="true" />
                unavailable
              </span>
            ) : (
              <span className="text-xs text-muted">checking…</span>
            )}
          </div>

          {health.isError ? (
            <p className="mt-4 text-sm leading-relaxed text-red-700">
              {health.error instanceof Error ? health.error.message : "The local API did not respond."}
            </p>
          ) : (
            <dl className="mt-4">
              <Row label="API version" value={health.data?.version ?? "—"} />
              <Row label="Schema" value={health.data?.schemaVersion ?? "—"} />
              <Row label="Database" value={health.data?.database ?? "—"} />
              <Row
                label="Pending migrations"
                value={health.data ? String(health.data.pendingMigrations.length) : "—"}
              />
            </dl>
          )}
        </section>

        <section className="rounded-xl border border-line bg-surface p-6 shadow-sm">
          <div className="flex items-center gap-2">
            <Database className="h-4 w-4 text-brand" aria-hidden="true" />
            <h2 className="text-sm font-semibold uppercase tracking-wider text-muted">Data ownership</h2>
          </div>
          <p className="mt-4 text-sm leading-relaxed text-slate-600">
            TuitionDesk stores everything in a single SQLite database on this computer. Nothing is sent to a
            server, and the app works with the network disconnected.
          </p>
          <p className="mt-3 text-sm leading-relaxed text-slate-600">
            Data folder: <code className="rounded bg-slate-100 px-1.5 py-0.5 text-xs">%LOCALAPPDATA%\TuitionDesk</code>
          </p>
          <p className="mt-3 text-xs leading-relaxed text-slate-400">
            Back up that folder before moving to another machine. Backups are taken automatically on startup and
            before schema upgrades.
          </p>
        </section>
      </div>
    </div>
  );
}
