namespace ET.Client
{
    /// <summary>
    /// 战斗HUD组件
    /// 管理战斗界面的整体布局，包含三消UI和战斗信息UI
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class BattleHUDComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 三消UI引用（来自 match3 包）
        /// </summary>
        public EntityRef<Match3LevelUIComponent> Match3UIRef;
    }
}
