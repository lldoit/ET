#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using I2.Loc;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace YIUIFramework.Editor
{
    internal sealed class BaiduBatchTranslationOptions
    {
        public string SourceLanguage;
        public string TargetLanguage;
        public bool OnlyEmpty = true;
        public int RetryCount = 1;
        public int RequestIntervalMs = 300;
    }

    internal sealed class BaiduBatchTranslationReport
    {
        public int TotalTextTerms;
        public int Translated;
        public int SkippedExisting;
        public int SkippedEmptySource;
        public int SkippedSpecialization;
        public int SkippedNonText;
        public int Failed;
        public int EstimatedRequests;
        public int EstimatedChars;
        public string LastError;
        public readonly List<string> FailedDetails = new List<string>();

        public string ToMessage(bool preview)
        {
            var builder = new StringBuilder();
            builder.AppendLine(preview ? "百度批量翻译预检" : "百度批量翻译完成");
            builder.AppendFormat("文本Term: {0}\n", TotalTextTerms);
            builder.AppendFormat("预计请求: {0}\n", EstimatedRequests);
            builder.AppendFormat("预计字符: {0}\n", EstimatedChars);
            if (!preview)
            {
                builder.AppendFormat("成功: {0}\n", Translated);
            }

            builder.AppendFormat("已有翻译跳过: {0}\n", SkippedExisting);
            builder.AppendFormat("源文案为空跳过: {0}\n", SkippedEmptySource);
            builder.AppendFormat("含多形态跳过: {0}\n", SkippedSpecialization);
            builder.AppendFormat("非文本跳过: {0}\n", SkippedNonText);
            builder.AppendFormat("失败: {0}", Failed);
            if (!string.IsNullOrEmpty(LastError))
            {
                builder.AppendFormat("\n最后错误: {0}", LastError);
            }

            if (FailedDetails.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("失败明细:");
                var count = Mathf.Min(5, FailedDetails.Count);
                for (var i = 0; i < count; i++)
                {
                    builder.Append("- ").AppendLine(FailedDetails[i]);
                }

                if (FailedDetails.Count > count)
                {
                    builder.AppendFormat("... 还有 {0} 条，请查看 Console", FailedDetails.Count - count);
                }
            }

            return builder.ToString();
        }
    }

    internal static class UII2LocalizationBaiduBatchTranslator
    {
        private const int RequestTimeoutSeconds = 30;

        public static bool Validate(LanguageSourceData sourceData, BaiduBatchTranslationOptions options, out string message)
        {
            return Validate(sourceData, options, false, out message, out _, out _, out _, out _);
        }

        public static BaiduBatchTranslationReport Preview(LanguageSourceAsset sourceAsset, BaiduBatchTranslationOptions options)
        {
            var report = new BaiduBatchTranslationReport();
            if (sourceAsset == null || sourceAsset.SourceData == null)
            {
                AddFailure(report, "没有找到 I2 多语言数据源");
                return report;
            }

            var sourceData = sourceAsset.SourceData;
            if (!Validate(sourceData, options, false, out var validateMessage, out var sourceIndex, out var targetIndex, out var from, out var to))
            {
                AddFailure(report, validateMessage);
                return report;
            }

            for (var i = 0; i < sourceData.mTerms.Count; i++)
            {
                InspectTerm(sourceData.mTerms[i], sourceIndex, targetIndex, from, to, options, report, out _);
            }

            return report;
        }

        private static bool Validate(
            LanguageSourceData sourceData,
            BaiduBatchTranslationOptions options,
            bool requireCredentials,
            out string message,
            out int sourceIndex,
            out int targetIndex,
            out string from,
            out string to)
        {
            message = null;
            sourceIndex = -1;
            targetIndex = -1;
            from = string.Empty;
            to = string.Empty;
            if (sourceData == null)
            {
                message = "没有找到 I2 多语言数据源";
                return false;
            }

            if (requireCredentials && !BaiduTranslationSettings.HasCredentials())
            {
                message = "请先配置百度翻译 App ID 和 Secret Key";
                return false;
            }

            if (string.IsNullOrEmpty(options.SourceLanguage) || string.IsNullOrEmpty(options.TargetLanguage))
            {
                message = "请选择源语言和目标语言";
                return false;
            }

            if (string.Equals(options.SourceLanguage, options.TargetLanguage, StringComparison.OrdinalIgnoreCase))
            {
                message = "源语言和目标语言不能相同";
                return false;
            }

            sourceIndex = sourceData.GetLanguageIndex(options.SourceLanguage, false, false);
            if (sourceIndex < 0)
            {
                message = string.Format("源语言不存在: {0}", options.SourceLanguage);
                return false;
            }

            targetIndex = sourceData.GetLanguageIndex(options.TargetLanguage, false, false);
            if (targetIndex < 0)
            {
                message = string.Format("目标语言不存在: {0}", options.TargetLanguage);
                return false;
            }

            from = GetBaiduLanguageCode(sourceData.mLanguages[sourceIndex]);
            to = GetBaiduLanguageCode(sourceData.mLanguages[targetIndex]);
            if (string.IsNullOrEmpty(from))
            {
                message = string.Format("百度翻译不支持源语言: {0}", options.SourceLanguage);
                return false;
            }

            if (string.IsNullOrEmpty(to) || to == "auto")
            {
                message = string.Format("百度翻译不支持目标语言: {0}", options.TargetLanguage);
                return false;
            }

            message = string.Format("语言码校验通过: {0}({1}) -> {2}({3})", options.SourceLanguage, from, options.TargetLanguage, to);
            return true;
        }

        public static BaiduBatchTranslationReport Translate(LanguageSourceAsset sourceAsset, BaiduBatchTranslationOptions options)
        {
            var report = new BaiduBatchTranslationReport();
            if (sourceAsset == null || sourceAsset.SourceData == null)
            {
                AddFailure(report, "没有找到 I2 多语言数据源");
                return report;
            }

            var sourceData = sourceAsset.SourceData;
            if (!Validate(sourceData, options, true, out var validateMessage, out var sourceIndex, out var targetIndex, out var from, out var to))
            {
                AddFailure(report, validateMessage);
                return report;
            }

            try
            {
                for (var i = 0; i < sourceData.mTerms.Count; i++)
                {
                    var term = sourceData.mTerms[i];
                    EditorUtility.DisplayProgressBar("百度批量翻译", term.Term, sourceData.mTerms.Count <= 0 ? 1f : (float)i / sourceData.mTerms.Count);
                    TranslateTerm(term, sourceIndex, targetIndex, from, to, options, report);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (report.Translated > 0)
            {
                EditorUtility.SetDirty(sourceAsset);
                AssetDatabase.SaveAssets();
            }

            return report;
        }

        private static void TranslateTerm(
            TermData term,
            int sourceIndex,
            int targetIndex,
            string from,
            string to,
            BaiduBatchTranslationOptions options,
            BaiduBatchTranslationReport report)
        {
            if (!InspectTerm(term, sourceIndex, targetIndex, from, to, options, report, out var query))
            {
                return;
            }

            if (!TryTranslateText(query, from, to, options, out var translated, out var error))
            {
                AddFailure(report, string.Format("{0}: {1}", term.Term, error));
                return;
            }

            term.SetTranslation(targetIndex, translated);
            term.Flags[targetIndex] = (byte)(term.Flags[targetIndex] | (byte)TranslationFlag.AutoTranslated);
            report.Translated++;
        }

        private static bool InspectTerm(
            TermData term,
            int sourceIndex,
            int targetIndex,
            string from,
            string to,
            BaiduBatchTranslationOptions options,
            BaiduBatchTranslationReport report,
            out TranslationQuery query)
        {
            query = default(TranslationQuery);
            if (term.TermType != eTermType.Text)
            {
                report.SkippedNonText++;
                return false;
            }

            report.TotalTextTerms++;
            term.Validate();

            var sourceText = term.GetTranslation(sourceIndex, null, true);
            if (string.IsNullOrEmpty(sourceText))
            {
                report.SkippedEmptySource++;
                return false;
            }

            var targetText = term.GetTranslation(targetIndex, null, true);
            if (options.OnlyEmpty && !string.IsNullOrEmpty(targetText))
            {
                report.SkippedExisting++;
                return false;
            }

            if (term.HasSpecializations())
            {
                report.SkippedSpecialization++;
                return false;
            }

            query = CreateProtectedQuery(sourceText, from, to);
            if (query.Text.Length > BaiduTranslationSettings.MaxChars)
            {
                AddFailure(report, string.Format("{0}: 文本长度 {1} 超过百度单次最大字符数 {2}", term.Term, query.Text.Length, BaiduTranslationSettings.MaxChars));
                return false;
            }

            report.EstimatedRequests++;
            report.EstimatedChars += query.Text.Length;
            return true;
        }

        private static bool TryTranslateText(
            TranslationQuery query,
            string from,
            string to,
            BaiduBatchTranslationOptions options,
            out string translated,
            out string error)
        {
            translated = null;
            error = null;

            var attempts = Math.Max(1, options.RetryCount + 1);
            for (var i = 0; i < attempts; i++)
            {
                if (options.RequestIntervalMs > 0)
                {
                    Thread.Sleep(options.RequestIntervalMs);
                }

                if (TryRequest(query.Text, from, to, out translated, out error))
                {
                    translated = BaiduTranslationUtility.RestoreTags(translated, query.Tags);
                    return true;
                }
            }

            return false;
        }

        private static void AddFailure(BaiduBatchTranslationReport report, string error)
        {
            report.Failed++;
            report.LastError = error;
            report.FailedDetails.Add(error);
            Debug.LogWarning(error);
        }

        private static TranslationQuery CreateProtectedQuery(string sourceText, string from, string to)
        {
            var dict = new Dictionary<string, TranslationQuery>(StringComparer.Ordinal);
            GoogleTranslation.AddQuery(sourceText, from, to, dict);
            return dict[sourceText];
        }

        private static bool TryRequest(string text, string from, string to, out string translated, out string error)
        {
            translated = null;
            error = null;

            var salt = DateTime.UtcNow.Ticks.ToString();
            var sign = BaiduTranslationUtility.GenerateSign(BaiduTranslationSettings.AppId, text, salt, BaiduTranslationSettings.SecretKey);
            var url = BaiduTranslationUtility.BuildRequestUrl(text, from, to, salt, sign, BaiduTranslationSettings.AppId);
            using (var request = UnityWebRequest.Get(url))
            {
                var operation = request.SendWebRequest();
                var start = DateTime.UtcNow;
                while (!operation.isDone)
                {
                    if ((DateTime.UtcNow - start).TotalSeconds > RequestTimeoutSeconds)
                    {
                        request.Abort();
                        error = "百度翻译请求超时";
                        return false;
                    }
                }

                if (!string.IsNullOrEmpty(request.error))
                {
                    error = request.error;
                    return false;
                }

                return BaiduTranslationUtility.TryParseResponse(request.downloadHandler.text, out translated, out error);
            }
        }

        private static string GetBaiduLanguageCode(LanguageData languageData)
        {
            if (languageData == null)
            {
                return string.Empty;
            }

            var code = BaiduTranslationUtility.GetBaiduLanguageCode(languageData.Code);
            if (!string.IsNullOrEmpty(code))
            {
                return code;
            }

            code = BaiduTranslationUtility.GetBaiduLanguageCode(languageData.Name);
            if (!string.IsNullOrEmpty(code))
            {
                return code;
            }

            return BaiduTranslationUtility.GetBaiduLanguageCode(GoogleLanguages.GetLanguageCode(languageData.Name));
        }
    }
}
#endif
