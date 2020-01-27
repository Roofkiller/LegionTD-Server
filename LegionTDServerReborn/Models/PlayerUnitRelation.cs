using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace LegionTDServerReborn.Models
{
    public class PlayerUnitRelation
    {
        public long PlayerId { get; set; }
        public int MatchId { get; set; }
        public string UnitName { get; set; }
        [ForeignKey("MatchId,PlayerId"), JsonIgnore]
        public virtual PlayerMatchData PlayerMatch { get; set; }
        [ForeignKey("UnitName")]
        public virtual Unit Unit { get; set; }

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
