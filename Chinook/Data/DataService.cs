using Chinook.ClientModels;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Data
{
    public class DataService: IDataService
    {
        private readonly IDbContextFactory<ChinookContext> _dbFactory;

        public DataService(IDbContextFactory<ChinookContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Artist>> GetArtists()
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();
            return await dbContext.Artists.ToListAsync();
        }

        public async Task<Artist> GetArtist(long artistId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();
            return await dbContext.Artists.SingleOrDefaultAsync(a => a.ArtistId == artistId);
        }

        public async Task<List<PlaylistTrack>> GetTracksForArtist(long artistId, string userId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();
            List<PlaylistTrack> playlistTracks = new List<PlaylistTrack>();

            var tracks = await dbContext.Tracks
                .Where(t => t.Album.ArtistId == artistId)
                .Include(t => t.Album)
                .ToListAsync();

            foreach (var track in tracks)
            {
                PlaylistTrack playlistTrack = new PlaylistTrack
                {
                    TrackId = track.TrackId,
                    TrackName = track.Name,
                    AlbumTitle = track.Album?.Title, // Make sure Album has a Title property
                    ArtistName = track.Album?.Artist?.Name, // Make sure Artist is accessible via Album
                    IsFavorite = DetermineIfTrackIsFavorite(track, userId) // Implement your favorite logic
                };

                playlistTracks.Add(playlistTrack);
            }

            return playlistTracks;
        }
        private bool DetermineIfTrackIsFavorite(Track track, string userId)
        {
            // Implement your logic to determine if a track is a favorite for a given user
            // For now, I'm assuming you have a way to determine this based on your implementation
            return true;
        }
    }
}
