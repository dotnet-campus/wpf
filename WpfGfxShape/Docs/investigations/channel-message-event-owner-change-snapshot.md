# Channel message event owner：变更前快照与执行合同

## 授权与范围

本轮授权优先修确定所有权缺口，取代静态调查§4“先句柄类型诊断”的当前动作；该节保持历史。不把每轮约+4归因全部Event/永久泄漏。本轮仅SameThreadChannel事件owner和相关单元；外部API/可见性/既有InternalsVisibleTo不改，不改scheduler架构、显示、媒体、NativeRun、采样器或统计。

## 变更前后表（编辑前落盘）

|路径|修改前|目标修改后|
|---|---|---|
|构造|字段创建AutoReset Event，后续初始化失败不显式关闭|finally在初始化未完成时Dispose并保留原异常|
|WaitForMessage|直接WaitForMultipleObjects裸句柄；SafeWaitHandle getter在_messages锁外，锁内DangerousAddRef，finally归还|getter/AddRef/Reset同锁并先判destroyed；保留native wait、HRESULT、输出及成对归还|
|ReleaseResources|destroyed锁外赋值，仅Set；后续资源可能抛错|锁内幂等终态/清队列/Set/finally Dispose，先于外部资源释放；活动borrow使物理关闭延迟到最后归还|
|WakeMessageWaiters|无终态guard Set|同_messages锁，终态no-op；活动channel的合法wake不屏蔽|
|其他event访问|Post已同锁且guard；Wait内Reset|统一_messages锁；不增加全局锁/新抽象|

## 并发与原合同

生产实际不是WaitHandle.WaitOne：WaitForMessage复制外部handles，加入queue裸句柄，调用kernel32 WaitForMultipleObjects。故必须保留SafeHandle显式借用；只依赖WaitOne内部安全借用不适用。SafeWaitHandle引用先在锁内获取，成功DangerousAddRef唯一对应finally DangerousRelease；早退不借用。Dispose释放owner引用，未结束borrow仍可安全native wait；Set发生在Dispose之前，最后borrow归还物理关闭。不在Dispose后再获取/Reset/Set。

_messages锁仅保护消息/event生命周期；命令/COM资源Destroy仍遵守原Gate或composition-thread串行合同，不声称任意并行调用Destroy/Commit可用。ReleaseResources终态在_messages锁下幂等。Scheduler在failed partition management Destroy后还会Wake，必须no-op以免worker无法完成Completion。ABI Wait在Gate中取得state后释放Gate再等待，所以与Destroy竞态实际存在。

原SameThreadChannel在等待前/返回后已规定destroyed返回0x80070006，Partition.Failure优先于消息等待；不创造新expected。ABI无效identity仍E_INVALIDARG，不把core终态HRESULT冒充无效identity ABI。官方参考src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/clientchannel.cpp:979–1071：有消息立即返回index=count；空队列在queue锁内Reset后WaitForMultipleObjects，保留waitAll/timeout/外部handle语义。官方未定义对已释放对象继续调用，此处仅维持现有core竞态合同。AutoReset一次Set不广播，waitAll仍需其他handle满足；本修正不发明多等待者广播或改变waitAll语义。

## 验证与停止

新类ChannelMessageEventLifecycleTests通过既有friend可访问真实生产类，不新增友元、不mock、不反射、不注入私有状态。计划9个新测试：重复Destroy、Destroy后Wake/Wait、已有消息、空队列timeout、外部signal、活动Post/Wake/Destroy；OR现PartitionSchedulerTests精确4个，共13个。活动等待使用entered事件、有限native timeout与有限Task.WaitAsync；Wake测试在有界SpinWait内重复合法Wake直到任务完成，避免entered先于Reset时一次wake丢失；finally销毁并有界等待。不用随机Sleep/无界Task。entered不能证明已到私有AddRef点，Post/Destroy覆盖并发入口竞态而不声称确定暂停在native wait内；最后borrow关闭的动态证明仍未证。

私有event无既有公开safehandle状态入口：不增加探针/反射，因此托管测试只证行为边界，不能直接确证OS CloseHandle；该剩余需Native验收。构造失败没有合法确定触发点，不引入故障注入API，静态保证清理，动态未证。确认兄弟模式：同生产目录仅此处显式DangerousAddRef；Post同锁guard已有，不扩修其他owner。

normal Release build恰1：WpfGfxShape/Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj，TFM net10.0，SDK global.json 10.0.107/latestFeature，不改TFM/包/分析器。仅成功后run_tests恰1，filter FullyQualifiedName~WpfGfxShape.Tests.ChannelMessageEventLifecycleTests|FullyQualifiedName~WpfGfxShape.Tests.PartitionSchedulerTests；工具默认Debug隐式build另计，不冒Release测试。任一失败停止，不修后重试。结果/真实exit/warnings/errors/可见截断及时落盘并读取日志。Native/COMpublish/沙箱/趋势/终端脚本/再委派0。

## 回撤范围

仅回撤SameThreadChannel.cs本轮constructor、WakeMessageWaiters、WaitForMessage、ReleaseResources局部变更和新测试文件；不覆盖其他既存改动。文档保留失败/执行账，不回撤历史证据。本轮生产改动待Native验证；旧15A545/54BC62不认证新代码。通过最多说明有限事件生命周期修正和托管验证。下一唯一门需另授权Native x64发布及原official/candidate各16轮4+12/65样本复验，不自动消耗。历史硬门Pass、PendingInvestigate、softDescriptiveOnly、memoryGreen=null、WorkerExitNotProven、完整替换Open不变。
