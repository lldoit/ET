#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace YIUIFramework.Editor
{
    internal sealed class LocalizationXlsxSheet
    {
        private readonly Dictionary<int, Dictionary<int, string>> m_Cells = new Dictionary<int, Dictionary<int, string>>();

        public string Name;
        public int EndRow;
        public int EndColumn;

        public void SetCell(int row, int column, string value)
        {
            if (!m_Cells.TryGetValue(row, out var rowCells))
            {
                rowCells = new Dictionary<int, string>();
                m_Cells[row] = rowCells;
            }

            rowCells[column] = value ?? string.Empty;
            EndRow = Math.Max(EndRow, row);
            EndColumn = Math.Max(EndColumn, column);
        }

        public string GetCell(int row, int column)
        {
            return m_Cells.TryGetValue(row, out var rowCells) && rowCells.TryGetValue(column, out var value) ? value : string.Empty;
        }
    }

    internal static class LocalizationXlsxReader
    {
        public static List<LocalizationXlsxSheet> ReadSheets(string path)
        {
            using (var archive = ZipFile.OpenRead(path))
            {
                var sharedStrings = ReadSharedStrings(archive);
                var relationships = ReadRelationships(archive);
                var result = new List<LocalizationXlsxSheet>();
                var workbook = LoadXml(archive, "xl/workbook.xml");
                foreach (XmlElement sheetNode in workbook.GetElementsByTagName("sheet"))
                {
                    var relationId = GetAttributeByLocalName(sheetNode, "id");
                    if (!relationships.TryGetValue(relationId, out var target))
                    {
                        continue;
                    }

                    var sheet = ReadSheet(archive, NormalizeEntryPath(target), sharedStrings);
                    sheet.Name = sheetNode.GetAttribute("name");
                    result.Add(sheet);
                }

                return result;
            }
        }

        private static LocalizationXlsxSheet ReadSheet(ZipArchive archive, string entryPath, List<string> sharedStrings)
        {
            var result = new LocalizationXlsxSheet();
            var xml = LoadXml(archive, entryPath);
            foreach (XmlElement cell in xml.GetElementsByTagName("c"))
            {
                var reference = cell.GetAttribute("r");
                if (!TryParseCellReference(reference, out var row, out var column))
                {
                    continue;
                }

                result.SetCell(row, column, ReadCellValue(cell, sharedStrings));
            }

            return result;
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var result = new List<string>();
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null)
            {
                return result;
            }

            var xml = LoadXml(entry);
            foreach (XmlElement item in xml.GetElementsByTagName("si"))
            {
                var text = string.Empty;
                foreach (XmlElement textNode in item.GetElementsByTagName("t"))
                {
                    text += textNode.InnerText;
                }

                result.Add(text);
            }

            return result;
        }

        private static Dictionary<string, string> ReadRelationships(ZipArchive archive)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            var xml = LoadXml(archive, "xl/_rels/workbook.xml.rels");
            foreach (XmlElement relation in xml.GetElementsByTagName("Relationship"))
            {
                result[relation.GetAttribute("Id")] = relation.GetAttribute("Target");
            }

            return result;
        }

        private static string ReadCellValue(XmlElement cell, List<string> sharedStrings)
        {
            var type = cell.GetAttribute("t");
            if (type == "inlineStr")
            {
                var inlineText = cell.GetElementsByTagName("t");
                return inlineText.Count > 0 ? inlineText[0].InnerText : string.Empty;
            }

            var valueNodes = cell.GetElementsByTagName("v");
            if (valueNodes.Count <= 0)
            {
                return string.Empty;
            }

            var value = valueNodes[0].InnerText;
            if (type != "s")
            {
                return value;
            }

            return int.TryParse(value, out var index) && index >= 0 && index < sharedStrings.Count ? sharedStrings[index] : string.Empty;
        }

        private static XmlDocument LoadXml(ZipArchive archive, string entryPath)
        {
            var entry = archive.GetEntry(entryPath);
            if (entry == null)
            {
                throw new FileNotFoundException("xlsx entry not found", entryPath);
            }

            return LoadXml(entry);
        }

        private static XmlDocument LoadXml(ZipArchiveEntry entry)
        {
            var xml = new XmlDocument();
            using (var stream = entry.Open())
            {
                xml.Load(stream);
            }

            return xml;
        }

        private static string NormalizeEntryPath(string target)
        {
            target = target.Replace("\\", "/");
            return target.StartsWith("xl/", StringComparison.Ordinal) ? target : "xl/" + target.TrimStart('/');
        }

        private static string GetAttributeByLocalName(XmlElement element, string localName)
        {
            foreach (XmlAttribute attribute in element.Attributes)
            {
                if (attribute.LocalName == localName)
                {
                    return attribute.Value;
                }
            }

            return string.Empty;
        }

        private static bool TryParseCellReference(string reference, out int row, out int column)
        {
            row = 0;
            column = 0;
            if (string.IsNullOrEmpty(reference))
            {
                return false;
            }

            var index = 0;
            while (index < reference.Length && char.IsLetter(reference[index]))
            {
                column = column * 26 + char.ToUpperInvariant(reference[index]) - 'A' + 1;
                index++;
            }

            return index > 0 && int.TryParse(reference.Substring(index), NumberStyles.Integer, CultureInfo.InvariantCulture, out row);
        }
    }
}
#endif
