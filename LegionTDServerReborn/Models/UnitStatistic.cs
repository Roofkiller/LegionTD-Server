using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LegionTDServerReborn.Models{
    public class UnitStatistic {
        public DateTimeOffset TimeStamp {get; set;}
        public string UnitName {get; set;}
        [ForeignKey("UnitName")]
        public virtual Unit Unit {get; set;}
        public int Killed {get; set;}
        public int Leaked {get; set;}
        public int Send {get; set;}
        public int Build {get; set;}
        public int GamesBuild {get; set;}
        public int GamesEvaluated {get; set;}
        public int GamesWon {get; set;}

        public float BuildRate => GamesBuild / (float)GamesEvaluated;
        public float BuildsPerGame => Build / (float)GamesEvaluated;
        public float WinRate => GamesBuild == 0 ? 0 : GamesWon / (float)GamesBuild;
    }
}
