using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public Fraction Fraction { get; set; }

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
            return "other";
        }
    }
}
