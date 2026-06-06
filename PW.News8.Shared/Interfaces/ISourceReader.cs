namespace PW.News8.Shared.Interfaces
{
    public class SourceReadResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<Dictionary<string, object>> Items { get; set; } = new();
    }

    public interface ISourceReader
    {
        // Tipo soportado: "json", "xml", "html"
        string SupportedType { get; }

        // Lee la URL y retorna items parseados
        Task<SourceReadResult> ReadAsync(string url, string? secret = null);
    }
}