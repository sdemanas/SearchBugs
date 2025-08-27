import React, { createContext, useState, useEffect } from "react";
import { apiClient, accessTokenKey } from "@/lib/api";

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
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
  isAuthenticated: boolean;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem(accessTokenKey);
    if (token) {
      try {
        // Decode JWT to check expiration and get user info
        const payload = JSON.parse(atob(token.split(".")[1]));
        const isExpired = payload.exp * 1000 < Date.now();

        if (isExpired) {
          localStorage.removeItem(accessTokenKey);
          setIsLoading(false);
        } else {
          setUser({
            id: payload.sub,
            email: payload.email,
            firstName: "", // You might want to fetch this from another endpoint
            lastName: "",
            role: payload.role || "User",
          });
          setIsLoading(false);
        }
      } catch (error) {
        console.error("Token validation error:", error);
        localStorage.removeItem(accessTokenKey);
        setIsLoading(false);
      }
    } else {
      setIsLoading(false);
    }
  }, []);

  const login = async (email: string, password: string) => {
    try {
      const response = await apiClient.auth.login({ email, password });

      if (!response.data.isSuccess) {
        throw new Error(response.data.error.message || "Login failed");
      }

      const { token } = response.data.value;
      localStorage.setItem(accessTokenKey, token);

      // Decode JWT to get user info (optional - you might want to make a separate API call)
      const payload = JSON.parse(atob(token.split(".")[1]));
      setUser({
        id: payload.sub,
        email: payload.email,
        firstName: "", // You might want to add these to the JWT or fetch from another endpoint
        lastName: "",
        role: payload.role || "User",
      });
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

      // Decode JWT to get user info
      const payload = JSON.parse(atob(token.split(".")[1]));
      setUser({
        id: payload.sub,
        email: payload.email,
        firstName: firstName,
        lastName: lastName,
        role: payload.role || "User",
      });
    } catch (error) {
      console.error("Registration error:", error);
      throw new Error("Registration failed");
    }
  };

  const logout = () => {
    localStorage.removeItem(accessTokenKey);
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        login,
        register,
        logout,
        isAuthenticated: !!user,
        isLoading,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export { AuthContext };
