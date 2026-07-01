namespace PW.News8.Shared.DTOs
{
    /// <summary>
    /// Resultado de leer una fuente en vivo (GET /api/sources/{id}/fetch).
    /// No se persiste nada todavía: el usuario ve estos ítems y decide cuál
    /// guardar mediante POST /api/sources/{id}/items.
    /// </summary>
    public class SourceFetchResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int SourceId { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public List<Dictionary<string, object>> Items { get; set; } = new();
    }

    /// <summary>Payload para guardar el ítem que el usuario eligió del fetch en vivo.</summary>
    public class SaveFetchedItemDto
    {
        public Dictionary<string, object> Item { get; set; } = new();
    }
}