using LegionTDServerReborn.Models;
using LegionTDServerReborn.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LegionTDServerReborn.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using MySql.Data.MySqlClient;
using Match = LegionTDServerReborn.Models.Match;
using LegionTDServerReborn.Utils;
using System.Net;
using System.IO;

namespace LegionTDServerReborn.Controllers
{
    [Route("api/[controller]")]
    public class LegionTdController : Controller
    {
        private readonly IMemoryCache _cache;
        private LegionTdContext _db;
        private const string PlayerCountKey = "player_count";


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public LegionTdController(LegionTdContext context, IMemoryCache cache)
        {
            _db = context;
            _cache = cache;
        }

        private static class GetMethods
        {
            public const string Info = "info";
            public const string MatchHistory = "match_history";
            public const string MatchInfo = "match_info";
            public const string Ranking = "ranking";
            public const string RankingPosition = "ranking_position";
            public const string RankingPositions = "ranking_positions";
            public const string RecentMatches = "recent_matches";
            public const string UpdateFractionStatistics = "update_fractions";
            public const string UpdateRanking = "update_ranking";
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
            bool ascending, string steamIds, int? matchId)
        {
            var rType = !string.IsNullOrEmpty(rankingType) && RankingTypeDict.ContainsKey(rankingType) ? RankingTypeDict[rankingType] : RankingTypes.Invalid;
            switch (method)
            {
                case GetMethods.UpdateRanking:
                    return await UpdateRankings();
                case GetMethods.UpdateFractionStatistics:
                    return await UpdateFractionStatistics();
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
                    return await GetMatchInfo(matchId);
                case GetMethods.RecentMatches:
                    return await GetRecentMatches(from, to);
                default:
                    break;
            }
            return Json(new InvalidRequestFailure());
        }

        public async Task<ActionResult> GetMatchInfo(int? matchId) {
            if (!matchId.HasValue) {
                return Json(new MissingArgumentFailure());
            }
            return Json(await _db.Matches.Include(m => m.Duels)
                .Include(m => m.PlayerDatas).SingleOrDefaultAsync(m => m.MatchId == matchId.Value));
        }

        public async Task<ActionResult> GetRecentMatches(int? from, int? to) {
            return Json(await _db.Matches.OrderByDescending(m => m.MatchId)
                                        .Skip(from ?? 0)
                                        .Take(to ?? 15).ToListAsync());
        }

        public async Task<ActionResult> GetMatchHistory(long? steamId, int? from, int? to)
        {
            if (!steamId.HasValue)
                return Json(new MissingArgumentFailure());
            Player player = await GetPlayer(steamId.Value);
            if (player == null)
                return Json(new { });
            return Json(player.Matches.OrderByDescending(m => m.Match.Date)
                .Select((m, i) => new MatchResponse(m, i + from ?? 0)));
        }

        public async Task<ActionResult> GetPlayerInfo(long? steamId)
        {
            if (!steamId.HasValue)
                return Json(new MissingArgumentFailure());
            Player player = await GetPlayer(steamId.Value);
            return player == null ? Json(new NotFoundFailure()) : Json(new PlayerResponse(player));
        }

        public async Task<ActionResult> GetPlayerPosition(long? steamId, RankingTypes rankingType, bool asc)
        {
            if (!steamId.HasValue || rankingType == RankingTypes.Invalid)
                return Json(new InvalidRequestFailure());
            int rank = (await _db.Rankings.FindAsync(rankingType, asc, steamId))?.Position ?? -1;
            return Json(new
            {
                Rank = rank,
                SteamId = steamId,
                Attribute = RankingTypeDict.FirstOrDefault(pair => pair.Value == rankingType).Key
            });
        }

        public async Task<JsonResult> GetRankingFromTo(RankingTypes rankingType, int? from, int? to, bool asc)
        {
            if (rankingType == RankingTypes.Invalid)
                return Json(new InvalidRequestFailure());
            var playerCount = await GetPlayerCount();
            int lower = (from ?? 0);
            int upper = (to + 1 ?? int.MaxValue);
            var ranking = (await _db.Rankings.Where(r => r.Position <= upper
                                            && r.Position >= lower)
                                        .ToListAsync()).OrderBy(d => d.Position).ToList();
            var result = await GetRankingData(ranking);
            var response = new RankingResponse {PlayerCount = playerCount, Ranking = result};
            return Json(response);
        }

        private async Task<List<PlayerRankingResponse>> GetRankingData(List<Ranking> ranking)
        {
            var ids = ranking.Select(r => r.PlayerId).ToArray();
            List<PlayerRankingResponse> result = new List<PlayerRankingResponse>();
            var query = GetFullPlayerQueryable(_db);
            var values = new StringBuilder();
            values.Append(ids[0]);
            for (int i = 0; i < ids.Length; i++) {
                values.Append($", {ids[i]}");
            }
            var sql = $"SELECT * FROM Players p WHERE SteamId IN ({values})";
            var players = await query.FromSql(sql).ToListAsync();
            foreach(var rang in ranking) {
                result.Add(new PlayerRankingResponse(players.First(p => p.SteamId == rang.PlayerId), rang.Position - 1));
            }
            return result;
        }

        private async Task<Player> GetPlayer(long steamId)
        {
            return await GetFullPlayerQueryable(_db).FirstOrDefaultAsync(p => p.SteamId == steamId);
        }

        public async Task<int> GetPlayerCount()
        {
            if (_cache.TryGetValue(PlayerCountKey, out int result))
                return result;
            result = await _db.Players.CountAsync();
            _cache.Set(PlayerCountKey, result, DateTimeOffset.Now.AddDays(1));
            return result;
        }

        private static IQueryable<Player> GetFullPlayerQueryable(LegionTdContext context)
        {
            return context.Players
                .Include(p => p.Matches)
                .ThenInclude(m => m.Fraction)
                .Include(p => p.Matches)
                .ThenInclude(m => m.Match)
                // .Include(p => p.FractionDatas)
                .AsNoTracking();
        }

        private async Task CheckUpdateRankings()
        {
            List<Task> tasks = new List<Task>();
            foreach (RankingTypes value in Enum.GetValues(typeof(RankingTypes)))
            {
                if (value != RankingTypes.Rating) continue;
                // var key = value + "|" + true;
                // if (!_cache.TryGetValue(key, out object a))
                //     tasks.Add(UpdateRanking(value, true));
                var key = value + "|" + false;
                if (!_cache.TryGetValue(key, out object b))
                    tasks.Add(UpdateRanking(value, false));
            }
            foreach (var task in tasks)
                await task;
        }

        private async Task<ActionResult> UpdateRankings() {
            await UpdateRanking(RankingTypes.Rating, false);
            return Json(new {success = true});
        }

        private async Task UpdateRanking(Models.RankingTypes type, bool asc)
        {
            string key = type + "|" + asc;
            _cache.Set(key, true, DateTimeOffset.Now.AddDays(1));

            await _db.Database.ExecuteSqlCommandAsync($"DELETE FROM Rankings WHERE Type = {(int) type} AND Ascending = {(asc ? 1 : 0)}");
            Console.WriteLine($"Cleared Ranking for {type} {asc}");
            string sql;
            string sqlSelects = "";
            string sqlJoins = "JOIN Matches AS m \n" +
                                "ON m.MatchId = pm.MatchId \n";
            string sqlOrderBy = "";
            string sqlWheres = "WHERE m.IsTraining = FALSE \n";
            switch (type)
            {
                case RankingTypes.EarnedTangos:
                    sqlSelects = ", SUM(EarnedTangos) AS Gold \n";
                    sqlOrderBy = "ORDER BY Gold "+ (asc? "ASC" : "DESC") +" \n";
                    break;
                case RankingTypes.EarnedGold:
                    sqlSelects = ", SUM(EarnedGold) AS Gold \n";
                    sqlOrderBy = "ORDER BY Gold "+ (asc? "ASC" : "DESC") +" \n";
                    break;
                case RankingTypes.Rating:
                default:
                    sqlSelects = ", SUM(RatingChange) AS Rating \n";
                    sqlOrderBy = "ORDER BY Rating "+ (asc? "ASC" : "DESC") +" \n";
                    sqlJoins = "";
                    sqlWheres = "";
                    break;
            }
            sql = "INSERT INTO Rankings \n" +
                "(Type, Ascending, PlayerId, Position) \n" +
                $"SELECT @t := {(int)type}, @a := {(asc ? "TRUE" : "FALSE")}, PlayerId, @rownum := @rownum + 1 AS position\n" +
                "FROM (SELECT PlayerId \n" +
                sqlSelects +
                "FROM PlayerMatchData AS pm \n" +
                sqlJoins +
                sqlWheres +
                "GROUP BY pm.PlayerId \n" +
                sqlOrderBy +
                ") AS pr, \n" +
                "(SELECT @rownum := 0) AS r \n";
            await _db.Database.ExecuteSqlCommandAsync(sql);
            Console.WriteLine($"Updated ranking for {type} {asc}");
        }

        private async Task<ActionResult> UpdateFractionStatistics() {
            var fractions = await _db.Fractions.ToListAsync();
            var tasks = new List<Task>(fractions.Count);
            foreach(var fraction in fractions) {
                tasks.Add(UpdateFractionStatistic(fraction.Name));
            }
            foreach(var task in tasks) {
                await task;
            }
            await _db.SaveChangesAsync();
            return Json(new {success = true});
        }

        private async Task UpdateFractionStatistic(string fractionName) {
            Fraction fraction = await GetOrCreateFraction(fractionName);
            var timeStamp = DateTime.Now;
            var yesterday = timeStamp.AddDays(-1);
            var wins = await _db.Fractions.Include(b => b.PlayedMatches).ThenInclude(m => m.Match)
                .Where(f => f.Name == fractionName)
                .SelectMany(b => b.PlayedMatches.Where(m => m.Match.Date > yesterday))
                .CountAsync(m => m.Team == m.Match.Winner);
            var count = await _db.Fractions.Include(b => b.PlayedMatches).ThenInclude(m => m.Match)
                .Where(f => f.Name == fractionName)
                .SelectMany(b => b.PlayedMatches.Where(m => m.Match.Date > yesterday))
                .CountAsync();
            var pickRate = await _db.Matches.Include(m => m.PlayerDatas)
                .Where(m => m.Date > yesterday)
                .AverageAsync(m => m.PlayerDatas.Count(p => p.FractionName == fractionName));
            Console.WriteLine(fractionName + " " + wins);
            Console.WriteLine(fractionName + " " + count);
            Console.WriteLine(fractionName + " " + pickRate);
            FractionStatistic statistic = new FractionStatistic() {
                TimeStamp = timeStamp,
                Fraction = fraction,
                FractionName = fractionName,
                WonGames = wins,
                LostGames = count - wins,
                PickRate = (float)pickRate
            };
            _db.Update(fraction);
            _db.FractionStatistics.Add(statistic);
        }





        private static class PostMethods
        {
            public const string SavePlayerData = "save_player";
            public const string SaveMatchData = "save_match";
            public const string UpdateAbilityData = "update_abilities";
            public const string UpdateUnitData = "update_units";
        }

        [HttpPost]
        public async Task<ActionResult> Post(string method, int? winner, string playerData, string data, float duration,
            int lastWave, string duelData, long? steamId)
        {
            if (!await CheckIp()) {
                return Json(new NoPermissionFailure());
            }
            switch (method)
            {
                case PostMethods.SaveMatchData:
                    return await SaveMatchData(winner, playerData, duration, lastWave, duelData);
                case PostMethods.UpdateUnitData:
                    return await UpdateUnitData(data);
                case PostMethods.UpdateAbilityData:
                    return await UpdateAbilityData(data);
                case PostMethods.SavePlayerData:
                default:
                    return Json(new InvalidRequestFailure());
            }
        }

        private async Task<bool> CheckIp() {
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;
            var ranges = await GetDotaIpRanges();
            foreach(var range in ranges) {
                if (range.IsInRange(ipAddress)) {
                    Console.WriteLine($"Client {ipAddress} is in Range {range.Lower} - {range.Upper}.");
                    return true;
                }
            }
            Console.WriteLine($"Connection to {ipAddress.ToString()} refused.");
            return false;
        }

        private async Task<List<IpAddressRange>> GetDotaIpRanges() {
            List<IpAddressRange> result = null;
            if (!_cache.TryGetValue("dota_ip_ranges", out result)) {
                result = new List<IpAddressRange>();
                WebRequest request = WebRequest.CreateHttp("http://media.steampowered.com/apps/sdr/network_config.json");
                request.Method = "GET";
                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                string content = reader.ReadToEnd();
                var json = JObject.Parse(content);
                var datacenters = json["data_centers"] as JObject;
                foreach(var datacenter in datacenters) {
                    var addressRanges = datacenter.Value["address_ranges"] as JArray;
                    if (addressRanges != null) {
                        foreach (var addressRange in addressRanges) {
                            result.Add(IpAddressRange.Parse((string)addressRange));
                        }
                    }
                }
                result.Add(new IpAddressRange("127.0.0.1", "127.0.0.1"));
                result.Add(new IpAddressRange("::1", "::1"));
                _cache.Set("dota_ip_ranges", result, TimeSpan.FromDays(1));
            }
            return result;
        }

        private async Task<ActionResult> UpdateUnitData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return Json(new MissingArgumentFailure());
            JObject unitData;
            try {
                unitData = JObject.Parse(data);
            } catch(Exception) {
                return Json(new InvalidRequestFailure());
            }
            var units = new List<Unit>();
            foreach (var pair in unitData)
            {
                string unitName = pair.Key;
                string fraction;
                try {
                    fraction = pair.Value["LegionFraction"].Value<string>();
                } catch(Exception) {
                    fraction = "other";
                }
                Unit unit = await GetOrCreateUnit(unitName);
                unit.Fraction = await GetOrCreateFraction(fraction);
                unit.UpdateValues(pair.Value);
                await _db.Database.ExecuteSqlCommandAsync($"DELETE FROM UnitAbilities WHERE UnitName = {unitName};");
                for (int i = 1; i <= 24; i++) {
                    string abilityName = pair.Value.GetValueOrDefault($"Ability{i}");
                    if (!string.IsNullOrEmpty(abilityName)) {
                        await GetOrCreateUnitAbility(unitName, abilityName, i);
                    }
                }
                units.Add(unit);
            }
            _db.UpdateRange(units.Select(u => u.Fraction).ToArray());
            _db.UpdateRange(units.ToArray());
            await _db.SaveChangesAsync();
            return Json(new {Success = true});
        }

        private async Task<ActionResult> UpdateAbilityData(string data) {
            if (string.IsNullOrEmpty(data))
                return Json(new MissingArgumentFailure());
            JObject abilityData;
            try {
                abilityData = JObject.Parse(data);
            } catch(Exception) {
                return Json(new InvalidRequestFailure());
            }
            var abilities = new List<Ability>();
            foreach(var pair in abilityData) {
                string abilityName = pair.Key;
                Ability ability = await GetOrCreateAbility(abilityName);
                ability.UpdateValues(pair.Value);
            }
            await _db.SaveChangesAsync();
            return Json(new {Success = true});
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
                Dictionary<int, Dictionary<String, float>> duelData = null;
                try {
                    duelData = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<String, float>>>(duelDataString);
                } catch (Exception) {
                    try {
                        var data = JsonConvert.DeserializeObject<List<Dictionary<String, float>>>(duelDataString);
                        for(int i = 0; i < data.Count; i++) {
                            duelData[i + 1] = data[i];
                        }
                    } catch (Exception) {}
                }
                if (duelData != null)
                {
                    foreach (var pair in duelData)
                    {
                        var order = pair.Key;
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
            // await UpdateFractionDatas(match);
            await ModifyRatings(playerMatchDatas, match);

            return Json(new {Success = true});
        }

        private async Task DecideIsTraining(Match match)
        {
            var ma = await _db.Matches.IgnoreQueryFilters().Include(m => m.PlayerDatas).SingleAsync(m => m.MatchId == match.MatchId);
            ma.IsTraining = ma.PlayerDatas.All(p => p.Team == match.Winner) ||
                            ma.PlayerDatas.All(p => p.Team != match.Winner);
            _db.Entry(ma).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }
        
        private async Task ModifyRatings(List<PlayerMatchData> playerMatchDatas, Match match)
        {
            var l = new List<Player>();
            foreach (var pl in playerMatchDatas)
                l.Add(await _db.Players
                    .Include(p => p.Matches)
                    .ThenInclude(m => m.Match.PlayerDatas)
                    .SingleAsync(player => player.SteamId == pl.PlayerId));
            foreach (var p in l.Select(p => p.Matches.Single(m => m.MatchId == match.MatchId)))
                p.RatingChange = p.CalculateRatingChange();
            await _db.SaveChangesAsync();
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
            _db.Duels.Add(duel);
            await _db.SaveChangesAsync();
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
            List<PlayerUnitRelation> result = relations.Values.ToList();

            _db.UpdateRange(result.Select(r => r.Unit));
            _db.Update(playerMatchData);
            _db.PlayerUnitRelations.AddRange(result);
            
            await _db.SaveChangesAsync();
            
            //Calculating Match statistics
            var p = await _db.PlayerMatchData
                .IgnoreQueryFilters()
                .Include(pd => pd.UnitDatas)
                .ThenInclude(r => r.Unit)
                .Include(pd => pd.Match)
                .ThenInclude(pd => pd.Duels)
                .SingleAsync(pd => pd.MatchId == playerMatchData.MatchId && pd.PlayerId == playerMatchData.PlayerId);
            p.CalculateStats();
            _db.Update(p);

            await _db.SaveChangesAsync();

            return result;
        }

        private async Task<Ability> GetOrCreateAbility(string abilityName) {
            Ability ability = await _db.Abilities.FindAsync(abilityName);
            if (ability == null) {
                if (Regex.IsMatch(abilityName, @".+builder_(spawn|upgrade)_.+")) {
                    string unitName = "tower_" + Regex.Replace(abilityName, @"_(spawn|upgrade)", "");
                    ability = new SpawnAbility{
                        Unit = await GetOrCreateUnit(unitName),
                        UnitName = unitName
                    };
                    _db.Update(((SpawnAbility)ability).Unit);
                    _db.SpawnAbilities.Add((SpawnAbility)ability);
                } else {
                    ability = new Ability();
                _db.Abilities.Add(ability);
                }
                ability.Name = abilityName;
                await _db.SaveChangesAsync();
            }
            return ability;
        }

        private async Task<UnitAbility> GetOrCreateUnitAbility(string unitName, string abilityName, int slot) {
            var result = await _db.UnitAbilities.FindAsync(unitName, slot);
            if (result == null) {
                result = new UnitAbility {
                    UnitName = unitName,
                    Unit = await GetOrCreateUnit(unitName),
                    AbilityName = abilityName,
                    Ability = await GetOrCreateAbility(abilityName),
                    Slot = slot
                };
                _db.UnitAbilities.Add(result);
                await _db.SaveChangesAsync();
            }
            return result;
        }

        private async Task<Unit> GetOrCreateUnit(string unitName)
        {
            Unit unit = await _db.Units.FindAsync(unitName);
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
                _db.Update(unit.Fraction);
                _db.Units.Add(unit);
                await _db.SaveChangesAsync();
            }
            return unit;
        }

        private async Task<Fraction> GetOrCreateFraction(string name)
        {
            Fraction result = await _db.Fractions.FindAsync(name);
            if (result == null)
            {
                result = new Fraction {Name = name};
                _db.Fractions.Add(result);
                await _db.SaveChangesAsync();
            }
            return result;
        }

        private async Task<PlayerMatchData> CreatePlayerMatchData(Player player, Match match, string fraction, int team,
            bool abandoned, int earnedTangos, int earnedGold)
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
            _db.Entry(player).State = EntityState.Modified;
            _db.Entry(match).State = EntityState.Modified;
            _db.Entry(result.Fraction).State = EntityState.Modified;
            _db.PlayerMatchData.Add(result);

            await _db.SaveChangesAsync();
            return result;
        }

        private async Task<Match> CreateMatch(int winner, float duration, int lastWave)
        {
            Match match = new Match
            {
                Winner = winner,
                Duration = duration,
                LastWave = lastWave,
                Date = DateTime.Now,
                IsTraining = true
            };
            _db.Matches.Add(match);
            await _db.SaveChangesAsync();
            return match;
        }

        private async Task<Player> GetOrCreatePlayer(long steamId)
        {
            Player result = await _db.Players.FindAsync(steamId);
            if (result == null)
            {
                result = new Player
                {
                    SteamId = steamId,
                };
                _db.Players.Add(result);
                await _db.SaveChangesAsync();
            }
            return result;
        }
    }
}
