'use client';

import { useEffect, useState } from 'react';

interface ApiInfo {
  name: string;
  version: string;
  status: string;
  environment: string;
}

export default function Home() {
  const [apiStatus, setApiStatus] = useState<ApiInfo | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const checkApi = async () => {
      try {
        const response = await fetch(
          process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'
        );
        if (response.ok) {
          const data = await response.json();
          setApiStatus(data);
        } else {
          setError('API no disponible');
        }
      } catch {
        setError('No se pudo conectar con el API');
      } finally {
        setLoading(false);
      }
    };

    checkApi();
  }, []);

  return (
    <main className="flex min-h-screen flex-col items-center justify-center p-24">
      <div className="max-w-2xl w-full space-y-8">
        <div className="text-center">
          <h1 className="text-4xl font-bold tracking-tight text-foreground">
            CobranzaCloud
          </h1>
          <p className="mt-2 text-muted-foreground">
            Plataforma inteligente de gestión de cobranza
          </p>
        </div>

        <div className="rounded-lg border bg-card p-6 shadow-sm">
          <h2 className="text-lg font-semibold mb-4">Estado del Sistema</h2>

          {loading ? (
            <div className="space-y-2">
              <div className="h-4 w-3/4 animate-pulse rounded bg-muted" />
              <div className="h-4 w-1/2 animate-pulse rounded bg-muted" />
            </div>
          ) : error ? (
            <div className="flex items-center gap-2 text-destructive">
              <span className="h-2 w-2 rounded-full bg-destructive" />
              <span>{error}</span>
            </div>
          ) : apiStatus ? (
            <div className="space-y-2">
              <div className="flex items-center gap-2">
                <span className="h-2 w-2 rounded-full bg-green-500" />
                <span className="text-green-600 font-medium">
                  API Conectada
                </span>
              </div>
              <div className="text-sm text-muted-foreground space-y-1">
                <p>
                  <span className="font-medium">Nombre:</span> {apiStatus.name}
                </p>
                <p>
                  <span className="font-medium">Versión:</span>{' '}
                  {apiStatus.version}
                </p>
                <p>
                  <span className="font-medium">Ambiente:</span>{' '}
                  {apiStatus.environment}
                </p>
              </div>
            </div>
          ) : null}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="rounded-lg border bg-card p-4">
            <h3 className="font-medium">Frontend</h3>
            <p className="text-sm text-muted-foreground">Next.js 14</p>
            <p className="text-xs text-green-600 mt-1">Funcionando</p>
          </div>
          <div className="rounded-lg border bg-card p-4">
            <h3 className="font-medium">Backend</h3>
            <p className="text-sm text-muted-foreground">.NET 9</p>
            <p
              className={`text-xs mt-1 ${apiStatus ? 'text-green-600' : 'text-amber-600'}`}
            >
              {apiStatus ? 'Funcionando' : 'Pendiente'}
            </p>
          </div>
        </div>

        <p className="text-center text-xs text-muted-foreground">
          M0: Foundation - Setup inicial del proyecto
        </p>
      </div>
    </main>
  );
}
