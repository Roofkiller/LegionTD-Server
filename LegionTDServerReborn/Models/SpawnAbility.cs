using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LegionTDServerReborn.Models {
    public class SpawnAbility : Ability {
        public string UnitName {get; set;}
        [ForeignKey("UnitName")]
        public Unit Unit {get; set;}
    }
}
