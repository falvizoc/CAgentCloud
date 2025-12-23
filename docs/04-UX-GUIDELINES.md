# Guías de UX - Mejores Prácticas 2025

> **Versión:** 1.0
> **Fecha:** 2025-12-22
> **Estado:** Definición
> **Referencia:** Mejores prácticas actuales de UX a nivel mundial

---

## 0. Visión FRICTIONLESS (Principio Central)

> **📋 Documento Normativo:** [08-FRICTIONLESS-MANIFEST.md](./08-FRICTIONLESS-MANIFEST.md)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     UX = FRICTIONLESS EN ACCIÓN                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  "El mejor UX es aquel donde el usuario NO piensa en la interfaz."         │
│                                                                              │
│  Métricas de Éxito FRICTIONLESS                                              │
│  ═════════════════════════════                                               │
│  • Time to First Value: < 5 minutos                                          │
│  • Clicks para enviar email: ≤ 3                                             │
│  • Setup del conector: < 2 minutos                                           │
│  • Drop-off en onboarding: < 20%                                             │
│                                                                              │
│  Herramientas UX FRICTIONLESS                                                │
│  ════════════════════════════                                                │
│  • Clerk: Auth con 1 clic (OAuth pre-built)                                  │
│  • NextStep.js: Onboarding de 3 pasos (skip-able)                           │
│  • cmdk: Navegación rápida con ⌘K                                           │
│  • Sonner: Toasts con acciones sugeridas                                    │
│  • next-intl: Multi-idioma auto-detectado                                   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 1. Principios Fundamentales de UX

### 1.1 Los 10 Heurísticos de Nielsen (Actualizados)

| # | Principio | Aplicación en Cobranza Cloud |
|---|-----------|------------------------------|
| 1 | **Visibilidad del estado** | Indicador de sync, último update, estado de conector |
| 2 | **Coincidencia sistema-mundo real** | Lenguaje de cobranza: "vencido", "por cobrar", "antigüedad" |
| 3 | **Control y libertad del usuario** | Deshacer acciones, cancelar envíos programados |
| 4 | **Consistencia y estándares** | Mismos patrones en todo el dashboard |
| 5 | **Prevención de errores** | Confirmación antes de enviar correos masivos |
| 6 | **Reconocer antes que recordar** | Autocompletado, historial reciente |
| 7 | **Flexibilidad y eficiencia** | Atajos de teclado, bulk actions |
| 8 | **Diseño estético y minimalista** | Solo información relevante visible |
| 9 | **Ayuda a reconocer y recuperar errores** | Mensajes claros, acciones sugeridas |
| 10 | **Ayuda y documentación** | Tooltips, onboarding, help center |

---

### 1.2 Principios Modernos (2024-2025)

```
┌─────────────────────────────────────────────────────────────┐
│           TENDENCIAS UX 2025                                 │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  🎯 CLARITY OVER CLEVERNESS                                  │
│     La claridad supera a lo ingenioso                       │
│                                                              │
│  ⚡ PROGRESSIVE DISCLOSURE                                   │
│     Revelar complejidad gradualmente                        │
│                                                              │
│  🌙 DARK MODE AS DEFAULT OPTION                             │
│     Modo oscuro como opción, no imposición                  │
│                                                              │
│  📱 MOBILE-FIRST, DESKTOP-ENHANCED                          │
│     Diseñar para móvil, enriquecer en desktop              │
│                                                              │
│  ♿ ACCESSIBILITY-FIRST                                      │
│     Accesibilidad desde el diseño, no como parche          │
│                                                              │
│  🔔 RESPECTFUL NOTIFICATIONS                                 │
│     Notificaciones útiles, no intrusivas                    │
│                                                              │
│  ⏱️ PERCEIVED PERFORMANCE                                    │
│     Optimistic UI, skeletons, instant feedback             │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Patrones de UI/UX para SaaS B2B

### 2.1 Onboarding Progresivo

```
┌─────────────────────────────────────────────────────────────┐
│                  FLUJO DE ONBOARDING                         │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  PASO 1: Bienvenida (3 segundos)                            │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  👋 ¡Bienvenido a Cobranza Cloud!                     │ │
│  │                                                        │ │
│  │  Vamos a configurar tu cuenta en 3 pasos simples.    │ │
│  │                                                        │ │
│  │  [Comenzar →]                                         │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                              │
│  PASO 2: Información básica                                  │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  Cuéntanos sobre tu empresa                           │ │
│  │                                                        │ │
│  │  Nombre de empresa: [__________________]              │ │
│  │  Tu rol:           [Gerente de cobranza ▼]           │ │
│  │                                                        │ │
│  │  [← Atrás]                        [Continuar →]      │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                              │
│  PASO 3: Conectar datos                                      │
│  ┌───────────────────────────────────────────────────────┐ │
│  │  Conecta tu sistema ASPEL                             │ │
│  │                                                        │ │
│  │  ℹ️ Necesitarás instalar el conector en tu servidor   │ │
│  │                                                        │ │
│  │  [Descargar conector]  [Lo haré después]              │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

**Mejores Prácticas de Onboarding:**
- Máximo 3-5 pasos
- Permitir saltar y volver después
- Mostrar progreso visualmente
- Celebrar completación
- Ofrecer tour guiado opcional

---

### 2.2 Dashboard Pattern

```
┌─────────────────────────────────────────────────────────────────────────┐
│  🏢 Mi Empresa                                    🔔  👤 Juan García ▼  │
├──────────────────┬──────────────────────────────────────────────────────┤
│                  │                                                       │
│  📊 Dashboard    │   RESUMEN DE CARTERA              Última sync: 5 min │
│  ────────────────│   ═══════════════════════════════════════════════════│
│  💰 Cartera      │                                                       │
│  👥 Clientes     │   ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐   │
│  📧 Cobranza     │   │ TOTAL   │ │ VIGENTE │ │ 1-30 d  │ │ 31-60 d │   │
│  📈 Reportes     │   │ $1.37M  │ │ $420K   │ │ $380K   │ │ $290K   │   │
│  ────────────────│   │ ↑ 5.2%  │ │ ↓ 2.1%  │ │ ↑ 8.4%  │ │ ↑ 3.2%  │   │
│  ⚙️ Config       │   └─────────┘ └─────────┘ └─────────┘ └─────────┘   │
│  🔌 Conectores   │                                                       │
│                  │   ┌──────────────────────────────────────────────┐   │
│                  │   │                                               │   │
│                  │   │    📊 GRÁFICO DE ANTIGÜEDAD                  │   │
│                  │   │                                               │   │
│                  │   │    [Gráfico de barras/dona]                  │   │
│                  │   │                                               │   │
│                  │   └──────────────────────────────────────────────┘   │
│                  │                                                       │
│                  │   ACCIONES PENDIENTES                                │
│                  │   ┌──────────────────────────────────────────────┐   │
│                  │   │ ⚠️ 12 facturas vencen hoy                    │   │
│                  │   │ 📧 5 recordatorios programados                │   │
│                  │   │ 🔄 Sync pendiente: Empresa 02                 │   │
│                  │   └──────────────────────────────────────────────┘   │
│                  │                                                       │
└──────────────────┴──────────────────────────────────────────────────────┘
```

**Mejores Prácticas de Dashboard:**
- KPIs más importantes arriba y a la izquierda
- Números grandes, legibles
- Indicadores de tendencia (↑↓)
- Acciones claras y priorizadas
- Datos frescos con timestamp

---

### 2.3 Empty States

```
┌─────────────────────────────────────────────────────────────┐
│                                                              │
│                         📊                                   │
│                                                              │
│              No hay datos de cartera aún                    │
│                                                              │
│    Conecta tu sistema ASPEL para empezar a ver              │
│    el estado de tu cartera en tiempo real.                  │
│                                                              │
│              [🔌 Conectar ASPEL]                            │
│                                                              │
│    ────────────────────────────────────────                 │
│    ¿Necesitas ayuda? Ver guía de instalación               │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

**Mejores Prácticas de Empty States:**
- Ilustración o icono amigable
- Explica POR QUÉ está vacío
- CTA claro para resolver
- Enlace a ayuda/documentación

---

### 2.4 Tablas de Datos

```
┌─────────────────────────────────────────────────────────────────────────┐
│  CLIENTES CON SALDO PENDIENTE                                           │
│  ───────────────────────────────────────────────────────────────────────│
│  🔍 Buscar cliente...          Filtrar: [Todos ▼]  Ordenar: [Saldo ▼]  │
├─────────────────────────────────────────────────────────────────────────┤
│  □  CLIENTE              SALDO        VENCIDO      DÍAS    ACCIONES    │
│  ─────────────────────────────────────────────────────────────────────  │
│  □  Juan Pérez           $45,230      $12,500      15      [···]       │
│  □  María García         $38,100      $38,100      45      [···]       │
│  ☑  Carlos López         $22,800      $0           -       [···]       │
│  □  Ana Martínez         $18,500      $8,200       30      [···]       │
│  □  Roberto Sánchez      $15,200      $15,200      62      [···]       │
├─────────────────────────────────────────────────────────────────────────┤
│  Mostrando 1-5 de 26                              [←] [1] [2] [3] [→]  │
│  ─────────────────────────────────────────────────────────────────────  │
│  Con seleccionados: [📧 Enviar recordatorio] [📋 Exportar]             │
└─────────────────────────────────────────────────────────────────────────┘
```

**Mejores Prácticas de Tablas:**
- Búsqueda y filtros prominentes
- Ordenamiento por columnas
- Selección múltiple con bulk actions
- Acciones por fila en menú contextual
- Paginación clara
- Responsive: cards en móvil

---

### 2.5 Formularios

```
┌─────────────────────────────────────────────────────────────┐
│  CONFIGURAR RECORDATORIO                                     │
│  ───────────────────────────────────────────────────────────│
│                                                              │
│  Plantilla de correo                                        │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ Recordatorio 30 días                              [▼] │ │
│  └───────────────────────────────────────────────────────┘ │
│  ℹ️ Se usarán las variables: {cliente}, {monto}, {dias}     │
│                                                              │
│  Enviar cuando                                               │
│  ○ La factura lleve [30] días vencida                       │
│  ○ En una fecha específica: [___________]                   │
│  ● Ahora (envío inmediato)                                  │
│                                                              │
│  Destinatarios                                               │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ contacto@empresa.com                              [×] │ │
│  │ cobranzas@empresa.com                             [×] │ │
│  └───────────────────────────────────────────────────────┘ │
│  [+ Agregar destinatario]                                   │
│                                                              │
│  ⚠️ Esta acción enviará correo a 3 contactos                │
│                                                              │
│  [Cancelar]                        [Vista previa] [Enviar] │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

**Mejores Prácticas de Formularios:**
- Labels siempre visibles (arriba del campo)
- Placeholder como ejemplo, no como label
- Validación inline en tiempo real
- Helper text para campos complejos
- Agrupar campos relacionados
- Botón primario a la derecha
- Confirmación para acciones destructivas

---

## 3. Microinteracciones

### 3.1 Feedback Visual

```
┌─────────────────────────────────────────────────────────────┐
│                  TIPOS DE FEEDBACK                           │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  LOADING STATES                                              │
│  ├── Skeleton screens (preferido sobre spinners)            │
│  ├── Progress bars para operaciones conocidas               │
│  └── Optimistic UI para acciones rápidas                    │
│                                                              │
│  SUCCESS                                                     │
│  ├── Toast verde efímero (3 segundos)                       │
│  ├── Checkmark animado                                       │
│  └── Transición suave al nuevo estado                       │
│                                                              │
│  ERROR                                                       │
│  ├── Toast rojo persistente hasta dismiss                   │
│  ├── Mensaje claro de qué falló                             │
│  └── Acción sugerida para resolver                          │
│                                                              │
│  WARNING                                                     │
│  ├── Inline para validación de formularios                  │
│  └── Modal para acciones destructivas                       │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 3.2 Skeleton Loading

```typescript
// components/ClienteSkeleton.tsx
export function ClienteSkeleton() {
  return (
    <div className="animate-pulse space-y-4">
      <div className="flex items-center space-x-4">
        <div className="h-12 w-12 rounded-full bg-gray-200" />
        <div className="space-y-2">
          <div className="h-4 w-48 rounded bg-gray-200" />
          <div className="h-3 w-32 rounded bg-gray-200" />
        </div>
      </div>
      <div className="h-24 rounded bg-gray-200" />
    </div>
  );
}
```

### 3.3 Optimistic UI

```typescript
// hooks/useUpdateCliente.ts
export function useUpdateCliente() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateCliente,
    onMutate: async (newData) => {
      // Cancelar queries pendientes
      await queryClient.cancelQueries({ queryKey: ['cliente', newData.id] });

      // Guardar estado anterior
      const previousData = queryClient.getQueryData(['cliente', newData.id]);

      // Actualizar optimistamente
      queryClient.setQueryData(['cliente', newData.id], newData);

      return { previousData };
    },
    onError: (err, newData, context) => {
      // Rollback en caso de error
      queryClient.setQueryData(['cliente', newData.id], context?.previousData);
      toast.error('Error al actualizar. Se revirtieron los cambios.');
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['clientes'] });
    },
  });
}
```

---

## 4. Responsive Design

### 4.1 Breakpoints

```css
/* Sistema de breakpoints (Tailwind defaults) */
sm: 640px   /* Móviles grandes */
md: 768px   /* Tablets */
lg: 1024px  /* Laptops */
xl: 1280px  /* Desktops */
2xl: 1536px /* Pantallas grandes */
```

### 4.2 Adaptaciones por Dispositivo

| Componente | Mobile | Tablet | Desktop |
|------------|--------|--------|---------|
| Sidebar | Drawer (hidden) | Mini (icons) | Expandido |
| Tabla | Cards verticales | Tabla scroll | Tabla completa |
| Dashboard | KPIs stacked | Grid 2 cols | Grid 4 cols |
| Formularios | Full width | 70% width | 50% width max |
| Modales | Full screen | Centrado | Centrado |

### 4.3 Touch Targets

```css
/* Mínimo 44x44px para touch (Apple HIG) */
.btn {
  min-height: 44px;
  min-width: 44px;
  padding: 12px 16px;
}

/* Espaciado entre elementos tocables */
.action-list > * + * {
  margin-top: 8px; /* Mínimo 8px entre touch targets */
}
```

---

## 5. Accesibilidad (a11y)

### 5.1 Checklist WCAG 2.2 AA

| Criterio | Implementación |
|----------|----------------|
| **Perceptible** | |
| Contraste texto | Mínimo 4.5:1 (3:1 para texto grande) |
| Alt text | Todas las imágenes informativas |
| Captions | Videos con subtítulos |
| **Operable** | |
| Keyboard nav | Todo accesible con Tab |
| Focus visible | Outline claro en :focus-visible |
| No tiempo límite | O advertencia + extensión |
| **Comprensible** | |
| Idioma | `lang="es"` en html |
| Labels | Todos los inputs con label |
| Errores | Identificados y descritos |
| **Robusto** | |
| Semántico | HTML semántico, ARIA cuando necesario |
| Compatible | Funciona con screen readers |

### 5.2 Componentes Accesibles

```tsx
// Ejemplo: Botón accesible
<Button
  aria-label="Enviar recordatorio a cliente Juan Pérez"
  aria-describedby="envio-info"
  disabled={isLoading}
>
  {isLoading ? (
    <>
      <Spinner aria-hidden="true" />
      <span className="sr-only">Enviando...</span>
    </>
  ) : (
    'Enviar recordatorio'
  )}
</Button>
<p id="envio-info" className="sr-only">
  Se enviará un correo electrónico al cliente
</p>
```

### 5.3 Focus Management

```tsx
// Mover foco a nuevo contenido (modales, toasts)
const modalRef = useRef<HTMLDivElement>(null);

useEffect(() => {
  if (isOpen) {
    modalRef.current?.focus();
  }
}, [isOpen]);

// Trap focus dentro de modal
<Dialog
  onOpenChange={setOpen}
  modal={true}  // shadcn/ui maneja focus trap
>
  <DialogContent ref={modalRef} tabIndex={-1}>
    {/* contenido */}
  </DialogContent>
</Dialog>
```

---

## 6. Sistema de Diseño

### 6.1 Paleta de Colores

```css
:root {
  /* Primarios - Azul corporativo */
  --primary-50: #eff6ff;
  --primary-100: #dbeafe;
  --primary-500: #3b82f6;  /* Main */
  --primary-600: #2563eb;  /* Hover */
  --primary-700: #1d4ed8;  /* Active */

  /* Semánticos */
  --success: #22c55e;      /* Verde - pagado, éxito */
  --warning: #f59e0b;      /* Amarillo - próximo a vencer */
  --error: #ef4444;        /* Rojo - vencido, error */
  --info: #3b82f6;         /* Azul - información */

  /* Neutrales */
  --gray-50: #f9fafb;
  --gray-100: #f3f4f6;
  --gray-500: #6b7280;
  --gray-900: #111827;
}
```

### 6.2 Tipografía

```css
:root {
  /* Font family */
  --font-sans: 'Inter', system-ui, sans-serif;
  --font-mono: 'JetBrains Mono', monospace;

  /* Font sizes */
  --text-xs: 0.75rem;    /* 12px */
  --text-sm: 0.875rem;   /* 14px */
  --text-base: 1rem;     /* 16px */
  --text-lg: 1.125rem;   /* 18px */
  --text-xl: 1.25rem;    /* 20px */
  --text-2xl: 1.5rem;    /* 24px */
  --text-3xl: 1.875rem;  /* 30px */
  --text-4xl: 2.25rem;   /* 36px */
}
```

### 6.3 Espaciado

```css
/* Escala de 4px */
--space-1: 0.25rem;  /* 4px */
--space-2: 0.5rem;   /* 8px */
--space-3: 0.75rem;  /* 12px */
--space-4: 1rem;     /* 16px */
--space-5: 1.25rem;  /* 20px */
--space-6: 1.5rem;   /* 24px */
--space-8: 2rem;     /* 32px */
--space-10: 2.5rem;  /* 40px */
--space-12: 3rem;    /* 48px */
```

---

## 7. Patrones de Navegación

### 7.1 Estructura de Navegación

```
NAVEGACIÓN PRINCIPAL (Sidebar)
├── Dashboard (Home)
├── Cartera
│   ├── Resumen
│   ├── Por antigüedad
│   └── Facturas
├── Clientes
│   ├── Listado
│   └── [Detalle cliente]
├── Cobranza
│   ├── Recordatorios
│   ├── Plantillas
│   └── Historial
└── Configuración
    ├── Mi cuenta
    ├── Organización
    ├── Usuarios
    └── Conectores
```

### 7.2 Breadcrumbs

```
Dashboard > Clientes > Juan Pérez > Facturas
```

### 7.3 Atajos de Teclado (Power Users)

| Atajo | Acción |
|-------|--------|
| `G` + `D` | Ir a Dashboard |
| `G` + `C` | Ir a Clientes |
| `G` + `K` | Ir a Cartera |
| `/` | Búsqueda global |
| `?` | Mostrar atajos |
| `Esc` | Cerrar modal/drawer |

---

## 8. Performance Percibida

### 8.1 Métricas Objetivo

| Métrica | Objetivo | Medición |
|---------|----------|----------|
| FCP | < 1.8s | First Contentful Paint |
| LCP | < 2.5s | Largest Contentful Paint |
| FID | < 100ms | First Input Delay |
| CLS | < 0.1 | Cumulative Layout Shift |
| TTI | < 3.8s | Time to Interactive |

### 8.2 Técnicas

```typescript
// 1. Prefetch de rutas probables
<Link href="/clientes" prefetch={true}>
  Clientes
</Link>

// 2. Lazy loading de componentes pesados
const ReportChart = dynamic(() => import('./ReportChart'), {
  loading: () => <ChartSkeleton />,
  ssr: false,
});

// 3. Debounce en búsqueda
const debouncedSearch = useDebouncedCallback(
  (term: string) => setSearchTerm(term),
  300
);

// 4. Virtualización de listas largas
<VirtualizedList
  items={clientes}
  itemHeight={72}
  renderItem={(cliente) => <ClienteRow cliente={cliente} />}
/>
```

---

## 9. Internacionalización (i18n)

### 9.1 Consideraciones para Futuro

```typescript
// Preparar para i18n aunque MVP sea solo español
const t = {
  dashboard: {
    title: 'Panel de Control',
    totalCartera: 'Cartera Total',
    vencido: 'Vencido',
    porVencer: 'Por Vencer',
  },
  clientes: {
    title: 'Clientes',
    sinSaldo: 'Sin saldo pendiente',
    // ...
  },
};

// Uso
<h1>{t.dashboard.title}</h1>
```

### 9.2 Formatos de Fecha/Número

```typescript
// Siempre usar formatters localizados
const formatCurrency = (amount: number) =>
  new Intl.NumberFormat('es-MX', {
    style: 'currency',
    currency: 'MXN',
  }).format(amount);

const formatDate = (date: Date) =>
  new Intl.DateTimeFormat('es-MX', {
    dateStyle: 'medium',
  }).format(date);

// Resultado:
// formatCurrency(1234.56) → "$1,234.56"
// formatDate(new Date()) → "22 dic 2025"
```

---

## 10. Testing de UX

### 10.1 Tipos de Pruebas

| Tipo | Cuándo | Herramienta |
|------|--------|-------------|
| Usability testing | Pre-launch, cambios mayores | Usuarios reales |
| A/B testing | Optimización continua | PostHog, Amplitude |
| Heatmaps | Análisis de uso | Hotjar, PostHog |
| Session replay | Debug de problemas | LogRocket, Sentry |

### 10.2 Métricas UX

- **Task success rate**: % que completa tarea
- **Time on task**: Tiempo promedio por tarea
- **Error rate**: Errores por sesión
- **SUS Score**: System Usability Scale (encuesta)
- **NPS**: Net Promoter Score

---

## 11. Checklist Pre-Launch

### UI/UX
- [ ] Todos los estados: loading, empty, error, success
- [ ] Responsive en mobile, tablet, desktop
- [ ] Dark mode funcional (si aplica)
- [ ] Animaciones suaves (no jarring)
- [ ] Copy revisado por humano (no lorem ipsum)

### Accesibilidad
- [ ] Navegación por teclado completa
- [ ] Contraste verificado
- [ ] Screen reader testing
- [ ] Focus visible
- [ ] Alt text en imágenes

### Performance
- [ ] Lighthouse score > 90
- [ ] No layout shifts visibles
- [ ] Lazy loading implementado
- [ ] Images optimizadas

---

*Guías de UX - Revisar cada release mayor*
