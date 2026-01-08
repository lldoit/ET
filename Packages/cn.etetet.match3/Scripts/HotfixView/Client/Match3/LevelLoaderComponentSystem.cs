using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace ET.Client
{
    /// <summary>
    /// 关卡加载器组件系统
    /// 使用YooAssets加载关卡JSON文件
    /// </summary>
    [FriendOf(typeof(LevelLoaderComponent))]
    [EntitySystemOf(typeof(LevelLoaderComponent))]
    public static partial class LevelLoaderComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LevelLoaderComponent self)
        {
            self.LevelCache = new Dictionary<int, Level>();
            self.LevelPathPrefix = "Match3/Levels/";
        }

        [EntitySystem]
        private static void Destroy(this LevelLoaderComponent self)
        {
            self.LevelCache?.Clear();
            self.LevelCache = null;
        }

        /// <summary>
        /// 异步加载关卡
        /// </summary>
        /// <param name="self">加载器组件</param>
        /// <param name="levelId">关卡ID</param>
        /// <param name="forceReload">是否强制重新加载（忽略缓存）</param>
        /// <returns>关卡数据</returns>
        public static async ETTask<Level> LoadLevelAsync(this LevelLoaderComponent self, int levelId, bool forceReload = false)
        {
            // 检查缓存
            if (!forceReload && self.LevelCache.TryGetValue(levelId, out var cachedLevel))
            {
                Log.Info($"从缓存加载关卡 {levelId}");
                return cachedLevel;
            }

            // 构建资源路径
            string assetPath = $"{self.LevelPathPrefix}{levelId}";
            
            try
            {
                // 使用YooAssets加载TextAsset
                var resourcePackage = YooAssets.GetPackage("DefaultPackage");
                var handle = resourcePackage.LoadAssetAsync<TextAsset>(assetPath);
                await handle.Task;
                
                var textAsset = handle.AssetObject as TextAsset;
                if (textAsset == null)
                {
                    Log.Error($"加载关卡失败: {assetPath}，TextAsset为空");
                    return default;
                }

                // 解析JSON
                var levelData = JsonUtility.FromJson<LevelData>(textAsset.text);
                if (levelData == null)
                {
                    Log.Error($"解析关卡JSON失败: {assetPath}");
                    return default;
                }

                // 转换为Level struct
                var level = ConvertToLevel(levelData);
                level.Id = levelId;

                // 缓存关卡
                self.LevelCache[levelId] = level;
                
                // 发布关卡UI初始化事件
                EventSystem.Instance?.Publish(
                    self.Root(), 
                    new LevelUIInitEvent { Level = level }
                );
                
                Log.Info($"成功加载关卡 {levelId}");
                return level;

            }
            catch (Exception e)
            {
                Log.Error($"加载关卡异常: {assetPath}, {e.Message}");
                return default;
            }
        }

        /// <summary>
        /// 清除关卡缓存
        /// </summary>
        public static void ClearCache(this LevelLoaderComponent self)
        {
            self.LevelCache?.Clear();
            Log.Info("关卡缓存已清除");
        }

        /// <summary>
        /// 从缓存获取关卡（同步方法）
        /// </summary>
        public static Level? GetCachedLevel(this LevelLoaderComponent self, int levelId)
        {
            if (self.LevelCache.TryGetValue(levelId, out var level))
            {
                return level;
            }
            return null;
        }

        #region 数据转换方法

        /// <summary>
        /// 将LevelData转换为Level结构体
        /// </summary>
        private static Level ConvertToLevel(LevelData data)
        {
            var level = new Level
            {
                Id = data.Id,
                Width = data.Width,
                Height = data.Height,
                LimitType = ParseEnum<LimitType>(data.LimitType),
                Limit = data.Limit,
                Score1 = data.Score1,
                Score2 = data.Score2,
                Score3 = data.Score3,
                AwardSpecialCandies = data.AwardSpecialCandies,
                AwardedSpecialCandyType = ParseEnum<AwardedSpecialCandyType>(data.AwardedSpecialCandyType),
                CollectableChance = data.CollectableChance,
                Tiles = new List<LevelTile>(),
                Goals = new List<Goal>(),
                AvailableColors = new List<CandyColor>(),
                AvailableBoosters = new Dictionary<BoosterType, bool>()
            };

            // 转换瓦片
            if (data.Tiles != null)
            {
                foreach (var tileData in data.Tiles)
                {
                    level.Tiles.Add(ConvertToLevelTile(tileData));
                }
            }

            // 转换目标
            if (data.Goals != null)
            {
                foreach (var goalData in data.Goals)
                {
                    level.Goals.Add(ConvertToGoal(goalData));
                }
            }

            // 转换可用颜色
            if (data.AvailableColors != null)
            {
                foreach (var colorStr in data.AvailableColors)
                {
                    level.AvailableColors.Add(ParseEnum<CandyColor>(colorStr));
                }
            }

            // 转换道具设置
            if (data.AvailableBoosters != null)
            {
                level.AvailableBoosters[BoosterType.Lollipop] = data.AvailableBoosters.Lollipop;
                level.AvailableBoosters[BoosterType.Bomb] = data.AvailableBoosters.Bomb;
                level.AvailableBoosters[BoosterType.Switch] = data.AvailableBoosters.Switch;
                level.AvailableBoosters[BoosterType.ColorBomb] = data.AvailableBoosters.ColorBomb;
            }

            return level;
        }

        private static LevelTile ConvertToLevelTile(LevelTileData data)
        {
            var tile = new LevelTile
            {
                TileType = ParseEnum<LevelTileType>(data.TileType),
                ElementType = ParseEnum<ElementType>(data.ElementType),
                CandyType = ParseEnum<CandyType>(data.CandyType),
                SpecialCandyType = ParseEnum<SpecialCandyType>(data.SpecialCandyType),
                SpecialBlockType = ParseEnum<SpecialBlockType>(data.SpecialBlockType),
                CollectableType = ParseEnum<CollectableType>(data.CollectableType)
            };
            return tile;
        }

        private static Goal ConvertToGoal(GoalData data)
        {
            var goal = new Goal
            {
                GoalType = ParseEnum<GoalType>(data.GoalType),
                Amount = data.Amount,
                CandyColor = ParseEnum<CandyColor>(data.CandyColor),
                ElementType = ParseEnum<ElementType>(data.ElementType),
                SpecialBlockType = ParseEnum<SpecialBlockType>(data.SpecialBlockType),
                CollectableType = ParseEnum<CollectableType>(data.CollectableType)
            };
            return goal;
        }

        private static T ParseEnum<T>(string value) where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }
            if (Enum.TryParse<T>(value, true, out var result))
            {
                return result;
            }
            return default;
        }

        #endregion
    }
}
