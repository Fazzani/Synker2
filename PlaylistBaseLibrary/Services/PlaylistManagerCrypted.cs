using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using PlaylistManager.Services;
using System.Threading;
using System.Threading.Tasks;

namespace PlaylistBaseLibrary.Services
{
    public class PlaylistManagerCrypted : PlaylistDecorator
    {
        public PlaylistManagerCrypted(PlaylistManagerBase playlistManager) : base(playlistManager)
        {
        }

    }
}
