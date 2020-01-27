using LegionTDServerReborn.Models;
using System;
using System.Text;
using System.Linq;

namespace LegionTDServerReborn.Extensions
{
    public static class UnitExtension
    {
        public static Unit GetParent(this Unit unit) {
            if (unit.SpawnAbility == null) {
                return null;
            }
            if (unit.SpawnAbility.Casters == null || unit.SpawnAbility.Casters.Count == 0) {
                return null;
            }
            return unit.SpawnAbility.Casters[0].Unit;
        }

        public static Unit[] GetChildren(this Unit unit) {
            if (unit.Abilities == null || unit.Abilities.Count == 0) {
                return new Unit[0];
            }
            var abilities = unit.Abilities.Where(u => u.Ability != null & u.Ability is SpawnAbility)
                .OrderByDescending(a => a.Slot).ToArray();
            var result = abilities.Select(a => ((SpawnAbility)a.Ability).Unit).ToArray();
            return result;
        }

        public static int GetLevel(this Unit unit) {
            int result = 0;
            Unit current = unit;
            while ((current = current.GetParent()) != null) {
                result++;
            }
            return result;
        }

        public static int GetGoldCost(this Unit unit) {
            return unit.SpawnAbility != null ? unit.SpawnAbility.GoldCost : 0;
        }

        public static int GetTotalGoldCost(this Unit unit) {
            var totalCost = 0;
            var currentUnit = unit;
            while (currentUnit != null) {
                totalCost += currentUnit.GetGoldCost();
                currentUnit = currentUnit.GetParent();
            }
            return totalCost;
        }
    }
}
