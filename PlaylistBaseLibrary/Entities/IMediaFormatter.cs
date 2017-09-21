using PlaylistManager.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlaylistBaseLibrary.Entities
{
    public interface IMediaFormatter
    {
        string Format(Media media);
    }
}
