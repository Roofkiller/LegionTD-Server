using LegionTDServerReborn.Models;
using System;
using System.Text;
using System.Linq;

namespace LegionTDServerReborn.Extensions
{
    public static class FractionExtension
    {
        public static string ToBuilderName(this Fraction fraction) {
            return fraction.Name.FirstCharToUpper() + "builder";
        }

        public static string FirstCharToUpper(this string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException();
            return input[0].ToString().ToUpper() + input.Substring(1);
        }
    }
}
