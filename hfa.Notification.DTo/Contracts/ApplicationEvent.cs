namespace hfa.Brokers.Messages.Contracts
{
    using MassTransit;
    using System;

    public class ApplicationEvent : CorrelatedBy<Guid>
    {
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public Guid CorrelationId => Guid.NewGuid();
    }
}