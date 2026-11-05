# CZ 当前源码 ComAcceptance 双架构完整基线

## 结论与边界
**两架构默认包已建立、各4项身份/官方输入smoke通过；无filter full在执行前被现有BL监督入口能力阻塞，未启动，不能称full完成或中断。**
先创建本报告，后探索/发布/运行，最后补充回读。不改生产、测试行为、入口、项目、共享文档、tracked ReferenceData/golden/阈值/skip/fallback/默认并行。保留CF/CX/CY真实整数fill；生产未重发。仅本报告及ignored独有artifacts/cz-current-comacceptance/。无终端、直接或间接脚本、Host/第三工程。

## 规则及源码核查
已读最新交接CY优先节、执行规则27行、完整方案（长输出截断后补88–120）、CY/CX全文、BD关键身份及失败分类、BN/CL所有权诊断关键段。历史不当本轮实证。
亲核当前NativeShapeReader Capture float设备乘加/半像素/28.4 TryRound/实际SoftwareFillBezier.Flatten（截断补64–88），SoftwareStrokeContours fillLocalFloat默认false及局部float路径，Group ordinary path传fillPrimitive、curved/group递归保持。没有撤回已验收修复。
复用CX native-x64/native-x86候选。对应性依据CX/CY已验收变更清单、当前三段源码和本轮相同二进制SHA；未取得全生产树hash/PDB源码校验，不拿ProductVersion提交串当dirty源码证明。

## 确定执行门阻塞与最小需求
当前ExitDiagnostics.cs:25–36：--supervise-exit必须恰有3参数；seconds只允许1–120；无条件构造--filter及第三参数。180传内监督会抛异常。沙箱父timeout允许至1800，但不能扩大子监督120秒。另有cleanup10秒/output drain5秒，不是业务预算。
因此不能用现入口满足“合理有界BL监督下真正无filter full”。匹配所有的filter仍不是无filter；外层1800不能阻止内层120误杀。本轮没有如此绕行或盲目full重跑。
按用户“入口上限不足先报告或明确最小参数验证改动需求”分支：建议仅精确扩展ExitDiagnostics诊断参数，支持无filter模式并真正不向MTP添加--filter，允许例如1500秒业务监督、外沙箱1800秒留清理余量；保留PID/起止/ExitCode/timeout/kill及独立stdout/stderr。先验证旧filtered smoke及新参数分支，再每架构各一次full。不改默认NativeRun/MTP行为、断言/并行。此属当前禁止入口改动边界，未擅自实施，需主线协调。

## 正常默认测试包发布
CX原包含显式探索测试和Old，不能充作默认full。故仅通过专用发布两次正式ComAcceptance Release/self-contained、RuntimeFrameworkVersion=10.0.13、ComAcceptanceSandbox=true、对应RID/AotRuntimeIdentifier，AotDllPath显式CX native-{arch}/wpfgfx_cor3.dll。正常还原/构建依赖图，不用NoBuild/NoDependencies，无DefineConstants或额外props注入。生产无ProjectReference，无新生产编译。
独有WpfGfxShapeArtifactsRoot分别本根x64/、x86/，正常PackageSelfContainedComAcceptance生成各com-acceptance-package，不覆盖公共包。两次均成功（11行日志）；WarningLevel=0仅限制日志，不宣称零警告。
TFM net10.0-windows/WPF/Nullable，MSTest4.0.1，global SDK10.0.107/latestFeature。CQ在CQ_FILL_EXACT_DIAGNOSTICS、CR含Old夹具在CS_FILL_ACCEPTANCE条件内；本次不启用探索符号、不依赖Old。TestSettings既有DoNotParallelize不改。

## 本轮任务与绝对路径
本地完整结果根：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\cz-current-comacceptance\`。

|架构|taskID|完整回收目录|BL直接监督MTP父测试进程TRX|
|---|---|---|---|
|x64|17d895cf30a443a6ab9c00e208d49919|smoke-x64/|smoke-x64/Results/cz-smoke-x64/TestResults/child.trx|
|x86|79cdad89ccc145378b7fbb566fd25121|smoke-x86/|smoke-x86/Results/cz-smoke-x86/TestResults/child.trx|

远程绝对父TRX分别：
- C:\CodingAgentSandbox\Tasks\17d895cf30a443a6ab9c00e208d49919\Results\cz-smoke-x64\TestResults\child.trx
- C:\CodingAgentSandbox\Tasks\79cdad89ccc145378b7fbb566fd25121\Results\cz-smoke-x86\TestResults\child.trx

未限制outputRelativePath，完整回收exe/DLL/runtime/ReferenceData/Results/Diagnostic/TestResults。每任务Results/cz-smoke-{arch}/有supervision.json及独立stdout.txt、stderr.txt。内监督120，外沙箱180（没有给内监督传180）。

|架构|监督PID/子PID|UTC起止2026-10-21|状态|
|---|---|---|---|
|x64|3480/3380|06:44:48.1447346→06:44:49.2489909|exited/Exit0|
|x86|8/5072|06:44:52.9493715→06:44:54.2762489|exited/Exit0|

两份supervision全文38行已读：HasExited/OutputCollected=true；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false；SupervisorError=null。TRX业务结果及54–63末节直接回读：Completed，total=executed=passed=4，failed/error/timeout/aborted/notExecuted及其余计数均0，无RunInfo错误或Failed结果。不仅看stdout进度。
过滤只有PublishedArtifactTests、GlyphReferenceTests、WhenTileFixturesAreRenderedThenOriginalWpfEvidenceIsRecorded、ReferenceRevalidationEvidenceTests。4项是身份/官方输入smoke，不是候选COM功能full。

## 亲核身份与官方来源
|身份|x64|x86|
|---|---|---|
|候选SHA256|F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7|7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA|
|默认测试SHA256|C739C0B2CEEFEBC5A85438BBB58DDE6C4796643FC1A6B2FFE5AD628F4EFC4A6E|150AE4E84ED97363AC311D6E536857E1210F95B89AB5F7DC5BD9AB3D2CCFF1D2|
|测试MVID|3342dfa2-bd8b-4446-a4e9-ee7f3b8a7440|22f91f6c-4621-42c3-98ab-385b3582bdf1|
|官方SHA256|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|
|PresentationCore SHA256|0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31|D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D|

.NET10.0.13，X64/8、X86/4。官方10,0,1326,50504/ProductVersion10.0.13-servicing.26504.104/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；PresentationCore实际从任务根加载，10.0.1326.50504。测试和候选版本1.0.0.0/ProductVersion1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299，不作全源码凭证。NativeAOT候选MVID不适用。
亲读两份Results/aot-identity.json全文、Results/az-reference-consumption.json身份/Glyph来源1–100。候选SHA来自任务Native文件；官方根wpfgfx_cor3.dll，WPF根PresentationCore.dll；测试Location/MVID来自实际typeof程序集。此smoke没有候选actual-loaded模块枚举，不能称候选COM调用成功。后续full仍须核actual模块。
独立真实官方同版同架构生成ignored任务ReferenceData，GlyphRaw OriginalLibrary/SHA和drawing NativeSha由既有断言核同包官方SHA。未改tracked输入。
- x64 glyph SHA86E14A6DD04778931999124242335682B126477DB8812CA1203CF148A6F41B78；drawing87853D1510543BB395EE0881A6B9AA5CECB54BCBB3E3C00F2A388EBD086D922B。
- x86 glyph SHAC64A4BE81300E104864488A914386C452B6700FE3E74F6807A420A1A4053F3A4；drawing34BB08D6DA1250E1BE9D5113915BED1FEA10100AF0CE4B9C842378E1D83CFFD5。
仅核这两类输入，不声称全full输入齐备，未逐字审完大像素数组。

## 失败分类、新fill与剩余切片
本轮smoke没有Failed。当前确定阻塞是监督入口能力不足，不是产品COM失败、缺输入或本轮sampler崩溃。
新曲线/fill在本候选full中：未执行，没有新结论；CY有限206场景修复继续保留，但不移绑为默认full通过。
BD旧x64 1740/1699/41与x86无父TRX不能绑定本SHA。HW37不能全归环境，旧官方PublicHwnd成功与候选失败仍有产品/夹具嫌疑，需要当前full重新裁决。rounded/curve、大shader、WindowQ固定resize长度等仅为历史线索，不擅改或skip。
BN同步Event0/E_HANDLE/Destroy前0xC0000374仍是历史。CL源级同线程失败batch已free但closed槽保留、Destroy重入可UAF，不能直接绑定真实官方DLL或当BN首个写坏根因。本轮没有sampler执行，既不填历史崩溃码，也不宣称sampler未崩通过。
当前内容/时序/资源/恢复业务未由full覆盖，没有新循环、真实loss/resize新帧/media证据，事件完成不等于真实消费。GPU/loss/media/resource/线程callback/通知/同步Event0/E_HANDLE/首次堆损坏/圆角大shader等合同全保留。

## 交付状态
|架构|无filter full状态|full taskID/TRX/退出/计数|
|---|---|---|
|x64 Release|执行前阻塞，未启动|未产生，不能用smoke4/4代替|
|x86 Release|执行前阻塞，未启动|未产生，不能填0失败或旧BD数字|

下一最小切片是协调上述ExitDiagnostics无filter/合理上限参数改动，不是生产算法回退。之后利用本轮完整回收smoke目录内真实官方输入，各一次无filter full，完整回收并读所有Failed/RunInfo/actual模块/业务资源恢复。
本轮实际取得的两份监督、父TRX、测试身份和官方输入来源均已回读；不是以报告未补或结果未读结束。尚缺的是受已证实入口能力限制未产生的full证据。最终Tests/COM×双架构四格及完整替换仍未交付。
