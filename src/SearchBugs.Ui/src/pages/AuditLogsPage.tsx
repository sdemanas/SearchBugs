import React, { useState, useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import {
  FileText,
  Search,
  RefreshCw,
  Clock,
  User,
  Globe,
  CheckCircle,
  XCircle,
  Eye,
  Activity,
  AlertCircle,
} from "lucide-react";
import { apiClient, AuditLog, User as UserType } from "@/lib/api";
import { format } from "date-fns";

interface AuditLogDetailDialogProps {
  auditLog: AuditLog | null;
  isOpen: boolean;
  onClose: () => void;
}

const isDataTruncated = (data?: string) => {
  return (
    data?.includes("[TRUNCATED") || data?.includes("... [TRUNCATED") || false
  );
};

const AuditLogDetailDialog: React.FC<AuditLogDetailDialogProps> = ({
  auditLog,
  isOpen,
  onClose,
}) => {
  if (!auditLog) return null;

  const formatJsonData = (data: string) => {
    try {
      // Check if data is truncated
      const isTruncated =
        data.includes("[TRUNCATED") || data.includes("... [TRUNCATED");

      if (isTruncated) {
        // Extract the original length if available
        const truncateMatch = data.match(/\[TRUNCATED.*?(\d+)\s*chars\]/);
        const originalLength = truncateMatch ? truncateMatch[1] : "unknown";

        // Try to parse the non-truncated part
        const truncatedPart = data.replace(/\.\.\.\s*\[TRUNCATED.*?\]$/, "");
        let formattedPart = truncatedPart;

        try {
          // Attempt to parse what we have
          const parsed = JSON.parse(truncatedPart + "}"); // Add closing brace in case it's missing
          formattedPart = JSON.stringify(parsed, null, 2);
        } catch {
          // If parsing fails, just use the raw truncated data
          formattedPart = truncatedPart;
        }

        return `${formattedPart}\n\n⚠️ [DATA TRUNCATED - Original size: ${originalLength} characters]`;
      }

      // Normal JSON formatting for non-truncated data
      return JSON.stringify(JSON.parse(data), null, 2);
    } catch {
      // Return raw data if JSON parsing fails
      return data;
    }
  };

  const formatDuration = (duration: string) => {
    try {
      const parts = duration.split(":");
      if (parts.length >= 3) {
        const hours = parseInt(parts[0]);
        const minutes = parseInt(parts[1]);
        const seconds = parseFloat(parts[2]);

        if (hours > 0) {
          return `${hours}h ${minutes}m ${seconds.toFixed(2)}s`;
        } else if (minutes > 0) {
          return `${minutes}m ${seconds.toFixed(2)}s`;
        } else {
          return `${seconds.toFixed(2)}s`;
        }
      }
    } catch {
      const ms = parseFloat(duration);
      if (!isNaN(ms)) {
        return `${ms.toFixed(2)}ms`;
      }
    }
    return duration;
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Audit Log Details
          </DialogTitle>
          <DialogDescription>
            Detailed information about the audit log entry
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6">
          {/* Basic Information */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label className="text-sm font-semibold text-muted-foreground">
                Request Name
              </Label>
              <p className="text-sm font-mono bg-muted p-2 rounded mt-1">
                {auditLog.requestName}
              </p>
            </div>
            <div>
              <Label className="text-sm font-semibold text-muted-foreground">
                Status
              </Label>
              <div className="mt-1">
                <Badge
                  variant={auditLog.isSuccess ? "default" : "destructive"}
                  className="flex items-center gap-1 w-fit"
                >
                  {auditLog.isSuccess ? (
                    <CheckCircle className="h-3 w-3" />
                  ) : (
                    <XCircle className="h-3 w-3" />
                  )}
                  {auditLog.isSuccess ? "Success" : "Failed"}
                </Badge>
              </div>
            </div>
            <div>
              <Label className="text-sm font-semibold text-muted-foreground">
                Duration
              </Label>
              <p className="text-sm flex items-center gap-1 mt-1">
                <Clock className="h-3 w-3" />
                {formatDuration(auditLog.duration)}
              </p>
            </div>
            <div>
              <Label className="text-sm font-semibold text-muted-foreground">
                Timestamp
              </Label>
              <p className="text-sm mt-1">
                {format(new Date(auditLog.createdOnUtc), "PPpp")}
              </p>
            </div>
          </div>

          {/* User Information */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label className="text-sm font-semibold text-muted-foreground">
                User
              </Label>
              <p className="text-sm flex items-center gap-1 mt-1">
                <User className="h-3 w-3" />
                {auditLog.userName || "Anonymous"}
                {auditLog.userId && (
                  <span className="text-xs text-muted-foreground">
                    (ID: {auditLog.userId.substring(0, 8)}...)
                  </span>
                )}
              </p>
            </div>
            <div>
              <Label className="text-sm font-semibold text-muted-foreground">
                IP Address
              </Label>
              <p className="text-sm flex items-center gap-1 mt-1">
                <Globe className="h-3 w-3" />
                {auditLog.ipAddress}
              </p>
            </div>
          </div>

          {/* User Agent */}
          <div>
            <Label className="text-sm font-semibold text-muted-foreground">
              User Agent
            </Label>
            <p className="text-sm bg-muted p-2 rounded mt-1 break-all font-mono">
              {auditLog.userAgent}
            </p>
          </div>

          {/* Error Message */}
          {auditLog.errorMessage && (
            <div>
              <Label className="text-sm font-semibold text-destructive">
                Error Message
              </Label>
              <p className="text-sm bg-destructive/10 p-2 rounded mt-1 text-destructive border border-destructive/20">
                {auditLog.errorMessage}
              </p>
            </div>
          )}

          {/* Request Data */}
          <div>
            <Label className="text-sm font-semibold text-muted-foreground">
              Request Data
              {isDataTruncated(auditLog.requestData) && (
                <Badge
                  variant="outline"
                  className="ml-2 text-xs bg-yellow-50 text-yellow-800 border-yellow-300"
                >
                  Truncated
                </Badge>
              )}
            </Label>
            <div className="max-h-32 overflow-y-auto border rounded mt-1">
              <pre className="text-xs p-3 whitespace-pre-wrap font-mono">
                {formatJsonData(auditLog.requestData)}
              </pre>
            </div>
          </div>

          {/* Response Data */}
          {auditLog.responseData && (
            <div>
              <Label className="text-sm font-semibold text-muted-foreground">
                Response Data
                {isDataTruncated(auditLog.responseData) && (
                  <Badge
                    variant="outline"
                    className="ml-2 text-xs bg-yellow-50 text-yellow-800 border-yellow-300"
                  >
                    Truncated
                  </Badge>
                )}
              </Label>
              <div className="max-h-32 overflow-y-auto border rounded mt-1">
                <pre className="text-xs p-3 whitespace-pre-wrap font-mono">
                  {formatJsonData(auditLog.responseData)}
                </pre>
              </div>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};

const AuditLogsPage = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [userFilter, setUserFilter] = useState<string>("all");
  const [startDate, setStartDate] = useState<string>("");
  const [endDate, setEndDate] = useState<string>("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(50);
  const [selectedAuditLog, setSelectedAuditLog] = useState<AuditLog | null>(
    null
  );
  const [isDetailDialogOpen, setIsDetailDialogOpen] = useState(false);

  // Fetch audit logs
  const {
    data: auditLogsResponse,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: [
      "auditLogs",
      {
        userId: userFilter === "all" ? undefined : userFilter,
        startDate: startDate || undefined,
        endDate: endDate || undefined,
        pageNumber,
        pageSize,
      },
    ],
    queryFn: async () => {
      try {
        const params = {
          userId: userFilter === "all" ? undefined : userFilter,
          startDate: startDate || undefined,
          endDate: endDate || undefined,
          pageNumber,
          pageSize,
        };

        const response = await apiClient.auditLogs.getAll(params);
        return response.data.value as AuditLog[];
      } catch (error) {
        console.error("Failed to fetch audit logs:", error);
        return [];
      }
    },
  });

  const auditLogs = useMemo(() => auditLogsResponse || [], [auditLogsResponse]);

  // Fetch users for filter dropdown
  const { data: usersResponse } = useQuery({
    queryKey: ["users"],
    queryFn: async () => {
      try {
        const response = await apiClient.users.getAll();
        console.log("Users API response:", response.data);
        return response.data.value as UserType[];
      } catch (error) {
        console.error("Failed to fetch users:", error);
        return [];
      }
    },
  });

  const users = useMemo(() => {
    console.log("usersResponse:", usersResponse);
    if (!usersResponse || !Array.isArray(usersResponse)) {
      console.warn("Users response is not an array:", usersResponse);
      return [];
    }
    return usersResponse;
  }, [usersResponse]);

  // Filter audit logs based on search term and status
  const filteredAuditLogs = useMemo(() => {
    return auditLogs.filter((log) => {
      const matchesSearch =
        log.requestName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        log.userName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        log.ipAddress.includes(searchTerm) ||
        (log.errorMessage?.toLowerCase().includes(searchTerm.toLowerCase()) ??
          false);

      const matchesStatus =
        statusFilter === "all" ||
        (statusFilter === "success" && log.isSuccess) ||
        (statusFilter === "failed" && !log.isSuccess);

      return matchesSearch && matchesStatus;
    });
  }, [auditLogs, searchTerm, statusFilter]);

  const handleViewDetails = (auditLog: AuditLog) => {
    setSelectedAuditLog(auditLog);
    setIsDetailDialogOpen(true);
  };

  const formatDate = (dateString: string) => {
    return format(new Date(dateString), "MMM dd, yyyy HH:mm:ss");
  };

  const formatDuration = (duration: string) => {
    try {
      const parts = duration.split(":");
      if (parts.length >= 3) {
        const hours = parseInt(parts[0]);
        const minutes = parseInt(parts[1]);
        const seconds = parseFloat(parts[2]);

        if (hours > 0) {
          return `${hours}h ${minutes}m`;
        } else if (minutes > 0) {
          return `${minutes}m ${seconds.toFixed(1)}s`;
        } else if (seconds >= 1) {
          return `${seconds.toFixed(1)}s`;
        } else {
          return `${(seconds * 1000).toFixed(0)}ms`;
        }
      }
    } catch {
      return duration;
    }
    return duration;
  };

  const getStatusBadge = (isSuccess: boolean) => {
    return (
      <Badge
        variant={isSuccess ? "default" : "destructive"}
        className="flex items-center gap-1"
      >
        {isSuccess ? (
          <CheckCircle className="h-3 w-3" />
        ) : (
          <XCircle className="h-3 w-3" />
        )}
        {isSuccess ? "Success" : "Failed"}
      </Badge>
    );
  };

  const getRequestTypeBadge = (requestName: string) => {
    const isCommand = requestName.includes("Command");
    const isQuery = requestName.includes("Query");

    if (isCommand) {
      return (
        <Badge variant="secondary" className="bg-orange-100 text-orange-800">
          Command
        </Badge>
      );
    } else if (isQuery) {
      return (
        <Badge variant="secondary" className="bg-blue-100 text-blue-800">
          Query
        </Badge>
      );
    }

    return <Badge variant="outline">Request</Badge>;
  };

  if (error) {
    return (
      <div className="container mx-auto p-6">
        <Card>
          <CardContent className="pt-6">
            <div className="text-center">
              <AlertCircle className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <h3 className="text-lg font-semibold mb-2">
                Error loading audit logs
              </h3>
              <p className="text-muted-foreground mb-4">
                There was an error loading the audit logs.
              </p>
              <Button onClick={() => refetch()}>Try Again</Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Audit Logs</h1>
          <p className="text-muted-foreground">
            View system activity logs and user actions
          </p>
        </div>

        <div className="flex items-center gap-2">
          <Button variant="outline" onClick={() => refetch()}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Logs</CardTitle>
            <Activity className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{auditLogs.length}</div>
            <p className="text-xs text-muted-foreground">Current page</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Successful</CardTitle>
            <CheckCircle className="h-4 w-4 text-green-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">
              {auditLogs.filter((log) => log.isSuccess).length}
            </div>
            <p className="text-xs text-muted-foreground">
              {auditLogs.length > 0
                ? Math.round(
                    (auditLogs.filter((log) => log.isSuccess).length /
                      auditLogs.length) *
                      100
                  )
                : 0}
              % success rate
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Failed</CardTitle>
            <XCircle className="h-4 w-4 text-red-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-red-600">
              {auditLogs.filter((log) => !log.isSuccess).length}
            </div>
            <p className="text-xs text-muted-foreground">
              {auditLogs.length > 0
                ? Math.round(
                    (auditLogs.filter((log) => !log.isSuccess).length /
                      auditLogs.length) *
                      100
                  )
                : 0}
              % failure rate
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Unique Users</CardTitle>
            <User className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {
                new Set(
                  auditLogs.filter((log) => log.userId).map((log) => log.userId)
                ).size
              }
            </div>
            <p className="text-xs text-muted-foreground">Active users</p>
          </CardContent>
        </Card>
      </div>

      {/* Filters and Search */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Audit Logs</CardTitle>
          <CardDescription>View and filter system audit logs</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 mb-4 md:grid-cols-2 lg:grid-cols-6">
            <div className="lg:col-span-2">
              <Label htmlFor="search">Search</Label>
              <div className="relative">
                <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input
                  id="search"
                  placeholder="Search logs..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-8"
                />
              </div>
            </div>

            <div>
              <Label htmlFor="status">Status</Label>
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All</SelectItem>
                  <SelectItem value="success">Success</SelectItem>
                  <SelectItem value="failed">Failed</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label htmlFor="user">User</Label>
              <Select value={userFilter} onValueChange={setUserFilter}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Users</SelectItem>
                  {Array.isArray(users) &&
                    users.map((user) => (
                      <SelectItem key={user.id} value={user.id}>
                        {user.firstName} {user.lastName}
                      </SelectItem>
                    ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label htmlFor="startDate">Start Date</Label>
              <Input
                id="startDate"
                type="datetime-local"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
              />
            </div>

            <div>
              <Label htmlFor="endDate">End Date</Label>
              <Input
                id="endDate"
                type="datetime-local"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
              />
            </div>
          </div>

          {(startDate || endDate) && (
            <div className="mb-4">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => {
                  setStartDate("");
                  setEndDate("");
                }}
              >
                Clear date filters
              </Button>
            </div>
          )}

          {isLoading ? (
            <div className="space-y-2">
              {[...Array(10)].map((_, i) => (
                <div
                  key={i}
                  className="flex items-center space-x-4 p-4 border rounded"
                >
                  <div className="h-4 w-4 bg-muted rounded animate-pulse"></div>
                  <div className="flex-1 space-y-2">
                    <div className="h-4 bg-muted rounded animate-pulse"></div>
                    <div className="h-3 bg-muted rounded w-2/3 animate-pulse"></div>
                  </div>
                  <div className="h-6 w-16 bg-muted rounded animate-pulse"></div>
                  <div className="h-8 w-8 bg-muted rounded animate-pulse"></div>
                </div>
              ))}
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Request</TableHead>
                  <TableHead>User</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Duration</TableHead>
                  <TableHead>Timestamp</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredAuditLogs.map((log) => (
                  <TableRow key={log.id}>
                    <TableCell>
                      <div className="space-y-1">
                        <div className="flex items-center gap-2 flex-wrap">
                          <code className="text-sm font-mono bg-muted px-2 py-1 rounded">
                            {log.requestName}
                          </code>
                          {getRequestTypeBadge(log.requestName)}
                          {(isDataTruncated(log.requestData) ||
                            isDataTruncated(log.responseData)) && (
                            <Badge
                              variant="outline"
                              className="text-xs bg-yellow-50 text-yellow-800 border-yellow-300"
                            >
                              <FileText className="h-3 w-3 mr-1" />
                              Large Data
                            </Badge>
                          )}
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <User className="h-4 w-4 text-muted-foreground" />
                        <div>
                          <div className="font-medium">
                            {log.userName || "Anonymous"}
                          </div>
                          <div className="text-xs text-muted-foreground flex items-center gap-1">
                            <Globe className="h-3 w-3" />
                            {log.ipAddress}
                          </div>
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>
                      {getStatusBadge(log.isSuccess)}
                      {log.errorMessage && (
                        <div className="text-xs text-destructive mt-1 truncate max-w-32">
                          {log.errorMessage}
                        </div>
                      )}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1 text-sm">
                        <Clock className="h-3 w-3 text-muted-foreground" />
                        {formatDuration(log.duration)}
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="text-sm text-muted-foreground">
                        {formatDate(log.createdOnUtc)}
                      </div>
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleViewDetails(log)}
                      >
                        <Eye className="h-4 w-4" />
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}

          {!isLoading && filteredAuditLogs.length === 0 && (
            <div className="text-center py-8">
              <FileText className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <h3 className="text-lg font-medium mb-2">No audit logs found</h3>
              <p className="text-muted-foreground">
                {searchTerm || statusFilter !== "all" || userFilter !== "all"
                  ? "Try adjusting your search or filter criteria"
                  : "No audit logs are available for the selected time period"}
              </p>
            </div>
          )}

          {/* Pagination */}
          {filteredAuditLogs.length >= pageSize && (
            <div className="flex items-center justify-between pt-4">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPageNumber(Math.max(1, pageNumber - 1))}
                disabled={pageNumber <= 1}
              >
                Previous
              </Button>
              <span className="text-sm text-muted-foreground">
                Page {pageNumber}
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPageNumber(pageNumber + 1)}
                disabled={filteredAuditLogs.length < pageSize}
              >
                Next
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Detail Dialog */}
      <AuditLogDetailDialog
        auditLog={selectedAuditLog}
        isOpen={isDetailDialogOpen}
        onClose={() => {
          setIsDetailDialogOpen(false);
          setSelectedAuditLog(null);
        }}
      />
    </div>
  );
};

export { AuditLogsPage };
export default AuditLogsPage;
