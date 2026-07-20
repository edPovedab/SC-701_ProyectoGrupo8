using System.Net.Http.Json;
using System.Text.Json;
using PW.News8.Shared.DTOs;

namespace PW.News8.Web.Services;

public class ConfigApiService : IConfigApiService
{
    private readonly HttpClient _http;
    private readonly ILogger<ConfigApiService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ConfigApiService(HttpClient http, ILogger<ConfigApiService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<UserRoleDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync("api/auth/users", cancellationToken);
        response.EnsureSuccessStatusCode();

        var users = await response.Content.ReadFromJsonAsync<List<UserRoleDto>>(JsonOptions, cancellationToken);
        return users ?? new List<UserRoleDto>();
    }

    public async Task<AuthResponseDto> AssignRoleAsync(string email, string role, CancellationToken cancellationToken = default)
    {
        var dto = new RegisterDto { Email = email, Role = role };
        var response = await _http.PutAsJsonAsync("api/auth/assign-role", dto, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOptions, cancellationToken);
        return result ?? new AuthResponseDto { Success = false, Message = "Error al asignar rol." };
    }
}