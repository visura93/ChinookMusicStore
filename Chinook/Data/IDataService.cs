using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.Data
{
    public interface IDataService
    {
        Task<List<Artist>> GetArtists();
        Task<Artist> GetArtist(long artistId);
        Task<List<PlaylistTrack>> GetTracksForArtist(long artistId, string userId);
    }
}
