namespace PW.News8.Shared.DTOs
{
    public class SourceDownloadDto
    {
        public SourceDto Source { get; set; } = new();
        public List<SourceItemExportDto> Items { get; set; } = new();
        public DateTime ExportedAt { get; set; } = DateTime.Now;
    }

    /// Resultado de procesar un archivo subido en POST /api/sources/upload.
    /// Se modela como un objeto de resultado (en vez de excepciones) para que el
    /// controlador solo traduzca el "Status" a un código HTTP, sin contener lógica.
    public class SourceUploadResultDto
    {
        public UploadStatus Status { get; set; }
        public int? SourceId { get; set; }
        public int InsertedCount { get; set; }
        public int DuplicateCount { get; set; }
        public string Message { get; set; } = string.Empty;

        public static SourceUploadResultDto Success(int sourceId, int inserted, int duplicates) => new()
        {
            Status = UploadStatus.Success,
            SourceId = sourceId,
            InsertedCount = inserted,
            DuplicateCount = duplicates,
            Message = inserted > 0
                ? $"Se importaron {inserted} ítem(s) nuevo(s). Duplicados omitidos: {duplicates}."
                : $"No se importaron ítems nuevos. Duplicados omitidos: {duplicates}."
        };

        public static SourceUploadResultDto NoItems(int sourceId) => new()
        {
            Status = UploadStatus.NoNewItems,
            SourceId = sourceId,
            Message = "El archivo no contiene ítems para importar."
        };

        public static SourceUploadResultDto Invalid(string message) => new()
        {
            Status = UploadStatus.Invalid,
            Message = message
        };

        public static SourceUploadResultDto NotFound(int sourceId) => new()
        {
            Status = UploadStatus.SourceNotFound,
            SourceId = sourceId,
            Message = $"No existe una fuente con Id {sourceId}."
        };
    }

    /// Resultado semántico del procesamiento de un upload, sin acoplar a códigos HTTP.
    public enum UploadStatus
    {
        Success,
        NoNewItems,
        Invalid,
        SourceNotFound
    }
}