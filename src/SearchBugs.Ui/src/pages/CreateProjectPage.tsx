import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { FieldValues, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useMutation } from "react-query";
import { api } from "@/lib/api";
import { useToast } from "@/components/ui/use-toast";
import { useNavigate } from "react-router-dom";

export const CreateProjectPage = () => {
  const CreateProjectSchema = z.object({
    name: z
      .string()
      .min(3, "Name must be at least 3 characters long")
      .max(50, "Name must be at most 50 characters long"),
    description: z
      .string()
      .max(500, "Description must be at most 500 characters long"),
  });
  const { toast } = useToast();
  const navigate = useNavigate();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(CreateProjectSchema),
  });

  const createProjectMutation = useMutation((data: FieldValues) => {
    return api.post("/projects", data).then(() =>
      toast({
        title: "Project created",
        description: "The project has been created successfully",
      })
    );
  });

  const onSubmit = (data: FieldValues) => {
    createProjectMutation.mutate(data);
    navigate("/projects");
  };

  return (
    <div className="flex flex-col gap-3 md:gap-8">
      <div className="flex items-center">
        <h5 className="text-lg font-semibold">Create Project</h5>
      </div>
      <Card className="w-full">
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid gap-2">
              <Label htmlFor="name">Project Name</Label>
              <Input
                id="name"
                placeholder="Enter project name"
                {...register("name")}
                className={errors.name ? "border-red-500" : ""}
              />
              {errors.name && (
                <p className="text-red-500 text-sm">{errors.name.message}</p>
              )}
            </div>
            <div className="grid gap-2">
              <Label htmlFor="description">Description</Label>
              <Textarea
                id="description"
                placeholder="Enter project description"
                {...register("description")}
                className={errors.description ? "border-red-500" : ""}
              />
              {errors.description && (
                <p className="text-red-500 text-sm">
                  {errors.description.message}
                </p>
              )}
            </div>
            <Button type="submit" disabled={createProjectMutation.isLoading}>
              Create Project
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
};
