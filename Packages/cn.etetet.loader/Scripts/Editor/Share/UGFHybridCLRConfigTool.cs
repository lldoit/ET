using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ET
{
    internal enum UGFHybridCLRConfigMode
    {
        StripLink,
        AotMetadata
    }

    internal static class UGFHybridCLRConfigTool
    {
        private const string LinkFile = "Assets/link.xml";
        private const string GenerateTag = "<!--UGF_GENERATE_TAG-->";
        private const string AssemblyPattern = "<assembly[\\s]+fullname[\\s]*=[\\s]*\"([^\"]+)\"";

        public static string GetStrippedAotDllDirectory()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            return Path.Combine(HybridCLRSettings.Instance.strippedAOTDllOutputRootDir, target.ToString());
        }

        public static string[] GetProjectAssemblyNames(UGFHybridCLRConfigMode mode)
        {
            string dir = GetStrippedAotDllDirectory();
            if (!Directory.Exists(dir))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories)
                .Select(path => GetAssemblyName(path, mode))
                .Distinct()
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public static string[] GetSelectedLinkAssemblies()
        {
            if (!File.Exists(LinkFile))
            {
                return Array.Empty<string>();
            }

            string[] lines = File.ReadAllLines(LinkFile);
            GetGenerateRange(lines, out int begin, out int end);
            IEnumerable<string> scanLines = begin >= 0 && end > begin
                ? lines.Skip(begin + 1).Take(end - begin - 1)
                : lines;

            return scanLines.Select(TryParseAssemblyName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToArray();
        }

        public static string[] GetSelectedAotDlls()
        {
            return HybridCLRSettings.Instance.patchAOTAssemblies ?? Array.Empty<string>();
        }

        public static bool SaveLinkAssemblies(string[] assemblyNames)
        {
            List<string> lines = LoadOrCreateLinkFile();
            EnsureGenerateRange(lines, out int begin, out int end);
            string[] autoLines = assemblyNames.Select(FormatLinkAssemblyLine).ToArray();
            lines.RemoveRange(begin + 1, end - begin - 1);
            lines.InsertRange(begin + 1, autoLines);

            File.WriteAllLines(LinkFile, lines);
            AssetDatabase.Refresh();
            return true;
        }

        public static bool SaveAotDlls(string[] aotDlls)
        {
            HybridCLRSettings.Instance.patchAOTAssemblies = aotDlls;
            HybridCLRSettings.Save();

            try
            {
                HybridCLREditor.CopyAotDll();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog("AOT泛型补充配置", $"配置已保存，但复制 AOT DLL 失败：\n{exception.Message}", "OK");
                return false;
            }
        }

        private static string GetAssemblyName(string path, UGFHybridCLRConfigMode mode)
        {
            return mode == UGFHybridCLRConfigMode.AotMetadata
                ? Path.GetFileName(path)
                : Path.GetFileNameWithoutExtension(path);
        }

        private static List<string> LoadOrCreateLinkFile()
        {
            if (File.Exists(LinkFile))
            {
                return File.ReadAllLines(LinkFile).ToList();
            }

            return new List<string> { "<linker>", "</linker>" };
        }

        private static void EnsureGenerateRange(List<string> lines, out int begin, out int end)
        {
            GetGenerateRange(lines.ToArray(), out begin, out end);
            int linkerLine = Math.Max(0, lines.FindIndex(line => line.Trim() == "<linker>"));

            if (begin < 0)
            {
                lines.Insert(linkerLine + 1, GenerateTag);
            }

            GetGenerateRange(lines.ToArray(), out begin, out end);
            if (end < 0)
            {
                lines.Insert(begin + 1, GenerateTag);
            }

            GetGenerateRange(lines.ToArray(), out begin, out end);
        }

        private static void GetGenerateRange(string[] lines, out int begin, out int end)
        {
            begin = -1;
            end = -1;
            for (int i = 0; i < lines.Length; ++i)
            {
                if (lines[i].Trim() != GenerateTag)
                {
                    continue;
                }

                if (begin < 0)
                {
                    begin = i;
                    continue;
                }

                end = i;
                return;
            }
        }

        private static string TryParseAssemblyName(string line)
        {
            Match match = Regex.Match(line, AssemblyPattern);
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string FormatLinkAssemblyLine(string assemblyName)
        {
            return $"    <assembly fullname=\"{assemblyName}\" preserve=\"all\" />";
        }
    }
}
