using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.Brokers.Messages
{
    public class PlaylistHealthState
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MediaCount { get; set; }
        public bool IsOnline { get; set; }

        public override string ToString() => $"{Name} => {IsOnline}";
    }
}
