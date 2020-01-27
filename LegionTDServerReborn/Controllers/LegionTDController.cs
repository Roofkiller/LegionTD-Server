using LegionTDServerReborn.Models;
using LegionTDServerReborn.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LegionTDServerReborn.Extensions;
using Match = LegionTDServerReborn.Models.Match;
using LegionTDServerReborn.Utils;
using LegionTDServerReborn.Services;
using System.Net;
using System.IO;

namespace LegionTDServerReborn.Controllers
{
    [Route("api/[controller]")]
    public class LegionTdController : Controller
    {
        public static bool _checkIp = true;
        private readonly IMemoryCache _cache;
        private readonly LegionTdContext _db;
        private readonly SteamApi _steamApi;
        private const string PlayerCountKey = "player_count";
        private readonly string _dedicatedServerKey;


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public LegionTdController(LegionTdContext context, SteamApi steamApi, IMemoryCache cache, IConfiguration configuration)
        {
            _db = context;
            _cache = cache;
            _steamApi = steamApi;
            _dedicatedServerKey = configuration["dedicatedServerKey"];
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
            public const string UpdateUnitStatistics = "update_units";
            public const string UpdatePlayerProfiles = "update_players";
            public const string CheckIp = "check_ip";
            public const string MatchesPerDay = "matches_per_day";
            public const string FractionDataHistory = "fraction_data_history";
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
            bool ascending, string steamIds, int? matchId, bool ipCheck, int? numDays, string fraction)
        {
            var rType = !string.IsNullOrWhiteSpace(rankingType) && RankingTypeDict.ContainsKey(rankingType) ? RankingTypeDict[rankingType] : RankingTypes.Invalid;
            switch (method)
            {
                case GetMethods.CheckIp:
                    return await SetIpCheck(ipCheck);
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
                case GetMethods.MatchHistory:
                    return await GetMatchHistory(steamId, from, to);
                case GetMethods.MatchInfo:
                    return await GetMatchInfo(matchId);
                case GetMethods.RecentMatches:
                    return await GetRecentMatches(from, to);
                case GetMethods.UpdatePlayerProfiles:
                    return await UpdatePlayerProfiles();
                case GetMethods.UpdateUnitStatistics:
                    return await UpdateUnitStatistics();
                case GetMethods.MatchesPerDay:
                    return await GetMatchesPerDay(numDays);
                case GetMethods.FractionDataHistory:
                    return await FractionDataHistory(numDays, fraction);
                default:
                    break;
            }
            return Json(new InvalidRequestFailure());
        }

        public async Task<ActionResult> SetIpCheck(bool value)
        {
            if (!await CheckIp())
            {
                return Json(new NoPermissionFailure());
            }
            _checkIp = value;
            return Json("Turned ip check " + (value ? "on" : "off"));
        }

        public async Task<ActionResult> FractionDataHistory(int? numDays, string fraction)
        {
            var now = DateTime.UtcNow;
            var dt = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            var nd = numDays ?? 31;
            dt = dt.AddDays(-nd);
            var result = await _db.FractionStatistics
                                  .Where(s => s.TimeStamp > dt && s.FractionName == fraction)
                                  .ToListAsync();
            return Json(result);
        }

        public async Task<ActionResult> GetMatchesPerDay(int? numDays)
        {
            var now = DateTime.UtcNow;
            var dt = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            var nd = numDays ?? 31;
            dt = dt.AddDays(-nd);
            return Json(await _db.Matches.Where(m => m.Date > dt)
                                .GroupBy(m => new { m.Date.Year, m.Date.Month, m.Date.Day })
                                .Select(g => new { name = g.Key, count = g.Count() }).ToListAsync());
        }

        public async Task<ActionResult> GetMatchInfo(int? matchId)
        {
            if (!matchId.HasValue)
            {
                return Json(new MissingArgumentFailure());
            }
            return Json(await _db.Matches.Include(m => m.Duels)
                .Include(m => m.PlayerData).SingleOrDefaultAsync(m => m.MatchId == matchId.Value));
        }

        public async Task<ActionResult> GetRecentMatches(int? from, int? to)
        {
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
            var response = new RankingResponse { PlayerCount = playerCount, Ranking = result };
            return Json(response);
        }

        private async Task<List<PlayerRankingResponse>> GetRankingData(List<Ranking> ranking)
        {
            var ids = ranking.Select(r => r.PlayerId).ToArray();
            List<PlayerRankingResponse> result = new List<PlayerRankingResponse>();
            var query = GetFullPlayerQueryable(_db);
            var steamIds = String.Join(", ", ids);
            var sql = $"SELECT * FROM Players p WHERE SteamId IN ({steamIds})";
            var players = await _db.Players.FromSqlRaw(sql)
                .Include(p => p.Matches)
                .ThenInclude(m => m.Fraction)
                .Include(p => p.Matches)
                .ThenInclude(m => m.Match)
                .AsNoTracking()
                .ToListAsync();
            foreach (var rang in ranking)
            {
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
            _cache.Set(PlayerCountKey, result, DateTimeOffset.UtcNow.AddDays(1));
            return result;
        }

        private static IQueryable<Player> GetFullPlayerQueryable(LegionTdContext context)
        {
            return context.Players
                .Include(p => p.Matches)
                .ThenInclude(m => m.Fraction)
                .Include(p => p.Matches)
                .ThenInclude(m => m.Match)
                .AsNoTracking();
        }

        private async Task<ActionResult> UpdatePlayerProfiles()
        {
            if (!await CheckIp())
            {
                return Json(new NoPermissionFailure());
            }
            int stepSize = 50;
            await _steamApi.UpdatePlayerInformation(await _db.Players.OrderByDescending(p => p.SteamId).Where(p => p.Avatar == null).Take(stepSize).Select(p => p.SteamId).ToListAsync());
            LoggingUtil.Log($"{stepSize} Player profiles have been requested.");
            return Json(new { success = true });
        }

        private async Task<ActionResult> UpdateRankings()
        {
            if (!await CheckIp())
            {
                return Json(new NoPermissionFailure());
            }
            try {
                await UpdateRanking(RankingTypes.Rating, false);
            } catch (Exception e) {
                LoggingUtil.Error("Failed to update ranking");
                LoggingUtil.Error(e.StackTrace);
                return Json(new { success = false });
            }
            return Json(new { success = true });
        }

        private async Task UpdateRanking(Models.RankingTypes type, bool asc)
        {
            string key = type + "|" + asc;
            _cache.Set(key, true, DateTimeOffset.UtcNow.AddDays(1));

            using (var transcation = await _db.Database.BeginTransactionAsync()) {
                await _db.Database.ExecuteSqlRawAsync($"DELETE FROM Rankings WHERE Type = {(int)type} AND Ascending = {(asc ? 1 : 0)}");
                Console.WriteLine($"Cleared Ranking for {type} {asc}");
                string sql;
                string sqlJoins = "JOIN Matches AS m \n" +
                                    "ON m.MatchId = pm.MatchId \n";
                string sqlWheres = "WHERE m.IsTraining = FALSE \n";
                string sqlSelects, sqlOrderBy;
                switch (type)
                {
                    case RankingTypes.EarnedTangos:
                        sqlSelects = ", SUM(EarnedTangos) AS Gold \n";
                        sqlOrderBy = "ORDER BY Gold " + (asc ? "ASC" : "DESC") + " \n";
                        break;
                    case RankingTypes.EarnedGold:
                        sqlSelects = ", SUM(EarnedGold) AS Gold \n";
                        sqlOrderBy = "ORDER BY Gold " + (asc ? "ASC" : "DESC") + " \n";
                        break;
                    case RankingTypes.Rating:
                    default:
                        sqlSelects = ", SUM(RatingChange) AS Rating \n";
                        sqlOrderBy = "ORDER BY Rating " + (asc ? "ASC" : "DESC") + " \n";
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
                await _db.Database.ExecuteSqlRawAsync(sql);
                await transcation.CommitAsync();
                LoggingUtil.Log("Ranking has been updated.");
            }
        }

        private async Task<ActionResult> UpdateUnitStatistics()
        {
            if (!await CheckIp())
            {
                return Json(new NoPermissionFailure());
            }
            using (var transaction = await _db.Database.BeginTransactionAsync()) {
                try {
                    var units = await _db.Units.ToListAsync();
                    var toWait = new List<Task>();
                    foreach (var unit in units)
                    {
                        await UpdateUnitStatistic(unit.Name);
                    }
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    LoggingUtil.Log("Unit statistics have been updated.");
                    return Json(new { success = true });
                } catch (Exception e) {
                    LoggingUtil.Error("Failed to compute unit statistics");
                    LoggingUtil.Error(e.StackTrace);
                    return Json(new { success = false});
                }
            }
        }

        private async Task UpdateUnitStatistic(string unitName)
        {
            var unit = await GetOrCreateUnit(unitName);
            var timeStamp = DateTimeOffset.UtcNow;
            var now = DateTime.UtcNow;
            var yesterday = now.AddDays(-1);
            var unitData = await _db.PlayerUnitRelations
                .Where(r => r.PlayerMatch.Match.Date >= yesterday && r.PlayerMatch.Match.Date <= now && r.UnitName == unitName && r.PlayerMatch.FractionName == unit.FractionName)
                .ToListAsync();
            var matchData = _db.PlayerMatchData
                .Where(r => r.Match.Date >= yesterday && r.Match.Date <= now && r.FractionName == unit.FractionName);
            int killed = unitData.Sum(d => d.Killed);
            int leaked = unitData.Sum(d => d.Leaked);
            int send = unitData.Sum(d => d.Send);
            int build = unitData.Sum(d => d.Build);
            var gameCount = await matchData.CountAsync();
            var gamesBuild = await matchData.CountAsync(m => m.UnitDatas.Any(u => u.UnitName == unitName && u.Build > 0));
            var gamesWon = await matchData.CountAsync(m => m.Match.Winner == m.Team && m.UnitDatas.Any(u => u.UnitName == unitName && u.Build > 0));
            _db.UnitStatistics.Add(new UnitStatistic
            {
                TimeStamp = timeStamp,
                UnitName = unitName,
                Killed = killed,
                Leaked = leaked,
                Send = send,
                Build = build,
                GamesBuild = gamesBuild,
                GamesEvaluated = gameCount,
                GamesWon = gamesWon
            });
        }

        private async Task<ActionResult> UpdateFractionStatistics()
        {
            if (!await CheckIp())
            {
                return Json(new NoPermissionFailure());
            }
            using (var transaction = await _db.Database.BeginTransactionAsync()) {
                try {
                    var fractions = await _db.Fractions.ToListAsync();
                    foreach (var fraction in fractions)
                    {
                        await UpdateFractionStatistic(fraction.Name);
                    }
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    LoggingUtil.Log("Fraction statistics have been updated.");
                    return Json(new { success = true });
                } catch (Exception e) {
                    LoggingUtil.Error("Failed to compute fraction statistics");
                    LoggingUtil.Error(e.StackTrace);
                    return Json(new { success = false });
                }
            }
        }

        private async Task UpdateFractionStatistic(string fractionName)
        {
            var timeStamp = DateTime.UtcNow;
            var yesterday = timeStamp.AddDays(-1);
            var wins = await _db.Fractions
                .Include(b => b.PlayedMatches)
                    .ThenInclude(m => m.Match)
                .Where(f => f.Name == fractionName)
                .SelectMany(b => b.PlayedMatches.Where(m => m.Match.Date > yesterday))
                .CountAsync(m => m.Team == m.Match.Winner);
            var count = await _db.Fractions
                .Include(b => b.PlayedMatches)
                    .ThenInclude(m => m.Match)
                .Where(f => f.Name == fractionName)
                .SelectMany(b => b.PlayedMatches.Where(m => m.Match.Date > yesterday))
                .CountAsync();
            var numPicks = await _db.Matches
                .Include(m => m.PlayerData)
                .Where(m => m.Date > yesterday)
                .Select(m => m.PlayerData.Count(m => m.FractionName == fractionName))
                .ToListAsync();
            var pickRate = ((float)numPicks.Sum())/numPicks.Count;
            FractionStatistic statistic = new FractionStatistic()
            {
                TimeStamp = timeStamp,
                FractionName = fractionName,
                WonGames = wins,
                LostGames = count - wins,
                PickRate = pickRate
            };
            _db.FractionStatistics.Add(statistic);
        }





        private static class PostMethods
        {
            public const string SavePlayerData = "save_player";
            public const string SaveMatchData = "save_match";
            public const string UpdateAbilityData = "update_abilities";
            public const string UpdateUnitData = "update_units";
            public const string UpdateBuilders = "update_heroes";
        }

        private bool ValidateSecretKey(string secretKey)
        {
            Console.WriteLine($"Received secret key: {secretKey}");
            return secretKey == this._dedicatedServerKey;
        }

        [HttpPost]
        public async Task<ActionResult> Post(string method, int? winner, string playerData, string data, float duration,
            int lastWave, string duelData, long? steamId, string secret_key)
        {
            if (!(await CheckIp() || ValidateSecretKey(secret_key)))
            {
                return Json(new NoPermissionFailure());
            }
            LoggingUtil.Log($"Called method {method}");
            switch (method)
            {
                case PostMethods.SaveMatchData:
                    return await SaveMatchData(winner, playerData, duration, lastWave, duelData);
                case PostMethods.UpdateUnitData:
                    return await UpdateUnitData(data);
                case PostMethods.UpdateAbilityData:
                    return await UpdateAbilityData(data);
                case PostMethods.UpdateBuilders:
                    return await UpdateUnitData(data, "builder");
                case PostMethods.SavePlayerData:
                default:
                    return Json(new InvalidRequestFailure());
            }
        }

        private async Task<bool> CheckIp()
        {
            // if (!_checkIp)
            // {
            //     return true;
            // }
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress;
            var ranges = await GetDotaIpRanges();
            foreach (var range in ranges)
            {
                if (range.IsInRange(ipAddress))
                {
                    LoggingUtil.Log($"Client {ipAddress} is in Range {range.Lower} - {range.Upper}");
                    return true;
                }
            }
            // LoggingUtil.Warn($"Connection to {ipAddress} refused.");
            return false;
        }

        private async Task<List<IpAddressRange>> GetDotaIpRanges()
        {
            List<IpAddressRange> result;
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
                if (datacenters != null) {
                    foreach(var datacenter in datacenters) {
                        var addressRanges = datacenter.Value["address_ranges"] as JArray;
                        if (addressRanges != null) {
                            foreach (var addressRange in addressRanges) {
                                result.Add(IpAddressRange.Parse((string)addressRange));
                            }
                        }
                    }
                }
                result.Add(new IpAddressRange("127.0.0.1", "127.0.0.1"));
                result.Add(new IpAddressRange("::1", "::1"));
                _cache.Set("dota_ip_ranges", result, TimeSpan.FromDays(1));
            }
            return result;
        }

        private async Task<ActionResult> UpdateUnitData(string data, string type="unit")
        {
            if (string.IsNullOrWhiteSpace(data))
                return Json(new MissingArgumentFailure());
            JObject unitData;
            try
            {
                unitData = JObject.Parse(data);
            }
            catch (Exception)
            {
                return Json(new InvalidRequestFailure());
            }
            using (var transaction = await _db.Database.BeginTransactionAsync()) {
                try {
                    var usedAbilities = new List<string>();
                    var unitNames = unitData.Properties().Select(p => p.Name).ToList();
                    var sqlUnitNames = $"({String.Join(", ", unitNames.Select(u => $"'{u}'"))})";
                    var sql = $"SELECT * FROM Units WHERE Name IN {sqlUnitNames}";
                    var existingUnits = await  _db.Units.FromSqlRaw(sql).ToDictionaryAsync(u => u.Name, u => u);
                    await _db.Database.ExecuteSqlRawAsync($"DELETE FROM UnitAbilities WHERE UnitName IN {sqlUnitNames}");
                    foreach (var pair in unitData)
                    {
                        string unitName = pair.Key;
                        string fraction = pair.Value.GetValueOrDefault("LegionFraction") ?? "other";
                        Unit unit = existingUnits.GetValueOrDefault(unitName);
                        if (unit == null) {
                            if (type == "builder") {
                                unit = CreateBuilder(unitName);
                            } else {
                                unit = CreateUnit(unitName);
                            }
                            existingUnits[unitName] = unit;
                        }
                        _db.Attach(unit);
                        unit.FractionName = fraction;
                        unit.UpdateValues(pair.Value);
                    }
                    await _db.SaveChangesAsync();
                    foreach (var pair in unitData)
                    {
                        string unitName = pair.Key;
                        for (int i = 1; i <= 24; i++)
                        {
                            string abilityName = pair.Value.GetValueOrDefault($"Ability{i}");
                            if (!string.IsNullOrWhiteSpace(abilityName)) {
                                var newAbility = new UnitAbility
                                {
                                    UnitName = unitName,
                                    AbilityName = abilityName,
                                    Slot = i
                                };
                                _db.UnitAbilities.Add(newAbility);
                                usedAbilities.Add(abilityName);
                            }
                        }
                    }
                    if (usedAbilities.Count > 0) {
                        sql = $"SELECT * FROM Abilities WHERE Name IN ({String.Join(", ", usedAbilities.Select(a => $"'{a}'"))})";
                        var foundNames = await _db.Abilities.FromSqlRaw(sql).Select(a => a.Name).ToListAsync();
                        usedAbilities.Except(foundNames).ToList().ForEach(a => CreateAbility(a));
                    }
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { Success = true });
                } catch (Exception e) {
                    LoggingUtil.Error($"Updating {type} data failed");
                    LoggingUtil.Error(e.StackTrace);
                    return Json(new { Success = false });
                }
            }
        }

        private async Task<ActionResult> UpdateAbilityData(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return Json(new MissingArgumentFailure());
            JObject abilityData;
            try
            {
                abilityData = JObject.Parse(data);
            }
            catch (Exception)
            {
                return Json(new InvalidRequestFailure());
            }
            using (var transaction =  await _db.Database.BeginTransactionAsync()) {
                try {
                    var seenAbilities = new HashSet<string>();
                    var abilities = new List<Ability>();
                    var abilityNames = abilityData.Properties().Select(p => p.Name).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                    var sql = $"SELECT * FROM Abilities WHERE Name IN ({String.Join(", ", abilityNames.Select(a => $"'{a}'"))})";
                    var existingAbilities = await _db.Abilities.FromSqlRaw(sql).ToDictionaryAsync(u => u.Name, u => u);
                    foreach (var pair in abilityData)
                    {
                        string abilityName = pair.Key;
                        if (!string.IsNullOrWhiteSpace(abilityName) && !seenAbilities.Contains(abilityName)) {
                            seenAbilities.Add(abilityName);
                            Ability ability = existingAbilities.GetValueOrDefault(abilityName) ?? CreateAbility(abilityName);
                            ability.UpdateValues(pair.Value);
                        }
                    }
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { Success = true });
                } catch (Exception e) {
                    LoggingUtil.Error("Updating ability data failed");
                    LoggingUtil.Error(e.StackTrace);
                    return Json(new { Success = false });
                }
            }
        }

        public async Task<ActionResult> SaveMatchData(int? winner, string playerDataString, float duration,
            int lastWave, string duelDataString)
        {
            if (!winner.HasValue || string.IsNullOrWhiteSpace(playerDataString))
                return Json(new MissingArgumentFailure());

            using (var transaction = await _db.Database.BeginTransactionAsync()) {
                try {
                    //Creating Match
                    LoggingUtil.Log("Creating match");
                    Match match = CreateMatch(winner.Value, duration, lastWave);
                    await _db.SaveChangesAsync();

                    //Adding Duels
                    LoggingUtil.Log($"Adding duels to #{match.MatchId}");
                    if (!string.IsNullOrWhiteSpace(duelDataString))
                    {
                        Dictionary<int, Dictionary<String, float>> duelData = null;
                        try
                        {
                            duelData = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<String, float>>>(duelDataString);
                        }
                        catch (Exception)
                        {
                            try
                            {
                                var data = JsonConvert.DeserializeObject<List<Dictionary<String, float>>>(duelDataString);
                                for (int i = 0; i < data.Count; i++)
                                {
                                    duelData[i + 1] = data[i];
                                }
                            }
                            catch (Exception) { }
                        }
                        if (duelData != null)
                        {
                            foreach (var pair in duelData)
                            {
                                var order = pair.Key;
                                var data = pair.Value;
                                Duel duel = CreateDuel(match, order, (int)data["winner"], data["time"]);
                            }
                        }
                    }
                    await _db.SaveChangesAsync();

                    //Adding player Data
                    LoggingUtil.Log($"Creating players for #{match.MatchId}");
                    var playerData =
                        JsonConvert.DeserializeObject<Dictionary<long, Dictionary<string, string>>>(playerDataString);
                    var steamIds = playerData.Keys.ToList();
                    List<Player> players = await GetPlayersOrCreate(steamIds);
                    await _db.SaveChangesAsync();

                    // Enter player data
                    LoggingUtil.Log($"Adding player data to #{match.MatchId}");
                    List<PlayerMatchData> playerMatchDatas = new List<PlayerMatchData>();
                    foreach (var pair in playerData.Zip(players))
                    {
                        var player = pair.Second;
                        Dictionary<string, string> decodedData = pair.First.Value;
                        PlayerMatchData playerMatchData = CreatePlayerMatchData(player,
                            match,
                            decodedData["fraction"],
                            int.Parse(decodedData["team"]),
                            bool.Parse(decodedData["abandoned"]),
                            int.Parse(decodedData["earned_tangos"]),
                            int.Parse(decodedData["earned_gold"]));
                        playerMatchDatas.Add(playerMatchData);
                    }
                    await _db.SaveChangesAsync();

                    // Add unit data
                    LoggingUtil.Log($"Adding player unit to #{match.MatchId}");
                    foreach (var pair in playerMatchDatas.Zip(playerData)) {
                        await CreatePlayerUnitRelations(pair.First, pair.Second.Value);
                    }

                    // Evaluate the match
                    LoggingUtil.Log($"Validating #{match.MatchId}");
                    await DecideIsTraining(match);
                    await ModifyRatings(playerMatchDatas, match);
                    await _steamApi.UpdatePlayerInformation(players.Select(p => p.SteamId));
                    await transaction.CommitAsync();

                    return Json(new { Success = true });
                } catch (Exception e) {
                    LoggingUtil.Error("Saving match data failed");
                    LoggingUtil.Log(playerDataString);
                    LoggingUtil.Log(duelDataString);
                    LoggingUtil.Error(e.StackTrace);
                    return Json(new { Success = false });
                }
            }
        }

        private async Task DecideIsTraining(Match match)
        {
            var ma = await _db.Matches.IgnoreQueryFilters()
                .Include(m => m.PlayerData)
                .SingleAsync(m => m.MatchId == match.MatchId);
            ma.IsTraining = ma.PlayerData.All(p => p.Team == match.Winner) ||
                            ma.PlayerData.All(p => p.Team != match.Winner);
            _db.Entry(ma).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        private async Task ModifyRatings(List<PlayerMatchData> playerMatchDatas, Match match)
        {
            var l = new List<Player>();
            foreach (var pl in playerMatchDatas)
                l.Add(await _db.Players
                    .Include(p => p.Matches)
                    .ThenInclude(m => m.Match.PlayerData)
                    .SingleAsync(player => player.SteamId == pl.PlayerId));
            foreach (var p in l.Select(p => p.Matches.Single(m => m.MatchId == match.MatchId)))
                p.RatingChange = p.CalculateRatingChange();
            await _db.SaveChangesAsync();
        }

        private Duel CreateDuel(Match match, int order, int winner, float time)
        {
            Duel duel = new Duel
            {
                MatchId = match.MatchId,
                Order = order,
                Winner = winner,
                TimeStamp = time
            };
            _db.Duels.Add(duel);
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
                int count = int.Parse(pair.Value);
                PlayerUnitRelation relation = relations.ContainsKey(unitName)
                    ? relations[unitName]
                    : new PlayerUnitRelation
                    {
                        PlayerMatch = playerMatchData,
                        UnitName = unitName
                    };
                relations[unitName] = relation;
                UnitRelationFunctions[type].Invoke(relation, count);
            }
            var result = relations.Values.ToList();

            _db.PlayerUnitRelations.AddRange(result);
            _db.Entry(playerMatchData).State = EntityState.Modified;
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
            _db.Entry(p).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return result;
        }

        private Ability CreateAbility(string abilityName)
        {
            Ability ability;
            if (Regex.IsMatch(abilityName, @".+builder_(spawn|upgrade)_.+"))
            {
                string unitName = "tower_" + Regex.Replace(abilityName, @"_(spawn|upgrade)", "");
                ability = new SpawnAbility
                {
                    Name = abilityName,
                    UnitName = unitName
                };
                _db.SpawnAbilities.Add((SpawnAbility)ability);
            }
            else
            {
                ability = new Ability{
                    Name = abilityName
                };
                _db.Abilities.Add(ability);
            }
            return ability;
        }

        private async Task<Ability> GetOrCreateAbility(string abilityName)
        {
            Ability ability = (await _db.Abilities.FindAsync(abilityName)) ?? CreateAbility(abilityName);
            return ability;
        }

        private Builder CreateBuilder(string builderName)
        {
            Builder builder = new Builder
            {
                Name = builderName
            };
            builder.SetTypeByName();
            string fraction = builder.GetFractionByName();
            builder.FractionName = fraction;
            _db.Builders.Add(builder);
            return builder;
        }

        private async Task<Builder> GetOrCreateBuilder(string builderName)
        {
            Builder builder = (await _db.Builders.FindAsync(builderName)) ?? CreateBuilder(builderName);
            return builder;
        }

        private Unit CreateUnit(string unitName) 
        {
            Unit unit = new Unit
            {
                Name = unitName,
                Experience = 0
            };
            unit.SetTypeByName();
            _db.Units.Add(unit);
            return unit;
        }

        private async Task<Unit> GetOrCreateUnit(string unitName)
        {
            Unit unit = (await _db.Units.FindAsync(unitName)) ?? CreateUnit(unitName);
            return unit;
        }

        private async Task<Fraction> GetOrCreateFraction(string name)
        {
            Fraction result = await _db.Fractions.FindAsync(name);
            if (result == null)
            {
                result = new Fraction { Name = name };
                _db.Fractions.Add(result);
            }
            return result;
        }

        private PlayerMatchData CreatePlayerMatchData(Player player, Match match, string fraction, int team,
            bool abandoned, int earnedTangos, int earnedGold)
        {
            PlayerMatchData result = new PlayerMatchData
            {
                Player = player,
                Match = match,
                Abandoned = abandoned,
                Team = team,
                FractionName = fraction,
                EarnedTangos = earnedTangos,
                EarnedGold = earnedGold
            };
            _db.Entry(player).State = EntityState.Modified;
            _db.Entry(match).State = EntityState.Modified;
            _db.Entry(result.Fraction).State = EntityState.Modified;
            _db.PlayerMatchData.Add(result);

            return result;
        }

        private Match CreateMatch(int winner, float duration, int lastWave)
        {
            Match match = new Match
            {
                Winner = winner,
                Duration = duration,
                LastWave = lastWave,
                Date = DateTime.UtcNow,
                IsTraining = true
            };
            _db.Matches.Add(match);
            return match;
        }

        private async Task<List<Player>> GetPlayersOrCreate(IEnumerable<long> steamIds) {
            var idString = String.Join(", ", steamIds);
            var sql = $"SELECT * IN Players WHERE SteamId IN ({idString})";
            var existingPlayers = await _db.Players.FromSqlRaw(sql).ToDictionaryAsync(p => p.SteamId, p => p);
            var result = new List<Player>();
            foreach (var steamId in steamIds) {
                if (!existingPlayers.ContainsKey(steamId)) {
                    existingPlayers[steamId] = CreatePlayer(steamId);
                }
                result.Append(existingPlayers[steamId]);
            }
            return result;
        }

        private Player CreatePlayer(long steamId) {
            Player result = new Player
            {
                SteamId = steamId,
            };
            _db.Players.Add(result);
            return result;
        }

        private async Task<Player> GetOrCreatePlayer(long steamId)
        {
            Player result = await _db.Players.FindAsync(steamId) ?? CreatePlayer(steamId);
            return result;
        }
    }
}
