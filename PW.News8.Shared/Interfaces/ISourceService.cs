using PW.News8.Shared.DTOs;

namespace PW.News8.Shared.Interfaces
{
    /// <summary>
    /// Contrato de la lógica de negocio para lectura, descarga y carga de
    /// Sources / SourceItems. El controlador depende únicamente de esta
    /// interfaz (DIP) y nunca de los repositorios directamente.
    /// </summary>
    public interface ISourceService
    {
        /// <summary>Obtiene todas las fuentes registradas.</summary>
        Task<IEnumerable<SourceDto>> GetAllSourcesAsync(CancellationToken cancellationToken = default);

        /// <summary>Obtiene una fuente por Id. Retorna null si no existe.</summary>
        Task<SourceDto?> GetSourceByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los ítems de una fuente. Retorna null si la fuente no existe
        /// (para que el controlador distinga 404 de una lista vacía con 200).
        /// </summary>
        Task<IEnumerable<SourceItemDto>?> GetItemsBySourceIdAsync(int sourceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Construye el paquete de exportación (fuente + ítems) listo para serializar
        /// a JSON y descargar. Retorna null si la fuente no existe.
        /// </summary>
        Task<SourceDownloadDto?> GetSourceForDownloadAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Lee la fuente en vivo (JSON/XML/HTML según ComponentType) sin guardar nada.</summary>
        Task<SourceFetchResultDto> FetchSourceAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Guarda el ítem elegido por el usuario tras un fetch en vivo.</summary>
        Task<SourceItemDto?> SaveFetchedItemAsync(int sourceId, Dictionary<string, object> item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Procesa un paquete de importación: valida la fuente, descarta ítems
        /// duplicados (mismo contenido Json ya almacenado) e inserta el resto.
        /// </summary>
        Task<SourceUploadResultDto> UploadSourceItemsAsync(SourceDownloadDto payload, CancellationToken cancellationToken = default);
    }
}