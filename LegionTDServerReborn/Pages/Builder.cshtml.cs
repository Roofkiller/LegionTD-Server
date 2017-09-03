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
    public class BuilderModel : SteamApiModel
    {
        public string BuilderName {get; set;}

        public Fraction Builder {get; set;}

        public FractionStatistic Statistic {get; set;}

        private LegionTdContext _db;

        public BuilderModel(IConfiguration configuration, LegionTdContext db)
            :base(configuration) {
                _db = db;
            }

        public void OnGet(string builder)
        {
            BuilderName = builder ?? "";
            Builder = _db.Fractions.Include(b => b.Units).Include(b => b.Statistics).Single(f => f.Name == BuilderName);
            Statistic = Builder.CurrentStatistic;
        }
    }
}
