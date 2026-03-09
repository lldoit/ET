using UnityEngine.SceneManagement;

namespace ET.Client
{
    /// <summary>
    /// KOF格斗场景切换助手
    /// 提供进入KOF格斗和退出KOF格斗的接口
    /// 参照 TpsSceneHelper 模式实现
    /// 注意：本类不持有 [FriendOf]，所有 Entity 字段访问通过 System 方法代理
    /// </summary>
    public static class KofBattleHelper
    {
        /// <summary>
        /// 进入KOF格斗场景
        /// </summary>
        /// <param name="root">根场景</param>
        public static async ETTask EnterKofBattleAsync(Scene root)
        {
            // 获取当前场景信息，用于退出后返回
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            currentScenesComponent.Scene?.Dispose();

            // 创建 KOF 格斗 ECS 场景
            Scene kofScene = EntitySceneFactory.CreateScene(
                root,
                IdGenerater.Instance.GenerateId(),
                IdGenerater.Instance.GenerateInstanceId(),
                SceneType.KofBattle,
                "KofBattle");

            currentScenesComponent.Scene = kofScene;

            // 加载 Unity 场景资源
            // TODO: 创建 KofBattle.unity 场景文件后取消注释
            // var resourcesLoaderComponent = kofScene.AddComponent<ResourcesLoaderComponent>();
            // await resourcesLoaderComponent.LoadSceneAsync(
            //     "Packages/cn.etetet.kof/Assets/GameRes/Scenes/KofBattle.unity",
            //     LoadSceneMode.Additive);
            // var unityScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            // SceneManager.SetActiveScene(unityScene);

            // ── 初始化 Model 层核心组件 ──

            // 对战管理器（回合/胜负）
            kofScene.AddComponent<KofBattleComponent>();
            KofBattleComponent battle = kofScene.GetComponent<KofBattleComponent>();

            // 玩家1格斗角色：AddChild 支持同类型多实例，每个角色是独立的子 Entity
            KofFighterComponent p1 = kofScene.AddChild<KofFighterComponent>();
            p1.InitFighter(characterId: 1, playerId: 1, facingRight: true, posX: -3f);

            // 玩家2格斗角色
            KofFighterComponent p2 = kofScene.AddChild<KofFighterComponent>();
            p2.InitFighter(characterId: 1, playerId: 2, facingRight: false, posX: 3f);

            // 绑定双方到对战管理器（通过 SetPlayers 方法，规避 ET0002）
            battle.SetPlayers(p1, p2);

            // ── 初始化 View 层输入缓冲组件 ──
            kofScene.AddChild<KofInputBufferComponent, int>(1);  // P1 输入缓冲
            kofScene.AddChild<KofInputBufferComponent, int>(2);  // P2 输入缓冲

            // ── 测试：模拟 P1 命中 P2（轻拳 MoveId=101）──
            // 预期：[KOF] 招式命中：轻拳，伤害=60 / [KOF] 角色受到 60 点伤害, 剩余HP: 940/1000
            EventSystem.Instance.Publish(kofScene, new Evt_KofHitDetection
            {
                AttackerId = p1.Id,
                DefenderId = p2.Id,
                Damage = 0,    // MoveId > 0 时此值被忽略
                MoveId = 101,  // 轻拳
            });

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 退出KOF格斗，返回之前的场景
        /// </summary>
        /// <param name="root">根场景</param>
        public static async ETTask ExitKofBattleAsync(Scene root)
        {
            CurrentScenesComponent currentScenesComponent = root.GetComponent<CurrentScenesComponent>();
            Scene kofScene = currentScenesComponent.Scene;
            if (kofScene == null)
            {
                Log.Error("[KOF] 当前没有KOF格斗场景");
                return;
            }

            // 释放格斗场景（会触发所有 IDestroy）
            kofScene.Dispose();

            Log.Info("[KOF] 已退出KOF格斗场景");

            await ETTask.CompletedTask;
        }
    }
}
