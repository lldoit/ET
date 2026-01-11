using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 三消游戏输入处理系统
    /// 使用IUpdate每帧检测玩家输入，分发到道具使用或普通交换逻辑
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
                self.OnPointerDown(board);
            }
            // 鼠标释放
            else if (Input.GetMouseButtonUp(0))
            {
                self.OnPointerUp(board);
            }
        }

        /// <summary>
        /// 指针按下处理
        /// 参考 CandyMatch3Kit.GameBoard.HandleInput
        /// </summary>
        private static void OnPointerDown(this Match3InputComponent self, Match3BoardComponent board)
        {
            // 获取相机
            Camera camera = self.GameCamera;
            if (camera == null)
            {
                camera = Camera.main;
            }
            if (camera == null)
            {
                Log.Warning("[Match3Input] 找不到相机");
                return;
            }
            
            // 使用 Physics2D.Raycast 检测瓦片（与 CandyMatch3Kit 一致）
            Vector3 screenPos = Input.mousePosition;
            Vector3 worldPos3D = camera.ScreenToWorldPoint(screenPos);
            Vector2 worldPos = new Vector2(worldPos3D.x, worldPos3D.y);
            
            Log.Debug($"[Match3Input] 射线检测: screenPos={screenPos}, worldPos={worldPos}, camera={camera.name}, orthographic={camera.orthographic}");
            
            // 打印第一个瓦片的世界位置用于调试
            var firstTile = board.GetTile(0, 0);
            if (firstTile != null)
            {
                var firstTileView = firstTile.GetComponent<TileView>();
                if (firstTileView != null && firstTileView.GameObject != null)
                {
                    Vector3 tilePos = firstTileView.GameObject.transform.position;
                    Log.Debug($"[Match3Input] 第一个瓦片(0,0)世界位置: {tilePos}");
                }
            }
            
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            
            if (hit.collider == null)
            {
                Log.Debug($"[Match3Input] 射线未命中任何Collider2D");
                // 点击空白区域
                // Switch模式下点击空白区域取消道具
                var boosterMgr = board.GetComponent<BoosterManagerComponent>();
                if (boosterMgr != null && boosterMgr.InSwitchMode)
                {
                    boosterMgr.DeactivateBooster();
                }
                return;
            }
            
            // 通过 Collider 的 GameObject 找到对应的 Tile
            GameObject hitObject = hit.collider.gameObject;
            
            // 查找这个 GameObject 对应的 Tile
            Tile tile = self.FindTileByGameObject(board, hitObject);
            if (tile == null)
            {
                Log.Debug($"[Match3Input] 点击的对象不是瓦片: {hitObject.name}");
                return;
            }
            
            int x = tile.GetX();
            int y = tile.GetY();
            
            Log.Debug($"[Match3Input] 检测到瓦片点击: ({x}, {y})");
            
            // 记录拖拽起点
            self.DragStartWorldPos = worldPos;

            // 检查是否有激活的道具
            var boosterManager = board.GetComponent<BoosterManagerComponent>();
            if (boosterManager != null && boosterManager.ActiveBoosterType.HasValue)
            {
                // Switch道具使用拖拽模式
                if (boosterManager.ActiveBoosterType == BoosterType.Switch)
                {
                    self.IsDragging = true;
                    self.SwitchSelectedX = x;
                    self.SwitchSelectedY = y;
                    
                    // 播放瓦片选中动画
                    self.PlayTilePressedAnimation(tile, true);
                    return;
                }
                
                // 其他道具直接应用
                self.ApplyBoosterAtPositionAsync(board, boosterManager, x, y).NoContext();
                return;
            }

            // 无道具，进入拖拽状态
            self.IsDragging = true;
            self.DragStartX = x;
            self.DragStartY = y;
            
            // 播放按压动画
            self.PlayTilePressedAnimation(tile, true);
        }
        
        /// <summary>
        /// 通过 GameObject 查找对应的 Tile
        /// </summary>
        private static Tile FindTileByGameObject(this Match3InputComponent self, Match3BoardComponent board, GameObject gameObject)
        {
            // 遍历所有瓦片，查找匹配的 GameObject
            for (int y = 0; y < board.Level.Height; y++)
            {
                for (int x = 0; x < board.Level.Width; x++)
                {
                    var tile = board.GetTile(x, y);
                    if (tile == null) continue;
                    
                    var tileView = tile.GetComponent<TileView>();
                    if (tileView != null && tileView.GameObject != null)
                    {
                        // 检查 hitObject 是否是 tileView.GameObject 或其子物体
                        if (tileView.GameObject == gameObject || gameObject.transform.IsChildOf(tileView.GameObject.transform))
                        {
                            return tile;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 指针释放处理
        /// 参考 CandyMatch3Kit.GameBoard.HandleInput
        /// </summary>
        private static void OnPointerUp(this Match3InputComponent self, Match3BoardComponent board)
        {
            if (!self.IsDragging)
            {
                return;
            }

            self.IsDragging = false;
            
            // 恢复起始瓦片的按压动画
            var startTile = board.GetTile(self.DragStartX, self.DragStartY);
            if (startTile != null)
            {
                self.PlayTilePressedAnimation(startTile, false);
            }

            // 检查是否是Switch道具模式
            var boosterManager = board.GetComponent<BoosterManagerComponent>();
            if (boosterManager != null && boosterManager.InSwitchMode && self.SwitchSelectedX >= 0)
            {
                // 恢复选中瓦片的动画
                var selectedTile = board.GetTile(self.SwitchSelectedX, self.SwitchSelectedY);
                if (selectedTile != null)
                {
                    self.PlayTilePressedAnimation(selectedTile, false);
                }
                
                // 处理Switch道具拖拽
                self.HandleSwitchDragAsync(board, boosterManager).NoContext();
                return;
            }

            // 获取相机
            Camera camera = self.GameCamera;
            if (camera == null)
            {
                camera = Camera.main;
            }
            if (camera == null)
            {
                return;
            }
            
            // 使用射线检测目标瓦片（与 CandyMatch3Kit 一致）
            Vector2 worldPos = camera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            
            if (hit.collider == null)
            {
                return;
            }
            
            // 查找目标瓦片
            GameObject hitObject = hit.collider.gameObject;
            Tile targetTile = self.FindTileByGameObject(board, hitObject);
            
            if (targetTile == null)
            {
                return;
            }
            
            int targetX = targetTile.GetX();
            int targetY = targetTile.GetY();
            
            // 确保不是同一个瓦片
            if (targetX == self.DragStartX && targetY == self.DragStartY)
            {
                return;
            }
            
            // 检查是否相邻（不允许对角线）
            int dx = Mathf.Abs(targetX - self.DragStartX);
            int dy = Mathf.Abs(targetY - self.DragStartY);
            
            if (dx > 1 || dy > 1)
            {
                Log.Debug($"[Match3Input] 目标瓦片不相邻: ({self.DragStartX},{self.DragStartY}) -> ({targetX},{targetY})");
                return;
            }
            
            // 不允许对角线交换
            if (dx == 1 && dy == 1)
            {
                Log.Debug($"[Match3Input] 不允许对角线交换");
                return;
            }
            
            Log.Debug($"[Match3Input] 尝试交换: ({self.DragStartX},{self.DragStartY}) -> ({targetX},{targetY})");

            // 尝试交换
            self.TrySwapTilesAsync(board, self.DragStartX, self.DragStartY, targetX, targetY).NoContext();
        }

        /// <summary>
        /// 处理Switch道具的拖拽交换
        /// </summary>
        private static async ETTask HandleSwitchDragAsync(this Match3InputComponent self, Match3BoardComponent board, BoosterManagerComponent boosterManager)
        {
            // 禁用输入
            self.InputEnabled = false;

            try
            {
                // 获取释放的世界坐标
                Vector3 worldPos = self.ScreenToWorldPosition(Input.mousePosition);
                Vector3 dragDelta = worldPos - self.DragStartWorldPos;

                int x1 = self.SwitchSelectedX;
                int y1 = self.SwitchSelectedY;
                int x2 = x1;
                int y2 = y1;

                // 计算目标位置（只允许相邻，不允许对角线）
                if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
                {
                    // 水平拖拽
                    x2 += dragDelta.x > 0 ? 1 : -1;
                }
                else
                {
                    // 垂直拖拽
                    y2 += dragDelta.y > 0 ? 1 : -1;
                }

                // 重置选择
                self.SwitchSelectedX = -1;
                self.SwitchSelectedY = -1;

                // 检查拖拽距离是否足够
                if (dragDelta.magnitude < self.MinDragDistance)
                {
                    // 拖拽距离不够，取消操作
                    return;
                }

                // 检查目标瓦片是否有效
                var targetTile = board.GetTile(x2, y2);
                if (targetTile == null)
                {
                    // 目标位置无效
                    return;
                }

                // 执行Switch道具交换（带视觉效果）
                await boosterManager.ExecuteSwitchDragWithViewAsync(board, x1, y1, x2, y2);
            }
            finally
            {
                // 恢复输入
                self.InputEnabled = true;
            }
        }

        /// <summary>
        /// 播放瓦片按压动画
        /// </summary>
        private static void PlayTilePressedAnimation(this Match3InputComponent self, Tile tile, bool pressed)
        {
            if (tile == null) return;
            
            var tileView = tile.GetComponent<TileView>();
            if (tileView == null || tileView.GameObject == null) return;
            
            var animator = tileView.GameObject.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(pressed ? "Pressed" : "Unpressed");
            }
        }

        /// <summary>
        /// 应用道具到指定位置（异步）
        /// 注意：Switch道具使用拖拽模式，不经过此方法
        /// </summary>
        private static async ETTask ApplyBoosterAtPositionAsync(this Match3InputComponent self, Match3BoardComponent board, BoosterManagerComponent boosterManager, int x, int y)
        {
            // 禁用输入，防止重复操作
            self.InputEnabled = false;

            try
            {
                // 应用道具（Lollipop、Bomb、ColorBomb）
                // Switch道具使用拖拽模式，在OnPointerDown/OnPointerUp中处理
                await boosterManager.ApplyBoosterWithViewAsync(board, x, y);
            }
            finally
            {
                // 恢复输入
                self.InputEnabled = true;
            }
        }

        /// <summary>
        /// 尝试交换瓦片（异步）
        /// </summary>
        private static async ETTask TrySwapTilesAsync(this Match3InputComponent self, Match3BoardComponent board, int x1, int y1, int x2, int y2)
        {
            // 禁用输入，防止重复操作
            self.InputEnabled = false;

            try
            {
                await board.TrySwapTilesAsync(x1, y1, x2, y2);
            }
            finally
            {
                // 恢复输入
                self.InputEnabled = true;
            }
        }

        #region 坐标转换辅助方法

        /// <summary>
        /// 屏幕坐标转世界坐标
        /// </summary>
        private static Vector3 ScreenToWorldPosition(this Match3InputComponent self, Vector3 screenPos)
        {
            Camera camera = self.GameCamera;
            if (camera == null)
            {
                camera = Camera.main;
            }

            if (camera == null)
            {
                return Vector3.zero;
            }

            Vector3 worldPos = camera.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;
            return worldPos;
        }

        /// <summary>
        /// 世界坐标转棋盘坐标
        /// </summary>
        private static bool WorldToBoardPosition(this Match3InputComponent self, Vector3 worldPos, out int x, out int y)
        {
            x = -1;
            y = -1;

            var board = self.GetParent<Match3BoardComponent>();
            if (board == null || !board.HasLevel)
            {
                return false;
            }

            // 应用棋盘偏移
            Vector2 localPos = new Vector2(worldPos.x - self.BoardOffset.x, worldPos.y - self.BoardOffset.y);

            // 计算棋盘坐标
            x = Mathf.FloorToInt(localPos.x / self.TileSize);
            y = Mathf.FloorToInt(localPos.y / self.TileSize);

            // 边界检查
            if (x < 0 || x >= board.Level.Width || y < 0 || y >= board.Level.Height)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化输入组件
        /// </summary>
        public static void Initialize(this Match3InputComponent self, Camera gameCamera, Transform boardTransform, float tileSize, Vector2 boardOffset)
        {
            self.GameCamera = gameCamera;
            self.BoardTransform = boardTransform;
            self.TileSize = tileSize;
            self.BoardOffset = boardOffset;
        }

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

        #endregion
    }
}
