# post-fixture 默认 Release full r1 — Complete report / ExecutedFailed

## 1. 最终结论与预算

两架构正式默认全适用 full 均已实际执行并完成回读，不是阶段待执行报告；完整替换仍 Open。未重试、未过滤显示/旧 knownfail、未改 skip/expected/阈值、未使用 full 调试配置或扩诊断。

|架构|total / executed|Passed|Failed|skip / notExecuted|timeout|子 OS / MTP exit|task|
|---|---|---|---|---|---|---|---|
|x64 Release|1823 / 1823|1821|2|0 / 0|0|2 / 2|2c35943e0906415f94807a75ae78937a|
|x86 Release|1823 / 1823|1821|2|0 / 0|0|2 / 2|bf38e6d05f1c4836962380809ec543db|

其余 Counters（error、aborted、inconclusive、notRunnable、disconnected 等）均0。skip由 notExecuted=0且全部1823已执行交叉确认，不另造分母。两次监督均 Mode=full、1500秒，沙箱外1800秒；exited，TimedOut/Cancelled/KillAttempted/KillReturned均false，HasExited/OutputCollected=true，两个drain timeout=false，SupervisorError=null。沙箱工具仅返回保存路径，没有提供 supervisor 自身 OS exit，不冒称该层 exit0。业务非零后仍完成x64分类再执行x86。

最终预算：COMpublish **0/2**，默认full Windows沙箱 **2/2**（各arch一次），Native/normalbuild/源码修改/定向重跑/额外实验/脚本/终端/委派均 **0**。复用正式 clean，不重发生产扰动SHA。所有本轮正式运行已closeout，无待核TRX或未写执行尾；整体未通过不是证据尾段未完成。

## 2. 调用与包认证

完整逐次调用前参数、原return、task与exit即时账：[execution receipt](../../artifacts/final-acceptance-post-fixture-execution-receipt.md)。

- sourceDirectory=`WpfGfxShape/artifacts/resize-export-contract-{arch}-r1/clean`；exe=`WpfGfxShape.ComAcceptance.exe`。
- arguments=`[--supervise-exit,--full,Results/final-acceptance-post-fixture-{arch}-r1,1500]`；timeoutSeconds=1800；全目录回收，未指定outputRelativePath。
- localOutputDirectory=`WpfGfxShape/artifacts/final-acceptance-post-fixture-{arch}-r1-readback`。
- 既有发布原参数为Release / net10.0-windows / self-contained / ComAcceptanceSandbox=true，显式绑定632F/147B。不是SamplerNativeCrashR2或其他诊断配置。两包runtimeconfig NETCore/Desktop均10.0.13；公开API各66类，D3DImageTests存在；package-files清单及Contracts/wpfgfx.def、wgx_sdk_version.h存在，aot.sha256匹配。
- 已读最新VisualCache world/DPI root创建→60byte矩阵更新→绑定→SetRoot，以及official resize `flush(channel)` / candidate `present(connection)`分流；既有resize执行的testSHA/MVID与本轮supervision完全相同，故publish0，不靠默认Aot路径漂移。未把二进制当文本读取或泛搜DLL。

### 本轮实际身份（不是只file预期）

|身份|x64|x86|
|---|---|---|
|production actual loaded SHA256|632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671|147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304|
|official actual loaded SHA256|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|
|testSHA256|C3D4F524400F5209C8DE44BD4621F2CA2431D1D7CF9BC0962FBE490FC57CD30F|B04E13D0FD88DE8B9AD6C8708EA95FDE81767EE15753416F14B4EA4053524653|
|testMVID|1989da39-bdf7-4b19-86b2-8decb666b6b9|26bdd27a-5782-4247-bfca-0735e0a4d994|
|runtime / arch / pointer contract|.NET10.0.13 / X64 / 8|.NET10.0.13 / X86 / 4|

实际模块由GetModuleFileNameW取路径、SHA取该路径文件：candidate位于task/Native/wpfgfx_cor3.dll，official位于task/wpfgfx_cor3.dll。production ProductVersion=1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a；official=10.0.13-servicing.26504.104/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。上述证明被读角色实际加载身份，不把它扩成所有passed角色均逐一加载认证；本轮pointer由对应进程架构及既有同SHA运行身份支持，不虚构supervision中不存在的PointerSize字段。aot.sha256本身只file身份；这次另外已读actual模块JSON及candidate resize TRX交叉认证。

## 3. 全部失败 ResultDefinition 精确映射

以下四行就是全部红实例，不是抽样。每项均无参数。根R=`WpfGfxShape/artifacts/final-acceptance-post-fixture-{arch}-r1-readback/Results/final-acceptance-post-fixture-{arch}-r1/`。

|arch|精确FQN|testId|executionId|TRX Result / Definition行|actual role与分类|
|---|---|---|---|---|---|
|x64|WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive|14e862b3-9b71-843a-9ff5-67f779272c33|bf03bab6-2030-42e3-812f-6212778898bd|2309–2339 / 10249–10252|actual official AD1C；初始shader像素失败，非candidate回归、非resize export错误|
|x64|WpfGfxShape.ComAcceptance.D3DImageTests.WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover|16ff21fd-ec04-8a70-814e-ca43404cfbd3|792d4a30-8b26-4364-bc80-aa0a2360e474|2340–2353 / 10253–10256|独立d3d9系统控制；真实loss触发不足，恢复未到|
|x86|WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive|14e862b3-9b71-843a-9ff5-67f779272c33|67501173-b994-4900-b3b3-29bd27182764|2309–2340 / 10251–10254|actual official5EBD；初始shader像素失败，非candidate回归、非resize export错误|
|x86|WpfGfxShape.ComAcceptance.D3DImageTests.WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover|16ff21fd-ec04-8a70-814e-ca43404cfbd3|fb95efc4-94f5-4f7e-9b12-67cb58ff6abf|2341–2356 / 10255–10258|独立d3d9系统控制；真实loss触发不足，恢复未到|

### official初帧两红

原消息均为 `Pixel 0x00000000, expected 0x0000FF80`，原逐通道<=1断言未改。native前置expected=actual=00563412，visible1/iconic0/cover自身、Default桌面/session1/state0；foreground=0x600FE不是target，但这只是现场，不足以认定根因。CreatePixelShader hr0，world root13/transform14/child3、M11=M22=1、size60、animation0，SetRoot/commit hr0，frame0 present前后均到，然后像素0；初始第二帧、三resize和mutation未到。两资源各deleted1，close/commit/destroy/disconnect/deinitialize均hr0；bitmap/surface/device/windows cleanup complete空错。

这不是旧x86 connect前F3F3F3失败：本轮前置确实过且已connect；也不是8000FFFF复发：根本尚未进入resize export。与fixture修正直接相关的root字段实际已生效，不能再归因“未应用root修正”；新full仍初帧失败，原因尚未确定，不无证归环境或生产。既有x64独立official resize全过、两arch隔离official sampler全过，而本轮official resize在共享full进程失败，提示下一应静态审查共享进程模块/partition初始化及清理合同与独立角色差异；这是调查方向而非已证根因。本轮不加诊断、不重试。

### real-loss两红

原stdout两arch相同：`firstCreate=0x00000000 mode=1920x1080 format=22`；`secondCreate=0x88760868 firstCooperative=0x00000000`。
原消息：`Exclusive displacement must produce real D3DERR_DEVICELOST; no synthetic replacement is accepted.` expected=-2005530520 / actual=0。失败的是firstCooperative断言，不是把secondCreate失败当成功。beforeReset/reset未到，无真实loss或恢复证据，不skip/不模拟/不称候选恢复失败。

## 4. fixture两缺项与历史边界

- world/DPI root缺项：已消除缺初始化的源码与执行事实；本轮两arch official sampler隔离真实像素五帧通过，无旧C0000005；official初帧失败也已记录实体root矩阵，不再是空root合同。不能由此宣称所有显示缺陷消除。
- official crossThread resize误用SameThreadPresent：源码已改SyncFlush，既有x64独立official resize三次hr0认证保留；本轮两archofficial resize初帧就失败，**本轮没有到达该分支，不能外推x86官方resize已认证**。没有8000FFFF复发证据。
- 本轮candidate resize两arch均Passed：TRX2268–2308，testId1eded079-5019-809c-bf7c-07ef4caf0e29；00563412、两FF80初帧、64/48/32×24三FF80帧、两FF0080变更帧全部expected=actual。这是本次默认full的真实结果，不是消耗额外定向重跑；x86此前candidate NotRun的历史仍保留，不把历史改成已跑。
- 隔离official sampler两arch子TRX57行已完整分段读，1/1Passed、失败0；两supervision40行全读，child11580/7840 exit0、无timeout/kill/drain错误，parent-role-exit各exit0/errors[]。隔离标识x64=48a21faea0aa49e999cc89eb52ac4c78，x86=3ffcefb9e40445de83eaf15ba8478022。
- 旧full x64 1823/1803/20fail、x86 1823/1820/3fail，23红作为独立历史完整保留；本轮新1823/1821/2分别来自真实Counters，不由定向通过相减生成，也不把所有历史红改善都归功于fixture。
- 旧x86官方resize connect前00563412≠00F3F3F3是独立显示前置失败；本轮该前置通过不证明偶发问题根治。real-loss触发不足属于另一合同，不与显示采样偶发混为一项。

## 5. real-loss 静态根因范围与明确下一动作（本轮无执行/源码修改）

已对照旧real-loss失败原件：旧x64 TRX2401起、x862390起，与本轮secondCreate=DEVICELOST / firstCooperative=0一致；不是新增生产回归。

源码 `D3DImageTests.HardwareClosure.cs47–104`：

1. 使用Direct3DCreate9(32)，不是Direct3DCreate9Ex；vtable16 CreateDevice，adapter0，DeviceType1=HAL，flags0x26=FPU_PRESERVE|MULTITHREADED|SOFTWARE_VERTEXPROCESSING。SOFTWARE_VERTEXPROCESSING只是顶点处理，不等于REF/SW rasterizer，不含ADAPTERGROUP_DEVICE；这些flags不构成“必须lost”的触发。
2. Windowed=0，mode取adapter0 GetAdapterDisplayMode，SwapEffect1；两个hFocusWindow各用自身窗口，且均为同进程同窗口线程。官方合同说同一应用各device共享focus window；当前换另一个HWND不等于“另一个应用夺取独占”。第二CreateDevice本身允许返回DEVICELOST，不能据此推导首设备已被成功置换。无第二设备成功证据。
3. 窗口是专用STA、WS_POPUP|WS_VISIBLE、topmost、32×16；loss测试在创建第一个设备前没有EstablishObservableWindow，也没有GetForegroundWindow==首focus窗口的确认，先创建otherWindow也可能改变激活。实际loss测试没有adapter identifier/GetCreationParameters/GetSwapChain PresentParameters或focus/session轨迹，不借其他native测试的Basic Render Driver描述给它认证adapter/backend；actual deviceHAL由创建请求及S_OK支持，但还缺独立GetCreationParameters核验。没有证据说沙箱不能管理会话。
4. **已明确违反官方线程合同**：CreateDevice和Reset通过windowThread.Invoke在创建线程执行；两处TestCooperativeLevel却直接在测试线程执行，最后finally的Release在window线程。MULTITHREADED不撤销TestCooperativeLevel必须在创建线程调用的限制；返回0不能当作合规首设备健康/lost判定。恢复轮询里的ProcessWindowMessages也在测试线程，不能泵另一个线程的窗口消息。此缺陷必须先修，但不能保证仅改线程就真实触发lost。

### 下一最小可实施修正与受控真实触发

只需现有C#系统API与正式fixture，不引入模拟/原生辅助/第三Host：

- 把所有TestCooperativeLevel、Present、Reset及与其相关的消息处理放进同一个windowThread.Invoke创建线程；GetCreationParameters记录AdapterOrdinal/DeviceType/hFocusWindow/BehaviorFlags，GetDirect3D/GetAdapterIdentifier记录实际adapter，GetSwapChain(0).GetPresentParameters记录实际Windowed及device window；保留线程ID、foreground、session、desktop实测。
- 先只建立一个传统fullscreen D3D9设备，确保首focus窗口已实际成为前台并完成一个真实Present，记录正常内容；不要用“secondCreate失败”作为首设备lost事件。
- 建立受控的**真实应用失焦/独占切换**：使用现有exe独立角色创建第二前台窗口（不是脚本/第三工程），通过C# P/Invoke SetForegroundWindow；需要时先由拥有前台资格的进程调用AllowSetForegroundWindow授权角色。必须检查实际GetForegroundWindow和WM_ACTIVATEAPP状态，而非仅API返回值。让首全屏应用在真实后台期间由其创建线程轮询Present/TestCooperativeLevel；确证DEVICELOST后才进入恢复。若未产生真实loss，仍Fail/NotTriggered，不能skip或伪造返回。
- 回切真实首focus窗口，创建线程轮询到DEVICENOTRESET，释放D3DPOOL_DEFAULT/额外swapchain，再Reset→重建资源→提交显著新帧→验证内容→再次resize/释放。前台切换合同只给真实触发原理，不承诺任意驱动必触发，必须以后实测。
- 当前独立d3d9控制即使通过也还不足以关闭official/candidate消费链恢复门：需把已证实的真实loss与被测device/surface/cache代际失效及恢复后新内容关联。不能仅完成系统device Reset就声称完整wpfgfx恢复。

官方合同原文已实际获取：
- https://learn.microsoft.com/en-us/windows/win32/direct3d9/lost-devices — 传统D3D9全屏失焦/其他应用全屏可能lost；D3D9Ex永不返回DEVICELOST；lost→NOTRESET→Reset及默认池释放。
- https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3d9-createdevice — 同应用共享focus；CreateDevice可返回DEVICELOST；Reset/IUnknown/TestCooperativeLevel线程限制。
- https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3ddevice9-testcooperativelevel — 必须创建线程；fullscreen失焦示例及NOTRESET恢复。

本节是可实施下一修正，不是本轮已经修改或执行；预算不挪用。

## 6. 回读闭环与最终矩阵

- 两full supervision全文38行、所有4个失败Result及对应Definition、Counters和TRX最终RunInfos/TestRun末尾已读；x64 TRX14079行、x8614081行。passed只统计，除必要sampler/candidate resize认证切片外没有逐一读取所有passed。
- 两stderr为空；两MTP diag末尾已读session exit2（x64 22737行、x86 22743行）。不声称逐字读取全部2万余行diagnostic。
- 必要official失败stages已读到完整cleanup，world实体root/transform删除各1、channel/disconnect/deinitialize hr0、bitmap/surface/device/window清理空错；两总stages1727行末尾HB cleanup已读。宽范围读取的4000字符截断已对关键失败与清理尾分段补读；原件全保留，不伪称所有stage逐字无截断读取。
- sampler隔离子监督/父role-exit/子TRX及必要actual官方身份/五帧证据已核，不扩大为所有隔离子角色逐一独立认证。

|最终能力门|x64|x86|裁决|
|---|---|---|---|
|最新默认full适用矩阵|1821过/2败|1821过/2败|已执行失败，非NotRun、非全绿|
|candidate resize原帧|Passed|Passed|本次full真实证据|
|隔离official sampler|Passed|Passed|实体world初始化/更新有限闭环|
|official resize本次full|初帧Failed|初帧Failed|resize/mutation NotReached；既有x64独立通过单列|
|真实device-loss与消费链恢复|NotTriggered|NotTriggered|独立控制断言Failed，线程/触发合同待修|
|target HW backend/GetDesc/GetDevice|Unknown/Open|Unknown/Open|native control或文件身份不替代target身份|
|音频与持续媒体完整闭包|Open|Open|full数字不自动关闭|
|资源自然长期收敛|Open，memoryGreen=null|Open，memoryGreen=null|有限cleanup不等于长期趋势|
|完整替换§1–10全部门|Open|Open|禁止按通过率关门|

下一工作应修正已确证real-loss线程合同及真实前台触发，并静态定位official共享full初帧差异；需要后续源码与运行授权。本轮两正式full、证据和文档均已收尾，不留下“先回读再报告”尾项。
