using Microsoft.AspNetCore.Mvc;
using PW.News8.Shared.DTOs;
using PW.News8.Web.Services;
using System.Text.Json;

namespace PW.News8.Web.Controllers
{
    
    public class SourcesController : Controller
    {
        private readonly ISourceApiService _api;
        private readonly ILogger<SourcesController> _logger;

        public SourcesController(ISourceApiService api, ILogger<SourcesController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // GET /Sources — Vista contenedora
        public IActionResult Index()
        {
            return View();
        }

        // GET /Sources/ListPartial 
        [HttpGet]
        public async Task<IActionResult> ListPartial(CancellationToken cancellationToken)
        {
            try
            {
                var sources = await _api.GetSourcesAsync(cancellationToken);
                return PartialView("_SourcesList", sources);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "No se pudo contactar a la API al listar fuentes.");
                return StatusCode(StatusCodes.Status502BadGateway, "No se pudo contactar a la API.");
            }
        }

        // GET /Sources
        public async Task<IActionResult> Items(int id, CancellationToken cancellationToken)
        {
            var source = await _api.GetSourceAsync(id, cancellationToken);
            if (source is null)
                return NotFound();

            return View(source);
        }

        // GET /Sources/ItemsPartial
        [HttpGet]
        public async Task<IActionResult> ItemsPartial(int id, CancellationToken cancellationToken)
        {
            try
            {
                var items = await _api.GetItemsAsync(id, cancellationToken);
                if (items is null)
                    return NotFound("La fuente no existe.");

                return PartialView("_SourceItemsList", items);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "No se pudo contactar a la API al listar ítems de la fuente {SourceId}.", id);
                return StatusCode(StatusCodes.Status502BadGateway, "No se pudo contactar a la API.");
            }
        }

        // GET /Sources/FetchLive
        [HttpGet]
        public async Task<IActionResult> FetchLive(int id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _api.FetchLiveAsync(id, cancellationToken);
                return Json(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "No se pudo contactar a la API al leer en vivo la fuente {SourceId}.", id);
                return StatusCode(StatusCodes.Status502BadGateway, new { success = false, errorMessage = "No se pudo contactar a la API." });
            }
        }

        // POST /Sources/SaveItem/
        [HttpPost]
        public async Task<IActionResult> SaveItem(int id, [FromBody] JsonElement body, CancellationToken cancellationToken)
        {
            var item = JsonSerializer.Deserialize<Dictionary<string, object>>(body.GetRawText());
            if (item is null || item.Count == 0)
                return BadRequest(new { message = "Debe enviar un ítem con al menos un campo." });

            try
            {
                var saved = await _api.SaveFetchedItemAsync(id, item, cancellationToken);
                if (saved is null)
                    return NotFound(new { message = $"No existe una fuente con Id {id}." });

                return Json(saved);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "No se pudo contactar a la API al guardar el ítem de la fuente {SourceId}.", id);
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo contactar a la API." });
            }
        }

        // GET /Sources/Download
        [HttpGet]
        public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
        {
            var download = await _api.DownloadSourceAsync(id, cancellationToken);
            if (download is null)
                return NotFound();

            return File(download.Value.Content, "application/json", download.Value.FileName);
        }

        // GET /Sources
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        // POST /Sources/Upload 
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { message = "Debe adjuntar un archivo." });

            var extension = Path.GetExtension(file.FileName);
            if (!string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "El archivo debe tener extensión .json." });

            try
            {
                await using var stream = file.OpenReadStream();
                var result = await _api.UploadSourceAsync(stream, file.FileName, cancellationToken);
                return Json(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "No se pudo contactar a la API al subir el archivo {FileName}.", file.FileName);
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "No se pudo contactar a la API." });
            }
        }
    }
}