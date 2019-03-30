namespace hfa.Brokers.Messages.Contracts
{
    public class TraceEvent : ApplicationEvent
    {
        public string UserId { get; set; }

        public string Message { get; set; }

        public string Level { get; set; } = LevelTrace.Info;

        public override string ToString() => $"{CreatedDate}:{Level}: UserId: {UserId} => {Message}";

        public static class LevelTrace
        {
            public static string Info = "info";
            public static string Warning = "warning";
            public static string Error = "error";
        }
    }
}
