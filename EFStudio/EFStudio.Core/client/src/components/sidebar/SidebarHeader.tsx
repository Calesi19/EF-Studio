export function SidebarHeader() {
  return (
    <div className="flex items-center gap-2 px-4 py-4 border-b border-sidebar-border">
      <div className="flex h-7 w-7 items-center justify-center rounded-md bg-primary text-primary-foreground text-xs font-bold">
        EF
      </div>
      <div>
        <p className="text-sm font-semibold text-sidebar-foreground leading-tight">EF Studio</p>
        <p className="text-xs text-muted-foreground leading-tight">Database Explorer</p>
      </div>
    </div>
  );
}
