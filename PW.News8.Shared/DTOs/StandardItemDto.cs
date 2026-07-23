using System.Text.Json.Serialization;

namespace PW.News8.Shared.DTOs
{
    //dto standar para exportar e importar items de fuentes, contiene los campos basicos y comunes a todas las fuentes  
    public class StandardItemDto
    {
        [JsonPropertyName("exported_at")]
        public DateTime ExportedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("source_name")]
        public string SourceName { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();
    }
}