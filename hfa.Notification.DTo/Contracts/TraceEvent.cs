namespace hfa.Brokers.Messages.Contracts
{
    using System;

    public class TraceEvent : ApplicationEvent
    {
        public string Message { get; set; }

        public override string ToString() =>  $"{CreatedDate}: {Message}";
    }
}
