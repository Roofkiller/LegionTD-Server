using LegionTDServerReborn.Models;
using System;
using System.Text;
using System.Linq;

namespace LegionTDServerReborn.Extensions
{
    public static class MatchExtension
    {
        public static string GetFormattedTime(this Match match) {
            var duration = match.Duration;
            return $"{(int)duration / 60:00}:{(int)duration % 60:00}";
        }
        
        public static string GetFormattedDate(this Match match) {
            return match.Date.ToRelativeText();
        }
    }
}
