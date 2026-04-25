export function SidebarHeader() {
  return (
    <div className="flex items-center gap-2 px-3 py-2.5 border-b border-sidebar-border">
      <div className="flex h-6 w-6 items-center justify-center rounded bg-primary text-primary-foreground text-[10px] font-bold shrink-0">
        EF
      </div>
      <div>
        <p className="text-xs font-semibold text-sidebar-foreground leading-tight">EF Studio</p>
        <p className="text-[10px] text-muted-foreground leading-tight">Database Explorer</p>
      </div>
    </div>
  );
}
