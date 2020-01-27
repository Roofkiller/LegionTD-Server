using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegionTDServerReborn.Models
{
    public class Fraction
    {
        [Key]
        public string Name { get; set; }
        [InverseProperty("Fraction")]
        public virtual List<PlayerMatchData> PlayedMatches {get; set;}
        [InverseProperty("Fraction")]
        public virtual List<Unit> Units {get; set;}
        [InverseProperty("Fraction")]
        public virtual List<FractionStatistic> Statistics {get; set;}
        public virtual FractionStatistic CurrentStatistic => Statistics.OrderByDescending(s => s.TimeStamp).FirstOrDefault();
    }
}
