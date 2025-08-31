import React from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ArrowLeft, Bug } from "lucide-react";
import { CommentSection } from "@/components/Comments";
import { useComments } from "@/hooks/useComments";
import { useApi } from "@/hooks/useApi";
import { PageLoadingState } from "@/components/ui/loading";
import type { Bug as BugType } from "@/lib/api";

export const BugCommentsPage: React.FC = () => {
  const { bugId } = useParams<{ bugId: string }>();
  const navigate = useNavigate();

  // Always call hooks before any early returns
  const { data: bugResponse, isLoading: isLoadingBug } = useApi<BugType>(
    `bugs/${bugId || "invalid"}`
  );
  const {
    comments,
    isLoading: isLoadingComments,
    isAddingComment,
    addComment,
  } = useComments(bugId || "");

  // Early return after hooks
  if (!bugId) {
    navigate("/bugs");
    return null;
  }

  const bug = bugResponse?.value;

  if (isLoadingBug) {
    return <PageLoadingState />;
  }

  if (!bug) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center py-12">
          <Bug className="w-16 h-16 text-muted-foreground mx-auto mb-4" />
          <h2 className="text-2xl font-bold mb-2">Bug not found</h2>
          <p className="text-muted-foreground mb-4">
            The bug you're looking for doesn't exist or you don't have
            permission to view it.
          </p>
          <Button onClick={() => navigate("/bugs")}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Back to Bugs
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => navigate(`/bugs/${bugId}`)}
          className="gap-2"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to Bug Details
        </Button>
      </div>

      {/* Bug Info Summary */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Bug className="w-5 h-5" />
            {bug.title}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div>
              <span className="text-muted-foreground">Status:</span>
              <div className="font-medium">{bug.status}</div>
            </div>
            <div>
              <span className="text-muted-foreground">Priority:</span>
              <div className="font-medium">{bug.priority}</div>
            </div>
            <div>
              <span className="text-muted-foreground">Severity:</span>
              <div className="font-medium">{bug.severity}</div>
            </div>
            <div>
              <span className="text-muted-foreground">Comments:</span>
              <div className="font-medium">{comments.length}</div>
            </div>
          </div>
          {bug.description && (
            <div className="mt-4 pt-4 border-t">
              <span className="text-muted-foreground text-sm">
                Description:
              </span>
              <p className="mt-2 text-sm whitespace-pre-wrap">
                {bug.description}
              </p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Comments Section */}
      <CommentSection
        comments={comments}
        isLoading={isLoadingComments}
        onAddComment={addComment}
        isAddingComment={isAddingComment}
        title="Discussion"
        maxHeight={600}
        className="max-w-4xl"
      />
    </div>
  );
};
