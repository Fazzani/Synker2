﻿namespace hfa.Brokers.Messages.Contracts
{
    using MassTransit;
    using System;

    public class ApplicationEvent : CorrelatedBy<Guid>
    {
        public DateTime CreatedDate
        {
            get
            {
                return UnixTimestampToDateTime(UnixTimestamp);
            }
        }

        public int UnixTimestamp { get; set; } = DateTime.UtcNow.ToUnixTimestamp();

        public Guid CorrelationId => Guid.NewGuid();

        public static DateTime UnixTimestampToDateTime(double unixTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
        }
    }
}