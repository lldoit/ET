using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  LL
    /// Date    2026.1.8
    /// Desc    确认弹窗面板系统
    /// </summary>
    [FriendOf(typeof(ConfirmPopupPanelComponent))]
    public static partial class ConfirmPopupPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this ConfirmPopupPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ConfirmPopupPanelComponent self)
        {
            self.ConfirmSource = null;
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this ConfirmPopupPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        /// <summary>
        /// 带参数打开确认弹窗
        /// </summary>
        /// <param name="title">弹窗标题</param>
        /// <param name="content">弹窗内容</param>
        /// <param name="source">确认来源标识，用于在确认后发布对应事件</param>
        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this ConfirmPopupPanelComponent self, string title, string content, string source)
        {
            // 设置标题和内容
            self.u_DataTitle.SetValue(title);
            self.u_DataContent.SetValue(content);
            
            // 保存确认来源
            self.ConfirmSource = source;
            
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(ConfirmPopupPanelComponent.OnEventOkInvoke)]
        private static async ETTask OnEventOkInvoke(this ConfirmPopupPanelComponent self)
        {
            // 发布确认事件
            string source = self.ConfirmSource;
            self.ConfirmSource = null;
            
            if (!string.IsNullOrEmpty(source))
            {
                EventSystem.Instance.Publish(self.Root().CurrentScene(), new ConfirmPopupConfirmedEvent { Source = source });
            }
            
            // 关闭面板
            await self.YIUIMgr().ClosePanelAsync<ConfirmPopupPanelComponent>();
        }
        
        [YIUIInvoke(ConfirmPopupPanelComponent.OnEventCancelInvoke)]
        private static void OnEventCancelInvoke(this ConfirmPopupPanelComponent self)
        {
            // 清理来源
            self.ConfirmSource = null;
            
            // 关闭面板
            self.YIUIMgr().ClosePanel<ConfirmPopupPanelComponent>();
        }
        #endregion YIUIEvent结束
    }
}
