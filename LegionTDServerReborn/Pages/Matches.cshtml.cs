using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace LegionTDServerReborn.Pages
{
    public class MatchesModel : PageModel
    {
        private LegionTdContext _db;

        public MatchesModel(LegionTdContext db) {
            _db = db;
        }
        
        public List<Match> Matches {get; set;}

        public int MatchCount {get; set;}

        public const int MatchesPerSite = 30;

        public int Site {get; set;}

        public int PageCount => (int)Math.Ceiling(MatchCount / (float)MatchesPerSite);

        public async Task OnGetAsync(int? site)
        {
            Site = site ?? 1;
            Matches = await _db.Matches.Include(m => m.PlayerData)
                                .OrderByDescending(m => m.MatchId)
                                .Skip((Site - 1) * MatchesPerSite)
                                .Take(MatchesPerSite).ToListAsync();
            MatchCount = await _db.Matches.Where(m => !m.IsTraining).CountAsync();
        }
    }
}
