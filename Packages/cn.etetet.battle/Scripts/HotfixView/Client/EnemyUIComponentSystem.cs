namespace ET.Client
{
    [FriendOf(typeof(EnemyUIComponent))]
    [EntitySystemOf(typeof(EnemyUIComponent))]
    public static partial class EnemyUIComponentSystem
    {
        [EntitySystem]
        private static void Awake(this EnemyUIComponent self)
        {
            // TODO: 加载并初始化敌人UI
        }

        [EntitySystem]
        private static void Destroy(this EnemyUIComponent self)
        {
            // 释放UI资源
            YIUIChild uiEntity = self.UIEntityRef;
            if (uiEntity != null)
            {
                uiEntity.Dispose();
            }
        }

        /// <summary>
        /// 更新血条显示
        /// </summary>
        /// <param name="self"></param>
        /// <param name="hpPercent">血量百分比 (0-1)</param>
        public static void UpdateHpBar(this EnemyUIComponent self, float hpPercent)
        {
            // TODO: 更新血条UI显示
        }

        /// <summary>
        /// 显示伤害数字
        /// </summary>
        /// <param name="self"></param>
        /// <param name="damage">伤害值</param>
        public static async ETTask ShowDamageNumber(this EnemyUIComponent self, int damage)
        {
            // TODO: 播放伤害数字动画
            await ETTask.CompletedTask;
        }
    }
}
