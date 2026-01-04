# 变更日志

所有重要的变更都会记录在这个文件中。

## [1.1.0] - 2026-01-04

### 新增
- ✨ 背景音乐淡入淡出切换功能 `PlayMusicWithFade()`
- ✨ 淡出停止音乐功能 `StopMusicWithFade()`
- ✨ 智能音乐切换（自动避免播放相同音乐）
- ✨ 取消令牌支持（可中断淡入淡出过程）
- ✨ 当前音乐地址记录 `CurrentMusicAddress`
- ✨ AudioHelper新增快速淡入淡出方法

### 改进
- 🔧 `SetMusicVolume()` 在淡入淡出期间不会干扰音量变化
- 🔧 组件销毁时自动取消淡入淡出任务
- 📝 新增高级音乐切换示例文档
- 📝 更新README和EXAMPLES文档

### 技术细节
- 使用 `ETCancellationToken` 管理异步任务
- 使用 `WaitFrameAsync()` 实现平滑音量过渡
- 使用 `Mathf.Lerp()` 实现线性插值
- 严格遵循 EntityRef 安全访问规范

## [1.0.0] - 2026-01-03

### 新增
- ✨ 初始版本发布
- ✨ SoundComponent音频管理组件
- ✨ 背景音乐播放功能（播放、暂停、停止、恢复）
- ✨ 2D音效播放功能
- ✨ 3D音效播放功能
- ✨ YooAsset资源加载集成
- ✨ AudioSource对象池管理系统
- ✨ 协程锁机制避免资源重复加载
- ✨ 音乐和音效独立音量控制
- ✨ AudioHelper辅助工具类
- ✨ 完整的文档和使用示例

### 架构
- 🏗️ 符合ET框架ECS架构设计
- 🏗️ Entity-System严格分离
- 🏗️ ModelView层负责数据定义
- 🏗️ HotfixView层负责业务逻辑
- 🏗️ 遵循ET分析器规范（EntityRef安全访问）

### 文档
- 📝 README.md - 完整的使用文档
- 📝 EXAMPLES.md - 6个详细的使用示例
- 📝 LICENSE - MIT开源协议

### 依赖
- cn.etetet.core: 3.0.3
- cn.etetet.yooassets: 2.3.6

---

版本格式说明：
- [主版本号.次版本号.修订号]
- 主版本号：不兼容的API修改
- 次版本号：向下兼容的功能性新增
- 修订号：向下兼容的问题修正

