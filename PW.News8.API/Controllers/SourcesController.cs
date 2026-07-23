using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PW.News8.Shared.DTOs;
using PW.News8.Shared.Interfaces;
using System.Text;
using System.Text.Json;

namespace PW.News8.API.Controllers
{
    /// Expone la lectura de fuentes (Sources) e ítems (SourceItems), así como la
    /// descarga (export) y carga (import) de esa información en formato JSON.
    /// El controlador no contiene lógica de negocio: solo orquesta llamadas al
    /// servicio (ISourceService) y traduce el resultado a códigos HTTP
    [ApiController]
    [Route("api/[controller]")]
    public class SourcesController : ControllerBase
    {
        private readonly ISourceService _sourceService;
        private readonly ILogger<SourcesController> _logger;

        // Opciones de serialización separadas: una para generar el archivo de
        // descarga (indentado y legible) y otra para leer archivos subidos
        // (tolerante a mayúsculas/minúsculas en las propiedades).
        private static readonly JsonSerializerOptions DownloadJsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly JsonSerializerOptions UploadJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public SourcesController(ISourceService sourceService, ILogger<SourcesController> logger)
        {
            _sourceService = sourceService;
            _logger = logger;
        }

        /// GET /api/sources — Lista todas las fuentes registradas.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SourceDto>>> GetSources(CancellationToken cancellationToken)
        {
            try
            {
                var sources = await _sourceService.GetAllSourcesAsync(cancellationToken);
                return Ok(sources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el listado de fuentes.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado al obtener las fuentes.");
            }
        }

        /// GET /api/sources/{id} — Obtiene una fuente puntual.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SourceDto>> GetSourceById(int id, CancellationToken cancellationToken)
        {
            try
            {
                var source = await _sourceService.GetSourceByIdAsync(id, cancellationToken);
                if (source is null)
                    return NotFound($"No existe una fuente con Id {id}.");

                return Ok(source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la fuente {SourceId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado al obtener la fuente.");
            }
        }

        ///GET /api/sources/{id}/items — Lista los ítems de una fuente.
        [HttpGet("{id:int}/items")]
        public async Task<ActionResult<IEnumerable<SourceItemDto>>> GetSourceItems(int id, CancellationToken cancellationToken)
        {
            try
            {
                var items = await _sourceService.GetItemsBySourceIdAsync(id, cancellationToken);
                if (items is null)
                    return NotFound($"No existe una fuente con Id {id}.");

                // Si la fuente existe pero no tiene ítems, se responde 200 con arreglo vacío.
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los ítems de la fuente {SourceId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado al obtener los ítems.");
            }
        }
        /// GET /api/sources/{id}/fetch — Lee la fuente en vivo (JSON/XML/HTML) sin guardar nada.
        [HttpGet("{id:int}/fetch")]
        public async Task<ActionResult<SourceFetchResultDto>> FetchSource(int id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _sourceService.FetchSourceAsync(id, cancellationToken);
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer en vivo la fuente {SourceId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado al leer la fuente.");
            }
        }

        /// POST /api/sources/{id}/items — Guarda el ítem que el usuario eligió del fetch en vivo.
        [HttpPost("{id:int}/items")]
        [Authorize] // Solo usuarios autorizados pueden guardar (según el enunciado)
        public async Task<ActionResult<SourceItemDto>> SaveFetchedItem(int id, [FromBody] SaveFetchedItemDto dto, CancellationToken cancellationToken)
        {
            if (dto?.Item is null || dto.Item.Count == 0)
                return BadRequest("Debe enviar un ítem con al menos un campo.");

            try
            {
                var saved = await _sourceService.SaveFetchedItemAsync(id, dto.Item, cancellationToken);
                if (saved is null)
                    return NotFound($"No existe una fuente con Id {id}.");

                return CreatedAtAction(nameof(GetSourceItems), new { id }, saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el ítem elegido de la fuente {SourceId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado al guardar el ítem.");
            }
        }

  
        /// Exporta un ítem puntual
        /// en el formato estándar acordado entre los líderes de todos los grupos.
        [HttpGet("items/{itemId:int}/export-standard")]
        public async Task<ActionResult<StandardItemDto>> ExportItemStandard(int itemId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _sourceService.ExportItemStandardAsync(itemId, cancellationToken);
                if (result is null)
                    return NotFound($"No existe un ítem con Id {itemId}.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar en formato estándar el ítem {ItemId}.", itemId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado al exportar el ítem.");
            }
        }

        // Este metodo permite importar un ítem estándar hacia una fuente específica. Se espera que el DTO contenga al menos el campo 'name'.
        [HttpPost("{id:int}/items/import-standard")]
        [Authorize]
        public async Task<ActionResult<SourceItemDto>> ImportStandardItem(int id, [FromBody] StandardItemDto dto, CancellationToken cancellationToken)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("El ítem estándar debe incluir al menos el campo 'name'.");

            try
            {
                var saved = await _sourceService.ImportStandardItemAsync(id, dto, cancellationToken);
                if (saved is null)
                    return NotFound($"No existe una fuente con Id {id}.");

                return CreatedAtAction(nameof(GetSourceItems), new { id }, saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al importar ítem estándar hacia la fuente {SourceId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado al importar el ítem.");
            }
        }

        /// Descarga la fuente y todos sus ítems
        [HttpGet("{id:int}/download")]
        public async Task<IActionResult> DownloadSource(int id, CancellationToken cancellationToken)
        {
            try
            {
                var package = await _sourceService.GetSourceForDownloadAsync(id, cancellationToken);
                if (package is null)
                    return NotFound($"No existe una fuente con Id {id}.");

                var json = JsonSerializer.Serialize(package, DownloadJsonOptions);
                var bytes = Encoding.UTF8.GetBytes(json);
                var fileName = $"source_{id}_{DateTime.Now:yyyyMMdd_HHmmss}.json";

                // File() configura automáticamente Content-Type y
                // Content-Disposition: attachment para forzar la descarga.
                return File(bytes, "application/json", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar la descarga de la fuente {SourceId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado al generar el archivo de descarga.");
            }
        }

        /// campo "file") con el formato generado por /download e importa los ítems
        /// nuevos de la fuente indicada, descartando duplicados por contenido
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadSource(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest("Debe adjuntar un archivo.");

            var extension = Path.GetExtension(file.FileName);
            if (!string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
                return BadRequest("El archivo debe tener extensión .json.");

            SourceDownloadDto? payload;
            try
            {
                await using var stream = file.OpenReadStream();
                payload = await JsonSerializer.DeserializeAsync<SourceDownloadDto>(stream, UploadJsonOptions, cancellationToken);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Archivo subido con formato JSON inválido: {FileName}.", file.FileName);
                return BadRequest("El archivo no contiene un JSON válido.");
            }

            if (payload is null)
                return BadRequest("El archivo está vacío o no se pudo interpretar.");

            try
            {
                var result = await _sourceService.UploadSourceItemsAsync(payload, cancellationToken);

                return result.Status switch
                {
                    UploadStatus.Invalid => BadRequest(result),
                    UploadStatus.SourceNotFound => NotFound(result),
                    _ => Ok(result) // Success o NoNewItems
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el archivo subido {FileName}.", file.FileName);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado al procesar el archivo.");
            }
        }
    }
}