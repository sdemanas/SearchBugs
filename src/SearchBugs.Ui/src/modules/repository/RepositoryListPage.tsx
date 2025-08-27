import React from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import {
  GitBranch,
  Plus,
  Search,
  Calendar,
  ExternalLink,
  RefreshCw,
} from "lucide-react";
import { apiClient, Repository } from "@/lib/api";
import { CardLoadingSkeleton } from "@/components/ui/loading";

export const RepositoryListPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = React.useState("");

  const {
    data: repositories = [],
    isLoading,
    refetch,
  } = useQuery({
    queryKey: ["repositories"],
    queryFn: async () => {
      try {
        const response = await apiClient.repositories.getAll();
        console.log("API Response:", response.data);
        return Array.isArray(response.data) ? response.data : [];
      } catch (error) {
        console.error("Error fetching repositories:", error);
        return [];
      }
    },
  });

  const filteredRepositories = Array.isArray(repositories)
    ? repositories.filter(
        (repo: Repository) =>
          repo.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
          repo.url.toLowerCase().includes(searchTerm.toLowerCase())
      )
    : [];

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const handleRepositoryClick = (repo: Repository) => {
    navigate(`/repositories/${encodeURIComponent(repo.url)}`);
  };

  if (isLoading) {
    return (
      <div className="container mx-auto p-6">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-3xl font-bold">Repositories</h1>
            <p className="text-gray-600 mt-1">Manage your code repositories</p>
          </div>

          <div className="flex space-x-2">
            <Button variant="outline" disabled>
              <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
              Loading...
            </Button>

            <Button disabled>
              <Plus className="h-4 w-4 mr-2" />
              Add Repository
            </Button>
          </div>
        </div>

        <div className="mb-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
            <Input
              placeholder="Search repositories..."
              disabled
              className="pl-10"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {[...Array(6)].map((_, index) => (
            <CardLoadingSkeleton key={index} />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold">Repositories</h1>
          <p className="text-gray-600 mt-1">Manage your code repositories</p>
        </div>

        <div className="flex space-x-2">
          <Button variant="outline" onClick={() => refetch()}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>

          <Button onClick={() => navigate("/repositories/create")}>
            <Plus className="h-4 w-4 mr-2" />
            Add Repository
          </Button>
        </div>
      </div>

      {/* Search */}
      <div className="mb-6">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
          <Input
            placeholder="Search repositories..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-10"
          />
        </div>
      </div>

      {/* Repository Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredRepositories.map((repo) => (
          <Card
            key={repo.id}
            className="hover:shadow-md transition-shadow cursor-pointer"
            onClick={() => handleRepositoryClick(repo)}
          >
            <CardHeader>
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <CardTitle className="flex items-center space-x-2">
                    <GitBranch className="h-5 w-5" />
                    <span className="truncate">{repo.name}</span>
                  </CardTitle>
                  <CardDescription className="mt-2 line-clamp-2 text-xs">
                    {repo.url}
                  </CardDescription>
                </div>

                <Button
                  variant="ghost"
                  size="sm"
                  onClick={(e) => {
                    e.stopPropagation();
                    window.open(repo.url, "_blank");
                  }}
                >
                  <ExternalLink className="h-4 w-4" />
                </Button>
              </div>
            </CardHeader>

            <CardContent>
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2 text-sm text-gray-500">
                  <Calendar className="h-4 w-4" />
                  <span>{formatDate(repo.createdOnUtc)}</span>
                </div>

                <Badge variant="secondary">{repo.type}</Badge>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Empty State */}
      {filteredRepositories.length === 0 && !isLoading && (
        <div className="text-center py-12">
          <GitBranch className="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            {searchTerm ? "No repositories found" : "No repositories yet"}
          </h3>
          <p className="text-gray-500 mb-4">
            {searchTerm
              ? `No repositories match "${searchTerm}"`
              : "Get started by adding your first repository"}
          </p>
          {!searchTerm && (
            <Button onClick={() => navigate("/repositories/create")}>
              <Plus className="h-4 w-4 mr-2" />
              Add Repository
            </Button>
          )}
        </div>
      )}
    </div>
  );
};
