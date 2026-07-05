using PW.News8.Shared.DTOs;

namespace PW.News8.Web.Services
{
    public interface ISourceApiService
    {
        Task<List<SourceDto>> GetSourcesAsync(CancellationToken cancellationToken = default);

        Task<SourceDto?> GetSourceAsync(int id, CancellationToken cancellationToken = default);

        Task<List<SourceItemDto>?> GetItemsAsync(int sourceId, CancellationToken cancellationToken = default);

        Task<SourceFetchResultDto> FetchLiveAsync(int sourceId, CancellationToken cancellationToken = default);

        Task<SourceItemDto?> SaveFetchedItemAsync(int sourceId, Dictionary<string, object> item, CancellationToken cancellationToken = default);

        Task<(byte[] Content, string FileName)?> DownloadSourceAsync(int sourceId, CancellationToken cancellationToken = default);

        Task<SourceUploadResultDto> UploadSourceAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    }
}