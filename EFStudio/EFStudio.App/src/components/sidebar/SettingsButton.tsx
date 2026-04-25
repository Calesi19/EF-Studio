import { useState } from "react";
import { Settings01Icon } from "@hugeicons/core-free-icons";
import { HugeiconsIcon } from "@hugeicons/react";
import { SettingsModal } from "@/components/settings/SettingsModal";

export function SettingsButton() {
  const [open, setOpen] = useState(false);

  return (
    <>
      <button
        onClick={() => setOpen(true)}
        className="flex w-full items-center gap-2 rounded px-2 py-1.5 text-xs text-muted-foreground hover:bg-sidebar-accent/50 hover:text-sidebar-foreground transition-colors"
      >
        <HugeiconsIcon icon={Settings01Icon} size={14} />
        Settings
      </button>
      <SettingsModal open={open} onOpenChange={setOpen} />
    </>
  );
}
