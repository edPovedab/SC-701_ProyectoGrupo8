using Microsoft.AspNetCore.Mvc;
using PW.News8.Web.Services;

namespace PW.News8.Web.Controllers
{
    /// Elemento sorpresa: muro de noticias personalizado, alimentado por NewsAPI
    /// a través de PW.News8.API.
    public class NewsController : Controller
    {
        private readonly INewsClientService _news;
        private readonly ILogger<NewsController> _logger;

        public NewsController(INewsClientService news, ILogger<NewsController> logger)
        {
            _news = news;
            _logger = logger;
        }

        // GET /News
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var catalog = await _news.GetCatalogAsync(cancellationToken);
            return View(catalog);
        }

        // Metodo get para obtener el muro de noticias parcial, que puede ser utilizado para actualizar dinámicamente la sección de noticias en la vista principal.
        [HttpGet]
        public async Task<IActionResult> WallPartial(
            string mode,
            string? country,
            string? category,
            string? q,
            string? language,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = mode == "search"
                    ? await _news.SearchAsync(q ?? string.Empty, language, cancellationToken)
                    : await _news.GetHeadlinesAsync(country, category, q, cancellationToken);

                return PartialView("_NewsWall", result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "No se pudo contactar a la API al cargar el muro de noticias.");
                return StatusCode(StatusCodes.Status502BadGateway, "No se pudo contactar a la API.");
            }
        }
    }
}
