import { QueryClientProvider } from "@tanstack/react-query";
import Route from "./Route";
import { queryClient } from "./lib/queryClient";
import { AuthProvider } from "./contexts/AuthContext";
import { NotificationProvider } from "./contexts/NotificationContext";
import { NotificationContainer } from "./components/NotificationContainer";
import { Toaster } from "@/components/ui/toaster";
import { useEffect } from "react";
import { useTimezoneStore } from "./stores/global/timezoneStore";
import { useThemeInitialization } from "./hooks/useThemeInitialization";

function App() {
  const initializeTimezone = useTimezoneStore(
    (state) => state.initializeTimezone
  );

  // Initialize theme
  useThemeInitialization();

  // Initialize timezone on app startup
  useEffect(() => {
    initializeTimezone();
  }, [initializeTimezone]);

  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <NotificationProvider>
          <Route />
          <Toaster />
          <NotificationContainer />
        </NotificationProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
