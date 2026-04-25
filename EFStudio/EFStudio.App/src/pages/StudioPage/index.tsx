import { TooltipProvider } from "@/components/ui/tooltip";
import { StudioContent } from "@/pages/StudioPage/components/StudioContent";
import { StudioContextProvider } from "@/pages/StudioPage/context/StudioContext";

export default function StudioPage() {
  return (
    <TooltipProvider>
      <StudioContextProvider>
        <StudioContent />
      </StudioContextProvider>
    </TooltipProvider>
  );
}
