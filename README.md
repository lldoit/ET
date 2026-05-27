# English: please use AI to translate to english

ET10
The AI Native Framework

ET10 is a next-generation game development framework designed for the AI coding era.

Unlike traditional game frameworks that only focus on runtime performance, ET10 is built to make AI-assisted game development practical for large-scale online games.

ET10 combines:

Distributed MMO architecture
Fiber-based actor runtime
Cloud-native server infrastructure
Hot reload workflows
Automated gameplay testing
AI harness skills
Unity runtime bridge
Analyzer-enforced architecture

so AI can safely develop, test, and maintain complex online game projects without breaking project structure.

Built for AI-Assisted Development

Most AI coding tools can generate code.

ET10 ensures the generated code can:

compile successfully
pass gameplay tests
obey architecture rules
survive hot reload
scale across distributed servers
integrate safely with existing systems

before being accepted into production.

ET10 includes a powerful analyzer ecosystem to enforce large-scale project architecture, including:

async safety rules
entity lifecycle protection
package dependency constraints
actor usage validation
runtime safety checks

This allows AI to work within strict architectural boundaries instead of generating uncontrolled code.

Designed for Large-Scale Online Games

ET10 provides a modern MMO runtime with:

distributed actor model
dynamic room scaling
service discovery
cloud-native deployment
location services
hotfix workflows
runtime observability
multi-process server architecture

The framework is designed for long-running, scalable online games rather than small standalone projects.

AI Harness + Automated Verification

ET10 goes beyond code generation.

It provides a complete AI development workflow including:

automated compilation
gameplay verification
multi-bot testing
Unity Editor integration
runtime log analysis
test harness execution
automated validation pipelines

AI is not only able to write code, but also verify whether the game actually works.

Unity Runtime Integration

ET10 includes a Unity runtime bridge that allows AI to interact directly with the engine runtime.

Examples include:

creating GameObjects
modifying components
controlling Play Mode
refreshing assets
reading logs
running gameplay tests
inspecting runtime entities

This enables a true AI-driven game development workflow instead of simple file-based code generation.

Why ET10?

Traditional frameworks are designed for human developers.

ET10 is designed for a future where:

AI writes code
analyzers enforce architecture
automated systems verify gameplay
cloud infrastructure scales dynamically
developers focus on game design instead of repetitive engineering tasks
ET10
AI writes code. ET10 makes it production-ready.

For running instructions, please refer to [Book/1.1Running Guide.md](Book/1.1Running%20Guide.md).

# ET10.0

1.

  | 包名                         | 详细内容                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
  | -------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
  | cn.etetet.harness          | ai各种skills跟分析器，包括et-code, et-async, et-excel, et-luban, et-build, et-test, et-tdd等skills，还有包分析器, await分析器等等分析器, 主要用于严格限制ai行为                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
  | cn.etetet.unitybridge      | 该包是Unity Skills, 实现了各种Unity操作，并且极其方便扩展，提出需求让ai实现即可。目前注册的68个指令全部包括：状态与生命周期 Ping、HostState、Compile、Refresh、RegenProject、EnterPlay、ExitPlay、Reload、EditorGetStateRequest、EditorPauseRequest、EditorUndoRequest、EditorRedoRequest；资源 AssetSearchRequest、AssetFindRequest、AssetGetPathRequest、AssetLoadRequest、AssetImportRequest、AssetRefreshRequest、AssetReadTextRequest；场景 SceneGetHierarchyRequest、SceneGetActiveRequest、SceneLoadRequest、SceneSaveRequest、SceneNewRequest；选择集 SelectionGetRequest、SelectionSetRequest、SelectionAddRequest、SelectionRemoveRequest、SelectionClearRequest；GameObject GameObjectCreateRequest、GameObjectDestroyRequest、GameObjectDuplicateRequest、GameObjectFindRequest、GameObjectGetInfoRequest、GameObjectRenameRequest、GameObjectSetActiveRequest；Transform TransformGetRequest、TransformSetPositionRequest、TransformSetRotationRequest、TransformSetScaleRequest、TransformSetParentRequest、TransformLookAtRequest、TransformResetRequest、TransformSetSiblingIndexRequest；Inspector InspectorGetComponentsRequest、InspectorGetPropertiesRequest、InspectorGetPropertyRequest、InspectorFindPropertyRequest、InspectorSetPropertyRequest、InspectorSetPropertiesRequest、InspectorAddComponentRequest、InspectorRemoveComponentRequest；Prefab PrefabInstantiateRequest、PrefabSaveRequest、PrefabUnpackRequest、PrefabGetInfoRequest、PrefabGetHierarchyRequest、PrefabApplyRequest；GameView GameViewGetResolutionRequest、GameViewListResolutionsRequest、GameViewSetResolutionRequest；其它 ConsoleGetLogsRequest、EditorLogRequest、ScreenshotCaptureRequest、UnityTestRunRequest、MenuItemExecuteRequest、BatchExecuteRequest、TestEcho |
  | cn.etetet.test             | 测试框架，可以模拟整个游戏环境，实现整条客户端跟服务器交互。比如组队功能，可以写一个test，里面创建5个客户端机器人，然后让机器人发送消息实现组队，从而可以测试整个客户端跟服务端的组队逻辑。有了test框架，ai的工作检查变得极其轻松，只需要让ai写test，然后 review自己的test，确保test执行通过即可。不仅成实现逻辑层Test，UI Test也完全可以实现                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
  | cn.etetet.behaviortree     | 行为树包，提供BTNode、Sequence、Selector、Not、Condition、Action等行为树节点定义与运行时支持，并带有Unity编辑器可视化编辑、节点参数配置、运行路径调试和配置代码导出能力，适合实现怪物AI、任务流程、技能释放、复杂条件判断等可配置决策逻辑。                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
  | cn.etetet.servicediscovery | 服务发现包，提供服务注册、注销、查询、订阅、心跳、租约和变更通知能力，进程级ServiceDiscoveryAgent会把请求转发到当前服务发现主节点，并支持主节点切换、故障恢复和多客户端并发访问，适合多进程多服务器架构下动态发现服务                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
  | cn.etetet.conditionexpr    | ConditionExpr包，可以把Excel/Luban中填写的条件表达式编译成行为树运行，支持数值比较、&&、||、!、括号、错误码绑定、多owner key和专用条件节点，适合副本进入条件、奖励领取条件、任务接取条件、功能开启条件等策划可配置条件判断                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
  | cn.etetet.spell            | Spell包，提供技能与Buff系统，包含Spell/Buff配置模型、运行时施法逻辑、客户端表现组件和相关Unity编辑器工具，并复用行为树配置导出链路，适合实现技能释放、Buff结算、技能表现和复杂战斗流程配置。Unity菜单 ET->Spell->Spell Editor提供集成化的编辑器。甚至直接让ai自己配置即可                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
  | cn.etetet.aspire           | Aspire包，整合ET框架与.NET Aspire，会根据StartConfig为每个ET进程和副本创建Aspire服务，自动传递SceneName、StartConfig、内外网端口等启动参数，并接入OTLP/OpenTelemetry，适合分布式应用编排、运行观测和监控                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |

2. game develop ai harness支持，ET10遥遥领先! 绝大部分框架要么客户端要么服务端，要想实现完善的vibe coding test框架是非常不舒服的，只有ET10这种前后端一起，才能极其方便开发双端test，直接打通前后端逻辑，联调都不用，test直接完成双端逻辑开发，完成联调工作。逻辑开发的工作量只剩下UI了。
3. 使用请参考 [Book/1.1Running Guide.md](Book/1.1Running%20Guide.md)
4. cn.etetet.webgl  客户端支持打包webgl，前后端websocket连接，注意微信小游戏需要自己接入，由于已经支持了webgl，小游戏接入并不复杂，不接入小游戏主要是因为有人会使用团结有人用unity，没法统一版本，售价999元
5. ETTask实现了传递上下文功能，可以去掉烦人的CancellationToken传递
6. Entity简化，去掉了ChildrenDB跟ComponentDB，自定义序列化SortedDictionary，可以指定某个Child或者Componet跟不跟随Parent序列化
7. Kcp改成非托管内存分配，GC更少
8. 可以在Unity中按F6进行编译，也可以在IDE中进行编译，运行中reload可以先按F6编译，再按F7进行热重载
9. 多线程多进程架构，架构更加灵活强大，多线程设计详细内容请看多线程设计课程
10. 抽象出纤程(Fiber)的概念，类似erlang的进程，非常轻松的创建多个纤程，利用多核，仍然是单线程开发的体验
11. 纤程调度: 主线程，线程池，每个纤程一个线程，3种调度方式
12. Fiber间通信的Actor消息机制
13. Entity方面，domain改成IScene，只要实现IScene接口，Entity就是domain，这样定义domain更加自由
14. 预测回滚的帧同步实现  想详细了解可以看帧同步课程
15. protobuf换成了memorypack，实现无gc的网络
16. 纯C#版的kcp库，性能非常强
17. 热更dll改成用ide编译，更加方便
18. sj利用source generater实现了代码自动模板功能，目前可以自动生成System类，开发者只需要定义Awake Update静态方法即可，特别方便
19. sj开发了分析器，实现了EntitySystemOf，根据entity接口一键生成对应的system方法
20. 客户端利用fiber实现网络独立线程（demo已实现），甚至可以把逻辑跟表现使用独立的纤程，更好的利用多核
21. 帧同步demo直接利用纤程创建房间，更加方便
22. 纯c#版寻路dotrecast，至此ET已经完全C#化，没有任何cpp代码了
23. kcp跟软路由底层同时支持tcp跟websocket，当udp联不通的情况下，可以切换成tcp Websocket，并且支持运行时动态切换，玩家不掉线！
24. 集成了sj的非托管容器库，性能爆炸
25. 多进程多线程Actor架构，客户端跟服务端都可以轻松创建纤程(fiber)利用多核，比如客户端网络一个纤程，寻路一个纤程，帧同步逻辑层一个纤程，表现层一个纤程
26. async await协程同步代码编写，避免回调地狱
27. 0GC消耗，超强的MemoryPack序列化, 超强的网络层性能
28. kcp支持，网络响应非常迅速，并且闪断wifi 4g都不会导致掉线，做竞技游戏必备
29. kcp底层可以使用tcp udp Websocket协议，当udp联不通的情况下，可以切换成tcp Websocket，并且支持运行时动态切换，玩家不掉线！
30. 软路由防攻击设计，买些垃圾主机就可以防住黑客攻击，比买高防省钱多了，并且用户不会掉线
31. 双端C#开发，前后端共享代码，C#本身性能极强，仅次于CPP，不需要学一些乱起八糟的语言，很多独立游戏开发者，一个人就能用ET开发mmorpg游戏
32. 强大的编译分析器，编译器就能帮助大家写出正确的ET风格的代码
33. 客户端hybridclr热更新支持
34. 客户端服务端均支持运行时热重载，客户端服务端不需要关闭进程就能修改代码，大大提升了开发效率以及运营效率
35. 完善的demo，源码带有状态同步跟预测回滚的帧同步demo
36. 完善的机器人开发机制，机器人直接共享客户端逻辑代码，减少95%机器人开发工作量，接入ai机器人非常轻松。大规模机器人压测，轻而易举
37. 强大的ai开发机制，比行为树更加容易
38. 强大的单元测试开发机制，每个单元测试都是整个游戏环境，不用搞mock隔离，开发起来非常轻松
39. 优美的程序结构，数据跟方法完全分离
40. all in one的开发体验，开发时只需要启动unity，发布的时候又可以单独发布服务端，并且可以跨windows跟linux平台
41. 客户端服务端数据开发期完全可视化，开启ENABLE_VIEW宏即可在Unity Hierarchy面板中看到客户端跟服务端的所有的Entity对象以及字段的内容
42. WebGL以及微信小游戏支持
43. 调整结构，机器人工程与服务器合并，更易使用，一个进程同时可以做server，也能创建机器人，真正的ALL IN ONE! -- 已实现
44. 客户端跟服务端合并，服务端代码全部放在了客户端，客户端中可以带一个服务端，开发超级方便，服务端发布的时候可以选择发布成Dotnet也可以发布成UnityServer，终极All IN ONE  -- 已实现
45. Entity可视化，客户端跟服务端所有的Entity都实现了可视化，开启ENABLE_CODES宏，运行游戏，查看Hierarchy面板，展开Init/Global/Scene(Process)即可看到 -- 已实现
46. 增加软路由，可以防各种网络攻击而不影响正常玩家，网游必备！-- 已实现
47. 各种事件跟网络消息订阅带上DomainSceneType，更精确，更不容易出错 -- 已实现
48. sj兄弟添加了各种分析器，分析器保证了写出的代码必须符合ET规范，否则编译不通过！
49. 网络改成独立线程，序列化反序列化都在网络线程处理，主线程压力大大减轻。并且重新整理了网络层代码，更优美了
50. 集成Unity.Mathematic数学库，逻辑层客户端跟服务端都使用这一套数学库，这样服务端跟客户端完全统一了
51. ENABLE_CODES模式下拆分成4个程序集，解决分析器失效的问题
52. Game管理的Singleton增加ISingletonUpdate跟ISingletonLateUpdate接口，实现相应的接口即可执行对应的Update跟LateUpdate方法，Game类解除了跟EventSystem等单间类的耦合关系
53. Actor消息判断如果是发向自己的进程则不用通过网络，直接处理即可，大大提升性能



1. 动态副本跟分线，按需分配，用完回收
2. 分线合线，分线人数较少会把多条线合并。合线功能基本上其它mmo游戏很少见到
3. 客户端服务端场景无缝切换，也就是无缝大世界技术
4. 跨服副本，跨服战场
5. 前后端一体化，利用客户端代码开发服务器压测机器人，4台24核机器轻松模拟1W人做任务
6. 千古风流各种ai设计，使用ET的全新开发的ai框架，使ai开发简单到跟写ui一样简单
7. 测试用例框架，大部分重要系统，千古风流都写了测试用例，跟市面上的测试用例不同，每个千古风流的测试用例都是一个完整的游戏环境，针对协议级别，不需要搞各种接口去mock。写起来非常快速。
8. 九宫格的aoi实现，动态调整看见的玩家，降低服务器负载
9. 防攻击，千古风流开发了软路由功能，即使攻击也只能攻击到软路由，一旦被攻击，玩家客户端发现几秒钟无响应，即可动态切换到其它软路由，用户几乎无感知。整个过程客户端网络连接不断开，数据不丢失。


### 1.可用VS单步调试的分布式服务端，N变1

一般来说，分布式服务端要启动很多进程，一旦进程多了，单步调试就变得非常困难，导致服务端开发基本上靠打log来查找问题。平常开发游戏逻辑也得开启一大堆进程，不仅启动慢，而且查找问题及其不方便，要在一堆堆日志里面查问题，这感觉非常糟糕，这么多年也没人解决这个问题。ET框架使用了类似守望先锋的组件设计，所有服务端内容都拆成了一个个组件，启动时根据服务器类型挂载自己所需要的组件。这有点类似电脑，电脑都模块化的拆成了内存，CPU，主板等等零件，搭配不同的零件就能组装成一台不同的电脑，例如家用台式机需要内存，CPU，主板，显卡，显示器，硬盘。而公司用的服务器却不需要显示器和显卡，网吧的电脑可能不需要硬盘等。正因为这样的设计，ET框架可以将所有的服务器组件都挂在一个服务器进程上，那么这个服务器进程就有了所有服务器的功能，一个进程就可以作为整组分布式服务器使用。这也类似电脑，台式机有所有的电脑组件，那它也完全可以当作公司服务器使用，也可以当作网吧电脑。

### 2.随意可拆分功能的分布式服务端，1变N

分布式服务端要开发多种类型的服务器进程，比如Login server，gate server，battle server，chat server friend server等等一大堆各种server，传统开发方式需要预先知道当前的功能要放在哪个服务器上，当功能越来越多的时候，比如聊天功能之前在一个中心服务器上，之后需要拆出来单独做成一个服务器，这时会牵扯到大量迁移代码的工作，烦不胜烦。ET框架在平常开发的时候根本不太需要关心当前开发的这个功能会放在什么server上，只用一个进程进行开发，功能开发成组件的形式。发布的时候使用一份多进程的配置即可发布成多进程的形式，是不是很方便呢？随便你怎么拆分服务器。只需要修改极少的代码就可以进行拆分。不同的server挂上不同的组件就行了嘛！

### 3.跨平台的分布式服务端

ET框架使用C#做服务端，现在C#是完全可以跨平台的，在linux上安装.netcore，即可，不需要修改任何代码，就能跑起来。性能方面，现在.netcore的性能非常强，比lua，python，js什么快的多了。做游戏服务端完全不在话下。平常我们开发的时候用VS在windows上开发调试，发布的时候发布到linux上即可。ET框架还提供了一键同步工具，打开unity->tools->rsync同步，即可同步代码到linux上

```bash
./Run.sh Config/StartConfig/192.168.12.188.txt
```

即可编译启动服务器。

### 4.提供协程支持

C#天生支持异步变同步语法 async和await，比lua，python的协程强大的多，新版python以及javascript语言甚至照搬了C#的协程语法。分布式服务端大量服务器之间的远程调用，没有异步语法的支持，开发将非常麻烦。所以java没有异步语法，做单服还行，不适合做大型分布式游戏服务端。例如：

```c#
// 发送C2R_Ping并且等待响应消息R2C_Ping
R2C_Ping pong = await session.Call(new C2R_Ping()) as R2C_Ping;
Log.Debug("收到R2C_Ping");

// 向mongodb查询一个id为1的Player，并且等待返回
Player player = await Game.Scene.GetComponent<DBProxyComponent>().Query<Player>(1);
Log.Debug($"打印player name: {player.Name}")
```

可以看出，有了async await，所有的服务器间的异步操作将变得非常连贯，不用再拆成多段逻辑。大大简化了分布式服务器开发

### 5.提供类似erlang的actor消息机制

erlang语言一大优势就是位置透明的消息机制，用户完全不用关心对象在哪个进程，拿到id就可以对对象发送消息。ET框架也提供了actor消息机制，实体对象只需要挂上MailBoxComponent组件，这个实体对象就成了一个Actor，任何服务器只需要知道这个实体对象的id就可以向其发送消息，完全不用关心这个实体对象在哪个server，在哪台物理机器上。其实现原理也很简单，ET框架提供了一个位置服务器，所有挂载MailBoxComponent的实体对象都会将自己的id跟位置注册到这个位置服务器，其它服务器向这个实体对象发送消息的时候如果不知道这个实体对象的位置，会先去位置服务器查询，查询到位置再进行发送。

### 6.提供服务器不停服动态更新逻辑功能

热更是游戏服务器不可缺少的功能，ET框架使用的组件设计，可以做成守望先锋的设计，组件只有成员，无方法，将所有方法做成扩展方法放到热更dll中，运行时重新加载dll即可热更所有逻辑。

### 7.客户端使用C#热更新，热更新一键切换

可以使用csharp.lua或者ILRuntime稍加改造即可做客户端热更。再也不用使用狗屎lua了，客户端可以实现所有逻辑热更新，包括协议，config，ui等等。

### 8.客户端热重载

开发不用重启客户端即可修改客户端逻辑代码，开发极其方便

### 9.客户端服务端用同一种语言，并且共享代码

下载ET框架，打开服务端工程，可以看到服务端引用了客户端很多代码，通过引用客户端代码的方式实现了双端共享代码。例如客户端服务端之间的网络消息两边完全共用一个文件即可，添加一个消息只需要修改一遍。

### 10.KCP ENET TCP Websocket协议无缝切换

ET框架不但支持TCP，而且支持可靠的UDP协议（ENET跟KCP），ENet是英雄联盟所使用的网络库，其特点是快速，并且网络丢包的情况下性能也非常好，这个我们做过测试TCP在丢包5%的情况下，moba游戏就卡的不行了，但是使用ENet，丢包20%仍然不会感到卡。非常强大。框架还支持使用KCP协议，KCP也是可靠UDP协议，据说比ENET性能更好，使用kcp请注意，需要自己加心跳机制，否则20秒没收到包，服务端将断开连接。协议可以无缝切换。

### 11. 3D Recast寻路功能

可以Unity导出场景数据，给服务端做recast寻路。做MMO非常方便，demo演示了服务端3d寻路功能

### 12. 服务端支持repl，也可以动态执行一段新代码

这样就可以打印出进程中任何数据，大大简化了服务端查找问题的难度，开启repl方法，直接在console中输入repl回车即可进入repl模式

### 13.提供客户端机器人框架支持

几行代码即可创建机器人登录游戏。机器人压测轻而易举，机器人跟正常的玩家完全一样，上线前用机器人做好压测，大大降低上线崩溃几率

### 14.AI框架

ET的AI框架让AI编写比UI还简单

### 15.测试用例框架

跟市面上的测试用例不同，ET的测试用例都是一个完整的游戏环境，针对协议级别，不需要搞各种接口去mock。写起来非常快速

### 16.还有很多很多功能，我就不详细介绍了

a.及其方便检查CPU占用和内存泄漏检查，vs自带分析工具，不用再为性能和内存泄漏检查而烦恼
b.使用NLog库，打log及其方便，平常开发时，可以将所有服务器log打到一个文件中，再也不用一个个文件搜索log了
c.统一使用Mongodb的bson做序列化，消息和配置文件全部都是bson或者json，并且以后使用mongodb做数据库，再也不用做格式转换了。
d.提供一个同步工具

# Benchmark

100W Ping Pong 平均耗时4秒左右，平均每秒收发20W的消息。这个网络性能远远超过主线程的需求，大家可以自己测试一下，测试方法：
Unity Menu->ServerTools select Benchmark, Start Watcher。然后在Logs目录，打开Debug日志等一会所有连接完成就能看到下面的日志了。
2022-12-02 22:19:48.9837 (C2G_BenchmarkHandler.cs:13) benchmark count: 1000001
2022-12-02 22:19:53.4621 (C2G_BenchmarkHandler.cs:13) benchmark count: 2000001
2022-12-02 22:19:57.0416 (C2G_BenchmarkHandler.cs:13) benchmark count: 3000001
2022-12-02 22:20:00.6186 (C2G_BenchmarkHandler.cs:13) benchmark count: 4000001
2022-12-02 22:20:04.1384 (C2G_BenchmarkHandler.cs:13) benchmark count: 5000001
2022-12-02 22:20:08.2236 (C2G_BenchmarkHandler.cs:13) benchmark count: 6000001
2022-12-02 22:20:12.2842 (C2G_BenchmarkHandler.cs:13) benchmark count: 7000001
2022-12-02 22:20:15.8544 (C2G_BenchmarkHandler.cs:13) benchmark count: 8000001
2022-12-02 22:20:19.4085 (C2G_BenchmarkHandler.cs:13) benchmark count: 9000001
2022-12-02 22:20:24.2969 (C2G_BenchmarkHandler.cs:13) benchmark count: 10000001
2022-12-02 22:20:41.1448 (C2G_BenchmarkHandler.cs:13) benchmark count: 11000001
2022-12-02 22:20:44.7174 (C2G_BenchmarkHandler.cs:13) benchmark count: 12000001
2022-12-02 22:20:48.3188 (C2G_BenchmarkHandler.cs:13) benchmark count: 13000001
2022-12-02 22:20:51.7793 (C2G_BenchmarkHandler.cs:13) benchmark count: 14000001
2022-12-02 22:20:55.3379 (C2G_BenchmarkHandler.cs:13) benchmark count: 15000001
2022-12-02 22:20:58.8810 (C2G_BenchmarkHandler.cs:13) benchmark count: 16000001
2022-12-02 22:21:02.5156 (C2G_BenchmarkHandler.cs:13) benchmark count: 17000001
2022-12-02 22:21:06.0132 (C2G_BenchmarkHandler.cs:13) benchmark count: 18000001
2022-12-02 22:21:09.5320 (C2G_BenchmarkHandler.cs:13) benchmark count: 19000001
