namespace hfa.synker.entities.MediaServer
{
    using System;

    public class MediaServerStats
    {
        public Os Os { get; set; }
        public Cpu Cpu { get; set; }
        public Mem Mem { get; set; }
        public Net Net { get; set; }
        public Nodejs Nodejs { get; set; }
        public Clients Clients { get; set; }
    }

    public class Os
    {
        public string Arch { get; set; }
        public string Platform { get; set; }
        public string Release { get; set; }
    }

    public class Cpu
    {
        public int Num { get; set; }
        public int Load { get; set; }
        public string Model { get; set; }
        public int Speed { get; set; }
    }

    public class Mem
    {
        public long Totle { get; set; }
        public int Free { get; set; }
    }

    public class Net
    {
        public long Inbytes { get; set; }

        public string HumanInBytes => Inbytes.ByteSize();

        public long Outbytes { get; set; }
        public string HumanOutBytes => Outbytes.ByteSize();
    }

    public class Nodejs
    {
        public long Uptime { get; set; }
        public string Version { get; set; }
        public Mem1 Mem { get; set; }
    }

    public class Mem1
    {
        public long Rss { get; set; }
        public long HeapTotal { get; set; }
        public long HeapUsed { get; set; }
        public long External { get; set; }
    }

    public class Clients
    {
        public int Accepted { get; set; }
        public int Active { get; set; }
        public int Idle { get; set; }
        public int Rtmp { get; set; }
        public int Http { get; set; }
        public int Ws { get; set; }
    }

}
