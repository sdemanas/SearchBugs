import { QueryClient, QueryFunction } from "@tanstack/react-query";
import { api } from "./api";

// Default query function that uses our API client
const defaultQueryFn: QueryFunction = async ({ queryKey }) => {
  const [endpoint, ...params] = queryKey as string[];
  const response = await api.get(`/${endpoint}${params.length ? `/${params.join('/')}` : ''}`);
  return response.data;
};

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60 * 1000 * 5, // 5 minutes
      retry: 1,
      refetchOnWindowFocus: false,
      queryFn: defaultQueryFn,
    },
    mutations: {
      retry: 1,
      onError: (error) => {
        console.error('Mutation error:', error);
      },
    },
  },
});