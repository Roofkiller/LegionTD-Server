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
    public class PlayerModel : PageModel
    {
        public long PlayerId {get; set;}

        public Player Player {get; set;}

        public SteamInformation SteamPlayer {get; set;}

        private LegionTdContext _db;

        private SteamApi _steamApi;

        public PlayerModel(SteamApi steamApi, LegionTdContext db) {
            _db = db;
            _steamApi = steamApi;
        }

        public async Task OnGetAsync(long? playerId)
        {
            PlayerId = playerId ?? -1;
            if (PlayerId != -1) {
                Player = await _db.Players.IgnoreQueryFilters()
                                    .Include(p => p.Matches)
                                    .ThenInclude(m => m.Match)
                                    .Include(p => p.Rankings)
                                    .SingleOrDefaultAsync(p => p.SteamId == PlayerId);
                if (Player == null) {
                    PlayerId = -1;
                    return;
                }
                var steamPlayer = await _steamApi.GetPlayerInformation(PlayerId);
                if (steamPlayer.ContainsKey(PlayerId)) {
                    SteamPlayer = steamPlayer[PlayerId];
                } else {
                    SteamPlayer = new SteamInformation() {
                        PersonaName = PlayerId.ToString(),
                        Avatar = ""
                    };
                    PlayerId = -1;
                }
            }
        }
    }
}
