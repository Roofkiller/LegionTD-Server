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
        public List<Unit> Units {get; private set;}
        public Dictionary<string, UnitStatistic> Statistics {get; private set;}

        private LegionTdContext _db;

        public BuilderModel(LegionTdContext db) {
                _db = db;
        }

        public async Task OnGetAsync(string builder)
        {
            BuilderName = builder ?? "";
            Builder = await _db.Fractions
                .Include(b => b.Statistics)
                .AsNoTracking()
                .SingleOrDefaultAsync(f => f.Name == BuilderName);
            Units = await _db.Units
                .Where(u => u.Fraction == Builder)
                .Include(u => u.SpawnAbility)
                .Include(u => u.Abilities)
                    .ThenInclude(a => a.Ability)
                .ToListAsync();
            // Units = tmp.Select(i => i.Item1).ToList();
            // Statistics = tmp.ToDictionary(i => i.Item1.Name, i => i.Item2);
            var unitString = string.Join(", ", Units.Select(u => $"'{u.Name}'"));
            var sql = $"SELECT * FROM UnitStatistics WHERE UnitName IN ({unitString}) ORDER BY TimeStamp DESC LIMIT {Units.Count}";
            var statistics = await _db.UnitStatistics.FromSqlRaw(sql).ToListAsync();
            Statistics = Units.Zip(statistics).ToDictionary(i => i.First.Name, i => i.Second);
        }
    }
}
