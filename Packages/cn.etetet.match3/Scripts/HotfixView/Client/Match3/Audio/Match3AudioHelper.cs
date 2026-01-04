namespace ET.Client
{
    /// <summary>
    /// 三消游戏音效辅助类
    /// 提供便捷的静态方法来播放各种游戏音效
    /// </summary>
    public static class Match3AudioHelper
    {
        /// <summary>
        /// 获取SoundComponent
        /// </summary>
        private static SoundComponent GetSoundComponent(Scene scene)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp == null)
            {
                soundComp = scene.AddComponent<SoundComponent>();
                soundComp.SetMusicVolume(0.7f);
                soundComp.SetSoundVolume(0.8f);
            }
            return soundComp;
        }
        
        #region 背景音乐
        
        /// <summary>
        /// 播放主菜单音乐
        /// </summary>
        public static async ETTask PlayMainMenuMusic(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            await soundComp.PlayMusicWithFade(Match3SoundType.BGM_MainMenu, 
                fadeOutDuration: 1.0f, 
                fadeInDuration: 2.0f);
        }
        
        /// <summary>
        /// 播放游戏音乐
        /// </summary>
        public static async ETTask PlayGameMusic(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            await soundComp.PlayMusicWithFade(Match3SoundType.BGM_Game, 
                fadeOutDuration: 1.0f, 
                fadeInDuration: 1.5f);
        }
        
        /// <summary>
        /// 播放胜利音乐
        /// </summary>
        public static async ETTask PlayVictoryMusic(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            await soundComp.PlayMusicWithFade(Match3SoundType.BGM_Victory, 
                fadeOutDuration: 0.5f, 
                fadeInDuration: 2.0f, 
                loop: false);
        }
        
        /// <summary>
        /// 播放失败音乐
        /// </summary>
        public static async ETTask PlayDefeatMusic(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            await soundComp.PlayMusicWithFade(Match3SoundType.BGM_Defeat, 
                fadeOutDuration: 0.5f, 
                fadeInDuration: 1.5f, 
                loop: false);
        }
        
        /// <summary>
        /// 停止音乐（带淡出）
        /// </summary>
        public static async ETTask StopMusic(Scene scene)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            if (soundComp != null)
            {
                await soundComp.StopMusicWithFade(1.0f);
            }
        }
        
        /// <summary>
        /// 暂停音乐
        /// </summary>
        public static void PauseMusic(Scene scene)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            soundComp?.PauseMusic();
        }
        
        /// <summary>
        /// 恢复音乐
        /// </summary>
        public static void ResumeMusic(Scene scene)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            soundComp?.ResumeMusic();
        }
        
        #endregion
        
        #region UI音效
        
        /// <summary>
        /// 播放按钮点击音效
        /// </summary>
        public static void PlayButtonClick(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_ButtonClick).NoContext();
        }
        
        /// <summary>
        /// 播放面板打开音效
        /// </summary>
        public static void PlayPanelOpen(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_PanelOpen).NoContext();
        }
        
        /// <summary>
        /// 播放面板关闭音效
        /// </summary>
        public static void PlayPanelClose(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_PanelClose).NoContext();
        }
        
        #endregion
        
        #region 游戏音效
        
        /// <summary>
        /// 播放瓦片交换音效
        /// </summary>
        public static void PlayTileSwap(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_TileSwap).NoContext();
        }
        
        /// <summary>
        /// 播放瓦片交换失败音效
        /// </summary>
        public static void PlayTileSwapFailed(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_TileSwapFailed).NoContext();
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
            
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(soundAddress).NoContext();
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
            
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(soundAddress).NoContext();
        }
        
        #endregion
        
        #region 特殊糖果音效
        
        /// <summary>
        /// 播放特殊糖果创建音效
        /// </summary>
        public static void PlaySpecialCandyCreate(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_SpecialCandyCreate).NoContext();
        }
        
        /// <summary>
        /// 播放条纹糖果激活音效
        /// </summary>
        public static void PlayStripedCandySound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_StripedCandy).NoContext();
        }
        
        /// <summary>
        /// 播放包装糖果激活音效
        /// </summary>
        public static void PlayWrappedCandySound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_WrappedCandy).NoContext();
        }
        
        /// <summary>
        /// 播放彩色炸弹激活音效
        /// </summary>
        public static void PlayColorBombSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_ColorBomb).NoContext();
        }
        
        #endregion
        
        #region 道具音效
        
        /// <summary>
        /// 播放棒棒糖道具音效
        /// </summary>
        public static void PlayBoosterLollipopSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_BoosterLollipop).NoContext();
        }
        
        /// <summary>
        /// 播放炸弹道具音效
        /// </summary>
        public static void PlayBoosterBombSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_BoosterBomb).NoContext();
        }
        
        /// <summary>
        /// 播放交换道具音效
        /// </summary>
        public static void PlayBoosterSwitchSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_BoosterSwitch).NoContext();
        }
        
        /// <summary>
        /// 播放彩色炸弹道具音效
        /// </summary>
        public static void PlayBoosterColorBombSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_BoosterColorBomb).NoContext();
        }
        
        #endregion
        
        #region 障碍物音效
        
        /// <summary>
        /// 播放巧克力破碎音效
        /// </summary>
        public static void PlayChocolateBreakSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_ChocolateBreak).NoContext();
        }
        
        /// <summary>
        /// 播放棉花糖破碎音效
        /// </summary>
        public static void PlayMarshmallowBreakSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_MarshmallowBreak).NoContext();
        }
        
        /// <summary>
        /// 播放冰块破碎音效
        /// </summary>
        public static void PlayIceBreakSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_IceBreak).NoContext();
        }
        
        #endregion
        
        #region 收集物音效
        
        /// <summary>
        /// 播放收集物收集音效
        /// </summary>
        public static void PlayCollectableCollectSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_CollectableCollect).NoContext();
        }
        
        #endregion
        
        #region 游戏事件音效
        
        /// <summary>
        /// 播放关卡开始音效
        /// </summary>
        public static void PlayLevelStartSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_LevelStart).NoContext();
        }
        
        /// <summary>
        /// 播放关卡完成音效
        /// </summary>
        public static void PlayLevelCompleteSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_LevelComplete).NoContext();
        }
        
        /// <summary>
        /// 播放关卡失败音效
        /// </summary>
        public static void PlayLevelFailedSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_LevelFailed).NoContext();
        }
        
        /// <summary>
        /// 播放获得星星音效
        /// </summary>
        public static void PlayStarEarnedSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_StarEarned).NoContext();
        }
        
        /// <summary>
        /// 播放无可用移动音效
        /// </summary>
        public static void PlayNoMovesLeftSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_NoMovesLeft).NoContext();
        }
        
        /// <summary>
        /// 播放新高分音效
        /// </summary>
        public static void PlayNewHighScoreSound(Scene scene)
        {
            SoundComponent soundComp = GetSoundComponent(scene);
            soundComp.PlaySound(Match3SoundType.SFX_NewHighScore).NoContext();
        }
        
        #endregion
        
        #region 音量控制
        
        /// <summary>
        /// 设置音乐音量
        /// </summary>
        public static void SetMusicVolume(Scene scene, float volume)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            soundComp?.SetMusicVolume(volume);
        }
        
        /// <summary>
        /// 设置音效音量
        /// </summary>
        public static void SetSoundVolume(Scene scene, float volume)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            soundComp?.SetSoundVolume(volume);
        }
        
        /// <summary>
        /// 停止所有音效
        /// </summary>
        public static void StopAllSounds(Scene scene)
        {
            SoundComponent soundComp = scene.GetComponent<SoundComponent>();
            soundComp?.StopAllSounds();
        }
        
        #endregion
    }
}


