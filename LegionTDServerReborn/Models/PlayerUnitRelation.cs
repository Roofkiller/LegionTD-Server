using System.ComponentModel.DataAnnotations.Schema;

namespace LegionTDServerReborn.Models
{
    public class PlayerUnitRelation
    {
        public long PlayerId { get; set; }
        public int MatchId { get; set; }
        public string UnitName { get; set; }
        [ForeignKey("MatchId,PlayerId")]
        public PlayerMatchData PlayerMatch { get; set; }
        [ForeignKey("UnitName")]
        public Unit Unit { get; set; }

        public int Killed { get; set; }
        public int Leaked { get; set; }
        public int Send { get; set; }
        public int Build { get; set; }

        public PlayerUnitRelation()
        {
            Killed = 0;
            Leaked = 0;
            Send = 0;
            Build = 0;
        }
    }
}
