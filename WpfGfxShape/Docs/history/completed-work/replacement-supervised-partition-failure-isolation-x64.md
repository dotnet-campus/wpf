# partition-failure-isolation x64 对称验证

## 终态：official/candidate 各真实执行1通过，有限合同关闭

本轮亲自用专用工具执行，终端/脚本/再委派0；源码、项目、资源、包规则、SDK、TFM和Ledger均未修改。2026-10-27取得当前x64新包实证，最终结果后停止，无追加运行。

### 纠正上轮零执行原因

上轮build/publish/Native/沙箱均0确为历史事实，但归因为“缺构建能力”不准确：把普通build必须绑定候选来源、独立obj/bin作为硬门过严。普通build仅编译代码，既有入口足够；不是缺工具、编译失败或产品失败。本轮明确允许默认obj/bin，不新增入口、不修改项目；普通build产物不作为候选验证发布包。

### 构建、发布与clean包

先亲读csproj全51行、Package.targets两打包目标必要范围及本地Directory.Build.props/targets、global.json。无ProjectReference、Native生成或测试执行目标；普通build不publish、不测试、不发Native。

- 正常COM build **1成功**：Release / win-x64 / net10.0-windows，Exit0，133 warnings / 0 errors，4.50秒。分析器未禁用，无重试。构建日志工具保留尾部，不能声称取得全部警告原文。
- 随后正式self-contained COM publish **1成功**：同Release/RID/TFM，使用既有合法`RuntimeFrameworkVersion=10.0.13`、`ComAcceptanceSandbox=true`、`AotRuntimeIdentifier=win-x64`。publish内部正常构建不另算独立build。未传NoBuild/NoDependencies，未发布生产Native；日志无warning总计，不虚构发布警告数。
- 显式`AotDllPath=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/kn-inplace-resize/com-acceptance-package/Native/wpfgfx_cor3.dll`。
- 全新独立输出根`WpfGfxShape/artifacts/partition-failure-isolation-x64-execution/`，PublishDir为`publish/`，ComAcceptancePackageDirectory为`clean/`；默认obj/bin获准使用。沙箱仅上传同一clean，不上传publish根。
- 亲读clean `aot.sha256`为 **40D093988D2F5BA13B40D58C02FA744CE31E84DB0C7DD8FD24C66746335DCF38**，由正式GetFileHash生成，非引用历史文本认证当前字节。两角色AssemblyInitialize再次核当前PE/架构/SHA，aot-identity记录X64/pointer8/40D0。
- runtimeconfig全21行两includedFrameworks均10.0.13；deps开头目标win-x64且NETCore/WindowsDesktop runtimepack均10.0.13。manifest必要段核exe/dll/deps/runtimeconfig、运行时/PresentationCore及Contracts、Native候选、摘要/清单。沿用正式白名单、去重及缺件检查，未遍读622行清单；root官方模块与Native候选的角色分离由下述actual运行身份实测确认。
- 当前源码两精确方法已读；两份TRX进一步确认新test DLL中均存在且各真实运行一次，无离线/运行时独立发现调用。

## 亲读原件与真实测试身份

证据根：`WpfGfxShape/artifacts/partition-failure-isolation-x64-execution/readback/`。official/candidate各`Results/partition-x64-{official,candidate}/`内：TRX全26行、supervision全40行、stdout全12行、空stderr，以及`PartitionFailureIsolation/{业务UID}/business.json`各402行分1–85/86–190/191–290/291–402亲读。两份`Results/aot-identity.json`全文已读。stdout中文存在解码乱码，结构化裁决不依赖乱码摘要；未声称遍读Diagnostic。

|角色|task|TRX testId|executionId|业务自生UID（不是TRX ID）|
|---|---|---|---|---|
|official|44785ac366ea4919b4f7a14e46bd62ba|1e3a533f-c51b-8855-b58a-2f84cc5b1ade|6f18cd6e-e9ea-4ea0-8548-13cdc174caca|6a005cd41a8a45359a46e7ad6e9c8b2c|
|candidate|59063052ff034988947ee651067be954|18b58946-d629-8139-8233-224beea22724|f464e405-6005-471a-b4f3-5c238c7522c7|7cde5a695c224e8393df2c1099d90712|

TRX TestRun id分别`7974b73b-cc59-425b-a5bd-4b72f044e95a` / `3e159633-2797-40de-820e-3fca37d643e4`；testId/executionId在定义/结果/Entry一致。业务UID由正式partial的Guid.NewGuid自生成，不混称MSTest标识。

准确FQN，先official全部实际合同/身份/清理通过才执行candidate：
- `WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives`
- `WpfGfxShape.ComAcceptance.UceChannelTests.WhenAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives`

均用`FullyQualifiedName=`精确等值filter，不用斜杠。各TRX total/executed/passed=1/1/1，failed/error/timeout/aborted/notExecuted=0，Passed。各独立进程`--supervise-exit`，filtered120秒，沙箱工具外180秒；Results唯一路径。两份Reason=exited、ExitCode0、HasExited/OutputCollected=true；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut/OutputDrainCancellationTimedOut均false，SupervisorError=null。外180为工具调用预算，工具仅返回执行完成，不能冒其另有数值Exit。PID official10124 / candidate1488。

## 实际模块与test身份（x64，不借x86 SHA）

共同test SHA256 **B4A1D11011BDB25D6F5F90F0B1EC2763FE012A57F458CA1B50055B409B34B7AB**；MVID **2254f62a-814f-482c-b8d7-d55429bf40fe**；实际`.NET 10.0.13 / X64 / pointer8`。test路径各自task根`WpfGfxShape.ComAcceptance.dll`，监督与业务SHA/MVID相同。

- official requested=actual=`C:\CodingAgentSandbox\Tasks\44785ac366ea4919b4f7a14e46bd62ba\wpfgfx_cor3.dll`；SHA **AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1**；version `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`；moduleHandle=actualBaseAddress=**140720669196288**。
- candidate requested=actual=`C:\CodingAgentSandbox\Tasks\59063052ff034988947ee651067be954\Native\wpfgfx_cor3.dll`；SHA **40D093988D2F5BA13B40D58C02FA744CE31E84DB0C7DD8FD24C66746335DCF38**；version **1.0.0.0**；moduleHandle=actualBaseAddress=**140720477765632**。

两者requestedEqualsActual=true。正式测试按handle匹配ProcessModule.BaseAddress再读actual路径/hash/version，非仅请求路径或初始化文件摘要。初始化两份均40D0仅认证候选文件，不作为official actual角色证据。未以包内PresentationCore推actual WPF加载。

## 真实合同、调用顺序与清理

两份42调用一致：Initialize0/Connect.Async0；A(source0)/S(sourceA)/B(source0)，同connection。三者visual39/完整SetAlpha32（16字节）/初始alpha0.75及flush0；A bitmap95合法创建并flush0后，一次完整CompleteNullSource12（x64 16字节=8+pointer8）。Order28/29 A/S原始HR **-2003303418 = 0x88980406**；未改expected、未依赖公开Peek type6。B新alpha0.5的Send/CloseBatch/Commit/Order33 flush均0；Order34/35真实销毁A/S均0；B再新alpha0.25更新并Order39 flush0；Order40真实销毁B、41 disconnect、42 deinitialize均0。

- official connection1692350268192；A/S/B=1692353498176/1692353499296/1692353500192。
- candidate connection1；A/S/B=2/3/4。

created/destroyed逐项相同，三真实channel不是清理占位；completed/cleaned/disconnected/deinitialized=true、primary=null、cleanupErrors=[]。siblingNegativeRejected/independentNegativeRejected均true，仅结果副本validator负控，不是产品故障注入或新增TRX测试。

## 真实预算及边界

|项目|本轮实际|
|---|---|
|正常COM build|1成功，133 warnings / 0 errors|
|正式COM self-contained publish|1成功|
|Native publish|0|
|沙箱|2，同一新clean，独立进程|
|official/candidate真实体|1通过 / 1通过|
|发现/重试/显示/full/x86重跑|全部0|
|源码/项目/资源/包规则/SDK/TFM/Ledger修改|全部0|
|终端/脚本/再委派|全部0|

历史x86两角色通过及其build1成功133warn0err/publish1成功/沙箱2/Native0原账保留，不重跑。direct累计build4（1败3成）/publish3/official3失败/candidate0、public-control独立1失败、WGC退场/default-build及通用包残项、KS与KN/KO历史账均独立保留，不并账或冲销。

仅当前x64 40D0对照AD1C与历史x86 20B3对照5EBD的非视觉普通错误ABI传播、共享/隔离、失败partition关闭后健康partition继续及有限清理合同具有双架构证据。不是实际HW/backend/Available=true、真实device-loss及恢复、consumer/resize像素、media持续资源趋势、full或最终四格认证。显示线仍冻结，完整替换Open；不再把本x64门列为未测或要求补构建入口。当前授权已全部完成收尾并停止，无自动下一运行。
