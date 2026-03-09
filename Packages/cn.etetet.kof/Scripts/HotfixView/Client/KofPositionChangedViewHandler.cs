using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// KOF位置变化View层处理器
    /// 接收 Evt_KofPositionChanged，同步 GameObject Transform
    /// </summary>
    [Event(SceneType.KofBattle)]
    public class KofPositionChangedViewHandler : AEvent<Scene, Evt_KofPositionChanged>
    {
        protected override async ETTask Run(Scene scene, Evt_KofPositionChanged args)
        {
            Log.Info($"[KOF][View] FighterId={args.FighterId} 位置=({args.PosX:F2},{args.PosY:F2}) 朝向={(args.FacingRight ? "右" : "左")}");
            // TODO: 通过 FighterId 找到对应 GameObject，更新 Transform
            // var go = FindFighterGO(scene, args.FighterId);
            // if (go != null) go.transform.position = new Vector3(args.PosX, args.PosY, 0f);
            // if (go != null) go.transform.localScale = new Vector3(args.FacingRight ? 1 : -1, 1, 1);

            await ETTask.CompletedTask;
        }
    }
}
