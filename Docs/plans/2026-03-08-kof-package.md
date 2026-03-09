# KOF Package Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Create the core ET 9.0 ECS backbone for the `cn.etetet.kof` package, establishing the hybrid View-Model interaction loop for a local fighting game MVP.

**Architecture:** Focuses on a Hybrid Architecture where Unity handles inputs and hitbox detection (View), while the ET Model maintains absolute authority over Health and state calculations.

**Tech Stack:** ET 9.0 Framework (C#), Unity

---

### Task 1: Package Configuration & PackageType

**Files:**
- Create: `Packages/cn.etetet.kof/package.json`
- Create: `Packages/cn.etetet.kof/packagegit.json`
- Create: `Packages/cn.etetet.kof/Scripts/Model/Share/PackageType.cs`

**Step 1: Write minimal package configs**
Create standard ET package config files assigning a unique PackageId, and the `PackageType.cs` to map the ID.

**Step 2: Verify package structure**
Run: `ls -la Packages/cn.etetet.kof/`
Expected: Configs are present.

**Step 3: Commit**
```bash
git add Packages/cn.etetet.kof/package* Packages/cn.etetet.kof/Scripts/Model/Share/PackageType.cs
git commit -m "build(kof): add package metadata and PackageType"
```

### Task 2: Assembly Definitions (AsmDefs)

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/Model/ClientServer/cn.etetet.kof.Model.asmdef`
- Create: `Packages/cn.etetet.kof/Scripts/Hotfix/ClientServer/cn.etetet.kof.Hotfix.asmdef`
- Create: `Packages/cn.etetet.kof/Scripts/ModelView/Client/cn.etetet.kof.ModelView.asmdef`
- Create: `Packages/cn.etetet.kof/Scripts/HotfixView/Client/cn.etetet.kof.HotfixView.asmdef`

**Step 1: Write AsmDef JSONs**
Create the JSON assembly definitions ensuring `Hotfix` references `Model`, and `HotfixView` references `ModelView` and `Hotfix` according to ET's layered architecture.

**Step 2: Verify Unity compilation**
(Assume Unity will auto-compile, rely on codebase checks later).

**Step 3: Commit**
```bash
git add Packages/cn.etetet.kof/Scripts/**/*.asmdef
git commit -m "build(kof): setup assembly definitions"
```

### Task 3: Core Fighter Model (State & Attributes)

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/Model/ClientServer/KofFighterComponent.cs`
- Create: `Packages/cn.etetet.kof/Scripts/Hotfix/ClientServer/KofFighterComponentSystem.cs`

**Step 1: Write the Entity and System**
Create `KofFighterComponent` containing `HP`, `MaxHP`, and `Energy`. Create static extension methods in `KofFighterComponentSystem` (e.g. `TakeDamage`).

**Step 2: Review ET Analyzer compliance**
Run standard DOTNET build or inspect code to ensure no methods are in the Entity, and System uses `[EntitySystem]` and `EntityRef`.

**Step 3: Commit**
```bash
git add Packages/cn.etetet.kof/Scripts/Model/ClientServer/KofFighterComponent.cs Packages/cn.etetet.kof/Scripts/Hotfix/ClientServer/KofFighterComponentSystem.cs
git commit -m "feat(kof): implement KofFighterComponent and System"
```

### Task 4: Communication Events

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/Model/ClientServer/KofEvents.cs`

**Step 1: Define View-Model interaction events**
Create struct events: `Evt_KofHitDetection` (View->Model), `Evt_KofHPChanged` (Model->View), and `Evt_KofRequestSkill` (View->Model).

**Step 2: Commit**
```bash
git add Packages/cn.etetet.kof/Scripts/Model/ClientServer/KofEvents.cs
git commit -m "feat(kof): add KOF boundary events"
```

### Task 5: Model Logic: Hit Detection Resolution

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/Hotfix/ClientServer/KofHitDetectionHandler.cs`

**Step 1: Write the Event Handler**
Implement `AEvent<Scene, Evt_KofHitDetection>`. Upon receiving the hit, query `KofFighterComponentSystem.TakeDamage`, update HP, and fire `Evt_KofHPChanged`.

**Step 2: Commit**
```bash
git add Packages/cn.etetet.kof/Scripts/Hotfix/ClientServer/KofHitDetectionHandler.cs
git commit -m "feat(kof): implement hit detection calculation in Model"
```

### Task 6: View Logic: Input & HP Binding Stubs

**Files:**
- Create: `Packages/cn.etetet.kof/Scripts/HotfixView/Client/KofHPChangedViewHandler.cs`

**Step 1: Write the View Event Handler for HP**
Implement `AEvent<Scene, Evt_KofHPChanged>` in the HotfixView layer. Add debug logs or comment stubs for linking to Unity UI/Animation.

**Step 2: Commit**
```bash
git add Packages/cn.etetet.kof/Scripts/HotfixView/Client/KofHPChangedViewHandler.cs
git commit -m "feat(kof): add HotfixView handler for HP changes"
```
