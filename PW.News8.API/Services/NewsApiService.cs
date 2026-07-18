using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PW.News8.Shared.DTOs;
using PW.News8.Shared.Interfaces;

namespace PW.News8.API.Services
{
    /// Elemento sorpresa: traduce las consultas del front a llamadas contra la
    /// API pública de newsapi.org
    public class NewsApiService : INewsService
    {
        private readonly HttpClient _http;
        private readonly ILogger<NewsApiService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public NewsApiService(HttpClient http, ILogger<NewsApiService> logger)
        {
            _http = http;
            _logger = logger;
        }

        /// Titulares (top-headlines): requiere country y/o category (o q como refuerzo).
        public async Task<NewsResultDto> GetTopHeadlinesAsync(NewsQueryDto query, CancellationToken cancellationToken = default)
        {
            var qs = new List<string>
            {
                $"pageSize={Math.Clamp(query.PageSize, 1, 100)}",
                $"page={Math.Max(query.Page, 1)}"
            };

            if (!string.IsNullOrWhiteSpace(query.Country)) qs.Add($"country={Uri.EscapeDataString(query.Country)}");
            if (!string.IsNullOrWhiteSpace(query.Category)) qs.Add($"category={Uri.EscapeDataString(query.Category)}");
            if (!string.IsNullOrWhiteSpace(query.Query)) qs.Add($"q={Uri.EscapeDataString(query.Query)}");

            // NewsAPI exige al menos un filtro (country, category o sources) en este endpoint.
            if (!qs.Any(p => p.StartsWith("country=") || p.StartsWith("category=")))
                qs.Add("country=us");

            return await FetchAsync($"top-headlines?{string.Join("&", qs)}", cancellationToken);
        }

        /// Búsqueda por tema (everything): requiere q.
        public async Task<NewsResultDto> SearchNewsAsync(NewsQueryDto query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query.Query))
                return new NewsResultDto { Success = false, ErrorMessage = "Debe indicar un tema o palabra clave para buscar." };

            var qs = new List<string>
            {
                $"q={Uri.EscapeDataString(query.Query)}",
                $"pageSize={Math.Clamp(query.PageSize, 1, 100)}",
                $"page={Math.Max(query.Page, 1)}",
                "sortBy=publishedAt"
            };

            if (!string.IsNullOrWhiteSpace(query.Language)) qs.Add($"language={Uri.EscapeDataString(query.Language)}");

            return await FetchAsync($"everything?{string.Join("&", qs)}", cancellationToken);
        }

        private async Task<NewsResultDto> FetchAsync(string relativeUrl, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _http.GetAsync(relativeUrl, cancellationToken);
                var raw = await response.Content.ReadFromJsonAsync<NewsApiRawResponse>(JsonOptions, cancellationToken);

                if (!response.IsSuccessStatusCode || raw is null || raw.Status != "ok")
                {
                    var message = raw?.Message ?? "No se pudo obtener información de NewsAPI.";
                    _logger.LogWarning("NewsAPI respondió {StatusCode}: {Message}", response.StatusCode, message);
                    return new NewsResultDto { Success = false, ErrorMessage = message };
                }

                var items = (raw.Articles ?? new List<NewsApiArticle>()).Select(a => new NewsItemDto
                {
                    Title = string.IsNullOrWhiteSpace(a.Title) ? "(Sin título)" : a.Title!,
                    Description = a.Description,
                    Author = a.Author,
                    Url = a.Url,
                    ImageUrl = a.UrlToImage,
                    PublishedAt = a.PublishedAt,
                    SourceName = a.Source?.Name ?? "Desconocida",
                    RawJson = JsonSerializer.Serialize(a)
                }).ToList();

                return new NewsResultDto { Success = true, TotalResults = raw.TotalResults, Items = items };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar NewsAPI en {Url}.", relativeUrl);
                return new NewsResultDto { Success = false, ErrorMessage = "Ocurrió un error al contactar NewsAPI." };
            }
        }

        // ── Modelos internos: reflejan la forma cruda de la respuesta de newsapi.org ──
        private class NewsApiRawResponse
        {
            public string? Status { get; set; }
            public string? Message { get; set; } // se presenta solo si status = "error"
            public int TotalResults { get; set; }
            public List<NewsApiArticle>? Articles { get; set; }
        }

        private class NewsApiArticle
        {
            public NewsApiSource? Source { get; set; }
            public string? Author { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? Url { get; set; }
            public string? UrlToImage { get; set; }
            public DateTime? PublishedAt { get; set; }
            public string? Content { get; set; }
        }

        private class NewsApiSource
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
        }
    }
}
