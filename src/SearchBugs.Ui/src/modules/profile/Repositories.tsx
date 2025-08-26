import {
    Card,
    CardContent,
    CardHeader,
  } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { GitFork, Star } from "lucide-react";

import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
  } from "@/components/ui/select";
import { Repo } from "src/models/Repo";
import { useApi } from "@/hooks/useApi";

export const Repositories : React.FC = () => {
    const { data, refetch } = useApi<Repo[]>("repo");

    return (
        <Card>
        <CardHeader>
            <div className="flex items-center space-x-4">
                <Input placeholder="Find a repository..." />
                <Button>Filter</Button>
                <Select>
                    <SelectTrigger>
                        <SelectValue placeholder="Sort by" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="stars">Stars</SelectItem>
                        <SelectItem value="forks">Forks</SelectItem>
                        <SelectItem value="date">Date created</SelectItem>
                    </SelectContent>
                </Select>
                <Select>
                    <SelectTrigger>
                        <SelectValue placeholder="Type" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="public">Public</SelectItem>
                        <SelectItem value="private">Private</SelectItem>
                    </SelectContent>
                </Select>
                <Select>
                    <SelectTrigger>
                        <SelectValue placeholder="Language" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="javascript">JavaScript</SelectItem>
                        <SelectItem value="typescript">TypeScript</SelectItem>
                        <SelectItem value="python">Python</SelectItem>
                    </SelectContent>
                </Select>

            </div>

            
        </CardHeader>
        <CardContent>
          <ul className="space-y-4">
            {
                data?.value?.map((repo) => (
                    <li key={repo.id} className="flex items-center space-x-4">
                    <div className="flex-1">
                        <h3 className="text-lg font-semibold">
                        <a href="#" className="text-blue-600 hover:underline">
                            {repo.name}
                        </a>
                        </h3>
                        <p className="text-sm text-gray-500">{repo.description}</p>
                    </div>
                    <div className="flex items-center space-x-2">
                        <Badge variant="outline" className="flex items-center space-x-1">
                        <Star className="h-4 w-4" />
                        <span>{repo.stars}</span>
                        </Badge>
                        <Badge variant="outline" className="flex items-center space-x-1">
                        <GitFork className="h-4 w-4" />
                        <span>{repo.forks}</span>
                        </Badge>
                    </div>
                    </li>
                ))
            }
          </ul>
        </CardContent>
      </Card>
    );
}