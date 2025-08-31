import React, { useState, useEffect } from "react";
import { useQuery, useMutation } from "@tanstack/react-query";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  GitBranch,
  Folder,
  File,
  Download,
  Copy,
  RefreshCw,
  Code2,
  FolderOpen,
  Eye,
  ChevronRight,
  Home,
} from "lucide-react";
import { apiClient, GitTreeItem } from "@/lib/api";
import { cn } from "@/lib/utils";

interface RepositoryBrowserProps {
  repoUrl: string;
  initialBranch?: string;
}

interface BreadcrumbItem {
  name: string;
  path: string;
}

export const RepositoryBrowser: React.FC<RepositoryBrowserProps> = ({
  repoUrl,
  initialBranch = "main",
}) => {
  const [currentBranch, setCurrentBranch] = useState(initialBranch);
  const [currentPath, setCurrentPath] = useState("");
  const [currentCommitSha, setCurrentCommitSha] = useState("HEAD");
  const [selectedFile, setSelectedFile] = useState<string | null>(null);
  const [fileContent, setFileContent] = useState<string>("");
  const [showCloneDialog, setShowCloneDialog] = useState(false);
  const [cloneTargetPath, setCloneTargetPath] = useState("");

  // Get branches
  const { data: branches = [], refetch: refetchBranches } = useQuery({
    queryKey: ["branches", repoUrl],
    queryFn: async () => {
      const response = await apiClient.repositories.getBranches(repoUrl);
      // Handle ApiResponse<string[]> wrapper from API
      if (response.data?.isSuccess && Array.isArray(response.data.value)) {
        return response.data.value;
      }
      return [];
    },
  });

  // Get tree items for current path
  const {
    data: treeItems = [],
    isLoading: isTreeLoading,
    refetch: refetchTree,
  } = useQuery({
    queryKey: ["tree", repoUrl, currentCommitSha, currentPath],
    queryFn: async () => {
      const response = await apiClient.repositories.getTree(
        repoUrl,
        currentCommitSha
      );
      // Handle ApiResponse<GitTreeItem[]> wrapper from API
      let allItems: GitTreeItem[] = [];
      if (response.data?.isSuccess && Array.isArray(response.data.value)) {
        allItems = response.data.value;
      }

      // Filter items based on current path
      if (!currentPath) {
        return allItems.filter((item) => !item.path.includes("/"));
      }

      const pathPrefix = currentPath + "/";
      return allItems
        .filter((item) => item.path.startsWith(pathPrefix))
        .map((item) => ({
          ...item,
          name: item.path.replace(pathPrefix, "").split("/")[0],
          path: item.path,
        }))
        .filter(
          (item, index, self) =>
            self.findIndex((i) => i.name === item.name) === index
        );
    },
  });

  // Get file content
  const { data: currentFileContent, isLoading: isFileLoading } = useQuery({
    queryKey: ["fileContent", repoUrl, currentCommitSha, selectedFile],
    queryFn: async () => {
      if (!selectedFile) return null;
      const response = await apiClient.repositories.getFileContent(
        repoUrl,
        currentCommitSha,
        selectedFile
      );
      // Handle ApiResponse<string> wrapper from API
      if (response.data?.isSuccess && typeof response.data.value === "string") {
        return response.data.value;
      }
      return "";
    },
    enabled: !!selectedFile,
  });

  // Clone mutation
  const cloneMutation = useMutation({
    mutationFn: async (targetPath: string) => {
      return await apiClient.repositories.clone(repoUrl, targetPath);
    },
    onSuccess: () => {
      console.log("Repository cloned successfully");
      setShowCloneDialog(false);
      setCloneTargetPath("");
    },
    onError: (error: Error) => {
      console.error("Failed to clone repository:", error.message);
    },
  });

  // Update file content when selectedFile changes
  useEffect(() => {
    if (currentFileContent) {
      setFileContent(currentFileContent);
    }
  }, [currentFileContent]);

  const handleBranchChange = (branch: string) => {
    setCurrentBranch(branch);
    setCurrentPath("");
    setSelectedFile(null);
    // In a real implementation, you'd get the commit SHA for the branch
    setCurrentCommitSha("HEAD");
  };

  const handleItemClick = (item: GitTreeItem) => {
    if (item.type === "Tree") {
      setCurrentPath(item.path);
      setSelectedFile(null);
    } else if (item.type === "Blob") {
      setSelectedFile(item.path);
    }
  };

  const generateBreadcrumbs = (): BreadcrumbItem[] => {
    if (!currentPath) return [];

    const parts = currentPath.split("/");
    const breadcrumbs: BreadcrumbItem[] = [];

    for (let i = 0; i < parts.length; i++) {
      breadcrumbs.push({
        name: parts[i],
        path: parts.slice(0, i + 1).join("/"),
      });
    }

    return breadcrumbs;
  };

  const copyToClipboard = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text);
      console.log("Copied to clipboard");
    } catch (error) {
      console.error("Failed to copy to clipboard");
    }
  };

  const getFileIcon = (filename: string) => {
    const extension = filename.split(".").pop()?.toLowerCase();

    switch (extension) {
      case "js":
      case "ts":
      case "jsx":
      case "tsx":
        return <Code2 className="h-4 w-4 text-yellow-500" />;
      case "md":
        return <File className="h-4 w-4 text-blue-500" />;
      case "json":
        return <File className="h-4 w-4 text-green-500" />;
      default:
        return <File className="h-4 w-4 text-gray-500" />;
    }
  };

  const breadcrumbs = generateBreadcrumbs();

  return (
    <div className="space-y-4">
      {/* Header with branch selector and actions */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-2">
          <GitBranch className="h-5 w-5" />
          <Select value={currentBranch} onValueChange={handleBranchChange}>
            <SelectTrigger className="w-48">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {branches.map((branch) => (
                <SelectItem key={branch} value={branch}>
                  {branch}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Badge variant="outline">{branches.length} branches</Badge>
        </div>

        <div className="flex space-x-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => {
              refetchBranches();
              refetchTree();
            }}
          >
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>

          <Dialog open={showCloneDialog} onOpenChange={setShowCloneDialog}>
            <DialogTrigger asChild>
              <Button variant="outline" size="sm">
                <Download className="h-4 w-4 mr-2" />
                Clone
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Clone Repository</DialogTitle>
              </DialogHeader>
              <div className="space-y-4">
                <div>
                  <label className="text-sm font-medium">Repository URL</label>
                  <div className="flex mt-1">
                    <Input value={repoUrl} readOnly className="flex-1" />
                    <Button
                      variant="outline"
                      size="sm"
                      className="ml-2"
                      onClick={() => copyToClipboard(repoUrl)}
                    >
                      <Copy className="h-4 w-4" />
                    </Button>
                  </div>
                </div>

                <div>
                  <label className="text-sm font-medium">Target Path</label>
                  <Input
                    value={cloneTargetPath}
                    onChange={(e) => setCloneTargetPath(e.target.value)}
                    placeholder="Enter target directory path"
                    className="mt-1"
                  />
                </div>

                <Button
                  onClick={() => cloneMutation.mutate(cloneTargetPath)}
                  disabled={!cloneTargetPath || cloneMutation.isPending}
                  className="w-full"
                >
                  {cloneMutation.isPending ? "Cloning..." : "Clone Repository"}
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      {/* Breadcrumb navigation */}
      {breadcrumbs.length > 0 && (
        <div className="flex items-center space-x-2 text-sm">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setCurrentPath("")}
            className="p-1 h-auto"
          >
            <Home className="h-4 w-4" />
          </Button>
          {breadcrumbs.map((crumb) => (
            <React.Fragment key={crumb.path}>
              <ChevronRight className="h-4 w-4 text-gray-400" />
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setCurrentPath(crumb.path)}
                className="p-1 h-auto"
              >
                {crumb.name}
              </Button>
            </React.Fragment>
          ))}
        </div>
      )}

      <Tabs defaultValue="files" className="w-full">
        <TabsList>
          <TabsTrigger value="files">
            <FolderOpen className="h-4 w-4 mr-2" />
            Files
          </TabsTrigger>
          <TabsTrigger value="preview" disabled={!selectedFile}>
            <Eye className="h-4 w-4 mr-2" />
            Preview
          </TabsTrigger>
        </TabsList>

        <TabsContent value="files">
          <Card>
            <CardContent className="p-0">
              {isTreeLoading ? (
                <div className="p-4 text-center">
                  <RefreshCw className="h-6 w-6 animate-spin mx-auto mb-2" />
                  Loading...
                </div>
              ) : (
                <div className="divide-y">
                  {treeItems.map((item) => (
                    <div
                      key={item.path}
                      className={cn(
                        "flex items-center p-3 hover:bg-gray-50 cursor-pointer transition-colors",
                        selectedFile === item.path && "bg-blue-50"
                      )}
                      onClick={() => handleItemClick(item)}
                    >
                      <div className="flex items-center flex-1 space-x-3">
                        {item.type === "Tree" ? (
                          <Folder className="h-5 w-5 text-blue-500" />
                        ) : (
                          getFileIcon(item.name)
                        )}
                        <span className="font-medium">{item.name}</span>
                        {item.type === "Tree" && (
                          <Badge variant="secondary" className="ml-2">
                            folder
                          </Badge>
                        )}
                      </div>
                    </div>
                  ))}

                  {treeItems.length === 0 && (
                    <div className="p-8 text-center text-gray-500">
                      <Folder className="h-12 w-12 mx-auto mb-2 opacity-50" />
                      <p>This directory is empty</p>
                    </div>
                  )}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="preview">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center space-x-2">
                {selectedFile && getFileIcon(selectedFile)}
                <span>{selectedFile}</span>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => selectedFile && copyToClipboard(fileContent)}
                >
                  <Copy className="h-4 w-4" />
                </Button>
              </CardTitle>
            </CardHeader>
            <CardContent>
              {isFileLoading ? (
                <div className="text-center py-8">
                  <RefreshCw className="h-6 w-6 animate-spin mx-auto mb-2" />
                  Loading file content...
                </div>
              ) : (
                <pre className="bg-gray-50 p-4 rounded-lg overflow-auto max-h-96 text-sm">
                  <code>{fileContent}</code>
                </pre>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};
