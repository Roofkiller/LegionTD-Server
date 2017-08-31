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

        public void OnGet(int? from, int? to)
        {
            using(var db = new LegionTdContext()) {
                Matches = db.Matches.Where(m => !m.IsTraining)
                                    .OrderByDescending(m => m.MatchId)
                                    .Skip(from ?? 0)
                                    .Take(to ?? 30).ToList();
            }
        }
    }
}
