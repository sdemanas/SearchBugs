import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api";

interface ErrorResult {
  code: string;
  message: string;
}

interface ApiResult<T> {
  value: T;
  error: ErrorResult;
  isFailure: boolean;
  isSuccess: boolean;
}

export const useApi = <T>(url: string) => {
  const queryClient = useQueryClient();
  const query = useQuery<ApiResult<T>>({
    queryKey: [url],
    queryFn: async () => {
      const response = await api.get<ApiResult<T>>(url);
      return response.data;
    },
  });

  const mutate = useMutation({
    mutationFn: async (data: unknown) => {
      const response = await api.post<ApiResult<T>>(url, data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [url] });
    },
  });

  const mutatePut = useMutation({
    mutationFn: async (data: unknown) => {
      const response = await api.put<ApiResult<T>>(url, data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [url] });
    },
  });

  const mutateDelete = useMutation({
    mutationFn: async () => {
      const response = await api.delete<ApiResult<T>>(url);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [url] });
    },
  });

  return {
    ...query,
    mutate,
    mutatePut,
    mutateDelete,
  };
};
