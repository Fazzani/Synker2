namespace hfa.Brokers.Messages.Contracts
{
    using NETCore.Encrypt;

    public class TraceEvent : ApplicationEvent
    {
        private string _userEmailHash;

        public string UserIdHash
        {
            get
            {
                if (string.IsNullOrEmpty(_userEmailHash))
                {
                    _userEmailHash = EncryptProvider.Md5(UserId);
                }
                return _userEmailHash;
            }
        }

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
