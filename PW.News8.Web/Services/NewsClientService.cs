using System.Net.Http.Json;
using System.Text.Json;
using PW.News8.Shared.DTOs;

namespace PW.News8.Web.Services
{
    public class NewsClientService : INewsClientService
    {
        private readonly HttpClient _http;
        private readonly ILogger<NewsClientService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public NewsClientService(HttpClient http, ILogger<NewsClientService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<NewsResultDto> GetHeadlinesAsync(string? country, string? category, string? keyword, CancellationToken cancellationToken = default)
        {
            var qs = new List<string>();
            if (!string.IsNullOrWhiteSpace(country)) qs.Add($"country={Uri.EscapeDataString(country)}");
            if (!string.IsNullOrWhiteSpace(category)) qs.Add($"category={Uri.EscapeDataString(category)}");
            if (!string.IsNullOrWhiteSpace(keyword)) qs.Add($"q={Uri.EscapeDataString(keyword)}");

            return await FetchAsync($"api/news/headlines?{string.Join("&", qs)}", cancellationToken);
        }

        public async Task<NewsResultDto> SearchAsync(string keyword, string? language, CancellationToken cancellationToken = default)
        {
            var qs = new List<string> { $"q={Uri.EscapeDataString(keyword)}" };
            if (!string.IsNullOrWhiteSpace(language)) qs.Add($"language={Uri.EscapeDataString(language)}");

            return await FetchAsync($"api/news/search?{string.Join("&", qs)}", cancellationToken);
        }

        public async Task<NewsCatalogDto> GetCatalogAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _http.GetAsync("api/news/categories", cancellationToken);
                response.EnsureSuccessStatusCode();
                var catalog = await response.Content.ReadFromJsonAsync<NewsCatalogDto>(JsonOptions, cancellationToken);
                return catalog ?? new NewsCatalogDto();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "No se pudo contactar a la API para obtener el catálogo de noticias.");
                return new NewsCatalogDto();
            }
        }

        private async Task<NewsResultDto> FetchAsync(string relativeUrl, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _http.GetAsync(relativeUrl, cancellationToken);
                var result = await response.Content.ReadFromJsonAsync<NewsResultDto>(JsonOptions, cancellationToken);
                return result ?? new NewsResultDto { Success = false, ErrorMessage = "Respuesta vacía de la API." };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "No se pudo contactar a la API de noticias.");
                return new NewsResultDto { Success = false, ErrorMessage = "No se pudo contactar a la API." };
            }
        }
    }
}
