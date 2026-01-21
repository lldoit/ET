using System.Collections.Generic;
using ET.Client;

namespace ET
{
    [FriendOf(typeof(BattleSceneComponent))]
    [FriendOf(typeof(TurnManagerComponent))]
    [FriendOf(typeof(EntityGroup))]
    [FriendOf(typeof(EntityHero))]
    [EntitySystemOf(typeof(BattleSceneComponent))]
    public static partial class BattleSceneComponentSystem
    {
        [EntitySystem]
        private static void Awake(this BattleSceneComponent self)
        {
            self.CurrentTurn = 0;
            self.BattleState = 0; // 准备中
            self.RedGroup = self.AddChild<EntityGroup>();
            self.BlueGroup = self.AddChild<EntityGroup>();

            // 添加回合管理器
            TurnManagerComponent turnManager = self.AddComponent<TurnManagerComponent>();
            turnManager.BattleSceneRef = self;
        }

        [EntitySystem]
        private static void Destroy(this BattleSceneComponent self)
        {
            // 清理战斗资源
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="self">战斗场景组件</param>
        /// <param name="levelId">关卡ID</param>
        /// <param name="maxTurns">最大回合数</param>
        public static async ETTask StartBattle(this BattleSceneComponent self, int levelId, int maxTurns = 30)
        {
            self.LevelId = levelId;
            self.BattleState = 1; // 进行中

            // 初始化队伍
            EntityGroup redGroup = self.RedGroup;
            EntityGroup blueGroup = self.BlueGroup;

            if (redGroup != null && blueGroup != null)
            {
                redGroup.SetOtherGroup(blueGroup);
                blueGroup.SetOtherGroup(redGroup);
                redGroup.Camp = ECamp.Red;
                blueGroup.Camp = ECamp.Blue;
            }

            // TODO: 根据关卡配置初始化英雄和敌人
            self.InitializeHeroes(levelId);
            self.InitializeEnemies(levelId);

            // 启动回合管理器
            TurnManagerComponent turnManager = self.GetComponent<TurnManagerComponent>();
            if (turnManager != null)
            {
                turnManager.StartBattle(maxTurns);
            }

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 结束战斗
        /// </summary>
        /// <param name="self">战斗场景组件</param>
        /// <param name="isVictory">是否胜利</param>
        public static async ETTask EndBattle(this BattleSceneComponent self, bool isVictory)
        {
            self.BattleState = isVictory ? 2 : 3;

            // 停止回合管理器
            TurnManagerComponent turnManager = self.GetComponent<TurnManagerComponent>();
            if (turnManager != null)
            {
                turnManager.EndBattle(isVictory ? EBattleResult.Victory : EBattleResult.Defeat);
            }

            // 发布战斗结束事件，UI层可以订阅此事件显示结算界面
            Scene scene = self.IScene as Scene;
            EventSystem.Instance.Publish(scene, new ET.Client.BattleEndEvent { IsVictory = isVictory });

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 添加玩家英雄
        /// </summary>
        /// <param name="self">战斗场景组件</param>
        /// <param name="heroId">英雄配置Id</param>
        /// <param name="heroColor">英雄对应颜色</param>
        /// <param name="maxEnergy">满能量值</param>
        public static EntityHero AddPlayerHero(this BattleSceneComponent self, int heroId, int heroColor, int maxEnergy = 100)
        {
            EntityGroup playerGroup = self.RedGroup;
            if (playerGroup == null)
                return null;

            EntityHero hero = playerGroup.AddChild<EntityHero, int>(heroId);
            hero.HeroColor = heroColor;
            hero.MaxEnergy = maxEnergy;
            hero.Energy = 0;
            hero.GroupRef = playerGroup;
            playerGroup.Entitys.Add(hero);

            // 添加Buff组件
            BuffComponent buffCom = hero.AddComponent<BuffComponent>();
            buffCom.SetOwner(hero);
            
            EventSystem.Instance.Publish(self.Scene(), new AfterEntityHeroCreate() {Hero = hero});

            return hero;
        }

        /// <summary>
        /// 添加敌方英雄
        /// </summary>
        /// <param name="self">战斗场景组件</param>
        /// <param name="heroId">英雄配置Id</param>
        /// <param name="attackInterval">攻击间隔</param>
        /// <param name="energyPerTurn">每回合能量增加</param>
        /// <param name="maxEnergy">满能量值</param>
        public static EntityHero AddEnemyHero(this BattleSceneComponent self, int heroId, int attackInterval = 2, int energyPerTurn = 20, int maxEnergy = 100)
        {
            EntityGroup enemyGroup = self.BlueGroup;
            if (enemyGroup == null)
                return null;

            EntityHero enemy = enemyGroup.AddChild<EntityHero, int>(heroId);
            enemy.MaxEnergy = maxEnergy;
            enemy.Energy = 0;
            enemy.GroupRef = enemyGroup;
            enemyGroup.Entitys.Add(enemy);

            // 添加AI组件
            EnemyAIComponent ai = enemy.AddComponent<EnemyAIComponent>();
            ai.Initialize(attackInterval, energyPerTurn);

            // 添加Buff组件
            BuffComponent buffCom = enemy.AddComponent<BuffComponent>();
            buffCom.SetOwner(enemy);
            
            EventSystem.Instance.Publish(self.Scene(), new AfterEntityHeroCreate() {Hero = enemy});

            return enemy;
        }

        /// <summary>
        /// 根据关卡配置初始化玩家英雄
        /// </summary>
        /// <param name="self">战斗场景组件</param>
        /// <param name="levelId">关卡ID</param>
        private static void InitializeHeroes(this BattleSceneComponent self, int levelId)
        {
            // TODO: 根据关卡配置表获取英雄配置
            // 目前使用默认配置进行测试
            // 后续需要从配置表读取英雄ID、颜色、能量等信息
        }

        /// <summary>
        /// 根据关卡配置初始化敌方单位
        /// </summary>
        /// <param name="self">战斗场景组件</param>
        /// <param name="levelId">关卡ID</param>
        private static void InitializeEnemies(this BattleSceneComponent self, int levelId)
        {
            // TODO: 根据关卡配置表获取敌人配置
            // 目前使用默认配置进行测试
            // 后续需要从配置表读取敌人ID、攻击间隔、能量等信息
        }
    }
}

