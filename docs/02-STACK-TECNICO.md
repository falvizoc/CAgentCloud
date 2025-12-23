# Stack Técnico Detallado

> **Versión:** 1.0
> **Fecha:** 2025-12-22
> **Estado:** Definido

---

## 1. Resumen del Stack

> **📋 Documento Normativo:** [08-FRICTIONLESS-MANIFEST.md](./08-FRICTIONLESS-MANIFEST.md)
>
> El stack está optimizado para experiencia FRICTIONLESS: mínima configuración, máximo valor.

```
┌─────────────────────────────────────────────────────────────┐
│              STACK TECNOLÓGICO (FRICTIONLESS)                │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  FRONTEND          BACKEND           DATABASE    INFRA      │
│  ─────────         ───────           ────────    ─────      │
│  Next.js 14        .NET 8            PostgreSQL  Docker     │
│  React 18          Minimal API       16          Azure      │
│  TypeScript        EF Core                                  │
│  Tailwind          MediatR           Redis       GitHub     │
│  shadcn/ui         FluentValidation  (cache)     Actions    │
│  TanStack Query    Serilog                                  │
│  Zustand           Polly                                    │
│                                                              │
│  FRICTIONLESS TOOLS                                          │
│  ──────────────────                                          │
│  Clerk (Auth)      MS Rules Engine   next-intl  NextStep.js │
│  cmdk (⌘K)         OpenAI API        Sonner     react-email │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Frontend Stack

### 2.1 Framework: Next.js 14+

**Versión:** 14.x (App Router)

**Características Utilizadas:**
| Feature | Uso |
|---------|-----|
| App Router | Routing basado en archivos |
| Server Components | Reducir JS del cliente |
| Server Actions | Mutaciones sin API routes |
| Streaming | UI progresiva |
| Middleware | Auth, redirects |

**Configuración Base:**
```typescript
// next.config.js
const nextConfig = {
  experimental: {
    serverActions: true,
  },
  images: {
    domains: ['lh3.googleusercontent.com'], // Para avatars OAuth
  },
};
```

---

### 2.2 UI Library: shadcn/ui + Tailwind

**¿Por qué shadcn/ui?**
- Componentes copiados, no dependencia npm
- Altamente customizables
- Accesibilidad (Radix primitives)
- Diseño moderno

**Componentes Planeados:**
```
components/ui/
├── button.tsx
├── input.tsx
├── card.tsx
├── dialog.tsx
├── table.tsx
├── dropdown-menu.tsx
├── avatar.tsx
├── badge.tsx
├── tabs.tsx
├── toast.tsx
└── ...
```

**Tema Personalizado:**
```css
/* globals.css */
@layer base {
  :root {
    --background: 0 0% 100%;
    --foreground: 222.2 84% 4.9%;
    --primary: 221.2 83.2% 53.3%;    /* Azul corporativo */
    --primary-foreground: 210 40% 98%;
    --destructive: 0 84.2% 60.2%;    /* Rojo para alertas */
    /* ... más variables */
  }
  .dark {
    --background: 222.2 84% 4.9%;
    --foreground: 210 40% 98%;
    /* ... modo oscuro */
  }
}
```

---

### 2.3 State Management

**Server State: TanStack Query**
```typescript
// hooks/useCartera.ts
export function useCartera(empresaId: string) {
  return useQuery({
    queryKey: ['cartera', empresaId],
    queryFn: () => api.getCartera(empresaId),
    staleTime: 5 * 60 * 1000, // 5 minutos
  });
}
```

**Client State: Zustand**
```typescript
// stores/uiStore.ts
interface UIState {
  sidebarOpen: boolean;
  toggleSidebar: () => void;
}

export const useUIStore = create<UIState>((set) => ({
  sidebarOpen: true,
  toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
}));
```

---

### 2.4 Forms & Validation

**React Hook Form + Zod:**
```typescript
// schemas/auth.ts
import { z } from 'zod';

export const loginSchema = z.object({
  email: z.string().email('Email inválido'),
  password: z.string().min(8, 'Mínimo 8 caracteres'),
});

// components/LoginForm.tsx
const form = useForm<z.infer<typeof loginSchema>>({
  resolver: zodResolver(loginSchema),
});
```

---

### 2.5 API Client

**Estructura:**
```typescript
// lib/api/client.ts
const API_BASE = process.env.NEXT_PUBLIC_API_URL;

class ApiClient {
  private token: string | null = null;

  async request<T>(path: string, options?: RequestInit): Promise<T> {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...(this.token && { Authorization: `Bearer ${this.token}` }),
    };

    const res = await fetch(`${API_BASE}${path}`, {
      ...options,
      headers: { ...headers, ...options?.headers },
    });

    if (!res.ok) throw new ApiError(res.status, await res.text());
    return res.json();
  }

  setToken(token: string) {
    this.token = token;
  }
}

export const api = new ApiClient();
```

---

## 3. Backend Stack

### 3.1 Framework: .NET 8 Minimal API

**Estructura de Proyecto:**
```
backend/
├── CobranzaCloud.sln
├── src/
│   ├── CobranzaCloud.Api/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── Endpoints/
│   │   │   ├── AuthEndpoints.cs
│   │   │   ├── CarteraEndpoints.cs
│   │   │   └── SyncEndpoints.cs
│   │   ├── Middleware/
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   └── TenantMiddleware.cs
│   │   └── Extensions/
│   │
│   ├── CobranzaCloud.Core/
│   │   ├── Entities/
│   │   ├── Services/
│   │   ├── Interfaces/
│   │   └── ValueObjects/
│   │
│   └── CobranzaCloud.Data/
│       ├── AppDbContext.cs
│       ├── Configurations/
│       ├── Migrations/
│       └── Repositories/
│
└── tests/
    └── CobranzaCloud.Tests/
```

**Program.cs Base:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* config */ });

builder.Services.AddAuthorization();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Serilog
builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapAuthEndpoints();
app.MapCarteraEndpoints();
app.MapSyncEndpoints();

app.Run();
```

---

### 3.2 Database: Entity Framework Core

**DbContext:**
```csharp
public class AppDbContext : DbContext
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Connector> Connectors => Set<Connector>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Factura> Facturas => Set<Factura>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global query filter para multi-tenant
        modelBuilder.Entity<Cliente>()
            .HasQueryFilter(c => c.OrganizationId == _currentTenantId);
    }
}
```

**Migrations:**
```bash
# Crear migración
dotnet ef migrations add InitialCreate -p CobranzaCloud.Data -s CobranzaCloud.Api

# Aplicar
dotnet ef database update -s CobranzaCloud.Api
```

---

### 3.3 Authentication

**JWT Configuration:**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
```

**OAuth Providers:**
```csharp
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["OAuth:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"]!;
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["OAuth:Microsoft:ClientId"]!;
        options.ClientSecret = builder.Configuration["OAuth:Microsoft:ClientSecret"]!;
    });
```

---

### 3.4 Validation: FluentValidation

```csharp
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}

// Registro automático
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
```

---

### 3.5 CQRS con MediatR

**Query Example:**
```csharp
// Query
public record GetCarteraQuery(Guid OrganizationId) : IRequest<CarteraResult>;

// Handler
public class GetCarteraHandler : IRequestHandler<GetCarteraQuery, CarteraResult>
{
    private readonly AppDbContext _db;

    public GetCarteraHandler(AppDbContext db) => _db = db;

    public async Task<CarteraResult> Handle(GetCarteraQuery request, CancellationToken ct)
    {
        var facturas = await _db.Facturas
            .Where(f => f.Cliente.OrganizationId == request.OrganizationId)
            .GroupBy(f => f.RangoAntiguedad)
            .Select(g => new { Rango = g.Key, Total = g.Sum(f => f.Saldo) })
            .ToListAsync(ct);

        return new CarteraResult(facturas);
    }
}

// Endpoint
app.MapGet("/api/cartera", async (IMediator mediator) =>
{
    var result = await mediator.Send(new GetCarteraQuery(currentOrgId));
    return Results.Ok(result);
}).RequireAuthorization();
```

---

### 3.6 Resilience: Polly

```csharp
// Para llamadas a servicios externos
builder.Services.AddHttpClient<IEmailService, SendGridEmailService>()
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

---

## 4. Database Stack

### 4.1 PostgreSQL 16

**Features Utilizadas:**
- JSONB para datos flexibles
- Row Level Security
- Full-text search (futuro)
- Partitioning (si escala)

**Connection String:**
```
Host=localhost;Database=cobranzacloud;Username=postgres;Password=secret;Include Error Detail=true
```

---

### 4.2 Redis

**Usos:**
- Cache de sesiones
- Rate limiting
- Queue para emails
- Cache de datos frecuentes

**Connection:**
```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "cobranza:";
});
```

---

## 5. Infrastructure Stack

### 5.1 Docker

**docker-compose.yml (desarrollo):**
```yaml
version: '3.8'

services:
  frontend:
    build: ./src/frontend
    ports:
      - "3000:3000"
    environment:
      - NEXT_PUBLIC_API_URL=http://localhost:5000
    depends_on:
      - backend

  backend:
    build: ./src/backend/CobranzaCloud.Api
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Host=postgres;Database=cobranzacloud;Username=postgres;Password=postgres
      - ConnectionStrings__Redis=redis:6379
    depends_on:
      - postgres
      - redis

  postgres:
    image: postgres:16-alpine
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_DB=cobranzacloud
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

volumes:
  postgres_data:
  redis_data:
```

---

### 5.2 Azure Services

| Servicio | Propósito |
|----------|-----------|
| Container Apps | Hosting de contenedores |
| Azure Database for PostgreSQL | Base de datos managed |
| Azure Cache for Redis | Cache/Sessions |
| Azure Key Vault | Secrets |
| Azure Front Door | CDN + WAF |
| Azure Monitor | Logs y métricas |

---

### 5.3 CI/CD: GitHub Actions

**.github/workflows/ci.yml:**
```yaml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build

  frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      - run: npm ci
      - run: npm run lint
      - run: npm run build
      - run: npm test
```

---

## 6. Dependencias Principales

### Frontend (package.json)
```json
{
  "dependencies": {
    "next": "^14.0.0",
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "@tanstack/react-query": "^5.0.0",
    "zustand": "^4.4.0",
    "react-hook-form": "^7.48.0",
    "@hookform/resolvers": "^3.3.0",
    "zod": "^3.22.0",
    "tailwindcss": "^3.3.0",
    "class-variance-authority": "^0.7.0",
    "clsx": "^2.0.0",
    "lucide-react": "^0.290.0",
    "date-fns": "^2.30.0",
    "recharts": "^2.9.0"
  },
  "devDependencies": {
    "typescript": "^5.2.0",
    "@types/react": "^18.2.0",
    "eslint": "^8.52.0",
    "prettier": "^3.0.0"
  }
}
```

### Backend (.csproj)
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
  <PackageReference Include="MediatR" Version="12.1.0" />
  <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
  <PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
  <PackageReference Include="StackExchange.Redis" Version="2.7.0" />
</ItemGroup>
```

---

## 7. Versiones Mínimas

| Tecnología | Versión Mínima | Razón |
|------------|----------------|-------|
| .NET | 8.0 | LTS, Minimal API mejorada |
| Node.js | 20.x | LTS, performance |
| PostgreSQL | 16 | RLS mejorada, performance |
| Docker | 24.x | BuildKit por defecto |
| Next.js | 14.x | App Router estable |

---

*Documento de stack técnico - Base para setup inicial*
