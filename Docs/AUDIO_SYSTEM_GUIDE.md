# 三消游戏音效管理系统使用指南

## 概述

基于 ET 框架的 `cn.etetet.audio` 包，通过 `Match3AudioHelper` 静态辅助类为三消游戏提供简洁的音效管理。

## 依赖

- `cn.etetet.core`: ET框架核心
- `cn.etetet.audio`: ET框架音频模块

## 架构设计

### 音效类型定义（ModelView层）
- `Match3SoundType.cs` - 音效常量定义（40+ 种音效类型）

### 音效辅助类（HotfixView层）
- `Match3AudioHelper.cs` - 静态辅助类，提供所有音效播放方法

## 快速开始

### 1. 播放背景音乐

```csharp
using ET.Client;

Scene scene = Game.Scene;

// 播放主菜单音乐
await Match3AudioHelper.PlayMainMenuMusic(scene);

// 播放游戏音乐
await Match3AudioHelper.PlayGameMusic(scene);

// 播放胜利音乐
await Match3AudioHelper.PlayVictoryMusic(scene);

// 播放失败音乐
await Match3AudioHelper.PlayDefeatMusic(scene);
```

### 2. 播放游戏音效

```csharp
Scene scene = Game.Scene;

// 瓦片交换
Match3AudioHelper.PlayTileSwap(scene);

// 瓦片交换失败
Match3AudioHelper.PlayTileSwapFailed(scene);

// 匹配音效（根据匹配数量）
Match3AudioHelper.PlayMatchSound(scene, 3);  // 3消
Match3AudioHelper.PlayMatchSound(scene, 4);  // 4消
Match3AudioHelper.PlayMatchSound(scene, 5);  // 5消

// Combo音效
Match3AudioHelper.PlayComboSound(scene, 1);  // Combo x1
Match3AudioHelper.PlayComboSound(scene, 2);  // Combo x2
Match3AudioHelper.PlayComboSound(scene, 5);  // Combo x5+
```

### 3. 播放UI音效

```csharp
// 按钮点击
Match3AudioHelper.PlayButtonClick(scene);

// 面板打开
Match3AudioHelper.PlayPanelOpen(scene);

// 面板关闭
Match3AudioHelper.PlayPanelClose(scene);
```

### 4. 播放特殊糖果音效

```csharp
// 特殊糖果创建
Match3AudioHelper.PlaySpecialCandyCreate(scene);

// 条纹糖果激活
Match3AudioHelper.PlayStripedCandySound(scene);

// 包装糖果激活
Match3AudioHelper.PlayWrappedCandySound(scene);

// 彩色炸弹激活
Match3AudioHelper.PlayColorBombSound(scene);
```

### 5. 播放道具音效

```csharp
// 棒棒糖道具
Match3AudioHelper.PlayBoosterLollipopSound(scene);

// 炸弹道具
Match3AudioHelper.PlayBoosterBombSound(scene);

// 交换道具
Match3AudioHelper.PlayBoosterSwitchSound(scene);

// 彩色炸弹道具
Match3AudioHelper.PlayBoosterColorBombSound(scene);
```

## 集成到游戏逻辑

### 在匹配检测中播放音效

```csharp
[FriendOf(typeof(Match3BoardComponent))]
public static partial class Match3BoardComponentMatchSystem
{
    public static async ETTask ProcessMatchesAsync(this Match3BoardComponent self, List<Match> matches)
    {
        // 获取Scene
        Scene scene = self.Root() as Scene;
        
        // 播放匹配音效
        if (matches.Count > 0)
        {
            int totalTiles = matches.Sum(m => m.tiles.Count);
            Match3AudioHelper.PlayMatchSound(scene, totalTiles);
        }
        
        // 处理匹配逻辑...
        foreach (var match in matches)
        {
            // ...
        }
    }
}
```

### 在Combo系统中播放音效

```csharp
public static void OnComboTriggered(this Match3BoardComponent self, int comboCount)
{
    // 播放Combo音效
    Scene scene = self.Root() as Scene;
    Match3AudioHelper.PlayComboSound(scene, comboCount);
    
    // 其他Combo逻辑...
}
```

### 在输入系统中播放音效

```csharp
public static async ETTask TrySwapTilesAsync(this Match3BoardComponent self, int x1, int y1, int x2, int y2)
{
    // 获取Scene
    Scene scene = self.Root() as Scene;
    
    // 播放交换音效
    Match3AudioHelper.PlayTileSwap(scene);
    
    // 执行交换逻辑
    var matches = // ... 检测匹配
    
    if (matches.Count > 0)
    {
        // 有匹配，继续处理
        await ProcessMatchesAsync(self, matches);
    }
    else
    {
        // 无匹配，播放失败音效
        Match3AudioHelper.PlayTileSwapFailed(scene);
        
        // 交换回来
        // ...
    }
}
```

### 在道具系统中使用（已自动集成）

道具系统的音效已经自动集成到 `BoosterViewComponentSystem` 中：

```csharp
// BoosterViewComponentSystem.cs 中的 PlayBoosterSound 方法
// 会自动调用 Match3AudioHelper 的对应方法
```

## 音乐控制

### 暂停/恢复

```csharp
// 暂停游戏时
Match3AudioHelper.PauseMusic(scene);
Match3AudioHelper.StopAllSounds(scene);

// 恢复游戏时
Match3AudioHelper.ResumeMusic(scene);

// 停止音乐（带淡出）
await Match3AudioHelper.StopMusic(scene);
```

## 音量控制

```csharp
// 设置音乐音量 (0.0 - 1.0)
Match3AudioHelper.SetMusicVolume(scene, 0.7f);

// 设置音效音量 (0.0 - 1.0)
Match3AudioHelper.SetSoundVolume(scene, 0.8f);

// 在设置面板中使用滑动条
public static void OnMusicVolumeChanged(Scene scene, float value)
{
    Match3AudioHelper.SetMusicVolume(scene, value);
}

public static void OnSoundVolumeChanged(Scene scene, float value)
{
    Match3AudioHelper.SetSoundVolume(scene, value);
}
```

## 完整使用示例

### 游戏场景初始化

```csharp
namespace ET.Client
{
    public static class Match3GameSceneHelper
    {
        /// <summary>
        /// 初始化游戏场景
        /// </summary>
        public static async ETTask InitGameScene(Scene scene)
        {
            // 播放游戏音乐
            await Match3AudioHelper.PlayGameMusic(scene);
            
            // 播放关卡开始音效
            Match3AudioHelper.PlayLevelStartSound(scene);
            
            // 初始化其他组件...
        }
        
        /// <summary>
        /// 关卡完成
        /// </summary>
        public static async ETTask OnLevelComplete(Scene scene, int stars)
        {
            // 播放完成音效
            Match3AudioHelper.PlayLevelCompleteSound(scene);
            
            // 根据星级数播放星星音效
            for (int i = 0; i < stars; i++)
            {
                await scene.GetComponent<TimerComponent>().WaitAsync(500); // 每颗星间隔0.5秒
                Match3AudioHelper.PlayStarEarnedSound(scene);
            }
            
            // 切换到胜利音乐
            await Match3AudioHelper.PlayVictoryMusic(scene);
        }
        
        /// <summary>
        /// 关卡失败
        /// </summary>
        public static async ETTask OnLevelFailed(Scene scene)
        {
            // 播放失败音效
            Match3AudioHelper.PlayLevelFailedSound(scene);
            
            // 切换到失败音乐
            await Match3AudioHelper.PlayDefeatMusic(scene);
        }
    }
}
```

### UI面板中使用

```csharp
namespace ET.Client
{
    [ComponentOf(typeof(YIUIChild))]
    public class GameUIComponent : Entity, IAwake, IDestroy,
        IYIUIBind,
        IYIUIInitialize
    {
    }
    
    [FriendOf(typeof(GameUIComponent))]
    [EntitySystemOf(typeof(GameUIComponent))]
    public static partial class GameUIComponentSystem
    {
        [EntitySystem]
        private static void YIUIBind(this GameUIComponent self)
        {
            // 绑定按钮点击事件
            // self.u_btn_pause.SetUIEventClick(self.OnPauseButtonClick);
        }
        
        private static void OnPauseButtonClick(this GameUIComponent self)
        {
            // 播放按钮点击音效
            Scene scene = self.Root() as Scene;
            Match3AudioHelper.PlayButtonClick(scene);
            
            // 暂停游戏
            Match3AudioHelper.PauseMusic(scene);
            
            // 显示暂停面板...
        }
        
        private static void OnResumeButtonClick(this GameUIComponent self)
        {
            Scene scene = self.Root() as Scene;
            Match3AudioHelper.PlayButtonClick(scene);
            
            // 恢复音乐
            Match3AudioHelper.ResumeMusic(scene);
        }
    }
}
```

### 特殊糖果激活

```csharp
public static async ETTask ActivateStripedCandy(this Match3BoardComponent self, int x, int y, bool isHorizontal)
{
    Scene scene = self.Root() as Scene;
    
    // 播放条纹糖果音效
    Match3AudioHelper.PlayStripedCandySound(scene);
    
    // 执行条纹糖果效果...
}

public static async ETTask ActivateWrappedCandy(this Match3BoardComponent self, int x, int y)
{
    Scene scene = self.Root() as Scene;
    
    // 播放包装糖果音效
    Match3AudioHelper.PlayWrappedCandySound(scene);
    
    // 执行包装糖果效果...
}

public static async ETTask ActivateColorBomb(this Match3BoardComponent self, int x, int y, CandyColor targetColor)
{
    Scene scene = self.Root() as Scene;
    
    // 播放彩色炸弹音效
    Match3AudioHelper.PlayColorBombSound(scene);
    
    // 执行彩色炸弹效果...
}
```

### 障碍物破坏

```csharp
public static void OnChocolateDestroyed(this Match3BoardComponent self)
{
    Scene scene = self.Root() as Scene;
    Match3AudioHelper.PlayChocolateBreakSound(scene);
}

public static void OnMarshmallowDestroyed(this Match3BoardComponent self)
{
    Scene scene = self.Root() as Scene;
    Match3AudioHelper.PlayMarshmallowBreakSound(scene);
}

public static void OnIceDestroyed(this Match3BoardComponent self)
{
    Scene scene = self.Root() as Scene;
    Match3AudioHelper.PlayIceBreakSound(scene);
}
```

## API参考

### 背景音乐
- `PlayMainMenuMusic(Scene)` - 播放主菜单音乐
- `PlayGameMusic(Scene)` - 播放游戏音乐
- `PlayVictoryMusic(Scene)` - 播放胜利音乐
- `PlayDefeatMusic(Scene)` - 播放失败音乐
- `StopMusic(Scene)` - 停止音乐（带淡出）
- `PauseMusic(Scene)` - 暂停音乐
- `ResumeMusic(Scene)` - 恢复音乐

### UI音效
- `PlayButtonClick(Scene)` - 按钮点击
- `PlayPanelOpen(Scene)` - 面板打开
- `PlayPanelClose(Scene)` - 面板关闭

### 游戏音效
- `PlayTileSwap(Scene)` - 瓦片交换
- `PlayTileSwapFailed(Scene)` - 瓦片交换失败
- `PlayMatchSound(Scene, int)` - 匹配音效（根据数量）
- `PlayComboSound(Scene, int)` - Combo音效（根据数量）

### 特殊糖果音效
- `PlaySpecialCandyCreate(Scene)` - 特殊糖果创建
- `PlayStripedCandySound(Scene)` - 条纹糖果激活
- `PlayWrappedCandySound(Scene)` - 包装糖果激活
- `PlayColorBombSound(Scene)` - 彩色炸弹激活

### 道具音效
- `PlayBoosterLollipopSound(Scene)` - 棒棒糖道具
- `PlayBoosterBombSound(Scene)` - 炸弹道具
- `PlayBoosterSwitchSound(Scene)` - 交换道具
- `PlayBoosterColorBombSound(Scene)` - 彩色炸弹道具

### 障碍物音效
- `PlayChocolateBreakSound(Scene)` - 巧克力破碎
- `PlayMarshmallowBreakSound(Scene)` - 棉花糖破碎
- `PlayIceBreakSound(Scene)` - 冰块破碎

### 收集物音效
- `PlayCollectableCollectSound(Scene)` - 收集物收集

### 游戏事件音效
- `PlayLevelStartSound(Scene)` - 关卡开始
- `PlayLevelCompleteSound(Scene)` - 关卡完成
- `PlayLevelFailedSound(Scene)` - 关卡失败
- `PlayStarEarnedSound(Scene)` - 获得星星
- `PlayNoMovesLeftSound(Scene)` - 无可用移动
- `PlayNewHighScoreSound(Scene)` - 新高分

### 音量控制
- `SetMusicVolume(Scene, float)` - 设置音乐音量
- `SetSoundVolume(Scene, float)` - 设置音效音量
- `StopAllSounds(Scene)` - 停止所有音效

## 音效资源命名规范

所有音效资源地址定义在 `Match3SoundType.cs` 中：

### 背景音乐（BGM）
```
Audio_BGM_Match3_MainMenu
Audio_BGM_Match3_Game
Audio_BGM_Match3_Victory
Audio_BGM_Match3_Defeat
```

### 游戏音效（SFX）
```
Audio_SFX_Match3_TileSwap
Audio_SFX_Match3_Match3
Audio_SFX_Match3_Match4
Audio_SFX_Match3_Match5
Audio_SFX_Match3_Combo1
...
```

## 优势

### 1. 简洁易用
```csharp
// 只需一行代码，无需获取组件
Match3AudioHelper.PlayMatchSound(scene, 3);
```

### 2. 自动管理SoundComponent
- 首次调用时自动创建 `SoundComponent`
- 自动设置默认音量
- 无需手动管理生命周期

### 3. 类型安全
- 所有音效地址都是常量
- 编译时检查，避免拼写错误

### 4. 高性能
- 音效自动使用 `.NoContext()` 避免阻塞
- 基于 ET 框架的 audio 包，性能优化

## 注意事项

1. **Scene参数**：所有方法都需要传入 Scene 参数
2. **资源地址**：确保音效资源地址与YooAsset配置一致
3. **Global节点**：需要场景中存在 `/Global` 节点
4. **异步音乐**：背景音乐播放方法是异步的，需要 await
5. **音效播放**：音效方法都是同步的，内部使用 NoContext()

## 性能优化

1. **预加载常用音效**
```csharp
public static async ETTask PreloadCommonSounds(Scene scene)
{
    SoundComponent soundComp = scene.GetComponent<SoundComponent>();
    if (soundComp == null)
    {
        soundComp = scene.AddComponent<SoundComponent>();
    }
    
    // 预加载常用音效
    await soundComp.GetAudioClip(Match3SoundType.SFX_Match3);
    await soundComp.GetAudioClip(Match3SoundType.SFX_ButtonClick);
    // ...
}
```

2. **控制音效数量**
```csharp
SoundComponent soundComp = scene.GetComponent<SoundComponent>();
soundComp?.SetMaxPoolSize(15);  // 限制最多15个同时播放的音效
```

## 总结

`Match3AudioHelper` 提供了：

✅ 简洁的静态方法API
✅ 40+ 种音效类型
✅ 自动管理 SoundComponent
✅ 类型安全的音效地址
✅ 高性能的音效播放
✅ 完整的文档和示例
✅ 符合ET框架规范

无需创建组件，直接调用即可使用！
