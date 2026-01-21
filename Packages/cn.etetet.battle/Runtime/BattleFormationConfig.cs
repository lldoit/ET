using UnityEngine;
using ET;

namespace ET.Client
{
    /// <summary>
    /// 战斗站位配置 - ScriptableObject
    /// 用于存储战斗界面中角色的站位信息
    /// </summary>
    [CreateAssetMenu(fileName = "BattleFormationConfig", menuName = "ET/Battle/Formation Config")]
    public class BattleFormationConfig : ScriptableObject
    {
        /// <summary>
        /// 玩家方站位（左侧，最多4个）
        /// </summary>
        [Header("玩家方站位（左侧）")]
        public FormationSide PlayerFormation = new FormationSide
        {
            Name = "玩家方",
            Slots = new FormationSlot[]
            {
                new FormationSlot { Index = 0, Position = new Vector2(-300, 150), FacingLeft = false },
                new FormationSlot { Index = 1, Position = new Vector2(-300, 50), FacingLeft = false },
                new FormationSlot { Index = 2, Position = new Vector2(-300, -50), FacingLeft = false },
                new FormationSlot { Index = 3, Position = new Vector2(-300, -150), FacingLeft = false },
            }
        };

        /// <summary>
        /// 敌方1人站位配置
        /// </summary>
        [Header("敌方1人站位")]
        public FormationSide EnemyFormation1 = new FormationSide
        {
            Name = "敌方1人",
            Slots = new FormationSlot[]
            {
                new FormationSlot { Index = 0, Position = new Vector2(300, 0), FacingLeft = true },
            }
        };

        /// <summary>
        /// 敌方2人站位配置
        /// </summary>
        [Header("敌方2人站位")]
        public FormationSide EnemyFormation2 = new FormationSide
        {
            Name = "敌方2人",
            Slots = new FormationSlot[]
            {
                new FormationSlot { Index = 0, Position = new Vector2(300, 75), FacingLeft = true },
                new FormationSlot { Index = 1, Position = new Vector2(300, -75), FacingLeft = true },
            }
        };

        /// <summary>
        /// 敌方3人站位配置
        /// </summary>
        [Header("敌方3人站位")]
        public FormationSide EnemyFormation3 = new FormationSide
        {
            Name = "敌方3人",
            Slots = new FormationSlot[]
            {
                new FormationSlot { Index = 0, Position = new Vector2(300, 100), FacingLeft = true },
                new FormationSlot { Index = 1, Position = new Vector2(300, 0), FacingLeft = true },
                new FormationSlot { Index = 2, Position = new Vector2(300, -100), FacingLeft = true },
            }
        };

        /// <summary>
        /// 敌方4人站位配置
        /// </summary>
        [Header("敌方4人站位")]
        public FormationSide EnemyFormation4 = new FormationSide
        {
            Name = "敌方4人",
            Slots = new FormationSlot[]
            {
                new FormationSlot { Index = 0, Position = new Vector2(300, 150), FacingLeft = true },
                new FormationSlot { Index = 1, Position = new Vector2(300, 50), FacingLeft = true },
                new FormationSlot { Index = 2, Position = new Vector2(300, -50), FacingLeft = true },
                new FormationSlot { Index = 3, Position = new Vector2(300, -150), FacingLeft = true },
            }
        };

        /// <summary>
        /// 获取玩家方站位
        /// </summary>
        /// <param name="index">站位索引（0-3）</param>
        /// <returns>站位数据</returns>
        public FormationSlot GetPlayerSlot(int index)
        {
            return PlayerFormation.GetSlot(index);
        }

        /// <summary>
        /// 根据敌人数量获取对应的敌方站位配置
        /// </summary>
        /// <param name="enemyCount">敌人数量（1-4）</param>
        /// <returns>站位配置</returns>
        public FormationSide GetEnemyFormation(int enemyCount)
        {
            return enemyCount switch
            {
                1 => EnemyFormation1,
                2 => EnemyFormation2,
                3 => EnemyFormation3,
                4 => EnemyFormation4,
                _ => EnemyFormation4
            };
        }

        /// <summary>
        /// 获取敌方站位
        /// </summary>
        /// <param name="enemyCount">敌人总数（1-4）</param>
        /// <param name="index">站位索引</param>
        /// <returns>站位数据</returns>
        public FormationSlot GetEnemySlot(int enemyCount, int index)
        {
            return GetEnemyFormation(enemyCount).GetSlot(index);
        }

        /// <summary>
        /// 获取玩家方站位数量
        /// </summary>
        public int PlayerSlotCount => PlayerFormation.SlotCount;

        /// <summary>
        /// 重置为默认站位
        /// </summary>
        public void ResetToDefault()
        {
            PlayerFormation = new FormationSide
            {
                Name = "玩家方",
                Slots = new FormationSlot[]
                {
                    new FormationSlot { Index = 0, Position = new Vector2(-300, 150), FacingLeft = false },
                    new FormationSlot { Index = 1, Position = new Vector2(-300, 50), FacingLeft = false },
                    new FormationSlot { Index = 2, Position = new Vector2(-300, -50), FacingLeft = false },
                    new FormationSlot { Index = 3, Position = new Vector2(-300, -150), FacingLeft = false },
                }
            };

            EnemyFormation1 = new FormationSide
            {
                Name = "敌方1人",
                Slots = new FormationSlot[]
                {
                    new FormationSlot { Index = 0, Position = new Vector2(300, 0), FacingLeft = true },
                }
            };

            EnemyFormation2 = new FormationSide
            {
                Name = "敌方2人",
                Slots = new FormationSlot[]
                {
                    new FormationSlot { Index = 0, Position = new Vector2(300, 75), FacingLeft = true },
                    new FormationSlot { Index = 1, Position = new Vector2(300, -75), FacingLeft = true },
                }
            };

            EnemyFormation3 = new FormationSide
            {
                Name = "敌方3人",
                Slots = new FormationSlot[]
                {
                    new FormationSlot { Index = 0, Position = new Vector2(300, 100), FacingLeft = true },
                    new FormationSlot { Index = 1, Position = new Vector2(300, 0), FacingLeft = true },
                    new FormationSlot { Index = 2, Position = new Vector2(300, -100), FacingLeft = true },
                }
            };

            EnemyFormation4 = new FormationSide
            {
                Name = "敌方4人",
                Slots = new FormationSlot[]
                {
                    new FormationSlot { Index = 0, Position = new Vector2(300, 150), FacingLeft = true },
                    new FormationSlot { Index = 1, Position = new Vector2(300, 50), FacingLeft = true },
                    new FormationSlot { Index = 2, Position = new Vector2(300, -50), FacingLeft = true },
                    new FormationSlot { Index = 3, Position = new Vector2(300, -150), FacingLeft = true },
                }
            };
        }
    }
}

