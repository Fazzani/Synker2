namespace Hfa.SyncLibrary.Infrastructure
{
    internal class ApplicationConfigData
    {
        public string ApiUrlMessage { get; set; }

        public string ElasticUrl { get; set; }

        public string DefaultIndex { get; set; }
        public string ElasticUser { get; set; }
        public string ElasticPassword { get; set; }
    }
}