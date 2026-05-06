#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace YIUIFramework.Editor
{
    internal sealed class LocalizationScanIssue
    {
        public string Path;
        public string Location;
        public string Key;
        public string Message;

        public override string ToString()
        {
            return string.Format("{0}{1} Key={2} {3}", Path, string.IsNullOrEmpty(Location) ? string.Empty : ":" + Location, Key, Message);
        }
    }

    internal sealed class LocalizationScanReport
    {
        public string Title;
        public int ScannedFiles;
        public int ScannedColumns;
        public int References;
        public int Missing;
        public int DynamicCalls;
        public int Skipped;
        public readonly List<LocalizationScanIssue> Issues = new List<LocalizationScanIssue>();

        public bool HasIssues => Issues.Count > 0;

        public void AddMissing(string path, string location, string key, string message)
        {
            Missing++;
            Issues.Add(new LocalizationScanIssue
            {
                Path = path,
                Location = location,
                Key = key,
                Message = message
            });
        }

        public string ToMessage()
        {
            return string.Format(
                "{0}\n扫描文件: {1}\n扫描列: {2}\n引用数: {3}\n缺失Key: {4}\n动态调用: {5}\n跳过: {6}",
                Title,
                ScannedFiles,
                ScannedColumns,
                References,
                Missing,
                DynamicCalls,
                Skipped);
        }

        public void LogDetails()
        {
            var builder = new StringBuilder();
            builder.AppendLine(ToMessage());
            if (Issues.Count > 0)
            {
                builder.AppendLine("问题明细:");
                foreach (var issue in Issues)
                {
                    builder.Append("- ").AppendLine(issue.ToString());
                }

                Debug.LogWarning(builder.ToString());
                return;
            }

            Debug.Log(builder.ToString());
        }
    }
}
#endif
