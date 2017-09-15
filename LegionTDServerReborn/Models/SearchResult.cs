using LegionTDServerReborn.Extensions;

namespace LegionTDServerReborn.Models {
    public class SearchResult {
        public string Title;
        public string Desc;
        public string Link;
        public string Image;
        public string AspPage;
        public string AspRouteName;
        public string AspRouteValue;

        public static SearchResult FromMatch(Match match) {
            return new SearchResult{
                Title = $"Match #{match.MatchId}",
                Desc = match.GetFormattedDate(),
                Link = $"/Matches/{match.MatchId}",
                AspRouteValue = match.MatchId.ToString(),
                Image = ""
            };
        }

        public static SearchResult FromUnit(Unit unit) {
            return new SearchResult{
                Title = unit.DisplayName,
                Desc = "",
                Image = ""
            };
        }

        public static SearchResult FromBuilder(Builder builder) {
            return new SearchResult{
                Title = builder.DisplayName,
                Desc = "",
                Link = $"/Builders/{builder.FractionName}",
                Image = $"/images/builder/{builder.FractionName}.png"
            };
        }

        public static SearchResult FromPlayer(Player player) {
            return new SearchResult{
                Title = player.PersonaName,
                Desc = "",
                Link = $"/Players/{player.SteamId}",
                Image = player.Avatar
            };
        }
    }
}
