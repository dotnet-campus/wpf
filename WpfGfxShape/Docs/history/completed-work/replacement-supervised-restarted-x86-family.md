# DN：新沙箱 x86 共享 runtime 与对应 DM 族验证

## 1. 最终裁决
**唯一轻量发现请求在apphost层失败：`.NET location: Not found`，Architecture=x86。按要求立即停止业务，未运行对应203族。** 不属于网络故障，不是候选产品失败，也不是203项失败或通过。

本报告最先创建；执行者完成探索、正式发布、沙箱请求和证据回读，主代理仅监督。所有新产物独有 `artifacts/dn-x86-family/`。无终端、直接/间接脚本、新Host/第三工程、自包含绕过、框架安装、重试或full。

## 2. 调查与最小修正
已读执行规则27行、完整方案120行、DI74行、DJ69行、DL核心身份修复及实际身份、DM实际filter/输入/结果/边界；索引正式项目并读取ComAcceptance csproj/global.json、根props/targets、白名单打包、DmBitmapGeometryTests全文件和Bitmap官方输入生成机制。未冒称重新审计DL全部大型业务原件。

TFM net10.0-windows、UseWPF/Nullable enable、SDK10.0.107/latestFeature未变。DM显式诊断原末条断言写死x64候选SHA，直接x86运行将产生假身份失败。唯一代码修改为 `Tests/WpfGfxShape.ComAcceptance/DmBitmapGeometryTests.cs`：在原DM_BITMAP_GEOMETRY条件内按ProcessArchitecture选择既有验收x86/x64 SHA，其他架构明确不支持。原x64预期和无符号默认编译不变，断言未降低；发布编译已验证，托管诊断执行因runtime阻塞未验证。

生产、入口、NativeRun、ExitDiagnostics、项目版本、默认打包、捕获、VisualCache、historical、WindowQ、共享交接均未改；DL官方完整路径/基址绑定和默认双模块回归未改；没有skip/阈值/golden/fallback或禁默认并行。

## 3. 正式发布与候选
专用发布成功：Release、win-x86、framework-dependent、ComAcceptanceSandbox=true、DefineConstants=DM_BITMAP_GEOMETRY、AotRuntimeIdentifier=win-x86，ArtifactsRoot独有 `artifacts/dn-x86-family/build/`。日志60行，含既有分析器警告，不称零警告；未重发生产。

候选源：`artifacts/de-upload-review/clean-x86-02/com-acceptance-package/Native/wpfgfx_cor3.dll`。文件存在，既有发布GetFileHash对实际文件计算SHA256，新包aot.sha256实读为 **7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA**，与验收值一致。不是官方替候选，不是仅引用历史hash。

唯一上传源绝对路径：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\dn-x86-family\build\com-acceptance-package`。不是PublishDir、父目录、回收根或旧ReferenceData。独立回收为 `artifacts/dn-x86-family/readback-list-x86/`，未再次上传。

源递归枚举与package-files.txt全24行一致，正常依赖图24文件，新增依赖路径0。无自包含runtime、旧Results、ReferenceData、PDB、coverage或异架构资产。清单：

1. WpfGfxShape.ComAcceptance.exe
2. WpfGfxShape.ComAcceptance.dll
3. WpfGfxShape.ComAcceptance.deps.json
4. WpfGfxShape.ComAcceptance.runtimeconfig.json
5. Microsoft.Testing.Platform.dll
6. Microsoft.Testing.Extensions.TrxReport.dll
7. Microsoft.Testing.Extensions.TrxReport.Abstractions.dll
8. Microsoft.Testing.Extensions.VSTestBridge.dll
9. Microsoft.VisualStudio.TestPlatform.ObjectModel.dll
10. Microsoft.TestPlatform.AdapterUtilities.dll
11. MSTest.TestAdapter.dll
12. MSTestAdapter.PlatformServices.dll
13. MSTest.TestFramework.dll
14. MSTest.TestFramework.Extensions.dll
15. Microsoft.TestPlatform.CoreUtilities.dll
16. Microsoft.TestPlatform.PlatformAbstractions.dll
17. Microsoft.TestPlatform.Utilities.dll
18. Newtonsoft.Json.dll
19. Microsoft.Testing.Extensions.MSBuild.dll
20. Contracts/wgx_sdk_version.h
21. Contracts/wpfgfx.def
22. Native/wpfgfx_cor3.dll
23. aot.sha256
24. package-files.txt

**逐文件实际字节及总字节未取得**：专用目录枚举不提供长度，DM诊断FileInfo统计没有机会执行；遵守缺runtime即停止，不追加审计exe/请求。不借用DM x64的9829090或DE x86自包含147783111字节。HTTP传输字节也未知。这是明确证据缺口，不称完整大小审计。

## 4. 唯一发现请求及精确启动错误
执行正式 `WpfGfxShape.ComAcceptance.exe`，参数为 `--list-tests --filter FullyQualifiedName~DmBitmapGeometryTests.WhenActualPackage --results-directory Results/dn-list-x86`，外预算90秒。不是业务运行，未进入BL监督，业务filtered120秒/外180秒预算尚不适用。

taskID：**a010dff545e846fab3d02d70593b2b83**，来自返回的实际App绝对路径。任务根 `C:\CodingAgentSandbox\Tasks\a010dff545e846fab3d02d70593b2b83`。原始响应转存 `artifacts/dn-x86-family/probe-response.txt`。工具明确回收完成，已递归亲核回收24文件，无Results/TRX/supervision/业务JSON。

apphost原文关键字段：
- You must install .NET to run this application.
- App: 上述任务根下WpfGfxShape.ComAcceptance.exe。
- Architecture: x86。
- App host version: 10.0.13。
- .NET location: Not found。
- 搜索应用目录、DOTNET_ROOT_X86和DOTNET_ROOT（均not set）、HKLM\SOFTWARE\dotnet\Setup\InstalledVersions\x86\InstallLocation（not set）、默认C:\Program Files (x86)\dotnet。

runtimeconfig实读请求 **Microsoft.NETCore.App 10.0.0 与 Microsoft.WindowsDesktop.App 10.0.0**。错误发生于.NET根目录定位，未给出某单一框架已安装版本列表；不能声称精确缺WindowsDesktop某patch，亦不能证明不存在非标准目录的未注册安装。10.0.13仅apphost版本，**不是已加载CLR/WPF版本**，也不是产品固定runtime合同。

工具未单列进程ExitCode、timeout/kill/drain字段，不填0或猜测错误码。无托管发现摘要，发现数未知。未开始业务，故本次父TRX Counters/RunInfo/Failed、BL Exit/timeout/kill/drain、testSHA/MVID均无运行证据。传输及回收成功不代表共享框架启动或产品验收。

## 5. 对应DM范围、输入及未执行证据
既有真实filter保留：BitmapRenderTargetTests.、GeometryUtilityTests.、IndependentRealizerAbiTests.、IndependentBitmapRealizerAbiTests.、OfficialRasterModuleTests.WhenSameNamedLibrariesAreLoaded、DmBitmapGeometryTests.WhenActualPackage，各加FullyQualifiedName~并以|连接。监督入口仍以AppContext.BaseDirectory产生task内绝对root，BL filtered120秒未改。

DM参照范围为Bitmap183 + Geometry12 + 独立ABI6 + 默认双模块身份1 + 诊断1 = 203。**本轮实际执行0项；x86发现分母未知，不强造203/203或0/203失败。** 未检查x86架构适用分母差异；DM x64通过不是本次结果。

Bitmap finiteIntersection两行复用CreateSourceClipReference：正式同exe官方角色在当次ignored任务目录生成source-clip-intersection.json，再保留候选32字节全等及原后续状态断言。启动失败后未生成参考，未搬x64 10.0.1或旧10.0.13 golden；本次同版同架构Reference来源未建立。其他Bitmap多为固定预期像素/锁/状态合同，不能全称实时独立官方像素差分。

独立ABI6本次没有成功原始内容、错误回调及后续状态证据，不拿DM记录充新结果。BW/Br/CK未选，不冒称覆盖。实际candidate/native模块路径/基址/SHA、runtime/WPF/official路径/SHA均未取得；包候选文件hash不能替代实际加载身份。

## 6. 失败分类与下一步
唯一分类：**环境前置阻塞——x86共享.NET根目录未找到**。无业务执行，不能称零业务失败即通过；没有业务缺输入、产品差异、ABI错误、网络错误或超时证据。未使用一次缺输入纠正额度，未重跑。

当前不能进入x86更大族或当前full。下一步需要环境提供可解析的x86共享.NET及WindowsDesktop 10.0兼容框架，再以新独有任务轻量证明启动，读取实际runtime/WPF/official身份后，按原DM范围生成本次同版同架构输入、运行有界族并完整回收。不能自包含或改x64绕过本目标。

交付：本报告、一处DM条件诊断架构中性修正、ignored独有构建/24文件源/独立回收/错误原文。未清理他人目录。报告和错误原件全文分段回读后提交。探测及停止裁决完成，203族验证因runtime阻塞未完成；不外推完整替换、GPU/loss/media/resource长期合同或其他版本架构。
