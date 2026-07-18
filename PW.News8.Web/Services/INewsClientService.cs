using PW.News8.Shared.DTOs;

namespace PW.News8.Web.Services
{
    // elemento sorpresa con un muro de noticias agregado.
    // INewsClientService es la interfaz que define los métodos para interactuar con la API de noticias.
    public interface INewsClientService
    {
        Task<NewsResultDto> GetHeadlinesAsync(string? country, string? category, string? keyword, CancellationToken cancellationToken = default);

        Task<NewsResultDto> SearchAsync(string keyword, string? language, CancellationToken cancellationToken = default);

        Task<NewsCatalogDto> GetCatalogAsync(CancellationToken cancellationToken = default);
    }
}
