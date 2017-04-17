using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.Extension;

namespace LegionTDServerReborn.Models
{
    public class Player
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long SteamId { get; set; }
        [InverseProperty("Player")]
        public List<PlayerMatchData> MatchDatas { get; set; }

        public int Rating => 1000 + MatchDatas.Where(m => !m.Match.IsTraining).Sum(m => m.RatingChange);
        public float TimePlayed => MatchDatas.Where(m => !m.Match.IsTraining).Sum(m => m.Match.Duration);
        public long EarnedTangos => MatchDatas.Where(m => !m.Match.IsTraining).Sum(m => m.EarnedTangos);
        public float TangosPerMinute => (EarnedTangos / TimePlayed).NaNToZero();
        public long EarnedGold => MatchDatas.Where(m => !m.Match.IsTraining).Sum(m => m.EarnedGold);
        public float GoldPerMinute => (EarnedGold / TimePlayed).NaNToZero();
        public int PlayedGames => MatchDatas.Count(m => !m.Match.IsTraining);
        public int WonGames => MatchDatas.Count(m => m.Won && !m.Match.IsTraining);
        public int LostGames => MatchDatas.Count(m => !m.Won && !m.Match.IsTraining);
        public float WinRate => (WonGames / (float)PlayedGames).NaNToZero();
        public int WonDuels => MatchDatas.Where(m => !m.Match.IsTraining).Sum(m => m.WonDuels);
        public int LostDuels => MatchDatas.Where(m => !m.Match.IsTraining).Sum(m => m.LostDuels);
        public int PlayedDuels => MatchDatas.Where(m => !m.Match.IsTraining).Sum(m => m.WonDuels + m.LostDuels);
        public float DuelWinRate => (WonDuels / (float)PlayedDuels).NaNToZero();
        public long Experience => MatchDatas.Where(m => !m.Match.IsTraining).Sum(m => m.Experience);
        //Cache for sorting
        private long _cachedExperience = -1;
        public long CachedExperience
        {
            get
            {
                if (_cachedExperience == -1)
                    _cachedExperience = Experience;
                return _cachedExperience;
            }
        }
        public long Kills => MatchDatas.Where(m => !m.Match.IsTraining).Sum(m => m.Kills);
        public long Leaks => MatchDatas.Where(m => !m.Match.IsTraining).Sum(m => m.Leaks);
        public long Sends => MatchDatas.Where(m => !m.Match.IsTraining).Sum(m => m.Sends);
        public long Builds => MatchDatas.Where(m => !m.Match.IsTraining).Sum(m => m.Builds);

        public Player()
        {
        }

        public int GetRatingBefore(DateTime date)
        {
            return 1000 + MatchDatas.Where(m => m.Match.Date < date).Sum(m => m.RatingChange);
        }

        private const string KilledPrefix = "killed_";
        public Dictionary<string, long> FractionKills
        {
            get
            {
                Dictionary<string, long> result = new Dictionary<string, long>();
                foreach (var matchData in MatchDatas.Where(m => !m.Match.IsTraining))
                {
                    foreach (var unitData in matchData.UnitDatas)
                    {
                        string key = KilledPrefix + unitData.Unit.Fraction.Name;
                        if (!result.ContainsKey(key))
                            result[key] = unitData.Killed;
                        else
                            result[key] += unitData.Killed;
                    }
                }
                return result;
            }
        }

        private const string PlayedPrefix = "played_";
        public Dictionary<string, int> PlayedFractions
        {
            get
            {
                Dictionary<string, int> result = new Dictionary<string, int>();
                foreach (var m in MatchDatas.Where(m => !m.Match.IsTraining))
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
            foreach (var pair in PlayedFractions)
                result[pair.Key] = pair.Value.ToString();
            foreach (var pair in FractionKills)
                result[pair.Key] = pair.Value.ToString();
            return result;
        }
    }
}
