# CLAUDE-BACKEND.md - Contexto del Agente Backend

> **Área:** Backend API (.NET 8)
> **Responsabilidad:** API REST, Autenticación, Base de Datos, Lógica de Negocio
> **Última actualización:** 2025-12-23

---

## 1. Contexto del Proyecto

Este es el backend de **CobranzaCloud**, una plataforma SaaS de gestión de cobranza.

### Stack Tecnológico
| Componente | Tecnología | Versión |
|------------|------------|---------|
| Framework | .NET | 8.0 LTS |
| API Style | Minimal API | - |
| ORM | Entity Framework Core | 8.x |
| Database | PostgreSQL | 16 |
| Cache | Redis | 7.x |
| Auth | JWT + Clerk Integration | - |
| Validation | FluentValidation | 11.x |
| CQRS | MediatR | 12.x |
| Logging | Serilog | 8.x |
| Resilience | Polly | 8.x |

### Documentos de Referencia
- [CLAUDE.md](../../CLAUDE.md) - Contexto general del proyecto
- [docs/01-ARQUITECTURA.md](../../docs/01-ARQUITECTURA.md) - Arquitectura del sistema
- [docs/03-SEGURIDAD.md](../../docs/03-SEGURIDAD.md) - Políticas OWASP 2025
- [docs/05-API-SPEC.md](../../docs/05-API-SPEC.md) - Especificación de API
- [docs/contracts/api-types.ts](../../docs/contracts/api-types.ts) - Contratos de tipos

---

## 2. Estructura del Proyecto

```
src/backend/
├── CobranzaCloud.sln
├── src/
│   ├── CobranzaCloud.Api/           # Entry point - Minimal API
│   │   ├── Program.cs               # Configuración y startup
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Endpoints/               # Endpoints agrupados por dominio
│   │   │   ├── AuthEndpoints.cs
│   │   │   ├── CarteraEndpoints.cs
│   │   │   ├── ClientesEndpoints.cs
│   │   │   ├── CobranzaEndpoints.cs
│   │   │   ├── ConnectorsEndpoints.cs
│   │   │   └── SyncEndpoints.cs
│   │   ├── Middleware/
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   ├── TenantMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   └── Extensions/
│   │       ├── ServiceCollectionExtensions.cs
│   │       └── EndpointRouteBuilderExtensions.cs
│   │
│   ├── CobranzaCloud.Core/          # Dominio y lógica de negocio
│   │   ├── Entities/                # Entidades de dominio
│   │   │   ├── Organization.cs
│   │   │   ├── User.cs
│   │   │   ├── Cliente.cs
│   │   │   ├── Factura.cs
│   │   │   └── Connector.cs
│   │   ├── Services/                # Servicios de dominio
│   │   ├── Interfaces/              # Contratos
│   │   ├── ValueObjects/            # Value objects
│   │   └── Exceptions/              # Excepciones de dominio
│   │
│   ├── CobranzaCloud.Application/   # Casos de uso (CQRS)
│   │   ├── Commands/
│   │   │   ├── Auth/
│   │   │   ├── Clientes/
│   │   │   └── Cobranza/
│   │   ├── Queries/
│   │   │   ├── Cartera/
│   │   │   └── Clientes/
│   │   ├── Validators/              # FluentValidation
│   │   └── Behaviors/               # MediatR pipelines
│   │
│   └── CobranzaCloud.Infrastructure/ # Implementaciones
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   ├── Configurations/      # EF Configurations
│       │   └── Migrations/
│       ├── Repositories/
│       ├── Services/                # Implementaciones de servicios
│       └── External/                # Integraciones externas
│
└── tests/
    ├── CobranzaCloud.UnitTests/
    ├── CobranzaCloud.IntegrationTests/
    └── CobranzaCloud.E2ETests/
```

---

## 3. Convenciones de Código

### 3.1 Naming Conventions

```csharp
// Clases: PascalCase
public class ClienteService { }

// Interfaces: IPascalCase
public interface IClienteRepository { }

// Métodos: PascalCase
public async Task<Result<Cliente>> GetByIdAsync(Guid id) { }

// Variables locales: camelCase
var clienteId = request.ClienteId;

// Constantes: PascalCase
public const string DefaultCurrency = "MXN";

// Campos privados: _camelCase
private readonly ILogger<ClienteService> _logger;
```

### 3.2 Estructura de Endpoint (Minimal API)

```csharp
// ✅ CORRECTO: Endpoint con todas las convenciones
public static class ClientesEndpoints
{
    public static void MapClientesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clientes")
            .WithTags("Clientes")
            .RequireAuthorization();

        group.MapGet("/", GetClientes)
            .WithName("GetClientes")
            .WithDescription("Obtiene lista paginada de clientes")
            .Produces<PagedResult<ClienteResponse>>(200)
            .Produces<ProblemDetails>(401);

        group.MapGet("/{id:guid}", GetClienteById)
            .WithName("GetClienteById")
            .Produces<ClienteDetailResponse>(200)
            .Produces<ProblemDetails>(404);

        group.MapPost("/", CreateCliente)
            .WithName("CreateCliente")
            .Produces<ClienteResponse>(201)
            .Produces<ValidationProblemDetails>(400);
    }

    private static async Task<IResult> GetClientes(
        [AsParameters] GetClientesQuery query,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(query, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetClienteById(
        Guid id,
        IMediator mediator,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var orgId = user.GetOrganizationId();
        var result = await mediator.Send(new GetClienteByIdQuery(id, orgId), ct);

        return result.Match(
            success => Results.Ok(success),
            notFound => Results.NotFound(new ProblemDetails
            {
                Title = "Cliente no encontrado",
                Status = 404
            })
        );
    }
}
```

### 3.3 Patrón CQRS con MediatR

```csharp
// Query
public record GetCarteraResumenQuery(Guid OrganizationId) : IRequest<CarteraResumenResponse>;

// Query Handler
public class GetCarteraResumenHandler : IRequestHandler<GetCarteraResumenQuery, CarteraResumenResponse>
{
    private readonly AppDbContext _db;
    private readonly ILogger<GetCarteraResumenHandler> _logger;

    public GetCarteraResumenHandler(AppDbContext db, ILogger<GetCarteraResumenHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<CarteraResumenResponse> Handle(
        GetCarteraResumenQuery request,
        CancellationToken ct)
    {
        var facturas = await _db.Facturas
            .Where(f => f.Cliente.OrganizationId == request.OrganizationId)
            .GroupBy(f => f.RangoAntiguedad)
            .Select(g => new {
                Rango = g.Key,
                Total = g.Sum(f => f.Saldo),
                Count = g.Count()
            })
            .ToListAsync(ct);

        return new CarteraResumenResponse
        {
            TotalCartera = facturas.Sum(f => f.Total),
            // ... mapear resto
        };
    }
}

// Command
public record CreateClienteCommand(
    string Clave,
    string Nombre,
    string Rfc,
    Guid OrganizationId
) : IRequest<Result<ClienteResponse>>;

// Command Handler con Result pattern
public class CreateClienteHandler : IRequestHandler<CreateClienteCommand, Result<ClienteResponse>>
{
    public async Task<Result<ClienteResponse>> Handle(
        CreateClienteCommand request,
        CancellationToken ct)
    {
        // Validar duplicados
        var exists = await _db.Clientes
            .AnyAsync(c => c.Clave == request.Clave && c.OrganizationId == request.OrganizationId, ct);

        if (exists)
            return Result<ClienteResponse>.Failure("Cliente con esa clave ya existe");

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Clave = request.Clave,
            Nombre = request.Nombre,
            Rfc = request.Rfc,
            OrganizationId = request.OrganizationId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync(ct);

        return Result<ClienteResponse>.Success(cliente.ToResponse());
    }
}
```

### 3.4 Validación con FluentValidation

```csharp
public class CreateClienteCommandValidator : AbstractValidator<CreateClienteCommand>
{
    public CreateClienteCommandValidator()
    {
        RuleFor(x => x.Clave)
            .NotEmpty().WithMessage("La clave es requerida")
            .MaximumLength(50).WithMessage("La clave no puede exceder 50 caracteres")
            .Matches(@"^[A-Za-z0-9\-]+$").WithMessage("La clave solo puede contener letras, números y guiones");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(255);

        RuleFor(x => x.Rfc)
            .NotEmpty()
            .Matches(@"^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$")
            .WithMessage("RFC inválido");
    }
}
```

### 3.5 Entity Framework Configuration

```csharp
public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Clave)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Nombre)
            .HasMaxLength(255)
            .IsRequired();

        // Índice único por organización
        builder.HasIndex(c => new { c.OrganizationId, c.Clave })
            .IsUnique();

        // Relaciones
        builder.HasOne(c => c.Organization)
            .WithMany(o => o.Clientes)
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filter para multi-tenant
        builder.HasQueryFilter(c => c.OrganizationId == EF.Property<Guid>(c, "CurrentOrgId"));
    }
}
```

---

## 4. Patrones Obligatorios

### 4.1 Result Pattern para Operaciones

```csharp
// Definición
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Error!);
    }
}

// Uso en endpoint
return result.Match(
    success => Results.Created($"/api/clientes/{success.Id}", success),
    error => Results.BadRequest(new ProblemDetails { Detail = error })
);
```

### 4.2 Multi-Tenant con ClaimsPrincipal

```csharp
// Extension method para obtener OrganizationId
public static class ClaimsPrincipalExtensions
{
    public static Guid GetOrganizationId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("org_id")
            ?? throw new UnauthorizedAccessException("Organization claim not found");
        return Guid.Parse(claim.Value);
    }

    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User claim not found");
        return Guid.Parse(claim.Value);
    }
}

// Uso en handler - SIEMPRE validar org_id
public async Task<Result<Cliente>> Handle(GetClienteQuery request, CancellationToken ct)
{
    var cliente = await _db.Clientes
        .Where(c => c.Id == request.Id && c.OrganizationId == request.OrganizationId) // ✅
        .FirstOrDefaultAsync(ct);

    // ❌ NUNCA hacer esto:
    // var cliente = await _db.Clientes.FindAsync(request.Id);
}
```

### 4.3 Logging Estructurado

```csharp
// ✅ CORRECTO: Log estructurado sin PII
_logger.LogInformation(
    "Sync completed for connector {ConnectorId}, processed {RecordCount} records in {ElapsedMs}ms",
    connectorId,
    recordCount,
    stopwatch.ElapsedMilliseconds);

// ❌ INCORRECTO: Loguear datos sensibles
_logger.LogInformation("User {Email} with password {Password} logged in", email, password);

// ✅ CORRECTO: Error con contexto
_logger.LogError(
    ex,
    "Failed to process sync for connector {ConnectorId}. Error: {ErrorType}",
    connectorId,
    ex.GetType().Name);
```

---

## 5. Patrones Prohibidos

```csharp
// ❌ NUNCA usar Controllers MVC
[ApiController]
public class ClientesController : ControllerBase { } // PROHIBIDO

// ❌ NUNCA concatenar SQL
var query = $"SELECT * FROM clientes WHERE clave = '{clave}'"; // SQL INJECTION

// ❌ NUNCA exponer IDs sin validar ownership
var cliente = await _db.Clientes.FindAsync(id); // Falta validar OrganizationId

// ❌ NUNCA loguear datos sensibles
_logger.LogInformation("Password: {Password}", password);

// ❌ NUNCA retornar excepciones crudas
catch (Exception ex)
{
    return Results.Problem(ex.ToString()); // Expone stack trace
}

// ❌ NUNCA hardcodear secrets
var connectionString = "Host=localhost;Password=secret123"; // Usar IConfiguration
```

---

## 6. Seguridad (OWASP 2025)

### Checklist por Feature

| Al crear endpoint | Verificar |
|-------------------|-----------|
| Autenticación | `.RequireAuthorization()` |
| Multi-tenant | Validar `OrganizationId` en query |
| Validación | FluentValidation en request |
| SQL Injection | Solo EF Core, nunca raw SQL |
| Logging | No loguear PII |
| Rate Limiting | Aplicar según `docs/05-API-SPEC.md` |

### Ejemplo de Endpoint Seguro

```csharp
group.MapPost("/", async (
    CreateClienteRequest request,
    IValidator<CreateClienteRequest> validator,  // Validación
    IMediator mediator,
    ClaimsPrincipal user,                         // Auth
    CancellationToken ct) =>
{
    // 1. Validar request
    var validation = await validator.ValidateAsync(request, ct);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    // 2. Obtener org_id del token (no del request)
    var orgId = user.GetOrganizationId();

    // 3. Ejecutar comando
    var result = await mediator.Send(
        new CreateClienteCommand(request.Clave, request.Nombre, request.Rfc, orgId),
        ct);

    // 4. Retornar con Result pattern
    return result.Match(
        success => Results.Created($"/api/clientes/{success.Id}", success),
        error => Results.BadRequest(new ProblemDetails { Detail = error })
    );
})
.RequireAuthorization()
.WithName("CreateCliente")
.Produces<ClienteResponse>(201)
.Produces<ValidationProblemDetails>(400)
.Produces<ProblemDetails>(401);
```

---

## 7. Testing

### Estructura de Tests

```csharp
// Unit Test
public class CreateClienteHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var db = CreateInMemoryDb();
        var handler = new CreateClienteHandler(db, NullLogger<CreateClienteHandler>.Instance);
        var command = new CreateClienteCommand("C001", "Test Cliente", "XAXX010101000", Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("C001", result.Value!.Clave);
    }

    [Fact]
    public async Task Handle_DuplicateClave_ReturnsFailure()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var db = CreateInMemoryDb();
        db.Clientes.Add(new Cliente { Clave = "C001", OrganizationId = orgId });
        await db.SaveChangesAsync();

        var handler = new CreateClienteHandler(db, NullLogger<CreateClienteHandler>.Instance);
        var command = new CreateClienteCommand("C001", "Otro", "XAXX010101000", orgId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("ya existe", result.Error);
    }
}
```

### Integration Test con TestContainers

```csharp
public class ClientesEndpointsTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgres = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();
        await _postgres.StartAsync();

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Reemplazar connection string
                    services.RemoveAll<DbContextOptions<AppDbContext>>();
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(_postgres.GetConnectionString()));
                });
            });

        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetClientes_Authenticated_ReturnsOk()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GetTestToken());

        // Act
        var response = await _client.GetAsync("/api/clientes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

---

## 8. Comandos Útiles

```bash
# Crear migración
dotnet ef migrations add NombreMigracion -p src/CobranzaCloud.Infrastructure -s src/CobranzaCloud.Api

# Aplicar migraciones
dotnet ef database update -s src/CobranzaCloud.Api

# Ejecutar tests
dotnet test --verbosity normal

# Ejecutar con watch
dotnet watch run --project src/CobranzaCloud.Api

# Verificar vulnerabilidades
dotnet list package --vulnerable --include-transitive
```

---

## 9. Triggers para Intervención de Este Agente

Este agente debe intervenir cuando:

1. **Crear/modificar endpoints** - Cualquier cambio en `/Endpoints/`
2. **Cambios en base de datos** - Migraciones, configuraciones EF
3. **Lógica de negocio** - Handlers, servicios de dominio
4. **Seguridad** - Autenticación, autorización, validación
5. **Integración con conector** - Endpoints de sync
6. **Performance backend** - Optimización de queries

---

*Documento de contexto para agente backend - Actualizar con cada patrón nuevo*
