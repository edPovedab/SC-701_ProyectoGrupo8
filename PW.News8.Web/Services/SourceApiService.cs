using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PW.News8.Shared.DTOs;

namespace PW.News8.Web.Services
{
    
    public class SourceApiService : ISourceApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<SourceApiService> _logger;

        
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public SourceApiService(HttpClient http, ILogger<SourceApiService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<SourceDto>> GetSourcesAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("api/sources", cancellationToken);
            response.EnsureSuccessStatusCode();

            var sources = await response.Content.ReadFromJsonAsync<List<SourceDto>>(JsonOptions, cancellationToken);
            return sources ?? new List<SourceDto>();
        }

        public async Task<SourceDto?> GetSourceAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"api/sources/{id}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SourceDto>(JsonOptions, cancellationToken);
        }

        public async Task<List<SourceItemDto>?> GetItemsAsync(int sourceId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"api/sources/{sourceId}/items", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            var items = await response.Content.ReadFromJsonAsync<List<SourceItemDto>>(JsonOptions, cancellationToken);
            return items ?? new List<SourceItemDto>();
        }

        public async Task<SourceFetchResultDto> FetchLiveAsync(int sourceId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"api/sources/{sourceId}/fetch", cancellationToken);

            // El endpoint responde 400 con el mismo DTO cuando la lectura en vivo falla,
            // así que se intenta deserializar tanto en éxito como en fallo controlado.
            if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest)
            {
                var result = await response.Content.ReadFromJsonAsync<SourceFetchResultDto>(JsonOptions, cancellationToken);
                if (result is not null)
                    return result;
            }

            response.EnsureSuccessStatusCode();
            return new SourceFetchResultDto { Success = false, ErrorMessage = "Respuesta inesperada de la API.", SourceId = sourceId };
        }

        public async Task<SourceItemDto?> SaveFetchedItemAsync(int sourceId, Dictionary<string, object> item, CancellationToken cancellationToken = default)
        {
            var payload = new SaveFetchedItemDto { Item = item };
            var response = await _http.PostAsJsonAsync($"api/sources/{sourceId}/items", payload, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SourceItemDto>(JsonOptions, cancellationToken);
        }

        public async Task<(byte[] Content, string FileName)?> DownloadSourceAsync(int sourceId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"api/sources/{sourceId}/download", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                           ?? response.Content.Headers.ContentDisposition?.FileName
                           ?? $"source_{sourceId}.json";

            return (bytes, fileName.Trim('"'));
        }

        public async Task<SourceUploadResultDto> UploadSourceAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
        {
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Add(streamContent, "file", fileName);

            var response = await _http.PostAsync("api/sources/upload", content, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<SourceUploadResultDto>(JsonOptions, cancellationToken);

            if (result is not null)
                return result;

            _logger.LogWarning("La API respondió {StatusCode} sin cuerpo interpretable al subir {FileName}.", response.StatusCode, fileName);
            return SourceUploadResultDto.Invalid("No se pudo interpretar la respuesta de la API.");
        }
    }
}