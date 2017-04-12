using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LegionTDServerReborn.Models
{
    public class PlayerMatchData
    {
        public long PlayerId { get; set; }
        [ForeignKey("PlayerId")]
        public Player Player { get; set; }
        public int MatchId { get; set; }
        [ForeignKey("MatchId")]
        public Match Match { get; set; }

        public int Team { get; set; } 

        public bool Abandoned { get; set; }

        [InverseProperty("PlayerMatch")]
        public List<PlayerUnitRelation> UnitDatas { get; set; }

        public int RatingChange { get; set; }
        public int EarnedGold { get; set; }
        public int EarnedTangos { get; set; }
        public Fraction Fraction { get; set; }
        
        public bool Won => Match.Winner == Team && !Abandoned;
        public int WonDuels => Match.Duels.Count(d => d.Winner == Team);
        public int LostDuels => Match.Duels.Count(d => d.Winner != Team);
        public long Experience => UnitDatas.Sum(u => u.Killed * u.Unit.Experience);
        public long Kills => UnitDatas.Sum(u => u.Killed);
        public long Leaks => UnitDatas.Sum(u => u.Leaked);
        public long Builds => UnitDatas.Sum(u => u.Build);
        public long Sends => UnitDatas.Sum(u => u.Send);

        public int CalculateRatingChange()
        {
            //Playing against nobody should not grant or lose rating
            if (Match.IsTraining)
                return 0;
            var oldRating = Player.GetRatingBefore(Match.Date);
            var enemies = Match.PlayerDatas.Where(p => p.Team != Team)
                .Select(p => p.Player);
            var team = Match.PlayerDatas.Where(p => p.Team == Team)
                .Select(p => p.Player);
            var enemyAvg = enemies.Average(e => e.GetRatingBefore(Match.Date));
            var teamAvg = team.Average(t => t.GetRatingBefore(Match.Date));
            var ratio = enemyAvg / teamAvg;

            var change = (int)(Math.Atan(Won ? ratio : -1/ratio)/Math.PI*100);
            //Preventing you from hitting negative ratings
            if (oldRating + change < 0)
                change = -oldRating;
            return change;
        }
    }
}
