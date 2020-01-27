using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.Extensions;

namespace LegionTDServerReborn.Models
{
    public class Player
    {
        public const int DefaultRating = 2500;
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long SteamId { get; set; }
        [InverseProperty("Player")]
        public virtual List<PlayerMatchData> Matches { get; set; }
        [InverseProperty("Player")]
        public virtual List<Ranking> Rankings {get; set;}
        [Column(TypeName = "VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci")]
        public string PersonaName {get; set;}
        public string Avatar {get; set;}
        [Column(TypeName = "VARCHAR(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci")]
        public string RealName {get; set;}
        [Column(TypeName = "VARCHAR(511) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci")]
        public string ProfileUrl {get; set;}
        public Ranking Ranking => Rankings.FirstOrDefault(r => r.Type == RankingTypes.Rating);
        public int Rating => DefaultRating + Matches.Sum(m => m.RatingChange);
        public float TimePlayed => Matches.Where(m => !m.Match.IsTraining).Sum(m => m.Match.Duration);
        public long EarnedTangos => Matches.Where(m => !m.Match.IsTraining).Sum(m => m.EarnedTangos);
        public float TangosPerMinute => (EarnedTangos / TimePlayed).NaNToZero();
        public long EarnedGold => Matches.Where(m => !m.Match.IsTraining).Sum(m => m.EarnedGold);
        public float GoldPerMinute => (EarnedGold / TimePlayed).NaNToZero();
        public int PlayedGames => Matches.Count(m => !m.Match.IsTraining);
        public int WonGames => Matches.Count(m => m.Won && !m.Match.IsTraining);
        public int LostGames => Matches.Count(m => !m.Won && !m.Match.IsTraining);
        public float WinRate => (WonGames / (float)PlayedGames).NaNToZero();
        public int WonDuels => Matches.Where(m => !m.Match.IsTraining).Sum(m => m.WonDuels);
        public int LostDuels => Matches.Where(m => !m.Match.IsTraining).Sum(m => m.LostDuels);
        public int PlayedDuels => Matches.Where(m => !m.Match.IsTraining).Sum(m => m.WonDuels + m.LostDuels);
        public float DuelWinRate => (WonDuels / (float)PlayedDuels).NaNToZero();
        public long Experience => Matches.Where(m => !m.Match.IsTraining).Sum(m => m.Experience);
        public long Kills => Matches.Where(m => !m.Match.IsTraining).Sum(m => m.Kills);
        public long Leaks => Matches.Where(m => !m.Match.IsTraining).Sum(m => m.Leaks);
        public long Sends => Matches.Where(m => !m.Match.IsTraining).Sum(m => m.Sends);
        public long Builds => Matches.Where(m => !m.Match.IsTraining).Sum(m => m.Builds);

        public Player()
        {
        }

        public int GetRatingBefore(DateTime date)
        {
            return DefaultRating + Matches.Where(m => m.Match.Date < date).Sum(m => m.RatingChange);
        }

        // public Dictionary<string, long> FractionKills
        // {
        //     get
        //     {
        //         Dictionary<string, long> result = new Dictionary<string, long>();
        //         // foreach (var matchData in MatchDatas.Where(m => !m.Match.IsTraining))
        //         // {
        //         //     foreach (var unitData in matchData.UnitDatas)
        //         //     {
        //         //         string key = KilledPrefix + unitData.Unit.Fraction.Name;
        //         //         if (!result.ContainsKey(key))
        //         //             result[key] = unitData.Killed;
        //         //         else
        //         //             result[key] += unitData.Killed;
        //         //     }
        //         // }
        //         return result;
        //     }
        // }

        private const string KilledPrefix = "killed_";
        private const string PlayedPrefix = "played_";
        public Dictionary<string, int> PlayedFractions
        {
            get
            {
                Dictionary<string, int> result = new Dictionary<string, int>();
                foreach (var m in Matches.Where(m => !m.Match.IsTraining))
                    result[PlayedPrefix + m.Fraction.Name] = result.ContainsKey(PlayedPrefix + m.Fraction.Name) 
                                                ? result[PlayedPrefix + m.Fraction.Name] + 1 
                                                : 1;
                return result;
            }
        }

        public Dictionary<string, string> DataToDict()
        {
            Dictionary<string, string> result = new Dictionary<string, string>
            {
                ["experience"] = Experience.ToString(),
                ["kills"] = Kills.ToString(),
                ["leaks"] = Leaks.ToString(),
                ["builds"] = Builds.ToString(),
                ["sends"] = Sends.ToString(),
                ["earned_tangos"] = EarnedTangos.ToString(),
                ["won_games"] = WonGames.ToString(),
                ["won_duels"] = WonDuels.ToString(),
                ["lost_games"] = LostGames.ToString(),
                ["lost_duels"] = LostDuels.ToString(),
                ["win_rate"] = WinRate.ToString(),
                ["duel_win_rate"] = DuelWinRate.ToString(),
                ["rating"] = Rating.ToString(),
                ["earned_gold"] = EarnedGold.ToString(),
                ["gold_per_minute"] = GoldPerMinute.ToString(),
                ["tangos_per_minute"] = TangosPerMinute.ToString(),
                ["time_played"] = TimePlayed.ToString()
            };
            // foreach (var data in FractionDatas) {
            //     result[KilledPrefix + data.FractionName] = data.Killed.ToString();
            //     result[PlayedPrefix + data.FractionName] = data.Played.ToString();
            // }
            foreach (var pair in PlayedFractions)
                result[pair.Key] = pair.Value.ToString();
            // foreach (var pair in FractionKills)
            //     result[pair.Key] = pair.Value.ToString();
            return result;
        }
    }
}
