namespace Hfa.WebApi.Commmon
{
    internal class CacheKeys
    {
        public const string CulturesKey = nameof(CulturesKey);
        public const string SitesKey = nameof(SitesKey);
        public const string PlaylistByUser = nameof(PlaylistByUser);

        public static object SitePacksKey { get; internal set; }
    }
}