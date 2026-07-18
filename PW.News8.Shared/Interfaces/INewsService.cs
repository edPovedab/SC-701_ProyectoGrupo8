using PW.News8.Shared.DTOs;

namespace PW.News8.Shared.Interfaces
{
    // Contrato del "elemento sorpresa": consulta noticias en vivo desde NewsAPI.
    // No persiste nada en la base de datos existente, es solo lectura externa.
    public interface INewsService
    {
        Task<NewsResultDto> GetTopHeadlinesAsync(NewsQueryDto query, CancellationToken cancellationToken = default);

        Task<NewsResultDto> SearchNewsAsync(NewsQueryDto query, CancellationToken cancellationToken = default);
    }
}
