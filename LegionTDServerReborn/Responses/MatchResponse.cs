using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.Models;

namespace LegionTDServerReborn.Responses
{
    public class MatchResponse
    {
        public int Id { get; set; }
//        public bool IsTraining { get; set; }
//        public DateTime Date { get; set; }
//        public bool Won { get; set; }
//        public int RatingChange { get; set; }
//        public float Duration { get; set; }
        public int Order { get; set; }
        public object Data { get; set; }

        public MatchResponse(PlayerMatchData playerData, int order)
        {
            Match match = playerData.Match;
            Id = match.MatchId;
            Data = new
            {
                Id = match.MatchId,
                IsTraining = match.IsTraining,
                Date = match.Date,
                Won = playerData.Won,
                RatingChange = playerData.RatingChange,
                Duration = match.Duration
            };
//            IsTraining = match.IsTraining;
//            Date = match.Date;
//            Won = match.Winner == playerData.Team;
//            RatingChange = playerData.RatingChange;
//            Duration = match.Duration;
            Order = order;
        }
    }
}
