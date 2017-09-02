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
        private string _steamApiKey => _configuration["steamApiKey"];

        private IConfiguration _configuration;

        private LegionTdContext _db;

        public MatchesModel(IConfiguration configuration, LegionTdContext db) {
            this._configuration = configuration;
            _db = db;
        }
        
        public List<Match> Matches {get; set;}

        public int MatchCount {get; set;}

        public const int MatchesPerSite = 30;

        public int Site {get; set;}

        public int PageCount => (int)Math.Ceiling(MatchCount / (float)MatchesPerSite);

        public void OnGet(int? site)
        {
            Site = site ?? 1;
            Matches = _db.Matches.Include(m => m.PlayerDatas)
                                .Where(m => !m.IsTraining)
                                .OrderByDescending(m => m.MatchId)
                                .Skip((Site - 1) * MatchesPerSite)
                                .Take(MatchesPerSite).ToList();
            MatchCount = _db.Matches.Where(m => !m.IsTraining).Count();
        }
    }
}
