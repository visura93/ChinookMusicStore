namespace Chinook.ClientModels;

public class Playlist
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<PlaylistTrack> Tracks { get; set; }
}