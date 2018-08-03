using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.synker.entities.MediaServer
{
    public class MediaServerLiveResponse
    {
        public string StreamUrl { get; set; }
        public string StreamId { get; set; }

        public string Commandline { get; set; }

        //public Command Command { get; set; }

    }

    public class MediaServerStopLiveResponse
    {
        public string StreamId { get; set; }

        public string Status { get; set; }
    }

    public class Command
    {
        public object Domain { get; set; }
        public Events _events { get; set; }
        public int _eventsCount { get; set; }
        public Inputs[] _inputs { get; set; }
        public Currentinput _currentInput { get; set; }
        public Outputs[] _outputs { get; set; }
        public Currentoutput _currentOutput { get; set; }
        public Options Options { get; set; }
        public object Logger { get; set; }
    }

    public class Events
    {
    }

    public class Currentinput
    {
        public string Source { get; set; }
        public bool IsFile { get; set; }
        public bool IsStream { get; set; }
    }

    public class Currentoutput
    {
        public bool IsFile { get; set; }
        public object Flags { get; set; }
        public object Pipeopts { get; set; }
        public Sizedata SizeData { get; set; }
        public string Target { get; set; }
    }

    public class Sizedata
    {
        public string Size { get; set; }
        public float Aspect { get; set; }
    }

    public class Options
    {
        public string Source { get; set; }
        public int StdoutLines { get; set; }
        public string Presets { get; set; }
        public int Niceness { get; set; }
    }

    public class Inputs
    {
        public string Source { get; set; }
        public bool IsFile { get; set; }
        public bool IsStream { get; set; }
    }

    public class Outputs
    {
        public bool IsFile { get; set; }
        public object Flags { get; set; }
        public object Pipeopts { get; set; }
        public Sizedata SizeData { get; set; }
        public string Target { get; set; }
    }

}