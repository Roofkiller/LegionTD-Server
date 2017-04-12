using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegionTDServerReborn.Responses
{
    public class RankingResponse
    {
        public int PlayerCount { get; set; }
        public int From => Ranking.Count > 0 ? Ranking[0].Rank : 0;
        public int To => Ranking.Count > 0 ? Ranking.Last().Rank : -1;
        public List<PlayerRankingResponse> Ranking { get; set; }
    }
}
