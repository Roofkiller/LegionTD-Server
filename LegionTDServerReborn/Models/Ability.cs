using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using LegionTDServerReborn.Extensions;


namespace LegionTDServerReborn.Models {
    public class Ability {
        [Key]
        public string Name {get; set;}
        public string DisplayName {get; set;}
        public int GoldCost {get; set;}
        public int ManaCost {get; set;}
        public float Cooldown {get; set;}
        public float CastRange {get; set;}
        [InverseProperty("Ability")]
        public virtual List<UnitAbility> Casters {get; set;}

        public bool UpdateValues(JsonElement values) {
            int oldGoldCost = GoldCost;
            int oldManaCost = ManaCost;
            float oldCooldown = Cooldown;
            float oldCastRange = CastRange;
            GoldCost = values.GetIntOrDefault("AbilityGoldCost");
            ManaCost = values.GetIntOrDefault("AbilityManaCost");
            Cooldown = values.GetFloatOrDefault("AbilityCooldown");
            CastRange = values.GetFloatOrDefault("AbilityCastRange");
            DisplayName = values.GetValueOrDefault("DisplayName");
            if (string.IsNullOrEmpty(DisplayName)) {
                DisplayName = Name;
            }
            return oldGoldCost != GoldCost || oldManaCost != ManaCost || oldCooldown != Cooldown || oldCastRange == CastRange;
        }
    }
}
