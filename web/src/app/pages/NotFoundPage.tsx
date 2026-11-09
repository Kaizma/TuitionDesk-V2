import { Link } from "react-router-dom";

export function NotFoundPage() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center text-center">
      <p className="text-sm font-semibold uppercase tracking-wider text-brand">404</p>
      <h1 className="mt-2 text-2xl font-semibold text-ink">That page does not exist</h1>
      <p className="mt-2 text-sm text-muted">The link may be old, or the address may be mistyped.</p>
      <Link
        to="/students"
        className="mt-6 rounded-lg bg-brand px-4 py-2 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-700"
      >
        Back to students
      </Link>
    </div>
  );
}
