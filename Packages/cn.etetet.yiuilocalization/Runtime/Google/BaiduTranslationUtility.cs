using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using I2.Loc.SimpleJSON;
using UnityEngine.Networking;

namespace I2.Loc
{
    public static class BaiduTranslationUtility
    {
        public const string Endpoint = "https://fanyi-api.baidu.com/api/trans/vip/translate";

        private static readonly Dictionary<string, string> LanguageCodes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "auto", "auto" },
                { "zh", "zh" }, { "zh-CN", "zh" }, { "zh-Hans", "zh" },
                { "zh-TW", "cht" }, { "zh-HK", "cht" }, { "zh-Hant", "cht" },
                { "en", "en" }, { "ja", "jp" }, { "ko", "kor" },
                { "fr", "fra" }, { "de", "de" }, { "es", "spa" },
                { "pt", "pt" }, { "it", "it" }, { "ru", "ru" },
                { "ar", "ara" }, { "nl", "nl" }, { "pl", "pl" },
                { "th", "th" }, { "tr", "tr" }, { "vi", "vie" },
                { "id", "id" }, { "hi", "hi" }, { "el", "el" },
                { "sv", "swe" }, { "da", "dan" }, { "fi", "fin" },
                { "cs", "cs" }, { "hu", "hu" }, { "ro", "rom" },
                { "bg", "bul" }, { "et", "est" }, { "lv", "lav" },
                { "lt", "lit" }, { "sk", "sk" }, { "sl", "slo" },
                { "uk", "ukr" }, { "he", "heb" }, { "fa", "per" },
                { "ms", "may" }, { "ca", "cat" }, { "hr", "hrv" },
                { "sr", "srp" }
            };

        public static string GenerateSign(string appId, string query, string salt, string secretKey)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(string.Format("{0}{1}{2}{3}", appId, query, salt, secretKey));
                var hash = md5.ComputeHash(bytes);
                var builder = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public static string GetBaiduLanguageCode(string internationalCode)
        {
            if (string.IsNullOrEmpty(internationalCode))
            {
                return string.Empty;
            }

            if (LanguageCodes.TryGetValue(internationalCode, out var baiduCode))
            {
                return baiduCode;
            }

            var dashIndex = internationalCode.IndexOf("-", StringComparison.Ordinal);
            if (dashIndex <= 0)
            {
                return string.Empty;
            }

            var parentCode = internationalCode.Substring(0, dashIndex);
            return LanguageCodes.TryGetValue(parentCode, out baiduCode) ? baiduCode : string.Empty;
        }

        public static string BuildRequestUrl(string text, string from, string to, string salt, string sign, string appId)
        {
            return string.Format(
                "{0}?q={1}&from={2}&to={3}&appid={4}&salt={5}&sign={6}",
                Endpoint,
                UnityWebRequest.EscapeURL(text),
                from,
                to,
                UnityWebRequest.EscapeURL(appId),
                salt,
                sign);
        }

        public static bool TryParseResponse(string json, out string translation, out string error)
        {
            translation = null;
            error = null;

            if (string.IsNullOrEmpty(json))
            {
                error = "Baidu translate returned empty response";
                return false;
            }

            var root = JSONNode.Parse(json);
            if (root == null)
            {
                error = string.Format("Baidu translate returned invalid json: {0}", json);
                return false;
            }

            var errorNode = root["error_code"];
            var errorCode = errorNode == null ? null : errorNode.Value;
            if (!string.IsNullOrEmpty(errorCode))
            {
                var messageNode = root["error_msg"];
                var message = messageNode == null ? string.Empty : messageNode.Value;
                error = string.Format("Baidu translate error {0}: {1}", errorCode, message);
                return false;
            }

            var resultNode = root["trans_result"];
            var results = resultNode == null ? null : resultNode.AsArray;
            if (results == null || results.Count <= 0)
            {
                error = string.Format("Baidu translate returned no result: {0}", json);
                return false;
            }

            var translationNode = results[0]["dst"];
            translation = translationNode == null ? null : translationNode.Value;
            if (!string.IsNullOrEmpty(translation))
            {
                return true;
            }

            error = string.Format("Baidu translate returned empty translation: {0}", json);
            return false;
        }

        public static string RestoreTags(string translatedText, string[] tags)
        {
            if (tags == null || tags.Length == 0 || string.IsNullOrEmpty(translatedText))
            {
                return translatedText;
            }

            var result = translatedText;
            for (var i = tags.Length - 1; i >= 0; --i)
            {
                result = result.Replace(GetNoTranslateTag(i), tags[i]);
            }
            return result;
        }

        private static string GetNoTranslateTag(int tagNumber)
        {
            if (tagNumber < 70)
            {
                return "++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++".Substring(0, tagNumber + 1);
            }

            return new string('+', tagNumber + 1);
        }
    }
}
