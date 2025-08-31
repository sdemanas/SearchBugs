import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { FormInput, FormTextarea } from "@/components/ui/form";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useToast } from "@/components/ui/use-toast";
import { useNavigate } from "react-router-dom";
import { apiClient } from "@/lib/api";
import { LoaderIcon, ArrowLeft } from "lucide-react";
import {
  createProjectSchema,
  type CreateProjectFormData,
} from "@/lib/validations";

export const CreateProjectPage = () => {
  const { toast } = useToast();
  const navigate = useNavigate();

  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateProjectFormData>({
    resolver: zodResolver(createProjectSchema),
    defaultValues: {
      name: "",
      description: "",
    },
  });

  const onSubmit = async (data: CreateProjectFormData) => {
    try {
      const response = await apiClient.projects.create({
        name: data.name,
        description: data.description || "",
      });

      if (response.data) {
        toast({
          title: "Project created",
          description: "The project has been created successfully.",
        });
        navigate("/projects");
      }
    } catch (error) {
      toast({
        title: "Error creating project",
        description: "Failed to create the project. Please try again.",
        variant: "destructive",
      });
    }
  };

  return (
    <div className="container mx-auto py-6 max-w-2xl">
      <div className="flex items-center gap-4 mb-6">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => navigate(-1)}
          className="gap-1"
        >
          <ArrowLeft className="h-4 w-4" />
          Back
        </Button>
        <h1 className="text-2xl font-bold">Create Project</h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)}>
        <Card>
          <CardHeader>
            <CardTitle>Project Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <FormInput
              control={control}
              name="name"
              label="Project Name"
              placeholder="Enter project name"
              required
              disabled={isSubmitting}
              error={errors.name}
            />
            <FormTextarea
              control={control}
              name="description"
              label="Description"
              placeholder="Enter project description (optional)"
              className="min-h-[100px]"
              disabled={isSubmitting}
              error={errors.description}
            />
          </CardContent>
        </Card>

        <div className="flex items-center gap-2 mt-6">
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate(-1)}
            disabled={isSubmitting}
          >
            Cancel
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting && (
              <LoaderIcon className="mr-2 h-4 w-4 animate-spin" />
            )}
            Create Project
          </Button>
        </div>
      </form>
    </div>
  );
};
