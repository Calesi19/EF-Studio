import { useTheme } from "@/hooks/useTheme";
import { useStudioContext } from "@/pages/StudioPage/context/StudioContext";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Separator } from "@/components/ui/separator";
import { cn } from "@/lib/utils";

function SunIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="12" cy="12" r="4" />
      <line x1="12" y1="2" x2="12" y2="4" />
      <line x1="12" y1="20" x2="12" y2="22" />
      <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" />
      <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" />
      <line x1="2" y1="12" x2="4" y2="12" />
      <line x1="20" y1="12" x2="22" y2="12" />
      <line x1="4.22" y1="19.78" x2="5.64" y2="18.36" />
      <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" />
    </svg>
  );
}

function MoonIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" />
    </svg>
  );
}

interface SettingsModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function SettingsModal({ open, onOpenChange }: SettingsModalProps) {
  const { theme, toggle } = useTheme();
  const { nameDisplay, setNameDisplay } = useStudioContext();

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-sm">
        <DialogHeader>
          <DialogTitle>Settings</DialogTitle>
        </DialogHeader>

        <div className="space-y-5 py-1">
          <div className="space-y-2">
            <p className="text-xs font-medium text-foreground">Appearance</p>
            <div className="flex gap-2">
              <button
                onClick={() => theme === "dark" && toggle()}
                className={cn(
                  "flex flex-1 items-center justify-center gap-1.5 rounded-md border px-3 py-2 text-xs transition-colors",
                  theme === "light"
                    ? "border-primary bg-primary/10 text-primary font-medium"
                    : "border-border text-muted-foreground hover:bg-muted/50 hover:text-foreground"
                )}
              >
                <SunIcon />
                Light
              </button>
              <button
                onClick={() => theme === "light" && toggle()}
                className={cn(
                  "flex flex-1 items-center justify-center gap-1.5 rounded-md border px-3 py-2 text-xs transition-colors",
                  theme === "dark"
                    ? "border-primary bg-primary/10 text-primary font-medium"
                    : "border-border text-muted-foreground hover:bg-muted/50 hover:text-foreground"
                )}
              >
                <MoonIcon />
                Dark
              </button>
            </div>
          </div>

          <Separator />

          <div className="space-y-2">
            <p className="text-xs font-medium text-foreground">Table Names</p>
            <div className="space-y-1.5">
              <button
                onClick={() => setNameDisplay("model")}
                className={cn(
                  "w-full rounded-md border px-3 py-2.5 text-left transition-colors",
                  nameDisplay === "model"
                    ? "border-primary bg-primary/10"
                    : "border-border hover:bg-muted/50"
                )}
              >
                <p className={cn("text-xs font-medium", nameDisplay === "model" ? "text-primary" : "text-foreground")}>
                  Model Names
                </p>
                <p className="mt-0.5 text-xs text-muted-foreground">Uses the C# entity class name</p>
              </button>
              <button
                onClick={() => setNameDisplay("database")}
                className={cn(
                  "w-full rounded-md border px-3 py-2.5 text-left transition-colors",
                  nameDisplay === "database"
                    ? "border-primary bg-primary/10"
                    : "border-border hover:bg-muted/50"
                )}
              >
                <p className={cn("text-xs font-medium", nameDisplay === "database" ? "text-primary" : "text-foreground")}>
                  Database Names
                </p>
                <p className="mt-0.5 text-xs text-muted-foreground">Uses the actual SQL table name</p>
              </button>
            </div>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" size="sm" onClick={() => onOpenChange(false)}>
            Close
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
