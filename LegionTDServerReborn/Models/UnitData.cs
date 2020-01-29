using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LegionTDServerReborn.Models
{
    public struct UnitData {
        public int Killed { get; set; }
        public int Leaked { get; set; }
        public int Sent { get; set; }
        public int Built { get; set; } 

        public static UnitData operator +(UnitData u1, UnitData u2)
        {
            return new UnitData
            {
                Killed = u1.Killed + u2.Killed,
                Leaked = u1.Leaked + u2.Leaked,
                Built = u1.Built + u2.Built,
                Sent = u1.Sent + u2.Sent
            };
        }
    }
}
