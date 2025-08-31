import React, { createContext, useState, useEffect } from "react";
import { apiClient, accessTokenKey } from "@/lib/api";

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  createdOnUtc: string;
  isImpersonating?: boolean;
  originalUserId?: string;
  originalUserEmail?: string;
}

export interface AuthContextType {
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
  register: (
    email: string,
    password: string,
    firstName: string,
    lastName: string
  ) => Promise<void>;
  logout: () => void;
  impersonate: (userId: string) => Promise<void>;
  stopImpersonate: () => Promise<void>;
  isAuthenticated: boolean;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Helper function to fetch user details from API
  const fetchUserDetails = async (userId: string): Promise<User | null> => {
    try {
      const response = await apiClient.users.getById(userId);
      if (response.data.isSuccess) {
        const userData = response.data.value;
        return {
          ...userData,
          roles: userData.roles || [],
        };
      } else {
        console.error("Failed to fetch user details:", response.data.error);
        return null;
      }
    } catch (error) {
      console.error("Error fetching user details:", error);
      return null;
    }
  };

  // Helper function to decode JWT and extract basic info
  const decodeToken = (token: string) => {
    try {
      const payload = JSON.parse(atob(token.split(".")[1]));
      const isExpired = payload.exp * 1000 < Date.now();

      if (isExpired) {
        return null;
      }

      return {
        sub: payload.sub,
        email: payload.email,
        impersonated_user_id: payload.impersonated_user_id,
        impersonated_email: payload.impersonated_email,
        roles: payload.role ? [payload.role] : [],
        exp: payload.exp,
      };
    } catch (error) {
      console.error("Token decode error:", error);
      return null;
    }
  };

  // Initialize user from token
  useEffect(() => {
    const initializeUser = async () => {
      const token = localStorage.getItem(accessTokenKey);
      if (!token) {
        setIsLoading(false);
        return;
      }

      const tokenPayload = decodeToken(token);
      if (!tokenPayload) {
        localStorage.removeItem(accessTokenKey);
        setIsLoading(false);
        return;
      }

      // Determine which user ID to fetch details for
      const isImpersonating = !!tokenPayload.impersonated_user_id;
      const userIdToFetch = isImpersonating
        ? tokenPayload.impersonated_user_id
        : tokenPayload.sub;

      // Fetch user details from API
      const userDetails = await fetchUserDetails(userIdToFetch);

      if (userDetails) {
        setUser({
          ...userDetails,
          isImpersonating,
          originalUserId: isImpersonating ? tokenPayload.sub : undefined,
          originalUserEmail: isImpersonating ? tokenPayload.email : undefined,
        });
      } else {
        // If we can't fetch user details, remove the token
        localStorage.removeItem(accessTokenKey);
      }

      setIsLoading(false);
    };

    initializeUser();
  }, []);

  const login = async (email: string, password: string) => {
    try {
      const response = await apiClient.auth.login({ email, password });

      if (!response.data.isSuccess) {
        throw new Error(response.data.error.message || "Login failed");
      }

      const { token } = response.data.value;
      localStorage.setItem(accessTokenKey, token);

      // Decode token to get user ID
      const tokenPayload = decodeToken(token);
      if (!tokenPayload) {
        throw new Error("Invalid token received");
      }

      // Fetch user details from API
      const userDetails = await fetchUserDetails(tokenPayload.sub);
      if (!userDetails) {
        throw new Error("Failed to fetch user details");
      }

      setUser(userDetails);
    } catch (error) {
      console.error("Login error:", error);
      throw new Error("Login failed");
    }
  };

  const register = async (
    email: string,
    password: string,
    firstName: string,
    lastName: string
  ) => {
    try {
      const response = await apiClient.auth.register({
        email,
        password,
        firstName,
        lastName,
      });

      if (!response.data.isSuccess) {
        throw new Error(response.data.error.message || "Registration failed");
      }

      const { token } = response.data.value;
      localStorage.setItem(accessTokenKey, token);

      // Decode token to get user ID
      const tokenPayload = decodeToken(token);
      if (!tokenPayload) {
        throw new Error("Invalid token received");
      }

      // Fetch user details from API
      const userDetails = await fetchUserDetails(tokenPayload.sub);
      if (!userDetails) {
        throw new Error("Failed to fetch user details");
      }

      setUser(userDetails);
    } catch (error) {
      console.error("Registration error:", error);
      throw new Error("Registration failed");
    }
  };

  const logout = () => {
    localStorage.removeItem(accessTokenKey);
    setUser(null);
  };

  const impersonate = async (userId: string) => {
    try {
      const response = await apiClient.auth.impersonate({
        userIdToImpersonate: userId,
      });

      if (!response.data.isSuccess) {
        throw new Error(response.data.error.message || "Impersonation failed");
      }

      const { token } = response.data.value;
      localStorage.setItem(accessTokenKey, token);

      // Decode token to get impersonation info
      const tokenPayload = decodeToken(token);
      if (!tokenPayload) {
        throw new Error("Invalid token received");
      }

      // Fetch impersonated user details from API
      const userDetails = await fetchUserDetails(
        tokenPayload.impersonated_user_id
      );
      if (!userDetails) {
        throw new Error("Failed to fetch impersonated user details");
      }

      setUser({
        ...userDetails,
        isImpersonating: true,
        originalUserId: tokenPayload.sub,
        originalUserEmail: tokenPayload.email,
      });
    } catch (error) {
      console.error("Impersonation error:", error);
      throw new Error("Impersonation failed");
    }
  };

  const stopImpersonate = async () => {
    try {
      const response = await apiClient.auth.stopImpersonate();

      if (!response.data.isSuccess) {
        throw new Error(
          response.data.error.message || "Stop impersonation failed"
        );
      }

      const { token } = response.data.value;
      localStorage.setItem(accessTokenKey, token);

      // Decode token to get original user info
      const tokenPayload = decodeToken(token);
      if (!tokenPayload) {
        throw new Error("Invalid token received");
      }

      // Fetch original user details from API
      const userDetails = await fetchUserDetails(tokenPayload.sub);
      if (!userDetails) {
        throw new Error("Failed to fetch original user details");
      }

      setUser({
        ...userDetails,
        isImpersonating: false,
      });
    } catch (error) {
      console.error("Stop impersonation error:", error);
      throw new Error("Stop impersonation failed");
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        login,
        register,
        logout,
        impersonate,
        stopImpersonate,
        isAuthenticated: !!user,
        isLoading,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export { AuthContext };
