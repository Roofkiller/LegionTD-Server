using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using LegionTDServerReborn.Models;
using LegionTDServerReborn.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LegionTDServerReborn.Pages
{
    public class MatchModel : PageModel
    {
        public int MatchId {get; set;}

        public Match Match {get; set;}

        public Dictionary<long, SteamInformation> Players {get; set;}

        private LegionTdContext _db;

        private SteamApi _steamApi;

        public MatchModel(SteamApi steamApi, LegionTdContext db) {
                _db = db;
                _steamApi = steamApi;
        }

        public async Task OnGetAsync(int matchId)
        {
            MatchId = matchId;
            Match = await _db.Matches.Include(m => m.Duels).Include(m => m.PlayerDatas).SingleOrDefaultAsync(m => m.MatchId == matchId);
            Players = await _steamApi.GetPlayerInformation(Match.PlayerDatas.Select(p => p.PlayerId).ToArray());
        }

    }
}
