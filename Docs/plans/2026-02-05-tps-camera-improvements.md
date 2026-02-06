# TpsCamera Improvements Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 解决 TPS 相机的三个问题：震动干扰移动、准星独立移动、移动边界限制。

**Architecture:** 采用偏移量叠加模式 (Offset Composition)，将震动与瞄准分离；引入虚拟瞄准点 (VirtualAimOffset) 作为准星与相机的共同驱动源，准星快速响应，相机滞后跟随；通过 Clamp 限制瞄准范围。

**Tech Stack:** ET 9.0 ECS, Unity Transform, UnityEngine.UI

---

## Task 1: Modify TpsInputComponent

**Files:**
- Modify: `Packages/cn.etetet.tps/Scripts/ModelView/Client/TpsInputComponent.cs`

**Step 1: Add new fields for clamping and crosshair position**

Add the following fields to `TpsInputComponent`:

```csharp
/// <summary>
/// 虚拟瞄准点相对于屏幕中心的偏移 (像素)
/// </summary>
public Vector2 AimScreenOffset;

/// <summary>
/// 准星允许移动的最大屏幕范围 (像素)
/// 默认约为屏幕宽度/高度的 45%
/// </summary>
public Vector2 MaxAimScreenOffset;

/// <summary>
/// 最终准星的屏幕坐标 (用于 UI)
/// </summary>
public Vector2 CrosshairScreenPosition;
```

**Step 2: Verify compilation**

Run: Unity Editor Console
Expected: No compilation errors

**Step 3: Commit**

```bash
git add Packages/cn.etetet.tps/Scripts/ModelView/Client/TpsInputComponent.cs
git commit -m "feat(tps): add AimScreenOffset and CrosshairScreenPosition to TpsInputComponent"
```

---

## Task 2: Modify TpsInputComponentSystem - Clamp Logic

**Files:**
- Modify: `Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsInputComponentSystem.cs:16-99`

**Step 1: Initialize new fields in Awake**

In `Awake` method, add initialization:

```csharp
self.AimScreenOffset = Vector2.zero;
self.MaxAimScreenOffset = new Vector2(Screen.width * 0.45f, Screen.height * 0.45f);
self.CrosshairScreenPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
```

**Step 2: Refactor ProcessInput to calculate AimScreenOffset with Clamp**

Replace the `ProcessInput` method logic (lines 49-99) with:

```csharp
private static void ProcessInput(this TpsInputComponent self)
{
    bool wasPressed = self.IsPressing;

#if UNITY_EDITOR || UNITY_STANDALONE
    self.IsPressing = Input.GetMouseButton(0);
    if (self.IsPressing)
    {
        self.ScreenPosition = Input.mousePosition;
    }
#else
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);
        self.IsPressing = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
        if (self.IsPressing)
        {
            self.ScreenPosition = touch.position;
        }
    }
    else
    {
        self.IsPressing = false;
    }
#endif

    // 计算相对于屏幕中心的偏移
    float halfWidth = Screen.width / 2f;
    float halfHeight = Screen.height / 2f;
    
    if (self.IsPressing)
    {
        // 计算原始偏移
        Vector2 rawOffset = new Vector2(
            self.ScreenPosition.x - halfWidth,
            self.ScreenPosition.y - halfHeight
        );
        
        // Clamp 限制边界
        self.AimScreenOffset = new Vector2(
            Mathf.Clamp(rawOffset.x, -self.MaxAimScreenOffset.x, self.MaxAimScreenOffset.x),
            Mathf.Clamp(rawOffset.y, -self.MaxAimScreenOffset.y, self.MaxAimScreenOffset.y)
        );
        
        // 计算准星屏幕坐标
        self.CrosshairScreenPosition = new Vector2(
            halfWidth + self.AimScreenOffset.x,
            halfHeight + self.AimScreenOffset.y
        );
        
        // 保留归一化方向用于兼容
        self.NormalizedAimDirection = new Vector2(
            self.AimScreenOffset.x / self.MaxAimScreenOffset.x,
            self.AimScreenOffset.y / self.MaxAimScreenOffset.y
        ) * self.Sensitivity;
    }

    // 检测状态切换
    if (self.IsPressing && !wasPressed)
    {
        self.OnPressDown();
    }
    else if (!self.IsPressing && wasPressed)
    {
        self.OnPressUp();
    }
}
```

**Step 3: Verify compilation**

Run: Unity Editor Console
Expected: No compilation errors

**Step 4: Commit**

```bash
git add Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsInputComponentSystem.cs
git commit -m "feat(tps): implement AimScreenOffset calculation with Clamp in TpsInputSystem"
```

---

## Task 3: Modify TpsCameraComponent

**Files:**
- Modify: `Packages/cn.etetet.tps/Scripts/ModelView/Client/TpsCameraComponent.cs`

**Step 1: Add new fields for shake and camera follow**

Add the following fields:

```csharp
/// <summary>
/// 当前因瞄准产生的相机偏移 (经过平滑处理)
/// </summary>
public Vector3 CurrentAimOffset;

/// <summary>
/// 当前因震动产生的临时位移
/// </summary>
public Vector3 ShakeOffset;

/// <summary>
/// 相机跟随准星的移动比例 (0-1)
/// 值越小，相机移动幅度越小
/// </summary>
public float CameraFollowRatio;

/// <summary>
/// 震动衰减速度
/// </summary>
public float ShakeDecay;

/// <summary>
/// 像素到世界单位的转换系数
/// </summary>
public float PixelToWorldRatio;
```

**Step 2: Verify compilation**

Run: Unity Editor Console
Expected: No compilation errors

**Step 3: Commit**

```bash
git add Packages/cn.etetet.tps/Scripts/ModelView/Client/TpsCameraComponent.cs
git commit -m "feat(tps): add ShakeOffset and CameraFollowRatio to TpsCameraComponent"
```

---

## Task 4: Refactor TpsCameraComponentSystem - Offset Composition

**Files:**
- Modify: `Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsCameraComponentSystem.cs`

**Step 1: Initialize new fields in Awake**

In `Awake` method (lines 17-29), add:

```csharp
self.CurrentAimOffset = Vector3.zero;
self.ShakeOffset = Vector3.zero;
self.CameraFollowRatio = 0.1f;  // 相机移动幅度是准星的 10%
self.ShakeDecay = 15f;          // 震动衰减速度
self.PixelToWorldRatio = 0.005f; // 500 像素 ≈ 2.5 世界单位
```

**Step 2: Refactor UpdateCameraPosition with Offset Composition**

Replace `UpdateCameraPosition` (lines 60-93) with:

```csharp
private static void UpdateCameraPosition(this TpsCameraComponent self)
{
    TpsInputComponent inputComponent = self.Scene().GetComponent<TpsInputComponent>();
    TpsStateComponent stateComponent = self.Scene().GetComponent<TpsStateComponent>();

    if (inputComponent == null || stateComponent == null)
    {
        return;
    }

    // 1. 计算目标瞄准偏移
    Vector3 targetAimOffset = Vector3.zero;
    if (stateComponent.IsAiming())
    {
        // 将屏幕偏移转换为世界坐标偏移
        targetAimOffset = new Vector3(
            inputComponent.AimScreenOffset.x * self.PixelToWorldRatio * self.CameraFollowRatio,
            inputComponent.AimScreenOffset.y * self.PixelToWorldRatio * self.CameraFollowRatio,
            0f
        );
    }

    // 2. 平滑移动 CurrentAimOffset
    self.CurrentAimOffset = Vector3.Lerp(
        self.CurrentAimOffset,
        targetAimOffset,
        Time.deltaTime * self.SmoothSpeed
    );

    // 3. 震动衰减
    self.ShakeOffset = Vector3.Lerp(
        self.ShakeOffset,
        Vector3.zero,
        Time.deltaTime * self.ShakeDecay
    );

    // 4. 合成最终位置
    Vector3 finalPosition = self.OriginalPosition + self.CurrentAimOffset + self.ShakeOffset;
    self.MainCamera.transform.position = finalPosition;
}
```

**Step 3: Refactor ShakeCamera to use ShakeOffset**

Replace `ShakeCamera` method (lines 111-143) with:

```csharp
/// <summary>
/// 应用相机震动效果（射击反馈）
/// 直接设置 ShakeOffset，由 Update 负责衰减
/// </summary>
public static void ApplyShake(this TpsCameraComponent self, float intensity)
{
    if (self.MainCamera == null)
    {
        return;
    }

    // 添加随机震动冲量
    float x = Random.Range(-1f, 1f) * intensity;
    float y = Random.Range(-1f, 1f) * intensity;
    self.ShakeOffset += new Vector3(x, y, 0f);
}

/// <summary>
/// 兼容旧接口（异步版本）
/// 现在不再需要异步，直接调用 ApplyShake
/// </summary>
public static async ETTask ShakeCamera(this TpsCameraComponent self, float intensity, float duration)
{
    // 多次应用震动冲量，模拟持续震动
    EntityRef<TpsCameraComponent> selfRef = self;
    float elapsed = 0f;
    float interval = 0.03f; // 30ms 间隔

    while (elapsed < duration)
    {
        self = selfRef;
        if (self == null || self.IsDisposed)
        {
            return;
        }

        self.ApplyShake(intensity);
        elapsed += interval;
        await self.Root().GetComponent<TimerComponent>().WaitAsync((long)(interval * 1000));
    }
}
```

**Step 4: Verify compilation**

Run: Unity Editor Console
Expected: No compilation errors

**Step 5: Commit**

```bash
git add Packages/cn.etetet.tps/Scripts/HotfixView/Client/TpsCameraComponentSystem.cs
git commit -m "feat(tps): implement Offset Composition in TpsCameraSystem"
```

---

## Task 5: Manual Verification

**Step 1: Run the game and enter TPS scene**

Run: Unity Editor Play Mode
Navigate to TPS battle scene

**Step 2: Test crosshair movement with clamping**

Action: Move mouse around the screen
Expected:
- Crosshair follows mouse position immediately
- Crosshair cannot move beyond 45% of screen width/height from center

**Step 3: Test camera follow with lag**

Action: Move mouse slowly
Expected:
- Camera follows crosshair direction with noticeable delay
- Camera movement amplitude is smaller than crosshair movement (about 10%)

**Step 4: Test shooting with vibration isolation**

Action: Click to shoot while aiming
Expected:
- Camera shakes briefly on each shot
- Shake does not interrupt smooth camera follow movement
- Crosshair position remains unaffected by shake
- Camera returns to correct aim-based position after shake decays

**Step 5: Final commit**

```bash
git add -A
git commit -m "feat(tps): complete TpsCamera improvements - vibration isolation, crosshair decoupling, clamping"
```

---

## Summary

| Task | Description | Duration |
|------|-------------|----------|
| 1 | Add fields to TpsInputComponent | 2-3 min |
| 2 | Implement Clamp logic in TpsInputSystem | 3-5 min |
| 3 | Add fields to TpsCameraComponent | 2-3 min |
| 4 | Implement Offset Composition in TpsCameraSystem | 5-8 min |
| 5 | Manual verification | 5-10 min |

**Total estimated time: 17-29 minutes**
