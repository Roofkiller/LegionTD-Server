using LegionTDServerReborn.Models;
using System;
using System.Text;
using System.Linq;

namespace LegionTDServerReborn.Extension
{
    public static class ModelExtension
    {
        public static string GetFormattedTime(this Match match) {
            var duration = match.Duration;
            return $"{(int)duration / 60:00}:{(int)duration % 60:00}";
        }
        
        public static string GetFormattedDate(this Match match) {
            return match.Date.DateToText();
        }

        public static string GetTeamName(this int team)  {
            return team == 2 ? "Radiant" : "Dire";
        }

        public static string DateToText(this DateTime date) {
            var now = DateTime.Now;
            var span = now - date;
            if (span.TotalMinutes < 0) {
                return $"less than a minute ago";
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

        public static string ToBuilderName(this Fraction fraction) {
            return fraction.Name.FirstCharToUpper() + "builder";
        }

        public static string FirstCharToUpper(this string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException();
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        public static string ToUnitName(this string input) {
            if (string.IsNullOrEmpty(input)) {
                return input;
            }
            var parts = input.Split('_');
            int startIndex = input.StartsWith("tower_") ? 2 : 1;
            var result = new StringBuilder();
            for (int i = startIndex; i < parts.Length; i++) {
                result.Append(parts[i].FirstCharToUpper() + " ");
            }
            return result.ToString();
        }
    }
}