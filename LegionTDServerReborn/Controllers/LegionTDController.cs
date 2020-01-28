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

        private async Task UpdateRanking(RankingTypes type, bool asc)
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
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var units = await _db.Units.ToListAsync();
                var toWait = new List<Task>();
                foreach (var unit in units)
                {
                    await UpdateUnitStatistic(unit);
                }
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

        private async Task UpdateUnitStatistic(Unit unit)
        {
            var timeStamp = DateTimeOffset.UtcNow;
            var now = DateTime.UtcNow;
            var yesterday = now.AddDays(-1);
            var unitData = await _db.PlayerUnitRelations
                .Where(r => r.PlayerMatch.Match.Date >= yesterday && r.PlayerMatch.Match.Date <= now && r.UnitName == unit.Name && r.PlayerMatch.FractionName == unit.FractionName)
                .ToListAsync();
            var matchData = _db.PlayerMatchData
                .Where(r => r.Match.Date >= yesterday && r.Match.Date <= now && r.FractionName == unit.FractionName);
            int killed = unitData.Sum(d => d.Killed);
            int leaked = unitData.Sum(d => d.Leaked);
            int send = unitData.Sum(d => d.Send);
            int build = unitData.Sum(d => d.Build);
            var gameCount = await matchData.CountAsync();
            var gamesBuild = await matchData.CountAsync(m => m.UnitDatas.Any(u => u.UnitName == unit.Name && u.Build > 0));
            var gamesWon = await matchData.CountAsync(m => m.Match.Winner == m.Team && m.UnitDatas.Any(u => u.UnitName == unit.Name && u.Build > 0));
            _db.UnitStatistics.Add(new UnitStatistic
            {
                TimeStamp = timeStamp,
                UnitName = unit.Name,
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
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var fractions = await _db.Fractions.ToListAsync();
                foreach (var fraction in fractions)
                {
                    await UpdateFractionStatistic(fraction.Name);
                }
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

        private async Task<ActionResult> UpdateUnitData(string dataString, string type="unit")
        {
            if (string.IsNullOrWhiteSpace(dataString))
                return Json(new MissingArgumentFailure());
            JObject unitData;
            try
            {
                unitData = JObject.Parse(dataString);
            }
            catch (Exception)
            {
                return Json(new InvalidRequestFailure());
            }
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var props = unitData.Properties().Where(p => !string.IsNullOrWhiteSpace(p.Name));
                var unitNames = unitData.Properties().Select(p => p.Name);
                var units = await _db.GetOrCreateAsync(unitNames, u => u.Name, name => CreateUnitOrBuilder(name, type));
                foreach (var (data, unit) in props.Select(p => p.Value).Zip(units))
                {
                    unit.UpdateValues(data);
                }
                await _db.SaveChangesAsync();

                // Remove all old links between abilities and units
                var sqlUnitNames = string.Join(", ", unitNames.Select(u => $"'{u}'"));
                var usedAbilities = new List<string>();
                await _db.Database.ExecuteSqlRawAsync($"DELETE FROM UnitAbilities WHERE UnitName IN ({sqlUnitNames})");
                foreach (var (unitName, values) in unitData)
                {
                    for (int i = 1; i <= 24; i++)
                    {
                        string abilityName = values.GetValueOrDefault($"Ability{i}");
                        if (!string.IsNullOrWhiteSpace(abilityName))
                        {
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
                LoggingUtil.Error(e.StackTrace);
                return Json(new { Success = false });
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
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var properties = abilityData.Properties().Where(p => !string.IsNullOrWhiteSpace(p.Name));
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
                LoggingUtil.Error(e.StackTrace);
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
                if (!string.IsNullOrWhiteSpace(duelDataString) && duelDataString.Length > 3)
                {
                    var duelData = JObject.Parse(duelDataString);
                    if (duelData != null)
                    {
                        foreach (var (id, data) in duelData)
                        {
                            var order = int.Parse(id);
                            var time = data.GetValueOrDefaultInt("time");
                            var duelWinner = data.GetValueOrDefaultInt("winner");
                            Duel duel = new Duel
                            {
                                MatchId = match.MatchId,
                                Order = order,
                                Winner = duelWinner,
                                TimeStamp = time
                            };
                            _db.Duels.Add(duel);
                        }
                    }
                } else
                {
                    LoggingUtil.Warn($"No duel data available for Game #{match.MatchId}");
                }
                await _db.SaveChangesAsync();

                //Adding player Data
                LoggingUtil.Log($"Creating players for #{match.MatchId}");
                var playerObjs = JObject.Parse(playerDataString);
                var steamIds = playerObjs.Properties().Select(p => long.Parse(p.Name)).ToList();
                var players = await _db.GetOrCreateAsync(steamIds, p => p.SteamId, steamId => new Player { SteamId = steamId });
                await _db.SaveChangesAsync();

                // Enter player data
                LoggingUtil.Log($"Found {players.Count} of {steamIds.Count} players; Adding player data to #{match.MatchId}");
                var playerData = new List<PlayerMatchData>();
                foreach (var (steamId, player) in steamIds.Zip(players))
                {
                    var data = playerObjs[steamId.ToString()] as JObject;
                    var newData = new PlayerMatchData
                    {
                        Player = player,
                        Match = match,
                        Abandoned = data.GetValueOrDefaultInt("abandoned") > 1,
                        Team = data.GetValueOrDefaultInt("team"),
                        FractionName = data.GetValueOrDefault("fraction"),
                        EarnedTangos = data.GetValueOrDefaultInt("earned_tangos"),
                        EarnedGold = data.GetValueOrDefaultInt("earned_gold")
                    };
                    _db.PlayerMatchData.Add(newData);
                    playerData.Add(newData);
                }
                await _db.SaveChangesAsync();

                // Now extract the units from the properties
                LoggingUtil.Log($"Added match data for {playerData.Count} player; Adding missing units");
                var unitNames = playerObjs.Properties() // from base to players
                    .SelectMany(p => (p.Value as JObject).Properties()) // from players to properties
                    .Select(p => p.Name) // all player property names
                    .Distinct() // make them unique
                    .Select(p => DecomposePlayerUnitIdentifier(p).unitName) // Get the actual unitname
                    .Where(u => u != null) // Remove null
                    .Distinct(); // Remove duplicates (in case a unit got killed and built, etc.)
                // Add missing units
                var newUnits = new List<string>();
                await _db.GetOrCreateAsync(unitNames, u => u.Name, name => {
                    var result = new Unit { Name = name, Experience = 0 };
                    result.SetTypeByName();
                    newUnits.Add(name);
                    return result;
                });
                await _db.SaveChangesAsync();

                // Add unit data
                LoggingUtil.Log($"Added {newUnits.Count} missing units; Adding player unit to #{match.MatchId}");
                foreach (var (pData, steamId) in playerData.Zip(steamIds))
                {
                    var newRelations = CreatePlayerUnitRelations(pData, playerObjs[steamId.ToString()] as JObject).ToList();
                    _db.AddRange(newRelations);
                }
                await _db.SaveChangesAsync();
                playerData = playerData
                    .Select(async p => await UpdatePlayerMatchStatistics(p))
                    .Select(t => t.Result)
                    .ToList();
                _db.UpdateRange(playerData);
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
                    { "errorMessage", e.Message },
                    { "stackTrace", e.StackTrace },
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

        private static readonly Dictionary<string, Action<PlayerUnitRelation, int>> UnitRelationFunctions =
            new Dictionary<string, Action<PlayerUnitRelation, int>>
            {
                {"killed_", (relation, count) => relation.Killed = count},
                {"build_", (relation, count) => relation.Build = count},
                {"leaked_", (relation, count) => relation.Leaked = count},
                {"send_", (relation, count) => relation.Send = count}
            };

        private static (string unitName, string type) DecomposePlayerUnitIdentifier(string identifier)
        {
            foreach (var t in UnitRelationFunctions.Keys)
            {
                if (identifier.StartsWith(t))
                {
                    return (identifier.Replace(t, ""), t);
                }
            }
            return (null, null);
        }

        private IEnumerable<PlayerUnitRelation> CreatePlayerUnitRelations(PlayerMatchData playerMatchData,
            JObject decodedData)
        {
            var result = new List<PlayerUnitRelation>();
            var updates = decodedData.Properties()
                .Select(p => new { Identifier = DecomposePlayerUnitIdentifier(p.Name), p.Value })
                .Where(p => !string.IsNullOrWhiteSpace(p.Identifier.unitName))
                .Select(p => new { p.Identifier, Value = p.Value.Value<int>() })
                .GroupBy(p => p.Identifier.unitName, p => new { Func = UnitRelationFunctions[p.Identifier.type], p.Value });

            return updates.Select(g =>
            {
                var result = new PlayerUnitRelation
                {
                    PlayerId = playerMatchData.PlayerId,
                    MatchId = playerMatchData.MatchId,
                    UnitName = g.Key
                };
                foreach (var update in g)
                {
                    update.Func(result, update.Value);
                }
                LoggingUtil.Log($"{result.PlayerId} {result.UnitName} {result.Leaked} {result.Killed} {result.Send} {result.Build}");
                return result;
            });
        }

        private async Task<PlayerMatchData> UpdatePlayerMatchStatistics(PlayerMatchData playerData)
        {
            var extendedData = await _db.PlayerMatchData
                .IgnoreQueryFilters()
                .Include(pd => pd.UnitDatas)
                .ThenInclude(r => r.Unit)
                .Include(pd => pd.Match)
                .ThenInclude(pd => pd.Duels)
                .SingleAsync(pd => pd.MatchId == playerData.MatchId && pd.PlayerId == playerData.PlayerId);
            extendedData.CalculateStats();
            return extendedData;
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
        
        private Unit CreateUnitOrBuilder(string unitName, string type)
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
