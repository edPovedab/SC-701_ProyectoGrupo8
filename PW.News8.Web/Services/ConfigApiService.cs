using System.Net.Http.Json;
using System.Text.Json;
using PW.News8.Shared.DTOs;
using PW.News8.Web.Controllers;

namespace PW.News8.Web.Services;

public class ConfigApiService : IConfigApiService
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ConfigApiService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ConfigApiService(HttpClient http, IHttpContextAccessor httpContextAccessor, ILogger<ConfigApiService> logger)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<List<UserRoleDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/users");
        AttachApiSessionCookie(request);

        var response = await _http.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("No se pudo obtener la lista de usuarios. Código: {StatusCode}", response.StatusCode);
            return new List<UserRoleDto>();
        }

        var users = await response.Content.ReadFromJsonAsync<List<UserRoleDto>>(JsonOptions, cancellationToken);
        return users ?? new List<UserRoleDto>();
    }

    public async Task<AuthResponseDto> AssignRoleAsync(string email, string role, CancellationToken cancellationToken = default)
    {
        var dto = new RegisterDto { Email = email, Role = role };

        using var request = new HttpRequestMessage(HttpMethod.Put, "api/auth/assign-role")
        {
            Content = JsonContent.Create(dto)
        };
        AttachApiSessionCookie(request);

        var response = await _http.SendAsync(request, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOptions, cancellationToken);
        return result ?? new AuthResponseDto { Success = false, Message = "Error al asignar rol." };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/register")
        {
            Content = JsonContent.Create(dto)
        };
        AttachApiSessionCookie(request);

        var response = await _http.SendAsync(request, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOptions, cancellationToken);
        return result ?? new AuthResponseDto { Success = false, Message = "Error al crear el usuario." };
    }

    
    private void AttachApiSessionCookie(HttpRequestMessage request)
    {
        var cookie = _httpContextAccessor.HttpContext?.User
            .FindFirst(AccountController.ApiSessionCookieClaimType)?.Value;

        if (!string.IsNullOrWhiteSpace(cookie))
            request.Headers.Add("Cookie", cookie);
        else
            _logger.LogWarning("No hay cookie de sesión de la API disponible para esta solicitud administrativa.");
    }
}