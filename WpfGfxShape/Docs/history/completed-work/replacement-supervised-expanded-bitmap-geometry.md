# DM 扩大真实 BitmapRenderTarget / Geometry 测试族

## 1. 结论与边界
一次正式 x64 filtered 运行完成：**203/203 passed，failed0，监督Exit0**。既有默认业务201项：完整BitmapRenderTarget183、Geometry12、独立ABI6；另身份诊断和默认双模块回归各1。不是原53换名：新增Bitmap166项，旧bitmap17仍包含在完整183中。Br/CK算法控制未选，不能冒充候选消费。完整替换仍未完成。

最先创建本报告；所有产物独有 `artifacts/dm-bitmap-geometry/`。仅新增正式ComAcceptance内全文件 `DM_BITMAP_GEOMETRY` 条件编译 `DmBitmapGeometryTests.cs`，默认不发现。没有终端/直接间接脚本、新Host/第三工程、生产重发、入口/版本/默认打包/捕获/VisualCache/historical/WindowQ/共享交接修改。DL身份修复已正常编译包含，未再改；未禁并行或改阈值/golden/skip/fallback。

已读规则、完整方案120行、最新交接DH/DF/DC段、DK/DL报告、两个正式项目索引、ComAcceptance csproj/global.json/props、监督/入口/打包、相关真实类及官方输入机制。TFM net10.0-windows、UseWPF/Nullable enable，SDK10.0.107/latestFeature不变。大型业务原件并非全部逐行另审，下文限定亲核范围。

## 2. 实际filter、适用分母和输入
以下片段统一加 `FullyQualifiedName~` 并以 `|` 联接，实际原文保存supervision第9行：

| 片段 | 通过/适用分母 | 范围 |
|---|---:|---|
| BitmapRenderTargetTests. |183/183|完整默认类29方法，brush31、path realizer38、float span28、float target6、bitmap draw5及其他锁/clip/layer/ownership/错误输入|
| GeometryUtilityTests. |12/12|normal/empty/degenerate/nonfinite/curves/complex/winding/transform/stroke/relative/callback-reject/null-callback|
| IndependentRealizerAbiTests. |2/2|官方/候选16次失败回调及后续状态|
| IndependentBitmapRealizerAbiTests. |4/4|官方/候选×平移false/true，16次成功成员调用|
| OfficialRasterModuleTests.WhenSameNamedLibrariesAreLoaded |1/1|默认DL双模块身份回归，非内容业务|
| DmBitmapGeometryTests.WhenActualPackage |1/1|显式身份/分母记录，非业务|

精确29方法与每个DataRow分母见 `Results/dm-package-identity.json` 132–283行，全文已读。201业务，不把父监督和两次官方生成角色相加。与DL重合业务35（bitmap17+Geometry12+ABI6），新增候选bitmap166；BW/Br/CK不在filter中，不按全量发现数设分母。

Bitmap主Api真实加载候选。Geometry和独立ABI直接调用同版安装官方及候选。Bitmap两个finiteIntersection DataRow使用现有CreateSourceClipReference：同一正式exe独立进程调用既有官方WPF生成机制，当次ignored目录写输入，再对候选32字节CollectionAssert全等。其他Bitmap很多是固定预期像素/锁/状态合同，不是183项全部实时独立官方差分；空clip/零opacity/错误输入允许预期空白，不能充有效绘制nonempty证据。

不需静态ReferenceData，无旧10.0.13参考，无缺输入/阻塞/skip。选择完整类而非挑绿子集。旧DL53约4.79秒；新增小表面合同与两次官方输入进程，预计数十秒内，选择一组BL120秒、外180秒。类既有DoNotParallelize不变；输入生成器原有180秒局部等待不能扩大BL整体120秒。

## 3. 正式包及逐文件增量
专用正常依赖图Release win-x64 framework-dependent publish成功，显式ComAcceptanceSandbox=true、DefineConstants=DM_BITMAP_GEOMETRY、独有ArtifactsRoot。Native复用DL已验收文件，没有重发生产。存在既有分析器警告，不称零警告。

唯一上传源：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\dm-bitmap-geometry\build\com-acceptance-package`，不是PublishDir/父目录/回收根。

24文件 **9829090字节**；对DL9826018增加3072，仅测试DLL1438720→1441792，新增路径0。以下逐项实测长度，除注明外对DL增量0：

| 相对路径 | 字节 | 增量 |
|---|---:|---:|
| WpfGfxShape.ComAcceptance.exe |162816|0|
| WpfGfxShape.ComAcceptance.dll |1441792|3072|
| WpfGfxShape.ComAcceptance.deps.json |40043|0|
| WpfGfxShape.ComAcceptance.runtimeconfig.json |567|0|
| Microsoft.Testing.Platform.dll |1137192|0|
| Microsoft.Testing.Extensions.TrxReport.dll |144928|0|
| Microsoft.Testing.Extensions.TrxReport.Abstractions.dll |27168|0|
| Microsoft.Testing.Extensions.VSTestBridge.dll |117280|0|
| Microsoft.VisualStudio.TestPlatform.ObjectModel.dll |335408|0|
| Microsoft.TestPlatform.AdapterUtilities.dll |43048|0|
| MSTest.TestAdapter.dll |55344|0|
| MSTestAdapter.PlatformServices.dll |373800|0|
| MSTest.TestFramework.dll |400416|0|
| MSTest.TestFramework.Extensions.dll |32800|0|
| Microsoft.TestPlatform.CoreUtilities.dll |99360|0|
| Microsoft.TestPlatform.PlatformAbstractions.dll |51744|0|
| Microsoft.TestPlatform.Utilities.dll |67104|0|
| Newtonsoft.Json.dll |712464|0|
| Microsoft.Testing.Extensions.MSBuild.dll |59936|0|
| Contracts/wgx_sdk_version.h |641|0|
| Contracts/wpfgfx.def |3881|0|
| Native/wpfgfx_cor3.dll |4520448|0|
| aot.sha256 |66|0|
| package-files.txt |844|0|

实际由任务读取manifest逐文件FileInfo.Length，identity第1–131行已亲读。这是文件长度和而非HTTP发送字节。正式白名单依赖图原样，不称绝对最小；无自包含runtime、历史/回收数据、PDB、coverage或异架构资产，不添加无用数据。

## 4. 唯一任务、TRX和监督
任务 `7976392cbb34466bab154d691a9cdd9c`，绝对根 `C:\CodingAgentSandbox\Tasks\7976392cbb34466bab154d691a9cdd9c`；全任务回收至 `artifacts/dm-bitmap-geometry/readback-x64`。一次运行，无纠正/重试/诊断重跑。

- 原始业务TRX绝对路径：`C:\CodingAgentSandbox\Tasks\7976392cbb34466bab154d691a9cdd9c\Results\dm-bitmap-geometry\TestResults\child.trx`。亲读2395–2402行：total/executed/passed203，failed/error/timeout/aborted/notExecuted等0，Completed。搜索无Failed或RunInfo节点，非仅控制台判断。
- 父TRX绝对路径：同根 `Results\dm-parent\parent.trx`，全26行亲读1/1 passed，只是监督入口，不能与203相加。
- `Results/dm-bitmap-geometry/supervision.json` 全39行亲读：filtered/120秒；09:24:08.2098885Z–09:24:16.5765531Z，约8.367秒；Reason exited、Exit0/0x00000000、HasExited/OutputCollected=true；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，SupervisorError=null。外预算180秒，工具不单独给外进程Exit数值，不伪填。
- 同步Diagnostic/stdout、官方输入TRX/JSON、Geometry12和ABI业务原件完整回收。失败分类：产品差异0、夹具失败0、缺输入0、环境阻塞0、崩溃/超时0、skip0。未触发停止扩量后的诊断额度。

## 5. actual身份
identity第1–34行与监督一致。测试SHA `337A1BEC17311B956987A12C104450432414882D19FD99AF28D7278BAC767113`，MVID `0e3dc063-7feb-4120-a29c-16823e38e34f`；实际.NET10.0.1 X64，pointerSize8。

- WPF安装路径 `C:/Program Files/dotnet/shared/Microsoft.WindowsDesktop.App/10.0.1/PresentationCore.dll`，版本10.0.125.57005，SHA `605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654`。
- 官方同目录wpfgfx_cor3.dll，实际句柄/模块基址7FFC17B50000，SHA `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`，10,0,125,57005，commit fad253f51b461736dfd3cd9c15977bb7493becef。
- 候选任务根Native/wpfgfx_cor3.dll，实际句柄/基址7FFC0C9E0000，SHA `F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7`，1.0.0.0。

身份诊断按NativeLibrary实际返回句柄Single匹配ProcessModule.BaseAddress，再哈希实际模块路径；不按basename任意First或仅哈希预期路径。

## 6. 内容和状态亲核范围
六份 `Results/independent-{bitmap-}realizer-*.json` 全文已读：bitmap各45行、失败各34行。成功ABI官方/候选16个0、Ensured/Brushes/Effects16、ContextMatches=true，MatrixOffset80/TextureOffset208；未平移像素 `[0,0,255,255,0,0,255,255]`，平移 `[0,0,0,0,0,0,255,255]`，逐值相同且非空。失败ABI双方16次E_ACCESSDENIED、Calls16、Adapter0/Display4294967295、ContextMatches=true，后续绿色像素base64 `AP8A/wD/AP8=`。只证明成员ABI与后续状态，不证明寄存器/ESP或通用泄漏收敛。

以下同次官方输入两原件各65行全文亲读：
- `Results/source-clip-reference/d10dbd584d664d389f92abd3376b1615/ReferenceData/source-clip-intersection.json`
- `Results/source-clip-reference/43c427bf847e41e2893b3bca403178f7/ReferenceData/source-clip-intersection.json`

均当前10.0.1官方SHA94C06484…，4×2/stride16，红/alpha两行72,191,84,0与36,96,36,0，确实非空。原测试对候选32字节CollectionAssert全等通过，包含锁失败后重试与最后重新锁成功。首官方角色TRX30行全文已读1/1；第二角色控制台1/1及父消费通过已读，未声称第二角色TRX全文已审。输入在当前任务ignored目录生成，不是旧10.0.13拷贝。

其余Bitmap像素/HDR/clip/layer/ownership/锁恢复合同原样执行通过；很多固定预期，部分纯ABI/错误输入或状态，不能全称nonempty+独立官方像素闭包。Geometry12原件完整回收，通过事实来自既有差分断言及TRX，未逐份大型轮廓全文另审。Geometry保留既有数值容差/coverage，不宣称全数值逐位相同。没有新增内容断言或降低原合同。

## 7. 下一最小切片与未完成项
当前族失败0、阻塞0、skip0；无生产差异需修复。已达数百既有默认测试，不为扩量继续无依据追加。下一最小切片建议：对新166项中仅固定预期的成功bitmap内容，复用正式exe独立官方角色补同版/nonempty差分闭包，先核私有ABI适用性；或单独探测x86安装runtime/WPF支持再决定适用运行。

未跑full，不宣称物理GPU/device-loss/resize恢复、长期资源收敛、任意.NET/WPF版本兼容或完整替换。交付仅本报告与条件诊断类；候选生产SHA、正常业务、默认打包和共享交接不变。最终报告分段全文回读后提交。
