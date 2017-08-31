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
    public class MatchModel : SteamApiModel
    {
        public int MatchId {get; set;}

        public Match Match {get; set;}

        public Dictionary<long, SteamPlayer> Players {get; set;}

        public MatchModel(IConfiguration configuration)
            :base(configuration) {
        }

        public void OnGet(int matchId)
        {
            MatchId = matchId;
            using(var db = new LegionTdContext()) {
                Match = db.Matches.Include(m => m.Duels).Include(m => m.PlayerDatas).SingleOrDefault(m => m.MatchId == matchId);
            }
            Players = RequestPlayers(Match.PlayerDatas.Select(p => p.PlayerId).ToArray());
        }

    }
}
