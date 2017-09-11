using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.Models;
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

        public List<Player> Players {get; private set;}

        public Match Match {get; private set;}

        public List<Fraction> Builders {get; private set;}

        public List<Unit> Units {get; private set;}

        public SearchModel(SteamApi steamApi, LegionTdContext db) {
            _db = db;
            _steamApi = steamApi;
        }

        public async Task OnGetAsync(string searchTerm)
        {
            SearchTerm = string.IsNullOrEmpty(searchTerm) ? "" : searchTerm;
            if (string.IsNullOrEmpty(SearchTerm)) {
                Players = new List<Player>();
                return;
            }
            if (int.TryParse(searchTerm, out var id)) {
                Match = await _db.Matches.FindAsync(id);
            } else {
                id = -1;
            }
            Players = await _db.Players.FromSql($"SELECT * FROM Players WHERE MATCH(PersonaName, ProfileUrl) AGAINST({SearchTerm} IN NATURAL LANGUAGE MODE) OR SteamId = {id}").ToListAsync();
            Players = (await _steamApi.UpdatePlayerInformation(Players.Select(p => p.SteamId))).Values.ToList();
            Builders = await _db.Fractions.Where(f => EF.Functions.Like(f.Name, $"%{searchTerm}%")).ToListAsync();
            Units = await _db.Units.Where(f => EF.Functions.Like(f.Name, $"%{searchTerm}%")).ToListAsync();
        }
    }
}
