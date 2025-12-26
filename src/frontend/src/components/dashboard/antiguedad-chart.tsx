'use client';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { useCarteraAntiguedad, formatMoney, formatPercent } from '@/lib/hooks/use-cartera';

function ChartSkeleton() {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-5 w-40" />
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {[...Array(5)].map((_, i) => (
            <div key={i} className="space-y-2">
              <div className="flex justify-between">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-4 w-20" />
              </div>
              <Skeleton className="h-2 w-full" />
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

const rangoColors: Record<string, string> = {
  Vigente: 'bg-green-500',
  Dias1a30: 'bg-yellow-500',
  Dias31a60: 'bg-orange-500',
  Dias61a90: 'bg-red-400',
  MasDe90: 'bg-red-600',
};

export function AntiguedadChart() {
  const { data, isLoading, isError } = useCarteraAntiguedad();

  if (isLoading) {
    return <ChartSkeleton />;
  }

  if (isError || !data) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Antiguedad de Cartera</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            Error al cargar datos de antiguedad.
          </p>
        </CardContent>
      </Card>
    );
  }

  const hasData = data.rangos.some((r) => r.monto > 0);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Antiguedad de Cartera</CardTitle>
      </CardHeader>
      <CardContent>
        {!hasData ? (
          <div className="py-8 text-center">
            <p className="text-sm text-muted-foreground">
              Sin facturas activas. Sincroniza tu cartera para ver los datos.
            </p>
          </div>
        ) : (
          <div className="space-y-4">
            {data.rangos.map((rango) => (
              <div key={rango.rango} className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span className="font-medium">{rango.label}</span>
                  <span className="text-muted-foreground">
                    {formatMoney(rango.monto)}
                    <span className="ml-2 text-xs">
                      ({formatPercent(rango.porcentaje)})
                    </span>
                  </span>
                </div>
                <div className="h-2 bg-muted rounded-full overflow-hidden">
                  <div
                    className={`h-full rounded-full transition-all ${rangoColors[rango.rango] || 'bg-primary'}`}
                    style={{ width: `${Math.max(rango.porcentaje, 1)}%` }}
                  />
                </div>
                <div className="flex justify-between text-xs text-muted-foreground">
                  <span>{rango.facturas} facturas</span>
                </div>
              </div>
            ))}
            <div className="pt-4 border-t">
              <div className="flex justify-between font-semibold">
                <span>Total</span>
                <span>{formatMoney(data.total)}</span>
              </div>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
