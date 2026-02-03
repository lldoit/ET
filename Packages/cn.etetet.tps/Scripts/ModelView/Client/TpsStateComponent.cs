namespace ET.Client
{
    /// <summary>
    /// TPS角色战斗状态枚举
    /// </summary>
    public enum TpsCharacterState
    {
        /// <summary>
        /// 掩体状态：角色躲在掩体后，无法射击，自动换弹，减少受到的伤害
        /// </summary>
        Cover = 0,
        
        /// <summary>
        /// 瞄准状态：角色探出掩体，可以射击，容易受到伤害
        /// </summary>
        Aiming = 1,
        
        /// <summary>
        /// 换弹状态：角色正在换弹，无法射击
        /// </summary>
        Reloading = 2
    }

    /// <summary>
    /// TPS角色状态组件
    /// 管理角色在战斗中的状态切换（掩体/瞄准/换弹）
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class TpsStateComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前角色状态
        /// </summary>
        public TpsCharacterState CurrentState;
        
        /// <summary>
        /// 状态进入时间（用于计算状态持续时间）
        /// </summary>
        public long StateEnterTime;
        
        /// <summary>
        /// 是否可以切换状态（防止快速连续切换）
        /// </summary>
        public bool CanSwitchState;
        
        /// <summary>
        /// 状态切换冷却时间（毫秒）
        /// </summary>
        public int StateSwitchCooldown;
    }
}
