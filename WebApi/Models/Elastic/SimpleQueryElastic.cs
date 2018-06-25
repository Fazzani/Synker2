namespace hfa.WebApi.Models.Elastic
{
    public class SimpleQueryElastic
    {
        public int From { get; set; }
        public int Size { get; set; }
        public string Query { get; set; }
        public string IndexName { get; set; }
    }
}
