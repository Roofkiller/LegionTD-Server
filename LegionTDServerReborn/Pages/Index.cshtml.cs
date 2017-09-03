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

        private LegionTdContext _db;

        public IndexModel(LegionTdContext db) {
            _db = db;
        }

        public void OnGet()
        {
            var timeStamp = DateTime.Now.AddDays(-1);
            DailyMatches = _db.Matches.Count(m => m.Date > timeStamp);
            timeStamp = DateTime.Now.AddMonths(-1);
            MonthlyPlayers = _db.Matches.Include(m => m.PlayerDatas)
                .Where(m => m.Date > timeStamp)
                .SelectMany(m => m.PlayerDatas)
                .Select(p => p.PlayerId)
                .Distinct()
                .Count();
        }
    }
}
