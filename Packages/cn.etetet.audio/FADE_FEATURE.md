# 淡入淡出功能说明

## 概述

为 ET.Audio 音频管理包添加了专业的背景音乐淡入淡出切换功能，让音乐切换更加平滑自然。

## 新增功能

### 1. PlayMusicWithFade - 带淡入淡出的音乐切换

```csharp
/// <summary>
/// 带淡入淡出效果切换背景音乐
/// </summary>
/// <param name="address">音频资源地址</param>
/// <param name="fadeOutDuration">淡出时长（秒）默认1.0秒</param>
/// <param name="fadeInDuration">淡入时长（秒）默认1.0秒</param>
/// <param name="loop">是否循环播放，默认true</param>
public static async ETTask PlayMusicWithFade(
    this SoundComponent self, 
    string address, 
    float fadeOutDuration = 1.0f, 
    float fadeInDuration = 1.0f, 
    bool loop = true)
```

**使用示例：**
```csharp
// 标准切换（1秒淡出 + 1秒淡入）
await soundComp.PlayMusicWithFade("Audio_BGM_Battle");

// 自定义淡入淡出时长
await soundComp.PlayMusicWithFade("Audio_BGM_Boss", 
    fadeOutDuration: 0.5f,   // 快速淡出
    fadeInDuration: 2.0f);   // 慢速淡入

// 不循环播放的音乐（如胜利音乐）
await soundComp.PlayMusicWithFade("Audio_BGM_Victory", 
    fadeOutDuration: 1.0f, 
    fadeInDuration: 3.0f, 
    loop: false);
```

### 2. StopMusicWithFade - 淡出停止音乐

```csharp
/// <summary>
/// 淡出并停止背景音乐
/// </summary>
/// <param name="fadeOutDuration">淡出时长（秒）默认1.0秒</param>
public static async ETTask StopMusicWithFade(
    this SoundComponent self, 
    float fadeOutDuration = 1.0f)
```

**使用示例：**
```csharp
// 标准淡出停止
await soundComp.StopMusicWithFade();

// 慢速淡出（营造结束氛围）
await soundComp.StopMusicWithFade(fadeOutDuration: 2.0f);
```

### 3. AudioHelper 快捷方法

```csharp
// 快速切换（不等待完成）
AudioHelper.PlayMusicWithFadeQuick(scene, "Audio_BGM_Battle");

// 快速淡出停止（不等待完成）
AudioHelper.StopMusicWithFadeQuick(scene, fadeOutDuration: 1.5f);
```

## 技术特性

### 智能切换
- ✅ 自动检测是否播放相同音乐，避免不必要的切换
- ✅ 记录当前播放的音乐地址 `CurrentMusicAddress`
- ✅ 淡入淡出过程可以被新的切换请求中断

### 取消机制
- ✅ 使用 `ETCancellationToken` 管理异步任务
- ✅ 新的音乐切换会自动取消之前的淡入淡出
- ✅ 组件销毁时自动取消所有淡入淡出任务

### 平滑过渡
- ✅ 使用 `WaitFrameAsync()` 实现逐帧音量调整
- ✅ 使用 `Mathf.Lerp()` 实现线性插值
- ✅ 支持自定义淡入淡出时长

### EntityRef安全
- ✅ 严格遵循ET框架的 EntityRef 规范
- ✅ await前创建EntityRef，await后重新获取Entity
- ✅ 多次await的复杂逻辑正确处理

## 使用场景

### 场景1: 游戏场景切换
```csharp
// 从主菜单进入战斗
await soundComp.PlayMusicWithFade("Audio_BGM_Battle", 1.5f, 2.0f);

// 战斗结束，播放胜利音乐
await soundComp.PlayMusicWithFade("Audio_BGM_Victory", 0.5f, 3.0f, false);
```

### 场景2: Boss战音乐
```csharp
// Boss出现，快速切换紧张音乐
await soundComp.PlayMusicWithFade("Audio_BGM_Boss", 0.3f, 0.5f);
```

### 场景3: 游戏退出
```csharp
// 退出游戏，慢速淡出
await soundComp.StopMusicWithFade(fadeOutDuration: 2.0f);
```

### 场景4: 动态音乐系统
```csharp
public static async ETTask UpdateMusicByGameState(Scene scene, GameState state)
{
    SoundComponent soundComp = scene.GetComponent<SoundComponent>();
    
    string musicAddress = state switch
    {
        GameState.Exploration => "Audio_BGM_Exploration",
        GameState.Combat => "Audio_BGM_Combat",
        GameState.Puzzle => "Audio_BGM_Puzzle",
        _ => null
    };
    
    if (!string.IsNullOrEmpty(musicAddress))
    {
        // 自动淡入淡出切换
        await soundComp.PlayMusicWithFade(musicAddress);
    }
}
```

## 实现细节

### 淡出流程
1. 记录当前音量
2. 逐帧降低音量（Lerp插值）
3. 音量降至0后停止播放
4. 标记淡入淡出结束

### 淡入流程
1. 加载新的音频资源
2. 设置音量为0并开始播放
3. 逐帧提升音量（Lerp插值）
4. 音量升至目标音量
5. 标记淡入淡出结束

### 中断处理
1. 新的音乐切换请求到来
2. 取消之前的 `FadeCancellationToken`
3. 创建新的取消令牌
4. 旧的淡入淡出任务检测到取消后立即退出

## 性能考虑

- ✅ 使用 `WaitFrameAsync()` 而非 `Update()`，避免额外开销
- ✅ 淡入淡出期间不创建额外对象（零GC）
- ✅ 及时取消不需要的任务，避免资源浪费
- ✅ EntityRef确保await后安全访问Entity

## 新增Entity字段

在 `SoundComponent` 中新增：

```csharp
/// <summary>
/// 是否正在淡入淡出
/// </summary>
public bool IsFading;

/// <summary>
/// 淡入淡出的取消令牌
/// </summary>
public ETCancellationToken FadeCancellationToken;

/// <summary>
/// 当前正在播放的音乐地址
/// </summary>
public string CurrentMusicAddress;
```

## 新增System方法

在 `SoundComponentSystem` 中新增：

```csharp
// 公共方法
public static async ETTask PlayMusicWithFade(...)
public static async ETTask StopMusicWithFade(...)

// 私有方法
private static async ETTask FadeOutMusic(...)
private static async ETTask FadeInMusic(...)
```

## 向后兼容

- ✅ 保留原有 `PlayMusic()` 方法（立即切换）
- ✅ 保留原有 `StopMusic()` 方法（立即停止）
- ✅ 新增方法不影响现有代码
- ✅ 完全向下兼容

## 建议用法

**推荐使用淡入淡出：**
- ✅ 场景切换
- ✅ 游戏状态改变
- ✅ 氛围音乐切换
- ✅ 结束/退出游戏

**可以使用立即切换：**
- ✅ 游戏初始化时播放音乐
- ✅ 某些需要立即反馈的情况
- ✅ 快节奏战斗游戏（如需要）

## 总结

淡入淡出功能为ET.Audio包带来了专业级的音乐切换体验，让游戏音频更加流畅自然。实现上严格遵循ET框架规范，性能优秀，使用简单，是游戏音频系统的重要升级。


