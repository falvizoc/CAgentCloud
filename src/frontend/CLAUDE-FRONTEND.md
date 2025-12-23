# CLAUDE-FRONTEND.md - Contexto del Agente Frontend

> **Área:** Frontend Web (Next.js 14)
> **Responsabilidad:** UI, UX, Accesibilidad, Estado del Cliente
> **Última actualización:** 2025-12-23

---

## 1. Contexto del Proyecto

Este es el frontend de **CobranzaCloud**, una plataforma SaaS de gestión de cobranza.

### Stack Tecnológico

| Componente | Tecnología | Versión |
|------------|------------|---------|
| Framework | Next.js (App Router) | 14.x |
| UI Library | React | 18.x |
| Language | TypeScript | 5.x |
| Styling | Tailwind CSS | 3.x |
| Components | shadcn/ui | latest |
| State (Server) | TanStack Query | 5.x |
| State (Client) | Zustand | 4.x |
| Forms | React Hook Form + Zod | 7.x / 3.x |
| Auth | Clerk | latest |
| i18n | next-intl | latest |
| Icons | Lucide React | latest |

### Documentos de Referencia

- [CLAUDE.md](../../CLAUDE.md) - Contexto general del proyecto
- [docs/04-UX-GUIDELINES.md](../../docs/04-UX-GUIDELINES.md) - Guías de UX
- [docs/08-FRICTIONLESS-MANIFEST.md](../../docs/08-FRICTIONLESS-MANIFEST.md) - Principios FRICTIONLESS
- [docs/contracts/api-types.ts](../../docs/contracts/api-types.ts) - Contratos de tipos

---

## 2. Estructura del Proyecto

```
src/frontend/
├── app/                          # Next.js App Router
│   ├── (auth)/                   # Grupo de rutas públicas (auth)
│   │   ├── login/
│   │   │   └── page.tsx
│   │   ├── register/
│   │   │   └── page.tsx
│   │   └── layout.tsx
│   ├── (dashboard)/              # Grupo de rutas protegidas
│   │   ├── page.tsx              # Dashboard home
│   │   ├── cartera/
│   │   │   └── page.tsx
│   │   ├── clientes/
│   │   │   ├── page.tsx
│   │   │   └── [id]/
│   │   │       └── page.tsx
│   │   ├── cobranza/
│   │   │   └── page.tsx
│   │   ├── settings/
│   │   │   ├── page.tsx
│   │   │   ├── profile/
│   │   │   ├── organization/
│   │   │   └── connectors/
│   │   └── layout.tsx            # Layout con sidebar
│   ├── api/                      # API Routes (si necesario)
│   ├── layout.tsx                # Root layout
│   ├── loading.tsx               # Global loading
│   ├── error.tsx                 # Global error
│   └── not-found.tsx
│
├── components/
│   ├── ui/                       # shadcn/ui components
│   │   ├── button.tsx
│   │   ├── card.tsx
│   │   ├── input.tsx
│   │   ├── table.tsx
│   │   └── ...
│   ├── layout/                   # Layout components
│   │   ├── sidebar.tsx
│   │   ├── header.tsx
│   │   ├── command-menu.tsx      # ⌘K navigation
│   │   └── breadcrumb.tsx
│   ├── features/                 # Feature-specific components
│   │   ├── cartera/
│   │   │   ├── cartera-chart.tsx
│   │   │   ├── antiguedad-table.tsx
│   │   │   └── kpi-cards.tsx
│   │   ├── clientes/
│   │   │   ├── cliente-list.tsx
│   │   │   ├── cliente-detail.tsx
│   │   │   └── cliente-form.tsx
│   │   └── cobranza/
│   │       ├── email-composer.tsx
│   │       └── template-selector.tsx
│   └── shared/                   # Shared components
│       ├── data-table.tsx
│       ├── loading-skeleton.tsx
│       ├── empty-state.tsx
│       └── error-boundary.tsx
│
├── lib/
│   ├── api/                      # API client
│   │   ├── client.ts
│   │   ├── auth.ts
│   │   ├── cartera.ts
│   │   ├── clientes.ts
│   │   └── types.ts              # Re-export from contracts
│   ├── hooks/                    # Custom hooks
│   │   ├── use-cartera.ts
│   │   ├── use-clientes.ts
│   │   └── use-debounce.ts
│   ├── utils/                    # Utilities
│   │   ├── cn.ts                 # className helper
│   │   ├── format.ts             # Formatters
│   │   └── validators.ts
│   └── stores/                   # Zustand stores
│       ├── ui-store.ts
│       └── filters-store.ts
│
├── styles/
│   └── globals.css               # Tailwind + custom CSS
│
├── messages/                     # i18n (next-intl)
│   ├── es.json
│   └── en.json
│
├── public/
│   ├── logo.svg
│   └── ...
│
├── next.config.js
├── tailwind.config.ts
├── tsconfig.json
└── package.json
```

---

## 3. Convenciones de Código

### 3.1 Naming Conventions

```typescript
// Componentes: PascalCase
export function ClienteDetail() { }

// Hooks: camelCase con prefix "use"
export function useCartera() { }

// Utilidades: camelCase
export function formatCurrency(amount: number) { }

// Archivos de componentes: kebab-case
// cliente-detail.tsx, cartera-chart.tsx

// Tipos/Interfaces: PascalCase
interface ClienteProps { }
type CarteraResponse = { }

// Constantes: SCREAMING_SNAKE_CASE
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL;
```

### 3.2 Estructura de Componente

```tsx
// ✅ CORRECTO: Componente con todas las convenciones
'use client'; // Solo si necesita interactividad

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { formatCurrency } from '@/lib/utils/format';
import { getCarteraResumen } from '@/lib/api/cartera';
import type { CarteraResumen } from '@/lib/api/types';

interface CarteraKpiCardsProps {
  empresaId?: string;
  className?: string;
}

export function CarteraKpiCards({ empresaId, className }: CarteraKpiCardsProps) {
  const { data, isLoading, error } = useQuery({
    queryKey: ['cartera', 'resumen', empresaId],
    queryFn: () => getCarteraResumen(empresaId),
  });

  if (isLoading) {
    return <CarteraKpiCardsSkeleton />;
  }

  if (error) {
    return <div className="text-destructive">Error al cargar datos</div>;
  }

  return (
    <div className={cn('grid gap-4 md:grid-cols-4', className)}>
      <KpiCard
        title="Cartera Total"
        value={formatCurrency(data.totalCartera)}
        trend={data.variacion?.cartera}
      />
      <KpiCard
        title="Cartera Vencida"
        value={formatCurrency(data.carteraVencida)}
        trend={data.variacion?.vencida}
        variant="destructive"
      />
      {/* ... más cards */}
    </div>
  );
}

// Skeleton separado para loading states
function CarteraKpiCardsSkeleton() {
  return (
    <div className="grid gap-4 md:grid-cols-4">
      {[...Array(4)].map((_, i) => (
        <Card key={i}>
          <CardHeader>
            <Skeleton className="h-4 w-24" />
          </CardHeader>
          <CardContent>
            <Skeleton className="h-8 w-32" />
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
```

### 3.3 Server Components vs Client Components

```tsx
// ✅ Server Component (default) - Para data fetching
// app/(dashboard)/cartera/page.tsx
import { CarteraKpiCards } from '@/components/features/cartera/kpi-cards';
import { AntiguedadChart } from '@/components/features/cartera/antiguedad-chart';

export default async function CarteraPage() {
  // Data fetching en el servidor
  const resumen = await getCarteraResumen();

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold">Cartera</h1>
      <CarteraKpiCards initialData={resumen} />
      <AntiguedadChart />
    </div>
  );
}

// ✅ Client Component - Para interactividad
// components/features/cartera/antiguedad-chart.tsx
'use client';

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';

export function AntiguedadChart() {
  const [period, setPeriod] = useState('month');
  // ... interactividad del cliente
}
```

### 3.4 Data Fetching con TanStack Query

```typescript
// lib/hooks/use-clientes.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getClientes, createCliente, updateCliente } from '@/lib/api/clientes';
import type { Cliente, CreateClienteRequest } from '@/lib/api/types';

// Query hook
export function useClientes(params?: { search?: string; page?: number }) {
  return useQuery({
    queryKey: ['clientes', params],
    queryFn: () => getClientes(params),
    staleTime: 5 * 60 * 1000, // 5 minutos
  });
}

export function useCliente(id: string) {
  return useQuery({
    queryKey: ['clientes', id],
    queryFn: () => getClienteById(id),
    enabled: !!id,
  });
}

// Mutation hook
export function useCreateCliente() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateClienteRequest) => createCliente(data),
    onSuccess: () => {
      // Invalidar cache de lista
      queryClient.invalidateQueries({ queryKey: ['clientes'] });
    },
  });
}
```

### 3.5 Forms con React Hook Form + Zod

```tsx
'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';

// Schema de validación
const clienteSchema = z.object({
  clave: z
    .string()
    .min(1, 'La clave es requerida')
    .max(50, 'Máximo 50 caracteres')
    .regex(/^[A-Za-z0-9\-]+$/, 'Solo letras, números y guiones'),
  nombre: z.string().min(1, 'El nombre es requerido').max(255),
  rfc: z
    .string()
    .regex(/^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$/, 'RFC inválido'),
  email: z.string().email('Email inválido').optional(),
});

type ClienteFormValues = z.infer<typeof clienteSchema>;

interface ClienteFormProps {
  defaultValues?: Partial<ClienteFormValues>;
  onSubmit: (data: ClienteFormValues) => Promise<void>;
  isLoading?: boolean;
}

export function ClienteForm({ defaultValues, onSubmit, isLoading }: ClienteFormProps) {
  const form = useForm<ClienteFormValues>({
    resolver: zodResolver(clienteSchema),
    defaultValues: {
      clave: '',
      nombre: '',
      rfc: '',
      email: '',
      ...defaultValues,
    },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="clave"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Clave</FormLabel>
              <FormControl>
                <Input placeholder="C001" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="nombre"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Nombre</FormLabel>
              <FormControl>
                <Input placeholder="Empresa S.A. de C.V." {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* ... más campos */}

        <Button type="submit" disabled={isLoading}>
          {isLoading ? 'Guardando...' : 'Guardar'}
        </Button>
      </form>
    </Form>
  );
}
```

---

## 4. Patrones FRICTIONLESS Obligatorios

### 4.1 Skeleton Loading (Nunca Spinner Genérico)

```tsx
// ✅ CORRECTO: Skeleton que refleja el contenido
function ClienteListSkeleton() {
  return (
    <div className="space-y-4">
      {[...Array(5)].map((_, i) => (
        <div key={i} className="flex items-center gap-4 p-4 border rounded-lg">
          <Skeleton className="h-10 w-10 rounded-full" />
          <div className="space-y-2 flex-1">
            <Skeleton className="h-4 w-48" />
            <Skeleton className="h-3 w-32" />
          </div>
          <Skeleton className="h-8 w-24" />
        </div>
      ))}
    </div>
  );
}

// ❌ INCORRECTO: Spinner genérico
function Loading() {
  return <Spinner />;
}
```

### 4.2 Command Menu (⌘K Navigation)

```tsx
// components/layout/command-menu.tsx
'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import {
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from '@/components/ui/command';
import {
  LayoutDashboard,
  Users,
  FileText,
  Settings,
  Mail,
} from 'lucide-react';

export function CommandMenu() {
  const [open, setOpen] = useState(false);
  const router = useRouter();

  useEffect(() => {
    const down = (e: KeyboardEvent) => {
      if (e.key === 'k' && (e.metaKey || e.ctrlKey)) {
        e.preventDefault();
        setOpen((open) => !open);
      }
    };

    document.addEventListener('keydown', down);
    return () => document.removeEventListener('keydown', down);
  }, []);

  const navigate = (path: string) => {
    router.push(path);
    setOpen(false);
  };

  return (
    <CommandDialog open={open} onOpenChange={setOpen}>
      <CommandInput placeholder="Buscar..." />
      <CommandList>
        <CommandEmpty>No se encontraron resultados.</CommandEmpty>
        <CommandGroup heading="Navegación">
          <CommandItem onSelect={() => navigate('/dashboard')}>
            <LayoutDashboard className="mr-2 h-4 w-4" />
            Dashboard
          </CommandItem>
          <CommandItem onSelect={() => navigate('/clientes')}>
            <Users className="mr-2 h-4 w-4" />
            Clientes
          </CommandItem>
          <CommandItem onSelect={() => navigate('/cartera')}>
            <FileText className="mr-2 h-4 w-4" />
            Cartera
          </CommandItem>
          <CommandItem onSelect={() => navigate('/cobranza')}>
            <Mail className="mr-2 h-4 w-4" />
            Cobranza
          </CommandItem>
        </CommandGroup>
        <CommandGroup heading="Acciones">
          <CommandItem onSelect={() => navigate('/settings')}>
            <Settings className="mr-2 h-4 w-4" />
            Configuración
          </CommandItem>
        </CommandGroup>
      </CommandList>
    </CommandDialog>
  );
}
```

### 4.3 Toast con Acciones

```tsx
// Uso de Sonner para toasts con acciones
import { toast } from 'sonner';

// ✅ CORRECTO: Toast con acción útil
function handleEmailSent(clienteId: string) {
  toast.success('Email enviado correctamente', {
    description: 'El cliente recibirá el recordatorio en breve.',
    action: {
      label: 'Ver historial',
      onClick: () => router.push(`/clientes/${clienteId}?tab=historial`),
    },
  });
}

// ✅ CORRECTO: Toast de error con retry
function handleSyncError() {
  toast.error('Error al sincronizar', {
    description: 'No se pudo conectar con el servidor.',
    action: {
      label: 'Reintentar',
      onClick: () => refetch(),
    },
  });
}
```

### 4.4 Empty States Informativos

```tsx
// components/shared/empty-state.tsx
import { LucideIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';

interface EmptyStateProps {
  icon: LucideIcon;
  title: string;
  description: string;
  action?: {
    label: string;
    onClick: () => void;
  };
}

export function EmptyState({ icon: Icon, title, description, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-12 text-center">
      <div className="rounded-full bg-muted p-4 mb-4">
        <Icon className="h-8 w-8 text-muted-foreground" />
      </div>
      <h3 className="text-lg font-semibold mb-2">{title}</h3>
      <p className="text-muted-foreground max-w-sm mb-4">{description}</p>
      {action && (
        <Button onClick={action.onClick}>
          {action.label}
        </Button>
      )}
    </div>
  );
}

// Uso
<EmptyState
  icon={Users}
  title="Sin clientes"
  description="Conecta tu ERP para sincronizar automáticamente tu cartera de clientes."
  action={{
    label: 'Conectar ERP',
    onClick: () => router.push('/settings/connectors'),
  }}
/>
```

---

## 5. Patrones Prohibidos

```tsx
// ❌ NUNCA usar "any"
const data: any = await fetch(...); // PROHIBIDO

// ❌ NUNCA usar dangerouslySetInnerHTML sin sanitizar
<div dangerouslySetInnerHTML={{ __html: userContent }} /> // XSS VULNERABLE

// ❌ NUNCA hardcodear URLs
const API_URL = 'http://localhost:5000'; // Usar env vars

// ❌ NUNCA console.log en producción
console.log('Debug:', data); // Usar logger condicional

// ❌ NUNCA fetch sin manejo de errores
const data = await fetch('/api/clientes').then(r => r.json()); // Sin error handling

// ❌ NUNCA ignorar loading states
{data && <ClienteList data={data} />} // Falta loading/error

// ❌ NUNCA usar spinners genéricos
{isLoading && <Spinner />} // Usar Skeleton específico
```

---

## 6. Accesibilidad (WCAG 2.1 AA)

### Checklist por Componente

| Elemento | Requisito |
|----------|-----------|
| Botones | `aria-label` si solo icono |
| Forms | Labels asociados, `aria-describedby` para errores |
| Modales | Focus trap, `aria-modal="true"` |
| Tablas | `<th scope="col">`, caption si necesario |
| Imágenes | `alt` descriptivo o `alt=""` si decorativa |
| Colores | Contraste mínimo 4.5:1 |
| Focus | Visible en todos los elementos interactivos |

```tsx
// ✅ CORRECTO: Botón accesible
<Button variant="ghost" size="icon" aria-label="Cerrar menú">
  <X className="h-4 w-4" />
</Button>

// ✅ CORRECTO: Form field accesible
<FormItem>
  <FormLabel htmlFor="email">Email</FormLabel>
  <FormControl>
    <Input
      id="email"
      type="email"
      aria-describedby="email-error"
      aria-invalid={!!errors.email}
    />
  </FormControl>
  <FormMessage id="email-error" />
</FormItem>
```

---

## 7. Testing

### Estructura de Tests

```tsx
// __tests__/components/cliente-form.test.tsx
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ClienteForm } from '@/components/features/clientes/cliente-form';

describe('ClienteForm', () => {
  it('renders all required fields', () => {
    render(<ClienteForm onSubmit={jest.fn()} />);

    expect(screen.getByLabelText(/clave/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/nombre/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/rfc/i)).toBeInTheDocument();
  });

  it('shows validation errors for invalid RFC', async () => {
    const user = userEvent.setup();
    render(<ClienteForm onSubmit={jest.fn()} />);

    await user.type(screen.getByLabelText(/rfc/i), 'invalid');
    await user.click(screen.getByRole('button', { name: /guardar/i }));

    await waitFor(() => {
      expect(screen.getByText(/rfc inválido/i)).toBeInTheDocument();
    });
  });

  it('calls onSubmit with valid data', async () => {
    const mockSubmit = jest.fn();
    const user = userEvent.setup();
    render(<ClienteForm onSubmit={mockSubmit} />);

    await user.type(screen.getByLabelText(/clave/i), 'C001');
    await user.type(screen.getByLabelText(/nombre/i), 'Test S.A.');
    await user.type(screen.getByLabelText(/rfc/i), 'XAXX010101000');
    await user.click(screen.getByRole('button', { name: /guardar/i }));

    await waitFor(() => {
      expect(mockSubmit).toHaveBeenCalledWith({
        clave: 'C001',
        nombre: 'Test S.A.',
        rfc: 'XAXX010101000',
        email: '',
      });
    });
  });
});
```

---

## 8. Comandos Útiles

```bash
# Desarrollo
npm run dev

# Build
npm run build

# Lint
npm run lint

# Tests
npm run test
npm run test:watch

# Storybook (si configurado)
npm run storybook

# Agregar componente shadcn
npx shadcn-ui@latest add button
npx shadcn-ui@latest add card
```

---

## 9. Triggers para Intervención de Este Agente

Este agente debe intervenir cuando:

1. **Crear/modificar páginas** - Cualquier cambio en `/app/`
2. **Crear componentes** - Nuevos componentes en `/components/`
3. **Cambios de UI/UX** - Estilos, layouts, interacciones
4. **Forms y validación** - Schemas Zod, formularios
5. **Estado del cliente** - Hooks, stores, queries
6. **Accesibilidad** - Auditorías, mejoras a11y
7. **Performance frontend** - Optimización, lazy loading

---

*Documento de contexto para agente frontend - Actualizar con cada patrón nuevo*
