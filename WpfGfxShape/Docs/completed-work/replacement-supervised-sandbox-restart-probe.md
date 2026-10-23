# DI 重启后最小沙箱连通性调查

## 范围与裁决

用户报告沙箱和路由器均已重启；本执行者调查，主代理仅监督。已读当前执行规则、最新交接DH/DF/DC段及DH、DE报告（长段截断处补读）。历史full仍未知。不更新共享交接。

**本轮一次x64框架依赖正式exe请求成功取得taskID、执行发现逻辑并回收文件。** 不预认重启修复因果，不证明150MB/full问题全修。没有第二次纠正或重试。

仅专用读写/发布及Windows沙箱真实纯C# exe；无终端、直接间接脚本、full、异步绕过、新Host/第三工程、生产/NativeRun/ExitDiagnostics/SDK/版本/默认断言修改或破坏性清理。

## 输入调查与构建

相对路径以用户指定WpfGfxShape根为准。解决方案为生产、Tests、ComAcceptance三个既有项目。Tests目录未发现现成exe；artifacts已有正式COM clean包是约150MB自包含包，回收目录不选作输入。亲读ComAcceptance csproj/global.json、根Directory.Build.props/targets、既有PackageComAcceptance白名单及NativeRun入口。

正式项目net10.0-windows、UseWPF=true、Nullable=enable；SDK请求10.0.107/latestFeature，未修改。既有入口将 --list-tests 交给MTP，仅发现，不执行测试主体。采用既有框架依赖白名单，零代码修改。

专用发布：x64 Release、--self-contained false、ComAcceptanceSandbox=true、WpfGfxShapeArtifactsRoot指向本轮build/。AotDllPath明确来自 artifacts/de-upload-review/clean-x64-02/com-acceptance-package/Native/wpfgfx_cor3.dll，非官方替充。发布退出0，有既有分析器警告，不宣称无警告；未重发生产DLL，未增加编译符号。

精确sourceDirectory：artifacts/di-sandbox-restart/build/com-acceptance-package/。不是PublishDir/回收根/父目录。发布生成24文件；提交前递归枚举与package-files.txt全24行核对一致：

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

无自包含runtime pack、ReferenceData、旧Results/TRX、coverage、PDB、obj、异架构/跨平台资产。Contracts/候选/hash由既有白名单保留，发现自身不消费；这是最小既有白名单闭包，不是逐文件裁剪证明的绝对最小包。未修改打包规则或随意删依赖。

**实际包总字节未知**：现有专用本地枚举/读取未暴露文件长度，本轮未增加大小审计执行；不猜MB，不借用DE的157451706字节，不当成传输字节。清单可核，大小缺口明确保留。

## 唯一实测及原响应边界

产物根 artifacts/di-sandbox-restart/。请求完整保存request.json；真实exe=WpfGfxShape.ComAcceptance.exe，参数 --list-tests --results-directory Results/di-list-tests，外timeout=90秒，一架构一次。outputRelativePath=null整任务回收。独立本地结果目录 results/list-x64/，没有再上传该回收根。

原响应为纯文本，无结构化taskId/阶段/ExitCode字段；response-evidence.json保存关键原文摘录及未知字段，明确不是完整1792项列表的副本。完整列表见本次调用原响应。

原文首行：MSTest v4.0.1 (UTC 2025/10/14) [win-x64 - .NET 10.0.1]。

原文末节：Test discovery summary: found 1792 test(s) - C:\CodingAgentSandbox\Tasks\e6e832bd74c54bd99a801fe60a208684\WpfGfxShape.ComAcceptance.dll (net10.0|x64)。duration: 180ms。

**taskID=e6e832bd74c54bd99a801fe60a208684**，从实际远端程序集绝对路径提取，非结构化字段。180ms仅发现耗时，非上传或整调用耗时。

工具明确返回“结果已保存到”本轮独立结果目录；亲自递归枚举确认24个输入文件已回收，另有空Results/di-list-tests目录。无父TRX/监督器；发现1792项不等1792通过，不是任何业务验收/full。

已证新任务创建、真实exe启动、CLR/MTP/测试程序集加载、发现逻辑执行并返回及本地文件回收。上传实际字节、起止时间/校验字段未提供，不宣称逐字节传输审计。**ExitCode未显式返回，未知，不填0。** stderr未单列，回收目录没有stdout/stderr持久文件，不伪造路径。

## runtime/WPF与下一最小行动

输入runtimeconfig同时请求Microsoft.NETCore.App 10.0.0与Microsoft.WindowsDesktop.App 10.0.0，本包无本地runtime；正式exe实际报告win-x64/.NET10.0.1并完成发现，证明该x64框架依赖启动所需共享框架解析可用，不是缺runtime或网络失败。

未取得安装runtime完整清单、WindowsDesktop具体patch、实际PresentationCore路径/hash、WPF渲染或官方模块加载身份；CLR10.0.1不能等同WPF10.0.1，不能沿用旧沙箱10.0.13或建立新固定合同。x86 runtime、候选Native加载及业务行为未测试。

下一轮可用同样独立干净输入安排一个明确filtered小场景，回收监督/父TRX；优先补实际runtime/WPF加载身份和逐文件长度/总字节，仅携带该场景必要且来源明确的候选及参考输入。x86另证runtime；不得直接恢复双架构150MB/full，不上传本轮回收根。发现成功仅支持框架依赖最小正式COM包启动可行性，不证明完整COM消费闭包。

旧DB/DG full仍未知，本地证据保留；没有以重启前远端task查询为本轮主任务。报告已补实际结果，提交前全文分段回读。
