using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LegionTDServerReborn.Models
{
    public class PlayerMatchData
    {
        public long PlayerId { get; set; }
        [ForeignKey("PlayerId"), JsonIgnore]
        public Player Player { get; set; }
        public int MatchId { get; set; }
        [ForeignKey("MatchId"), JsonIgnore]
        public Match Match { get; set; }

        public int Team { get; set; } 

        public bool Abandoned { get; set; }

        [InverseProperty("PlayerMatch")]
        public List<PlayerUnitRelation> UnitDatas { get; set; }
        [InverseProperty("PlayerMatch")]
        public List<FractionData> FractionDatas {get;set;}
        public int RatingChange { get; set; }
        public int EarnedGold { get; set; }
        public int EarnedTangos { get; set; }
        [ForeignKey("FractionName")]
        public Fraction Fraction { get; set; }
        public string FractionName {get; set;}        
        public bool Won { get; set; }
        public int WonDuels { get; set; }
        public int LostDuels { get; set; }
        public int Experience { get; set; }
        public int Kills { get; set; }
        public int Leaks { get; set; }
        public int Builds { get; set; }
        public int Sends { get; set; }

        public void CalculateStats() {
            Won = Match.Winner == Team && !Abandoned;
            WonDuels = Match.Duels.Count(d => d.Winner == Team);
            LostDuels = Match.Duels.Count(d => d.Winner != Team);
            Experience = UnitDatas.Sum(u => u.Killed * u.Unit.Experience);
            Kills = UnitDatas.Sum(u => u.Killed);
            Leaks = UnitDatas.Sum(u => u.Leaked);
            Builds = UnitDatas.Sum(u => u.Build);
            Sends = UnitDatas.Sum(u => u.Send);
        }

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
