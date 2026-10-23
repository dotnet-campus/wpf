# DK：重启后 x64 有界定向族

## 1. 结论与执行边界

**一次正式 filtered 运行完成：52/52 执行，40 passed、12 failed，监督 Exit2，无 timeout/kill/drain 超时。失败全部为 Br5/CK7 的同名模块身份枚举夹具错误，不能称整族通过。** 没有缺输入证据，没有纠正上传、重试或 full。

主代理仅监督，本执行者完成探索、编码、发布、执行和证据亲读。无终端、直接/间接脚本、新 Host/第三工程。原验收候选、生产实现、NativeRun/ExitDiagnostics、项目版本、打包默认规则、断言/阈值/golden/skip/并行设置均未改。仅新增现有正式 ComAcceptance 内 `DkX64FamilyTests.cs`，全文件 `DK_X64_FAMILY` 条件编译，默认不发现。运行产物只在独有 ignored `artifacts/dk-x64-family/`。

已读 current-execution-rules 全27行、完整替换方案全120行、最新交接 DH/DF/DC段、DI全文、DJ分段全文、正式csproj/global.json/props、白名单框架依赖打包与实际族源码。SDK10.0.107/latestFeature，TFM net10.0-windows/UseWPF/Nullable enable，不修改版本。

## 2. 实际族、适用范围与分母

实际筛选保存在 `readback-x64/Results/dk-x64/supervision.json` 第9行，以下每项以 `FullyQualifiedName~` 前缀、`|` 联接：

- `BwCurvedFillPrecisionTests.`：1，passed1。真实候选 WPF 软件 fill，三角色独立进程。
- `CkFillIntegerTests.`：10，passed3（舍入控制）、failed7（栅格控制）。链接生产算法的托管控制，不计 Native 消费。
- `BrFillIntegerTests.`：5，failed5。独立整数细分/官方栅格控制，不计 Native 消费。
- `GeometryUtilityTests.`：12，passed12。用户所指 PublicGeometry12 的实际符号为此类，不存在名为 PublicGeometryTests 的类；normal/empty/degenerate/nonfinite/curves/complex/winding/transform/stroke/relative/callback-reject/null-callback，直接官方/候选公开几何出口比较。保留既有数值容差及轮廓覆盖检查，不将其说成所有数值逐位相同。
- `IndependentRealizerAbiTests.`：2，passed2。官方/候选各16次失败回调、上下文及后续绿色像素。
- `IndependentBitmapRealizerAbiTests.`：4，passed4。官方/候选×平移false/true，每项16次成功member调用、红色非空内容。
- `BitmapRenderTargetTests.WhenBitmapCopyBufferIsShort`：2，passed2。16/26格式短缓冲哨兵、锁释放后读取。
- `BitmapRenderTargetTests.WhenNativePathHasNoBrush`：8，passed8。16/26格式×finite/infinite×locked，真实候选表面锁合同。
- `BitmapRenderTargetTests.WhenNativeBitmapClipIsEmpty`：6，passed6。16/26格式空/NaN/负无穷裁剪和依赖消费顺序。
- `BitmapRenderTargetTests.WhenNativeOpaqueLayerClears`：1，passed1。真实候选图层边界清除像素。
- `DkX64FamilyTests.WhenPackageIdentityIsSelected`：1，passed1。显式输入身份诊断，不计业务。

合计既有默认业务51项＋身份诊断1项=52；通过既有业务39＋身份1=40。候选/官方适用族与算法控制分开，不按1792全量发现数设分母；无全匹配filter或full伪装。没有为了扩大分母添加 CQ206，其旧静态基线依赖未在本轮引入；也不扩大到需要额外参考的整个 BitmapRenderTarget 类。

选中bitmap方法无静态文件输入；Geometry/ABI直接使用安装官方模块；BW同次官方/候选/官方重复原始像素。CK/Br同进程官方WPF实时生成像素，不用旧golden。ReferenceData新增0、生成任务0，不混入10.0.13输入。

## 3. 正式包、全部输入与大小

专用发布成功（退出0，有既有分析器警告；不是无警告），x64 Release、framework-dependent、ComAcceptanceSandbox=true、DefineConstants=DK_X64_FAMILY。原生候选直接复用 DE clean-x64-02 已验收DLL，没有重发生产。

精确上传 sourceDirectory：
`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\dk-x64-family\build\com-acceptance-package`

它是正式白名单输出，不是PublishDir、回收根或父目录。唯一执行exe `WpfGfxShape.ComAcceptance.exe`。正式依赖图闭包24文件，实际长度总和 **9818850字节**，与DJ相同总字节及路径集合；新增路径0，不添加无用文件。测试程序集因显式诊断不同，SHA不同，不称逐字节同包。全清单由 `Results/dk-package-identity.json` 1–126行亲读：

| 相对路径 | 字节 |
|---|---:|
| WpfGfxShape.ComAcceptance.exe | 162816 |
| WpfGfxShape.ComAcceptance.dll | 1431552 |
| WpfGfxShape.ComAcceptance.deps.json | 40043 |
| WpfGfxShape.ComAcceptance.runtimeconfig.json | 567 |
| Microsoft.Testing.Platform.dll | 1137192 |
| Microsoft.Testing.Extensions.TrxReport.dll | 144928 |
| Microsoft.Testing.Extensions.TrxReport.Abstractions.dll | 27168 |
| Microsoft.Testing.Extensions.VSTestBridge.dll | 117280 |
| Microsoft.VisualStudio.TestPlatform.ObjectModel.dll | 335408 |
| Microsoft.TestPlatform.AdapterUtilities.dll | 43048 |
| MSTest.TestAdapter.dll | 55344 |
| MSTestAdapter.PlatformServices.dll | 373800 |
| MSTest.TestFramework.dll | 400416 |
| MSTest.TestFramework.Extensions.dll | 32800 |
| Microsoft.TestPlatform.CoreUtilities.dll | 99360 |
| Microsoft.TestPlatform.PlatformAbstractions.dll | 51744 |
| Microsoft.TestPlatform.Utilities.dll | 67104 |
| Newtonsoft.Json.dll | 712464 |
| Microsoft.Testing.Extensions.MSBuild.dll | 59936 |
| Contracts/wgx_sdk_version.h | 641 |
| Contracts/wpfgfx.def | 3881 |
| Native/wpfgfx_cor3.dll | 4520448 |
| aot.sha256 | 66 |
| package-files.txt | 844 |

MTP插件按既有依赖图保留；Contracts延续原白名单，候选用于真实调用；不声称逐文件绝对最小。无自包含runtime、旧Results/历史、ReferenceData、PDB、coverage或异架构资产。此数是文件长度之和，不是HTTP发送字节/压缩量。

## 4. 唯一任务、预算与精确证据路径

实际taskID **7074b35a6fca494bb3a215109943125c**，工具实际远端程序集及监督Executable一致。一次上传执行、一次整体回收；未指定outputRelativePath，回收整个任务至 `artifacts/dk-x64-family/readback-x64/`，回收根没有再作上传输入。外timeout180秒。

外层只选择 `DkX64FamilyTests.WhenDkFamilyIsSelected`，它将任务内绝对root传给现有BL filtered监督。实际父TRX：
`C:\CodingAgentSandbox\Tasks\7074b35a6fca494bb3a215109943125c\Results\dk-x64\TestResults\child.trx`

本地精确同构路径 `readback-x64/Results/dk-x64/TestResults/child.trx`（目录枚举未列出不等于不存在，已直接读取）。Counters/RunInfo亲读第660–670行：total/executed52、passed40、failed12，error/timeout/aborted/notExecuted等0；RunInfo仅Exit2。失败Br5/CK7的异常见TRX与完整stdout，不删测试。

`Results/dk-x64/supervision.json` 全39行亲读：filtered/120秒，09:06:44.4917671Z至09:06:49.0620369Z，Reason=exited、ExitCode2/0x00000002；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，OutputCollected=true、SupervisorError=null。不是产品卡死或父预算误杀。BW既有每角色100秒、STA join30秒已读；本次三个角色实际正常很快退出。NativeRun通用RoleAsync三分钟角色不在本次所选族内。

外层 `Results/dk-outer/outer.trx` 第34–42行亲读：total/executed1、passed0、failed1，其断言只是监督Exit2，不能与52项重复累加。工具未单独返回外层进程Exit字段，不伪填工具Exit；内层Exit2有正式记录。

## 5. 实际身份与版本

本次测试SHA **DE3A143E7FD4EA554B0FB475D1E754230C9621D49EF09C43969A8ABB2D6C079C**，MVID **4e85c7cb-57a0-4004-a85c-4bdeaece5bdb**。监督、包诊断、BW三角色相同。

实际.NET10.0.1，X64、pointerSize8。PresentationCore实际路径 `C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\10.0.1\PresentationCore.dll`，版本10.0.125.57005，SHA `605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654`。

官方源为该同版同架构安装目录下 `wpfgfx_cor3.dll`，版本 `10,0,125,57005 @Commit: fad253f51b461736dfd3cd9c15977bb7493becef`，SHA **94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F**。不是上传旧官方模块。

候选版本1.0.0.0，SHA **F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7**；BW actual=requested=`C:\CodingAgentSandbox\Tasks\7074b35a6fca494bb3a215109943125c\Native\wpfgfx_cor3.dll`。BW按实际模块加载地址取身份，不是只哈希预期文件。不能外推10.0.13与10.0.1任意兼容。

## 6. 已亲读的关键业务及角色证据

`readback-x64/artifacts/bw-curved-fill/precision/`：三角色identity全文各14行、supervision全文各17行；三角色TRX第20–26行均1/1 passed、failed0，三者Exit0/timedOut=false。comparison全文31行：threshold0、exact=true、hardwareVerified=false；rectangle/ellipse分别candidate及official-repeat共4比较均count0/nonempty=true。六份原始.pbgra32完整回收，由既有比较器实际读取；本轮未在文本工具中逐字节展开二进制，不冒称另写独立像素校验器。backend明确RenderTargetBitmap software。

`Results/independent-bitmap-realizer-{False,True}-*.json` 四原件全文各45行亲读：官方/候选identity同本次；MatrixOffset80、TextureOffset208，16个返回值均0，Ensured/Brushes/Effects均16、ContextMatches=true。无平移像素 `[0,0,255,255,0,0,255,255]`；平移 `[0,0,0,0,0,0,255,255]`，官方/候选逐值相同且非空。这是成功真实Native消费，不是CK算法控制。

`Results/independent-realizer-{False,True}-*.json` 两原件全文各34行亲读：每项16次E_ACCESSDENIED(-2147024891)，Calls16、Adapter0、Display4294967295、ContextMatches=true，PixelsBgra=AP8A/wD/AP8=（两绿色像素），官方/候选一致。范围为失败回调与后续状态，不冒称寄存器/ESP直接观测。

Geometry12的业务JSON全部已回收；本轮通过事实来自TRX及既有断言，未声称亲读12份大型原始轮廓全文。bitmap合同方法的像素/锁/哨兵断言保留，TRX通过不是为其新增独立官方角色。角色与父计数不合并重复计算。

## 7. 12项失败分类与最小下一步

**分类：夹具身份选择错误，非缺输入/版本参考/环境前置或已证产品差异。** Br样本0–4及CK栅格样本0–6全部抛 `InvalidOperationException: Sequence contains more than one matching element`，在各Compare中 `process.Modules.Cast<ProcessModule>().Single(m => ModuleName == wpfgfx_cor3.dll)`。同一父进程已由bitmap/几何调用加载候选与官方两个同名模块，因此“basename全进程唯一”的前提不成立；源码与本次栈相符。它发生在像素生成后、身份JSON落盘前，不能将未写出的differences或nonempty猜为0/true，也不能说这12项像素已通过。

最小根因建议：后续获准的夹具修复应从实际WPF加载的官方模块句柄/完整路径及加载地址绑定身份，核对SHA和同架构，不能改成任意First、候选fallback、关闭并行或删测试。Br/CK两处同类问题一起核对，并维持零阈值及非空断言。更强隔离可按既有正式exe角色独立官方控制，但须明确预算/角色闭包；本轮不擅改默认夹具或为绿重跑。

本轮按授权在报告具体最小根因后停止运行：没有确定缺输入，故不使用“最多一次纠正”额度。扩大包运行本身已证明24文件支持这52项发现执行及主要消费族，不证明全部业务通过。

## 8. 剩余条件与交付

下一可执行扩展条件：先授权并修复Br/CK实际模块身份绑定，保留这52项与当前候选SHA复测；之后按真实输入依赖选择更多BitmapRenderTarget/Geometry族。需要静态参考时，使用现有官方机制在当前同版同架构进程生成到ignored输入，记录actual SHA/版本，失败准确阻塞，不搬旧10.0.13数据。CQ206本轮未加入，不能报告已覆盖。

x86共享runtime未新增探测，仍未知；没有双架构大包。未跑full，未验证物理GPU/device-loss/resize恢复，也不证明完整替换完成。历史full未知不移绑本次SHA。所有失败保留，无skip/阈值/golden/fallback调整。

交付改动仅本报告与 `Tests/WpfGfxShape.ComAcceptance/DkX64FamilyTests.cs`；独有ignored正式包/完整回收在 `artifacts/dk-x64-family/`。本报告最终全文分段回读后提交，不以初稿或结果未读关门。
