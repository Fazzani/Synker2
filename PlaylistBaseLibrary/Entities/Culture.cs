﻿using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.PlaylistBaseLibrary.Entities
{
    [Serializable]
    public class Culture
    {
        public string Code { get; set; }
        public string Country { get; set; }

        public override string ToString() => $"{Code} : {Country}";
    }

    [Serializable]
    public class TvgSource : Culture
    {
        public string Site { get; set; }

        public override string ToString() => $"{Site} : {base.ToString()}";
    }

}