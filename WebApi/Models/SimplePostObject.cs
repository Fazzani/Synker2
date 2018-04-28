using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hfa.WebApi.Models
{
    public class SimpleModelPost
    {
        [Required]
        public string Value { get; set; }
    }
}
