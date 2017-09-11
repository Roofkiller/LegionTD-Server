using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.Extensions;

namespace LegionTDServerReborn.Models
{
    public class SteamInformation
    {
        public long SteamId {get; set;}
        public DateTimeOffset Time {get; set;}
        [ForeignKey("SteamId")]
        public Player Player {get; set;}
        public string PersonaName {get; set;}
        public string Avatar {get; set;}
        public string RealName {get; set;}
        public string ProfileUrl {get; set;}
    }
}
