'use client';

import { useMutation, useQuery } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { toast } from 'sonner';
import { apiClient, ApiRequestError } from '../api/client';
import { useAuthStore } from '../stores/auth-store';
import type {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  RefreshTokenRequest,
  RefreshTokenResponse,
  User,
} from '../api/types';

// ============================================================
// AUTH HOOKS
// ============================================================

/**
 * Hook to access auth state and actions
 */
export function useAuth() {
  const store = useAuthStore();
  return {
    user: store.user,
    organization: store.organization,
    isAuthenticated: store.isAuthenticated,
    isLoading: store.isLoading,
    logout: store.logout,
  };
}

/**
 * Login mutation hook
 */
export function useLogin() {
  const router = useRouter();
  const login = useAuthStore((state) => state.login);

  return useMutation({
    mutationFn: async (data: LoginRequest) => {
      const response = await apiClient.post<AuthResponse>('/api/auth/login', data);
      return response.data;
    },
    onSuccess: (data) => {
      login(data.tokens, data.user, data.organization);
      toast.success('Inicio de sesión exitoso');
      router.push('/dashboard');
    },
    onError: (error: Error) => {
      if (error instanceof ApiRequestError) {
        if (error.status === 401) {
          toast.error('Credenciales incorrectas');
        } else if (error.status === 429) {
          toast.error('Demasiados intentos. Intente más tarde.');
        } else {
          toast.error(error.message || 'Error al iniciar sesión');
        }
      } else {
        toast.error('Error de conexión');
      }
    },
  });
}

/**
 * Register mutation hook
 */
export function useRegister() {
  const router = useRouter();
  const login = useAuthStore((state) => state.login);

  return useMutation({
    mutationFn: async (data: RegisterRequest) => {
      const response = await apiClient.post<AuthResponse>('/api/auth/register', data);
      return response.data;
    },
    onSuccess: (data) => {
      login(data.tokens, data.user, data.organization);
      toast.success('Cuenta creada exitosamente');
      router.push('/dashboard');
    },
    onError: (error: Error) => {
      if (error instanceof ApiRequestError) {
        if (error.status === 409) {
          toast.error('El correo electrónico ya está registrado');
        } else if (error.status === 400) {
          const problemDetails = error.data as { detail?: string };
          toast.error(problemDetails?.detail || 'Datos de registro inválidos');
        } else {
          toast.error(error.message || 'Error al crear cuenta');
        }
      } else {
        toast.error('Error de conexión');
      }
    },
  });
}

/**
 * Logout mutation hook
 */
export function useLogout() {
  const router = useRouter();
  const { logout, tokens } = useAuthStore();

  return useMutation({
    mutationFn: async () => {
      if (tokens?.refreshToken) {
        try {
          await apiClient.post('/api/auth/logout', {
            refreshToken: tokens.refreshToken,
          });
        } catch {
          // Ignore errors on logout - we'll clear local state anyway
        }
      }
    },
    onSettled: () => {
      logout();
      router.push('/login');
      toast.success('Sesión cerrada');
    },
  });
}

/**
 * Refresh token mutation hook
 */
export function useRefreshToken() {
  const { tokens, setTokens, logout } = useAuthStore();

  return useMutation({
    mutationFn: async () => {
      if (!tokens?.refreshToken) {
        throw new Error('No refresh token available');
      }

      const data: RefreshTokenRequest = {
        refreshToken: tokens.refreshToken,
      };

      const response = await apiClient.post<RefreshTokenResponse>(
        '/api/auth/refresh',
        data
      );
      return response.data;
    },
    onSuccess: (data) => {
      setTokens({
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
        expiresIn: data.expiresIn,
      });
    },
    onError: () => {
      // If refresh fails, logout the user
      logout();
    },
  });
}

/**
 * Get current user query hook
 */
export function useCurrentUser() {
  const { isAuthenticated } = useAuth();
  const logout = useAuthStore((state) => state.logout);

  return useQuery({
    queryKey: ['auth', 'me'],
    queryFn: async () => {
      const response = await apiClient.get<User>('/api/auth/me');
      return response.data;
    },
    enabled: isAuthenticated,
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: (failureCount, error) => {
      if (error instanceof ApiRequestError && error.status === 401) {
        logout();
        return false;
      }
      return failureCount < 2;
    },
  });
}
