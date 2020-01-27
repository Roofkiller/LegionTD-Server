using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LegionTDServerReborn.Models {
    public class UnitAbility {
        public string UnitName {get; set;}
        [ForeignKey("UnitName")]
        public virtual Unit Unit {get; set;}
        public string AbilityName {get; set;}
        [ForeignKey("AbilityName")]
        public virtual Ability Ability {get; set;}
        public int Slot {get; set;}
    }
}
