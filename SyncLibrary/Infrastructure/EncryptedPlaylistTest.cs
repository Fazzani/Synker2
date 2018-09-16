namespace ConsoleAppTest
{
    using hfa.PlaylistBaseLibrary.Providers;
    using PlaylistManager.Entities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    internal class EncryptedPlaylistTest
    {
        public const string HeaderFile = "#EXTM3U";
        public const string HeaderUrl = "#EXTINF:";
        private const string SYMMETRIC_KEY = "ABCDEFGH";

        static Playlist<TvgMedia> GetPlaylistCrypted(string filePath, bool forRead = true)
        {
            //CryptoStream crStream = GetStream(filePath, forRead);
            return new Playlist<TvgMedia>(new M3uProvider(new Uri(filePath))) { Name = "tv_channels_crypted", Id = 0 };
        }

        private static CryptoStream GetStream(string filePath, bool forRead = true)
        {
            var cryptic = new DESCryptoServiceProvider
            {
                Key = ASCIIEncoding.ASCII.GetBytes(SYMMETRIC_KEY),
                IV = ASCIIEncoding.ASCII.GetBytes(SYMMETRIC_KEY)
            };

            var m3uFile3 = new FileStream(filePath, FileMode.OpenOrCreate, forRead ? FileAccess.Read : FileAccess.Write, FileShare.Read);
            var crStream = new CryptoStream(m3uFile3, forRead ? cryptic.CreateDecryptor() : cryptic.CreateEncryptor(), forRead ? CryptoStreamMode.Read : CryptoStreamMode.Write);
            return crStream;
        }

    }
}
