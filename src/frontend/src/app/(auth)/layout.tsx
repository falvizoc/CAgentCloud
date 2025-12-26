import type { ReactNode } from 'react';

interface AuthLayoutProps {
  children: ReactNode;
}

export default function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-background px-4 py-8">
      <div className="w-full max-w-md space-y-8">
        {/* Logo/Brand */}
        <div className="text-center">
          <h1 className="text-2xl font-bold tracking-tight">
            CobranzaCloud
          </h1>
          <p className="text-sm text-muted-foreground mt-1">
            Gestión inteligente de cobranza
          </p>
        </div>

        {/* Auth Content */}
        {children}

        {/* Footer */}
        <p className="text-center text-xs text-muted-foreground">
          &copy; {new Date().getFullYear()} CobranzaCloud. Todos los derechos reservados.
        </p>
      </div>
    </div>
  );
}
