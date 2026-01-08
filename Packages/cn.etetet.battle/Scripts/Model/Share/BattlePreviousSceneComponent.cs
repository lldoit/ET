namespace ET
{
    /// <summary>
    /// 保存进入战斗前的场景信息
    /// 用于战斗结束后返回之前的场景
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class BattlePreviousSceneComponent : Entity, IAwake
    {
        /// <summary>
        /// 之前场景的ID
        /// </summary>
        public long PreviousSceneId;
        
        /// <summary>
        /// 之前场景的名称
        /// </summary>
        public string PreviousSceneName;
        
        /// <summary>
        /// 之前场景的类型
        /// </summary>
        public int PreviousSceneType;
    }
}
