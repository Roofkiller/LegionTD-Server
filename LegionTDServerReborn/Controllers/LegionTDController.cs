using LegionTDServerReborn.Models;
using LegionTDServerReborn.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LegionTDServerReborn.Extension;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using MySql.Data.MySqlClient;
using Match = LegionTDServerReborn.Models.Match;

namespace LegionTDServerReborn.Controllers
{
    [Route("api/[controller]")]
    public class LegionTdController : Controller
    {
        private readonly IMemoryCache _cache;
        private const string PlayerCountKey = "player_count";

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public LegionTdController(IMemoryCache cache)
        {
            _cache = cache;
        }

        private static class GetMethods
        {
            public const string Info = "info";
            public const string RankingPosition = "ranking_position";
            public const string RankingPositions = "ranking_positions";
            public const string Ranking = "ranking";
            public const string MatchHistory = "match_history";
            public const string MatchInfo = "match_info";
        }

        private static class RankingTypesO
        {
            public const string Kills = "kills";
            public const string WonGames = "won_games";
            public const string WinRate = "win_rate";
            public const string Experience = "experience";
            public const string EarnedTangos = "earned_tangos";
            public const string DuelWinRate = "duel_win_rate";
            public const string Rating = "rating";
            public const string TangosPerMinute = "tangos_per_minute";
            public const string GoldPerMinute = "gold_per_minute";
        }

        private static readonly Dictionary<string, RankingTypes> RankingTypeDict = new Dictionary<string, RankingTypes>
        {
            ["won_games"] = RankingTypes.WonGames,
            ["win_rate"] = RankingTypes.WinRate,
            ["earned_tangos"] = RankingTypes.EarnedTangos,
            ["duel_win_rate"] = RankingTypes.DuelWinRate,
            ["rating"] = RankingTypes.Rating,
            ["tangos_per_minute"] = RankingTypes.TangosPerMinute,
            ["GoldPerMinute"] = RankingTypes.GoldPerMinute
        };

        [HttpGet]
        public async Task<ActionResult> Get(string method, long? steamId, string rankingType, int? from, int? to,
            bool ascending, string steamIds)
        {
            await CheckUpdateRankings();
            var rType = !string.IsNullOrEmpty(rankingType) && RankingTypeDict.ContainsKey(rankingType) ? RankingTypeDict[rankingType] : RankingTypes.Invalid;
            switch (method)
            {
                case GetMethods.Info:
                    return await GetPlayerInfo(steamId);
                case GetMethods.Ranking:
                    return await GetRankingFromTo(rType, from, to, ascending);
                case GetMethods.RankingPosition:
                    return await GetPlayerPosition(steamId, rType, ascending);
                case GetMethods.RankingPositions:
                    break;
                case GetMethods.MatchHistory:
                    return await GetMatchHistory(steamId, from, to);
                case GetMethods.MatchInfo:
                    break;
                default:
                    break;
            }
            return Json(new InvalidRequestFailure());
        }

        public async Task<ActionResult> GetMatchHistory(long? steamId, int? from, int? to)
        {
            if (!steamId.HasValue)
                return Json(new MissingArgumentFailure());
            Player player = await GetPlayer(steamId.Value);
            if (player == null)
                return Json(new { });
            return Json(player.MatchDatas.OrderByDescending(m => m.Match.Date)
                .Select((m, i) => new MatchResponse(m, i + from ?? 0)));
        }

        public async Task<ActionResult> GetPlayerInfo(long? steamId)
        {
            if (!steamId.HasValue)
                return Json(new MissingArgumentFailure());
//            Player player = await _db.Players.FindAsync(steamId.Value);
//            await _db.Entry(player).Collection(p => p.MatchDatas).LoadAsync();
//            foreach (var matchData in player.MatchDatas)
//            {
//                await _db.Entry(matchData).Collection(m => m.UnitDatas).LoadAsync();
//                await _db.Entry(matchData).Reference(m => m.Match).LoadAsync();
//                await _db.Entry(matchData.Match).Collection(m => m.PlayerDatas).LoadAsync();
//                foreach (var unitData in matchData.UnitDatas)
//                {
//                    await _db.Entry(unitData).Reference(u => u.Unit).LoadAsync();
//                }
//            }
            Player player = await GetPlayer(steamId.Value);
            return player == null ? Json(new NotFoundFailure()) : Json(new PlayerResponse(player));
        }

        public async Task<ActionResult> GetPlayerPosition(long? steamId, RankingTypes rankingType, bool asc)
        {
            if (!steamId.HasValue || rankingType == RankingTypes.Invalid)
                return Json(new InvalidRequestFailure());
//            var player = await GetPlayer(steamId.Value);
            //            var ranking = await GetRanking(rankingType, asc);
            //            var rank = await GetPositionInRanking(ranking, player);
            int rank;
            using (var db = new LegionTdContext())
            {
                rank = (await db.Rankings.FindAsync(rankingType, asc, steamId))?.Position ?? -1;
            }
            return Json(new
            {
                Rank = rank,
                SteamId = steamId,
                Attribute = rankingType
            });
        }

        private async Task<ActionResult> GetPlayerPositions(string steamIds, string rankingType,
            bool asc)
        {
            if (string.IsNullOrEmpty(steamIds) || string.IsNullOrEmpty(rankingType))
                return Json(new InvalidRequestFailure());
            return await GetPlayerPositions(JsonConvert.DeserializeObject<List<long>>(steamIds), rankingType, asc);
        }

        private async Task<ActionResult> GetPlayerPositions(List<long> steamIds, string rankingType,
            bool asc)
        {
            var ranking = await GetRanking(rankingType, asc);
            List<object> result = new List<object>();
            var players = await GetPlayers(ranking);
            foreach (var player in players)
            {
                var rank = await GetPositionInRanking(ranking, player);
                result.Add(new
                {
                    Rank = rank,
                    SteamId = player.SteamId,
                    Attribute = rankingType,
                });
            }
            return Json(result);
        }

        public async Task<JsonResult> GetRankingFromTo(RankingTypes rankingType, int? from, int? to, bool asc)
        {
            if (rankingType == RankingTypes.Invalid)
                return Json(new InvalidRequestFailure());
            var playerCount = await GetPlayerCount();
            Models.RankingTypes t = Models.RankingTypes.Rating;
            List<long> ranking;
            using (var db = new LegionTdContext())
            {
                ranking = await db.Rankings.Where(r => r.Ascending == asc && r.Type == t
                                                    && r.Position <= (to + 1 ?? int.MaxValue) 
                                                    && r.Position >= (from ?? 0)).Select(r => r.PlayerId)
                                            .ToListAsync();
            }
//          var ranking = await GetRanking(rankingType, asc, from ?? 0, to ?? -1);
            var players = await GetPlayers(ranking);
            var result = players.Select((p, i) => new PlayerRankingResponse(p, i)).ToList();
            var response = new RankingResponse {PlayerCount = playerCount, Ranking = result};
            return Json(response);
        }

        private async Task<List<Player>> GetPlayers(IEnumerable<long> steamIds)
        {
            using (var db = new LegionTdContext())
            {
                List<Player> result = new List<Player>();
                var query = GetFullPlayerQueryable(db);
                foreach (var steamId in steamIds)
                {
                    result.Add(await query.FirstOrDefaultAsync(p => p.SteamId == steamId));
                }
                return result;
            }
        }

        private async Task<Player> GetPlayer(long steamId)
        {
            using (var db = new LegionTdContext())
            {
                return await GetFullPlayerQueryable(db).FirstOrDefaultAsync(p => p.SteamId == steamId);
            }
        }

        private async Task<int> GetPositionInRanking(List<long> ranking, Player player)
        {
            if (player == null)
                return await GetPlayerCount();
            var result = ranking.FindIndex(p => p == player.SteamId);
            if (result == -1)
                result = await GetPlayerCount();
            return result;
        }

        public async Task<int> GetPlayerCount()
        {
            if (_cache.TryGetValue(PlayerCountKey, out int result))
                return result;
            using (var db = new LegionTdContext())
            {
                result = await db.Players.CountAsync();
                _cache.Set(PlayerCountKey, result, DateTimeOffset.Now.AddDays(1));
            }
            return result;
        }

        private static IQueryable<Player> GetFullPlayerQueryable(LegionTdContext context)
        {
            return context.Players
                .Include(p => p.MatchDatas)
                .ThenInclude(m => m.UnitDatas)
                .ThenInclude(u => u.Unit.Fraction)
                .Include(p => p.MatchDatas)
                .ThenInclude(m => m.Match.Duels)
                .Include(p => p.MatchDatas)
                .ThenInclude(m => m.Fraction)
                .AsNoTracking();
        }

        private async Task<List<long>> GetRanking(string rankingType, bool asc, int from = 0, int to = -1)
        {
            var cacheKey = $"{rankingType}|{asc}";
            List<long> result;
            if (!_cache.TryGetValue(cacheKey, out result))
            {
                using (var db = new LegionTdContext())
                {
                    IQueryable<Player> players = db.Players;
                    switch (rankingType)
                    {
                        case RankingTypesO.Experience:
                            return new List<long>();
//                            players = db.Players.Include(p => p.MatchDatas)
//                                .ThenInclude(m => m.Match)
//                                .ThenInclude(m => m.PlayerDatas)
//                                .Include(m => m.MatchDatas)
//                                .ThenInclude(m => m.UnitDatas)
//                                .ThenInclude(u => u.Unit)
//                                .OrderBy(asc, p => p.CachedExperience);
//                            break;
                        case RankingTypesO.EarnedTangos:
                            players = db.Players.Include(p => p.MatchDatas)
                                .ThenInclude(m => m.Match)
                                .ThenInclude(m => m.PlayerDatas)
                                .OrderBy(asc, p => p.EarnedTangos);
                            break;
                        case RankingTypesO.Kills:
                            return new List<long>();
//                            players = db.Players.Include(p => p.MatchDatas)
//                                .ThenInclude(m => m.Match)
//                                .ThenInclude(m => m.PlayerDatas)
//                                .Include(m => m.MatchDatas)
//                                .ThenInclude(m => m.UnitDatas)
//                                .OrderBy(asc, p => p.Kills);
//                            break;
                        case RankingTypesO.WinRate:
                            players = db.Players.Include(p => p.MatchDatas)
                                .ThenInclude(m => m.Match)
                                .ThenInclude(m => m.PlayerDatas)
                                .Include(m => m.MatchDatas)
                                .ThenInclude(m => m.Match.Duels)
                                .OrderBy(asc, p => p.WinRate);
                            break;
                        case RankingTypesO.DuelWinRate:
                            players = db.Players.Include(p => p.MatchDatas)
                                .ThenInclude(m => m.Match)
                                .ThenInclude(m => m.PlayerDatas)
                                .Include(m => m.MatchDatas)
                                .ThenInclude(m => m.Match.Duels)
                                .OrderBy(asc, p => p.WonGames);
                            break;
                        case RankingTypesO.WonGames:
                            players = db.Players.Include(p => p.MatchDatas)
                                .ThenInclude(m => m.Match)
                                .ThenInclude(m => m.PlayerDatas)
                                .OrderBy(asc, p => p.WonGames);
                            break;
                        case RankingTypesO.Rating:
                            players = db.Players.Include(p => p.MatchDatas)
                                .OrderBy(asc, p => p.Rating);
                            break;
                        case RankingTypesO.TangosPerMinute:
                            players = db.Players.Include(p => p.MatchDatas)
                                .ThenInclude(m => m.Match)
                                .ThenInclude(m => m.PlayerDatas)
                                .OrderBy(asc, p => p.TangosPerMinute);
                            break;
                        case RankingTypesO.GoldPerMinute:
                            players = db.Players.Include(p => p.MatchDatas)
                                .ThenInclude(m => m.Match)
                                .ThenInclude(m => m.PlayerDatas)
                                .OrderBy(asc, p => p.GoldPerMinute);
                            break;
                        default:
                            return new List<long>();
                    }
                    var rank = await players.ToListAsync();
                    _cache.Set(cacheKey, result = rank.Select(p => p.SteamId).ToList(), DateTimeOffset.Now.AddDays(1));
                }
            }

            return to > 0
                ? result.Skip(from).Take(to - from + 1).ToList()
                : result.Skip(from).ToList();
        }

        private async Task CheckUpdateRankings()
        {
            List<Task> tasks = new List<Task>();
            foreach (RankingTypes value in Enum.GetValues(typeof(RankingTypes)))
            {
                if (value != RankingTypes.Rating) continue;
                var key = value + "|" + true;
                if (!_cache.TryGetValue(key, out object a))
                    tasks.Add(UpdateRanking(value, true));
                key = value + "|" + false;
                if (!_cache.TryGetValue(key, out object b))
                    tasks.Add(UpdateRanking(value, false));
            }
            foreach (var task in tasks)
                await task;
        }

        private async Task UpdateRanking(Models.RankingTypes type, bool asc)
        {
            string key = type + "|" + asc;
            _cache.Set(key, true, DateTimeOffset.Now.AddDays(1));
            using (var db = new LegionTdContext())
            {
                db.Database.ExecuteSqlCommand("DELETE FROM Rankings WHERE Type = " + (int) type + " AND Ascending = " +
                                              (asc ? 1 : 0));
//                IQueryable<Player> ranking = db.Players.AsNoTracking();
                string sql;
                string sqlSelects = "";
                string sqlJoins = "JOIN Matches AS m \n" +
                                  "ON m.MatchId = pm.MatchId \n";
                string sqlOrderBy = "";
                string sqlWheres = "WHERE m.IsTraining = FALSE \n";
                switch (type)
                {
//                    case RankingTypes.DuelWinRate:
//                        ranking = ranking.Include(p => p.MatchDatas)
//                            .ThenInclude(m => m.Match)
//                            .Include(m => m.MatchDatas)
//                            .ThenInclude(m => m.Match.Duels)
//                            .OrderBy(asc, p => p.DuelWinRate);
//                        break;
                    case RankingTypes.EarnedTangos:
                        sqlSelects = ", SUM(EarnedTangos) AS Gold \n";
                        sqlOrderBy = "ORDER BY Gold \n";
                        break;
//                    case RankingTypes.GoldPerMinute:
//                        ranking = ranking.Include(p => p.MatchDatas)
//                            .ThenInclude(m => m.Match)
//                            .OrderBy(asc, p => p.GoldPerMinute);
//                        break;
//                    case RankingTypes.TangosPerMinute:
//                        ranking = ranking.Include(p => p.MatchDatas)
//                            .ThenInclude(m => m.Match)
//                            .OrderBy(asc, p => p.TangosPerMinute);
//                        break;
//                    case RankingTypes.WinRate:
//                        ranking = ranking.Include(p => p.MatchDatas)
//                            .ThenInclude(m => m.Match)
//                            .OrderBy(asc, p => p.WinRate);
//                        sqlJoins = "JOIN Duels AS d \n" +
//                                   "ON d.MatchId = m.MatchId";
//                        sqlSelects = "";
//                        break;
                    case RankingTypes.EarnedGold:
                        sqlSelects = ", SUM(EarnedGold) AS Gold \n";
                        sqlOrderBy = "ORDER BY Gold \n";
                        break;
                    case RankingTypes.Rating:
                    default:
                        sqlSelects = ", SUM(RatingChange) AS Rating \n";
                        sqlOrderBy = "ORDER BY Rating "+ (asc? "ASC" : "DESC") +" \n";
                        sqlJoins = "";
                        sqlWheres = "";
                        break;
                }
                MySqlParameter CreateParameter(string k, DbType t, object value)
                {
                    MySqlParameter result = new MySqlParameter
                    {
                        DbType = t,
                        Value = value,
                        ParameterName = k
                    };
                    return result;
                }
                sql = "INSERT INTO Rankings \n" +
                    "(Type, Ascending, PlayerId, Position) \n" +
                    "SELECT @t := @type, @a := @asc, SteamId, @rownum := @rownum + 1 AS position\n" +
                    "FROM (SELECT SteamId \n" +
                    sqlSelects +
                    "FROM Players AS p \n" +
                    "JOIN PlayerMatchDatas AS pm \n" +
                    "ON p.SteamId = pm.PlayerId \n" +
                    sqlJoins +
                    sqlWheres +
                    "GROUP BY p.SteamId \n" +
                    sqlOrderBy +
                    ") AS pr, \n" +
                    "(SELECT @rownum := 0) AS r \n";
                await db.Database.ExecuteSqlCommandAsync(sql,
                    CancellationToken.None,
                    CreateParameter("@type", DbType.Int32, type),
                    CreateParameter("@asc", DbType.Boolean, asc));
                Console.WriteLine("Updated ranking for " + type + " " + asc);
            }
        }




        private static class PostMethods
        {
            public const string SavePlayerData = "save_player";
            public const string SaveMatchData = "save_match";
            public const string UpdateUnitData = "update_units";
        }

        [HttpPost]
        public async Task<ActionResult> Post(string method, int? winner, string playerData, string data, float duration,
            int lastWave, string duelData, long? steamId)
        {
            switch (method)
            {
                case PostMethods.SaveMatchData:
                    return await SaveMatchData(winner, playerData, duration, lastWave, duelData);
                case PostMethods.UpdateUnitData:
                    return await UpdateUnitData(data);
                case PostMethods.SavePlayerData:
                default:
                    return Json(new InvalidRequestFailure());
            }
        }

        private async Task<ActionResult> UpdateUnitData(string data)
        {
            Debug.WriteLine("The following is the unit data: ");
            Debug.WriteLine(data);
            var unitData = JObject.Parse(data);
            foreach (var pair in unitData)
            {
                string unitName = pair.Key;
                int experience = int.Parse(pair.Value["experience"].Value<string>());
                string fraction = pair.Value["fraction"].Value<string>();
                await UpdateOrInsertUnit(unitName, fraction, experience);
            }
            return Json(new {Success = true});
        }

        private async Task<Unit> UpdateOrInsertUnit(string name, string fraction, int experience)
        {
            Unit unit = await GetOrCreateUnit(name);
            using (var db = new LegionTdContext())
            {
                unit.Fraction = await GetOrCreateFraction(fraction);
                unit.Experience = experience;
                db.Entry(unit).State = EntityState.Modified;
                db.Entry(unit.Fraction).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            return unit;
        }

        public async Task<ActionResult> SaveMatchData(int? winner, string playerDataString, float duration,
            int lastWave, string duelDataString)
        {
            if (!winner.HasValue || string.IsNullOrEmpty(playerDataString))
                return Json(new MissingArgumentFailure());

            //Creating Match
            Match match = await CreateMatch(winner.Value, duration, lastWave);

            //Adding Duels
            if (!string.IsNullOrEmpty(duelDataString))
            {
                var duelData =
                    JsonConvert.DeserializeObject<Dictionary<int, Dictionary<String, float>>>(duelDataString);
                if (duelData != null)
                {
                    foreach (var pair in duelData)
                    {
                        int order = pair.Key;
                        var data = pair.Value;
                        Duel duel = await CreateDuel(match, order, (int) data["winner"], data["time"]);
                    }
                }
            }

            //Adding player Data
            var playerData =
                JsonConvert.DeserializeObject<Dictionary<long, Dictionary<string, string>>>(playerDataString);
            List<Player> players = new List<Player>();
            List<PlayerMatchData> playerMatchDatas = new List<PlayerMatchData>();
            foreach (var pair in playerData)
            {
                long steamId = pair.Key;
                Dictionary<string, string> decodedData = pair.Value;
                Player player = await GetOrCreatePlayer(steamId);
                PlayerMatchData playerMatchData = await CreatePlayerMatchData(player,
                    match,
                    decodedData["fraction"],
                    int.Parse(decodedData["team"]),
                    bool.Parse(decodedData["abandoned"]),
                    int.Parse(decodedData["earned_tangos"]),
                    int.Parse(decodedData["earned_gold"]));
                List<PlayerUnitRelation> playerUnitRelations =
                    await CreatePlayerUnitRelations(playerMatchData, decodedData);
                players.Add(player);
                playerMatchDatas.Add(playerMatchData);
            }
            await DecideIsTraining(match);
            await ModifyRatings(playerMatchDatas, match);

            return Json(new {Success = true});
        }

        private async Task DecideIsTraining(Match match)
        {
            using (var db = new LegionTdContext())
            {
                await db.Entry(match).Collection(m => m.PlayerDatas).LoadAsync();
                match.IsTraining = match.PlayerDatas.All(p => p.Team == match.Winner) ||
                                   match.PlayerDatas.All(p => p.Team != match.Winner);
                db.Entry(match).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
        }

        private async Task ModifyRatings(List<PlayerMatchData> playerMatchDatas, Match match)
        {
            using (var db = new LegionTdContext())
            {
                var l = new List<Player>();
                foreach (var pl in playerMatchDatas)
                    l.Add(await db.Players
                        .Include(p => p.MatchDatas)
                        .ThenInclude(m => m.Match.PlayerDatas)
                        .SingleAsync(player => player.SteamId == pl.PlayerId));
                foreach (var p in l.Select(p => p.MatchDatas.Single(m => m.MatchId == match.MatchId)))
                    p.RatingChange = p.CalculateRatingChange();
                await db.SaveChangesAsync();
            }
        }

        private async Task<Duel> CreateDuel(Match match, int order, int winner, float time)
        {
            Duel duel = new Duel
            {
                MatchId = match.MatchId,
                Order = order,
                Winner = winner,
                TimeStamp = time
            };
            using (var db = new LegionTdContext())
            {
                db.Duels.Add(duel);
                await db.SaveChangesAsync();
            }
            return duel;
        }

        private static readonly Dictionary<string, Action<PlayerUnitRelation, int>> UnitRelationFunctions =
            new Dictionary<string, Action<PlayerUnitRelation, int>>
            {
                {"killed_", (relation, count) => relation.Killed = count},
                {"build_", (relation, count) => relation.Build = count},
                {"leaked_", (relation, count) => relation.Leaked = count},
                {"send_", (relation, count) => relation.Send = count}
            };

        private async Task<List<PlayerUnitRelation>> CreatePlayerUnitRelations(PlayerMatchData playerMatchData,
            Dictionary<string, string> decodedData)
        {
            //local function to interpet the key
            //key is constructed like this: <type>_<unitname>
            (string unitName, string type) InterpretIdentifier(string identifier)
            {
                foreach (var t in UnitRelationFunctions.Keys)
                    if (identifier.StartsWith(t))
                        return (identifier.Replace(t, ""), t);
                return (null, null);
            }

            Dictionary<string, PlayerUnitRelation> relations = new Dictionary<string, PlayerUnitRelation>();
            foreach (var pair in decodedData)
            {
                var (unitName, type) = InterpretIdentifier(pair.Key);
                if (unitName == null || type == null)
                    continue;
                Unit unit = await GetOrCreateUnit(unitName);
                int count = int.Parse(pair.Value);
                PlayerUnitRelation relation = relations.ContainsKey(unitName)
                    ? relations[unitName]
                    : new PlayerUnitRelation
                    {
                        PlayerMatch = playerMatchData,
                        Unit = unit
                    };
                relations[unitName] = relation;
                UnitRelationFunctions[type].Invoke(relation, count);
            }
            List<PlayerUnitRelation> result = relations.Select(p => p.Value).ToList();
            using (var db = new LegionTdContext())
            {
                result.ForEach(r => db.Entry(r.Unit).State = EntityState.Modified);
                db.Entry(playerMatchData).State = EntityState.Modified;
                db.PlayerUnitRelations.AddRange(result);
                await db.SaveChangesAsync();
            }
            return result;
        }

        private async Task<Unit> GetOrCreateUnit(string unitName)
        {
            using (var db = new LegionTdContext())
            {
                Unit unit = await db.Units.FindAsync(unitName);
                if (unit == null)
                {
                    unit = new Unit
                    {
                        Name = unitName,
                        Experience = 0
                    };
                    unit.SetTypeByName();
                    string fraction = unit.GetFractionByName();
                    unit.Fraction = await GetOrCreateFraction(fraction);
                    db.UpdateRange(unit.Fraction);
                    db.Units.Add(unit);
                    await db.SaveChangesAsync();
                }
                return unit;
            }
        }

        private async Task<Fraction> GetOrCreateFraction(string name)
        {
            using (var db = new LegionTdContext())
            {
                Fraction result = await db.Fractions.FindAsync(name);
                if (result == null)
                {
                    result = new Fraction {Name = name};
                    db.Fractions.Add(result);
                    await db.SaveChangesAsync();
                }
                return result;
            }
        }

        private async Task<PlayerMatchData> CreatePlayerMatchData(Player player, Match match, string fraction, int team,
            bool abandoned, int earnedTangos, int earnedGold)
        {
            using (var db = new LegionTdContext())
            {
                PlayerMatchData result = new PlayerMatchData
                {
                    Player = player,
                    Match = match,
                    Abandoned = abandoned,
                    Team = team,
                    Fraction = await GetOrCreateFraction(fraction),
                    EarnedTangos = earnedTangos,
                    EarnedGold = earnedGold
                };
                db.Entry(player).State = EntityState.Modified;
                db.Entry(match).State = EntityState.Modified;
                db.Entry(result.Fraction).State = EntityState.Modified;
                db.PlayerMatchDatas.Add(result);
                await db.SaveChangesAsync();
                return result;
            }
        }

        private async Task<Match> CreateMatch(int winner, float duration, int lastWave)
        {
            using (var db = new LegionTdContext())
            {
                Match match = new Match
                {
                    Winner = winner,
                    Duration = duration,
                    LastWave = lastWave,
                    Date = DateTime.Now
                };
                db.Matches.Add(match);
                await db.SaveChangesAsync();
                return match;
            }
        }

        private async Task<Player> GetOrCreatePlayer(long steamId)
        {
            using (var db = new LegionTdContext())
            {
                Player result = await db.Players.FindAsync(steamId);
                if (result == null)
                {
                    result = new Player
                    {
                        SteamId = steamId,
                    };
                    db.Players.Add(result);
                    await db.SaveChangesAsync();
                }
                return result;
            }
        }
    }
}