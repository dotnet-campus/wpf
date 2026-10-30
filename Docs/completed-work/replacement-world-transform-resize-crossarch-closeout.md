# world-transform resize crossarch 本轮执行收尾

状态：ExecutedFailed / FirstFailureStopped / CloseoutCompleted；五体目标未完成，完整替换Open。生产及fixture源码修改0。

## 固定目标及顺序
1. x64 原 official shaderresize；仅真实全通过才执行 x64 candidate shaderresize。
2. 当前 fixture 正式 x86 COM publish（self-contained，NETCore/Desktop 10.0.13，显式147B AotDllPath，ComAcceptanceSandbox=true，独立 sampler-world-transform-x86-r1publishclean）。
3. x86 原 official sampler；通过后 x86 official shaderresize；后者通过才 x86 candidate shaderresize。
五个无参精确 FQN 须先源码核验；每个独有 Results，--supervise-exit，内120秒，外180秒（既有嵌套隔离必要时外300秒并静态说明）。第六沙箱只供确有必要的精确 UID discovery，不重跑失败。

## 预算实际登记
normal COM build 0/1；COM publish 0/2（publish隐式build单列）；sandbox 0/6；Native/offline/full/源码修改/脚本/再委派均0。
当前先读取精确嵌套 WpfGfxShape/artifacts/sampler-world-transform-x64-r1 及恢复规则，不以根枚举结果推断缺件。

## 基线
x64既有sampler official/candidate第三DataRow已Passed，引用 WpfGfxShape/Docs/completed-work/replacement-sampler-world-transform-x64-closeout.md；本轮不重跑这两项。x64 Native632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671；x86 Native147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304。file预检不代表actual加载。
旧full：x64 1823/1803/20fail，x86 1823/1820/3fail，23红不减。未触发真实loss仍Open；targetGetDesc/GetDevice/backend证据不足仍Unknown。完整替换Open。

## 每任务必核
调用前参数、原工具返回、task路径即时记录；原native00563412→初态两FF80→64/48/32×24逐次原颜色→secondary mutation后00FF0080，保持原expected/阈值/顺序/等待/参数。核外TRX/监督、子隔离、diag/stages、actual模块/testSHA/MVID/runtime/pointer、像素、transform/contentroot Deleted1及cleanup，完成才下一任务。

## sandbox1 x64 official resize 调用前
sourceDirectory=WpfGfxShape/artifacts/sampler-world-transform-x64-r1/clean；executableRelativePath=WpfGfxShape.ComAcceptance.exe；arguments=[--supervise-exit,Results/x64-official-resize,120,FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive]；localOutputDirectory=WpfGfxShape/artifacts/world-transform-resize-crossarch-r1/x64-official-readback；timeoutSeconds=180；outputRelativePath省略完整回收。HardwareClosure.cs20-21为无参原TestMethod，无HistoricalProcessIsolation；内120外180。sandbox调用预占1/6。

sandbox1原工具返回："沙箱执行完成。结果已保存到：d:\\lindexi\\Code\\dotnetcampus\\WpfLab\\WpfGfxShape\\WpfGfxShape\\artifacts\\world-transform-resize-crossarch-r1\\x64-official-readback"。工具无exit/task编号，待监督原件判定。实际sandbox1/6，其余仍0。

## 实际结果与首错停止
sandbox task=6d399cb49230401787bc70e7e39fac8f，远端 C:/CodingAgentSandbox/Tasks/6d399cb49230401787bc70e7e39fac8f；回收位于上述x64-official-readback。
- supervision.json完整40行已读：supervisor12484，MTP PID12544，内120秒；Reason=exited、OS exit2、TimedOut/Cancelled/KillAttempted=false，OutputCollected=true，drain无超时，SupervisorError=null。工具自身不提供supervisor OS exit，不伪造。
- TestResults/child.trx完整79行分段读取：total/executed/failed=1/1/1，passed0，timeout/aborted/notExecuted0。唯一精确FQN正确；Diagnostic/log_261029131139937.diag末152行MTP session exit2；stderr空。
- actual官方模块AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，版本10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；实际testSHA08AE0DB6F7045FD6CFCF1CFF04D2EAAB415B35D74731CB897D55FEAD482FE56F，MVIDdfd0392d-f53e-4ae9-b3a0-555c3d08f1f7，runtime.NET10.0.13，Architecture X64（pointer8由架构推导，未独立记录IntPtr.Size）。Native632F只是测试前置file校验，不是本task候选actual加载。
- 原生前置00563412及两初态0000FF80全部expected=actual；没有64x24成功日志，没有48/32x24及secondary mutation00FF0080结果。错误断言actual present(connection)=-2147418113=0x8000FFFF(E_UNEXPECTED)，不是像素差，也不是C0000005/crash。
- 最窄静态定位：VisualCache.cs358-372第一个width64迭代；SetWindowPos/GetClientRect/width64/height24及send/close/commit断言先于present且未失败，present失败发生在DwmFlush/HardwareResize输出/像素断言之前。runtime缺少逐次resize stage，不冒独立记录尺寸值。最初两帧present均已返回S_OK。
- stages.jsonl全63行已分段读：root13/transform14/child3/target12；M11=M22=1、size60、animation0，create/update/bind/SetRoot存在；worldTransform14/contentRoot13各release S_OK且deleted1；close/commit/destroy/disconnect/deinitialize均S_OK，cleanup complete错误串空。无HistoricalProcessIsolation，故子隔离TRX/parent-role-exit不适用，不称缺件。
- targetGetDesc/GetDevice/target backend证据仍Unknown；nativeControl=d3d9.dll和CreatePixelShader S_OK不代表官方target HWbackend认证。

## 最终五体矩阵
|真实体|终态|
|---|---|
|x64 official shaderresize|ExecutedFailed：首次resize present 8000FFFF|
|x64 candidate shaderresize|NotRun：official前置失败|
|x86 official sampler|NotRun：本轮首错停，x86未发布|
|x86 official shaderresize|NotRun：首错停及sampler前置未过|
|x86 candidate shaderresize|NotRun：首错停及official resize前置未过|

最终预算：normalCOMbuild0/1、COMpublish0/2、publish隐式build0、sandbox1/6，Native/offline/full/源码改/终端脚本/再委派0。未使用余量是用户明确首错即停，不是缺件或工具失效；不花剩余预算重跑/跨架构绕过。一次文档替换因新建后未Read被拒，读取后成功，无业务改动。x86147B和official5EBD本轮未actual认证。

## 下一直接主线修正与验收
下一唯一动作：定位官方真实HWND resize在首次64x24后返回E_UNEXPECTED的协议/生命周期差异，对照真实WPF resize的UpdateWindowSettings与target/设备尺寸更新顺序；重点从OutputWindow.cs73-85的0x33/72字节包和VisualCache.cs358-372调用时序入手，当前只有定位不是根因确证。取得确定原因再最小修正fixture（另行授权），不改原expected/阈值/顺序/等待，不默认改生产，不添加fallback。修正后正式COM包先原official resize，再candidate；继续本轮未完成的x86sampler→officialresize→candidate。两架构定向完成后，下一主要验收是修正后defaultfull重新获取真实分母，再处理真实loss触发/恢复；本轮无full授权，不执行。原full23红全部保留，音频/资源趋势等不迁移、不凭定向相减。完整替换未完成。

