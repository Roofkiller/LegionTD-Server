using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegionTDServerReborn.Extensions
{
    public static class DbExtension
    {
        public static float NaNToZero(this float value)
        {
            return float.IsNaN(value) ? 0 : value;
        }
    }
}
