using System;

namespace hfa.Brokers.Messages.Contracts
{
    public class TraceEvent : ApplicationEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Message { get; set; }

        public override string ToString() =>  $"{CreatedDate}: {Message}";
    }
}
