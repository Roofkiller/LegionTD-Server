using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using LegionTDServerReborn.Models;
using LegionTDServerReborn.Extensions;
using System.IO;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace LegionTDServerReborn.Services
{
    public class SteamApi {

        public string SteamPlayerApi {get; private set;}
        
        public string SteamApiKey {get; private set;}

        private LegionTdContext _db;

        public SteamApi(IConfiguration configuration, LegionTdContext db) {
            SteamPlayerApi = configuration["steamApi"];
            SteamApiKey = configuration["steamApiKey"];
            _db = db;
        }

        public async Task<Player> GetPlayerInformation(long playerIds, Func<IQueryable<Player>, IQueryable<Player>> query = null) {
            var result = await GetPlayerInformation(new long[]{playerIds}, query);
            return result.Count > 0 ? result.Values.First() : null;
        }


        public async Task<Dictionary<long, Player>> GetPlayerInformation(IEnumerable<long> playerIds, Func<IQueryable<Player>, IQueryable<Player>> query = null) {
            if (query == null) {
                query = q => q;
            }
            Dictionary<long, Player> result = new Dictionary<long, Player>();
            List<long> toRequest = new List<long>();
            var ids = playerIds.ToArray();
            var saved = await query(_db.Players)
                .Where(s => ids.Contains(s.SteamId))
                .AsNoTracking()
                .ToListAsync();
            foreach(var id in ids) {
                var info = saved.FirstOrDefault(s => s.SteamId == id);
                if (info == null || info.Avatar == null) {
                    toRequest.Add(id);
                } else {
                    result[id] = info;
                }
            }
            var requested = await UpdatePlayerInformation_Internal(toRequest, query);
            foreach(var entry in requested) {
                result[entry.Key] = entry.Value;
            }
            return result;
        }
        
        public async Task<Dictionary<long, Player>> UpdatePlayerInformation(IEnumerable<long> ids) {
            return await UpdatePlayerInformation_Internal(ids);
        }

        private async Task<Dictionary<long, Player>> UpdatePlayerInformation_Internal(IEnumerable<long> playerIds, Func<IQueryable<Player>, IQueryable<Player>> query = null) {
            if (query == null) {
                query = q => q;
            }
            var ids = playerIds.ToArray();
            var data = await RequestPlayerInformation(ids);
            var players = await query(_db.Players).Where(p => ids.Contains(p.SteamId)).ToListAsync();
            var result = new Dictionary<long, Player>(data.Count);
            foreach(var entry in data) {
                var player = players.FirstOrDefault(p => p.SteamId == entry.Key);
                if (player == null) {
                    _db.Players.Add(player = new Player{SteamId = entry.Key, Matches = new List<PlayerMatchData>()});
                } else {
                    _db.Entry(player).State = EntityState.Modified;
                }
                player.Avatar = entry.Value.Avatar;
                player.PersonaName = entry.Value.PersonaName;
                player.RealName = entry.Value.RealName;
                player.ProfileUrl = entry.Value.ProfileUrl;
                result[entry.Key] = player;
            }
            await _db.SaveChangesAsync();
            return result;
        }

        public async Task<Dictionary<long, Player>> RequestPlayerInformation(IEnumerable<long> ids) {
            StringBuilder param = new StringBuilder();
            foreach(var player in ids) {
                param.Append(player + ",");
            }
            WebRequest request = WebRequest.CreateHttp($"{SteamPlayerApi}?key={SteamApiKey}&steamids={param.ToString()}");
            request.Method = "GET";
            var response = await request.GetResponseAsync();
            var responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
            string content = await reader.ReadToEndAsync();
            JObject parsed = JObject.Parse(content);
            var playerInfos = parsed["response"]["players"].ToArray();
            Dictionary<long, Player> result = new Dictionary<long, Player>();
            foreach(var playerInfo in playerInfos) {
                var id = long.Parse(playerInfo["steamid"].ToString());
                result[id] = JsonConvert.DeserializeObject<Player>(playerInfo.ToString());
            }
            return result;
        }
    }
}
