using System.Collections.Generic;

namespace ET;

/// <summary>
/// 设置实体状态事件
/// </summary>
public struct SetEntityState
{
    public int EntityId;
    public int state;
}

/// <summary>
/// 取消实体状态事件
/// </summary>
public struct UnsetEntityState
{
    public int EntityId;
    public int state;
}

public struct DamageInfo
{
    public int TargetId;
    public int Damage;
    public int SpellResult;
}

public struct EntityCastSpell
{
    public int CasterId;
    public int SpellId;
    public List<DamageInfo> DamageInfos;
}