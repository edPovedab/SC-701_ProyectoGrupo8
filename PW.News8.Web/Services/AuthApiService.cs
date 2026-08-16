using System.Net.Http.Json;
using System.Text.Json;
using PW.News8.Shared.DTOs;
using PW.News8.Web.Models;

namespace PW.News8.Web.Services
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AuthApiService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthApiService(HttpClient http, ILogger<AuthApiService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<LoginResult> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login", dto, cancellationToken);

                // El AuthController responde 200 (éxito) o 401 (credenciales inválidas),
                // pero en ambos casos el cuerpo es un AuthResponseDto. Solo tratamos
                // como error "duro" los códigos que no traen ese formato (500, etc.).
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOptions, cancellationToken)
                                        ?? new AuthResponseDto { Success = false, Message = "Respuesta vacía del servidor." };

                    string? sessionCookie = null;

                    // Si el login fue exitoso, la API manda su propia cookie de Identity
                    // en el header Set-Cookie. La guardamos (solo "nombre=valor") para
                    // poder reenviarla en llamadas administrativas más adelante.
                    if (authResponse.Success && response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
                    {
                        var identityCookie = setCookieValues.FirstOrDefault(c =>
                            c.StartsWith(".AspNetCore.Identity.Application", StringComparison.OrdinalIgnoreCase));

                        if (identityCookie != null)
                            sessionCookie = identityCookie.Split(';')[0];
                    }

                    return new LoginResult { Response = authResponse, ApiSessionCookie = sessionCookie };
                }

                _logger.LogWarning("Login falló con código inesperado {StatusCode}", response.StatusCode);
                return new LoginResult
                {
                    Response = new AuthResponseDto { Success = false, Message = "Ocurrió un error inesperado al iniciar sesión." }
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "No se pudo contactar la API para el login.");
                return new LoginResult
                {
                    Response = new AuthResponseDto { Success = false, Message = "No se pudo conectar con el servidor. Intente más tarde." }
                };
            }
        }

        public async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _http.PostAsync("api/auth/logout", null, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                // El logout local (cookie del Web) igual va a proceder aunque
                // la API no responda, así que solo lo registramos.
                _logger.LogWarning(ex, "No se pudo notificar el logout a la API.");
            }
        }
    }
}