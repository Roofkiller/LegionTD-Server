using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LegionTDServerReborn.Extensions;
using Newtonsoft.Json.Linq;

namespace LegionTDServerReborn.Models
{
    public enum UnitType
    {
        Tower,
        Wave,
        IncomeUnit,
        Boss,
        King,
        Other
    }

    public class Unit
    {
        [Key]
        public string Name { get; set; }
        public string DisplayName {get; set;}
        public int Experience { get; set; }
        public virtual UnitType Type { get; set; }
        public string FractionName {get; set;}
        [ForeignKey("FractionName")]
        public virtual Fraction Fraction { get; set; }

        public float AttackDamageMin {get; set;}
        public float AttackDamageMax {get; set;}
        public float AttackRate {get; set;}
        public float AttackRange {get; set;}

        public float ArmorPhysical {get; set;}
        public float MagicResistance {get; set;}
        public float StatusHealth {get; set;}
        public float StatusHealthRegen {get; set;}
        public float StatusMana {get; set;}
        public float StatusManaRegen {get; set;}

        public float BountyGoldMin {get; set;}
        public float BountyGoldMax {get; set;}

        public string LegionAttackType {get; set;}
        public string LegionDefendType {get; set;}
        
        [InverseProperty("Unit")]
        public virtual List<UnitAbility> Abilities {get; set;}

        [InverseProperty("Unit")]
        public virtual SpawnAbility SpawnAbility {get; set;}

        [InverseProperty("Unit")]
        public virtual List<UnitStatistic> Statistics {get; set;}
        public virtual UnitStatistic CurrentStatistic => Statistics.OrderByDescending(s => s.TimeStamp).FirstOrDefault();
    

        public void UpdateValues(JsonElement values) {
            Experience = values.GetIntOrDefault("LegionExperience");
            
            AttackDamageMin = values.GetFloatOrDefault("AttackDamageMin");
            AttackDamageMax = values.GetFloatOrDefault("AttackDamageMax");
            AttackRate = values.GetFloatOrDefault("AttackRate");
            AttackRange = values.GetFloatOrDefault("AttackRange");

            ArmorPhysical = values.GetFloatOrDefault("ArmorPhysical");
            MagicResistance = values.GetFloatOrDefault("MagicResistance");
            StatusHealth = values.GetFloatOrDefault("StatusHealth");
            StatusHealthRegen = values.GetFloatOrDefault("StatusHealthRegen");
            StatusMana = values.GetFloatOrDefault("StatusMana");
            StatusManaRegen = values.GetFloatOrDefault("StatusManaRegen");

            BountyGoldMin = values.GetFloatOrDefault("BountyGoldMin");
            BountyGoldMax = values.GetFloatOrDefault("BountyGoldMax");

            LegionAttackType = values.GetValueOrDefault("LegionAttackType");
            LegionDefendType = values.GetValueOrDefault("LegionDefendType");       

            FractionName = values.GetValueOrDefault("LegionFraction") ?? "other";

            DisplayName = values.GetValueOrDefault("DisplayName");
            if (string.IsNullOrWhiteSpace(DisplayName)) {
                DisplayName = Name;
            }
        }


        public void SetTypeByName()
        {
            if (Name.StartsWith("tower_"))
                Type = UnitType.Tower;
            else if (Name.StartsWith("unit_"))
                Type = UnitType.Wave;
            else if (Name.StartsWith("incomeunit_"))
                Type = UnitType.IncomeUnit;
            else if (Name.EndsWith("_boss"))
                Type = UnitType.Boss;
            else if (Name.EndsWith("_king"))
                Type = UnitType.King;
            else
                Type = UnitType.Other;
        }

        public string GetFractionByName()
        {
            if (Name.StartsWith("tower_"))
            {
                var tmp = Name.Replace("tower_", "");
                return Regex.Replace(tmp, "(?:builder_).*", "");
            }
            if (Name.StartsWith("unit_"))
                return "wave";
            if (Name.StartsWith("incomeunit_"))
                return "income";
            if (Name.EndsWith("_boss"))
                return "boss";
            if (Name.EndsWith("_king"))
                return "king";
            return "other";
        }
    }
}
