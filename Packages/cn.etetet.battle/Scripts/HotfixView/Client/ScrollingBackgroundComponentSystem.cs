using System;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 滚动背景组件系统
    /// 提供滚动控制API
    /// </summary>
    [FriendOf(typeof(ScrollingBackgroundComponent))]
    [EntitySystemOf(typeof(ScrollingBackgroundComponent))]
    public static partial class ScrollingBackgroundComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ScrollingBackgroundComponent self)
        {
            self.CurrentPosition = 0f;
            self.TargetPosition = -1f;
            self.IsScrolling = false;
            self.ScrollSpeed = 0.1f;
        }
        
        [EntitySystem]
        private static void Destroy(this ScrollingBackgroundComponent self)
        {
            self.StopScrolling();
            self.Controller = null;
        }
        
        /// <summary>
        /// 初始化控制器
        /// </summary>
        /// <param name="self">组件实例</param>
        /// <param name="controller">MonoBehaviour控制器</param>
        public static void Initialize(this ScrollingBackgroundComponent self, ScrollingBackground controller)
        {
            self.Controller = controller;
            if (controller != null)
            {
                self.CurrentPosition = controller.CurrentPosition;
                self.IsScrolling = controller.IsScrolling;
            }
        }
        
        /// <summary>
        /// 开始无限滚动
        /// </summary>
        /// <param name="self">组件实例</param>
        /// <param name="speed">滚动速度</param>
        public static void StartScrolling(this ScrollingBackgroundComponent self, float speed = 0.1f)
        {
            if (self.Controller == null)
            {
                Log.Warning("[ScrollingBackground] Controller未初始化");
                return;
            }
            
            self.ScrollSpeed = speed;
            self.IsScrolling = true;
            self.TargetPosition = -1f;
            self.Controller.StartInfinite(speed);
        }
        
        /// <summary>
        /// 停止滚动
        /// </summary>
        /// <param name="self">组件实例</param>
        public static void StopScrolling(this ScrollingBackgroundComponent self)
        {
            self.IsScrolling = false;
            if (self.Controller != null)
            {
                self.Controller.StopScrolling();
                self.CurrentPosition = self.Controller.CurrentPosition;
            }
        }
        
        /// <summary>
        /// 移动到指定位置（异步）
        /// </summary>
        /// <param name="self">组件实例</param>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="speed">滚动速度</param>
        /// <returns>到达目标位置时完成</returns>
        public static async ETTask MoveToAsync(this ScrollingBackgroundComponent self, float targetPosition, float speed = 0.1f)
        {
            if (self.Controller == null)
            {
                Log.Warning("[ScrollingBackground] Controller未初始化");
                return;
            }
            
            // 如果目标位置在当前位置之前，直接返回
            if (targetPosition <= self.Controller.CurrentPosition)
            {
                return;
            }
            
            // 创建EntityRef用于await后安全使用
            EntityRef<ScrollingBackgroundComponent> selfRef = self;
            
            self.TargetPosition = targetPosition;
            self.ScrollSpeed = speed;
            self.IsScrolling = true;
            
            // 创建等待任务
            ETTask tcs = ETTask.Create(true);
            
            // 注册到达回调
            Action onReached = null;
            onReached = () =>
            {
                // await后重新获取Entity
                ScrollingBackgroundComponent component = selfRef;
                if (component != null && component.Controller != null)
                {
                    component.Controller.OnReachedTarget -= onReached;
                    component.IsScrolling = false;
                    component.CurrentPosition = component.Controller.CurrentPosition;
                }
                tcs.SetResult();
            };
            
            self.Controller.OnReachedTarget += onReached;
            self.Controller.MoveTo(targetPosition, speed);
            
            await tcs;
        }
        
        /// <summary>
        /// 获取当前位置
        /// </summary>
        /// <param name="self">组件实例</param>
        /// <returns>当前虚拟位置</returns>
        public static float GetCurrentPosition(this ScrollingBackgroundComponent self)
        {
            if (self.Controller != null)
            {
                self.CurrentPosition = self.Controller.CurrentPosition;
            }
            return self.CurrentPosition;
        }
        
        /// <summary>
        /// 设置当前位置（不触发滚动）
        /// </summary>
        /// <param name="self">组件实例</param>
        /// <param name="position">新位置</param>
        public static void SetPosition(this ScrollingBackgroundComponent self, float position)
        {
            self.CurrentPosition = position;
            if (self.Controller != null)
            {
                self.Controller.SetPosition(position);
            }
        }
        
        /// <summary>
        /// 重置到初始状态
        /// </summary>
        /// <param name="self">组件实例</param>
        public static void Reset(this ScrollingBackgroundComponent self)
        {
            self.CurrentPosition = 0f;
            self.TargetPosition = -1f;
            self.IsScrolling = false;
            
            if (self.Controller != null)
            {
                self.Controller.Reset();
            }
        }
        
        /// <summary>
        /// 同步状态到Entity
        /// </summary>
        /// <param name="self">组件实例</param>
        public static void SyncState(this ScrollingBackgroundComponent self)
        {
            if (self.Controller != null)
            {
                self.CurrentPosition = self.Controller.CurrentPosition;
                self.IsScrolling = self.Controller.IsScrolling;
                self.TargetPosition = self.Controller.TargetPosition;
            }
        }
    }
}
