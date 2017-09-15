using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LegionTDServerReborn.Models;
using Microsoft.EntityFrameworkCore;

namespace LegionTDServerReborn.Pages
{
    public class IndexModel : PageModel
    {
        public int DailyMatches {get; set;}
        public int MonthlyPlayers {get; set;}

        public string message;

        private LegionTdContext _db;

        public IndexModel(LegionTdContext db) {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            var timeStamp = DateTime.Now.AddDays(-1);
            DailyMatches = await _db.Matches.CountAsync(m => m.Date > timeStamp);
            timeStamp = DateTime.Now.AddMonths(-1);
            MonthlyPlayers = await _db.Matches.Include(m => m.PlayerData)
                .Where(m => m.Date > timeStamp)
                .SelectMany(m => m.PlayerData)
                .Select(p => p.PlayerId)
                .Distinct()
                .CountAsync();
        }
    }
}
