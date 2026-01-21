using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 战斗站位组件 - 管理战斗中角色的站位配置
    /// 只包含数据，不包含方法
    /// 所有逻辑请使用 FormationComponentSystem 扩展方法
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class FormationComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 站位配置引用
        /// </summary>
        public BattleFormationConfig FormationConfig;
        
        /// <summary>
        /// 站位配置资源路径
        /// </summary>
        public string ConfigAssetPath;
        
        /// <summary>
        /// 战斗界面根节点（用于坐标转换）
        /// </summary>
        public RectTransform BattleRoot;
    }
}
