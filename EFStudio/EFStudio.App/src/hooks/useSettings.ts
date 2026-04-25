import { useEffect, useState } from "react";

const STORAGE_KEY_NAME_DISPLAY = "ef-studio-name-display";

export type NameDisplay = "model" | "database";

function getInitialNameDisplay(): NameDisplay {
  const stored = localStorage.getItem(STORAGE_KEY_NAME_DISPLAY);
  if (stored === "model" || stored === "database") return stored;
  return "model";
}

export function useSettings() {
  const [nameDisplay, setNameDisplay] = useState<NameDisplay>(getInitialNameDisplay);

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY_NAME_DISPLAY, nameDisplay);
  }, [nameDisplay]);

  return { nameDisplay, setNameDisplay };
}
