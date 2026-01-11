namespace ET.Client
{
    /// <summary>
    /// 三消游戏音效辅助类
    /// </summary>
    public static class Match3AudioHelper
    {
        #region 游戏音效
        
        /// <summary>
        /// 播放瓦片交换失败音效
        /// </summary>
        public static void PlayTileSwapFailed(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_TileSwapFailed);
        }
        
        /// <summary>
        /// 播放匹配音效（根据匹配数量）
        /// </summary>
        public static void PlayMatchSound(Scene scene, int matchCount)
        {
            string soundAddress = matchCount switch
            {
                3 => Match3SoundType.SFX_Match3,
                4 => Match3SoundType.SFX_Match4,
                5 => Match3SoundType.SFX_Match5,
                _ => Match3SoundType.SFX_MatchSpecial
            };
            
            AudioHelper.PlaySoundQuick(scene.Root(), soundAddress);
        }
        
        /// <summary>
        /// 播放Combo音效
        /// </summary>
        public static void PlayComboSound(Scene scene, int comboCount)
        {
            string soundAddress = comboCount switch
            {
                1 => Match3SoundType.SFX_Combo1,
                2 => Match3SoundType.SFX_Combo2,
                3 => Match3SoundType.SFX_Combo3,
                4 => Match3SoundType.SFX_Combo4,
                _ => Match3SoundType.SFX_Combo5Plus
            };
            
            AudioHelper.PlaySoundQuick(scene.Root(), soundAddress);
        }
        
        #endregion
        
        #region 特殊糖果音效
        
        /// <summary>
        /// 播放特殊糖果创建音效
        /// </summary>
        public static void PlaySpecialCandyCreate(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_SpecialCandyCreate);
        }
        
        /// <summary>
        /// 播放条纹糖果激活音效
        /// </summary>
        public static void PlayStripedCandySound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_StripedCandy);
        }
        
        /// <summary>
        /// 播放包装糖果激活音效
        /// </summary>
        public static void PlayWrappedCandySound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_WrappedCandy);
        }
        
        /// <summary>
        /// 播放彩色炸弹激活音效
        /// </summary>
        public static void PlayColorBombSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_ColorBomb);
        }
        
        #endregion
        
        #region 道具音效
        
        /// <summary>
        /// 播放棒棒糖道具音效
        /// </summary>
        public static void PlayBoosterLollipopSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_BoosterLollipop);
        }
        
        /// <summary>
        /// 播放炸弹道具音效
        /// </summary>
        public static void PlayBoosterBombSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_BoosterBomb);
        }
        
        /// <summary>
        /// 播放交换道具音效
        /// </summary>
        public static void PlayBoosterSwitchSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_BoosterSwitch);
        }
        
        /// <summary>
        /// 播放彩色炸弹道具音效
        /// </summary>
        public static void PlayBoosterColorBombSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_BoosterColorBomb);
        }
        
        #endregion
        
        #region 障碍物音效
        
        /// <summary>
        /// 播放巧克力破碎音效
        /// </summary>
        public static void PlayChocolateBreakSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_ChocolateBreak);
        }
        
        /// <summary>
        /// 播放棉花糖破碎音效
        /// </summary>
        public static void PlayMarshmallowBreakSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_MarshmallowBreak);
        }
        
        /// <summary>
        /// 播放冰块破碎音效
        /// </summary>
        public static void PlayIceBreakSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_IceBreak);
        }
        
        #endregion
        
        #region 收集物音效
        
        /// <summary>
        /// 播放收集物收集音效
        /// </summary>
        public static void PlayCollectableCollectSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_CollectableCollect);
        }
        
        #endregion
        
        #region 游戏事件音效
        
        /// <summary>
        /// 播放关卡开始音效
        /// </summary>
        public static void PlayLevelStartSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_LevelStart);
        }
        
        /// <summary>
        /// 播放关卡完成音效
        /// </summary>
        public static void PlayLevelCompleteSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_LevelComplete);
        }
        
        /// <summary>
        /// 播放关卡失败音效
        /// </summary>
        public static void PlayLevelFailedSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_LevelFailed);
        }
        
        /// <summary>
        /// 播放获得星星音效
        /// </summary>
        public static void PlayStarEarnedSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_StarEarned);
        }
        
        /// <summary>
        /// 播放无可用移动音效
        /// </summary>
        public static void PlayNoMovesLeftSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_NoMovesLeft);
        }
        
        /// <summary>
        /// 播放新高分音效
        /// </summary>
        public static void PlayNewHighScoreSound(Scene scene)
        {
            AudioHelper.PlaySoundQuick(scene.Root(), Match3SoundType.SFX_NewHighScore);
        }
        
        #endregion
    }
}
