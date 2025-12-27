using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CobranzaCloud.Application.ExternalServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CobranzaCloud.Infrastructure.Services;

/// <summary>
/// HTTP client for Cobranza Agent API (ASPEL Connector)
/// </summary>
public class CobranzaAgentClient : ICobranzaAgentClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CobranzaAgentClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CobranzaAgentClient(
        HttpClient httpClient,
        IOptions<CobranzaAgentOptions> options,
        ILogger<CobranzaAgentClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Configure base address and API key
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", options.Value.ApiKey);
        _httpClient.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<AgentHealthResponse?> GetHealthAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<AgentHealthResponse>(
                "/api/health", _jsonOptions, ct);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking agent health");
            return null;
        }
    }

    public async Task<AgentResponse<List<AgentEmpresa>>?> GetEmpresasAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<AgentResponse<List<AgentEmpresa>>>(
                "/api/empresas", _jsonOptions, ct);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching empresas");
            return null;
        }
    }

    public async Task<AgentResponse<AgentCarteraResumen>?> GetCarteraResumenAsync(
        string empresaId,
        int moneda = 1,
        CancellationToken ct = default)
    {
        try
        {
            // MUST: Always use moneda parameter (DEC-009)
            var url = $"/api/empresas/{empresaId}/cartera/resumen?moneda={moneda}";
            _logger.LogDebug("Fetching cartera resumen: {Url}", url);

            var response = await _httpClient.GetFromJsonAsync<AgentResponse<AgentCarteraResumen>>(
                url, _jsonOptions, ct);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching cartera resumen for empresa {EmpresaId}", empresaId);
            return null;
        }
    }

    public async Task<AgentResponse<AgentCarteraAntiguedad>?> GetCarteraAntiguedadAsync(
        string empresaId,
        int moneda = 1,
        CancellationToken ct = default)
    {
        try
        {
            // MUST: Always use moneda parameter (DEC-009)
            var url = $"/api/empresas/{empresaId}/cartera/antiguedad?moneda={moneda}";
            _logger.LogDebug("Fetching cartera antiguedad: {Url}", url);

            var response = await _httpClient.GetFromJsonAsync<AgentResponse<AgentCarteraAntiguedad>>(
                url, _jsonOptions, ct);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching cartera antiguedad for empresa {EmpresaId}", empresaId);
            return null;
        }
    }

    public async Task<AgentPaginatedResponse<AgentCliente>?> GetClientesAsync(
        string empresaId,
        int limite = 50,
        int offset = 0,
        CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/empresas/{empresaId}/clientes?limite={limite}&offset={offset}";
            _logger.LogDebug("Fetching clientes: {Url}", url);

            var response = await _httpClient.GetFromJsonAsync<AgentPaginatedResponse<AgentCliente>>(
                url, _jsonOptions, ct);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching clientes for empresa {EmpresaId}", empresaId);
            return null;
        }
    }

    public async Task<AgentResponse<AgentClienteDetalle>?> GetClienteDetalleAsync(
        string empresaId,
        string claveCliente,
        CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/empresas/{empresaId}/clientes/{claveCliente}";
            _logger.LogDebug("Fetching cliente detalle: {Url}", url);

            var response = await _httpClient.GetFromJsonAsync<AgentResponse<AgentClienteDetalle>>(
                url, _jsonOptions, ct);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching cliente {ClaveCliente} for empresa {EmpresaId}",
                claveCliente, empresaId);
            return null;
        }
    }

    public async Task<AgentPaginatedResponse<AgentFactura>?> GetCarteraVencidaAsync(
        string empresaId,
        int moneda = 1,
        int limite = 50,
        int offset = 0,
        CancellationToken ct = default)
    {
        try
        {
            // MUST: Always use moneda parameter (DEC-009)
            var url = $"/api/empresas/{empresaId}/cartera/vencida?moneda={moneda}&limite={limite}&offset={offset}";
            _logger.LogDebug("Fetching cartera vencida: {Url}", url);

            var response = await _httpClient.GetFromJsonAsync<AgentPaginatedResponse<AgentFactura>>(
                url, _jsonOptions, ct);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching cartera vencida for empresa {EmpresaId}", empresaId);
            return null;
        }
    }
}

/// <summary>
/// Configuration options for Cobranza Agent client
/// </summary>
public class CobranzaAgentOptions
{
    public const string SectionName = "CobranzaAgent";

    /// <summary>
    /// Base URL of the Cobranza Agent API
    /// </summary>
    public string BaseUrl { get; set; } = "http://bitmovil.ddns.net:5000";

    /// <summary>
    /// API Key for authentication
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Default empresa ID to use
    /// </summary>
    public string DefaultEmpresaId { get; set; } = "01";

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Default currency code (1=MXN, 2=USD)
    /// </summary>
    public int DefaultMoneda { get; set; } = 1;
}
