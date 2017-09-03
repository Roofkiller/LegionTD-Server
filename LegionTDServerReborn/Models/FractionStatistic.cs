using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LegionTDServerReborn.Models{
    public class FractionStatistic {
        public DateTime TimeStamp {get; set;}
        public string FractionName {get; set;}
        [ForeignKey("FractionName")]
        public Fraction Fraction {get; set;}
        public int WonGames {get; set;}
        public int LostGames {get; set;}
        public float PickRate {get; set;}
        public float WinRate => (float)WonGames / (WonGames+LostGames);
    }
}
