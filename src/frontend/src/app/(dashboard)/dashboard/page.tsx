import { KpiCards } from '@/components/dashboard/kpi-cards';
import { AntiguedadChart } from '@/components/dashboard/antiguedad-chart';
import { ClientesTable } from '@/components/dashboard/clientes-table';

export const metadata = {
  title: 'Dashboard - CobranzaCloud',
  description: 'Resumen de tu cartera de cobranza',
};

export default function DashboardPage() {
  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Dashboard</h2>
        <p className="text-muted-foreground">
          Resumen de tu cartera de cobranza
        </p>
      </div>

      <KpiCards />

      <div className="grid gap-6 lg:grid-cols-2">
        <AntiguedadChart />
        <div className="rounded-xl border bg-card p-6">
          <h3 className="text-lg font-semibold mb-4">Actividad Reciente</h3>
          <p className="text-sm text-muted-foreground">
            Pronto: historial de comunicaciones y pagos recientes
          </p>
        </div>
      </div>

      <ClientesTable />
    </div>
  );
}
