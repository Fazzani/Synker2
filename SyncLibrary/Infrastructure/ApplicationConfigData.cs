namespace Hfa.SyncLibrary.Infrastructure
{

    public class ConnectionStrings
    {
        public string PlDatabase { get; set; }
    }

    public class ApiOptions
    {
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }
    public class TvhOptions
    {
        public string TvhUrl { get; set; }
        public string TvhUserName { get; set; }
        public string TvhPassword { get; set; }
    }
}