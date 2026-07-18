namespace PW.News8.Shared.DTOs
{
    // Catálogo fijo de categorías y países soportados por NewsAPI,
    // usado para construir los filtros del muro de noticias en el front.
    public class NewsCatalogDto
    {
        public IEnumerable<string> Categories { get; set; } = Enumerable.Empty<string>();
        public IDictionary<string, string> Countries { get; set; } = new Dictionary<string, string>();
    }
}
