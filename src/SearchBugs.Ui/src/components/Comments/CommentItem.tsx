import React from "react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { safeFormatDistance } from "@/lib/date-utils";
import { Comment } from "@/lib/api";

interface CommentItemProps {
  comment: Comment;
}

export const CommentItem: React.FC<CommentItemProps> = ({ comment }) => {
  // Generate initials from userName or use default
  const getInitials = (name?: string) => {
    if (!name) return "U";
    return name
      .split(" ")
      .map((n) => n[0])
      .join("")
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <div className="flex gap-4 p-4 rounded-lg bg-muted/30 hover:bg-muted/50 transition-colors">
      <Avatar className="h-8 w-8">
        <AvatarFallback className="bg-primary/10 text-primary text-xs">
          {getInitials(comment.userName)}
        </AvatarFallback>
      </Avatar>
      <div className="flex-1 space-y-1">
        <div className="flex items-center gap-2">
          <span className="font-medium text-sm">
            {comment.userName || "Unknown User"}
          </span>
          <span className="text-xs text-muted-foreground">
            {safeFormatDistance(comment.createdOnUtc)}
          </span>
          {comment.modifiedOnUtc &&
            comment.modifiedOnUtc !== comment.createdOnUtc && (
              <span className="text-xs text-muted-foreground italic">
                (edited)
              </span>
            )}
        </div>
        <div className="text-sm text-foreground whitespace-pre-wrap">
          {comment.commentText}
        </div>
      </div>
    </div>
  );
};
