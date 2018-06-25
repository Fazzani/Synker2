namespace PlaylistBaseLibrary.Entities
{
    using PlaylistManager.Entities;

    public interface IMediaFormatter
    {
        string Format(Media media);
    }
}
