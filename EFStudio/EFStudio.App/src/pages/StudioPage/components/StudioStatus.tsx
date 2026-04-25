interface StudioStatusProps {
  title: string;
  message: string;
}

export function StudioStatus({ title, message }: StudioStatusProps) {
  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-2 px-6 text-center">
      <p className="text-sm font-medium text-foreground">{title}</p>
      <p className="max-w-md text-sm text-muted-foreground">{message}</p>
    </div>
  );
}
