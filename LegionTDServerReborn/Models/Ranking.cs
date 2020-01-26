using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LegionTDServerReborn.Models
{
    public enum RankingTypes
    {
        Rating,
        EarnedTangos,
        EarnedGold,
        WinRate,
        DuelWinRate,
        TangosPerMinute,
        GoldPerMinute,
        WonGames,
        Invalid
    }

    public class Ranking
    {
        public RankingTypes Type { get; set; }
        public int Position { get; set; }
        public long PlayerId { get; set; }
        [ForeignKey("PlayerId")]
        public Player Player { get; set; }
        public bool Ascending { get; set; }

        public Ranking()
        {
        }
    }
}
