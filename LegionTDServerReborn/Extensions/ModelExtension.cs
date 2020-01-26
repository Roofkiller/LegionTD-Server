using LegionTDServerReborn.Models;
using System;
using System.Text;
using System.Linq;

namespace LegionTDServerReborn.Extensions
{
    public static class ModelExtension
    {

        public static string GetTeamName(this int team)  {
            return team == 2 ? "Radiant" : "Dire";
        }

        public static string ToRelativeText(this DateTimeOffset date) {
            return date.LocalDateTime.ToRelativeText();
        }

        public static string ToRelativeText(this DateTime date) {
            var now = DateTimeOffset.UtcNow.UtcDateTime;
            var span = now - date;
            if (span.TotalMinutes < 0) {
                return "less than a minute ago";
            }
            if (span.TotalMinutes < 2) {
                return "1 minute ago";
            }
            if (span.TotalHours < 1) {
                return $"{Math.Floor(span.TotalMinutes)} minutes ago";
            }
            if (span.TotalHours < 2) {
                return "1 hour ago";
            }
            if (span.TotalHours < 24) {
                return $"{Math.Floor(span.TotalHours)} hours ago";
            }
            if (span.TotalDays < 2) {
                return "1 day ago";
            }
            if (span.TotalDays < 31) {
                return $"{Math.Floor(span.TotalDays)} days ago";
            }
            if (span.TotalDays < 62) {
                return "1 month ago";
            }
            if (span.TotalDays < 365) {
                return $"{Math.Floor(span.TotalDays / 31)} months ago";
            }
            if (span.TotalDays < 730) {
                return "1 year ago";
            }
            return $"{Math.Floor(span.TotalDays / 365)} years ago";
        }
    }
}
