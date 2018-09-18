using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.PlaylistBaseLibrary.Options
{
    public class PlaylistProviderOption
    {
      public  const string PlaylistProvidersConfigurationKeyName = "PlaylistProviders";
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
