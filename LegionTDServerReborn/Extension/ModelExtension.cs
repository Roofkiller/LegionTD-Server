using LegionTDServerReborn.Models;

namespace LegionTDServerReborn.Extension
{
    public static class ModelExtension
    {
        public static string GetFormattedTime(this Match match) {
            var duration = match.Duration;
            return $"{(int)duration / 60:00}:{(int)duration % 60:00}";
        }
        
        public static string GetFormattedDate(this Match match) {
            return match.Date.ToString("dd.MM.yy HH:mm");
        }
    }
}