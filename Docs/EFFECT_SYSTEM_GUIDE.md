# 三消爆炸特效系统使用指南

## 概述

本系统实现了 CandyMatch3Kit 中的糖果消除爆炸效果，支持所有类型的瓦片特效播放，以及生成特效和表扬特效。

## 系统架构

```
┌─────────────────────────────────────────────────────────┐
│              Match3BoardComponent                        │
│                    │                                     │
│         ┌──────────┴──────────┐                         │
│         ▼                      ▼                        │
│  FxPoolComponent      Match3BoardViewHelper             │
│  (特效池管理)          (特效播放辅助)                    │
│         │                      │                        │
│         └──────────┬───────────┘                        │
│                   ▼                                    │
│            EffectAutoReturn                            │
│            (自动回收组件)                               │
└─────────────────────────────────────────────────────────┘
```

## 文件结构

```
Scripts/
├── ModelView/Client/Match3/Common/
│   ├── FxPoolComponent.cs              # 特效池组件（数据）
│   ├── ComplimentType.cs               # 表扬类型枚举
│   └── ShowComplimentEvent.cs          # 表扬事件
├── HotfixView/Client/Match3/Common/
│   ├── FxPoolComponentSystem.cs        # 特效池系统（逻辑）
│   ├── Match3BoardViewHelper.cs        # 特效播放辅助类
│   └── EffectAutoReturn.cs             # 自动回收组件
```

## 特效类型

### 1. 普通糖果爆炸特效（6种颜色）
- `BlueCandyMatchParticles` - 蓝色糖果
- `GreenCandyMatchParticles` - 绿色糖果
- `OrangeCandyMatchParticles` - 橙色糖果
- `PurpleCandyMatchParticles` - 紫色糖果
- `RedCandyMatchParticles` - 红色糖果
- `YellowCandyMatchParticles` - 黄色糖果

### 2. 条纹糖果爆炸特效
- `HorizontalStripes` - 横向条纹
- `VerticalStripes` - 纵向条纹

### 3. 特殊糖果爆炸特效
- `WrappedCandyParticles` - 包装糖果爆炸
- `ColorBombParticles` - 彩色炸弹爆炸

### 4. 元素爆炸特效
- `HoneyParticles` - 蜂蜜
- `IceParticles` - 冰块
- `SyrupParticles` - 糖浆

### 5. 特殊方块爆炸特效
- `MarshmallowParticles` - 棉花糖
- `ChocolateParticles` - 巧克力

### 6. 其他特效
- `CollectablesParticles` - 收集物爆炸
- `Spawn` - 生成特效（创建特殊糖果时的闪光效果）

## 使用步骤

### 1. 拷贝特效 Prefab

将 CandyMatch3Kit 中的特效 prefab 拷贝到 match3 包的资源目录：

**源路径：**
```
Candy Match 3 Kit 3.0.0/Assets/CandyMatch3Kit/Prefabs/Particles/Game/
```

**目标路径：**
```
GameRes/Match3/Effect/
```

> 注意：Spawn.prefab 需要单独拷贝

### 2. 配置 YooAssets 资源包

确保所有特效 prefab 都已配置到 YooAssets 资源包中。

### 3. 初始化特效池

在创建 Match3BoardComponent 后，初始化 FxPoolComponent：

```csharp
// 在 HotfixView 层初始化
var fxPool = match3Board.AddComponent<FxPoolComponent>();
await fxPool.InitializeAsync();
```

### 4. 自动播放特效

特效会在以下情况自动播放：

- **瓦片消除时**：在 `ExplodeTileAsync` 方法中自动调用 `PlayTileExplosionEffect`
- **生成特殊糖果时**：自动调用 `PlaySpawnEffect`
- **条纹糖果激活时**：在整行/整列播放条纹特效
- **包装糖果激活时**：在爆炸区域中心播放包装特效
- **连续消除时**：根据连续次数显示 Good/Super/Yummy

## API 说明

### FxPoolComponentSystem

#### 播放特效方法

```csharp
// 播放普通糖果爆炸特效
fxPool.PlayCandyExplosion(CandyColor.Red, position);

// 播放条纹糖果爆炸特效
fxPool.PlayStripedCandyExplosion(StripeDirection.Horizontal, position);

// 播放包装糖果爆炸特效
fxPool.PlayWrappedCandyExplosion(position);

// 播放彩色炸弹爆炸特效
fxPool.PlayColorBombExplosion(position);

// 播放元素爆炸特效
fxPool.PlayElementExplosion(ElementType.Honey, position);

// 播放特殊方块爆炸特效
fxPool.PlaySpecialBlockExplosion(SpecialBlockType.Chocolate, position);

// 播放收集物爆炸特效
fxPool.PlayCollectableExplosion(position);

// 播放生成特效
fxPool.PlaySpawnParticles(position);
```

#### 表扬类型辅助方法

```csharp
// 根据连续消除次数获取表扬类型（2次=Good，4次=Super，6次=Yummy）
ComplimentType? type = FxPoolComponentSystem.GetComplimentType(cascadeCount);

// 判断是否应该显示表扬
bool shouldShow = FxPoolComponentSystem.ShouldShowCompliment(cascadeCount);
```

### Match3BoardViewHelper

#### 播放特效方法

```csharp
// 根据瓦片类型自动播放对应的特效
match3Board.PlayTileExplosionEffect(tile, worldPosition);

// 播放生成特效
match3Board.PlaySpawnEffect(worldPosition);

// 根据连续消除次数显示表扬（发布 ShowComplimentEvent）
match3Board.ShowComplimentIfNeeded(cascadeCount);

// 播放元素消除特效
match3Board.PlayElementDestroyEffect(ElementType.Honey, worldPosition);

// 播放条纹特效（用于Combo）
match3Board.PlayStripedExplosionAtPosition(StripeDirection.Horizontal, worldPosition);

// 播放包装特效（用于Combo）
match3Board.PlayWrappedExplosionAtPosition(worldPosition);
```

## 表扬系统

### 表扬类型

```csharp
public enum ComplimentType
{
    Good,   // 2次连续消除
    Super,  // 4次连续消除
    Yummy   // 6次连续消除
}
```

### 表扬事件

当达到连续消除条件时，会发布 `ShowComplimentEvent` 事件：

```csharp
// 订阅表扬事件（在UI层）
[Event(SceneType.Client)]
public class ShowComplimentEventHandler : AEvent<Scene, ShowComplimentEvent>
{
    protected override async ETTask Run(Scene scene, ShowComplimentEvent args)
    {
        // 显示 Good/Super/Yummy 文本
        // 建议使用 YIUI Tips 系统实现
        await ETTask.CompletedTask;
    }
}
```

## 对象池机制

系统使用对象池管理特效实例，避免频繁创建和销毁：

1. **获取特效**：优先从对象池获取，如果池中没有则创建新实例
2. **自动回收**：特效播放完成后（所有粒子系统结束或超时）自动回收到对象池
3. **智能回收**：检查所有子 ParticleSystem 是否都停止播放
4. **延迟回收**：默认 2 秒后强制回收，可通过 `EffectAutoReturn` 组件调整

### EffectAutoReturn 功能

- 缓存所有子 ParticleSystem
- 激活时自动播放所有粒子系统
- 检查所有子粒子系统是否播放完成
- 回收前自动停止并清理粒子系统

## 注意事项

1. **资源加载**：确保所有特效 prefab 都已正确配置到 YooAssets 资源包中
2. **初始化顺序**：在创建棋盘后立即初始化 FxPoolComponent
3. **坐标系统**：特效位置基于 TileView 的 GameObject 世界坐标
4. **性能优化**：对象池会自动管理特效实例，无需手动销毁
5. **表扬文本**：建议使用 YIUI Tips 系统实现，通过订阅 ShowComplimentEvent 事件

## 扩展说明

### 添加新特效

1. 在 `FxPoolComponent` 中添加新的 GameObject 字段
2. 在 `FxPoolComponentSystem.InitializeAsync` 中加载新特效
3. 添加对应的播放方法
4. 在 `Match3BoardViewHelper` 中添加辅助方法（如需要）

### 自定义回收时间

修改 `EffectAutoReturn` 组件的 `returnDelay` 参数：

```csharp
var autoReturn = effectObj.GetComponent<EffectAutoReturn>();
autoReturn.Initialize(fxPool, prefab, delay: 3.0f); // 3秒后回收
```

## 故障排查

### 特效不显示

1. 检查 YooAssets 资源包配置是否正确
2. 检查 FxPoolComponent 是否已初始化
3. 检查特效 prefab 路径是否正确
4. 查看 Unity Console 是否有加载错误

### 特效位置不正确

1. 检查 TileView 是否正确设置
2. 检查 GameObject 的 transform.position 是否正确
3. 确认世界坐标计算逻辑

### 内存泄漏

1. 检查对象池是否正确回收特效
2. 检查 EffectAutoReturn 组件是否正常工作
3. 查看对象池字典中的对象数量

### 表扬不显示

1. 确认已订阅 `ShowComplimentEvent` 事件
2. 检查 `ConsecutiveCascades` 计数是否正确
3. 确认 UI 层正确处理了事件
