#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ET
{
    /// <summary>
    /// 红点系统 所有key枚举
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [UniqueId]
    public static class ERedDotKeyType
    {
#if ODIN_INSPECTOR
        [LabelText("无")]
#endif
        public const int None = 0;

#if ODIN_INSPECTOR
        [LabelText("主")]
#endif
        public const int Key1 = 1;

#if ODIN_INSPECTOR
        [LabelText("商店")]
#endif
        public const int Key2 = 2;

#if ODIN_INSPECTOR
        [LabelText("钻石")]
#endif
        public const int Key3 = 3;

#if ODIN_INSPECTOR
        [LabelText("金币")]
#endif
        public const int Key4 = 4;

#if ODIN_INSPECTOR
        [LabelText("装备")]
#endif
        public const int Key5 = 5;

#if ODIN_INSPECTOR
        [LabelText("强化")]
#endif
        public const int Key6 = 6;

#if ODIN_INSPECTOR
        [LabelText("升级")]
#endif
        public const int Key7 = 7;
    }
}