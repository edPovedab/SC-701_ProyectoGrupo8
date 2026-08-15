namespace PW.News8.Shared.DTOs
{
    public class ApwImportItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string PublishedAt { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string SourceDescription { get; set; } = string.Empty;
        public string SourceComponentType { get; set; } = string.Empty;
        public bool SourceRequiresSecret { get; set; }
    }
}