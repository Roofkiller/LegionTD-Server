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
    public class RankingModel : SteamApiModel
    {
        public Dictionary<long, SteamPlayer> SteamPlayers {get; set;}
        public List<Ranking> Ranking {get; set;}
        public List<Player> Players {get; set;}

        private LegionTdContext _db;

        public RankingModel(IConfiguration configuration, LegionTdContext db)
            :base(configuration) {
            _db = db;
        }

        public void OnGet()
        {
            Ranking = _db.Rankings
                .Where(r => r.Position >= 0 
                    && r.Position <= 50)
                .ToList()
                .OrderBy(r => r.Position)
                .ToList();

            var idList = Ranking.Select(r => r.PlayerId).ToArray();
            var values = new StringBuilder();
            values.Append(idList[0]);
            for (int i = 0; i < idList.Length; i++) {
                values.Append($", {idList[i]}");
            }
            var sql = $"SELECT * FROM Players p WHERE SteamId IN ({values})";
            var tmp = _db.Players.Include(p => p.MatchDatas).FromSql(sql).AsNoTracking().ToList();
            Players = new List<Player>();
            for(int i = 0; i < Ranking.Count; i++) {
                Players.Add(tmp.First(p => p.SteamId == Ranking[i].PlayerId));
            }
            SteamPlayers = RequestPlayers(Ranking.Select(p => p.PlayerId).ToArray());
        }
    }
}
