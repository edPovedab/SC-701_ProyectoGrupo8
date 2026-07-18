using Microsoft.AspNetCore.Mvc;
using PW.News8.Shared.DTOs;
using PW.News8.Shared.Interfaces;

namespace PW.News8.API.Controllers
{
    /// Elemento sorpresa: expone noticias en vivo desde NewsAPI (newsapi.org),
    /// permitiendo consultar titulares por país/categoría o buscar por tema.
    /// No persiste nada en la base de datos: es una consulta directa a la fuente externa.
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(INewsService newsService, ILogger<NewsController> logger)
        {
            _newsService = newsService;
            _logger = logger;
        }

        /// GET /api/news/headlines?country=us&category=technology&q=&pageSize=20&page=1
        [HttpGet("headlines")]
        public async Task<ActionResult<NewsResultDto>> GetHeadlines(
            [FromQuery] string? country,
            [FromQuery] string? category,
            [FromQuery] string? q,
            [FromQuery] int pageSize = 20,
            [FromQuery] int page = 1,
            CancellationToken cancellationToken = default)
        {
            var query = new NewsQueryDto { Country = country, Category = category, Query = q, PageSize = pageSize, Page = page };

            try
            {
                var result = await _newsService.GetTopHeadlinesAsync(query, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener titulares de noticias.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado al obtener los titulares.");
            }
        }

        /// GET /api/news/search?q=inteligencia+artificial&language=es&pageSize=20&page=1
        [HttpGet("search")]
        public async Task<ActionResult<NewsResultDto>> Search(
            [FromQuery] string q,
            [FromQuery] string? language = "es",
            [FromQuery] int pageSize = 20,
            [FromQuery] int page = 1,
            CancellationToken cancellationToken = default)
        {
            var query = new NewsQueryDto { Query = q, Language = language, PageSize = pageSize, Page = page };

            try
            {
                var result = await _newsService.SearchNewsAsync(query, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar noticias por tema.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado al buscar noticias.");
            }
        }

        /// GET Catálogo fijo de categorías y países soportados
        /// por NewsAPI, usado para alimentar los filtros del muro personalizado.
        [HttpGet("categories")]
        public ActionResult<NewsCatalogDto> GetCatalog()
        {
            return Ok(new NewsCatalogDto
            {
                Categories = new[] { "general", "business", "entertainment", "health", "science", "sports", "technology" },
                Countries = new Dictionary<string, string>
                {
                    ["us"] = "Estados Unidos",
                    ["mx"] = "México",
                    ["ar"] = "Argentina",
                    ["co"] = "Colombia",
                    ["es"] = "España",
                    ["gb"] = "Reino Unido",
                    ["fr"] = "Francia",
                    ["de"] = "Alemania",
                    ["br"] = "Brasil",
                    ["ca"] = "Canadá"
                }
            });
        }
    }
}
