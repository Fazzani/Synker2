namespace hfa.Brokers.Messages.Contracts
{
    using System;

    public class TraceEvent : ApplicationEvent
    {
        public string Message { get; set; }

        public string Level { get; set; } = LevelTrace.info;

        public override string ToString() => $"{CreatedDate}:{Level}: {Message}";

        public static class LevelTrace
        {
            public static string info;
            public static string warning;
            public static string error;
        }
    }
}
