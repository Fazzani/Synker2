namespace hfa.Synker.Service.Entities
{
    using hfa.synker.entities;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Host : EntityBase
    {
        public Host()
        {
            Authentication = new Authentication();
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        public Authentication Authentication { get; set; }

        public string Port { get; set; }

        public Uri AddressUri => new Uri($"http://{Address}:{Port}");

        public string Comments { get; set; }

        public bool Enabled { get; set; } = true;

        public virtual ICollection<WebGrabConfigDocker> WebGrabConfigDockers { get; set; } = new List<WebGrabConfigDocker>();
    }

    public class Authentication
    {
        public string CertPath { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
