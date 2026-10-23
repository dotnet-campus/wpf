# DJ 重启后受控扩大包与定向业务验证

## 1. 范围、规则与结论

本执行者承担调查操作，主代理仅监督。已完整读取当前执行规则、当前120行完整替换方案、DI sandbox-restart-probe、DE upload-boundary-review、现有正式白名单打包 targets、项目/SDK和相关测试源码。报告在操作前创建，本节为结果回读后的最终补完。

**x64 框架依赖包实际业务验证成功：监督父测试19/19，Exit0；BW 官方/候选/官方重复三角色成功，矩形和椭圆均非空且零差异。** 原已验收候选 F34DEAB0… 未重发，在新沙箱 .NET/WPF10.0.1实际加载消费成立。没有运行 full，也不据此宣布完整替换或所有网络问题解决。

仅专用读写/发布及 Windows 沙箱正式纯 C# exe；无终端、直接间接脚本、新 Host/第三工程、异步绕过。零生产/NativeRun/ExitDiagnostics/项目版本/默认断言/默认并行修改。仅新增正式 ComAcceptance 项目内显式 DJ_EXPANDED_PROBE 隔离诊断文件，不影响无此符号的默认编译行为。没有修改其他代理文件或共享交接。

## 2. 包边界：相对 DI 24 文件包

本轮扩大的是实际业务执行范围；**文件集合仍24个，新增相对路径为0**。源码核对确认 BW 所需候选 Native 和 Contracts 已由原正式框架依赖白名单保留，不需要人为膨胀输入。候选 Native 原先仅发现未消费，此次真实 WPF消费。Contracts仍保留，BW本身不读取头文件；不冒称逐文件绝对最小闭包。

- Native/wpfgfx_cor3.dll：复用 DE clean-x64-02/com-acceptance-package 原生产候选，发布前亲读 aot.sha256，运行后再核实际加载 SHA；无生产发布。
- Contracts/wgx_sdk_version.h、Contracts/wpfgfx.def：原白名单已有，未新增。
- 官方同架构同runtime独立模块：直接使用本次沙箱安装 WindowsDesktop.App/10.0.1/wpfgfx_cor3.dll，官方及重复角色独立进程实际加载；无需上传旧版官方DLL。
- ReferenceData 新增0：BW 使用同次独立官方原始像素及重复对照，不读取旧10.0.13 golden。没有为本轮生成或上传 glyph/original-wpf 静态参考。
- 测试程序集因专属诊断重编译，不能视作与DI同一测试二进制。白名单中的exe/dll/deps/runtimeconfig和MTP闭包按正式依赖图生成；不手删插件DLL。

最终源为 artifacts/dj-expanded-probe/clean-x64-final/com-acceptance-package/，全新独有root下正式白名单目录，不是 PublishDir/回收根/父目录。前一 clean-x64 只本地构建一次，补绝对监督路径后在新的 clean-x64-final 最终发布；两次正式发布均成功、有既有分析器警告，非无警告。仅最终包上传一次。

上传输入24项的实际长度在正式项目显式诊断中读取，Results/dj-package-identity.json记录 **9818850字节**（约9.82MB十进制），FileCount=24。其中候选4520448字节、测试DLL1431552字节、exe162816字节；逐文件全部长度已分段亲读原件。此数为上传源清单文件长度总和，不是HTTP线上字节、压缩量或回收目录大小。DI原包总字节未知，因此不造相对增长字节/缩减百分比。不上传150MB自包含runtime，不上传旧Results/TRX/PDB/异架构/跨平台资产。

## 3. 唯一上传、执行、回收与计数

独有产物根 artifacts/dj-expanded-probe/。sourceDirectory=clean-x64-final/com-acceptance-package；本地回收=readback-x64，两者分离，回收未再次输入。外timeout180秒，整任务回收（outputRelativePath省略）。

**taskID fb327da84f7e48ad924404de4773771b**，由工具实际远端程序集路径及监督Executable一致确认。工具返回正式exe .NET10.0.1启动、外层测试1/1、失败0/跳过0、3s333ms，明确结果保存。已证任务创建/正式应用执行/文件回收；不宣称上传起止时间和HTTP逐字节校验已知。工具不显式给进程Exit字段，不能把工具成功伪填外层进程Exit0；内层正式监督记录ExitCode=0。

外层选择唯一 DjExpandedProbeTests.WhenDjSupervisionIsSelectedThenBoundedBusinessSmokeRunsAsync，诊断测试调用未改动的 BL 旧filtered监督。监督root由 AppContext.BaseDirectory 拼接，远端绝对TestResults为 C:\CodingAgentSandbox\Tasks\fb327da84f7e48ad924404de4773771b\Results\dj-x64\TestResults，父TRX=child.trx。无 --full。

内层精确filter：FullyQualifiedName~BwCurvedFillPrecisionTests|FullyQualifiedName~AcceptancePathsTests.WhenSandboxPath|FullyQualifiedName~AcceptancePathsTests.WhenLocalPath|FullyQualifiedName~AcceptancePathsTests.WhenArtifactIsValidated|FullyQualifiedName~ExitDiagnosticsTests.WhenLegacyFilteredBudget|FullyQualifiedName~ExitDiagnosticsTests.WhenBudgetIsOutsideModeBounds|FullyQualifiedName~DjExpandedProbeTests.WhenExpandedProbeRuns。

计数分别记录，不合并重复计算：
- 沙箱请求1、x64上传执行1、x86请求0、纠正上传0、full0。
- 外层TRX Results/dj-outer/outer.trx：1/1 passed、failed0，最终Counters亲读。
- 正式监督父TRX Results/dj-x64/TestResults/child.trx：total/executed/passed=19，failed/error/timeout/aborted/notExecuted=0，最终Counters亲读。
- 19项构成：BW1、路径及身份9、旧filtered合法预算2、越界预算6、DJ身份/包诊断1。预算参数里的 --full 仅解析拒绝用例，不执行full。
- BW再启动官方、候选、官方重复三个角色进程，同一task内；三个supervision全文均ExitCode0/timedOut=false。不是三个上传或三组新沙箱。

正式 supervision.json 全文亲读：Mode=filtered，TimeoutSeconds=120，2026-10-21T08:52:19.8505519Z至08:52:22.8939769Z，Reason=exited，Exit0/0x00000000，TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut均false，OutputCollected=true，SupervisorError=null。输出及诊断完整回收。

## 4. 实际身份与候选消费

实际test SHA BF423728AF784D6CACA67FA4A44648D9F4D8E7640A3433D7508984CDE42B2A95；MVID e3498e76-9192-4784-a06d-b17117a63067。正式监督、包诊断和BW三角色identity一致；不是DE旧test SHA。

- runtime=.NET10.0.1；Architecture=X64，pointerSize=8。
- 实际PresentationCore路径 C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\10.0.1\PresentationCore.dll；文件版本10.0.125.57005；SHA605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654。
- 官方实际加载路径同目录 wpfgfx_cor3.dll；版本10,0,125,57005 @Commit: fad253f51b461736dfd3cd9c15977bb7493becef；SHA94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F。官方重复角色同身份。
- 候选requested=actual=C:\CodingAgentSandbox\Tasks\fb327da84f7e48ad924404de4773771b\Native\wpfgfx_cor3.dll；版本1.0.0.0；SHA **F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7**。实际ProcessModule按加载地址核对，不是仅哈希预期路径。

上述WPF/官方身份来自本次进程及实际模块，不能只凭CLR推WPF patch。本次选择10.0.1不是产品固定版本合同，也不外推其他patch/x86。

## 5. BW完整关键比较与原始内容

证据根 readback-x64/artifacts/bw-curved-fill/precision/。三个identity.json和supervision.json全文、comparison.json全部31行、官方/候选矩形及椭圆原始内容JSON（含alpha）已亲读；六个.pbgra32原始像素已回收，零阈值比较实际读取它们。

comparison：threshold=0，exact=true，hardwareVerified=false。完整四项：rectangle candidate count0/nonempty=true；rectangle official-repeat count0/nonempty=true；ellipse candidate count0/nonempty=true；ellipse official-repeat count0/nonempty=true。

实际场景Rect=(1,1,2^-20,16)，local.M11=1+2^-24、OffsetX=-1，parent.M11=2^20、OffsetX=8；32x24、stride128、Pbgra32。两种图形官方及候选均非空，窄矩形全alpha条带与椭圆变化alpha不同，非双空白相等。候选实际加载旧生产DLL并产生像素，未以官方替候选。backend=RenderTargetBitmap software，明确非硬件验收、非software fallback冒充HW。

## 6. 后续裁决、缺口与交付文件

x64关键证据已闭合，本轮不再用x86探测消耗请求。x86共享runtime是否安装仍未知，没有发生x86缺runtime或网络错误，不能伪分类。没有新的HTTP故障，但不能以一次9.82MB成功证明大包/所有网络问题已解决。

**可进入下一组明确有界的x64更大定向族**：依据实际测试消费链逐项补必要文件并继续使用全新clean输入、明确filter/监督/完整回收。**本轮不授权或执行直接full**：尚缺更大族静态输入与插件闭包的实际验证；若需静态官方ReferenceData，必须本次10.0.1同架构生成或核对身份，不能复用DE10.0.13参考。HW/恢复/资源时序、完整组合、x86均未由BW小场景覆盖。历史full仍未知，不移绑本次test/native SHA。

修改仅 Docs/completed-work/replacement-supervised-expanded-package-probe.md 和 Tests/WpfGfxShape.ComAcceptance/DjExpandedProbeTests.cs（全文件 DJ_EXPANDED_PROBE 条件编译）。正式打包targets/入口/项目版本/生产未改。Ignored证据仅 artifacts/dj-expanded-probe/；最终报告提交前全文分段回读。结论为本次x64定向软件内容与包运行闭合，不是完整替换完成。
