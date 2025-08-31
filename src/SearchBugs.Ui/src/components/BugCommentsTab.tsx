import React from "react";
import { CommentSection } from "@/components/Comments";
import { useComments } from "@/hooks/useComments";

interface BugCommentsTabProps {
  bugId: string;
}

export const BugCommentsTab: React.FC<BugCommentsTabProps> = ({ bugId }) => {
  const { comments, isLoading, isAddingComment, addComment } =
    useComments(bugId);

  return (
    <CommentSection
      comments={comments}
      isLoading={isLoading}
      onAddComment={addComment}
      isAddingComment={isAddingComment}
      title=""
      showCommentCount={false}
      maxHeight={500}
    />
  );
};

// You can replace the comments TabsContent in BugDetailsPage with:
// <TabsContent value="comments" className="space-y-4">
//   <BugCommentsTab bugId={bugId} />
// </TabsContent>
