# Phase 4: 游戏循环与UI实现计划

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 实现单波次战斗流程和最小可行UI

**Architecture:** TpsBattleComponent管理战斗状态，监听玩家死亡和敌人全灭事件。TpsBattlePanelComponent管理UI显示。

**Tech Stack:** ET 9.0 ECS, Unity UI

---

## Task 1: 创建战斗状态组件

**Files:**
- Create: `Packages/cn.etetet.tps/Scripts/Model/Share/TpsBattleComponent.cs`

```csharp
namespace ET
{
    public enum TpsBattleState
    {
        Ready,
        Fighting,
        Win,
        Loss
    }

    [ComponentOf(typeof(Scene))]
    public class TpsBattleComponent : Entity, IAwake, IDestroy
    {
        public TpsBattleState State;
        public int TotalEnemyCount;
        public int KilledEnemyCount;
    }
}
```

---

## Task 2: 创建战斗系统

**Files:**
- Create: `Packages/cn.etetet.tps/Scripts/Hotfix/Share/TpsBattleComponentSystem.cs`

**功能：**
- Awake: 设置初始状态为Fighting
- StartBattle: 开始战斗
- CheckWinCondition: 检查胜利（敌人全灭）
- CheckLossCondition: 检查失败（玩家死亡）
- OnBattleEnd: 发布战斗结束事件

---

## Task 3: 添加战斗结束事件

**Files:**
- Modify: `Packages/cn.etetet.tps/Scripts/Model/Share/TpsEventType.cs`

添加事件结构体:
```csharp
public struct TpsBattleEndEvent
{
    public bool IsWin;
}
```

---

## Task 4: 修改HP和敌人系统

**修改:**
- `TpsPlayerHpComponentSystem.cs`: 玩家死亡时检查战斗失败
- `TpsEnemyComponentSystem.cs`: 敌人死亡时检查战斗胜利

---

## Task 5: 创建战斗UI面板组件

**Files:**
- Create: `Packages/cn.etetet.tps/Scripts/ModelView/Client/TpsBattlePanelComponent.cs`

```csharp
[ComponentOf(typeof(Scene))]
public class TpsBattlePanelComponent : Entity, IAwake, IDestroy
{
    public GameObject PanelGo;
    public Text AmmoText;
    public Slider HpSlider;
    public Text HpText;
    public GameObject GameOverPanel;
    public Text ResultText;
}
```

---

## Task 6: 创建战斗UI系统

**Files:**
- Create: `Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsBattlePanelComponentSystem.cs`

**功能：**
- Awake: 动态创建UI元素
- UpdateAmmo: 更新弹药显示
- UpdateHp: 更新血条
- ShowGameOver: 显示胜利/失败

---

## Task 7: 集成到场景初始化

**修改:**
- `AfterCreateTpsScene_InitBattle.cs`: 添加TpsBattleComponent
- `AfterCreateTpsScene_AddComponent.cs`: 添加TpsBattlePanelComponent

---

## Task 8: 测试验证

**验证步骤:**
1. 进入战斗，UI显示弹药和血条
2. 射击消耗弹药，UI更新
3. 受击扣血，血条变化
4. 击杀敌人 → 显示"胜利!"
5. 血量归零 → 显示"失败!"
