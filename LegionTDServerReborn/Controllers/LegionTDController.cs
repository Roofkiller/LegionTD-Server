using LegionTDServerReborn.Models;
using LegionTDServerReborn.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text;
using System.Text.Json;
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
        private readonly FileLogger _fileLogger;
        private const string PlayerCountKey = "player_count";
        private readonly string _dedicatedServerKey;


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public LegionTdController(
            LegionTdContext context, 
            SteamApi steamApi,
            FileLogger fileLogger,
            IMemoryCache cache, 
            IConfiguration configuration)
        {
            _db = context;
            _cache = cache;
            _fileLogger = fileLogger;
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
            var steamIds = string.Join(", ", ids);
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
            _cache.Set(PlayerCountKey, result, DateTime.UtcNow.AddDays(1));
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

        private async Task UpdateRanking(RankingTypes type, bool asc)
        {
            string key = type + "|" + asc;
            _cache.Set(key, true, DateTime.UtcNow.AddDays(1));

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
                return Json(new NoPermissionFailure());

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                await UpdateUnitStatistics_Internal();
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                LoggingUtil.Log("Unit statistics have been updated.");
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                LoggingUtil.Error("Failed to compute unit statistics");
                LoggingUtil.Error(e.StackTrace);
                return Json(new { success = false });
            }
        }

        private async Task UpdateUnitStatistics_Internal()
        {
            var now = DateTime.UtcNow;
            var yesterday = now.AddDays(-1);
            var matches = await _db.PlayerMatchData
                .Include(m => m.Match)
                .Where(m => m.UnitData != null && m.Match.Date > yesterday && m.Match.Date <= now)
                .ToListAsync();
            var unitData = matches
                .SelectMany(m => m.UnitData.Object)
                .GroupBy(d => d.Key, d => d.Value)
                .ToDictionary(d => d.Key);
            var units = await _db.Units.ToListAsync();
            var gamesByFraction = matches
                .GroupBy(m => m.FractionName)
                .ToDictionary(g => g.Key, g => (g, g.Count()));
            foreach (var unit in units)
            {
                var statistics = new UnitData();
                if (unitData.TryGetValue(unit.Name, out var stats))
                {
                    Console.WriteLine(unit.Name);
                    foreach (var s in stats)
                        statistics += s;
                }

                int gameCount = 0, gamesBuilt = 0, gamesWon = 0;
                if (gamesByFraction.TryGetValue(unit.FractionName, out var games))
                {
                    var selectedMatches = games.g;
                    gameCount = games.Item2;
                    var matchesBuilt = selectedMatches.Where(m => m.UnitData.Object.ContainsKey(unit.Name)
                                                               && m.UnitData.Object[unit.Name].Built > 0);
                    gamesBuilt = matchesBuilt.Count();
                    gamesWon = matchesBuilt.Count(m => m.Match.Winner == m.Team);
                }
                _db.UnitStatistics.Add(new UnitStatistic
                {
                    TimeStamp = now,
                    UnitName = unit.Name,
                    Killed = statistics.Killed,
                    Leaked = statistics.Leaked,
                    Send = statistics.Sent,
                    Build = statistics.Built,
                    GamesBuild = gamesBuilt,
                    GamesEvaluated = gameCount,
                    GamesWon = gamesWon
                });
            }
        }

        private async Task<ActionResult> UpdateFractionStatistics()
        {
            if (!await CheckIp())
            {
                return Json(new NoPermissionFailure());
            }
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                await UpdateFractionStatistics_Internal();
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                LoggingUtil.Log("Fraction statistics have been updated.");
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                LoggingUtil.Error("Failed to compute fraction statistics");
                LoggingUtil.Error(e.StackTrace);
                return Json(new { success = false });
            }
        }

        private async Task UpdateFractionStatistics_Internal()
        {
            var fractions = await _db.Fractions.ToListAsync();
            var timeStamp = DateTime.UtcNow;
            var yesterday = timeStamp.AddDays(-1);
            var playerMatches = await _db.PlayerMatchData
                .Include(m => m.Match)
                .Where(m => m.Match.Date > yesterday && m.Match.Date <= timeStamp)
                .ToListAsync();
            var totalGames = playerMatches.Select(p => p.MatchId).Distinct().Count();
            foreach (var fraction in fractions)
            {
                var playedMatches = playerMatches
                    .Where(p => p.FractionName == fraction.Name)
                    .ToList();
                int wins = playedMatches
                    .Where(p => p.Match.Winner == p.Team)
                    .Count();
                int numPicks = playedMatches.Count;
                float pickRate = ((float)numPicks) / totalGames;
                _db.FractionStatistics.Add(new FractionStatistic()
                {
                    TimeStamp = timeStamp,
                    FractionName = fraction.Name,
                    WonGames = wins,
                    LostGames = numPicks - wins,
                    PickRate = pickRate
                });
            }
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
                reader.Close();
                var json = JsonDocument.Parse(content).RootElement;
                if (json.TryGetProperty("data_centers", out var datacenters))
                {
                    foreach (var datacenter in datacenters.EnumerateArray())
                    {
                        if (datacenter.TryGetProperty("address_ranges", out var addressRanges))
                        {
                            foreach (var addressRange in addressRanges.EnumerateArray())
                            {
                                result.Add(IpAddressRange.Parse(addressRange.ToString()));
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

        private async Task<ActionResult> UpdateUnitData(string dataString, string type="unit")
        {
            if (string.IsNullOrWhiteSpace(dataString))
                return Json(new MissingArgumentFailure());
            if (!dataString.TryToJson(out JsonDocument parsedUnitData))
                return Json(new InvalidRequestFailure());
            var unitData = parsedUnitData.RootElement;

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var properties = unitData.EnumerateObject().Where(p => !string.IsNullOrWhiteSpace(p.Name));
                var unitNames = properties.Select(p => p.Name);
                var units = await _db.GetOrCreateAsync(unitNames, u => u.Name, name => CreateUnitOrBuilder(name, type));
                foreach (var (data, unit) in properties.Select(p => p.Value).Zip(units))
                {
                    unit.UpdateValues(data);
                }
                await _db.SaveChangesAsync();

                // Remove all old links between abilities and units
                var sqlUnitNames = string.Join(", ", unitNames.Select(u => $"'{u}'"));
                var usedAbilities = new List<string>();
                await _db.Database.ExecuteSqlRawAsync($"DELETE FROM UnitAbilities WHERE UnitName IN ({sqlUnitNames})");
                foreach (var unitObject in unitData.EnumerateObject())
                {
                    for (int i = 1; i <= 24; i++)
                    {
                        string abilityName = unitObject.Value.GetValueOrDefault($"Ability{i}");
                        if (!string.IsNullOrWhiteSpace(abilityName))
                        {
                            var newAbility = new UnitAbility
                            {
                                UnitName = unitObject.Name,
                                AbilityName = abilityName,
                                Slot = i
                            };
                            _db.UnitAbilities.Add(newAbility);
                            usedAbilities.Add(abilityName);
                        }
                    }
                }
                if (usedAbilities.Count > 0)
                {
                    await _db.GetOrCreateAsync(usedAbilities, a => a.Name, CreateAbility);
                }
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { Success = true });
            }
            catch (Exception e)
            {
                LoggingUtil.Error($"Updating {type} data failed");
                await _fileLogger.LogToFile($"update_{type}_data", new Dictionary<string, object>{
                    { "exception", e.ToString() },
                    { "errorSource", e.Source },
                    { "targetSite", e.TargetSite }
                });
                return Json(new { Success = false });
            }
        }

        private async Task<ActionResult> UpdateAbilityData(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return Json(new MissingArgumentFailure());
            if (!data.TryToJson(out JsonDocument parsedAbilityData))
                return Json(new InvalidRequestFailure());
            var abilityData = parsedAbilityData.RootElement;
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var properties = abilityData.EnumerateObject().Where(p => !string.IsNullOrWhiteSpace(p.Name));
                var abilities = await _db.GetOrCreateAsync(properties.Select(p => p.Name), a => a.Name, CreateAbility);
                foreach (var (values, ability) in properties.Select(p => p.Value).Zip(abilities))
                {
                    ability.UpdateValues(values);
                }
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { Success = true });
            }
            catch (Exception e)
            {
                LoggingUtil.Error("Updating ability data failed");
                LoggingUtil.Error(e.ToString());
                await _fileLogger.LogToFile("update_ability_data", new Dictionary<string, object>{
                    { "exception", e.ToString() },
                    { "errorSource", e.Source },
                    { "targetSite", e.TargetSite }
                });
                return Json(new { Success = false });
            }
        }

        public async Task<ActionResult> SaveMatchData(int? winner, string playerDataString, float duration,
            int lastWave, string duelDataString)
        {
            if (!winner.HasValue || string.IsNullOrWhiteSpace(playerDataString))
                return Json(new MissingArgumentFailure());

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                //Creating Match
                LoggingUtil.Log("Creating match");
                Match match = new Match
                {
                    Winner = winner.Value,
                    Duration = duration,
                    LastWave = lastWave,
                    Date = DateTime.UtcNow,
                    IsTraining = true
                };
                _db.Matches.Add(match);
                await _db.SaveChangesAsync();

                //Adding Duels
                LoggingUtil.Log($"Adding duels to #{match.MatchId}");
                if (duelDataString.TryToJson(out JsonDocument duelDocument))
                {
                    foreach (var duelProp in duelDocument.RootElement.EnumerateObject())
                    {
                        var order = int.Parse(duelProp.Name);
                        var time = duelProp.Value.GetFloatOrDefault("time");
                        var duelWinner = duelProp.Value.GetIntOrDefault("winner");
                        Duel duel = new Duel
                        {
                            MatchId = match.MatchId,
                            Order = order,
                            Winner = duelWinner,
                            TimeStamp = time
                        };
                        _db.Duels.Add(duel);
                    }
                } else
                {
                    LoggingUtil.Warn($"No duel data available for Game #{match.MatchId}");
                }
                await _db.SaveChangesAsync();

                //Adding player Data
                LoggingUtil.Log($"Creating players for #{match.MatchId}");
                var playerObjs = playerDataString.ToJsonElement();
                var steamIds = playerObjs.EnumerateObject().Select(p => long.Parse(p.Name)).ToList();
                var players = await _db.GetOrCreateAsync(steamIds, p => p.SteamId, steamId => new Player { SteamId = steamId });
                await _db.SaveChangesAsync();

                // Enter player data
                LoggingUtil.Log($"Found {players.Count} of {steamIds.Count} players; Adding player data to #{match.MatchId}");
                var playerData = new List<PlayerMatchData>();
                foreach (var (steamId, player) in steamIds.Zip(players))
                {
                    var data = playerObjs.GetProperty(steamId.ToString());
                    var unitData = ExtractPlayerUnitData(data);
                    var newData = new PlayerMatchData
                    {
                        Player = player,
                        Match = match,
                        Abandoned = data.GetBoolOrDefault("abandoned"),
                        Team = data.GetIntOrDefault("team"),
                        FractionName = data.GetValueOrDefault("fraction"),
                        EarnedTangos = data.GetIntOrDefault("earned_tangos"),
                        EarnedGold = data.GetIntOrDefault("earned_gold"),
                        UnitData = unitData
                    };
                    _db.PlayerMatchData.Add(newData);
                    playerData.Add(newData);
                }
                await _db.SaveChangesAsync();

                // Ensure all units exists
                var unitNames = playerData.SelectMany(p => p.UnitData.Object.Keys).ToList();
                var units = await _db.GetOrCreateAsync(unitNames, u => u.Name, name => CreateUnitOrBuilder(name));
                await _db.SaveChangesAsync();

                // Now extract the units from the properties
                LoggingUtil.Log($"Added match data for {playerData.Count} player; Computing Player stats #{match.MatchId}");
                var experiences = units.ToDictionary(u => u.Name, u => u.Experience);
                _db.AttachRange(playerData);
                foreach (var pData in playerData)
                    pData.CalculateStats(experiences);
                await _db.SaveChangesAsync();

                // Evaluate the match
                LoggingUtil.Log($"Validating #{match.MatchId}");
                match.IsTraining = DecideIsTraining(match, playerData);
                await ModifyRatings(playerData, match);
                await _db.SaveChangesAsync();

                // Query steam info for all players
                await _steamApi.UpdatePlayerInformation(players.Select(p => p.SteamId));
                await transaction.CommitAsync();

                LoggingUtil.Log($"Succesfully saved #{match.MatchId}. Is training: {match.IsTraining}");
                return Json(new { Success = true });
            }
            catch (Exception e)
            {
                var logFile = await _fileLogger.LogToFile("save_match_error", new Dictionary<string, object>{
                    { "winner", winner },
                    { "duration", duration },
                    { "lastWave", lastWave },
                    { "playerData", playerDataString },
                    { "duelData", duelDataString },
                    { "exception", e.ToString() },
                    { "errorSource", e.Source },
                    { "targetSite", e.TargetSite }
                });
                LoggingUtil.Error($"Saving match data failed; logged to {logFile}");
                return Json(new { Success = false });
            }
        }

        private bool DecideIsTraining(Match match, IEnumerable<PlayerMatchData> playerData)
        {
            return playerData.All(p => p.Team == match.Winner) ||
                   playerData.All(p => p.Team != match.Winner);
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
        }


        private const string UnitStatPrefix = "unitstat_";
        private Dictionary<string, UnitData> ExtractPlayerUnitData(JsonElement data)
        {
            return data.EnumerateObject()
                .Where(p => p.Name.StartsWith(UnitStatPrefix))
                .ToDictionary(p => p.Name.Replace(UnitStatPrefix, ""), p => new UnitData
                {
                    Built = p.Value.GetIntOrDefault("built"),
                    Killed = p.Value.GetIntOrDefault("killed"),
                    Leaked = p.Value.GetIntOrDefault("leaked"),
                    Sent = p.Value.GetIntOrDefault("sent")
                });

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
            }
            else
            {
                ability = new Ability{
                    Name = abilityName
                };
            }
            return ability;
        }
        
        private Unit CreateUnitOrBuilder(string unitName, string type="unit")
        {
            switch(type)
            {
                case "builder":
                    Builder builder = new Builder
                    {
                        Name = unitName
                    };
                    builder.SetTypeByName();
                    builder.FractionName = builder.GetFractionByName();
                    return builder;
                case "unit":
                    Unit unit = new Unit
                    {
                        Name = unitName,
                        Experience = 0
                    };
                    unit.SetTypeByName();
                    return unit;
                default:
                    throw new ArgumentException($"{type} is invalid unit type");
            }
        }
    }
}
