using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        public int Experience { get; set; }
        public UnitType Type { get; set; }
        [ForeignKey("FractionName")]
        public Fraction Fraction { get; set; }
        public string FractionName {get; set;}

        public string ParentName {get; set;}
        [ForeignKey("ParentName")]
        public Unit Parent {get; set;}
        [InverseProperty("Parent")]
        public List<Unit> Children {get; set;}

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

        public string Ability1 {get; set;}
        public string Ability2 {get; set;}
        public string Ability3 {get; set;}
        public string Ability4 {get; set;}

        public void UpdateValues(JToken values) {
            Experience = TryGetValueInt(values, "LegionExperience");
            
            AttackDamageMin = TryGetValueFloat(values, "AttackDamageMin");
            AttackDamageMax = TryGetValueFloat(values, "AttackDamageMax");
            AttackRate = TryGetValueFloat(values, "AttackRate");
            AttackRange = TryGetValueFloat(values, "AttackRange");

            ArmorPhysical = TryGetValueFloat(values, "ArmorPhysical");
            MagicResistance = TryGetValueFloat(values, "MagicResistance");
            StatusHealth = TryGetValueFloat(values, "StatusHealth");
            StatusHealthRegen = TryGetValueFloat(values, "StatusHealthRegen");
            StatusMana = TryGetValueFloat(values, "StatusMana");
            StatusManaRegen = TryGetValueFloat(values, "StatusManaRegen");

            BountyGoldMin = TryGetValueFloat(values, "BountyGoldMin");
            BountyGoldMax = TryGetValueFloat(values, "BountyGoldMax");

            LegionAttackType = TryGetValue(values, "LegionAttackType");
            LegionDefendType = TryGetValue(values, "LegionDefendType");   

            Ability1 = TryGetValue(values, "Ability1");
            Ability2 = TryGetValue(values, "Ability2");    
            Ability3 = TryGetValue(values, "Ability3");
            Ability4 = TryGetValue(values, "Ability4");            
        }

        private int TryGetValueInt(JToken source, string name) {
            try {
                return source[name].Value<int>();
            } catch(Exception) {
                return 0;
            }
        }

        private float TryGetValueFloat(JToken source, string name) {
            try {
                return source[name].Value<float>();
            } catch(Exception) {
                return 0;
            }
        }

        private string TryGetValue(JToken source, string name) {
            try {
                return source[name].Value<string>();
            } catch(Exception) {
                return "";
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
