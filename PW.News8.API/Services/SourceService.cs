using PW.News8.API.Repositories;
using PW.News8.Shared.DTOs;
using PW.News8.Shared.Interfaces;
using PW.News8.Shared.Models;

namespace PW.News8.API.Services
{

    /// Implementa <see cref="ISourceService"/> apoyándose exclusivamente en los
    /// repositorios ya existentes (ISourceRepository / ISourceItemRepository).
    /// Toda la lógica de negocio de lectura, exportación e importación vive aquí,
    /// para que los controladores se mantengan delgados (sin lógica de negocio).
    public class SourceService : ISourceService
    {
        private readonly ISourceRepository _sourceRepository;
        private readonly ISourceItemRepository _sourceItemRepository;
        private readonly IEnumerable<ISourceReader> _readers;
        private readonly IAppSettingRepository _appSettingRepository;

        public SourceService(
        ISourceRepository sourceRepository,
        ISourceItemRepository sourceItemRepository,
        IEnumerable<ISourceReader> readers,
        IAppSettingRepository appSettingRepository)
        {
            _sourceRepository = sourceRepository;
            _sourceItemRepository = sourceItemRepository;
            _readers = readers;
            _appSettingRepository = appSettingRepository;
        }

        public async Task<IEnumerable<SourceDto>> GetAllSourcesAsync(CancellationToken cancellationToken = default)
        {
            var sources = await _sourceRepository.GetAllAsync();
            return sources.Select(MapToDto);
        }

        public async Task<SourceDto?> GetSourceByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var source = await _sourceRepository.GetByIdAsync(id);
            return source is null ? null : MapToDto(source);
        }

        public async Task<IEnumerable<SourceItemDto>?> GetItemsBySourceIdAsync(int sourceId, CancellationToken cancellationToken = default)
        {
            var source = await _sourceRepository.GetByIdAsync(sourceId);
            if (source is null)
                return null; // El controlador traduce esto a 404

            var items = await _sourceItemRepository.GetBySourceIdAsync(sourceId);
            return items.Select(item => MapToItemDto(item, source.Name));
        }

        public async Task<SourceDownloadDto?> GetSourceForDownloadAsync(int id, CancellationToken cancellationToken = default)
        {
            var source = await _sourceRepository.GetByIdAsync(id);
            if (source is null)
                return null;

            var items = await _sourceItemRepository.GetBySourceIdAsync(id);

            return new SourceDownloadDto
            {
                Source = MapToDto(source),
                Items = items.Select(item => new SourceItemExportDto
                {
                    SourceUrl = source.Url,
                    SourceName = source.Name,
                    ComponentType = source.ComponentType,
                    Json = item.Json,
                    ExportedAt = item.CreatedAt
                }).ToList(),
                ExportedAt = DateTime.Now
            };
        }
        public async Task<SourceFetchResultDto> FetchSourceAsync(int id, CancellationToken cancellationToken = default)
        {
            var source = await _sourceRepository.GetByIdAsync(id);
            if (source is null)
            {
                return new SourceFetchResultDto
                {
                    Success = false,
                    ErrorMessage = $"No existe una fuente con Id {id}.",
                    SourceId = id
                };
            }

            var reader = _readers.FirstOrDefault(r =>
                string.Equals(r.SupportedType, source.ComponentType, StringComparison.OrdinalIgnoreCase));

            if (reader is null)
            {
                return new SourceFetchResultDto
                {
                    Success = false,
                    ErrorMessage = $"No hay un reader disponible para el tipo '{source.ComponentType}'.",
                    SourceId = id,
                    SourceName = source.Name
                };
            }

            string? secret = null;
            if (source.RequiresSecret)
            {
                // Convención de clave: "Source:{Id}:Secret"
                var setting = await _appSettingRepository.GetByKeyAsync($"Source:{source.Id}:Secret");
                secret = setting?.Value;
            }

            var readResult = await reader.ReadAsync(source.Url, secret);

            return new SourceFetchResultDto
            {
                Success = readResult.Success,
                ErrorMessage = readResult.ErrorMessage,
                SourceId = source.Id,
                SourceName = source.Name,
                Items = readResult.Items
            };
        }

        public async Task<SourceItemDto?> SaveFetchedItemAsync(int sourceId, Dictionary<string, object> item, CancellationToken cancellationToken = default)
        {
            var source = await _sourceRepository.GetByIdAsync(sourceId);
            if (source is null)
                return null;

            var json = System.Text.Json.JsonSerializer.Serialize(item);

            var entity = new SourceItem
            {
                SourceId = sourceId,
                Json = json,
                CreatedAt = DateTime.Now
            };

            await _sourceItemRepository.AddAsync(entity);

            return MapToItemDto(entity, source.Name);
        }

        public async Task<SourceUploadResultDto> UploadSourceItemsAsync(SourceDownloadDto payload, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(payload);

            // Validación de estructura básica del JSON recibido
            if (payload.Source is null || payload.Source.Id <= 0)
                return SourceUploadResultDto.Invalid("El archivo no contiene una fuente válida (se requiere Source.Id).");

            var source = await _sourceRepository.GetByIdAsync(payload.Source.Id);
            if (source is null)
                return SourceUploadResultDto.NotFound(payload.Source.Id);

            var incomingItems = payload.Items ?? new List<SourceItemExportDto>();
            if (incomingItems.Count == 0)
                return SourceUploadResultDto.NoItems(source.Id);

            // Set de contenidos ya existentes para esa fuente, usado para evitar duplicados
            var existingItems = await _sourceItemRepository.GetBySourceIdAsync(source.Id);
            var existingJsonSet = existingItems
                .Select(item => item.Json.Trim())
                .ToHashSet(StringComparer.Ordinal);

            var newEntities = new List<SourceItem>();
            var duplicateCount = 0;

            foreach (var incoming in incomingItems)
            {
                var json = incoming.Json?.Trim();
                if (string.IsNullOrWhiteSpace(json))
                    continue; // ítem vacío/corrupto, se ignora silenciosamente

                if (!existingJsonSet.Add(json))
                {
                    duplicateCount++; // ítem duplicado, se ignora
                    continue;
                }

                newEntities.Add(new SourceItem
                {
                    SourceId = source.Id,
                    Json = json,
                    CreatedAt = DateTime.Now
                });
            }

            if (newEntities.Count > 0)
                await _sourceItemRepository.AddRangeAsync(newEntities);

            return SourceUploadResultDto.Success(source.Id, newEntities.Count, duplicateCount);
        }
        public async Task<StandardItemDto?> ExportItemStandardAsync(int itemId, CancellationToken cancellationToken = default)
        {
            var item = await _sourceItemRepository.GetByIdAsync(itemId);
            if (item is null || item.Source is null)
                return null;

            Dictionary<string, object>? fields = null;
            try
            {
                fields = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(item.Json);
            }
            catch (System.Text.Json.JsonException)
            {
               
            }

            string? Get(params string[] keys)
            {
                if (fields is null) return null;
                foreach (var key in keys)
                {
                    var match = fields.FirstOrDefault(kv => string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase));
                    if (!match.Equals(default(KeyValuePair<string, object>)) && match.Value is not null)
                        return match.Value.ToString();
                }
                return null;
            }

            return new StandardItemDto
            {
                ExportedAt = item.CreatedAt,
                SourceName = item.Source.Name,
                Id = item.Id.ToString(),
                Name = Get("name", "title") ?? item.Source.Name,
                Category = Get("category") ?? item.Source.ComponentType,
                Description = Get("description", "body", "text") ?? string.Empty,
                ImageUrl = Get("image_url", "imageUrl", "thumbnail") ?? string.Empty,
                Tags = new List<string>()
            };
        }

        public async Task<SourceItemDto?> ImportStandardItemAsync(int sourceId, StandardItemDto standardItem, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(standardItem);

            var source = await _sourceRepository.GetByIdAsync(sourceId);
            if (source is null)
                return null;

            var json = System.Text.Json.JsonSerializer.Serialize(standardItem);

            var entity = new SourceItem
            {
                SourceId = sourceId,
                Json = json,
                CreatedAt = DateTime.Now
            };

            await _sourceItemRepository.AddAsync(entity);

            return MapToItemDto(entity, source.Name);
        }
        private static SourceDto MapToDto(Source source) => new()
        {
            Id = source.Id,
            Url = source.Url,
            Name = source.Name,
            Description = source.Description,
            ComponentType = source.ComponentType,
            RequiresSecret = source.RequiresSecret
        };

        private static SourceItemDto MapToItemDto(SourceItem item, string sourceName) => new()
        {
            Id = item.Id,
            SourceId = item.SourceId,
            SourceName = sourceName,
            Json = item.Json,
            CreatedAt = item.CreatedAt
        };
    }
}