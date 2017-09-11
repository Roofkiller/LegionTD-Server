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

        public async Task<Dictionary<long, SteamInformation>> GetPlayerInformation(params long[] ids) {
            Dictionary<long, SteamInformation> result = new Dictionary<long, SteamInformation>();
            List<long> toRequest = new List<long>();
            var saved = await _db.SteamInformation
                .OrderByDescending(s => s.Time)
                .Where(s => ids.Contains(s.SteamId))
                .ToListAsync();
            foreach(var id in ids) {
                var info = saved.FirstOrDefault(s => s.SteamId == id);
                if (info == null) {
                    toRequest.Add(id);
                } else {
                    result[id] = info;
                }
            }
            var requested = await RequestPlayerInformation(toRequest.ToArray());
            foreach(var entry in requested) {
                result[entry.Key] = entry.Value;
                entry.Value.Time = DateTimeOffset.UtcNow;
            }
            if (requested.Count > 0) {
                _db.SteamInformation.AddRange(requested.Values);
                await _db.SaveChangesAsync();
            }
            return result;
        }

        public async Task<Dictionary<long, SteamInformation>> RequestPlayerInformation(params long[] ids) {
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
            Dictionary<long, SteamInformation> result = new Dictionary<long, SteamInformation>();
            foreach(var playerInfo in playerInfos) {
                var id = long.Parse(playerInfo["steamid"].ToString());
                result[id] = JsonConvert.DeserializeObject<SteamInformation>(playerInfo.ToString());
            }
            return result;
        }
    }
}
