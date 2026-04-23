import type { ReactNode } from "react";

interface AppShellProps {
  sidebar: ReactNode;
  children: ReactNode;
}

export function AppShell({ sidebar, children }: AppShellProps) {
  return (
    <div className="flex h-full w-full overflow-hidden bg-background">
      <aside className="flex h-full w-60 shrink-0 flex-col border-r border-sidebar-border bg-sidebar">
        {sidebar}
      </aside>
      <main className="flex flex-1 flex-col overflow-hidden">
        {children}
      </main>
    </div>
  );
}
