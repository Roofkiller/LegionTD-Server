using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LegionTDServerReborn.Models
{
    public class Duel
    {
        public int MatchId { get; set; }
        [ForeignKey("MatchId")]
        public Match Match { get; set; }
        public int Order { get; set; }
        public int Winner { get; set; }

        public float TimeStamp { get; set; }
    }
}
