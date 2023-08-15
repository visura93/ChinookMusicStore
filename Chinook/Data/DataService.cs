using Chinook.ClientModels;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Security.AccessControl;

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
                    IsFavorite = track.IsFavorite, // Implement your favorite logic
                };

                playlistTracks.Add(playlistTrack);
            }

            return playlistTracks;
        }
       

        public async Task SetTrackAsUnfavorite(long trackId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();

            var track = await dbContext.Tracks.FindAsync(trackId);

            if (track != null)
            {
                track.IsFavorite = false;
                await dbContext.SaveChangesAsync();
            }
        }
        public async Task SetTrackAsFavorite(long trackId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();

            var track = await dbContext.Tracks.FindAsync(trackId);

            if (track != null)
            {
                track.IsFavorite = true;
                await dbContext.SaveChangesAsync();
            }
        }
        public async Task<List<PlaylistTrack>> GetTracksWithFavorite()
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();
            List<PlaylistTrack> playlistTracks = new List<PlaylistTrack>();

            var tracks = await dbContext.Tracks
                .Where(t => t.IsFavorite == true)
                .ToListAsync();

            foreach (var track in tracks)
            {
                PlaylistTrack playlistTrack = new PlaylistTrack
                {
                    TrackId = track.TrackId,
                    TrackName = track.Name,
                    AlbumTitle = track.Album?.Title, // Make sure Album has a Title property
                    ArtistName = track.Album?.Artist?.Name, // Make sure Artist is accessible via Album
                    IsFavorite = track.IsFavorite, // Implement your favorite logic
                };

                playlistTracks.Add(playlistTrack);
            }

            return playlistTracks;
        }

        public async Task<List<Models.Playlist>> GetAllPlayLists()
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();
            List<PlaylistTrack> playlistTracks = new List<PlaylistTrack>();

            var tracks = await dbContext.Playlists
                .ToListAsync();

            //foreach (var track in tracks)
            //{
            //    PlaylistTrack playlistTrack = new PlaylistTrack
            //    {
            //        TrackId = track.TrackId,
            //        TrackName = track.Name,
            //        AlbumTitle = track.Album?.Title, // Make sure Album has a Title property
            //        ArtistName = track.Album?.Artist?.Name, // Make sure Artist is accessible via Album
            //        IsFavorite = track.IsFavorite, // Implement your favorite logic
            //    };

            //    playlistTracks.Add(playlistTrack);
            //}

            return tracks;
        }

        public async Task<List<PlaylistTrack>> GetTracksWithPlayListId(long playListId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();
            List<PlaylistTrack> playlistTracks = new List<PlaylistTrack>();

            var tracksList = await dbContext.Tracks.ToListAsync(); // Fetch all tracks

            var tracks = tracksList
                .Where(t => t.PlaylistIds != null && t.PlaylistIds.Split(',').Any(id => id == playListId.ToString()));

            foreach (var track in tracks)
            {
                PlaylistTrack playlistTrack = new PlaylistTrack
                {
                    TrackId = track.TrackId,
                    TrackName = track.Name,
                    AlbumTitle = track.Album?.Title,
                    ArtistName = track.Album?.Artist?.Name,
                    IsFavorite = track.IsFavorite,
                };

                playlistTracks.Add(playlistTrack);
            }

            return playlistTracks;
        }



        public async Task AddTrackToPlaylist(long trackId, long selectedPlaylistId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();

            var track = await dbContext.Tracks.FindAsync(trackId);

            if (track != null)
            {
                if (track.PlaylistIds != null)
                {
                    track.PlaylistIds += "," + selectedPlaylistId.ToString();
                }
                else
                {
                    track.PlaylistIds = selectedPlaylistId.ToString();
                }

                await dbContext.SaveChangesAsync();
            }
        }
        public async Task RemoveTrackFromPlaylist(long trackId, long selectedPlaylistId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();

            var track = await dbContext.Tracks.FindAsync(trackId);

            if (track != null && track.PlaylistIds != null)
            {
                var playlistIdsList = track.PlaylistIds.Split(',').ToList();
                playlistIdsList.Remove(selectedPlaylistId.ToString());
                track.PlaylistIds = string.Join(",", playlistIdsList);

                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<long> AddNewPlaylist(string newPlaylistName)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();

            var playlist = new Models.Playlist
            {
                Name = newPlaylistName
            };

            dbContext.Playlists.Add(playlist);
            await dbContext.SaveChangesAsync();

            return playlist.PlaylistId; // Return the newly created PlaylistId
        }




    }
}
