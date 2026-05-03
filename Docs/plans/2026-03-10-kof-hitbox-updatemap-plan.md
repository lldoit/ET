# KOF HitBox 与 UpdateMap Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 在 ET 9.0 cn.etetet.kof 包中，实现基于单一脚本的静态判定框配置，同时编写编辑器工具从 UFE 原工程中导出 AnimationMap 逐帧重载数据，供 Model 层实现精确的 UpdateMap 碰撞判定机制。

**Architecture:** Model层定义HitBox和AnimationMap数据结构及Component；View层通过MonoBehaviour获取基础配置与提供可视化；通过Editor工具导出UFE动效数据给Model层；在Tick中由Model更新每帧实际的HitBox中心点坐标。

**Tech Stack:** ET 9.0, Unity Editor Serialization, MonoBehaviours (仅做配置和Gizmos表现)。

---

### Task 1: 创建 Model 层 HitBox 数据结构

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/Model/Share/KofHitBoxData.cs`
- Create: `Packages/cn.etetet.kof/Scripts/Model/Share/KofHitBoxesComponent.cs`

**Step 1: 创建数据与组件结构**

编写代码，定义 `KofHitBoxType` 和 `KofHitBoxShape` 以及 `KofHitBoxData`。创建 `KofHitBoxesComponent`，继承 `Entity`。

```csharp
namespace ET
{
    public enum KofHitBoxType { High, Low }
    public enum KofHitBoxShape { Circle, Rectangle }
    
    public struct KofHitBoxData
    {
        public KofHitBoxType BoxType;
        public KofHitBoxShape Shape;
        public float Radius;
        public Unity.Mathematics.float2 Offset;
        public string BoneName;
    }
    
    [ComponentOf(typeof(KofFighterComponent))]
    public class KofHitBoxesComponent : Entity, IAwake
    {
        public System.Collections.Generic.List<KofHitBoxData> Boxes = new();
    }
}
```

**Step 2: 验证编译**

Run: `dotnet build Packages/cn.etetet.kof/cn.etetet.kof.asmdef -c Debug`
Expected: 成功构建无报错。

### Task 2: 创建 View 层 HitBox 配置与可视化

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/ModelView/Client/KofHitBoxesView.cs`

**Step 1: 创建配置与 Mono 组件**

编写 `KofHitBoxConfig` 类与 `KofHitBoxesView` (MonoBehaviour)。实现 `OnDrawGizmos` 实现在 Scene 视图可视化的红绿框。

```csharp
using UnityEngine;

namespace ET.Client
{
    [System.Serializable]
    public class KofHitBoxConfig
    {
        public KofHitBoxType BoxType;
        public KofHitBoxShape Shape;
        public float Radius;
        public Vector2 Offset;
        public string BoneName;
    }

    public class KofHitBoxesView : MonoBehaviour
    {
        public System.Collections.Generic.List<KofHitBoxConfig> BoxConfigs = new();
        
        private void OnDrawGizmos()
        {
            foreach (var box in BoxConfigs)
            {
                Gizmos.color = box.BoxType == KofHitBoxType.High ? Color.red : Color.green;
                var center = transform.position + new Vector3(box.Offset.x, box.Offset.y, 0);
                if (box.Shape == KofHitBoxShape.Circle)
                    Gizmos.DrawWireSphere(center, box.Radius);
            }
        }
    }
}
```

**Step 2: 验证编译**

Run: `dotnet build Packages/cn.etetet.kof/cn.etetet.kof.asmdef -c Debug`
Expected: 成功构建。

### Task 3: 实现 View 到 Model 的配置投射逻辑

**Files:**
- Modify: `Packages/cn.etetet.kof/Scripts/HotfixView/Client/KofBattleHelper.cs`

**Step 1: 在生成化身时抓取数据**

在 `KofBattleHelper.EnterBattle` 或相应生成角色节点，读取 `KofHitBoxesView` 中的数据写入对应 Entity 的 `KofHitBoxesComponent`。

```csharp
// 伪代码示例
var go = unit.GetComponent<GameObjectComponent>().GameObject;
var viewBoxes = go.GetComponent<KofHitBoxesView>();
if (viewBoxes != null)
{
    var hitBoxesComp = fighter.AddComponent<KofHitBoxesComponent>();
    foreach (var b in viewBoxes.BoxConfigs)
    {
        hitBoxesComp.Boxes.Add(new KofHitBoxData
        {
            BoxType = b.BoxType,
            Shape = b.Shape,
            Radius = b.Radius,
            Offset = new Unity.Mathematics.float2(b.Offset.x, b.Offset.y),
            BoneName = b.BoneName
        });
    }
}
```

**Step 2: 验证编译**

Run: `dotnet build Packages/cn.etetet.kof/cn.etetet.kof.asmdef -c Debug`
Expected: 成功构建。

### Task 4: 创建 Model 层 UpdateMap 数据结构

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/Model/Share/KofAnimationMapData.cs`

**Step 1: 制定核心重载结构**

定义 `KofFrameHitBoxData`, `KofAnimationFrameData`, `KofAnimationMapConfig` 与存放查表字典的 `KofAnimationMapComponent`。

```csharp
namespace ET
{
    public struct KofFrameHitBoxData
    {
        public string BoneName;
        public Unity.Mathematics.float2 Offset;
    }
    public struct KofAnimationFrameData
    {
        public int Frame;
        public System.Collections.Generic.List<KofFrameHitBoxData> BoxesData;
    }
    public class KofAnimationMapConfig
    {
        public int MoveId;
        public System.Collections.Generic.List<KofAnimationFrameData> FramesData = new();
    }
    
    [ComponentOf(typeof(KofFighterComponent))]
    public class KofAnimationMapComponent : Entity, IAwake
    {
        public System.Collections.Generic.Dictionary<int, KofAnimationMapConfig> MoveMaps = new();
    }
}
```

**Step 2: 验证编译**

Run: `dotnet build Packages/cn.etetet.kof/cn.etetet.kof.asmdef -c Debug`
Expected: 成功构建。

### Task 5: 开发 UpdateMap 编辑器导出工具

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/Editor/UfeAnimationMapExporter.cs`

**Step 1: 实现导出能力**

编写 C# Editor 脚本以 JSON 格式提取 MoveSetScript 中的逐帧数据。

```csharp
using UnityEditor;
using UnityEngine;
using System.IO;

namespace ET.Editor
{
    public class UfeAnimationMapExporter
    {
        [MenuItem("Tools/KOF/Export AnimationMap")]
        public static void Export()
        {
            // 通过 Selection 或者定死路径寻找 UFE 的 MoveSetScript
            // 解析 AnimationMap 里的 frame，将 bodyPart 与映射坐标对应导出到 json
            // 存入 Config 目录或对应打表结构
        }
    }
}
```

**Step 2: 验证逻辑**

检查代码逻辑是否能抓取到对应属性。由于依赖 UFE 类型，初步先构建成功即可，并在编辑器中测试 Tools 功能。

### Task 6: 在 Model 帧更新时应用 UpdateMap

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/Hotfix/Share/KofHitBoxesUpdateSystem.cs`

**Step 1: HitBox 覆写逻辑**

编写系统，查询 `KofAnimationMapComponent` 得到对应帧，覆写 `KofHitBoxesComponent`。此 System 需要执行在碰撞检测前。

```csharp
namespace ET
{
    public static class KofHitBoxesUpdateSystem
    {
        public static void UpdateHitBoxes(KofHitBoxesComponent hitBoxesComp, KofAnimationMapComponent mapComp, int moveId, int currentFrame, bool facingRight)
        {
            if (!mapComp.MoveMaps.TryGetValue(moveId, out var moveMap)) return;
            var frameData = moveMap.FramesData.Find(f => f.Frame == currentFrame);
            if (frameData.BoxesData == null) return;
            
            for (int i = 0; i < hitBoxesComp.Boxes.Count; i++) {
                var box = hitBoxesComp.Boxes[i];
                var targetOffset = frameData.BoxesData.Find(x => x.BoneName == box.BoneName);
                if (targetOffset.BoneName != null) {
                    box.Offset = new Unity.Mathematics.float2(facingRight ? targetOffset.Offset.x : -targetOffset.Offset.x, targetOffset.Offset.y);
                    hitBoxesComp.Boxes[i] = box;
                }
            }
        }
    }
}
```

**Step 2: 验证编译**

Run: `dotnet build Packages/cn.etetet.kof/cn.etetet.kof.asmdef -c Debug`

### Task 7: 动态坐标可视化与验证

**Files:**
- Modify: `Packages/cn.etetet.kof/Scripts/ModelView/Client/KofHitBoxesView.cs`

**Step 1: 渲染真实运算结果**

增加暴露的变量给 View 进行实时查看运算坐标。

```csharp
namespace ET.Client
{
    // 在 KofHitBoxesView 中补充
    // public System.Collections.Generic.List<KofHitBoxConfig> RealTimeBoxes = new();
    // 增加对 RealTimeBoxes 的绿色框渲染支持。
}
```

**Step 2: 验证流程**

在 Unity 环境中，通过播放查看 Gzimos。
