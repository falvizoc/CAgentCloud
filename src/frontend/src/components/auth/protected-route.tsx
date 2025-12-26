'use client';

import { useEffect, type ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/lib/stores/auth-store';

interface ProtectedRouteProps {
  children: ReactNode;
  fallback?: ReactNode;
}

function LoadingSkeleton() {
  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="space-y-4 w-full max-w-md px-4">
        {/* Header skeleton */}
        <div className="h-8 w-48 bg-muted animate-pulse rounded mx-auto" />

        {/* Card skeleton */}
        <div className="bg-card border rounded-xl p-6 space-y-4">
          <div className="h-4 w-32 bg-muted animate-pulse rounded" />
          <div className="h-10 w-full bg-muted animate-pulse rounded" />
          <div className="h-4 w-24 bg-muted animate-pulse rounded" />
          <div className="h-10 w-full bg-muted animate-pulse rounded" />
          <div className="h-10 w-full bg-muted animate-pulse rounded mt-4" />
        </div>
      </div>
    </div>
  );
}

export function ProtectedRoute({ children, fallback }: ProtectedRouteProps) {
  const router = useRouter();
  const { isAuthenticated, isLoading } = useAuthStore();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace('/login');
    }
  }, [isLoading, isAuthenticated, router]);

  if (isLoading) {
    return fallback ?? <LoadingSkeleton />;
  }

  if (!isAuthenticated) {
    return fallback ?? <LoadingSkeleton />;
  }

  return <>{children}</>;
}
