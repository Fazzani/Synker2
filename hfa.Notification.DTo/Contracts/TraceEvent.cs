using PlaylistManager.Entities;
using System;
using System.Collections.Generic;

namespace hfa.Brokers.Messages.Contracts
{
    public class TraceEvent : ApplicationEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Message { get; set; }

    }
}
