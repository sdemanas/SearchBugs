import { create } from "zustand";
import { accessTokenKey } from "@/lib/api";

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

interface AuthState {
  // State only - no loading states (React Query handles that)
  user: User | null;
  isAuthenticated: boolean;
  token: string | null;

  // Actions for state management only
  setUser: (user: User | null) => void;
  setToken: (token: string | null) => void;
  logout: () => void;
  initializeAuth: () => void;

  // Utils
  getTokenPayload: () => any;
  isTokenExpired: () => boolean;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  // State - no loading states (React Query handles that)
  user: null,
  isAuthenticated: false,
  token: null,

  // Actions for state management only
  setUser: (user: User | null) => {
    set({
      user,
      isAuthenticated: !!user,
    });
  },

  setToken: (token: string | null) => {
    set({ token });
    if (token) {
      localStorage.setItem(accessTokenKey, token);
    } else {
      localStorage.removeItem(accessTokenKey);
    }
  },

  logout: () => {
    localStorage.removeItem(accessTokenKey);
    set({
      user: null,
      isAuthenticated: false,
      token: null,
    });
  },

  initializeAuth: () => {
    const token = localStorage.getItem(accessTokenKey);
    if (token && !get().isTokenExpired()) {
      set({
        token,
        // User will be set when React Query fetches user data
      });
    } else {
      localStorage.removeItem(accessTokenKey);
    }
  },

  // Utils
  getTokenPayload: () => {
    const token = get().token;
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split(".")[1]));
      return payload;
    } catch (error) {
      console.error("Error parsing token payload:", error);
      return null;
    }
  },

  isTokenExpired: () => {
    const payload = get().getTokenPayload();
    if (!payload || !payload.exp) return true;

    const currentTime = Math.floor(Date.now() / 1000);
    return payload.exp < currentTime;
  },
}));
