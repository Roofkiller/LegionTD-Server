using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LegionTDServerReborn.Models
{
    public class Duel
    {
        public int MatchId { get; set; }
        [ForeignKey("MatchId"), JsonIgnore]
        public virtual Match Match { get; set; }
        public int Order { get; set; }
        public int Winner { get; set; }

        public float TimeStamp { get; set; }
    }
}
