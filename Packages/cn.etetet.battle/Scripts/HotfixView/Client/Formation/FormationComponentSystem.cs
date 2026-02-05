using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 战斗站位组件System
    /// 处理站位配置的加载和角色位置获取
    /// </summary>
    [FriendOf(typeof(FormationComponent))]
    [EntitySystemOf(typeof(FormationComponent))]
    public static partial class FormationComponentSystem
    {
        /// <summary>
        /// 默认站位配置资源路径
        /// </summary>
        public const string DefaultConfigPath = "BattleFormationConfig";

        [EntitySystem]
        private static void Awake(this FormationComponent self)
        {
            self.ConfigAssetPath = DefaultConfigPath;
        }

        [EntitySystem]
        private static void Destroy(this FormationComponent self)
        {
            self.FormationConfig = null;
            self.BattleRoot = null;
        }

        /// <summary>
        /// 异步初始化站位组件
        /// </summary>
        /// <param name="self">站位组件</param>
        /// <param name="battleRoot">战斗界面根节点</param>
        /// <param name="configPath">配置资源路径（可选）</param>
        public static async ETTask InitializeAsync(this FormationComponent self, RectTransform battleRoot, string configPath = null)
        {
            self.BattleRoot = battleRoot;

            if (!string.IsNullOrEmpty(configPath))
            {
                self.ConfigAssetPath = configPath;
            }

            // 加载站位配置
            var loader = self.Scene().GetComponent<ResourcesLoaderComponent>();
            if (loader != null)
            {
                self.FormationConfig = await loader.LoadAssetAsync<BattleFormationConfig>(self.ConfigAssetPath);
                if (self.FormationConfig != null)
                {
                    Log.Info($"[Formation] 站位配置加载成功");
                }
                else
                {
                    Log.Warning($"[Formation] 站位配置加载失败: {self.ConfigAssetPath}");
                }
            }

            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 设置战斗界面根节点（用于坐标转换）
        /// </summary>
        /// <param name="self">站位组件</param>
        /// <param name="battleRoot">战斗界面根节点</param>
        public static void SetBattleRoot(this FormationComponent self, RectTransform battleRoot)
        {
            self.BattleRoot = battleRoot;
            Log.Info($"[Formation] BattleRoot已设置: {(battleRoot != null ? battleRoot.name : "null")}");
        }

        /// <summary>
        /// 获取玩家方站位位置
        /// </summary>
        /// <param name="self">站位组件</param>
        /// <param name="slotIndex">站位索引（0-3）</param>
        /// <returns>站位UI坐标</returns>
        public static Vector2 GetPlayerSlotPosition(this FormationComponent self, int slotIndex)
        {
            if (self.FormationConfig == null)
            {
                Log.Warning("[Formation] 站位配置未加载");
                return Vector2.zero;
            }

            var slot = self.FormationConfig.GetPlayerSlot(slotIndex);
            return slot.Position;
        }

        /// <summary>
        /// 获取敌方站位位置
        /// </summary>
        /// <param name="self">站位组件</param>
        /// <param name="enemyCount">敌人总数（1-4）</param>
        /// <param name="slotIndex">站位索引</param>
        /// <returns>站位UI坐标</returns>
        public static Vector2 GetEnemySlotPosition(this FormationComponent self, int enemyCount, int slotIndex)
        {
            if (self.FormationConfig == null)
            {
                Log.Warning("[Formation] 站位配置未加载");
                return Vector2.zero;
            }

            var slot = self.FormationConfig.GetEnemySlot(enemyCount, slotIndex);
            return slot.Position;
        }

        /// <summary>
        /// 获取玩家方站位朝向
        /// </summary>
        /// <param name="self">站位组件</param>
        /// <param name="slotIndex">站位索引（0-3）</param>
        /// <returns>是否面向左侧</returns>
        public static bool GetPlayerSlotFacing(this FormationComponent self, int slotIndex)
        {
            if (self.FormationConfig == null)
            {
                return false;
            }

            var slot = self.FormationConfig.GetPlayerSlot(slotIndex);
            return slot.FacingLeft;
        }

        /// <summary>
        /// 获取敌方站位朝向
        /// </summary>
        /// <param name="self">站位组件</param>
        /// <param name="enemyCount">敌人总数（1-4）</param>
        /// <param name="slotIndex">站位索引</param>
        /// <returns>是否面向左侧</returns>
        public static bool GetEnemySlotFacing(this FormationComponent self, int enemyCount, int slotIndex)
        {
            if (self.FormationConfig == null)
            {
                return true;
            }

            var slot = self.FormationConfig.GetEnemySlot(enemyCount, slotIndex);
            return slot.FacingLeft;
        }

        /// <summary>
        /// 根据阵营和索引获取站位位置
        /// </summary>
        /// <param name="self">站位组件</param>
        /// <param name="camp">阵营</param>
        /// <param name="slotIndex">站位索引</param>
        /// <param name="totalCount">该阵营总人数（用于敌方站位选择）</param>
        /// <returns>站位UI坐标</returns>
        public static Vector2 GetSlotPosition(this FormationComponent self, ECamp camp, int slotIndex, int totalCount = 4)
        {
            return camp == ECamp.Red
                ? self.GetPlayerSlotPosition(slotIndex)
                : self.GetEnemySlotPosition(totalCount, slotIndex);
        }

        /// <summary>
        /// 根据阵营和索引获取站位朝向
        /// </summary>
        /// <param name="self">站位组件</param>
        /// <param name="camp">阵营</param>
        /// <param name="slotIndex">站位索引</param>
        /// <param name="totalCount">该阵营总人数（用于敌方站位选择）</param>
        /// <returns>是否面向左侧</returns>
        public static bool GetSlotFacing(this FormationComponent self, ECamp camp, int slotIndex, int totalCount = 4)
        {
            return camp == ECamp.Red
                ? self.GetPlayerSlotFacing(slotIndex)
                : self.GetEnemySlotFacing(totalCount, slotIndex);
        }

        /// <summary>
        /// 获取玩家方站位数量
        /// </summary>
        public static int GetPlayerSlotCount(this FormationComponent self)
        {
            return self.FormationConfig?.PlayerSlotCount ?? 0;
        }

        /// <summary>
        /// 将UI坐标转换为世界坐标
        /// </summary>
        /// <param name="self">站位组件</param>
        /// <param name="uiPosition">UI坐标（相对于BattleRoot中心）</param>
        /// <returns>世界坐标</returns>
        public static Vector3 UIToWorldPosition(this FormationComponent self, Vector2 uiPosition)
        {
            if (self.BattleRoot == null)
            {
                Log.Warning("[Formation] BattleRoot未设置，无法进行坐标转换");
                return new Vector3(uiPosition.x, uiPosition.y, 0);
            }

            // 将本地坐标转换为世界坐标
            Vector3 worldPos = self.BattleRoot.TransformPoint(new Vector3(uiPosition.x, uiPosition.y, 0));
            return worldPos;
        }

        /// <summary>
        /// 获取站位的世界坐标
        /// </summary>
        /// <param name="self">站位组件</param>
        /// <param name="camp">阵营</param>
        /// <param name="slotIndex">站位索引</param>
        /// <param name="totalCount">该阵营总人数</param>
        /// <returns>世界坐标</returns>
        public static Vector3 GetSlotWorldPosition(this FormationComponent self, ECamp camp, int slotIndex, int totalCount = 4)
        {
            Vector2 uiPos = self.GetSlotPosition(camp, slotIndex, totalCount);
            return self.UIToWorldPosition(uiPos);
        }
    }
}

