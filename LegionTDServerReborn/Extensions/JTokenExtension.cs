using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegionTDServerReborn.Extensions {
    public static class JTokenExtension {
        public static int  GetValueOrDefaultInt(this JToken source, string name) {
            try {
                return source[name].Value<int>();
            } catch(Exception) {
                return 0;
            }
        }

        public static float GetValueOrDefaultFloat(this JToken source, string name) {
            try {
                return source[name].Value<float>();
            } catch(Exception) {
                return 0;
            }
        }

        public static string  GetValueOrDefault(this JToken source, string name) {
            try {
                return source[name].Value<string>();
            } catch(Exception) {
                return "";
            }
        }
    }
}