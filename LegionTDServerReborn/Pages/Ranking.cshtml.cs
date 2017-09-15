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
    public class RankingModel : PageModel, SiteView
    {
        public const int PlayersPerSite = 50;
        public List<Ranking> Ranking {get; set;}
        public List<Player> Players {get; set;} = new List<Player>();

        private LegionTdContext _db;

        public int Site {get; set;}
        public int PageCount => (int)Math.Ceiling(PlayerCount / (float)PlayersPerSite);
        public int PlayerCount {get; set;}

        public string PageName => "./Ranking";

        public int EntryCount => PlayerCount;

        public int EntriesPerSite => PlayersPerSite;

        public RankingModel(LegionTdContext db) {
            _db = db;
        }

        public async Task OnGetAsync(int? site)
        {
            Site = site ?? 1;
            Ranking = (await _db.Rankings
                .Where(r => r.Position > (Site - 1) * PlayersPerSite 
                         && r.Position <= Site * PlayersPerSite)
                .Include(r => r.Player)
                .ThenInclude(p => p.Matches)
                .ToListAsync())
                .OrderBy(r => r.Position)
                .ToList();

            PlayerCount = await _db.Rankings.MaxAsync(r => r.Position);

            Players = Ranking.Select(r => r.Player).ToList();
        }
    }
}
