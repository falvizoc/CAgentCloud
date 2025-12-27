'use client';

import { useParams, useRouter } from 'next/navigation';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { useClienteDetalle, formatMoney, getOverdueColorClass } from '@/lib/hooks/use-cartera';

function DetailSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Skeleton className="h-8 w-8" />
        <Skeleton className="h-8 w-64" />
      </div>
      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <Skeleton className="h-5 w-32" />
          </CardHeader>
          <CardContent className="space-y-3">
            {[...Array(5)].map((_, i) => (
              <Skeleton key={i} className="h-4 w-full" />
            ))}
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <Skeleton className="h-5 w-32" />
          </CardHeader>
          <CardContent className="space-y-3">
            {[...Array(4)].map((_, i) => (
              <Skeleton key={i} className="h-4 w-full" />
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

export default function ClienteDetailPage() {
  const params = useParams();
  const router = useRouter();
  const id = params.id as string;
  const { data: cliente, isLoading, isError } = useClienteDetalle(id);

  if (isLoading) {
    return <DetailSkeleton />;
  }

  if (isError || !cliente) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => router.back()}>
          ← Volver
        </Button>
        <Card>
          <CardContent className="pt-6">
            <p className="text-center text-muted-foreground">
              Cliente no encontrado o error al cargar.
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" onClick={() => router.back()}>
            ← Volver
          </Button>
          <div>
            <h1 className="text-2xl font-bold">{cliente.nombre}</h1>
            <p className="text-sm text-muted-foreground font-mono">{cliente.clave}</p>
          </div>
        </div>
        <div className="text-right">
          <p className="text-2xl font-bold">{formatMoney(cliente.saldoTotal)}</p>
          <p className={`text-sm ${getOverdueColorClass(cliente.diasMaxVencido)}`}>
            {cliente.saldoVencido > 0
              ? `${formatMoney(cliente.saldoVencido)} vencido (${cliente.diasMaxVencido} dias)`
              : 'Sin saldo vencido'}
          </p>
        </div>
      </div>

      {/* Info Cards */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* General Info */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Informacion General</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3 text-sm">
            <div className="flex justify-between">
              <span className="text-muted-foreground">RFC</span>
              <span>{cliente.rfc || '-'}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Email</span>
              <span>{cliente.email || '-'}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Telefono</span>
              <span>{cliente.telefono || '-'}</span>
            </div>
            {cliente.direccion && (
              <div className="pt-2 border-t">
                <span className="text-muted-foreground block mb-1">Direccion</span>
                <p>
                  {[
                    cliente.direccion.calle,
                    cliente.direccion.colonia,
                    cliente.direccion.ciudad,
                    cliente.direccion.estado,
                    cliente.direccion.codigoPostal,
                  ]
                    .filter(Boolean)
                    .join(', ') || '-'}
                </p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Contacts */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Contactos</CardTitle>
          </CardHeader>
          <CardContent>
            {cliente.contactos.length === 0 ? (
              <p className="text-sm text-muted-foreground">Sin contactos registrados</p>
            ) : (
              <div className="space-y-3">
                {cliente.contactos.map((contacto) => (
                  <div
                    key={contacto.id}
                    className="flex justify-between items-start text-sm border-b pb-2 last:border-0"
                  >
                    <div>
                      <p className="font-medium">
                        {contacto.nombre}
                        {contacto.principal && (
                          <span className="ml-2 text-xs bg-primary/10 text-primary px-2 py-0.5 rounded">
                            Principal
                          </span>
                        )}
                      </p>
                      {contacto.email && (
                        <p className="text-muted-foreground">{contacto.email}</p>
                      )}
                    </div>
                    {contacto.telefono && (
                      <span className="text-muted-foreground">{contacto.telefono}</span>
                    )}
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Facturas */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">
            Facturas Activas ({cliente.facturas.length})
          </CardTitle>
        </CardHeader>
        <CardContent>
          {cliente.facturas.length === 0 ? (
            <p className="text-sm text-muted-foreground text-center py-4">
              Sin facturas activas
            </p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Folio</TableHead>
                  <TableHead>Fecha</TableHead>
                  <TableHead>Vencimiento</TableHead>
                  <TableHead className="text-right">Total</TableHead>
                  <TableHead className="text-right">Saldo</TableHead>
                  <TableHead className="text-right">Dias</TableHead>
                  <TableHead>Estado</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {cliente.facturas.map((factura) => (
                  <TableRow key={factura.id}>
                    <TableCell className="font-mono text-xs">{factura.folio}</TableCell>
                    <TableCell>
                      {new Date(factura.fecha).toLocaleDateString('es-MX')}
                    </TableCell>
                    <TableCell>
                      {new Date(factura.vencimiento).toLocaleDateString('es-MX')}
                    </TableCell>
                    <TableCell className="text-right">
                      {formatMoney(factura.total)}
                    </TableCell>
                    <TableCell className="text-right font-medium">
                      {formatMoney(factura.saldo)}
                    </TableCell>
                    <TableCell
                      className={`text-right font-medium ${getOverdueColorClass(factura.diasVencido)}`}
                    >
                      {factura.diasVencido > 0 ? factura.diasVencido : '-'}
                    </TableCell>
                    <TableCell>
                      <span
                        className={`text-xs px-2 py-1 rounded ${
                          factura.status === 'Vencida'
                            ? 'bg-destructive/10 text-destructive'
                            : 'bg-green-100 text-green-700'
                        }`}
                      >
                        {factura.status}
                      </span>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
