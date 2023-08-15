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
                    AlbumTitle = track.Album?.Title, 
                    ArtistName = track.Album?.Artist?.Name, 
                    IsFavorite = track.IsFavorite, 
                };

                playlistTracks.Add(playlistTrack);
            }

            return playlistTracks;
        }
       

        public async Task SetTrackAsUnfavorite(long trackId,string CurrentUserId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();

            var track = await dbContext.Tracks.FindAsync(trackId);

            if (track != null)
            {
                track.IsFavorite = false;
                await dbContext.SaveChangesAsync();
            }
        }
        public async Task SetTrackAsFavorite(long trackId, string CurrentUserId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();

            var track = await dbContext.Tracks.FindAsync(trackId);

            if (track != null)
            {
                track.IsFavorite = true;
                track.CurrentUserId = CurrentUserId;
                await dbContext.SaveChangesAsync();
            }
        }
        public async Task<List<PlaylistTrack>> GetTracksWithFavorite(string CurrentUserId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();
            List<PlaylistTrack> playlistTracks = new List<PlaylistTrack>();

            var tracks = await dbContext.Tracks
                .Where(t => t.IsFavorite == true && t.CurrentUserId == CurrentUserId)
                .ToListAsync();

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

        public async Task<List<Models.Playlist>> GetAllPlayLists(string CurrentUserId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();

            var playlists = await dbContext.Playlists
                .Where(p => p.CurrentUserId == CurrentUserId)  // Filter playlists by CurrentUserId
                .ToListAsync();

            return playlists;
        }
        public async Task<List<PlaylistTrack>> GetTracksWithPlayListId(long playListId, string CurrentUserId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();
            List<PlaylistTrack> playlistTracks = new List<PlaylistTrack>();

            var tracksList = await dbContext.Tracks.ToListAsync(); 

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



        public async Task AddTrackToPlaylist(long trackId, long selectedPlaylistId,string CurrentUserId)
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

        public async Task<long> AddNewPlaylist(string newPlaylistName,string CurrentUserId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();

            var playlist = new Models.Playlist
            {
                Name = newPlaylistName,
                CurrentUserId= CurrentUserId
            };

            dbContext.Playlists.Add(playlist);
            await dbContext.SaveChangesAsync();

            return playlist.PlaylistId; 
        }

        public async Task DeletePlaylist(long playlistId)
        {
            using var dbContext = await _dbFactory.CreateDbContextAsync();

            var playlistToRemove = await dbContext.Playlists.FindAsync(playlistId);

            if (playlistToRemove != null)
            {
                dbContext.Playlists.Remove(playlistToRemove);
                await dbContext.SaveChangesAsync();
            }
        }



    }
}
