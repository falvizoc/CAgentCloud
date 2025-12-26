'use client';

import { ProtectedRoute } from '@/components/auth';
import { useAuthStore, useUser, useOrganization } from '@/lib/stores/auth-store';
import { useLogout } from '@/lib/hooks/use-auth';
import { Button } from '@/components/ui/button';
import Link from 'next/link';

function UserMenu() {
  const user = useUser();
  const organization = useOrganization();
  const logout = useLogout();

  return (
    <div className="flex items-center gap-4">
      <div className="text-right hidden sm:block">
        <p className="text-sm font-medium">{user?.name}</p>
        <p className="text-xs text-muted-foreground">{organization?.name}</p>
      </div>
      <Button
        variant="outline"
        size="sm"
        onClick={() => logout.mutate()}
        disabled={logout.isPending}
      >
        {logout.isPending ? 'Saliendo...' : 'Salir'}
      </Button>
    </div>
  );
}

function DashboardSkeleton() {
  return (
    <div className="min-h-screen bg-background">
      <header className="border-b">
        <div className="container flex h-16 items-center justify-between px-4">
          <div className="h-6 w-36 bg-muted animate-pulse rounded" />
          <div className="flex items-center gap-4">
            <div className="h-4 w-24 bg-muted animate-pulse rounded" />
            <div className="h-8 w-16 bg-muted animate-pulse rounded" />
          </div>
        </div>
      </header>
      <main className="container py-6 px-4">
        <div className="space-y-6">
          <div className="h-8 w-48 bg-muted animate-pulse rounded" />
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            {[...Array(4)].map((_, i) => (
              <div key={i} className="h-32 bg-muted animate-pulse rounded-xl" />
            ))}
          </div>
          <div className="h-64 bg-muted animate-pulse rounded-xl" />
        </div>
      </main>
    </div>
  );
}

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <ProtectedRoute fallback={<DashboardSkeleton />}>
      <div className="min-h-screen bg-background">
        <header className="border-b bg-card">
          <div className="container flex h-16 items-center justify-between px-4">
            <Link href="/dashboard" className="flex items-center gap-2">
              <span className="text-xl font-bold text-primary">CobranzaCloud</span>
            </Link>
            <nav className="hidden md:flex items-center gap-6">
              <Link
                href="/dashboard"
                className="text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
              >
                Dashboard
              </Link>
              <Link
                href="/clientes"
                className="text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
              >
                Clientes
              </Link>
              <Link
                href="/configuracion"
                className="text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
              >
                Configuracion
              </Link>
            </nav>
            <UserMenu />
          </div>
        </header>
        <main className="container py-6 px-4">
          {children}
        </main>
      </div>
    </ProtectedRoute>
  );
}
