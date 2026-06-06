namespace PW.News8.Shared.DTOs
{
    //campos específicos de noticias para mostrar en la UI
    public class NewsItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Author { get; set; }
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string SourceName { get; set; } = string.Empty;

        // JSON original por si se quiere guardar
        public string RawJson { get; set; } = string.Empty;
    }
}