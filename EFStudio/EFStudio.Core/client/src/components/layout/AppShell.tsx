import type { ReactNode } from "react";

interface AppShellProps {
  sidebar: ReactNode;
  children: ReactNode;
  sidebarOpen: boolean;
}

export function AppShell({ sidebar, children, sidebarOpen }: AppShellProps) {
  return (
    <div className="flex h-full w-full overflow-hidden bg-background">
      <aside
        className="flex h-full min-h-0 shrink-0 flex-col overflow-hidden border-r border-sidebar-border bg-sidebar shadow-[1px_0_3px_0_hsl(var(--sidebar-border)/0.4)] transition-all duration-200 ease-in-out"
        style={{ width: sidebarOpen ? 208 : 0 }}
      >
        {sidebar}
      </aside>
      <main className="flex min-h-0 flex-1 flex-col overflow-hidden">
        {children}
      </main>
    </div>
  );
}
