namespace I2.Loc
{
    public static class BaiduTranslationSettings
    {
        private const string AppIdKey = "ET.YIUI.Localization.Baidu.AppId";
        private const string SecretKeyKey = "ET.YIUI.Localization.Baidu.SecretKey";
        private const string MaxCharsKey = "ET.YIUI.Localization.Baidu.MaxChars";

        public const int DefaultMaxChars = 3000;

        public static string AppId
        {
            get
            {
                #if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetString(AppIdKey, string.Empty);
                #else
                return string.Empty;
                #endif
            }
            set
            {
                #if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetString(AppIdKey, value ?? string.Empty);
                #endif
            }
        }

        public static string SecretKey
        {
            get
            {
                #if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetString(SecretKeyKey, string.Empty);
                #else
                return string.Empty;
                #endif
            }
            set
            {
                #if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetString(SecretKeyKey, value ?? string.Empty);
                #endif
            }
        }

        public static int MaxChars
        {
            get
            {
                #if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetInt(MaxCharsKey, DefaultMaxChars);
                #else
                return DefaultMaxChars;
                #endif
            }
            set
            {
                #if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetInt(MaxCharsKey, value > 0 ? value : DefaultMaxChars);
                #endif
            }
        }

        public static bool HasCredentials()
        {
            return !string.IsNullOrEmpty(AppId) && !string.IsNullOrEmpty(SecretKey);
        }

        public static void Clear()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorPrefs.DeleteKey(AppIdKey);
            UnityEditor.EditorPrefs.DeleteKey(SecretKeyKey);
            UnityEditor.EditorPrefs.DeleteKey(MaxCharsKey);
            #endif
        }
    }
}
