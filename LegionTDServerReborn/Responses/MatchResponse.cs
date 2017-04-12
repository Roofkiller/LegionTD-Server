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
        public bool IsTraining { get; set; }
        public DateTime Date { get; set; }
        public bool Won { get; set; }
        public int RatingChange { get; set; }
        public float Duration { get; set; }

        public MatchResponse(PlayerMatchData playerData)
        {
            Match match = playerData.Match;
            Id = match.MatchId;
            IsTraining = match.IsTraining;
            Date = match.Date;
            Won = match.Winner == playerData.Team;
            RatingChange = playerData.RatingChange;
            Duration = match.Duration;
        }
    }
}
