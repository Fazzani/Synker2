using Newtonsoft.Json;
using PlaylistBaseLibrary.Common.Security;
using SyncLibrary.Configuration.Entites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SyncLibrary.Configuration
{
    class SynchronizableConfigManager
    {
        private const string ENCRYPT_PASSWORD = "playlistslibrary";

        public static async Task<ConfigSync> Load(string filePath)
        {
            using (var sr = new StreamReader(filePath))
            {
                string dataJson = await sr.ReadToEndAsync();

                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                return JsonConvert.DeserializeObject<ConfigSync>(dataJson, settings);
            }
        }

        public static async Task<ConfigSync> LoadEncryptedConfig(string filePath, string certificateName)
        {
            using (var sr = new StreamReader(filePath))
            {
                string dataJsonEncrypted = await sr.ReadToEndAsync();
                string dataJson = Cryptography.DecryptString(dataJsonEncrypted, ENCRYPT_PASSWORD, certificateName);
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                return JsonConvert.DeserializeObject<ConfigSync>(dataJson, settings);
            }
        }

        public static async Task Save(ConfigSync configSync, string filePath)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            string strJson = JsonConvert.SerializeObject(configSync, settings);
            using (var sw = File.CreateText(filePath))
            {
                await sw.WriteAsync(strJson);
            }
        }

        public static async Task SaveAndEncrypt(ConfigSync configSync, string filePath, string certificateName)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            string strJson = JsonConvert.SerializeObject(configSync, settings);
            string dataJson = Cryptography.EncryptString(strJson, ENCRYPT_PASSWORD, certificateName);

            using (var sw = File.CreateText(filePath))
            {
                await sw.WriteAsync(dataJson);
            }
        }

    }
}
