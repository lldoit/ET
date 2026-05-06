#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using I2.Loc;

namespace YIUIFramework.Editor
{
    internal static class UII2LocalizationExcelI18nScanner
    {
        public static LocalizationScanReport Scan(LanguageSourceData sourceData)
        {
            var report = new LocalizationScanReport { Title = "配置表 i18n 列扫描完成" };
            if (sourceData == null)
            {
                report.AddMissing(string.Empty, string.Empty, string.Empty, "没有找到 I2 多语言数据源");
                return report;
            }

            foreach (var path in FindExcelFiles())
            {
                report.ScannedFiles++;
                ScanWorkbook(path, sourceData, report);
            }

            return report;
        }

        private static void ScanWorkbook(string path, LanguageSourceData sourceData, LocalizationScanReport report)
        {
            try
            {
                var sheets = LocalizationXlsxReader.ReadSheets(path);
                foreach (var sheet in sheets)
                {
                    if (sheet.Name.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    ScanSheet(path, sheet, sourceData, report);
                }
            }
            catch (Exception e)
            {
                report.AddMissing(ToProjectPath(path), string.Empty, string.Empty, "读取 xlsx 失败: " + e.Message);
            }
        }

        private static void ScanSheet(string path, LocalizationXlsxSheet sheet, LanguageSourceData sourceData, LocalizationScanReport report)
        {
            for (var column = 3; column <= sheet.EndColumn; column++)
            {
                var fieldType = sheet.GetCell(5, column).Trim();
                if (!IsI18nField(fieldType))
                {
                    continue;
                }

                report.ScannedColumns++;
                ScanColumn(path, sheet, column, sourceData, report);
            }
        }

        private static void ScanColumn(
            string path,
            LocalizationXlsxSheet sheet,
            int column,
            LanguageSourceData sourceData,
            LocalizationScanReport report)
        {
            var fieldName = sheet.GetCell(4, column).Trim();
            for (var row = 6; row <= sheet.EndRow; row++)
            {
                if (sheet.GetCell(row, 2).Contains("#"))
                {
                    report.Skipped++;
                    continue;
                }

                if (string.IsNullOrEmpty(sheet.GetCell(row, 3).Trim()))
                {
                    continue;
                }

                var key = sheet.GetCell(row, column).Trim();
                if (string.IsNullOrEmpty(key))
                {
                    report.Skipped++;
                    continue;
                }

                report.References++;
                if (sourceData.GetTermData(key) == null)
                {
                    report.AddMissing(ToProjectPath(path), string.Format("{0}!{1}{2}", sheet.Name, ToColumnName(column), row), key, fieldName + " 未在 I2 中找到");
                }
            }
        }

        private static IEnumerable<string> FindExcelFiles()
        {
            foreach (var root in new[] { "Assets", "Packages" })
            {
                if (!Directory.Exists(root))
                {
                    continue;
                }

                foreach (var path in Directory.GetFiles(root, "*.xlsx", SearchOption.AllDirectories))
                {
                    var normalized = path.Replace("\\", "/");
                    var fileName = Path.GetFileName(normalized);
                    if (!normalized.Contains("/Excel/", StringComparison.Ordinal) || fileName.StartsWith("~$", StringComparison.Ordinal) || fileName.Contains("#"))
                    {
                        continue;
                    }

                    yield return normalized;
                }
            }
        }

        private static bool IsI18nField(string fieldType)
        {
            return !string.IsNullOrEmpty(fieldType) && fieldType.IndexOf("i18n", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ToColumnName(int column)
        {
            var result = string.Empty;
            while (column > 0)
            {
                column--;
                result = (char)('A' + column % 26) + result;
                column /= 26;
            }

            return result;
        }

        private static string ToProjectPath(string path)
        {
            return path.Replace("\\", "/");
        }
    }
}
#endif
