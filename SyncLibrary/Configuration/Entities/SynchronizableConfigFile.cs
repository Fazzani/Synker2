using System;
using System.Collections.Generic;
using System.Text;

namespace SyncLibrary.Configuration.Entites
{
    class SynchronizableConfigFile: ISynchronizableConfig
    {
        public SynchronizableConfigFile()
        {

        }
        public SynchronizableConfigFile(string path)
        {
            Path = path;
        }
        public string  Path { get; set; }
        public SynchronizableConfigType SynchConfigType { get ; set; }

        public override string ToString() => Path;
    }

    class SynchronizableConfigUri : ISynchronizableConfig
    {
        public SynchronizableConfigUri()
        {

        }
        public SynchronizableConfigUri(string url)
        {
            Uri = new Uri(url);
        }
        public SynchronizableConfigUri(Uri uri)
        {
            Uri = uri;
        }
        public Uri Uri { get; set; }
        public SynchronizableConfigType SynchConfigType { get ; set; }

        public override string ToString() => Uri.ToString();
            
    }

    class SynchronizableConfigTvh : SynchronizableConfigConnected , ISynchronizableConfig
    {
    }

    class SynchronizableConfigElastic : SynchronizableConfigConnected, ISynchronizableConfig
    {
    }
}
