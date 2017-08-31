using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using LegionTDServerReborn.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LegionTDServerReborn.Pages
{
    public class PlayerModel : SteamApiModel
    {
        public long PlayerId {get; set;}

        public Player Player {get; set;}

        public SteamPlayer SteamPlayer {get; set;}

        public PlayerModel(IConfiguration configuration)
            :base(configuration) {}

        public void OnGet(long? playerId)
        {
            if (!playerId.HasValue) return;
            PlayerId = playerId.Value;
            using(var db = new LegionTdContext()) {
                Player = db.Players.Include(p => p.MatchDatas).ThenInclude(m => m.Match).Single(p => p.SteamId == PlayerId);
            }
            SteamPlayer = RequestPlayers(PlayerId)[PlayerId];
        }
    }
}
