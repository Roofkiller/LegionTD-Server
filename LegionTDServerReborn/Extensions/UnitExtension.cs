using LegionTDServerReborn.Models;
using System;
using System.Text;
using System.Linq;

namespace LegionTDServerReborn.Extensions
{
    public static class UnitExtension
    {
        public static string GetDisplayName(this Unit unit) {
            if (unit == null) {
                throw new ArgumentNullException();
            }
            string input = unit.Name;
            if (string.IsNullOrEmpty(input)) {
                return input;
            }
            var parts = input.Split('_');
            int startIndex = input.StartsWith("tower_") ? 2 : 1;
            var result = new StringBuilder();
            for (int i = startIndex; i < parts.Length; i++) {
                result.Append(parts[i].FirstCharToUpper() + " ");
            }
            return result.ToString();
        }

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
            var result = new Unit[abilities.Length];
            for (int i = 0; i < abilities.Length; i++) {
                result[i] = ((SpawnAbility)abilities[i].Ability).Unit;
            }
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