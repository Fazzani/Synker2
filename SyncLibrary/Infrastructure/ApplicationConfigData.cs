namespace Hfa.SyncLibrary.Infrastructure
{

    public class ConnectionStrings
    {
        public string PlDatabase { get; set; }
    }

    public class ApiOptions
    {
        public string ApiUrlMessage { get; set; }
        public string ApiUserName { get; set; }
        public string ApiPassword { get; set; }

    }
    public class TvhOptions
    {
        public string TvhUrl { get; set; }
        public string TvhUserName { get; set; }
        public string TvhPassword { get; set; }
    }
}