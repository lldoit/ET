using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 道具视图组件 - 负责道具的视觉表现
    /// </summary>
    [ComponentOf(typeof(BoosterManagerComponent))]
    public class BoosterViewComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 道具特效预制体
        /// </summary>
        public GameObject LollipopEffectPrefab;
        public GameObject BombEffectPrefab;
        public GameObject ColorBombEffectPrefab;
        public GameObject SwitchEffectPrefab;
        
        /// <summary>
        /// 道具使用音效
        /// </summary>
        public string LollipopSound = "BoosterLollipop";
        public string BombSound = "BoosterBomb";
        public string ColorBombSound = "BoosterColorBomb";
        public string SwitchSound = "BoosterSwitch";
        
        /// <summary>
        /// 特效对象池（可选）
        /// </summary>
        public GameObject EffectPool;
        
        /// <summary>
        /// 动画持续时间（毫秒）
        /// </summary>
        public int LollipopAnimDuration = 300;
        public int BombAnimDuration = 500;
        public int ColorBombAnimDuration = 600;
        public int SwitchAnimDuration = 250;
    }
}

