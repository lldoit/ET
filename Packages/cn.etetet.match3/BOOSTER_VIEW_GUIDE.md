# 道具视觉表现系统使用指南

## 概述

本文档说明如何使用道具系统的视觉表现功能。

## 架构说明

道具系统分为三层：

1. **Model 层**：`BoosterManagerComponent` - 道具数据
2. **Hotfix 层**：`BoosterManagerComponentSystem` - 道具逻辑
3. **ModelView 层**：`BoosterViewComponent` - 道具视图数据
4. **HotfixView 层**：`BoosterViewComponentSystem` + `BoosterManagerViewSystem` - 视觉表现逻辑

## 使用方法

### 1. 初始化道具视图组件

```csharp
// 创建道具管理器
var boosterManager = scene.AddComponent<BoosterManagerComponent>();

// 添加视图组件
var boosterView = boosterManager.AddComponent<BoosterViewComponent>();

// 加载并设置特效预制体（从资源系统加载）
boosterView.LollipopEffectPrefab = await YooAssets.LoadAssetAsync<GameObject>("BoosterLollipopEffect");
boosterView.BombEffectPrefab = await YooAssets.LoadAssetAsync<GameObject>("BoosterBombEffect");
boosterView.ColorBombEffectPrefab = await YooAssets.LoadAssetAsync<GameObject>("BoosterColorBombEffect");
boosterView.SwitchEffectPrefab = await YooAssets.LoadAssetAsync<GameObject>("BoosterSwitchEffect");

// 配置音效名称
boosterView.LollipopSound = "BoosterLollipop";
boosterView.BombSound = "BoosterBomb";
boosterView.ColorBombSound = "BoosterColorBomb";
boosterView.SwitchSound = "BoosterSwitch";

// 配置动画时长
boosterView.LollipopAnimDuration = 300;  // 毫秒
boosterView.BombAnimDuration = 500;
boosterView.ColorBombAnimDuration = 600;
boosterView.SwitchAnimDuration = 250;
```

### 2. 使用带视觉效果的道具

#### 方式一：使用扩展方法（推荐）

```csharp
using ET.Client;  // 引入 HotfixView 命名空间

// 激活道具（带视觉提示）
if (boosterManager.ActivateBoosterWithView(BoosterType.Lollipop))
{
    // 道具已激活，等待玩家点击瓦片
}

// 玩家点击瓦片后，应用道具（自动播放特效）
await boosterManager.ApplyBoosterWithViewAsync(board, targetX, targetY);
```

#### 方式二：手动控制

```csharp
// 激活道具
boosterManager.ActivateBooster(BoosterType.Bomb);

// 获取视图组件
var boosterView = boosterManager.GetComponent<BoosterViewComponent>();

// 显示激活提示
boosterView.ShowBoosterActivatedHint(BoosterType.Bomb);

// 玩家点击瓦片后
var tile = board.GetTile(x, y);
Vector3 worldPos = tile.GetComponent<TileView>().GameObject.transform.position;

// 播放特效
await boosterView.PlayBombEffectAsync(worldPos);

// 执行道具逻辑
await boosterManager.ExecuteBombAsync(board, tile);

// 隐藏提示
boosterView.HideBoosterActivatedHint();
```

### 3. Switch 道具的特殊处理

```csharp
using ET.Client;

// 激活 Switch 道具
if (boosterManager.ActivateBoosterWithView(BoosterType.Switch))
{
    // 道具已激活，等待玩家选择两个瓦片
}

// 玩家点击第一个瓦片
await boosterManager.HandleSwitchInputWithViewAsync(board, x1, y1);
// 此时会高亮第一个瓦片

// 玩家点击第二个瓦片
await boosterManager.HandleSwitchInputWithViewAsync(board, x2, y2);
// 自动播放交换特效并执行交换
```

## 特效预制体要求

### Lollipop 特效
- 单点爆炸特效
- 持续时间：约 300ms
- 建议：粒子爆发效果

### Bomb 特效
- 3x3 范围爆炸特效
- 持续时间：约 500ms
- 建议：扩散波纹 + 粒子系统

### ColorBomb 特效
- 彩色炸弹生成特效
- 持续时间：约 600ms
- 建议：彩色光芒 + 闪光效果

### Switch 特效
- 两点连线特效
- 持续时间：约 250ms
- 建议：使用 LineRenderer 组件
- 预制体应包含 LineRenderer，系统会自动设置起止点

## 音效集成

当前音效通过 Log 输出，需要集成实际的音效系统：

```csharp
// 在 BoosterViewComponentSystem.PlayBoosterSound 中修改
private static void PlayBoosterSound(this BoosterViewComponent self, string soundName)
{
    // 集成你的音效系统
    // 示例1：ET框架的音效组件
    self.Root().GetComponent<SoundComponent>()?.PlaySound(soundName);
    
    // 示例2：第三方音效管理器
    // SoundManager.Instance.PlaySound(soundName);
    
    // 示例3：YooAssets音效
    // var audioClip = await YooAssets.LoadAssetAsync<AudioClip>(soundName);
    // AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);
}
```

## UI 提示集成

### 激活提示

```csharp
public static void ShowBoosterActivatedHint(this BoosterViewComponent self, BoosterType boosterType)
{
    // 方式1：改变光标
    Cursor.SetCursor(boosterCursorTexture, Vector2.zero, CursorMode.Auto);
    
    // 方式2：高亮道具按钮
    var button = GetBoosterButton(boosterType);
    button.GetComponent<Image>().color = Color.yellow;
    
    // 方式3：显示提示文本
    var tipText = GameObject.Find("BoosterTipText").GetComponent<Text>();
    tipText.text = $"使用 {boosterType} 道具，请选择目标";
    tipText.gameObject.SetActive(true);
}

public static void HideBoosterActivatedHint(this BoosterViewComponent self)
{
    // 恢复光标
    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    
    // 取消按钮高亮
    // ...
    
    // 隐藏提示文本
    GameObject.Find("BoosterTipText")?.SetActive(false);
}
```

## 高级功能

### 瓦片高亮

```csharp
public static void HighlightTargetTiles(this BoosterViewComponent self, List<(int x, int y)> positions)
{
    foreach (var pos in positions)
    {
        var tile = board.GetTile(pos.x, pos.y);
        var tileView = tile.GetComponent<TileView>();
        if (tileView != null)
        {
            // 添加高亮效果
            var highlight = UnityEngine.Object.Instantiate(highlightPrefab, tileView.GameObject.transform);
            highlight.name = "Highlight";
            
            // 可以使用动画或闪烁效果
            var animator = highlight.GetComponent<Animator>();
            animator?.SetTrigger("Highlight");
        }
    }
}

public static void ClearHighlights(this BoosterViewComponent self)
{
    // 移除所有高亮
    var highlights = GameObject.FindGameObjectsWithTag("TileHighlight");
    foreach (var h in highlights)
    {
        UnityEngine.Object.Destroy(h);
    }
}
```

## 完整使用示例

```csharp
namespace ET.Client
{
    public class BoosterUIController
    {
        private BoosterManagerComponent boosterManager;
        private Match3BoardComponent board;

        public async ETTask OnBoosterButtonClicked(BoosterType boosterType)
        {
            // 检查道具数量
            if (boosterManager.GetBoosterCount(boosterType) <= 0)
            {
                ShowTip("道具不足！");
                return;
            }

            // 激活道具（带视觉提示）
            if (boosterManager.ActivateBoosterWithView(boosterType))
            {
                // 锁定输入，等待玩家选择目标
                board.InputLocked = true;
            }
        }

        public async ETTask OnTileClicked(int x, int y)
        {
            var boosterView = boosterManager.GetComponent<BoosterViewComponent>();
            
            // 检查是否有激活的道具
            if (boosterManager.ActiveBoosterType.HasValue)
            {
                if (boosterManager.InSwitchMode)
                {
                    // Switch 道具的特殊处理
                    await boosterManager.HandleSwitchInputWithViewAsync(board, x, y);
                }
                else
                {
                    // 其他道具
                    await boosterManager.ApplyBoosterWithViewAsync(board, x, y);
                }
                
                // 解锁输入
                board.InputLocked = false;
                return;
            }

            // 正常的游戏逻辑
            await board.TrySwapTilesAsync(selectedX, selectedY, x, y);
        }

        private void ShowTip(string message)
        {
            // 显示UI提示
        }
    }
}
```

## 注意事项

1. **资源管理**：特效预制体需要通过资源系统加载，避免硬编码路径
2. **性能优化**：考虑使用对象池管理特效对象
3. **错误处理**：如果没有 `BoosterViewComponent`，系统会自动回退到无视觉效果的逻辑
4. **坐标转换**：确保正确获取瓦片的世界坐标
5. **音效集成**：需要手动集成实际的音效系统
6. **UI 集成**：高亮、提示等UI效果需要根据实际UI框架实现

## 调试技巧

```csharp
// 开启详细日志
#if UNITY_EDITOR
Log.Debug($"播放道具特效: {boosterType} at {worldPos}");
#endif

// 在 Scene 视图中显示调试信息
#if UNITY_EDITOR
private void OnDrawGizmos()
{
    if (boosterView != null && boosterView.ActiveBoosterType.HasValue)
    {
        Gizmos.color = Color.yellow;
        // 绘制激活状态的可视化信息
    }
}
#endif
```

