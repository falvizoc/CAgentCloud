namespace CobranzaCloud.Application.ExternalServices;

/// <summary>
/// Cache service interface for storing and retrieving data
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get a value from cache
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Set a value in cache with optional expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Remove a value from cache
    /// </summary>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Remove all keys matching a pattern
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken ct = default);

    /// <summary>
    /// Get or set a value - retrieves from cache or calls factory to get and cache value
    /// </summary>
    Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T?>> factory,
        TimeSpan? expiration = null,
        CancellationToken ct = default) where T : class;
}

/// <summary>
/// Cache key builder for consistent key generation
/// </summary>
public static class CacheKeys
{
    private const string Prefix = "cobranza";

    /// <summary>
    /// Default cache duration (15 minutes, aligned with connector sync cycle)
    /// </summary>
    public static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Short cache duration for frequently changing data
    /// </summary>
    public static readonly TimeSpan ShortExpiration = TimeSpan.FromMinutes(5);

    public static string CarteraResumen(Guid orgId, string empresaId, string moneda = "MXN")
        => $"{Prefix}:{orgId}:{empresaId}:resumen:{moneda}";

    public static string CarteraAntiguedad(Guid orgId, string empresaId, string moneda = "MXN")
        => $"{Prefix}:{orgId}:{empresaId}:antiguedad:{moneda}";

    public static string Clientes(Guid orgId, string empresaId)
        => $"{Prefix}:{orgId}:{empresaId}:clientes";

    public static string Clientes(Guid orgId, string empresaId, int page, int pageSize)
        => $"{Prefix}:{orgId}:{empresaId}:clientes:{page}:{pageSize}";

    public static string ClienteDetalle(Guid orgId, string empresaId, string claveCliente)
        => $"{Prefix}:{orgId}:{empresaId}:cliente:{claveCliente}";

    public static string Empresas(Guid orgId)
        => $"{Prefix}:{orgId}:empresas";

    public static string Health()
        => $"{Prefix}:health";

    /// <summary>
    /// Pattern to invalidate all cache for an organization
    /// </summary>
    public static string OrgPattern(Guid orgId)
        => $"{Prefix}:{orgId}:*";

    /// <summary>
    /// Pattern to invalidate all cache for an empresa
    /// </summary>
    public static string EmpresaPattern(Guid orgId, string empresaId)
        => $"{Prefix}:{orgId}:{empresaId}:*";
}
