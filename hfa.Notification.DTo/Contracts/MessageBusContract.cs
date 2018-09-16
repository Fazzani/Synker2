namespace hfa.Brokers.Messages.Contracts
{
    using MassTransit;
    using System;

    public class ApplicationEvent : CorrelatedBy<Guid>
    {
        public ApplicationEvent()
        {
            CreatedDate = DateTime.UtcNow;
        }

        public DateTime CreatedDate { get; set; }

        public Guid CorrelationId => Guid.NewGuid();
    }
}