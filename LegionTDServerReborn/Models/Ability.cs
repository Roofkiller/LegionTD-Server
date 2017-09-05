using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.Extensions;
using Newtonsoft.Json.Linq;

namespace LegionTDServerReborn.Models {
    public class Ability {
        [Key]
        public string Name {get; set;}
        public int GoldCost {get; set;}
        public int ManaCost {get; set;}
        public float Cooldown {get; set;}
        public float CastRange {get; set;}
        [InverseProperty("Ability")]
        public List<UnitAbility> Casters {get; set;}

        public bool UpdateValues(JToken values) {
            int oldGoldCost = GoldCost;
            int oldManaCost = ManaCost;
            float oldCooldown = Cooldown;
            float oldCastRange = CastRange;
            GoldCost = values.GetValueOrDefaultInt("AbilityGoldCost");
            ManaCost = values.GetValueOrDefaultInt("AbilityManaCost");
            Cooldown = values.GetValueOrDefaultFloat("AbilityCooldown");
            CastRange = values.GetValueOrDefaultFloat("AbilityCastRange");
            return oldGoldCost != GoldCost || oldManaCost != ManaCost || oldCooldown != Cooldown || oldCastRange == CastRange;
        }
    }
}
