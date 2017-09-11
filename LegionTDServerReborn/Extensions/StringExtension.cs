using System.Text;

namespace LegionTDServerReborn.Extensions {
    public static class StringExtension {
        public static string Win1252ToUtf8(this string src) {
            if (src == null) {
                return null;
            }
            var wind1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
            var utf8 = Encoding.UTF8;
            return utf8.GetString(Encoding.Convert(wind1252, utf8, wind1252.GetBytes(src)));
        }
    }
}
