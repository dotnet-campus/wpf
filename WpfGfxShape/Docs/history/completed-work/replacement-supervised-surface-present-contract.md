# BS 真实 surface 异步 Present 合同调查

## 1. 最终裁决与范围

**未完成修复，根因未闭。** 本轮在原生 fresh 内容前置失败处有界停止，没有进入真实 sampler 的 connect、异步 Present 或业务 cleanup 验收。没有引入生产或夹具行为修改，因此无本轮新增行为需要撤回；BO 已撤回状态、BL 同 exe 监督和 BN 阶段/HRESULT 诊断保持不变。

实际执行身份：GitHub Copilot 编程子代理，主代理仅监督。仅专用工具及 Windows 沙箱正式纯 C# exe，无终端、直接/间接脚本、新 Host 或第三工程。未构建、发布或重打包，未绑定 BR 并发源码。

唯一新增受跟踪文件是本报告，先创建后持续写入。产物根 `artifacts/bs-surface-present/`。未修改生产、VisualCache、historical、NativeRun、ExitDiagnostics、BJ CaptureFreshFrame、WindowQ、曲线、项目打包或共享交接。

## 2. 阅读与可复用 API

读取当前执行规则、当前工作项、最新交接与状态的 BQ 节、完整方案各节及 BN/BO/BP/BJ/BQ 报告。长输出有截断，关键尾节补读；不声称所有历史报告逐字无遗漏。查询解决方案及 ComAcceptance 文件清单，读取 VisualCache 前置/提交段、historical 全文、BJ gate、原生 Present/诊断、window owner/output window 设施。

BJ `WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired(uint)` 已是公开正式测试，可直接筛选，无需复制或修改 BJ。底层现有 partial API 包括 ReadDuplicatedDesktopPixel、ReadNativeBackBufferPixel、ReadFreshGateFrontPixel、CompleteNativePresent、windowThread.Invoke。本轮未改变实现或可见性。

VisualCache 仍为 PresentNativeControl → DiagnoseNativePresent → AssertCombinedPixel(0x00563412) → connect(1)。BJ门独立核对 backbuffer、client/screen COLORREF、frontbuffer 和 duplication，适合排除仅红蓝换序误判。BJ 历史真实成功反证继续保留，不能因本轮失败宣称 Basic Render Driver 永远不能产帧。

## 3. 有界执行与退出

复用 `artifacts/bp-safe-closeout/final-{arch}/package`，生产为可追溯 BD 旧候选、测试为 BP 最终快照。**不是旧生产+BS新测试发布，而是旧生产+既有BP测试复跑。** 无新中间图、无公共包覆盖。

既有 --supervise-exit，filter为 FullyQualifiedName~WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired。默认并行不变。先x64内限30秒/外限75秒；其两数据门正常失败耗时超过30秒，仅为收齐失败再作一次x64内限60秒/外限90秒和一次x86同限对照。不更改单帧截止、断言或阈值，不重跑追绿。

|产物根下目录|task / 监督PID / 子PID|监督与TRX|
|---|---|---|
|native-x64/bs-native|18d9a6b20f8444d8adfe5f11de8556a1 / 6384 / 4200|Reason=timeout，TimedOut=true，KillAttempted/KillReturned=true，ExitCode=-1/0xFFFFFFFF；无完成TRX|
|native-x64-complete/bs-native|5693314defb646fbab11ed92f55b56c1 / 8720 / 1584|Reason=exited，ExitCode=2/0x00000002；2执行/0通过/2失败|
|native-x86-complete/bs-native|3863b247efc74696b9b194e5cddd1c39 / 2636 / 9448|Reason=exited，ExitCode=2/0x00000002；2执行/0通过/2失败|

三份 supervision.json 全文回读：Cancelled=false、HasExited=true、OutputCollected=true、OutputDrainTimedOut=false、SupervisorError=null。后两份TimedOut/KillAttempted/KillReturned均false。首次timeout不是原生崩溃证据，后两次退出2不是成功验收。

完整TRX位于两完整目录各自 `bs-native/TestResults/child.trx`。亲读x64第210–218行、x86第201–209行：ResultSummary=Failed，total/executed/failed=2/2/2、passed=0，error/timeout/aborted/notExecuted/disconnected及其余异常计数0，RunInfo明确退出2。没有sampler/full TRX。

## 4. actual 身份

实际运行均 .NET 10.0.13，测试路径 `C:\CodingAgentSandbox\Tasks\<task>\WpfGfxShape.ComAcceptance.dll`。身份由实际监督JSON获取并与完整任务业务JSON核对。

|架构|测试SHA256|MVID|
|---|---|---|
|x64，两次相同|3618466036359A53BC1030F6F3BB949B7BBEA8304E519A185EB41466B5441FBC|bb4fb7f5-a654-452a-9b3b-defec0e1a303|
|x86|83B35C8205BF24B541FE663FA7BCBED3DB75ED3655FEB6B1089A35E93DB89DDD|51a184a4-864c-4499-8282-c5e2c4e04db8|

业务JSON pointerSize为x64=8/x86=4。x64原生诊断实际计算Native候选SHA为1C381539E7C82DDD75908C0563F0AC062E583166BDB23AEE066BDD21E838752F，与BD旧候选吻合。BP/BQ继承的x86候选SHA为E0D3F5EFEDFACFD5CC3491DD43B194E19028E8B541A462123C8F73C73D6E25B5；此处不冒称本轮已独立重算x86候选SHA。原生门只执行系统D3D9/DXGI，不表示官方/候选sampler已实跑。未声称新的官方模块版本验收。

## 5. 内容失败与证据边界

完整两任务收取整个任务目录：Results、监督、stdout/stderr、diagnostic及TRX。关键业务为两完整目录各自 `Results/bj-fresh-{1,3}.json`。已读两份effect1业务及effect3身份/首帧；x64 effect1异常长字符串有截断，不声称四份业务JSON全文已读。

双架构effect1 red/blue/red：expected/backbuffer为FFFF0000→FF0000FF→FFFF0000，Present=0，submitThread=owner（x64=2012、x86=3360），submitted/completed非零递增。但duplicated=0，clientPixel=screenPixel=13348487（00CBAE87），frontResult=0、frontPixel=4287082187（FF87AECB），accepted=false。effect3首红相同。错误是期限内没有属于本次提交的desktop content frame。

日志显示Microsoft Basic Render Driver、DISPLAY1 1920x1080、window/output monitor一致；初始duplication pointer-only帧lastPresent=0，之后0x887A0027/resource0。GPU backbuffer已更新，而client/screen/frontbuffer三独立观察共同停在旧色，不是简单COLORREF红蓝换序；不能改期望色求绿。仍未排除窗口可见性/桌面合成状态/交付时序，不声称全部读回及时序前置已排除，更不推翻BJ过去成功。

## 6. 合同完成度与下一定位点

既有BN/BP审查结论保留：同步channel不应发送D3DIMAGE_PRESENT；异步应初始化partition manager、正确创建channel、DuplicateHandle转交事件，在通道/连接释放后反初始化。VisualCache初始和mutation两处同因偏差仍在。**本轮未完成全部ABI、channel分配/release、初始化/反初始化线程及原柄/复制柄成功失败责任的独立新验收**，不把历史无surface最小控制冒充真实surface证明。

确切下一定位点：原生 Clear/backbuffer → Present → query/DwmFlush → frontbuffer/desktop duplication 的桌面交付边界。保持同一窗口与设备，比较BJ成功任务与本轮Default桌面、owner/window覆盖、Present完成、pointer-only初始帧和后续lastPresent轨迹，隔离backbuffer更新未进入frontbuffer的原因。不是继续无信息full或重试追绿。

若要修改共享窗口/捕获/入口设施，需先报告具体修改点并协调，本轮没有越界。前置稳定后再在VisualCache/historical范围实现异步、partition及初始/mutation/兄弟路径事件唯一所有权，验证发送成功与失败两条不双关；正式官方/BD候选×双架构须有重复内容、callback、cleanup及Process.ExitCode=0证据。

## 7. 保留状态

无新增默认失败测试，无skip/阈值修改/禁并行，无删Present绕过消费、无RTB替HW、无跳过DestroyChannel。既有BL/BN诊断和本轮失败产物保留。生产/夹具行为没有本轮修改，无需撤回他人代码。

E_HANDLE与首次堆损坏位置均未闭；真实surface官方/候选双架构正常退出、重复内容、事件回调及释放证据均未取得。仅交付有界失败调查，不声称异步Present合同已修复或完整替换完成。
