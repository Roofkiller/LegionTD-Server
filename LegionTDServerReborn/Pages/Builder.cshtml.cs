using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using LegionTDServerReborn.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LegionTDServerReborn.Pages
{
    public class BuilderModel : PageModel
    {
        public string BuilderName {get; private set;}

        public bool Valid => Builder != null;

        public Fraction Builder {get; private set;}

        public FractionStatistic Statistic {get; private set;}

        private LegionTdContext _db;

        public BuilderModel(LegionTdContext db) {
                _db = db;
        }

        public async Task OnGetAsync(string builder)
        {
            BuilderName = builder ?? "";
            Builder = await _db.Fractions.Include(b => b.Units)
                .ThenInclude(u => u.Abilities)
                .ThenInclude(a => a.Ability)
                .Include(b => b.Units)
                .ThenInclude(u => u.SpawnAbility)
                .Include(b => b.Units)
                .ThenInclude(u => u.Statistics)
                .Include(b => b.Statistics)
                .SingleOrDefaultAsync(f => f.Name == BuilderName);
            if (Builder != null) {
                Statistic = Builder.CurrentStatistic;
            }
        }
    }
}
