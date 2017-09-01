using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace LegionTDServerReborn.Pages
{
    public class MatchesModel : PageModel
    {
        private string _steamApiKey => _configuration["steamApiKey"];

        private IConfiguration _configuration;

        public MatchesModel(IConfiguration configuration) {
            this._configuration = configuration;
        }
        
        public List<Match> Matches {get; set;}

        public int MatchCount {get; set;}

        public const int MatchesPerSite = 30;

        public int Site {get; set;}

        public int PageCount => (int)Math.Ceiling(MatchCount / (float)MatchesPerSite);

        public void OnGet(int? site)
        {
            Site = site ?? 1;
            using(var db = new LegionTdContext()) {
                Matches = db.Matches.Where(m => !m.IsTraining)
                                    .OrderByDescending(m => m.MatchId)
                                    .Skip((Site - 1) * MatchesPerSite)
                                    .Take(MatchesPerSite).ToList();
                MatchCount = db.Matches.Where(m => !m.IsTraining).Count();
            }
        }
    }
}
