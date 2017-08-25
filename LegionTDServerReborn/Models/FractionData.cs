using LegionTDServerReborn.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegionTDServerReborn.Models
{
    public class FractionData {
        [ForeignKey("MatchId,PlayerId")]
        public PlayerMatchData PlayerMatch {get; set;}
        public long PlayerId {get; set;}
        public int MatchId {get; set;}
        [ForeignKey("FractionName")]
        public Fraction Fraction {get; set;}
        public string FractionName {get; set;}
        public long Killed {get; set;}
        public long Played {get; set;}
    }
}