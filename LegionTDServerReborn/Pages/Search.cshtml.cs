using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.Models;
using LegionTDServerReborn.Extensions;
using LegionTDServerReborn.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace LegionTDServerReborn.Pages
{
    public class SearchModel : PageModel
    {
        public string SearchTerm { get; private set; }

        private LegionTdContext _db;
        
        private SteamApi _steamApi;

        public List<Player> Players {get; private set;} = new List<Player>();

        public Match Match {get; private set;}

        public List<Fraction> Builders {get; private set;} = new List<Fraction>();

        public List<Unit> Units {get; private set;} = new List<Unit>();

        public SearchModel(SteamApi steamApi, LegionTdContext db) {
            _db = db;
            _steamApi = steamApi;
        }

        public async Task OnGetAsync(string searchTerm)
        {
            SearchTerm = string.IsNullOrEmpty(searchTerm) ? "" : searchTerm;
            if (string.IsNullOrEmpty(SearchTerm)) {
                return;
            }
            if (int.TryParse(searchTerm, out var id)) {
                Match = await _db.Matches.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.MatchId == id);
            } else {
                id = -1;
            }
            Players = await _db.Players.Where(p => p.Avatar != null)
                .Where(p => p.SteamId == id || 
                    p.ProfileUrl == SearchTerm || 
                    (p.PersonaName.Contains(SearchTerm) && p.PersonaName.Length <= 4*SearchTerm.Length))
                .Take(50)
                .ToListAsync();
            Builders = await _db.Fractions.Where(f => EF.Functions.Like(f.ToBuilderName(), $"%{searchTerm}%")).ToListAsync();
            //Units = await _db.Units.Where(f => EF.Functions.Like(f.GetDisplayName(), $"%{searchTerm}%")).ToListAsync();
        }
    }
}
