import { BookOpen, GraduationCap, Info, Users } from "lucide-react";
import { NavLink, Outlet } from "react-router-dom";
import { cn } from "../shared/lib/cn";

const navItems = [
  { to: "/students", label: "Students", icon: Users },
  { to: "/classes", label: "Classes", icon: BookOpen },
  { to: "/about", label: "About", icon: Info },
];

const appVersion = window.__TUITIONDESK__?.appVersion ?? "0.1.0";

export function AppShell() {
  return (
    <div className="flex h-full min-h-screen bg-canvas">
      <aside className="flex w-60 shrink-0 flex-col border-r border-line bg-surface">
        <div className="flex items-center gap-3 px-5 py-5">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-brand text-white">
            <GraduationCap className="h-5 w-5" aria-hidden="true" />
          </div>
          <div>
            <p className="text-sm font-semibold tracking-tight text-ink">TuitionDesk</p>
            <p className="text-[11px] font-medium uppercase tracking-wider text-muted">MVP</p>
          </div>
        </div>

        <nav className="flex flex-1 flex-col gap-1 px-3 py-2" aria-label="Main">
          {navItems.map(({ to, label, icon: Icon }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                cn(
                  "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-brand-soft text-brand"
                    : "text-slate-600 hover:bg-slate-50 hover:text-ink",
                )
              }
            >
              <Icon className="h-4 w-4" aria-hidden="true" />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="border-t border-line px-5 py-4">
          <p className="text-xs font-medium text-muted">Version {appVersion}</p>
          <p className="mt-1 text-[11px] leading-relaxed text-slate-400">
            Local-first. Your data stays on this machine.
          </p>
        </div>
      </aside>

      <main className="min-w-0 flex-1 overflow-y-auto">
        <div className="mx-auto w-full max-w-6xl px-8 py-8">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
