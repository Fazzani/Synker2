namespace hfa.Synker.Service.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;

    public class Host : EntityBase
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        public Authentication Authentication { get; set; }

        public string Port { get; set; }

        public Uri AdressUri => new Uri($"http://{Address}:{Port}");

        public string Comments { get; set; }

        public bool Enabled { get; set; } = true;
    }

    public class Authentication
    {
        public string CertPath { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
