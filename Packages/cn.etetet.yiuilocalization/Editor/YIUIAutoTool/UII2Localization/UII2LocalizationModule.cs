#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    [YIUIAutoMenu("多语言", 100100)]
    public class UII2LocalizationModule : BaseYIUIToolModule
    {
        [Button("文档", 30, Icon = SdfIconType.Link45deg, IconAlignment = IconAlignment.LeftOfText)]
        [PropertyOrder(-99999)]
        public void OpenDocument()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/ZOKxwi5XsijdX8kPU9McSxs1nxd");
        }

        [BoxGroup("百度翻译设置", CenterLabel = true)]
        [LabelText("App ID")]
        [ShowInInspector]
        private string BaiduAppId
        {
            get => BaiduTranslationSettings.AppId;
            set => BaiduTranslationSettings.AppId = value;
        }

        [BoxGroup("百度翻译设置", CenterLabel = true)]
        [LabelText("Secret Key")]
        [ShowInInspector]
        [PropertySpace(0, 8)]
        private string BaiduSecretKey
        {
            get => BaiduTranslationSettings.SecretKey;
            set => BaiduTranslationSettings.SecretKey = value;
        }

        [BoxGroup("百度翻译设置", CenterLabel = true)]
        [LabelText("单次最大字符数")]
        [ShowInInspector]
        private int BaiduMaxChars
        {
            get => BaiduTranslationSettings.MaxChars;
            set => BaiduTranslationSettings.MaxChars = value;
        }

        [BoxGroup("百度翻译设置", CenterLabel = true)]
        [Button("清空百度翻译配置", 30)]
        private void ClearBaiduTranslateSettings()
        {
            BaiduTranslationSettings.Clear();
        }

        [BoxGroup("百度批量翻译", CenterLabel = true)]
        [LabelText("源语言")]
        [ShowInInspector]
        [ValueDropdown("GetI2LanguageNames")]
        private string BaiduBatchSourceLanguage = YIUIConstHelper.Const.I2DefaultLanguage;

        [BoxGroup("百度批量翻译", CenterLabel = true)]
        [LabelText("目标语言")]
        [ShowInInspector]
        [ValueDropdown("GetI2LanguageNames")]
        private string BaiduBatchTargetLanguage = "English";

        [BoxGroup("百度批量翻译", CenterLabel = true)]
        [LabelText("只翻译空值")]
        [ShowInInspector]
        private bool BaiduBatchOnlyEmpty = true;

        [BoxGroup("百度批量翻译", CenterLabel = true)]
        [LabelText("失败重试次数")]
        [ShowInInspector]
        private int BaiduBatchRetryCount = 1;

        [BoxGroup("百度批量翻译", CenterLabel = true)]
        [LabelText("请求间隔毫秒")]
        [ShowInInspector]
        private int BaiduBatchRequestIntervalMs = 300;

        [BoxGroup("百度批量翻译", CenterLabel = true)]
        [Button("校验百度语言码", 30)]
        private void ValidateBaiduBatchLanguage()
        {
            var editorAsset = LocalizationManager.GetEditorAsset(true);
            var sourceData = editorAsset?.SourceData;
            if (UII2LocalizationBaiduBatchTranslator.Validate(sourceData, GetBaiduBatchOptions(), out var message))
            {
                UnityTipsHelper.Show(message);
            }
            else
            {
                UnityTipsHelper.ShowError(message);
            }
        }

        [BoxGroup("百度批量翻译", CenterLabel = true)]
        [Button("统计待翻译项", 30)]
        private void PreviewBaiduBatchMissing()
        {
            var editorAsset = LocalizationManager.GetEditorAsset(true);
            var report = UII2LocalizationBaiduBatchTranslator.Preview(editorAsset, GetBaiduBatchOptions());
            if (report.Failed > 0)
            {
                UnityTipsHelper.ShowError(report.ToMessage(true));
            }
            else
            {
                UnityTipsHelper.Show(report.ToMessage(true));
            }
        }

        [BoxGroup("百度批量翻译", CenterLabel = true)]
        [Button("翻译目标语言空白项", 35)]
        [GUIColor(0.2f, 0.8f, 0.4f)]
        private void TranslateBaiduBatchMissing()
        {
            UnityTipsHelper.CallBack(
                "确认使用百度翻译批量补全目标语言吗\n\n默认只会翻译空白项\n\n请确认",
                () =>
                {
                    var editorAsset = LocalizationManager.GetEditorAsset(true);
                    var report = UII2LocalizationBaiduBatchTranslator.Translate(editorAsset, GetBaiduBatchOptions());
                    if (report.Failed > 0)
                    {
                        UnityTipsHelper.ShowError(report.ToMessage(false));
                    }
                    else
                    {
                        UnityTipsHelper.Show(report.ToMessage(false));
                    }
                });
        }

        private BaiduBatchTranslationOptions GetBaiduBatchOptions()
        {
            return new BaiduBatchTranslationOptions
            {
                SourceLanguage = BaiduBatchSourceLanguage,
                TargetLanguage = BaiduBatchTargetLanguage,
                OnlyEmpty = BaiduBatchOnlyEmpty,
                RetryCount = Mathf.Max(0, BaiduBatchRetryCount),
                RequestIntervalMs = Mathf.Max(0, BaiduBatchRequestIntervalMs)
            };
        }

        private static string[] GetI2LanguageNames()
        {
            var editorAsset = LocalizationManager.GetEditorAsset(true);
            var sourceData = editorAsset?.SourceData;
            if (sourceData == null)
            {
                return Array.Empty<string>();
            }

            var result = new string[sourceData.mLanguages.Count];
            for (var i = 0; i < sourceData.mLanguages.Count; i++)
            {
                result[i] = sourceData.mLanguages[i].Name;
            }

            return result;
        }

        private LanguageSourceData m_LanguageSourceData;

        [LabelText("全数据名称")]
        [ShowInInspector]
        [ReadOnly]
        public const string UII2SourceResName = "AllSource";

        [LabelText("全数据保存路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        public const string UII2SourceResPath = "Packages/cn.etetet.yiuilocalization/Assets/Editor/I2Localization"; //这是编辑器下的数据 平台运行时 是不需要的

        [LabelText("指定数据保存路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        public const string UII2TargetLanguageResPath = "Packages/cn.etetet.yiuilocalization/Assets/GameRes/I2Localization"; //运行时的资源是拆分的 根据需求加载

        [Button("打开多语言数据", 50)]
        [GUIColor(0.4f, 0.8f, 1)]
        private void OpenI2Languages()
        {
            EditorApplication.ExecuteMenuItem("Tools/I2 Localization/Open I2Languages.asset");
        }

        [Button("导入", 50)]
        [GUIColor(0f, 1f, 1f)]
        private void ImportAllCsvTips()
        {
            UnityTipsHelper.CallBack("确认导入当前所有多语言数据吗\n\n此操作将会覆盖现有数据\n\n请确认", ImportAllCsv);
        }

        [Button("导出", 50)]
        [GUIColor(0f, 1f, 1f)]
        private void ExportAllCsvTips()
        {
            UnityTipsHelper.CallBack("确认导出当前所有多语言数据吗\n\n此操作将会覆盖现有数据\n\n请确认", ExportAllCsv);
        }

        #region 导出

        private string GetSourceResPath()
        {
            var projPath = EditorHelper.GetProjPath(UII2SourceResPath);
            var path     = $"{projPath}/{I2LocalizeHelper.I2ResAssetNamePrefix}{UII2SourceResName}.csv";
            return path;
        }

        private void ExportAllCsv()
        {
            var editorAsset = LocalizationManager.GetEditorAsset(true);
            m_LanguageSourceData = editorAsset?.SourceData;

            if (m_LanguageSourceData == null)
            {
                UnityTipsHelper.ShowError($"没有找到多语言编辑器下的源数据 请检查 {I2LocalizeHelper.I2GlobalSourcesEditorPath}");
                return;
            }

            var path = GetSourceResPath();

            try
            {
                var content = Export_CSV(null);
                var utf8    = new UTF8Encoding(false);
                File.WriteAllText(path, content, utf8);
            }
            catch (Exception e)
            {
                UnityTipsHelper.ShowError($"导出全数据时发生错误 请检查");
                Debug.LogError(e);
                return;
            }

            Debug.Log($"多语言 全数据 {UII2SourceResName} 导出CSV成功 {path}");

            var projPath = EditorHelper.GetProjPath(UII2TargetLanguageResPath);
            if (!Directory.Exists(projPath))
            {
                Directory.CreateDirectory(projPath);
            }

            foreach (var languages in m_LanguageSourceData.mLanguages)
            {
                var targetPath = "";

                try
                {
                    var content = Export_CSV(languages.Name);
                    targetPath = $"{projPath}/{I2LocalizeHelper.I2ResAssetNamePrefix}{languages.Name}.csv";
                    File.WriteAllText(targetPath, content, Encoding.UTF8);
                }
                catch (Exception e)
                {
                    UnityTipsHelper.ShowError($"导出指定数据时发生错误 {languages.Name} 请检查 ");
                    Debug.LogError(e);
                    return;
                }

                Debug.Log($"多语言 指定数据 {languages.Name} 导出CSV成功 {targetPath}");
            }

            UnityTipsHelper.Show($"导出全数据完成 {path}");
            YIUIAutoTool.CloseWindowRefresh();
        }

        #region 导出方法

        private string Export_CSV(string selectLanguage)
        {
            char Separator = ',';
            var  Builder   = new StringBuilder();

            var languages      = m_LanguageSourceData.mLanguages;
            var languagesCount = languages.Count;
            Builder.AppendFormat("Key{0}Type{0}Desc", Separator);
            var currentLanguageIndex = -1;

            for (int i = 0; i < languagesCount; i++)
            {
                var langData = languages[i];

                var currentLanguage = GoogleLanguages.GetCodedLanguage(langData.Name, langData.Code);

                if (!string.IsNullOrEmpty(selectLanguage) && currentLanguage != selectLanguage)
                {
                    continue;
                }

                Builder.Append(Separator);
                if (!langData.IsEnabled())
                    Builder.Append('$');
                AppendString(Builder, currentLanguage, Separator);
                currentLanguageIndex = i;
            }

            if (string.IsNullOrEmpty(selectLanguage))
            {
                currentLanguageIndex = -1;
            }

            Builder.Append("\n");

            var terms = m_LanguageSourceData.mTerms;

            if (string.IsNullOrEmpty(selectLanguage))
            {
                terms.Sort((a, b) => string.CompareOrdinal(a.Term, b.Term));
            }

            foreach (var termData in terms)
            {
                var term = termData.Term;

                foreach (var specialization in termData.GetAllSpecializations())
                    AppendTerm(Builder, currentLanguageIndex, term, termData, specialization, Separator);
            }

            return Builder.ToString();
        }

        private static void AppendTerm(StringBuilder Builder,        int  selectLanguageIndex, string Term, TermData termData,
                                       string        specialization, char Separator)
        {
            //--[ Key ] --------------
            AppendString(Builder, Term, Separator);

            if (!string.IsNullOrEmpty(specialization) && specialization != "Any")
                Builder.AppendFormat("[{0}]", specialization);

            //--[ Type and Description ] --------------
            Builder.Append(Separator);
            Builder.Append(termData.TermType.ToString());
            Builder.Append(Separator);
            AppendString(Builder, selectLanguageIndex <= -1 ? termData.Description : "", Separator);

            var startIndex = selectLanguageIndex <= -1 ? 0 : selectLanguageIndex;
            var maxIndex   = selectLanguageIndex <= -1 ? termData.Languages.Length : selectLanguageIndex + 1;

            //--[ Languages ] --------------
            for (var i = startIndex; i < maxIndex; ++i)
            {
                Builder.Append(Separator);

                var translation = termData.Languages[i];
                if (!string.IsNullOrEmpty(specialization))
                    translation = termData.GetTranslation(i, specialization);

                AppendTranslation(Builder, translation, Separator, null);
            }

            Builder.Append("\n");
        }

        private static void AppendString(StringBuilder Builder, string Text, char Separator)
        {
            if (string.IsNullOrEmpty(Text))
                return;
            Text = Text.Replace("\\n", "\n");
            if (Text.IndexOfAny((Separator + "\n\"").ToCharArray()) >= 0)
            {
                Text = Text.Replace("\"", "\"\"");
                Builder.AppendFormat("\"{0}\"", Text);
            }
            else
            {
                Builder.Append(Text);
            }
        }

        private static void AppendTranslation(StringBuilder Builder, string Text, char Separator, string tags)
        {
            if (string.IsNullOrEmpty(Text))
                return;
            Text = Text.Replace("\\n", "\n");
            if (Text.IndexOfAny((Separator + "\n\"").ToCharArray()) >= 0)
            {
                Text = Text.Replace("\"", "\"\"");
                Builder.AppendFormat("\"{0}{1}\"", tags, Text);
            }
            else
            {
                Builder.Append(tags);
                Builder.Append(Text);
            }
        }

        #endregion

        #endregion

        #region 导入

        private void ImportAllCsv()
        {
            var editorAsset = LocalizationManager.GetEditorAsset(true);
            m_LanguageSourceData = editorAsset?.SourceData;

            if (m_LanguageSourceData == null)
            {
                UnityTipsHelper.ShowError($"没有找到多语言编辑器下的源数据 请检查 {I2LocalizeHelper.I2GlobalSourcesEditorPath}");
                return;
            }

            var path = GetSourceResPath();

            try
            {
	            var utf8  = new UTF8Encoding(false);
                var content = LocalizationReader.ReadCSVfile(path, utf8);
                var sError = m_LanguageSourceData.Import_CSV(string.Empty, content, eSpreadsheetUpdateMode.Replace, ',');
                if (!string.IsNullOrEmpty(sError))
                    UnityTipsHelper.ShowError($"导入全数据时发生错误 请检查 {sError} {path}");
                else
                {
                    var globalSourcesAsset = UpgradeManager.CreateLanguageSources();

                    if (globalSourcesAsset == null)
                        Debug.LogError($"没有找到数据源 {I2LocalizeHelper.I2GlobalSourcesEditorPath}");
                    else
                    {
                        Selection.activeObject = globalSourcesAsset;
                        EditorUtility.SetDirty(globalSourcesAsset);
                    }
                }
            }
            catch (Exception e)
            {
                UnityTipsHelper.ShowError($"导入全数据时发生错误 请检查 {path}");
                Debug.LogError(e);
                return;
            }

            UnityTipsHelper.Show($"导入全数据完成 {path}");
            YIUIAutoTool.CloseWindowRefresh();
        }

        #endregion
    }
}
#endif
