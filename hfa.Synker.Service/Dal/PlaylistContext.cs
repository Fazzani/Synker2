namespace hfa.Synker.Service.Dal
{
    using hfa.Synker.Service.Entities.Playlists;
    using MongoDB.Driver;
    using System;

    public class PlaylistContext
    {
        private readonly IMongoDatabase _db;
        public PlaylistContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            //client.Settings.ConnectTimeout = new TimeSpan(0, 1, 0);
            _db = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Playlist> Playlists => _db.GetCollection<Playlist>(nameof(Playlists));

        public bool HealthCheck()
        {
            try
            {

                var dbNames = _db.Client.ListDatabaseNames();
                return true;
            }
            catch (Exception)
            {

                return false;
            }

        }
    }
}
