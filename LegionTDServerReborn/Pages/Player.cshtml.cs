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

        private LegionTdContext _db;

        public PlayerModel(IConfiguration configuration, LegionTdContext db)
            :base(configuration) {
                _db = db;
        }

        public void OnGet(long? playerId)
        {
            PlayerId = playerId ?? -1;
            if (PlayerId != -1) {
                try {
                    Player = _db.Players.Include(p => p.MatchDatas)
                                        .ThenInclude(m => m.Match)
                                        .Include(p => p.Rankings)
                                        .Single(p => p.SteamId == PlayerId);
                } catch (Exception) {
                    PlayerId = -1;
                    return;
                }
                var steamPlayer = RequestPlayers(PlayerId);
                if (steamPlayer.ContainsKey(PlayerId)) {
                    SteamPlayer = steamPlayer[PlayerId];
                } else {
                    SteamPlayer = new SteamPlayer() {
                        PersonaName = PlayerId.ToString(),
                        Avatar = ""
                    };
                    PlayerId = -1;
                }
            }
        }
    }
}
