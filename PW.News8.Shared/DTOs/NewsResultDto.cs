namespace PW.News8.Shared.DTOs
{
    // Envoltorio de respuesta para cualquier consulta al módulo de noticias:
    // permite distinguir éxito/fallo sin romper el flujo cuando NewsAPI no responde.
    public class NewsResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int TotalResults { get; set; }
        public List<NewsItemDto> Items { get; set; } = new();
    }
}
