namespace PW.News8.Shared.DTOs
{
    // Parámetros de búsqueda/filtro para el módulo de noticias (NewsAPI).
    // Country/Category se usan para "top-headlines"; Query/Language para "everything".
    public class NewsQueryDto
    {
        public string? Country { get; set; }
        public string? Category { get; set; }
        public string? Query { get; set; }
        public string? Language { get; set; } = "es";
        public int PageSize { get; set; } = 20;
        public int Page { get; set; } = 1;
    }
}
