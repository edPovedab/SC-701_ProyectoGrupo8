namespace PW.News8.Shared.DTOs
{
    public class SourceItemDto
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string Json { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class SourceItemExportDto
    {
        public string SourceUrl { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public string ComponentType { get; set; } = string.Empty;
        public string Json { get; set; } = string.Empty;
        public DateTime ExportedAt { get; set; } = DateTime.Now;
    }
}
