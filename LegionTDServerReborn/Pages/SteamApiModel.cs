using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using LegionTDServerReborn.Models;
using System.IO;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;

namespace LegionTDServerReborn.Pages
{
    public abstract class SteamApiModel : PageModel
    {
        public const string SteamPlayerApi = "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/";
        
        protected string SteamApiKey => _configuration["steamApiKey"];

        private IConfiguration _configuration;

        public SteamApiModel(IConfiguration configuration) {
            this._configuration = configuration;
        }

        protected Dictionary<long, SteamPlayer> RequestPlayers(params long[] ids) {
            StringBuilder param = new StringBuilder();
            foreach(var player in ids) {
                param.Append(player + ",");
            }
            WebRequest request = WebRequest.CreateHttp($"{SteamPlayerApi}?key={SteamApiKey}&steamids={param.ToString()}");
            request.Method = "GET";
            var response = request.GetResponse();
            var responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            JObject parsed = JObject.Parse(content);
            var playerInfos = parsed["response"]["players"].ToArray();
            Dictionary<long, SteamPlayer> result = new Dictionary<long, SteamPlayer>();
            foreach(var playerInfo in playerInfos) {
                var id = long.Parse(playerInfo["steamid"].ToString());
                result[id] = JsonConvert.DeserializeObject<SteamPlayer>(playerInfo.ToString());
            }
            return result;
        }
    }
}