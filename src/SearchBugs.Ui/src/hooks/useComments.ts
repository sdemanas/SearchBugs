import { useState } from "react";
import { useToast } from "@/components/ui/use-toast";
import { apiClient, Comment } from "@/lib/api";
import { useApi } from "@/hooks/useApi";

interface UseCommentsReturn {
  comments: Comment[];
  isLoading: boolean;
  isAddingComment: boolean;
  addComment: (content: string) => Promise<void>;
  refreshComments: () => void;
}

export const useComments = (bugId: string): UseCommentsReturn => {
  const { toast } = useToast();
  const [isAddingComment, setIsAddingComment] = useState(false);

  // Fetch comments using existing useApi hook
  const {
    data: commentsResponse,
    mutate: refreshCommentsMutate,
    isLoading,
  } = useApi<Comment[]>(`bugs/${bugId}/comments`);

  const comments = commentsResponse?.value || [];

  const refreshComments = () => {
    if (refreshCommentsMutate.mutate) {
      refreshCommentsMutate.mutate({});
    }
  };

  const addComment = async (content: string): Promise<void> => {
    if (!content.trim()) {
      toast({
        title: "Invalid comment",
        description: "Comment cannot be empty.",
        variant: "destructive",
      });
      return;
    }

    if (content.length > 2000) {
      toast({
        title: "Comment too long",
        description: "Comment must be less than 2000 characters.",
        variant: "destructive",
      });
      return;
    }

    setIsAddingComment(true);
    try {
      console.log("Adding comment with data:", { bugId, content });

      const response = await apiClient.comments.create({
        bugId: bugId,
        content: content,
      });

      console.log("API response:", response);

      if (response.data.isSuccess) {
        // Refresh comments list
        refreshComments();

        toast({
          title: "Comment added",
          description: "Your comment has been added successfully.",
        });
      } else {
        // Handle validation errors specifically
        if (response.data.error?.code === "ValidationError") {
          const errorMessage =
            response.data.error?.message || "Validation failed";
          toast({
            title: "Validation Error",
            description: errorMessage,
            variant: "destructive",
          });
        } else {
          toast({
            title: "Error",
            description:
              response.data.error?.message || "Failed to add comment",
            variant: "destructive",
          });
        }
      }
    } catch (error) {
      console.error("Error adding comment:", error);
      toast({
        title: "Error",
        description: "Failed to add comment. Please try again.",
        variant: "destructive",
      });
    } finally {
      setIsAddingComment(false);
    }
  };

  return {
    comments,
    isLoading,
    isAddingComment,
    addComment,
    refreshComments,
  };
};
