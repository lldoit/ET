using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 三消游戏输入处理系统
    /// 使用IUpdate每帧检测玩家输入，分发到道具使用或普通交换逻辑
    /// 仅支持UI渲染模式
    /// </summary>
    [FriendOf(typeof(Match3InputComponent))]
    [FriendOf(typeof(Match3BoardComponent))]
    [FriendOf(typeof(BoosterManagerComponent))]
    [EntitySystemOf(typeof(Match3InputComponent))]
    public static partial class Match3InputComponentSystem
    {
        [EntitySystem]
        private static void Awake(this Match3InputComponent self)
        {
            self.IsDragging = false;
            self.DragStartX = -1;
            self.DragStartY = -1;
            self.TileSize = 1.0f;
            self.MinDragDistance = 0.3f;
            self.InputEnabled = true;
        }

        [EntitySystem]
        private static void Update(this Match3InputComponent self)
        {
            if (!self.InputEnabled)
            {
                return;
            }

            var board = self.GetParent<Match3BoardComponent>();
            if (board == null)
            {
                return;
            }

            // 检查棋盘是否锁定
            if (board.InputLocked || board.CurrentlySwapping || board.CurrentlyAwarding)
            {
                return;
            }

            // 处理鼠标/触摸输入
            self.HandleMouseInput(board);
        }

        /// <summary>
        /// 处理鼠标输入
        /// </summary>
        private static void HandleMouseInput(this Match3InputComponent self, Match3BoardComponent board)
        {
            // 鼠标按下
            if (Input.GetMouseButtonDown(0))
            {
                self.OnPointerDownUI(board);
            }
            // 鼠标释放
            else if (Input.GetMouseButtonUp(0))
            {
                self.OnPointerUpUI(board);
            }
            // 拖拽中 - 检测是否拖到另一个瓦片
            else if (self.IsDragging && Input.GetMouseButton(0))
            {
                self.OnDraggingUI(board);
            }
        }

        /// <summary>
        /// UI模式拖拽中处理 - 检测是否拖到另一个有效瓦片
        /// </summary>
        private static void OnDraggingUI(this Match3InputComponent self, Match3BoardComponent board)
        {
            if (self.UIBoardRoot == null) return;

            Vector2 screenPos = Input.mousePosition;
            Camera camera = self.UICanvas?.renderMode == RenderMode.ScreenSpaceOverlay ? null : self.UICanvas?.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                self.UIBoardRoot, screenPos, camera, out Vector2 localPos))
            {
                return;
            }

            // 计算当前拖拽位置的瓦片坐标
            if (!self.UILocalPositionToTile(board, localPos, out int targetX, out int targetY))
            {
                return;
            }

            // 确保不是起始瓦片
            if (targetX == self.DragStartX && targetY == self.DragStartY)
            {
                return;
            }

            // 检查是否相邻（不允许对角线）
            int dx = Mathf.Abs(targetX - self.DragStartX);
            int dy = Mathf.Abs(targetY - self.DragStartY);

            if (dx > 1 || dy > 1 || (dx == 1 && dy == 1))
            {
                return;
            }

            // 检查目标瓦片是否有效
            var targetTile = board.GetTile(targetX, targetY);
            if (targetTile == null)
            {
                return;
            }

            // 拖拽到另一个有效瓦片，播放Unpressed动画
            self.PlayTileUnpressAnimation(board);

            // 停止拖拽状态
            self.IsDragging = false;

            // 尝试交换
            self.TrySwapTilesAsync(board, self.DragStartX, self.DragStartY, targetX, targetY).NoContext();
        }

        /// <summary>
        /// UI模式指针按下处理
        /// </summary>
        private static void OnPointerDownUI(this Match3InputComponent self, Match3BoardComponent board)
        {
            if (self.UIBoardRoot == null)
            {
                Log.Warning("[Match3Input] UIBoardRoot 未设置");
                return;
            }

            Vector2 screenPos = Input.mousePosition;

            // 将屏幕坐标转换为UI棋盘本地坐标
            Camera camera = self.UICanvas?.renderMode == RenderMode.ScreenSpaceOverlay ? null : self.UICanvas?.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                self.UIBoardRoot, screenPos, camera, out Vector2 localPos))
            {
                return;
            }

            // 计算瓦片坐标
            if (!self.UILocalPositionToTile(board, localPos, out int x, out int y))
            {
                // 点击空白区域时取消道具
                var boosterMgr = board.GetComponent<BoosterManagerComponent>();
                if (boosterMgr != null && boosterMgr.InSwitchMode)
                {
                    boosterMgr.DeactivateBooster();
                }
                return;
            }

            var tile = board.GetTile(x, y);
            if (tile == null) return;

            // 直接播放press动画
            self.PlayTilePressAnimation(tile, x, y);

            // 记录拖拽起点
            self.DragStartWorldPos = new Vector3(localPos.x, localPos.y, 0);

            // 检查是否有激活的道具
            var boosterManager = board.GetComponent<BoosterManagerComponent>();
            if (boosterManager != null && boosterManager.ActiveBoosterType.HasValue)
            {
                if (boosterManager.ActiveBoosterType == BoosterType.Switch)
                {
                    self.IsDragging = true;
                    self.SwitchSelectedX = x;
                    self.SwitchSelectedY = y;
                    return;
                }

                self.ApplyBoosterAtPositionAsync(board, boosterManager, x, y).NoContext();
                return;
            }

            // 无道具，进入拖拽状态
            self.IsDragging = true;
            self.DragStartX = x;
            self.DragStartY = y;
        }

        /// <summary>
        /// 播放瓦片按下动画
        /// </summary>
        private static void PlayTilePressAnimation(this Match3InputComponent self, Tile tile, int x, int y)
        {
            if (tile == null) return;

            // 记录当前按下的瓦片坐标
            self.PressedTileX = x;
            self.PressedTileY = y;

            // 普通糖果
            var candyView = tile.GetComponent<CandyViewComponent>();
            if (candyView != null)
            {
                candyView.PlayPressAnimation();
                return;
            }

            // 技能糖果
            var skillCandyView = tile.GetComponent<SkillCandyViewComponent>();
            if (skillCandyView != null)
            {
                skillCandyView.PlayPressAnimation();
                return;
            }
        }

        /// <summary>
        /// 播放瓦片松开动画
        /// </summary>
        private static void PlayTileUnpressAnimation(this Match3InputComponent self, Match3BoardComponent board)
        {
            if (self.PressedTileX < 0 || self.PressedTileY < 0) return;

            var tile = board.GetTile(self.PressedTileX, self.PressedTileY);
            if (tile == null)
            {
                self.PressedTileX = -1;
                self.PressedTileY = -1;
                return;
            }

            // 普通糖果
            var candyView = tile.GetComponent<CandyViewComponent>();
            if (candyView != null)
            {
                candyView.PlayUnpressAnimation();
            }

            // 技能糖果
            var skillCandyView = tile.GetComponent<SkillCandyViewComponent>();
            if (skillCandyView != null)
            {
                skillCandyView.PlayUnpressAnimation();
            }

            // 重置按下坐标
            self.PressedTileX = -1;
            self.PressedTileY = -1;
        }

        /// <summary>
        /// UI模式指针释放处理
        /// </summary>
        private static void OnPointerUpUI(this Match3InputComponent self, Match3BoardComponent board)
        {
            // 播放松开动画
            self.PlayTileUnpressAnimation(board);

            if (!self.IsDragging)
            {
                return;
            }

            self.IsDragging = false;

            // 检查是否是Switch道具拖拽
            var boosterManager = board.GetComponent<BoosterManagerComponent>();
            if (boosterManager != null && boosterManager.ActiveBoosterType == BoosterType.Switch)
            {
                self.HandleSwitchDragAsync(board, boosterManager).NoContext();
                return;
            }

            if (self.UIBoardRoot == null) return;

            Vector2 screenPos = Input.mousePosition;
            Camera camera = self.UICanvas?.renderMode == RenderMode.ScreenSpaceOverlay ? null : self.UICanvas?.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                self.UIBoardRoot, screenPos, camera, out Vector2 localPos))
            {
                return;
            }

            // 计算目标瓦片坐标
            if (!self.UILocalPositionToTile(board, localPos, out int targetX, out int targetY))
            {
                return;
            }

            // 确保不是同一个瓦片
            if (targetX == self.DragStartX && targetY == self.DragStartY)
            {
                return;
            }

            // 检查是否相邻（不允许对角线）
            int dx = Mathf.Abs(targetX - self.DragStartX);
            int dy = Mathf.Abs(targetY - self.DragStartY);

            if (dx > 1 || dy > 1 || (dx == 1 && dy == 1))
            {
                return;
            }

            // 尝试交换
            self.TrySwapTilesAsync(board, self.DragStartX, self.DragStartY, targetX, targetY).NoContext();
        }

        /// <summary>
        /// 处理Switch道具的拖拽交换
        /// </summary>
        private static async ETTask HandleSwitchDragAsync(this Match3InputComponent self, Match3BoardComponent board, BoosterManagerComponent boosterManager)
        {
            self.InputEnabled = false;

            try
            {
                Vector2 screenPos = Input.mousePosition;
                Camera camera = self.UICanvas?.renderMode == RenderMode.ScreenSpaceOverlay ? null : self.UICanvas?.worldCamera;

                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    self.UIBoardRoot, screenPos, camera, out Vector2 localPos))
                {
                    return;
                }

                Vector3 dragDelta = new Vector3(localPos.x, localPos.y, 0) - self.DragStartWorldPos;

                int x1 = self.SwitchSelectedX;
                int y1 = self.SwitchSelectedY;
                int x2 = x1;
                int y2 = y1;

                // 计算目标位置（只允许相邻，不允许对角线）
                if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
                {
                    x2 += dragDelta.x > 0 ? 1 : -1;
                }
                else
                {
                    y2 += dragDelta.y > 0 ? 1 : -1;
                }

                self.SwitchSelectedX = -1;
                self.SwitchSelectedY = -1;

                if (dragDelta.magnitude < self.MinDragDistance)
                {
                    return;
                }

                var targetTile = board.GetTile(x2, y2);
                if (targetTile == null)
                {
                    return;
                }

                await boosterManager.ExecuteSwitchDragWithViewAsync(board, x1, y1, x2, y2);
            }
            finally
            {
                self.InputEnabled = true;
            }
        }

        /// <summary>
        /// 应用道具到指定位置（异步）
        /// </summary>
        private static async ETTask ApplyBoosterAtPositionAsync(this Match3InputComponent self, Match3BoardComponent board, BoosterManagerComponent boosterManager, int x, int y)
        {
            self.InputEnabled = false;

            try
            {
                await boosterManager.ApplyBoosterWithViewAsync(board, x, y);
            }
            finally
            {
                self.InputEnabled = true;
            }
        }

        /// <summary>
        /// 尝试交换瓦片（异步）
        /// </summary>
        private static async ETTask TrySwapTilesAsync(this Match3InputComponent self, Match3BoardComponent board, int x1, int y1, int x2, int y2)
        {
            self.InputEnabled = false;
            bool success = false;

            try
            {
                success = await board.TrySwapTilesAsync(x1, y1, x2, y2);
            }
            finally
            {
                // 只有失败时才恢复输入
                // 成功时保持锁定，直到 TurnManager 处理完回合后发布 Match3CanEliminateEvent
                if (!success)
                {
                    self.InputEnabled = true;
                }
            }
        }

        /// <summary>
        /// UI本地坐标转瓦片坐标
        /// </summary>
        private static bool UILocalPositionToTile(this Match3InputComponent self, Match3BoardComponent board, Vector2 localPos, out int x, out int y)
        {
            x = -1;
            y = -1;

            if (!board.HasLevel) return false;

            int width = board.Level.Width;
            int height = board.Level.Height;

            float cellWidth = self.UITileSize.x + self.UITileSpacing.x;
            float cellHeight = self.UITileSize.y + self.UITileSpacing.y;

            // 计算居中偏移（与GetUITilePosition保持一致）
            float offsetX = -(width - 1) * cellWidth / 2;
            float offsetY = (height - 1) * cellHeight / 2;

            // 反向计算瓦片坐标
            x = Mathf.FloorToInt((localPos.x - offsetX + cellWidth / 2) / cellWidth);
            y = Mathf.FloorToInt((offsetY - localPos.y + cellHeight / 2) / cellHeight);

            return x >= 0 && x < width && y >= 0 && y < height;
        }

        #region 初始化方法

        /// <summary>
        /// 启用输入
        /// </summary>
        public static void EnableInput(this Match3InputComponent self)
        {
            self.InputEnabled = true;
        }

        /// <summary>
        /// 禁用输入
        /// </summary>
        public static void DisableInput(this Match3InputComponent self)
        {
            self.InputEnabled = false;
            self.IsDragging = false;
        }

        /// <summary>
        /// 初始化输入组件（UI模式）
        /// </summary>
        public static void InitializeUI(this Match3InputComponent self, RectTransform uiBoardRoot, Canvas canvas, Vector2 tileSize, Vector2 tileSpacing)
        {
            self.UIBoardRoot = uiBoardRoot;
            self.UICanvas = canvas;
            self.UITileSize = tileSize;
            self.UITileSpacing = tileSpacing;
            Log.Info($"[Match3Input] UI模式初始化完成 UIBoardRoot:{uiBoardRoot.name} TileSize:{tileSize} TileSpacing:{tileSpacing}");
        }

        #endregion
    }
}
