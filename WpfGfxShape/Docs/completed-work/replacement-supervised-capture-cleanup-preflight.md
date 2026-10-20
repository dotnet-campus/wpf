# 第四批 L：捕获清理修复与全量输出预检（第五批 N 证据收尾）

## 结论与边界

本报告已按实际源码和回收证据收尾，不再是实施中初稿。L 修复测试捕获清理异常路径，完成两个正式 exe 的有界输出预检；**未运行全量，捕获 control 仍失败，不能关闭捕获或完整替换。** N 仅专用读写审查和更新文档，未构建、发布、执行测试、脚本或命令，未改生产、项目或测试源码。发布参数来自 L 交接；下表计数及身份由 N 直接回读。

## 实际变更

- `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.DesktopDuplication.cs`：实际 finally 调用共用 `CleanupDesktopCapture`，ReleaseFrame 的失败断言或异常不再跳过后续十个 COM 引用释放。四个数据用例验证返回失败、抛异常、未持有/持有帧的释放次数和引用释放顺序；保留失败与原异常。其余四项为既有内容帧选择边界。
- `Tests/WpfGfxShape.ComAcceptance/NativeRun.cs:21-27`：创建 TestApplication 前，缺少独立参数 `--results-directory` 时补 `Path.Combine(AppContext.BaseDirectory, "TestResults")`，返回 `app.RunAsync()` 的实际返回码。
- `Tests/WpfGfxShape.Tests/SandboxTestEntryPoint.cs`：手写 Main 和 `SandboxOutputPreflightTests` **全部**在 `#if SANDBOX_TEST_ENTRY_POINT` 内；不是无条件增加常规测试。Main 同样设置默认绝对 TestResults 并返回实际运行码；预检写包内 `Results/preflight-identity.json`。

此处修复的是测试清理缺陷，不是候选生产泄漏已修的证据；可控回归覆盖 ReleaseFrame 失败，不宣称任意 COM Release 自身抛异常仍全部释放。

## 最终有界实跑与原始证据

统一证据根（相对工程根）：`artifacts/replacement-capture-cleanup-l/`。下表相对 `readback/`，均为顶层 TRX，子角色不重复累计。

|格|TRX|Counters|身份|
|---|---|---|---|
|COM x64|`com/x64/TestResults/capture-cleanup-x64.trx:148`|total/executed=11，passed=9，failed=2，notExecuted/timeout/aborted/error=0|`com/x64/Results/aot-identity.json`|
|COM x86|`com/x86/TestResults/capture-cleanup-x86.trx:147`|total/executed=11，passed=9，failed=2，notExecuted/timeout/aborted/error=0|`com/x86/Results/aot-identity.json`|
|Managed x64|`managed/x64-final/TestResults/output-preflight-managed-x64.trx:24`|1执行/1通过，其余失败/未执行/超时/中断=0|`managed/x64-final/Results/preflight-identity.json`|
|Managed x86|`managed/x86-final/TestResults/output-preflight-managed-x86.trx:24`|1执行/1通过，其余失败/未执行/超时/中断=0|`managed/x86-final/Results/preflight-identity.json`|

COM 九项通过是四个选择谓词、四个清理数据用例及一个产物身份用例。两项失败均为 `WhenNativeGpuPresentsThenFrontBufferAndClientPixelsAgree` swapEffect=1/3：`No desktop content frame belonging to the current submission arrived.` 这是内容等待断言失败，TRX 的 timeout 计数仍为0，不能将其写成 MTP timeout 两项。

x64 TRX:25-73、x86:25-73 保存 staging/backbuffer=FF123456、真实 Present 提交/完成 QPC，duplication 为 Microsoft Basic Render Driver、DISPLAY1、identity rotation、同 monitor。等待未得到合格内容帧，不能按此前 x64 42/42、x86 6/6 宣布当前 control 已关闭，也不能把它认定为候选绘制失败或物理 GPU 通过。COM 两份 TRX 的 RunInfo 明确记失败退出码2；N 没有另行观测执行接口的进程返回码。

COM 实际候选 SHA：x64 `2BFC4AC24A83EBAF45A4B375CDC5FE1F9C90BD9370D73BD590C259091C5BF58F`；x86 `747F307104A9E51E9689307BD85CAABC60EB60294F7F307E21CF687F77E7A142`。这些是 L 选择的既有产物，不是 M 最终生产身份。

Managed identity 直接记录 .NET10.0.9，X64/pointer8 与 X86/pointer4；两格实际加载 managed `wpfgfx_cor3.dll` SHA 均为 `D4D45D516CEBF5BE39C268D47E5F59C288838BAB5C2959A58355C72D1530837D`。测试程序集 SHA 分别为 x64 `9168A8A672F64F10E22C5F54E532F774DF0C6F547B55B680E67F9B07A2C8859A`、x86 `16F3100FFA0121FBFFB1433BB03ED8442B9D8C9573A9701043B463C2188199CB`。x64 runtimeconfig 的 includedFrameworks=10.0.9。身份能证明实际加载什么，不能证明依赖来自最后生产源码。

## 可复用配置（参数说明，不是命令或脚本）

L 交接已验证：Release，RID 分别 win-x86/win-x64，SelfContained=true，RuntimeFrameworkVersion=10.0.9；该版本仅描述本次环境，不设固定支持合同。输出、包与回收目录必须独有。

- COM：`ComAcceptanceSandbox=true`，`AotRuntimeIdentifier` 对应 RID，`AotDllPath` **显式**指向所选架构 DLL；独立 PublishDir、ComAcceptancePackageDirectory。不要误用默认 x64 或共享包目录。
- Managed：`EnableMSTestRunner=true`、`DefineConstants=SANDBOX_TEST_ENTRY_POINT`、`StartupObject=WpfGfxShape.Tests.SandboxTestEntryPoint`，L 预检另用 `BuildProjectReferences=false`。这些是调用方属性，不是 csproj 已永久配置。SDK 默认编译该文件，但不定义条件符号时没有该入口和预检类；不能仅指定 StartupObject 而忘记条件符号。保留项目所需的其他常量，不把预检配方机械用作常规产品构建。
- 使用 `--filter` 的 FullyQualifiedName 表达式，不是 `filter-method`。COM identity 的实参记录四谓词/清理、呈现 control、PublishedArtifact 的有界筛选。Managed TRX 明确仅执行预检类。
- 开启 `--report-trx` 并设置独有 `--report-trx-filename`；入口仅补结果目录，不自动开启 TRX。缺省绝对 TestResults 已经以上四格验证，不依赖 shell 展开或远端当前目录。回收**整个任务目录**，包含 TestResults 与业务 Results。

## 入口审查与明确转交

1. 条件编译：未发现预检类泄漏到不带符号的常规构建；源码整个文件受条件保护。尚未独立构建常规配置验证，不冒称已验证。
2. 返回码：两个 Main 都直接返回 RunAsync；COM 失败 TRX 记录2，未见吞失败路径。Managed 目前只有成功预检，**受控失败返回码传播实跑未覆盖**，转交入口负责人补一个有界失败验证。
3. 用户参数：独立形式 `--results-directory <path>` 被保留，默认不覆盖它。现有检查仅精确匹配独立 token；对于 `--results-directory=<path>` 等其他写法是否被所用 MTP 接受，本批未验证，不把猜测写成已确认 bug。转交负责人确认支持语法并验证自定义目录、默认目录、报告开关与失败码。
4. **最终依赖风险明确**：Managed 项目有生产 ProjectReference，但 L 使用 `BuildProjectReferences=false`，不会重新构建生产依赖。预检证明输出可回收，不证明最后源码依赖新鲜。最终全量前必须在常规生产配置下先构建最终依赖，再发布测试并核对实际加载身份；不能把旧依赖全量称最后源码全量，也不能将 SANDBOX_TEST_ENTRY_POINT 作为生产依赖构建配置传播。

上述问题交下一有界包，不抢改共享入口/项目。测试建设可独立推进，不因 SHA 归属核验停工；最终验收才要求最后源码与实际依赖闭合。

## 未闭

呈现内容帧超时、Acquire/Release 状态机负控、外部更新/旧帧识别、多屏旋转及官方 geometry/sampler 对照仍开放。L 不覆盖物理 HW、真实 device-loss、持续媒体/资源收敛或四格全量；全局下一包见唯一会话汇总 `replacement-supervised-session-summary.md`。
