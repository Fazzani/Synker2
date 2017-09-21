using System;

namespace SyncLibrary.Configuration.Entites
{
    public class ConnectionData
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public Uri ServerUri { get; set; }
    }
}