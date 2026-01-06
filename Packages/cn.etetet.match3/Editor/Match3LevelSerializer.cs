using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ET.Match3.Editor
{
    /// <summary>
    /// 关卡JSON序列化包装类（用于JsonUtility序列化struct）
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public int Id;
        public int Width;
        public int Height;
        public string LimitType;
        public int Limit;
        public int Score1;
        public int Score2;
        public int Score3;
        public bool AwardSpecialCandies;
        public string AwardedSpecialCandyType;
        public int CollectableChance;
        public List<LevelTileData> Tiles;
        public List<GoalData> Goals;
        public List<string> AvailableColors;
        public BoosterSettingsData AvailableBoosters;

        public LevelData()
        {
            Tiles = new List<LevelTileData>();
            Goals = new List<GoalData>();
            AvailableColors = new List<string>();
            AvailableBoosters = new BoosterSettingsData();
        }
    }

    /// <summary>
    /// 关卡瓦片序列化数据
    /// </summary>
    [Serializable]
    public class LevelTileData
    {
        public string TileType;
        public string ElementType;
        public string CandyType;
        public string SpecialCandyType;
        public string SpecialBlockType;
        public string CollectableType;
    }

    /// <summary>
    /// 目标序列化数据
    /// </summary>
    [Serializable]
    public class GoalData
    {
        public string GoalType;
        public int Amount;
        public string CandyColor;
        public string ElementType;
        public string SpecialBlockType;
        public string CollectableType;
    }

    /// <summary>
    /// 道具设置序列化数据
    /// </summary>
    [Serializable]
    public class BoosterSettingsData
    {
        public bool Lollipop;
        public bool Bomb;
        public bool Switch;
        public bool ColorBomb;
    }

    /// <summary>
    /// 关卡序列化工具
    /// </summary>
    public static class Match3LevelSerializer
    {
        /// <summary>
        /// 从文件加载关卡
        /// </summary>
        public static Level LoadLevel(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"关卡文件不存在: {path}");
                return default;
            }

            try
            {
                string json = File.ReadAllText(path);
                LevelData data = JsonUtility.FromJson<LevelData>(json);
                return ConvertToLevel(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"加载关卡失败: {e.Message}");
                return default;
            }
        }

        /// <summary>
        /// 保存关卡到文件
        /// </summary>
        public static void SaveLevel(string path, Level level)
        {
            try
            {
                LevelData data = ConvertToLevelData(level);
                string json = JsonUtility.ToJson(data, true);
                
                // 确保目录存在
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(path, json);
                Debug.Log($"关卡已保存: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"保存关卡失败: {e.Message}");
            }
        }

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

        /// <summary>
        /// 将Level结构体转换为LevelData
        /// </summary>
        private static LevelData ConvertToLevelData(Level level)
        {
            var data = new LevelData
            {
                Id = level.Id,
                Width = level.Width,
                Height = level.Height,
                LimitType = level.LimitType.ToString(),
                Limit = level.Limit,
                Score1 = level.Score1,
                Score2 = level.Score2,
                Score3 = level.Score3,
                AwardSpecialCandies = level.AwardSpecialCandies,
                AwardedSpecialCandyType = level.AwardedSpecialCandyType.ToString(),
                CollectableChance = level.CollectableChance
            };

            // 转换瓦片
            if (level.Tiles != null)
            {
                foreach (var tile in level.Tiles)
                {
                    data.Tiles.Add(ConvertToLevelTileData(tile));
                }
            }

            // 转换目标
            if (level.Goals != null)
            {
                foreach (var goal in level.Goals)
                {
                    data.Goals.Add(ConvertToGoalData(goal));
                }
            }

            // 转换可用颜色
            if (level.AvailableColors != null)
            {
                foreach (var color in level.AvailableColors)
                {
                    data.AvailableColors.Add(color.ToString());
                }
            }

            // 转换道具设置
            if (level.AvailableBoosters != null)
            {
                data.AvailableBoosters.Lollipop = level.AvailableBoosters.GetValueOrDefault(BoosterType.Lollipop);
                data.AvailableBoosters.Bomb = level.AvailableBoosters.GetValueOrDefault(BoosterType.Bomb);
                data.AvailableBoosters.Switch = level.AvailableBoosters.GetValueOrDefault(BoosterType.Switch);
                data.AvailableBoosters.ColorBomb = level.AvailableBoosters.GetValueOrDefault(BoosterType.ColorBomb);
            }

            return data;
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

        private static LevelTileData ConvertToLevelTileData(LevelTile tile)
        {
            return new LevelTileData
            {
                TileType = tile.TileType.ToString(),
                ElementType = tile.ElementType.ToString(),
                CandyType = tile.CandyType.ToString(),
                SpecialCandyType = tile.SpecialCandyType.ToString(),
                SpecialBlockType = tile.SpecialBlockType.ToString(),
                CollectableType = tile.CollectableType.ToString()
            };
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

        private static GoalData ConvertToGoalData(Goal goal)
        {
            return new GoalData
            {
                GoalType = goal.GoalType.ToString(),
                Amount = goal.Amount,
                CandyColor = goal.CandyColor.ToString(),
                ElementType = goal.ElementType.ToString(),
                SpecialBlockType = goal.SpecialBlockType.ToString(),
                CollectableType = goal.CollectableType.ToString()
            };
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
    }
}
