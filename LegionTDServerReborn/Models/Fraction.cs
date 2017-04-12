using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LegionTDServerReborn.Models
{
    public class Fraction
    {
        [Key]
        public string Name { get; set; }
    }
}
