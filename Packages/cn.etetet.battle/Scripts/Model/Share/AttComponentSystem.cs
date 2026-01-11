using System;
using System.Collections;

namespace ET
{
    /// <summary>
    /// AttComponent系统类 - 属性组件逻辑
    /// 遵循ET框架ECS规范：所有逻辑放在System类中
    /// </summary>
    [FriendOf(typeof(AttComponent))]
    [EntitySystemOf(typeof(AttComponent))]
    public static partial class AttComponentSystem
    {
        static AttComponentSystem()
        {
            GetSynAttFlags();
        }
        #region 生命周期方法

        [EntitySystem]
        private static void Awake(this AttComponent self, DREntityAttEntry attEntry)
        {
            self.Init(attEntry);
        }

        [EntitySystem]
        private static void Destroy(this AttComponent self)
        {
            self.Reset();
        }

        #endregion

        #region 静态初始化

        /// <summary>
        /// 初始化同步属性配置
        /// </summary>
        [StaticField]
        private static BitArray s_SynAttFlags = GetSynAttFlags();

        /// <summary>
        /// 获取同步属性标志
        /// </summary>
        public static BitArray GetSynAttFlags()
        {
            if (s_SynAttFlags == null)
            {
                s_SynAttFlags = new BitArray((int)EAttType.End);
                s_SynAttFlags[(int)EAttType.CurHP] = true;
                s_SynAttFlags[(int)EAttType.MaxHP] = true;
                s_SynAttFlags[(int)EAttType.MaxHPBase] = true;
                s_SynAttFlags[(int)EAttType.MaxHPBasePct] = true;
                s_SynAttFlags[(int)EAttType.MaxHPFlat] = true;
                s_SynAttFlags[(int)EAttType.MaxHPPct] = true;
                s_SynAttFlags[(int)EAttType.CurShield] = true;
                s_SynAttFlags[(int)EAttType.NumLives] = true;

                // 初始化最小属性值
                InitMinAttData();
            }
            return s_SynAttFlags;
        }

        /// <summary>
        /// 初始化最小属性值
        /// </summary>
        private static void InitMinAttData()
        {
            AttComponent.MinAttData[(int)EAttType.CurHP] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.AttackMeleePct] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.AttackMagicPct] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.DefenceMeleePct] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.DefenceMagicPct] = int.MinValue;

            AttComponent.MinAttData[(int)EAttType.PctDmgInc] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.PctDmgDec] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.PVPPctDmgInc] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.PVPPctDmgDec] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.MeleeDmgInc] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.MagicDmgInc] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.MeleeDmgDec] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.MagicDmgDec] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.PctHealDoneInc] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.PctHealTakenInc] = int.MinValue;

            AttComponent.MinAttData[(int)EAttType.ClassDone1] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.ClassDone2] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.ClassDone3] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.ClassDone4] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.ClassDone5] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.ClassDone6] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.ClassDone7] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.BossDone] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.ClassTaken1] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.ClassTaken2] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.ClassTaken3] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.ClassTaken4] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.ClassTaken5] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.ClassTaken6] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.ClassTaken7] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.BossTaken] = int.MinValue;

            AttComponent.MinAttData[(int)EAttType.SchoolDone1] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.SchoolDone2] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.SchoolDone3] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.SchoolDone4] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.SchoolDone5] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.SchoolDone6] = int.MinValue;

            AttComponent.MinAttData[(int)EAttType.SchoolTaken1] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.SchoolTaken2] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.SchoolTaken3] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.SchoolTaken4] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.SchoolTaken5] = int.MinValue;
            AttComponent.MinAttData[(int)EAttType.SchoolTaken6] = int.MinValue;
        }

        #endregion

        #region 重置方法

        /// <summary>
        /// 重置属性数据
        /// </summary>
        public static void Reset(this AttComponent self)
        {
            Array.Clear(self.AttData, 0, self.AttData.Length);
            self.RecalLock = false;
        }

        #endregion

        #region 锁定/解锁属性计算

        /// <summary>
        /// 锁定属性最终值计算
        /// </summary>
        public static void LockAttCal(this AttComponent self)
        {
            self.RecalLock = true;
        }

        /// <summary>
        /// 解锁属性最终值计算
        /// </summary>
        public static void UnLockAttCal(this AttComponent self)
        {
            self.RecalLock = false;

            // 计算攻防需最终值
            for (EAttType i = EAttType.AStart; i < EAttType.AEnd; i += 5)
            {
                int nValue = (int)((self.AttData[(int)i + 1] *
                        (1.0f + self.AttData[(int)i + 2] / 10000.0f) +
                        self.AttData[(int)i + 3]) *
                    (1.0f + self.AttData[(int)i + 4] / 10000.0f));

                self.AttData[(int)i] = nValue;
            }
        }

        #endregion

        #region 血量相关

        /// <summary>
        /// 重置当前血量为最大血量
        /// </summary>
        public static void ResetCurHp(this AttComponent self)
        {
            self.AttData[(int)EAttType.CurHP] = self.AttData[(int)EAttType.MaxHP];
        }
        
        /// <summary>
        /// 设置当前血量
        /// </summary>
        public static void SetCurHP(this AttComponent self, int value)
        {
            int nOldValue = self.AttData[(int)EAttType.CurHP];

            if (value == nOldValue)
                return;

            self.AttData[(int)EAttType.CurHP] = value;

            if (self.RecalLock)
                return;

            self.OnAttChange(EAttType.CurHP, nOldValue);
        }

        #endregion

        #region 初始化方法

        /// <summary>
        /// 从配置初始化属性
        /// </summary>
        public static void Init(this AttComponent self, DREntityAttEntry entry)
        {
            self.LockAttCal();
            self.InitBase(entry);
            self.UnLockAttCal();
            self.ResetCurHp();
        }

        /// <summary>
        /// 初始化基础属性
        /// </summary>
        public static void InitBase(this AttComponent self, DREntityAttEntry attEntry)
        {
            self.AttData[(int)EAttType.AttackMeleeBase] = attEntry.MeleeAttack;
            self.AttData[(int)EAttType.AttackMagicBase] = attEntry.MagicAttack;
            self.AttData[(int)EAttType.DefenceMeleeBase] = attEntry.MeleeDefence;
            self.AttData[(int)EAttType.DefenceMagicBase] = attEntry.MagicDefence;
            self.AttData[(int)EAttType.MaxHPBase] = attEntry.MaxHP;
            self.AttData[(int)EAttType.Speed] = attEntry.Speed;
            self.AttData[(int)EAttType.Crit] = attEntry.Crit;
            self.AttData[(int)EAttType.Resilience] = attEntry.Resilience;
            self.AttData[(int)EAttType.Block] = attEntry.Block;
            self.AttData[(int)EAttType.Broken] = attEntry.Broken;
            self.AttData[(int)EAttType.NumLives] = attEntry.NumLives;
            self.AttData[(int)EAttType.PctDmgInc] = attEntry.PctDmgInc;
            self.AttData[(int)EAttType.PctDmgDec] = attEntry.PctDmgDec;
            self.AttData[(int)EAttType.PctHealDoneInc] = attEntry.PctHealInc;
            self.AttData[(int)EAttType.StrikeBack] = attEntry.StrikeBack;
            self.AttData[(int)EAttType.JoinAttack] = attEntry.JoinAttack;

            self.AttData[(int)EAttType.SchoolTaken1] = attEntry.SchoolTaken1;
            self.AttData[(int)EAttType.SchoolTaken2] = attEntry.SchoolTaken2;
            self.AttData[(int)EAttType.SchoolTaken3] = attEntry.SchoolTaken3;
            self.AttData[(int)EAttType.SchoolTaken4] = attEntry.SchoolTaken4;
            self.AttData[(int)EAttType.SchoolTaken5] = attEntry.SchoolTaken5;
            self.AttData[(int)EAttType.SchoolTaken6] = attEntry.SchoolTaken6;
        }

        #endregion

        #region 属性获取/设置

        /// <summary>
        /// 设置基础属性
        /// </summary>
        public static void SetBaseAtt(this AttComponent self, int index, int value)
        {
            if (value < 0)
                value = 0;

            var old = self.AttData[index];
            if (old == value)
                return;

            self.AttData[index] = value;

            if (self.RecalLock)
                return;

            self.OnAttChange((EAttType)index, old);
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        public static int GetAttValue(this AttComponent self, int index)
        {
            return self.AttData[index];
        }

        /// <summary>
        /// 获取属性值（枚举版本）
        /// </summary>
        public static int GetAttValue(this AttComponent self, EAttType index)
        {
            return self.GetAttValue((int)index);
        }

        /// <summary>
        /// 修改属性值
        /// </summary>
        public static void ModAttValue(this AttComponent self, int index, int value)
        {
            if (value == 0) return;

            if (index >= (int)EAttType.Start && index < (int)EAttType.End)
            {
                var old = self.AttData[index];

                self.AttData[index] += value;

                if (self.AttData[index] < AttComponent.MinAttData[index])
                    self.AttData[index] = AttComponent.MinAttData[index];

                if (self.RecalLock)
                    return;

                self.OnAttChange((EAttType)index, old);
            }
        }

        /// <summary>
        /// 修改属性值（枚举版本）
        /// </summary>
        public static void ModAttValue(this AttComponent self, EAttType index, int value)
        {
            self.ModAttValue((int)index, value);
        }

        #endregion

        #region 属性变化事件

        /// <summary>
        /// 属性变化事件处理
        /// </summary>
        private static void OnAttChange(this AttComponent self, EAttType index, int oldValue = int.MaxValue)
        {
            switch (index)
            {
                case EAttType.MaxHP:
                {
                    if (self.GetAttValue(EAttType.CurHP) > self.GetAttValue(EAttType.MaxHP))
                    {
                        self.AttData[(int)EAttType.CurHP] = Math.Min(self.GetAttValue(EAttType.CurHP),
                            self.GetAttValue(EAttType.MaxHP));
                    }
                }
                break;

                case EAttType.CurHP:
                {
                    if (self.AttData[(int)EAttType.CurHP] <= 0)
                    {
                        while (self.AttData[(int)EAttType.NumLives] > 0)
                        {
                            self.AttData[(int)EAttType.NumLives] -= 1;
                            self.AttData[(int)EAttType.CurHP] += self.GetAttValue(EAttType.MaxHP);

                            if (self.AttData[(int)EAttType.CurHP] > 0)
                                return;
                        }

                        self.AttData[(int)EAttType.CurHP] = 0;
                    }

                    self.OnAttChange(EAttType.MaxHP);
                }
                break;

                case EAttType.AttackMeleeBase:
                case EAttType.AttackMeleeBasePct:
                case EAttType.AttackMeleeFlat:
                case EAttType.AttackMeleePct:
                {
                    int nValue = (int)((self.AttData[(int)EAttType.AttackMeleeBase] *
                            (1.0f + self.AttData[(int)EAttType.AttackMeleeBasePct] / 10000.0f) +
                            self.AttData[(int)EAttType.AttackMeleeFlat]) *
                        (1.0f + self.AttData[(int)EAttType.AttackMeleePct] / 10000.0f));

                    self.AttData[(int)EAttType.AttackMelee] = nValue;
                }
                break;

                case EAttType.AttackMagicBase:
                case EAttType.AttackMagicBasePct:
                case EAttType.AttackMagicFlat:
                case EAttType.AttackMagicPct:
                {
                    int nValue = (int)((self.AttData[(int)EAttType.AttackMagicBase] *
                            (1.0f + self.AttData[(int)EAttType.AttackMagicBasePct] / 10000.0f) +
                            self.AttData[(int)EAttType.AttackMagicFlat]) *
                        (1.0f + self.AttData[(int)EAttType.AttackMagicPct] / 10000.0f));

                    self.AttData[(int)EAttType.AttackMagic] = nValue;
                }
                break;

                case EAttType.DefenceMeleeBase:
                case EAttType.DefenceMeleeBasePct:
                case EAttType.DefenceMeleeFlat:
                case EAttType.DefenceMeleePct:
                {
                    int nValue = (int)((self.AttData[(int)EAttType.DefenceMeleeBase] *
                            (1.0f + self.AttData[(int)EAttType.DefenceMeleeBasePct] / 10000.0f) +
                            self.AttData[(int)EAttType.DefenceMeleeFlat]) *
                        (1.0f + self.AttData[(int)EAttType.DefenceMeleePct] / 10000.0f));

                    self.AttData[(int)EAttType.DefenceMelee] = nValue;
                }
                break;

                case EAttType.DefenceMagicBase:
                case EAttType.DefenceMagicBasePct:
                case EAttType.DefenceMagicFlat:
                case EAttType.DefenceMagicPct:
                {
                    int nValue = (int)((self.AttData[(int)EAttType.DefenceMagicBase] *
                            (1.0f + self.AttData[(int)EAttType.DefenceMagicBasePct] / 10000.0f) +
                            self.AttData[(int)EAttType.DefenceMagicFlat]) *
                        (1.0f + self.AttData[(int)EAttType.DefenceMagicPct] / 10000.0f));

                    self.AttData[(int)EAttType.DefenceMagic] = nValue;
                }
                break;

                case EAttType.MaxHPBase:
                case EAttType.MaxHPBasePct:
                case EAttType.MaxHPFlat:
                case EAttType.MaxHPPct:
                {
                    int nValue = (int)((self.AttData[(int)EAttType.MaxHPBase] *
                            (1.0f + self.AttData[(int)EAttType.MaxHPBasePct] / 10000.0f) +
                            self.AttData[(int)EAttType.MaxHPFlat]) *
                        (1.0f + self.AttData[(int)EAttType.MaxHPPct] / 10000.0f));

                    var old = self.AttData[(int)EAttType.MaxHP];
                    self.AttData[(int)EAttType.MaxHP] = nValue;

                    self.OnAttChange(EAttType.MaxHP, old);
                }
                break;
            }
        }

        #endregion
    }
}
