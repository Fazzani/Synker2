using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.Brokers.Messages.Configuration
{
    public class RabbitMQConfiguration
    {
            public string Hostname { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
    }
}
