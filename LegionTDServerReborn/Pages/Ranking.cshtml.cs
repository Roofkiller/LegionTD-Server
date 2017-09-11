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
    public class RankingModel : PageModel
    {
        public const int PlayersPerSite = 50;
        public Dictionary<long, SteamInformation> SteamPlayers {get; set;}
        public List<Ranking> Ranking {get; set;}
        public List<Player> Players {get; set;} = new List<Player>();

        private LegionTdContext _db;

        public int Site {get; set;}
        public int PageCount => (int)Math.Floor(PlayerCount / (float)PlayersPerSite);
        public int PlayerCount {get; set;}

        private SteamApi _steamApi;

        public RankingModel(SteamApi steamApi, LegionTdContext db) {
            _db = db;
            _steamApi = steamApi;
        }

        public async Task OnGetAsync(int? site)
        {
            Site = site ?? 1;
            Ranking = (await _db.Rankings
                .Where(r => r.Position >= (Site - 1) * PlayersPerSite 
                         && r.Position < Site * PlayersPerSite)
                .ToListAsync())
                .OrderBy(r => r.Position)
                .ToList();

            PlayerCount = _db.Players.Count();

            var idList = Ranking.Select(r => r.PlayerId).ToArray();
            if (!idList.Any()) return;
            var values = new StringBuilder();
            values.Append(idList[0]);
            for (int i = 0; i < idList.Length; i++) {
                values.Append($", {idList[i]}");
            }
            var sql = $"SELECT * FROM Players p WHERE SteamId IN ({values})";
            var tmp = _db.Players.Include(p => p.Matches).FromSql(sql).AsNoTracking().ToList();
            for(int i = 0; i < Ranking.Count; i++) {
                Players.Add(tmp.First(p => p.SteamId == Ranking[i].PlayerId));
            }
            SteamPlayers = await _steamApi.GetPlayerInformation(Ranking.Select(p => p.PlayerId).ToArray());
        }
    }
}
