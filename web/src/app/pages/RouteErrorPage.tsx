import { isRouteErrorResponse, Link, useRouteError } from "react-router-dom";

export function RouteErrorPage() {
  const error = useRouteError();
  const message = isRouteErrorResponse(error)
    ? `${error.status} ${error.statusText}`
    : error instanceof Error
      ? error.message
      : "An unexpected error occurred.";

  return (
    <div className="flex min-h-screen items-center justify-center bg-canvas px-6">
      <div className="max-w-md rounded-xl border border-line bg-surface p-8 text-center shadow-sm">
        <h1 className="text-xl font-semibold text-ink">Something went wrong</h1>
        <p className="mt-3 break-words text-sm text-muted">{message}</p>
        <Link
          to="/students"
          className="mt-6 inline-flex rounded-lg bg-brand px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-700"
        >
          Return to the app
        </Link>
      </div>
    </div>
  );
}
