using PW.News8.Shared.DTOs;

namespace PW.News8.Shared.Interfaces
{
    /// Contrato de la lógica de negocio para lectura, descarga y carga de
    /// Sources / SourceItems. El controlador depende únicamente de esta
    /// interfaz (DIP) y nunca de los repositorios directamente.

    public interface ISourceService
    {
        /// Obtiene todas las fuentes registradas.
        Task<IEnumerable<SourceDto>> GetAllSourcesAsync(CancellationToken cancellationToken = default);

        /// Obtiene una fuente por Id. Retorna null si no existe.
        Task<SourceDto?> GetSourceByIdAsync(int id, CancellationToken cancellationToken = default);


        /// Obtiene los ítems de una fuente. Retorna null si la fuente no existe
        /// (para que el controlador distinga 404 de una lista vacía con 200).
  
        Task<IEnumerable<SourceItemDto>?> GetItemsBySourceIdAsync(int sourceId, CancellationToken cancellationToken = default);


        /// Construye el paquete de exportación (fuente + ítems) listo para serializar
        /// a JSON y descargar. Retorna null si la fuente no existe.

        Task<SourceDownloadDto?> GetSourceForDownloadAsync(int id, CancellationToken cancellationToken = default);

        /// Lee la fuente en vivo (JSON/XML/HTML según ComponentType) sin guardar nada.
        Task<SourceFetchResultDto> FetchSourceAsync(int id, CancellationToken cancellationToken = default);

        /// Guarda el ítem elegido por el usuario tras un fetch en vivo.
        Task<SourceItemDto?> SaveFetchedItemAsync(int sourceId, Dictionary<string, object> item, CancellationToken cancellationToken = default);


        /// Procesa un paquete de importación: valida la fuente, descarta ítems
        /// duplicados (mismo contenido Json ya almacenado) e inserta el resto.

        Task<SourceUploadResultDto> UploadSourceItemsAsync(SourceDownloadDto payload, CancellationToken cancellationToken = default);
    }
}