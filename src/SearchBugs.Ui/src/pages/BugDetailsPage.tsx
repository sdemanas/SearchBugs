import { useParams, useNavigate } from "react-router-dom";
import { useApi } from "@/hooks/useApi";
import { apiClient } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Textarea } from "@/components/ui/textarea";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useState } from "react";
import {
  Clock,
  MessageSquare,
  Paperclip,
  History,
  Settings,
  ArrowLeft,
  Download,
} from "lucide-react";
import { safeFormatDistance } from "@/lib/date-utils";
import { useToast } from "@/components/ui/use-toast";
import { PageLoadingState } from "@/components/ui/loading";

interface Bug {
  id: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  severity: string;
  projectId: string;
  assigneeId: string;
  reporterId: string;
  createdAt: string;
  updatedAt: string;
}

interface Comment {
  id: string;
  commentText: string;
  userId: string;
  createdOnUtc: string;
  modifiedOnUtc?: string;
  userName?: string; // This might come from a join or separate call
}

interface Attachment {
  id: string;
  fileName: string;
  fileSize: number;
  uploadedAt: string;
  uploadedBy: string;
}

interface TimeEntry {
  id: string;
  duration: string;
  description: string;
  loggedAt: string;
  loggedBy: string;
}

interface HistoryEntry {
  id: string;
  action: string;
  description: string;
  timestamp: string;
  userId: string;
  userName: string;
}

interface CustomField {
  id: string;
  name: string;
  value: string;
}

export const BugDetailsPage = () => {
  const { bugId } = useParams<{ bugId: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [newComment, setNewComment] = useState("");
  const [isAddingComment, setIsAddingComment] = useState(false);
  const [timeEntry, setTimeEntry] = useState({ duration: "", description: "" });
  const [customField, setCustomField] = useState({ name: "", value: "" });

  const { data: bug, isLoading: isLoadingBug } = useApi<Bug>(`bugs/${bugId}`);
  const { data: comments, mutate: mutateComments } = useApi<Comment[]>(
    `bugs/${bugId}/comments`
  );
  const {
    data: attachments,
    mutate: { mutateAsync: mutateAttachments },
  } = useApi<Attachment[]>(`bugs/${bugId}/attachments`);
  const { data: timeEntries, mutate: mutateTimeEntries } = useApi<TimeEntry[]>(
    `bugs/${bugId}/time-tracking`
  );
  const { data: history } = useApi<HistoryEntry[]>(`bugs/${bugId}/history`);
  const { data: customFields, mutate: mutateCustomFields } = useApi<
    CustomField[]
  >(`bugs/${bugId}/custom-fields`);

  const handleAddComment = async () => {
    if (!newComment.trim() || !bugId) return;
    setIsAddingComment(true);
    try {
      const response = await apiClient.comments.create({
        bugId: bugId,
        content: newComment,
      });

      if (response.data.isSuccess) {
        setNewComment("");
        // Refetch comments
        if (mutateComments.mutate) {
          mutateComments.mutate({});
        }
        toast({
          title: "Comment added",
          description: "Your comment has been added successfully.",
        });
      } else {
        toast({
          title: "Error",
          description: response.data.error?.message || "Failed to add comment",
          variant: "destructive",
        });
      }
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to add comment. Please try again.",
        variant: "destructive",
      });
    } finally {
      setIsAddingComment(false);
    }
  };

  const handleTrackTime = async () => {
    if (!timeEntry.duration || !timeEntry.description) return;
    try {
      const result = await mutateTimeEntries.mutateAsync(timeEntry);
      if (result.isSuccess) {
        setTimeEntry({ duration: "", description: "" });
        toast({
          title: "Time logged",
          description: "Time has been logged successfully.",
        });
      } else {
        toast({
          title: "Error",
          description: result.error.message,
          variant: "destructive",
        });
      }
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to log time. Please try again.",
        variant: "destructive",
      });
    }
  };

  const handleAddCustomField = async () => {
    if (!customField.name || !customField.value) return;
    try {
      const result = await mutateCustomFields.mutateAsync(customField);
      if (result.isSuccess) {
        setCustomField({ name: "", value: "" });
        toast({
          title: "Custom field added",
          description: "Custom field has been added successfully.",
        });
      } else {
        toast({
          title: "Error",
          description: result.error.message,
          variant: "destructive",
        });
      }
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to add custom field. Please try again.",
        variant: "destructive",
      });
    }
  };

  const handleFileUpload = async (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const formData = new FormData();
    formData.append("file", file);

    try {
      const response = await fetch(`/api/bugs/${bugId}/attachments`, {
        method: "POST",
        body: formData,
      });
      const result = await response.json();
      if (result.isSuccess) {
        await mutateAttachments({});
        toast({
          title: "File uploaded",
          description: "File has been uploaded successfully.",
        });
      } else {
        toast({
          title: "Error",
          description: result.error.message,
          variant: "destructive",
        });
      }
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to upload file. Please try again.",
        variant: "destructive",
      });
    }
  };

  if (isLoadingBug) return <PageLoadingState text="Loading bug details..." />;
  if (!bug?.value) return <div>Bug not found</div>;

  const bugData = bug.value;

  return (
    <div className="container mx-auto py-6 space-y-6 max-w-7xl">
      <div className="flex items-center justify-between bg-card p-4 rounded-lg shadow-sm">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{bugData.title}</h1>
          <div className="flex gap-2 mt-2">
            <Badge
              variant={
                bugData.status === "Open"
                  ? "default"
                  : bugData.status === "In Progress"
                  ? "secondary"
                  : "outline"
              }
            >
              {bugData.status}
            </Badge>
            <Badge
              variant={
                bugData.priority === "Critical"
                  ? "destructive"
                  : bugData.priority === "High"
                  ? "default"
                  : "outline"
              }
            >
              {bugData.priority}
            </Badge>
            <Badge
              variant={
                bugData.severity === "Critical"
                  ? "destructive"
                  : bugData.severity === "High"
                  ? "default"
                  : "outline"
              }
            >
              {bugData.severity}
            </Badge>
          </div>
        </div>
        <Button
          variant="outline"
          onClick={() => navigate("/bugs")}
          className="gap-2"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Bugs
        </Button>
      </div>

      <Tabs defaultValue="details" className="space-y-4">
        <TabsList className="w-full justify-start h-auto p-1 bg-muted/50">
          <TabsTrigger
            value="details"
            className="data-[state=active]:bg-background"
          >
            Details
          </TabsTrigger>
          <TabsTrigger
            value="comments"
            className="data-[state=active]:bg-background"
          >
            <MessageSquare className="w-4 h-4 mr-2" />
            Comments
          </TabsTrigger>
          <TabsTrigger
            value="attachments"
            className="data-[state=active]:bg-background"
          >
            <Paperclip className="w-4 h-4 mr-2" />
            Attachments
          </TabsTrigger>
          <TabsTrigger
            value="time"
            className="data-[state=active]:bg-background"
          >
            <Clock className="w-4 h-4 mr-2" />
            Time Tracking
          </TabsTrigger>
          <TabsTrigger
            value="history"
            className="data-[state=active]:bg-background"
          >
            <History className="w-4 h-4 mr-2" />
            History
          </TabsTrigger>
          <TabsTrigger
            value="custom-fields"
            className="data-[state=active]:bg-background"
          >
            <Settings className="w-4 h-4 mr-2" />
            Custom Fields
          </TabsTrigger>
        </TabsList>

        <TabsContent value="details" className="space-y-4">
          <Card className="border-none shadow-sm">
            <CardHeader>
              <CardTitle className="text-lg">Description</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="whitespace-pre-wrap text-muted-foreground">
                {bugData.description}
              </p>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="comments" className="space-y-4">
          <Card className="border-none shadow-sm">
            <CardContent className="pt-6">
              <div className="space-y-6">
                {comments?.value?.map((comment) => (
                  <div key={comment.id} className="flex gap-4">
                    <Avatar className="h-8 w-8">
                      <AvatarFallback className="bg-primary/10 text-primary">
                        {comment.userName?.[0] || "U"}
                      </AvatarFallback>
                    </Avatar>
                    <div className="flex-1 space-y-1">
                      <div className="flex items-center gap-2">
                        <span className="font-medium">
                          {comment.userName || "Unknown User"}
                        </span>
                        <span className="text-sm text-muted-foreground">
                          {safeFormatDistance(comment.createdOnUtc)}
                        </span>
                      </div>
                      <p className="text-sm text-muted-foreground">
                        {comment.commentText}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
              <div className="mt-6 space-y-4">
                <Textarea
                  placeholder="Add a comment..."
                  value={newComment}
                  onChange={(e) => setNewComment(e.target.value)}
                  className="min-h-[100px]"
                />
                <Button
                  onClick={handleAddComment}
                  disabled={isAddingComment}
                  className="w-full sm:w-auto"
                >
                  {isAddingComment ? "Adding..." : "Add Comment"}
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="attachments" className="space-y-4">
          <Card className="border-none shadow-sm">
            <CardContent className="pt-6">
              <div className="space-y-4">
                {attachments?.value?.map((attachment) => (
                  <div
                    key={attachment.id}
                    className="flex items-center gap-4 p-4 rounded-lg bg-muted/50"
                  >
                    <Paperclip className="w-4 h-4 text-muted-foreground" />
                    <div className="flex-1">
                      <div className="font-medium">{attachment.fileName}</div>
                      <div className="text-sm text-muted-foreground">
                        {safeFormatDistance(attachment.uploadedAt)} by{" "}
                        {attachment.uploadedBy}
                      </div>
                    </div>
                    <Button variant="ghost" size="sm" className="gap-2">
                      <Download className="h-4 w-4" />
                      Download
                    </Button>
                  </div>
                ))}
              </div>
              <div className="mt-6">
                <Label htmlFor="file" className="text-sm font-medium">
                  Upload Attachment
                </Label>
                <div className="mt-2 flex items-center gap-4">
                  <Input
                    id="file"
                    type="file"
                    onChange={handleFileUpload}
                    className="max-w-sm"
                  />
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="time" className="space-y-4">
          <Card className="border-none shadow-sm">
            <CardContent className="pt-6">
              <div className="space-y-4">
                {timeEntries?.value?.map((entry) => (
                  <div
                    key={entry.id}
                    className="flex items-center gap-4 p-4 rounded-lg bg-muted/50"
                  >
                    <Clock className="w-4 h-4 text-muted-foreground" />
                    <div className="flex-1">
                      <div className="font-medium">{entry.duration}</div>
                      <div className="text-sm text-muted-foreground">
                        {entry.description}
                      </div>
                      <div className="text-sm text-muted-foreground">
                        {safeFormatDistance(entry.loggedAt)} by {entry.loggedBy}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
              <div className="mt-6 space-y-4">
                <div className="grid gap-2">
                  <Label htmlFor="duration" className="text-sm font-medium">
                    Duration (e.g., 2h 30m)
                  </Label>
                  <Input
                    id="duration"
                    value={timeEntry.duration}
                    onChange={(e) =>
                      setTimeEntry({ ...timeEntry, duration: e.target.value })
                    }
                    placeholder="Enter duration"
                    className="max-w-sm"
                  />
                </div>
                <div className="grid gap-2">
                  <Label
                    htmlFor="timeDescription"
                    className="text-sm font-medium"
                  >
                    Description
                  </Label>
                  <Textarea
                    id="timeDescription"
                    value={timeEntry.description}
                    onChange={(e) =>
                      setTimeEntry({
                        ...timeEntry,
                        description: e.target.value,
                      })
                    }
                    placeholder="What did you work on?"
                    className="min-h-[100px]"
                  />
                </div>
                <Button
                  onClick={handleTrackTime}
                  disabled={mutateTimeEntries.isPending}
                  className="w-full sm:w-auto"
                >
                  {mutateTimeEntries.isPending ? "Logging..." : "Log Time"}
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="history" className="space-y-4">
          <Card className="border-none shadow-sm">
            <CardContent className="pt-6">
              <div className="space-y-4">
                {history?.value?.map((entry) => (
                  <div
                    key={entry.id}
                    className="flex gap-4 p-4 rounded-lg bg-muted/50"
                  >
                    <Avatar className="h-8 w-8">
                      <AvatarFallback className="bg-primary/10 text-primary">
                        {entry.userName[0]}
                      </AvatarFallback>
                    </Avatar>
                    <div className="flex-1">
                      <div className="font-medium">{entry.action}</div>
                      <div className="text-sm text-muted-foreground">
                        {entry.description}
                      </div>
                      <div className="text-sm text-muted-foreground">
                        {safeFormatDistance(entry.timestamp)} by{" "}
                        {entry.userName}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="custom-fields" className="space-y-4">
          <Card className="border-none shadow-sm">
            <CardContent className="pt-6">
              <div className="space-y-4">
                {customFields?.value?.map((field) => (
                  <div
                    key={field.id}
                    className="grid grid-cols-2 gap-4 p-4 rounded-lg bg-muted/50"
                  >
                    <div className="font-medium">{field.name}</div>
                    <div className="text-muted-foreground">{field.value}</div>
                  </div>
                ))}
              </div>
              <div className="mt-6 space-y-4">
                <div className="grid gap-2">
                  <Label htmlFor="fieldName" className="text-sm font-medium">
                    Field Name
                  </Label>
                  <Input
                    id="fieldName"
                    value={customField.name}
                    onChange={(e) =>
                      setCustomField({ ...customField, name: e.target.value })
                    }
                    placeholder="Enter field name"
                    className="max-w-sm"
                  />
                </div>
                <div className="grid gap-2">
                  <Label htmlFor="fieldValue" className="text-sm font-medium">
                    Value
                  </Label>
                  <Input
                    id="fieldValue"
                    value={customField.value}
                    onChange={(e) =>
                      setCustomField({ ...customField, value: e.target.value })
                    }
                    placeholder="Enter value"
                    className="max-w-sm"
                  />
                </div>
                <Button
                  onClick={handleAddCustomField}
                  disabled={mutateCustomFields.isPending}
                  className="w-full sm:w-auto"
                >
                  {mutateCustomFields.isPending
                    ? "Adding..."
                    : "Add Custom Field"}
                </Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};
