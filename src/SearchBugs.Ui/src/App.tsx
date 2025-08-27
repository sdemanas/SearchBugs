import { QueryClientProvider } from "@tanstack/react-query";
import Route from "./Route";
import { queryClient } from "./lib/queryClient";
import { AuthProvider } from "./contexts/AuthContext";
import { Toaster } from "@/components/ui/toaster";

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <Route />
        <Toaster />
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
