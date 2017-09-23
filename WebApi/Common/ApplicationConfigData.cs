namespace hfa.WebApi.Common
{
    public class ApplicationConfigData
    {
        public string ElasticUrl { get; set; }

        public string DefaultIndex { get; set; }
        public string MessageIndex { get; set; }
        public string ElasticUserName { get; set; }
        public string ElasticPassword { get; set; }
    }
}