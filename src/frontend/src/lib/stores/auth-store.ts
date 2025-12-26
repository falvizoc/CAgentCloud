import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import type { User, TokenPair, OrganizationSummary } from '../api/types';

const TOKEN_REFRESH_MARGIN_MS = 60 * 1000; // Refresh 1 min before expiry

interface AuthState {
  // State
  user: User | null;
  organization: OrganizationSummary | null;
  tokens: TokenPair | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  tokenExpiresAt: number | null;

  // Actions
  login: (tokens: TokenPair, user: User, organization: OrganizationSummary) => void;
  logout: () => void;
  setTokens: (tokens: TokenPair) => void;
  setLoading: (loading: boolean) => void;
  getAccessToken: () => string | null;
  shouldRefreshToken: () => boolean;
  clearAuth: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      // Initial state
      user: null,
      organization: null,
      tokens: null,
      isAuthenticated: false,
      isLoading: true,
      tokenExpiresAt: null,

      // Actions
      login: (tokens, user, organization) => {
        const expiresAt = Date.now() + tokens.expiresIn * 1000;
        set({
          tokens,
          user,
          organization,
          isAuthenticated: true,
          isLoading: false,
          tokenExpiresAt: expiresAt,
        });
      },

      logout: () => {
        set({
          user: null,
          organization: null,
          tokens: null,
          isAuthenticated: false,
          isLoading: false,
          tokenExpiresAt: null,
        });
      },

      setTokens: (tokens) => {
        const expiresAt = Date.now() + tokens.expiresIn * 1000;
        set({
          tokens,
          tokenExpiresAt: expiresAt,
        });
      },

      setLoading: (loading) => {
        set({ isLoading: loading });
      },

      getAccessToken: () => {
        const { tokens } = get();
        return tokens?.accessToken ?? null;
      },

      shouldRefreshToken: () => {
        const { tokenExpiresAt, tokens } = get();
        if (!tokenExpiresAt || !tokens) return false;
        return Date.now() >= tokenExpiresAt - TOKEN_REFRESH_MARGIN_MS;
      },

      clearAuth: () => {
        set({
          user: null,
          organization: null,
          tokens: null,
          isAuthenticated: false,
          isLoading: false,
          tokenExpiresAt: null,
        });
      },
    }),
    {
      name: 'cobranza-auth',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        user: state.user,
        organization: state.organization,
        tokens: state.tokens,
        isAuthenticated: state.isAuthenticated,
        tokenExpiresAt: state.tokenExpiresAt,
      }),
      onRehydrateStorage: () => (state) => {
        // After rehydration, check if token is still valid
        if (state) {
          const isExpired = state.tokenExpiresAt
            ? Date.now() >= state.tokenExpiresAt
            : true;

          if (isExpired) {
            state.clearAuth();
          }
          state.setLoading(false);
        }
      },
    }
  )
);

// Selector hooks for common use cases
export const useUser = () => useAuthStore((state) => state.user);
export const useOrganization = () => useAuthStore((state) => state.organization);
export const useIsAuthenticated = () => useAuthStore((state) => state.isAuthenticated);
export const useAuthLoading = () => useAuthStore((state) => state.isLoading);
