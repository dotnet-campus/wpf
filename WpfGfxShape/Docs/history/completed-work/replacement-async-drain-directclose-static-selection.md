# AsyncDrain directClose 6/8：零执行最小回归选择

## 裁决与本次账

本次仅读源码、既有报告、包清单及编译程序集公开API，写Docs。源码/项目/测试代码修改、build/test/publish/Native发布/沙箱/终端/脚本/再委派新增全部0；未重算二进制SHA，未生成新运行通过。完整替换Open，整体不升，显示冻结，KN17及所有旧红绿账独立保留。

唯一下一门：另授权使用现有 asyncownership clean 包，每架构最多一次 **仅 list-tests 的参数节点发现**，测试体预算0，build/publish/Native预算0。发现回收真实参数行UID后再独立授权下表精确参数体，不先跑整组、不造薄入口/Host/脚本/项目。此前x64收尾提及“两个薄入口”不作为本轮方案。

## 实际源码合同（不是按方法名推断）

源文件 `Tests/WpfGfxShape.ComAcceptance/UceChannelTests.AsyncDrain.cs`：

- 10–35：candidate方法 `WhenAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates(int queueState)` 有 DataRow(0)…DataRow(11)；official方法 `WhenOriginalAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates(int queueState)` 有 DataRow(0),(1),(2),(6),(7),(8)。两者均 Timeout(30000)，分别调用 RunAsyncWicDrain(false/true, queueState)，retainExternal默认false。类主声明有 TestClass/DoNotParallelize。
- 167–168、181–183：failed = queueState % 6 >= 3；directClose = queueState >= 6；state = queueState % 3。queueState是**复合场景编码**，不是底层队列状态枚举，更不是队列长度6/8。6 = failed false / directClose true / state0(open，未CloseBatch/Commit)；8 = false / true / state2(committed，CloseBatch和Commit均成功)。7是closed未commit，本次不选；9–11错误分支不选。
- 140–165：connect(0)异步；channel及同partition sibling；Bitmap类型95；真实62字节2x1 BMP；通过被测模块 MILCreateStreamFromStreamDescriptor 创建真实COM stream，经系统WIC DecodeFrame，再 MilResource_CreateCWICWrapperBitmap、SendCommandBitmapSource。wrapper/frame caller真实Release后，WIC/transport仍持有stream；不是假引用计数或空队列。
- 184–196：Disconnect.ReferenceOnly后不走SyncFlush；caller释放自身stream；健康分支要求dispose0，然后 **caller调用 Destroy.Drain(channel)**。directClose指不显式flush直接Destroy主channel，不是指主channel在callback中关闭。
- 156–164：最终stream释放回调记录callback.enter与releaseThread；仅 `!system && !retainExternal` 在该回调调用 Destroy(sibling)。随后才increment disposals、记录callback.exit、置callbackExited=1。即candidate真实composition释放线程重入关闭sibling；不是caller模拟回调。official跳过该重入，reentry保持int.MinValue，随后由caller显式Destroy.Sibling。
- 209–238：Destroy.Drain返回0并记录closeReturn后，最多5秒等disposals==1；两角色健康分支都断言releaseThread非caller；candidate要求Callback.DestroySibling返回0；official要求caller Destroy.Sibling返回0。Deinitialize返回0后，**只有candidate**最多5秒等释放OS线程不存在；最后disposals精确1、completed=true。

### 不应夸大的完成偏序

本次选择retainExternal=false，不进入HS_NOTIFICATION_CLOSE条件编译的存活外部stream合同。Destroy.Drain返回只断言HR0，随后才等dispose1，源码没有强制 callbackOrder < closeReturned 或相反。disposals在callback.exit事件/标志之前递增，因此 SpinUntil(disposals==1)本身不严格证明callback完整退出。最终JSON的callbackExited=1、enter/exit事件及顺序必须回读；不能把头部completed或TRX Passed独自当callback退出证据。candidate worker消失加最终事件可支持本次有限退出，非全进程所有worker或任意回调静止证明。

Callback.DestroySibling HR0是在真实callback内部取得；它不是所有资源/worker在Destroy返回前全部退出的承诺。official无worker退出强断言，不能借它的成功补candidate的重入/worker断言。finally的补救Destroy/Disconnect返回值被忽略，不等于正常cleanup通过；认证必须检查正常calls及completed，并保留失败JSON。

真实资源来源补核：`SystemWicBitmap.cs:27–45`系统CoCreateInstance、decoder选项0、GetFrame及factory/decoder真实Release；`ManagedStreamTests.Lifecycle.cs:132–140,217–225`真实descriptor stream创建，MemoryDispose先Dispose MemoryStream并Free GCHandle再调用Disposed action。本回归无CopyPixels/取色/呈现消费断言，不外推像素正确性。

## 唯一最小角色/参数表

下表每项分别在x86 Release和x64 Release执行一次；C/O只是本文记号，不新增角色入口。精确FQN均加前缀 `WpfGfxShape.ComAcceptance.UceChannelTests.`。

|项|既存方法后缀|DataRow / system / retainExternal|真实编码与独立断言角色|
|---|---|---|---|
|C6|WhenAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates|6 / false / false|open未封包；caller directDestroy排空pending transport，真实释放callback重入Destroy sibling，最终dispose1及有限worker退出|
|C8|同C6方法|8 / false / false|committed；已提交所有权与关闭竞争，真实callback重入Destroy sibling、dispose1及有限worker退出|
|O6|WhenOriginalAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates|6 / true / false|open正常关闭独立控制；不重入，caller显式关闭sibling；dispose1/释放非caller线程|
|O8|同O6方法|8 / true / false|committed正常关闭独立控制；不重入，caller显式关闭sibling；无worker退出强断言|

选择理由：6与8覆盖未提交transport及已提交端点，两者只取健康directClose，不加closed中间态、sticky错误、存活stream、通知、显示或共享变体。candidate 6/8是受影响回归主体；official 6/8是同WIC输入/正常关闭的适用独立控制，**不是同重入合同oracle**。不要求official-first；将来建议每架构C6→C8→O6→O8逐项审核首错即停，x86完成才转x64。至多4参数体/架构，共8，仍须另授权；本次0。不会因official正常分支成功裁决candidate重入等价。

## clean复用及身份限定

相对 `WpfGfxShape/`，sourceDirectory分别为 `artifacts/async-shared-ownership-x86-r1/clean` 与 `artifacts/async-shared-ownership-x64-r1/clean`；入口均根 `WpfGfxShape.ComAcceptance.exe`，不用publish根，不混旧任务回收包。

两份clean DLL均经公开API元数据工具独立确认以上两个public void方法及int queueState参数存在；HS surviving-external两个条件方法不在这两份公开方法清单。**元数据方法存在不是DataRow UID，也不证明测试体已执行。** DataRow值来自现存源码；最终发现应再证包内行值与节点关联。

|架构|clean/aot.sha256（本次全文读取）|既有同clean运行的测试身份（引用ownership收尾）|既有official actual|
|---|---|---|---|
|x86|15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620|D8A5F57FE4A61CC28459A89DCB50DC75E0ADE80E1D0AEDC35D7E18AC5069A7BB / e207b681-4c15-453e-b0a4-a1690fa3db78 / .NET10.0.13 X86 pointer4|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|
|x64|54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD|4A8356201F062B01161744C6B3BBF7D9AB7DB5909F6F12A9389806FFB6B6ED43 / d41adfe8-8904-4c9b-887d-8d1f5979ba46 / .NET10.0.13 X64 pointer8|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|

两份package-files.txt头部1–12为正式exe/dll/deps/runtimeconfig/MTP及MSTest，根official在x86行413/x64行414；Native候选在619/620，末尾aot.sha256及package-files.txt；与本轮读取的03-preflight、02-publish-return正式Release/self-contained打包链一致，无新DefineConstants。两份clean deps均文本定位到Microsoft.Testing.Platform/2.0.1（x86行1306/1877，x64行1309/1880），均为MSTest4.0.1发行配置；发现仍应核diag实际版本。未逐字重认证全部621/622文件或重算所有SHA；这是零build/publish复用可行性，不是新包全字节认证。

NativeRun.Main普通MTP路径直接CreateBuilder/AddSelfRegisteredExtensions/RunAsync，无隐蔽native取色前门；仅显式特殊首参数才分支。AcceptancePaths唯一ModuleInitializer只切工作目录；AssemblyInitialize只读候选PE/架构/SHA并写identity，不调用native取色。仓内另一ClassInitialize属于D3DImageTests，不是UceChannelTests。发现不是调用取色测试；候选identity即使出现仍不能冒official actual。

AsyncDrain的official路径直接按MediaPlayer所在PresentationCore目录取wpfgfx_cor3.dll，candidate按AcceptancePaths.AotDll；最终JSON实际模块sha/path、test SHA/MVID/runtime/arch必须重新绑定上述值。AsyncDrain JSON不含独立moduleHandle/BaseAddress/pointerSize字段，不伪造已有ownership更丰富字段为本合同输出。架构指针宽度可由既存初始化identity辅助。方法公开API读取也不等于本次重新算出了test SHA/MVID。

## 精确参数选择：已认证什么、尚缺什么

已亲读微软官方CLI文档：

- https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-cli-options ：`--list-tests`仅发现不执行；`--filter-uid`接受一个或多个真实test node UID，自1.8支持；`--list-tests json`从2.3才支持，本包2.0.1不能用json格式猜UID。
- https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-running-tests ：MSTest exe支持 `--filter` 的FQN布尔表达式，不是路径式filter。第一次错误URL返回404及搜索无关结果未作为依据，以上正确页面均已实际获取。
- 历史 `replacement-supervised-lifecycle-thread-scope.md:155–190`证实本仓同exe既有list+FQN+diagnostic路径和MTP2.0.1，能在diag回收真实节点UID；旧KS UID不能移到AsyncDrain。

现有 `--supervise-exit` Parse恰收 root/seconds/filter 三参数，ChildArguments把第三参固定放在 `--filter` 后；**没有filter-uid透传**。把UID或`--filter-uid`塞进第三参不是精确UID执行。方法FQN等值会选该DataRow方法组（candidate12行/official6行），不能声称只执行6/8；不猜 Name、DisplayName、queueState 等filter属性，也不把TRX testId/executionId或文件名中的6/8当UID。

本次所查当前ownership回收仅有ownership角色，无本方法参数节点diag；旧AsyncDrain定向原报告有旧任务/TRX但指定旧回收目录当前不存在。当前两clean尚无已认证6/8真实UID；不声称全盘搜索证明永不存在。缺口是**精确参数节点身份**，不是方法/包不存在。

### 具体下一门（仅建议参数，未执行）

另授权每架构一次现有沙箱调用：sourceDirectory取上述对应clean；executableRelativePath=`WpfGfxShape.ComAcceptance.exe`；arguments逐项：`--list-tests`，`--filter`，`FullyQualifiedName=WpfGfxShape.ComAcceptance.UceChannelTests.WhenAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates|FullyQualifiedName=WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates`，`--results-directory`，`Results/async-drain-directclose-discovery`，`--diagnostic`，`--diagnostic-synchronous-write`，`--diagnostic-output-directory`，`Results/async-drain-directclose-discovery/Diagnostic`，`--no-progress`。outputRelativePath=`Results`；localOutputDirectory取对应架构独立的 `artifacts/async-drain-directclose-discovery/readback/x86` 或 `x64`（加WpfGfxShape/前缀）；timeoutSeconds=120。不经supervise-exit，不加json，不加report-trx，不跑体。

此FQN组用于**发现**，允许列出18参数行，不授权执行18体。回读直接返回、stdout/diag，核两方法各自6/8的真实node UID、display/参数关联、实际程序集/架构/MTP版本；保存原始UID映射，不按SHA算法猜UID。若只发现方法级节点、缺参数行UID/diag、零节点、意外其他方法、启动失败或超时，当架构停止，不重试/补help/跑体/重build。必须在测试体执行前静态证实选择项确是一参数节点。

随后另授权的精确体路径应直接同clean exe用 `--filter-uid <该架构真实行UID>`（一体一进程），加既有TRX/diagnostic/results参数，外部180秒上限；不改变现有监督器，也不声称有supervision.json。若授权方要求必须现有supervise-exit监督且发现未提供已认证`--filter`参数行标识，则精确执行仍Blocked，回报而不是新增入口/改监督器。未知UID意味着现在不能提供实值体执行参数，这是必须先发现的准确边界。

## 旧覆盖/新缺口与预注册停止规则

历史 `async-drain-owning-finite-acceptance.md`：57CE56EA65057F8F609AD56E318B459A4248790C1231B5909DDD4DEA6E0A6F74 / win-x64 / .NET10.0.1候选12/12，94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F官方6/6及旧full相关18体有限通过；candidate重入/worker断言只在candidate适用。仅引用报告，未冒本次亲读全部旧TRX/JSON。旧SHA、runtime及official均非当前，不能覆盖15A545/54BC62；当前各两partition合同与ownership有限绿也未执行directClose6/8，更不证明其callback重入/worker退出。HU retainExternal=true旧合同不替代本表。

未来体逐项要求TRX total/executed/passed精确1/1/1、正确方法和参数、实际进程正常退出、无超时/kill/重试、JSON仅该system/queueState/retainExternal=false且completed=true；正常Initialize/Connect/Channel/Sibling/Create/Wrap/Transfer/Disconnect/Destroy/Deinitialize calls均成功，8另有CloseBatch/Commit0，6不得被记录成committed；无显式Flush.AfterDisconnect。最终disposals=1、callbackExited=1、真实enter/exit轨迹、releaseThread非caller；candidate reentry0及Deinitialize后有限worker退出断言必须通过，official reentry哨兵不是失败、caller Destroy.Sibling0才是适用控制。

任何身份不符、参数不唯一/多跑/零跑、missing JSON/TRX/实际退出证据、回调未退出/重复释放、HR非0、超时/崩溃/残留退出失败均首错停，保留全部原件，不放宽5秒/30秒、不重试追绿、不加更多角色、不因official异分支通过豁免candidate失败。直接UID执行没有监督原件时不将工具“完成”文字冒Exit0；若实际返回不能认证退出码或输出收集，则报告证据缺口并停止。

成功最多证明当前架构/当前SHA的关闭、真实释放callback、candidate特定sibling重入及有限worker退出；不证明长期资源趋势、所有线程静止、官方重入等价、Register异常/OOM、图像消费、HW/resize/loss/media/full。最终完整适用矩阵、所有历史问题仍独立Open，不能借此次有限回归升级完整替换。
