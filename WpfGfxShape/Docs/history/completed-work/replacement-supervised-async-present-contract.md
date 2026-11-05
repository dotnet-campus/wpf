# DS_ASYNC_PRESENT_CONTRACT：合法异步事件转交专项

## 1. 最终裁决与范围

**合法无 surface 异步序列四格通过；事件转交和 normal cleanup 已有本轮运行证据。真实内容消费、native duplicate 恰好一次关闭的直接计数、失败态全所有权及历史堆损坏因果未证。没有生产或共享夹具修复。**

先建立本报告并记录下表假设，随后新增测试、发布、实跑、回读再更新结论。仅新增 `Tests/WpfGfxShape.ComAcceptance/DsAsyncPresentContractTests.cs` 和本报告。测试全文受 `DS_ASYNC_PRESENT_CONTRACT` 包围，只有显式编译才发现；没有 Ignore/skip、阈值改动或禁默认并行。未改 VisualCache/historical、NativeRun/ExitDiagnostics、生产、捕获、打包、共享交接或 DR 文件；未新增 Host/工程。无终端、直接/间接脚本。

独有产物绝对根：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\ds-async-present\`。全部本轮 obj/bin/publish/package/回收位于此根。候选复用 DP final 包，未重发生产，不把当前生产源码映射为旧 DLL。

## 2. 初始合同表与核验后状态

|阶段|预先记录的合同/假设|本轮裁决|
|---|---|---|
|partition init/deinit|线程与注销对称，cleanup 不省略|初始化先于 connect，销毁连接后 deinit；四格同一 managed/native owner 线程完成|
|connect/channel|仅异步合法 channel，不复用同步 Event0|connect(0)、资源97；四格 S_OK|
|DuplicateHandle|原柄归调用者，副本转交消费端；失败按接收状态确定|原柄 SafeWaitHandle 复制期保活，每次新副本；正常转交实测，失败态未测|
|ProcessPresent|成功/失败均 SetEvent/CloseHandle|当前官方源码 cleanup 已核；本轮仅无 surface 正常路径|
|batch/flush|接收不等于执行；释放、注销须明确|separate batch、Commit、事件完成、SyncFlush、delete+flush、Destroy、Disconnect、Deinit 全完成|
|无 surface|只证明事件合同，不证明 HW|四格通过；不能升级为内容消费|
|真实 surface|必须已确认 ABI，不猜声明|本轮未取得可据以执行的独立认证前置，不调用 Create/Available；DR 自行核验|

## 3. 阅读与源码审计边界

读取执行规则27行、完整方案120行，DQ current-full-crash-review、CJ present-ownership-audit、CL batch-failure-ownership、BP safe-closeout、BS surface-present-contract，枚举正式工程、读取 csproj/global.json/Directory.Build.props/targets 及打包 targets。部分历史报告首轮长输出截断后补读关键节；不声称所有历史长段逐字无截断。名称搜索未发现 AGENTS。TFM net10.0-windows、Nullable enable、SDK10.0.107/latestFeature、MSTest4.0.1，均不改。

直接核对的官方源码（相对 `src/Microsoft.DotNet.Wpf/src/`）：
- WpfGfx/core/uce/apifunc.cpp:62–120：InitializePartitionManager(int) 建 scheduler/workers，DeinitializePartitionManager() 释放基础设施。
- PresentationCore/System/Windows/InterOp/D3DImage.cs:657–730：DuplicateHandle SAME_ACCESS、不可继承，只发送副本；Present 必须本次提交的 separate batch。真实 WPF 在 send 后 Reset，依赖未 Commit；独立测试先 Reset 再发送/Commit，避免 worker 已置位后 Reset。
- WpfGfx/core/resources/d3dimage.cpp:249–289：无 interop 也进入 cleanup；即使 Present 失败仍 SetEvent，再 CloseHandle。故事件完成不等价于成功消费像素。
- WpfGfx/core/uce/clientchannel.cpp:310–370：Send 录入、不执行；separate batch 自行关闭。550–578：SubmitBatch 后才清 closed 槽。768–840：Destroy 内 SyncFlush，忽略该 flush HRESULT 后继续销毁；SyncFlush 不可并发。
- connectioncontext.cpp:350–384：Submit 后局部 pBatch=NULL，再检查 hr；composition.cpp:490–511：处理后 delete batch。Commit 失败不意味着未接收，更不允许调用者无条件关闭副本/释放传输引用。

候选当前 Core/GeneratedD3DImageResource.cs:130–149 在 finally 完成事件；SameThreadChannel.cs:550–572 在丢弃补偿前清 packet 指针/事件字段。正常消费与丢弃补偿不能双跑同一所有权。源码观察不等于本轮旧候选二进制逐指令证明。

CL 当前源码的同线程失败 UAF 与 serving 二进制因果仍分开；不为兼容危险行为修改候选。DQ x64 task2f63b2fd89e7405fa076d5faa1a65464 的 HistoricalCaptureGeometry 首次 Commit E_HANDLE、Destroy 无 after、C0000374 属历史事实，不是 DS 重现。本轮不运行该旧输入。Event0 到达 ProcessPresent cleanup 有 E_HANDLE 源级解释，但尚未证明历史首次坏写/实际失败指令。

## 4. 独立合法序列与所有权

新测试独立声明16字节 packed PresentPacket（Event 偏移8，x86同为 ulong）；四格布局断言通过。各格独立进程，只选择官方或候选一个测试。

固定序列：initialize(0) → connect(0) → create channel → create D3DImage resource97 → close/commit/flush → 三次〔Reset 原事件 → SafeWaitHandle DangerousAddRef → DuplicateHandle → DangerousRelease → Present separate=1 → Commit → 最多5秒等待 → SyncFlush → Reset/Set/Wait 原事件验证仍可用〕 → ReleaseOnChannel(deleted=1) → close/commit/flush → DestroyChannel → Disconnect → Deinitialize → Dispose 原事件 → cleanup complete。

三次是预设重复次数，不因失败调整；无 surface、无 update pointer、无 callback、无 Create ABI 调用。没有删除 Present 或跳 cleanup。所有导出 HRESULT 必须严格 S_OK，异常不转成功。finally 嵌套保证已创建 channel/connection/manager 仍尝试清理；native 内部无限等待由既有 BL 监督有界终止并判失败。

调用者从不 CloseHandle 已交付副本，也不在失败返回后凭猜测补关；没有查询/关闭可能被复用的裸 duplicate 来伪证唯一释放。本轮未注入 send/commit 失败。若复制成功但尚未发送即诊断 I/O 失败，或 send 部分录入后失败，当前专项不能证明副本回收；这是未覆盖错误分支，不是已通过的唯一关闭合同。

**唯一关闭分层**：调用侧只拥有原事件且由 EventWaitHandle/SafeWaitHandle 管理（显式 Dispose 后 using 的重复 Dispose 是幂等，不是再次 native close）；正常转交副本按源码由 ProcessPresent 关闭。实际证明的是事件完成、原柄仍可用、后续 flush/cleanup/退出正常。未 hook native CloseHandle、未做全局句柄枚举，因此不能声称直接观测每个副本恰好一次关闭；业务 JSON 明确 `NativeCloseCountDirectlyObserved=false`。

## 5. 清洁发布与实际运行

正式现有 ComAcceptance 正常依赖图 Release、win-x64/win-x86、self-contained=true；ComAcceptanceSandbox=true、DefineConstants=DS_ASYNC_PRESENT_CONTRACT；WpfGfxShapeArtifactsRoot 分别为独有 x64/x86，PublishDir 为对应 publish。AotDllPath 指向 `artifacts/dp-comacceptance-full/final-{arch}/com-acceptance-package/Native/wpfgfx_cor3.dll`。复用现有 targets 生成 com-acceptance-package，无项目文件修改、无 NoDependencies。两次发布专用接口均成功；存在既有 analyzer 警告，不宣称零警告。一次空参数发布接口调用失败，未形成构建或运行证据；随后有效 x86 发布成功。

四次均复用未改 BL `--supervise-exit` filtered 模式，内部30秒、外部75秒，仅匹配 `WhenDsOfficialAsyncPresentRepeatsThenEventsAndCleanupComplete` 或对应 Candidate 方法；非 full、不跑旧 Event0、不重试追绿。

|格|task|监督PID/子PID|TRX RunId|结果|
|---|---|---|---|---|
|official x64|7e3093c7f6f54db4b8432c065087feb0|3572/6932|a8727572-99a3-4c6d-ab75-026769151d8c|1执行1通过，Exit0|
|candidate x64|d1ceff4407d745cbbbec4398b98c74da|10448/7120|bbf7a802-8535-403f-9c69-3f3a6604c061|1执行1通过，Exit0|
|official x86|2682aec7533d4acbb9b2ba91ab49caa6|2016/8648|dff8db7d-8705-4a8d-8a35-9d40a05c08b9|1执行1通过，Exit0|
|candidate x86|f6233b9409e24c8aad5d597cd853cfdc|872/3508|873f408a-7719-45af-a394-ea3181715e7a|1执行1通过，Exit0|

四份 supervision.json 全39行、父 child.trx 全26行、stdout 全12行、stderr 空均回读。所有 TRX Completed，其他失败/未执行/中断/超时计数0；监督 Reason=exited、HasExited/OutputCollected=true，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，SupervisorError=null。

亲核 Arguments 的 results-directory 为 `C:\CodingAgentSandbox\Tasks\<上表task>\Results\ds-<role>-<arch>\TestResults`，TRX固定 child.trx，不是相对结果路径。整任务回收到 `run-<role>-<arch>/`，没有仅取摘要。

四份业务 JSON 各387行分段全文回读；阶段文件38行关键正常序列及尾节回读（首轮长输出截断处补尾）。每格三次 Signalled=true，全部 send/commit/flush/release/destroy/disconnect/deinit HRESULT=0，最终 cleanup complete。原柄每次仍可 Reset/Set/Wait，deleted=1 有测试断言。managed/native owner 线程依次为 official x64=6/7096、candidate x64=4/1360、official x86=4/4224、candidate x86=4/9604；没有把 owner 线程记录冒充合成 worker 线程观测。

## 6. actual 身份

测试 SHA/MVID 在监督与业务 JSON 一致：
- x64：`FD592849E564842DFA1BF07AE3D39333B3CDBCEADF3D7CD7D8FA7D68A9C27787`；`de753351-1e5a-4d10-b1ce-f58625265b40`。
- x86：`8ECD1D35DCA103AC7B8FAAD40C2D92D1460A516A4C0CE18B32F3D9D18E250848`；`541e975c-602f-47f2-bca4-a9fe74ea2f70`。

实际模块通过 NativeLibrary 返回基址匹配 Process.Modules，不仅记录预期路径。official 实际为各 task 根 wpfgfx_cor3.dll，candidate 为 Native/wpfgfx_cor3.dll，Requested=Actual。SHA：
- official x64 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`。
- official x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`。
- candidate x64 `F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7`。
- candidate x86 `7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA`。

official File10,0,1326,50504、Product10.0.13-servicing.26504.104、commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；候选 File1.0.0.0、Product1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299。实际 runtime.NET10.0.13，x64/8、x86/4；真实 PresentationCore 为各 task 根，Product10.0.13-servicing.26504.104+4099f839…，SHA x64 `0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31`、x86 `D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D`。这是同版同架构 oracle，不外推其它版本。

## 7. 证据索引、最小建议与未闭项

每格 `artifacts/ds-async-present/run-<role>-<arch>/Results/ds-<role>-<arch>/` 保存 supervision.json、stdout.txt、stderr.txt、stages.jsonl、Diagnostic 和 TestResults/child.trx；业务为同任务 `Results/ds-async-present/<role>.json`。两架构包在 x64/x86/com-acceptance-package，源码最终运行后未修改。

足够提出**最小共享夹具设计建议**：异步连接前初始化；初始和 mutation 两个兄弟 Present 均使用独立 duplicate 而非 Event0；保持原柄、owner线程、separate batch、本次 Commit、事件后 SyncFlush；资源注销/删除排空后 Destroy→Disconnect→Deinit；失败返回不盲补关。该建议有官方源码及双架构无 surface 对照，不是共享修复已验收或可直接合并证明。

仍不足批准真实 surface 夹具修复关门：须整合 DR 已认证 Create/Available ABI 结论（本轮不重复其探针，不改其文件），并提供真实 surface 初始/mutation、Detach/callback 根与引用收敛、consumer 新内容和 cleanup 的独立双架构证据。没有本轮可复用且已认证的真实 surface 前置，故不调用可疑声明、不拿 GetAsSoftwareBitmap/backbuffer 或事件完成补 consumer。

未执行错误输入实验：没有已建立安全、可区分 first-free/second-use 且保留 cleanup 的公开方案；不以无效句柄/指针或主动再入官方悬空 batch 逼错。发送未录入/部分录入/提交失败、zombie 丢 batch、实际 SetEvent/CloseHandle 失败、重复 close 的直接计数、长期线程/句柄趋势均未证。资源删除和正常 deinit 不证明完整线程趋势或 device-loss/resize 恢复。

E_HANDLE 的候选源级解释成立，但 DS 不证明历史 servicing DLL 的具体出错指令或 C0000374 首次破坏位置。无生产修复，完整替换仍未完成；本轮交付是合法事件正常路径四格实测和精确缺证。
