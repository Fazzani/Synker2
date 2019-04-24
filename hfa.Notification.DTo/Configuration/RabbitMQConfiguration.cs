using System.ComponentModel.DataAnnotations;
namespace hfa.Brokers.Messages.Configuration
{
    public class RabbitMQConfiguration
    {
        [Required]
        public string Hostname { get; set; }
        public ushort Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
    }
}
