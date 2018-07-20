using System;

namespace hfa.Brokers.Messages.Contracts
{
    public class ApplicationEvent
    {
        public ApplicationEvent()
        {
            CreatedDate = DateTime.UtcNow;
        }

        public DateTime CreatedDate
        {
            get; set;

        }
        private Guid CorrelationId { get; }
    }
}