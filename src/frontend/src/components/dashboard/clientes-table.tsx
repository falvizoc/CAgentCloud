'use client';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { useClientes, formatMoney, getOverdueColorClass } from '@/lib/hooks/use-cartera';

function TableSkeleton() {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-5 w-40" />
      </CardHeader>
      <CardContent>
        <div className="space-y-3">
          {/* Header */}
          <div className="grid grid-cols-5 gap-4">
            {[...Array(5)].map((_, i) => (
              <Skeleton key={i} className="h-4" />
            ))}
          </div>
          {/* Rows */}
          {[...Array(5)].map((_, i) => (
            <div key={i} className="grid grid-cols-5 gap-4">
              {[...Array(5)].map((_, j) => (
                <Skeleton key={j} className="h-6" />
              ))}
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

export function ClientesTable() {
  const { data, isLoading, isError } = useClientes({
    conSaldo: true,
    pageSize: 10,
  });

  if (isLoading) {
    return <TableSkeleton />;
  }

  if (isError || !data) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Clientes con Saldo</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            Error al cargar la lista de clientes.
          </p>
        </CardContent>
      </Card>
    );
  }

  if (data.items.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Clientes con Saldo</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="py-8 text-center">
            <p className="text-sm text-muted-foreground">
              No hay clientes con saldo pendiente.
            </p>
            <p className="text-xs text-muted-foreground mt-1">
              Los clientes apareceran aqui cuando sincronices tu cartera.
            </p>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle>Clientes con Saldo</CardTitle>
        <span className="text-sm text-muted-foreground">
          {data.meta.total} clientes
        </span>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Clave</TableHead>
              <TableHead>Cliente</TableHead>
              <TableHead className="text-right">Saldo Total</TableHead>
              <TableHead className="text-right">Vencido</TableHead>
              <TableHead className="text-right">Dias</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.items.map((cliente) => (
              <TableRow key={cliente.id}>
                <TableCell className="font-medium font-mono text-xs">
                  {cliente.clave}
                </TableCell>
                <TableCell className="max-w-[200px] truncate">
                  {cliente.nombre}
                </TableCell>
                <TableCell className="text-right">
                  {formatMoney(cliente.saldoTotal)}
                </TableCell>
                <TableCell className="text-right text-destructive">
                  {formatMoney(cliente.saldoVencido)}
                </TableCell>
                <TableCell className={`text-right font-medium ${getOverdueColorClass(cliente.diasMaxVencido)}`}>
                  {cliente.diasMaxVencido > 0 ? cliente.diasMaxVencido : '-'}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        {data.meta.totalPages > 1 && (
          <div className="mt-4 text-center text-sm text-muted-foreground">
            Mostrando {data.items.length} de {data.meta.total} clientes
          </div>
        )}
      </CardContent>
    </Card>
  );
}
