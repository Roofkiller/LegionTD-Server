using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.Models;

namespace LegionTDServerReborn.Responses
{

    public class PlayerResponse
    {
        public long SteamId { get; set; }
        public Dictionary<string, string> Data { get; set; }

        public PlayerResponse(Player player)
        {
            SteamId = player.SteamId;
            Data = player.DataToDict();
        }
    }

    public class PlayerRankingResponse : PlayerResponse
    {
        public int Rank { get; set; }

        public PlayerRankingResponse(Player player, int rank) : base(player)
        {
            Rank = rank;
        }
    }
}
