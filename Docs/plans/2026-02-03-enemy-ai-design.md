# 敌人AI系统实现计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 实现简单定时攻击型敌人AI和玩家HP系统

**Architecture:** 敌人通过Update循环定时攻击玩家，伤害根据玩家状态（掩体/瞄准）调整。玩家HP归零触发游戏结束。

**Tech Stack:** ET 9.0 ECS框架，C#

---

## Task 1: 创建玩家HP组件

**Files:**
- Create: `Packages/cn.etetet.tps/Scripts/Model/Share/TpsPlayerHpComponent.cs`

**Step 1: 创建组件文件**

```csharp
namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class TpsPlayerHpComponent : Entity, IAwake, IDestroy
    {
        public int MaxHp;
        public int CurrentHp;
        public bool IsAlive;
    }
}
```

**Step 2: 验证编译**
- Unity Console无报错

---

## Task 2: 创建玩家HP系统

**Files:**
- Create: `Packages/cn.etetet.tps/Scripts/Hotfix/Share/TpsPlayerHpComponentSystem.cs`

**Step 1: 创建System文件**

```csharp
namespace ET
{
    [FriendOf(typeof(TpsPlayerHpComponent))]
    [EntitySystemOf(typeof(TpsPlayerHpComponent))]
    public static partial class TpsPlayerHpComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TpsPlayerHpComponent self)
        {
            self.MaxHp = 1000;
            self.CurrentHp = self.MaxHp;
            self.IsAlive = true;
            Log.Info($"[TPS] 玩家HP初始化: {self.CurrentHp}/{self.MaxHp}");
        }

        [EntitySystem]
        private static void Destroy(this TpsPlayerHpComponent self) { }

        public static void TakeDamage(this TpsPlayerHpComponent self, int damage, bool fromCover)
        {
            if (!self.IsAlive) return;
            
            int finalDamage = fromCover ? damage / 2 : damage;
            self.CurrentHp -= finalDamage;
            
            Log.Info($"[TPS] 玩家受到{finalDamage}点伤害{(fromCover ? "(掩体减伤)" : "")}, 剩余HP: {self.CurrentHp}/{self.MaxHp}");
            
            if (self.CurrentHp <= 0)
            {
                self.CurrentHp = 0;
                self.IsAlive = false;
                Log.Info("[TPS] 玩家死亡! 游戏结束");
            }
        }
    }
}
```

**Step 2: 验证编译**

---

## Task 3: 创建敌人AI组件

**Files:**
- Create: `Packages/cn.etetet.tps/Scripts/Model/Share/TpsEnemyAIComponent.cs`

**Step 1: 创建组件文件**

```csharp
namespace ET
{
    [ComponentOf(typeof(TpsEnemyComponent))]
    public class TpsEnemyAIComponent : Entity, IAwake, IUpdate, IDestroy
    {
        public float MinAttackInterval;
        public float MaxAttackInterval;
        public int BaseDamage;
        public long NextAttackTime;
    }
}
```

**Step 2: 验证编译**

---

## Task 4: 创建敌人AI系统

**Files:**
- Create: `Packages/cn.etetet.tps/Scripts/Hotfix/Share/TpsEnemyAIComponentSystem.cs`

**Step 1: 创建System文件**

```csharp
namespace ET
{
    [FriendOf(typeof(TpsEnemyAIComponent))]
    [FriendOf(typeof(TpsEnemyComponent))]
    [FriendOf(typeof(TpsPlayerHpComponent))]
    [EntitySystemOf(typeof(TpsEnemyAIComponent))]
    public static partial class TpsEnemyAIComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TpsEnemyAIComponent self)
        {
            self.MinAttackInterval = 2f;
            self.MaxAttackInterval = 4f;
            self.BaseDamage = 100;
            self.ScheduleNextAttack();
        }

        [EntitySystem]
        private static void Update(this TpsEnemyAIComponent self)
        {
            TpsEnemyComponent enemy = self.Parent as TpsEnemyComponent;
            if (enemy == null || !enemy.IsAlive) return;
            
            if (TimeInfo.Instance.ServerNow() >= self.NextAttackTime)
            {
                self.PerformAttack();
                self.ScheduleNextAttack();
            }
        }

        [EntitySystem]
        private static void Destroy(this TpsEnemyAIComponent self) { }

        private static void ScheduleNextAttack(this TpsEnemyAIComponent self)
        {
            float interval = RandomGenerator.RandomNumber(
                (int)(self.MinAttackInterval * 1000),
                (int)(self.MaxAttackInterval * 1000)) / 1000f;
            self.NextAttackTime = TimeInfo.Instance.ServerNow() + (long)(interval * 1000);
        }

        private static void PerformAttack(this TpsEnemyAIComponent self)
        {
            Scene scene = self.Root();
            TpsPlayerHpComponent playerHp = scene.GetComponent<TpsPlayerHpComponent>();
            TpsStateComponent state = scene.GetComponent<TpsStateComponent>();
            
            if (playerHp == null || !playerHp.IsAlive) return;
            
            bool isCover = state?.CurrentState == TpsCharacterState.Cover;
            playerHp.TakeDamage(self.BaseDamage, isCover);
        }
    }
}
```

**Step 2: 验证编译**

---

## Task 5: 修改敌人创建逻辑

**Files:**
- Modify: `Packages/cn.etetet.tps/Scripts/Hotfix/Share/TpsEnemyManagerComponentSystem.cs`

**Step 1: 在CreateEnemy方法中添加AI组件**

在 `TpsEnemyComponent enemy = self.AddChild...` 后添加：
```csharp
enemy.AddComponent<TpsEnemyAIComponent>();
```

**Step 2: 验证编译**

---

## Task 6: 修改场景初始化

**Files:**
- Modify: `Packages/cn.etetet.tps/Scripts/Hotfix/Client/AfterCreateTpsScene_InitBattle.cs`

**Step 1: 添加玩家HP组件**

在 `scene.AddComponent<TpsWeaponComponent>()` 后添加：
```csharp
scene.AddComponent<TpsPlayerHpComponent>();
```

**Step 2: 验证编译和运行**
- 进入TPS场景后应看到日志:
  - `[TPS] 玩家HP初始化: 1000/1000`
  - 每2-4秒: `[TPS] 玩家受到XX点伤害`

---

## Task 7: 运行测试验证

**验证步骤:**
1. 进入TPS场景
2. 观察日志：敌人应每2-4秒攻击一次
3. 瞄准状态受到100点伤害
4. 掩体状态受到50点伤害
5. HP归零后显示游戏结束

---

**计划完成！**
