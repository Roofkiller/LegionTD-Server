using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using LegionTDServerReborn.Models;
using LegionTDServerReborn.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LegionTDServerReborn.Pages
{
    public class BuildersModel : PageModel
    {
        public List<Builder> Builders {get; set;}

        private LegionTdContext _db;

        private SteamApi _steamApi;

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
