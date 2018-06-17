using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models.Notifications
{
    public class WebPushModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Payload { get; set; }
    }
}
