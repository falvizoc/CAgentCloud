'use client';

import { useEffect, useRef, type ReactNode } from 'react';
import { useAuthStore } from '@/lib/stores/auth-store';
import { apiClient } from '@/lib/api/client';
import { useRefreshToken } from '@/lib/hooks/use-auth';

interface AuthProviderProps {
  children: ReactNode;
}

const TOKEN_CHECK_INTERVAL = 30 * 1000; // Check every 30 seconds

export function AuthProvider({ children }: AuthProviderProps) {
  const { getAccessToken, shouldRefreshToken, isAuthenticated, setLoading } =
    useAuthStore();
  const { mutate: refreshToken } = useRefreshToken();
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  // Set token getter for API client
  useEffect(() => {
    apiClient.setTokenGetter(getAccessToken);
  }, [getAccessToken]);

  // Auto-refresh token before expiry
  useEffect(() => {
    if (!isAuthenticated) {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
      return;
    }

    const checkAndRefresh = () => {
      if (shouldRefreshToken()) {
        refreshToken();
      }
    };

    // Check immediately
    checkAndRefresh();

    // Set up interval
    intervalRef.current = setInterval(checkAndRefresh, TOKEN_CHECK_INTERVAL);

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    };
  }, [isAuthenticated, shouldRefreshToken, refreshToken]);

  // Mark loading as complete after hydration
  useEffect(() => {
    // Small delay to ensure hydration is complete
    const timeout = setTimeout(() => {
      setLoading(false);
    }, 100);

    return () => clearTimeout(timeout);
  }, [setLoading]);

  return <>{children}</>;
}
