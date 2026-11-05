# 真实持续媒体 default-only x86 r1：ExecutedFailed / Closeout Completed

## 裁决与停止点

已亲自实施和执行，未再委派。正式新体 `WpfGfxShape.ComAcceptance.PcoreComparisonTests.WhenContinuousMediaRunsOnceThenOriginalAndCandidateFramesMatch` 精确等值筛选执行1次，TRX total/executed1、failed1、passed0、notExecuted0、timeout0。**official 角色 exit0 后，独立颜色断言首帧失败；candidate0、comparator0，当前15A545候选媒体未覆盖。** 不把角色自身Green当内容通过，不按本次结果改expected，不重跑、不改生产。

首错：`Results/original/media-0-0.pbgra32`，预注册expected ARGB `FFFF0000`（4294901760），实际首像素BGRA `1110EBFF`（ARGB `FFEB1011`，4293595153）。原始AVI为DIB/BI_RGB并不意味着整个系统解码/颜色转换链一定精确保持输入RGB；本次不能区分独立oracle过强、系统颜色转换与其它原因，根因 **Unknown**。此失败不是候选生产错误证据，也不是媒体内容通过。任何后续oracle变更须先有独立合同依据，不能直接把观察值固化为golden。

## 实施范围与预注册

唯一代码变更：正式 `Tests/WpfGfxShape.ComAcceptance/NativeRun.cs`，原119行至148行，新增无参TestMethod/XML说明、私有四帧全像素颜色校验和helper可选 `verifyContinuousMedia=false`。新入口仅 `CompareSceneAsync("media", "default", verifyContinuousMedia: true)`；official成功断言与内容校验均在candidate启动前。旧DataRow仍default/false/true，默认参数维持旧启动/断言/复制/比较顺序；其它场景不启用新校验。生产、项目、SDK、TFM、包、依赖、Host、新素材、Native发布0改。MediaScenes/AVI生成器和WaitMedia均未改。

执行前已写 `artifacts/continuous-media-default-x86-r1/execution-ledger.md` 固定范围、内容/事件/对照/清理分类和预算。颜色引用既有 `Pcore/MediaScenes.cs` 的WriteMediaAsset：asset0前2秒红FFFF0000/后2秒蓝FF0000FF；asset1绿FF00FF00/黄FFFFFF00。新夹具按该合同逐像素精确断言，不从运行读回生成expected。独立断言首帧失败即停止，因此其余三帧未完成该独立校验。

实际RenderMedia确实把采样绑定请求0.5/2.5秒与实际Position窗口[position,position+1)，WaitMedia在原15秒内要求RGB非零、同asset前后字节不同，Pause/替换VideoDrawing后验证保留帧，再自然Ended；非仅两边相等。CopyMedia实际32×16、96dpi、Pbgra32、stride128、2048bytes/帧；四帧8192bytes。result通用input仍写32×32，**不适用于媒体，明确以CopyMedia为准**。本次未启动独立comparator，无8192bytes差分通过；既有CompareMedia才是四帧每帧长度2048且全字节SequenceEqual、threshold0。

## 真实调用及模块角色

公开MediaPlayer→VideoDrawing→RenderTargetBitmap；既有Program在WPF对象前为PresentationCore/WindowsBase安装wpfgfx resolver，selected决定official或candidate；VerifyLoaded枚举当前进程实际Process.Modules并取身份，允许media候选另载PresentationCore同目录官方系统媒体服务。

已亲读SystemMediaModule.Acquire：通过实际已加载PresentationCore目录定位官方wpfgfx，文件身份避免自身递归，缓存进程驻留；这是候选复用系统decoder/provider服务，不代表候选整个RTB由官方代做。本次candidate根本未启动，因此候选实际加载、消费和清理均未证；不能用frozenCandidate/requested路径冒actual。

## 构建、发布及清包

- normal COM build1：正式csproj，Release/win-x86/net10.0-windows，exit0、135 warnings、0 errors，3.45秒；未抑warning、未修后重建。
- COM publish1：同配置/RID/TFM，self-contained true，RuntimeFrameworkVersion10.0.13，ComAcceptanceSandbox=true，AotRuntimeIdentifier=win-x86，显式AotDllPath=`d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/postfailure-source-rebuild-x86-r1/native/wpfgfx_cor3.dll`。PublishDir/ComAcceptancePackageDirectory分别本门publish/clean。exit0；未用--no-build，**正常publish隐含编译1**，不藏作零编译。publish警告总数Unknown（可见warning，无汇总）。完整精确命令在publish-tool-return.txt。
- 既有PackageSelfContainedComAcceptance先验证Native和whitelist依赖，仅RemoveDir本门clean，再Copy正式二进制。无手工二进制/文本伪造，无旧产物清理。clean公开API元数据核新无参入口与旧矩阵均在；runtimeconfig全文核NETCore/Desktop均10.0.13；正式MTP/MSTest/TRX依赖及exe/dll/deps已核。运行亦核媒体入口确实执行。
- clean aot SHA：`15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`。此为打包/父初始化身份，不是candidate已加载实证。
- 工具返回即时持久化build-tool-return/publish-tool-return/sandbox-tool-return；可用构建日志64行、发布60行均已回读。源工具自身分别丢弃前86496/106496字符、33539/53539字符，只保留末20000字符；该不可恢复截断如实写入readback账，**不宣称完整原始编译日志已保全**。无重建补日志。

## 唯一执行及父测试身份

唯一沙箱task `9add3442fe144cb6aa38c0c68398b02d`，source为本门clean，仅启动正式exe：`--supervise-exit Results/continuous-media-default-x86-r1 120 FullyQualifiedName=WpfGfxShape.ComAcceptance.PcoreComparisonTests.WhenContinuousMediaRunsOnceThenOriginalAndCandidateFramesMatch`，外层timeout180秒；整任务拉回本门readback。未另启动角色/发现/full/x64/探针。框架内部日志45行有自动Discoverer1823条，属于同一正式测试调用内部发现，**独立发现调用0**，实际体仅1。

监督原件40行全文回读：09:20:57.2450892–09:21:08.5778847 UTC，supervisor8692/child7804；X86/.NET10.0.13；Reason=exited、ExitCode2、HasExited=true、TimedOut=false、Cancelled=false、KillAttempted=false、OutputCollected=true、drain timeout均false、SupervisorError=null。无超时强杀。

实际测试程序集位于任务根 `WpfGfxShape.ComAcceptance.dll`：SHA `D3C92F2E288BFB6E5123D1E60BD8E8C27A668EBECFFF51555FFAF3F389AEFB71`，MVID `f2e0adb8-bfe3-471b-907d-081711edb077`。runtime从监督、diag和角色均核10.0.13。TRX run `f00fe184-475d-44a8-aa8d-4ced66a20937`，42行已分段读完，唯一体10.3520383秒，正式Failed，不是零体/skip。初次递归目录未列出TestResults，随后直接列目录及读取确认child.trx实际存在；不是丢失，不以初次列表下结论。

stdout19行全文、stderr空已读；中文stdout存在编码替换字符，TRX中文错误正常，不改写原件。角色未单独重定向stdout/stderr，既有RoleAsync继承父输出，本次没有独立角色stdoutstderr文件，不能声称已读不存在的原件。diag79行含长行截断补读至尾。

## official 实证（与候选未覆盖严格分账）

业务根：`readback/Results/pcore/media/default/b9641ead43744cceaa8d1d08675c2384/Results/original`。result.json共507行，截断后分段补齐全文；stage completed、status Green、exitCode0、comparisonStatus OriginalOnly。随后新夹具失败使整个门Failed。

实际加载模块来自 `actualNativeModules`（VerifyLoaded真实枚举），仅任务根 `wpfgfx_cor3.dll`，不是Native子目录：
- SHA `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`，1807144bytes。
- fileVersion `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`；productVersion `10.0.13-servicing.26504.104 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。
- 实际PresentationCore任务根，SHA `D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D`，WindowsBase SHA `C047D97A1BD3CD29E50806340C246B01482CC71CA6E41216AD5C9A0C1568F1ED`，均fileVersion10.0.1326.50504/product10.0.13-servicing.26504.104+4099…；同目录official符合本角色要求。
- frozenCandidate记录Native子目录15A545…/4003328bytes/file1.0.0.0/product1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a，仅文件身份，未加载。候选actual与systemMediaServiceModule本门均未达。

四帧均HasVideo=true/HasAudio=true、NaturalVideoWidth2/Height1；原二进制保留未当文本读取：

|文件|请求/实际秒|sample BGRA（首2像素）|SHA256|
|---|---|---|---|
|media-0-0.pbgra32|0.5 / 0.658666|1110EBFF1110EBFF|56E48108ACA0E07CF0D9F531679C1036E1980631DDF3A76C0409D7AAF9F6A8A4|
|media-0-2.pbgra32|2.5 / 2.535233|EB1011FFEB1011FF|8F5F79953DF7C91E4BF56AC75222912DFE5E3C57B5B5E3164A63FA83F7FF07A5|
|media-1-0.pbgra32|0.5 / 0.5307796|11EA11FF11EA11FF|D800E6BCB44942F4D4DEF110BE0625AA4C45DEE86DCB597940CDE163FD70B6A0|
|media-1-2.pbgra32|2.5 / 2.5294644|10EBEAFF10EBEAFF|BD6FD4A7D9D5B84E82D5681BC174193A170A208B368BF794B1E86CFF6B4056A7|

2048bytes由CopyMedia固定生成且首帧新断言已通过长度后才颜色失败，其余帧长度未独立重测，不虚称本次工具逐文件测量。四帧hash与sample不同，支持非空/不同时间平台有限观察；不能覆盖未完成的独立颜色全帧断言。media-0.avi/media-1.avi原件存在，输入hash未由既有证据记录，**Unknown**；不以像素工具读二进制、不增执行取hash。

事件：opened2（0.0066841/0.0031248秒）、ended2（均4秒），expected-failed-close1 HRESULT -2147024894；parallel-close-first/second各1，非预期failed无记录。mediaClosed记opened2/ended2但HasVideo/HasAudio仍true，Close被调用不等于同步这些属性已清零；不凭此宣称资源完全回收。源码finally Close、清空drawing/visual，角色记录Dispatcher shutdown/finalizers drained，角色正常exit0；父也正常退出无强杀。没有逐COM ref0、全部系统worker退出、callback静默区间或长期趋势实证；音轨存在/HasAudio/Volume0不证实际音频内容或设备输出。

## 原件、预算、恢复及唯一建议

全部本门证据在 `WpfGfxShape/artifacts/continuous-media-default-x86-r1/`：预注册execution-ledger.md；三工具原返回账；build/publish-readback；stop-record.md；publish/clean；readback下supervision.json/stdout.txt/stderr.txt/Diagnostic/child.trx/aot-identity.json；上述official result、2 AVI、4 pbgra32。candidate result/帧、comparison.json **未生成（首错前置阻断）**，不伪造、不拷文本作二进制。

最终预算：normal build1/1、COMpublish1/1（正常隐含编译1另列）、沙箱1/1、正式体1/1 Failed、official1/1 exit0但独立内容断言失败、candidate0/1、comparator0/1；Native发布0，生产改0，新工程/Host/样本/SDK/依赖0，独立发现0、额外test0、重试0、x640、显示native采样0、委派0、终端脚本0。失败后源码无尾改，仅归档文档。

Closeout Completed不等于业务通过；本门Failed/候选NotCovered。旧9FDF/当前其它有限绿、4199 Debug与Release编译历史和全部历史预算各自保留，不移绑。显示Unknown，媒体完整闭包/音频输出/全部worker/长期资源趋势/HW/loss/full与完整替换Open。

**下一唯一建议：另授权仅只读审查系统媒体颜色转换与独立内容oracle合同，先判定输入RGB与解码输出RGB的合法关系；不以本次sample改expected，不运行/重建/发布，不自动x64。** 无充分独立依据前不恢复本门执行。
