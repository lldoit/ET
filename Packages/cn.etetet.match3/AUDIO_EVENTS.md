# Match3 音效事件系统

## 概述

由于 ET 框架的程序集分层架构，`Hotfix` 程序集无法直接访问 `HotfixView` 程序集中的 `Match3AudioHelper`。因此我们通过事件系统实现解耦。

## 架构说明

```
┌──────────────────────┐
│   Hotfix 层          │
│ (游戏逻辑)           │
│                      │
│  发布音效事件 →      │
└──────────────────────┘
         ↓
┌──────────────────────┐
│   Model 层           │
│ (事件定义)           │
│                      │
│  AudioEvents.cs      │
└──────────────────────┘
         ↓
┌──────────────────────┐
│   HotfixView 层      │
│ (音效播放)           │
│                      │
│  AudioEventHandlers  │
│  → Match3AudioHelper │
└──────────────────────┘
```

## 文件结构

```
Scripts/
├── Model/Share/Match3/Audio/
│   └── AudioEvents.cs                    # 事件定义
├── Hotfix/Share/Match3/
│   ├── Match3BoardComponentMatchSystem.cs    # 发布事件
│   ├── Match3BoardComponentInputSystem.cs    # 发布事件
│   └── Boosters/BoosterManagerComponentSystem.cs # 发布事件
└── HotfixView/Client/Match3/Audio/
    ├── AudioEventHandlers.cs             # 事件处理器
    └── Match3AudioHelper.cs              # 音效播放实现
```

## 事件类型

### 1. PlaySoundEvent - 通用音效事件

```csharp
public struct PlaySoundEvent
{
    public string SoundType;
}
```

**支持的音效类型：**
- `TileSwap` - 瓦片交换
- `TileSwapFailed` - 交换失败
- `ChocolateBreak` - 巧克力破坏
- `MarshmallowBreak` - 棉花糖破坏
- `BoosterLollipop` - 棒棒糖道具
- `BoosterBomb` - 炸弹道具
- `BoosterColorBomb` - 彩色炸弹道具
- `BoosterSwitch` - 交换道具
- `SpecialCandyCreate` - 特殊糖果创建

### 2. PlayMatchSoundEvent - 匹配音效事件

```csharp
public struct PlayMatchSoundEvent
{
    public int MatchCount; // 匹配的瓦片数量
}
```

### 3. PlayComboSoundEvent - Combo音效事件

```csharp
public struct PlayComboSoundEvent
{
    public int ComboCount; // 连击数
}
```

## 使用方法

### 在 Hotfix 层发布事件

```csharp
// 1. 播放通用音效
Scene scene = self.Root() as Scene;
EventSystem.Instance.Publish(scene, new PlaySoundEvent 
{ 
    SoundType = "TileSwap" 
});

// 2. 播放匹配音效（带参数）
EventSystem.Instance.Publish(scene, new PlayMatchSoundEvent 
{ 
    MatchCount = 5 
});

// 3. 播放Combo音效
EventSystem.Instance.Publish(scene, new PlayComboSoundEvent 
{ 
    ComboCount = 3 
});
```

### 在 HotfixView 层处理事件

事件处理器会自动调用 `Match3AudioHelper` 中的相应方法：

```csharp
[Event(SceneType.All)]
public class PlaySoundEventHandler : AEvent<Scene, PlaySoundEvent>
{
    protected override async ETTask Run(Scene scene, PlaySoundEvent args)
    {
        switch (args.SoundType)
        {
            case "TileSwap":
                Match3AudioHelper.PlayTileSwap(scene);
                break;
            // ... 其他音效类型
        }
        await ETTask.CompletedTask;
    }
}
```

## 音效触发位置

| 位置 | 事件类型 | 触发时机 |
|------|---------|---------|
| **Match3BoardComponentMatchSystem** | | |
| `ProcessMatchesAsync` | PlayMatchSoundEvent | 检测到匹配时 |
| `ExplodeTileAsync` | PlaySoundEvent | 瓦片爆炸时（巧克力/棉花糖） |
| **Match3BoardComponentInputSystem** | | |
| `SwapTilesWithAnimationAsync` | PlaySoundEvent | 瓦片开始交换 |
| `TrySwapTilesAsync` | PlaySoundEvent | 交换失败 |
| **BoosterManagerComponentSystem** | | |
| `ExecuteLollipopAsync` | PlaySoundEvent | 棒棒糖道具使用 |
| `ExecuteBombAsync` | PlaySoundEvent | 炸弹道具使用 |
| `ExecuteColorBombAsync` | PlaySoundEvent | 彩色炸弹道具使用 + 生成 |
| `ExecuteSwitchAsync` | PlaySoundEvent | 交换道具使用 |

## 优点

1. **解耦架构**：Hotfix 层不依赖 HotfixView 层
2. **易于扩展**：添加新音效只需添加事件类型和处理器
3. **灵活性**：可以在不修改游戏逻辑的情况下更换音效实现
4. **符合 ET 框架规范**：遵循程序集分层原则

## 添加新音效

### 步骤 1：在 AudioEvents.cs 中定义事件（如果需要）

```csharp
public struct PlayNewSoundEvent
{
    public string Param1;
    public int Param2;
}
```

### 步骤 2：在 AudioEventHandlers.cs 中添加处理器

```csharp
[Event(SceneType.All)]
public class PlayNewSoundEventHandler : AEvent<Scene, PlayNewSoundEvent>
{
    protected override async ETTask Run(Scene scene, PlayNewSoundEvent args)
    {
        Match3AudioHelper.PlayNewSound(scene, args.Param1, args.Param2);
        await ETTask.CompletedTask;
    }
}
```

### 步骤 3：在 Match3AudioHelper.cs 中实现播放方法

```csharp
public static void PlayNewSound(Scene scene, string param1, int param2)
{
    if (!_isSoundEnabled) return;
    GetSoundComponent(scene)?.PlaySound($"Audio_SFX_{param1}_{param2}").NoContext();
}
```

### 步骤 4：在 Hotfix 层发布事件

```csharp
EventSystem.Instance.Publish(scene, new PlayNewSoundEvent 
{ 
    Param1 = "value",
    Param2 = 123
});
```

## 注意事项

1. **事件是异步的**：音效播放不会阻塞游戏逻辑
2. **使用 EventSystem.Instance**：在 Hotfix 层使用 `EventSystem.Instance.Publish()` 发布事件
3. **Scene 传递**：确保传递正确的 Scene 实例
4. **SceneType.All**：音效事件处理器使用 `SceneType.All` 以支持所有场景类型

## 性能考虑

- 事件发布是轻量级操作
- 音效播放在 HotfixView 层异步执行
- 不会影响游戏逻辑性能

