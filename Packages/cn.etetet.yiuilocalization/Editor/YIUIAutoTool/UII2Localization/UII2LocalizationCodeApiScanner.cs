#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using I2.Loc;

namespace YIUIFramework.Editor
{
    internal static class UII2LocalizationCodeApiScanner
    {
        private static readonly Regex LiteralCallRegex = new Regex(
            @"LocalizationManager\.(?:GetTranslation|TryGetTranslation)\s*\(\s*(?<key>@?""(?:""""|\\""|[^""])*"")",
            RegexOptions.Compiled);

        private static readonly Regex AnyCallRegex = new Regex(
            @"LocalizationManager\.(?:GetTranslation|TryGetTranslation)\s*\(",
            RegexOptions.Compiled);

        public static LocalizationScanReport Scan(LanguageSourceData sourceData)
        {
            var report = new LocalizationScanReport { Title = "代码多语言 API 扫描完成" };
            if (sourceData == null)
            {
                report.AddMissing(string.Empty, string.Empty, string.Empty, "没有找到 I2 多语言数据源");
                return report;
            }

            foreach (var path in FindCodeFiles())
            {
                report.ScannedFiles++;
                ScanFile(path, sourceData, report);
            }

            return report;
        }

        private static void ScanFile(string path, LanguageSourceData sourceData, LocalizationScanReport report)
        {
            if (!TryReadText(path, report, out var rawContent))
            {
                return;
            }

            var content = StripComments(rawContent);
            var literalMatches = LiteralCallRegex.Matches(content);
            foreach (Match match in literalMatches)
            {
                var key = DecodeStringLiteral(match.Groups["key"].Value);
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                report.References++;
                if (sourceData.GetTermData(key) == null)
                {
                    report.AddMissing(ToProjectPath(path), GetLine(content, match.Index).ToString(), key, "代码中引用的 Key 未在 I2 中找到");
                }
            }

            var allCallCount = AnyCallRegex.Matches(content).Count;
            if (allCallCount > literalMatches.Count)
            {
                report.DynamicCalls += allCallCount - literalMatches.Count;
            }
        }

        private static bool TryReadText(string path, LocalizationScanReport report, out string content)
        {
            content = null;
            try
            {
                if (!File.Exists(path))
                {
                    report.Skipped++;
                    return false;
                }

                content = File.ReadAllText(path);
                return true;
            }
            catch (Exception)
            {
                report.Skipped++;
                return false;
            }
        }

        private static IEnumerable<string> FindCodeFiles()
        {
            foreach (var root in new[] { "Assets", "Packages" })
            {
                if (!Directory.Exists(root))
                {
                    continue;
                }

                foreach (var path in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
                {
                    var normalized = path.Replace("\\", "/");
                    if (normalized.Contains("/cn.etetet.yiuilocalization/", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    yield return normalized;
                }
            }
        }

        private static string StripComments(string content)
        {
            var result = new char[content.Length];
            var inString = false;
            var inVerbatim = false;
            var inLineComment = false;
            var inBlockComment = false;
            for (var i = 0; i < content.Length; i++)
            {
                var current = content[i];
                var next = i + 1 < content.Length ? content[i + 1] : '\0';
                if (inLineComment)
                {
                    result[i] = current == '\n' ? '\n' : ' ';
                    inLineComment = current != '\n';
                    continue;
                }

                if (inBlockComment)
                {
                    result[i] = current == '\n' ? '\n' : ' ';
                    if (current == '*' && next == '/')
                    {
                        result[++i] = ' ';
                        inBlockComment = false;
                    }
                    continue;
                }

                if (!inString && current == '/' && next == '/')
                {
                    result[i] = result[++i] = ' ';
                    inLineComment = true;
                    continue;
                }

                if (!inString && current == '/' && next == '*')
                {
                    result[i] = result[++i] = ' ';
                    inBlockComment = true;
                    continue;
                }

                if (current == '"' && (!inString || !inVerbatim || next != '"') && (inVerbatim || i == 0 || content[i - 1] != '\\'))
                {
                    inString = !inString;
                    inVerbatim = inString && i > 0 && content[i - 1] == '@';
                }

                result[i] = current;
            }

            return new string(result);
        }

        private static string DecodeStringLiteral(string literal)
        {
            if (literal.StartsWith("@\"", StringComparison.Ordinal))
            {
                return literal.Substring(2, literal.Length - 3).Replace("\"\"", "\"");
            }

            var inner = literal.Substring(1, literal.Length - 2);
            return Regex.Unescape(inner);
        }

        private static int GetLine(string content, int index)
        {
            var line = 1;
            for (var i = 0; i < index && i < content.Length; i++)
            {
                if (content[i] == '\n')
                {
                    line++;
                }
            }

            return line;
        }

        private static string ToProjectPath(string path)
        {
            return path.Replace("\\", "/");
        }
    }
}
#endif
