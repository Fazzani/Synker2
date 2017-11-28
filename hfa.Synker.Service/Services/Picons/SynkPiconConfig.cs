using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

namespace hfa.Synker.Service.Services.Picons
{
    using System;
    using System.Linq;

    public class SynkPiconConfig
    {
        public string CommitsApiUrl => $"repos/{Author}/{Repo}/commits";
        public string BranchesApiUrl => $"repos/{Author}/{Repo}/branches";

        public string TreeApiUrl(string sha) => $"repos/{Author}/{Repo}/git/trees/{sha}";

        public string Author { get; set; } = "fazzani";
        public string Repo { get; set; } = "epg";

        public string ApiUrl { get; set; } = "https://api.github.com";
    }

    public class Picon : IEqualityComparer<Picon>
    {
        private string _id;
        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                    //_id = Url.Remove(0, Url.LastIndexOf('/') + 1);
                    _id = Path.Contains('.') ? Path.Remove(Path.LastIndexOf('.')) : Path;
                return _id;
            }
            set { _id = value; }
        }

        public string Path { get; set; }
        public string Url { get; set; }

        public string RawUrl => $"{Url.Remove(Url.IndexOf("git/blobs")).Replace("api.github.com/repos", "raw.githubusercontent.com")}master/logosChannels/{Path}";

        public string Name => Path.Remove(GetExtentionIndex(Path));

        public bool Equals(Picon x, Picon y)
        {
            if (x == null && y == null)
                return true;
            else if (x == null | y == null)
                return false;
            else if (x.Id == y.Id)
                return true;
            return false;
        }

        public int GetHashCode(Picon obj) => obj.Id.GetHashCode();

        private int GetExtentionIndex(string path)
        {
            var last = path.LastIndexOf('.');
            return last > 0 ? last : path.Length - 1;
        }

        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }

    public class GithubApiResponse
    {
        public string Url { get; set; }
        public string Sha { get; set; }

        public IEnumerable<Picon> Picons => Tree.Select(x => x.ToObject<Picon>());

        public bool Truncated { get; set; }

        public List<JObject> Tree { get; set; }


    }
}