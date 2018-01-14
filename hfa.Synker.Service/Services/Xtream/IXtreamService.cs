using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using hfa.PlaylistBaseLibrary.Entities.XtreamCode;

namespace hfa.Synker.Service.Services.Xtream
{
    public interface IXtreamService
    {
        Task<List<Epg_Listings>> GetAllEpgAsync(string playlistUrl, CancellationToken cancellationToken);
        Task<List<Live>> GetLiveCategoriesAsync(string playlistUrl, CancellationToken cancellationToken);
        Task<List<Channels>> GetLiveStreamsAsync(string playlistUrl, CancellationToken cancellationToken);
        Task<List<Channels>> GetLiveStreamsByCategoriesAsync(string playlistUrl, string categoryId, CancellationToken cancellationToken);
        Task<XtreamPanel> GetPanelAsync(string playlistUrl, CancellationToken cancellationToken);
        Task<List<Epg_Listings>> GetShortEpgForStreamAsync(string playlistUrl, string streamId, CancellationToken cancellationToken);
        Task<PlayerApi> GetUserAndServerInfoAsync(string playlistUrl, CancellationToken cancellationToken);
        Task<List<Channels>> GetVodStreamsAsync(string playlistUrl, CancellationToken cancellationToken);
        Task<PlayerApi> GetXmltvAsync(string playlistUrl, CancellationToken cancellationToken);
    }
}