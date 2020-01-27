using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegionTDServerReborn.Models
{
    public class Match
    {
        public int MatchId { get; set; }
        public DateTime Date { get; set; }
        public bool IsTraining { get; set; }
        public int Winner { get; set; }
        public int LastWave { get; set; }
        public float Duration { get; set; }
        [InverseProperty("Match")]
        public virtual List<PlayerMatchData> PlayerData { get; set; }
        [InverseProperty("Match")]
        public virtual List<Duel> Duels { get; set; }

        public Match() { }

        public Match(int winner)
        {
            Winner = winner;
        }
    }
}
