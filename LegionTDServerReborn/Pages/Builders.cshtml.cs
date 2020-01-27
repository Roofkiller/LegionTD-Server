using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.Models;
using LegionTDServerReborn.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LegionTDServerReborn.Pages
{
    public class BuildersModel : PageModel
    {
        public List<Builder> Builders {get; set;}

        private LegionTdContext _db;

        public BuildersModel(LegionTdContext db) {
            _db = db;
        }

        public async Task OnGetAsync()
        {
            Builders = await _db.Builders
                .Include(b => b.Fraction)
                .ThenInclude(f => f.Statistics)
                .Where(b => b.Public)
                .ToListAsync();
        }
    }
}
