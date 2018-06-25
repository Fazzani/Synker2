namespace TvheadendLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    public interface IQuery
    {
        byte[] PostData { get; }
    }

    public class GenericQuery : IQuery
    {
        Dictionary<string, object> _data;
        public GenericQuery(Dictionary<string, object> data)
        {
            _data = data;
        }

        public byte[] PostData
        {
            get
            {
                var postData = new StringBuilder();
                foreach (var item in _data)
                    postData.AppendUrlEncoded(item.Key, item.Value as string);
                return Encoding.UTF8.GetBytes(postData.ToString());
            }
        }
    }
    public class QueryParams : IQuery
    {
        public QueryParams()
        {
            Start = 0;
            All = 1;
            Dir = "ASC";
            Limit = 20;
            Sort = "name";
            Columns = new List<string>();
        }
        public int Start { get; set; }
        public int Limit { get; set; }
        public string Sort { get; set; }
        public string Dir { get; set; }
        public int All { get; set; }

        /// <summary>
        /// Columns to display
        /// filter:[{"type":"string","value":"bein","field":"name"}]
        /// </summary>
        public List<string> Columns { get; set; }
        public List<IQueryParamsFilter> Filters { get; set; }

        public static QueryParams EPG_ALL { get { return new QueryParams { Start = 0, All = 1, Dir = "ASC", Limit = Int32.MaxValue, Sort = "updated" }; } }
        public static QueryParams ALL { get { return new QueryParams { Start = 0, All = 1, Dir = "ASC", Limit = Int32.MaxValue, Sort = "name" }; } }

        /// <summary>
        /// Create POST data and convert it to a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] PostData
        {
            get
            {
                // Create POST data and convert it to a byte array.
                var postData = new StringBuilder();
                postData.AppendUrlEncoded("start", Start.ToString());
                postData.AppendUrlEncoded("sort", Sort);
                postData.AppendUrlEncoded("limit", Limit.ToString());
                postData.AppendUrlEncoded("dir", Dir);
                postData.AppendUrlEncoded("all", All.ToString());
                if (Filters != null && Filters.Any())
                {
                    var filters = string.Join(",", Filters.Select(x => x.ToString()));
                    postData.AppendUrlEncoded("filter", $"[{filters}]");
                }
                return Encoding.UTF8.GetBytes(postData.ToString());
            }
        }

        public class NumericQueryParamsFilter : StringQueryParamsFilter
        {
            readonly string Comparaison;
            private readonly int Insplit;

            public NumericQueryParamsFilter(string field, string value, string comparaison, int insplit = 1_000_000) : base(field, value)
            {
                Comparaison = comparaison;
                Insplit = insplit;
                Value = (Convert.ToInt32(value) * insplit).ToString();
            }

            public override string ToString() => $"{{\"type\":\"{Type}\",\"comparaison\":\"{Comparaison}\",\"value\":\"{Value}\",\"intsplit\":\"{Insplit}\",\"field\":\"{Field}\"}}";
        }

        [Serializable]
        public class StringQueryParamsFilter: IQueryParamsFilter
        {
            public StringQueryParamsFilter(string field, string value, string type) : this(field, value)
            {
                Type = type;
            }

            public StringQueryParamsFilter(string field, string value)
            {
                Field = field;
                Value = value;
                Type = "string";
            }

            public readonly string Type;
            public readonly string Field;
            public string Value { get; protected set; }

            public override string ToString() => $"{{\"type\":\"{Type}\",\"value\":\"{Value}\",\"field\":\"{Field}\"}}";
        }
    }
}
