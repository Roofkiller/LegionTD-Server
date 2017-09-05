using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using LegionTDServerReborn.Models;
using LegionTDServerReborn.Extensions;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LegionTDServerReborn.Helpers {

    public static class HtmlHelpers {

        public static IHtmlContent MatchesToTable(this IHtmlHelper html, IEnumerable<Match> matches) {
            string result = 
                "<div class='table-responsive'>" +
                    "<table class='table table-responsive table-hover table-striped'>" +
                        "<thead>" +
                            "<td>Match ID</td>" +
                            "<td>Date</td>" +
                            "<td>Duration</td>" +
                            "<td>Winner</td>" +
                            "<td>Last Wave</td>" +
                        "</thead>" +
                        "<tbody id='match_table_body'>" +
                            string.Join("", matches.Select(m => m.MatchToTableRow()).ToArray()) +
                        "</tbody>" +
                    "</table>" +
                "</div>";
            return new HtmlString(result);
        }

        public static IHtmlContent MatchToTableRow(this IHtmlHelper html, Match match) {
            return new HtmlString(match.MatchToTableRow());                
        }

        public static string MatchToTableRow(this Match match) {
            var winnerTeam = match.Winner.GetTeamName();
            string result = $"<tr class='clickable-row' data-href='/Match/{match.MatchId}'>"+
                $"<td>{match.MatchId}</td>" +
                $"<td>{match.GetFormattedDate()}</td>" +
                $"<td>{match.GetFormattedTime()}</td>" +
                $"<td><span class='{winnerTeam.ToLower()}'>{winnerTeam}</span>" +
                $"<td>{match.LastWave}</td>" +
                "</tr>";
            return result;                
        }

        public static IHtmlContent FormatRatingChange(this IHtmlHelper html, int ratingChange) {
            string result;
            if ( ratingChange < 0) {
                result = $"<span class='dire'>{ratingChange}</span>";
            } else {
                result = $"<span class='radiant'>+{ratingChange}</span>";
            }
            return new HtmlString(result);
        }

        public static IHtmlContent FormatColor<T>(this IHtmlHelper html, T value, Func<T, bool> condition) {
            string result;
            if (condition(value)) {
                result = $"<span class='dire'>{value}</span>";
            } else {
                result = $"<span class='radiant'>{value}</span>";
            }
            return new HtmlString(result);
        }

        public static HtmlString TeamToHtml(this IHtmlHelper html, int team) {
            return new HtmlString($"<span class='{team.GetTeamName().ToLower()}'>{team.GetTeamName()}</span>");
        }

        public static HtmlString ToBuilderIcon(this string builder) {
            return new HtmlString($"<a href='/Builder/{builder}'><img class='builder-icon' src='/images/builder/{builder}.png'></a>");
        }
    }

}
