import React from "react";
import { cn } from "@/lib/utils";
import { Loader2 } from "lucide-react";

// Basic loading spinner
export const LoadingSpinner: React.FC<{
  size?: "sm" | "md" | "lg";
  className?: string;
}> = ({ size = "md", className }) => {
  const sizeClasses = {
    sm: "h-4 w-4",
    md: "h-6 w-6",
    lg: "h-8 w-8",
  };

  return (
    <Loader2 className={cn("animate-spin", sizeClasses[size], className)} />
  );
};

interface LoadingSkeletonProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: "text" | "card" | "avatar" | "button";
}

export const LoadingSkeleton = ({
  variant = "text",
  className,
  ...props
}: LoadingSkeletonProps) => {
  const variantClasses = {
    text: "h-4 w-full",
    card: "h-32 w-full",
    avatar: "h-10 w-10 rounded-full",
    button: "h-9 w-20",
  };

  return (
    <div
      className={cn(
        "animate-pulse bg-gray-200 rounded",
        variantClasses[variant],
        className
      )}
      {...props}
    />
  );
};

interface LoadingStateProps {
  children?: React.ReactNode;
  text?: string;
  size?: "sm" | "md" | "lg";
  className?: string;
}

export const LoadingState = ({
  children,
  text = "Loading...",
  size = "md",
  className,
}: LoadingStateProps) => {
  return (
    <div
      className={cn(
        "flex flex-col items-center justify-center py-8",
        className
      )}
    >
      <LoadingSpinner size={size} className="mb-4" />
      {children || <p className="text-sm text-muted-foreground">{text}</p>}
    </div>
  );
};

export const PageLoadingState = ({
  text = "Loading page...",
}: {
  text?: string;
}) => {
  return (
    <div className="flex items-center justify-center min-h-[400px]">
      <div className="text-center">
        <LoadingSpinner size="lg" className="mb-4 mx-auto" />
        <h3 className="text-lg font-medium mb-2">Please wait</h3>
        <p className="text-muted-foreground">{text}</p>
      </div>
    </div>
  );
};

export const CardLoadingSkeleton = () => {
  return (
    <div className="border rounded-lg p-4">
      <div className="space-y-3">
        <LoadingSkeleton variant="text" className="h-6 w-3/4" />
        <LoadingSkeleton variant="text" className="h-4 w-full" />
        <LoadingSkeleton variant="text" className="h-4 w-2/3" />
      </div>
    </div>
  );
};

export const ListLoadingSkeleton = ({ items = 3 }: { items?: number }) => {
  return (
    <div className="space-y-4">
      {Array.from({ length: items }).map((_, i) => (
        <div key={i} className="flex items-center space-x-3">
          <LoadingSkeleton variant="avatar" />
          <div className="flex-1 space-y-2">
            <LoadingSkeleton variant="text" className="h-4 w-1/2" />
            <LoadingSkeleton variant="text" className="h-3 w-3/4" />
          </div>
        </div>
      ))}
    </div>
  );
};
