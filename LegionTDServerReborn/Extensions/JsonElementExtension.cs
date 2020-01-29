using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LegionTDServerReborn.Extensions
{
    public static class JsonElementExtension
    {
        public static int GetIntOrDefault(this JsonElement json, string key)
        {
            int result = 0;
            if (json.TryGetProperty(key, out JsonElement property))
            {
                try {
                    property.TryGetInt32(out result);
                } catch (Exception) {
                    int.TryParse(property.ToString(), out result);
                }
            }
            return result;
        }

        public static bool GetBoolOrDefault(this JsonElement json, string key)
        {
            bool result = false;
            if (json.TryGetProperty(key, out JsonElement property))
            {
                try
                {
                    result = property.GetBoolean();
                } catch (Exception) {
                    bool.TryParse(property.ToString(), out result);
                }
            }
            return result;
        }

        public static float GetFloatOrDefault(this JsonElement json, string key)
        {
            float result = 0;
            if (json.TryGetProperty(key, out JsonElement property))
            {
                try {
                    property.TryGetSingle(out result);
                } catch (Exception) {
                    float.TryParse(property.ToString(), out result);
                }
            }
            return result;
        }

        public static string GetValueOrDefault(this JsonElement json, string key)
        {
            if (json.TryGetProperty(key, out JsonElement property))
            {
                return property.ToString();
            }
            return null;
        }

        public static bool TryToJson(this string rawJson, out JsonDocument result)
        {
            try
            {
                result = JsonDocument.Parse(rawJson);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        public static JsonElement ToJsonElement(this string rawJson)
        {
            return JsonDocument.Parse(rawJson).RootElement;
        }
    }
}
