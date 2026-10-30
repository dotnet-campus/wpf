# 官方 resize E_UNEXPECTED 根因与再验证（ClosedLimited / Complete report）

## 最终终态（本节覆盖下方执行中历史）
收尾完成，无待读尾项；不是五体全绿。五主线3 Passed / 1 Failed / 1 NotRun：
|主线|实际终态|task / 原件|
|---|---|---|
|x64 official resize|Passed（既有完整认证，未重跑）|d35a30de53714ac0952ca17908b4e206；resize-export-contract-x64-r1/official-readback|
|x64 candidate resize|Passed（既有完整认证，未重跑）|fdeaae57119d4239ae482ae7e3b5c615；resize-export-contract-x64-r1/candidate-readback|
|x86 official sampler|Passed（本次补核完父子TRX、diagnostic及cleanup）|6e607a16de814c42962743d9bc691e43；resize-export-contract-x86-r1/sampler-readback|
|x86 official resize|Failed / FirstFailureStopped|4ef9982a81244922820ee6b23b884e79；resize-export-contract-x86-r1/official-readback|
|x86 candidate resize|NotRun / stopped after official failure|无task、无candidate-readback，不补跑|

完整即时参数与原return见 `WpfGfxShape/artifacts/resize-export-contract-x86-r1/execution-receipt.md`。本次仅执行sandbox4；五体不虚报5/5。阶段累计normalbuild1/2、COMpublish2/2（隐式编译2）、sandbox4/6（本次1；允许最多两次但首错不花第二次），Native0/full0/offline0。本次源码/build/publish修改与执行均0；无脚本、无再委派。剩预算不用。

### x86 已补核 sampler
精确隔离目录 `historical-isolation/WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain/ccbb238d38124591b0fbc36ce09cbd0c`：父TRX35行/子57行均1执行1Passed0Failed；supervision父child7908/子2236 exit0，全部40行均exited、无timeout/cancel/kill/drain错误，parent-role-exit errors[]。00563412、FF80两初帧、FF0080两mutation帧精确相等；sampler无resize帧。stages68行亲读到末尾：root12/transform13/child3，M11=M22=1、size60、animation0；两资源分别deleted1；close/commit/destroy/disconnect/deinitialize hr0，bitmap/surface/device/windows cleanup complete空错。父diag75行/子118行session exit0，两stderr空。TRX已分段核末尾；diagnostic部分超长行工具截断，不声称完整raw逐字显示，原件保全。

### x86 official resize 首错与真实cleanup
原FQN `WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive`。TRX64行已完整分段读，1执行0Passed1Failed；监督40行child12984 exit2/exited，timeout/cancel/kill均false，drain无错、SupervisorError=null；MTP diagnostic120行读到session exit2，stderr空。沙箱工具只返回保存路径，不提供supervisor OS exit。
失败发生于 native control 前置像素：expected=00563412、actual=00F3F3F3（原阈值<=1未改）。32x16窗口visible1/iconic0/foreground与cover均自身、Default desktop、cooperative0，不能仅据此断言根因或泛环境恢复。stages21行显示失败在connect前，未创建channel/world/contentroot/bitmap/surface；不得写成deleted1或释放未创建对象。实际device/windows cleanup complete空错；bitmap/surface只是空槽清理路径，不是活资源释放证据。FF80两初帧、64/48/32x24三帧、FF0080两mutation帧全部NotReached，无resize export返回可认证，不是8000FFFF复发，也不是候选生产回归。

### 身份与边界
x86 actual official=5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834，产品10.0.13-servicing.26504.104/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；clean Native=147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304（只file身份，不冒本轮candidate resize实际加载）。testSHA=B04E13D0FD88DE8B9AD6C8708EA95FDE81767EE15753416F14B4EA4053524653，MVID26bdd27a-5782-4247-bfca-0735e0a4d994，.NET10.0.13/X86/pointer4。x64身份及逐帧/Deleted1/cleanup沿用下方已认证原件，不重跑：testC3D4F524…/MVID1989da39…、actual official AD1C、candidate632F。
world matrix修正是合法WPF初始化；official crossThread连接误调SameThreadPresent才是原8000FFFF确切来源，改同原帧SyncFlush。不称生产修复，不称泛环境恢复，不外推完整WPF cookie链或target backend。旧full x64 1823/1803/20fail、x86 1823/1820/3fail，23红原样单列，不用3体定向通过相减。HW target backend/GetDesc/GetDevice、真实loss触发与恢复、音频、资源自然收敛、finalfull仍Open，memoryGreen=null。
下一直接工作（未自动执行）：修正后defaultfull两arch重新取得真实分母，并处理真实loss未触发；纳入本次前置像素失败，不能以重试追绿补齐x86。需要另授执行预算。完整替换仍Open。

## 执行历史（保留，不代表最终待办）

状态：执行中；五主线已完整认证2/5（x64 official/candidate resize），禁止重跑。x86 official sampler task6e607a16de814c42962743d9bc691e43已有原件，正在补核隔离角色清理尾段、完整TRX/diagnostic；未核完前不算第三体通过，不执行后续。normalbuild1/2、COMpublish2/2、sandbox3/6已用；本次仅余必要两个x86 resize执行，Native/full/offline=0。不替代旧full基线。

## 授权与预算
normal COM build ≤2；COM publish ≤2（隐式编译单列）；Windows sandbox ≤6；Native/offline/full=0。禁止终端脚本与再委派。生产不改，expected/颜色/阈值/64、48、32×24/原后态语义不改。

## 待做顺序
1. 读取旧 task6d399cb49230401787bc70e7e39fac8f 实际原件及规则、VisualCache/OutputWindow。
2. 逐字段核官方 HWND settings / resize / cookie / channel / world matrix，定位 SameThreadPresent 实际返回路径；没有确定证据不修。
3. 确认遗漏后先登记 before/after、源码来源、最小回撤表，再仅修公共夹具。
4. 明确 COM 绝对路径 Release/win-x64/net10.0-windows build；成功后 clean self-contained 10.0.13 publish，绑定632F。
5. x64 official resize 全过才 candidate；两过才 x86 publish绑定147B及 official sampler→official resize→candidate resize。
6. 每个任务完整核 TRX、supervision、stages、diagnostics、pixels、identity、matrixroot和资源清理，失败停止后续业务但继续调查收尾。

## 修改前证据 / before-after / 回撤最小表
|范围|Before|After|源码来源|回撤|
|---|---|---|---|---|
|公共 RunVisualCacheConsumer resize 提交|official connect(0) 后无条件 SameThreadPresent(connection)|按现有 initial/mutation 同一 contract：official SyncFlush(channel)，candidate SameThreadPresent(connection)，仍严格 hr==S_OK|apifunc.cpp185-198：0→CrossThread；129-138→PresentAllPartitions；connection.cpp479-494：非SameThread直接IFC(E_UNEXPECTED)，无需进入Compose/resize枚举/锁；初始与mutation已正确分流|仅恢复该分支及只读stage日志|

72byte包已逐字段核 Generated/wgx_commands.cs331-348：type0=0x33，handle4，rect8，layer24=NotLayered0，transparency28=Opaque0，alpha32=1，child36=0，RTL40=0，enabled44=1，colorKey48零，cookie64=0，gdi68=0。当前 WS_POPUP/visible 0x90000000、exstyle8（topmost非layered），无边框无子窗；96DPI时GetWindowRect等于屏幕client bounds。此包没有DPI字段，不能把world matrix塞入包。world/contentroot保持既有修正。
HwndTarget.cs1905-1963、2098-2273：真实管理链在WINDOWPOSCHANGING先disable OOB同步flush，CHANGED再inband enable并携带最新cookie；每次disable才递增cookie。当前原生fixture窗口过程未实现该管理链，不能据此宣称完整WPF链；但本次E_UNEXPECTED在连接类型检查直接返回、根本未进入target处理，不需要臆加cookie/强制enable来掩盖错误。本轮先仅修已确定的错误export调用，后续实际原像素判断resize契约，若新失败不得同条件追绿。

## 本轮实际
normal COM build 1/2；COM publish 1/2；隐式编译1；sandbox2/6。
Sandbox2原返回保存至WpfGfxShape/artifacts/resize-export-contract-x64-r1/candidate-readback；task=fdeaae57119d4239ae482ae7e3b5c615。监督child11680 exit0，exited、无timeout/cancel/kill/drain错误；testSHA/MVID与official同，实际加载Native/632F完整预期。TRX72行passed1/failed0；前置00563412、初态FF80两帧、三尺寸FF80、mutation FF0080两帧全等；stages66行三次SameThreadPresent hr0，world13/14/3矩阵同上、两deleted1、close/commit/destroy/disconnect全hr0，candidate未初始化partition manager故deinitialize不适用；bitmap/surface/device/windows cleanup complete空错误。stderr空，diag148 session exit0。x64两体完成。
Publish2调用前：明确同COM绝对项目 Release/win-x86/net10.0-windows、self-contained10.0.13，显式AotDllPath=既有final-candidate-major-x86-r1/clean/Native/wpfgfx_cor3.dll，原aot.sha256回读147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304；新resize-export-contract-x86-r1/{publish,clean}。旧postfailure-source-rebuild-x86-r1/native不存在，不据此猜路径，改用实际已读正式包。
Build1参数：targetPath=d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj，configuration=Release，runtimeIdentifier=win-x64，targetFramework=net10.0-windows。工具返回“执行成功。完整日志共64行”；回读exit0，146 warnings/0errors，4.69s。原stdout114542字符，工具截断前94542字符，只保留尾20000，不能称全量警告日志保全。
Publish1已执行，工具返回“执行成功。完整日志共60行”；exit0。隐式编译1。原stdout57560字符，前37560被工具截断；尾有COM dll和publish目录。回读clean aot.sha256=632F完整预期，runtimeconfig includedFrameworks NETCore/Desktop均10.0.13。Publish参数：同绝对COM项目，Release/win-x64/net10.0-windows、self-contained、RuntimeFrameworkVersion=10.0.13、TargetLatestRuntimePatch=false、ComAcceptanceSandbox=true，显式AotDllPath绑定既有sampler-world-transform-x64-r1/clean/Native/wpfgfx_cor3.dll（该包aot.sha256=632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671）；新独立resize-export-contract-x64-r1/{publish,clean}，不覆盖旧证据。五体真实通过2/5：x64 official/candidate已Passed，其余待执行。
Publish2原返回“执行成功。完整日志共60行”；exit0、隐式编译2，stdout57560/前37560截断；clean aot147B完整匹配，includedFrameworks均10.0.13。COMpublish2/2，normalbuild1/2。
Sandbox3调用前：sourceDirectory=WpfGfxShape/artifacts/resize-export-contract-x86-r1/clean；exe=WpfGfxShape.ComAcceptance.exe；arguments=[--supervise-exit,Results/x86-official-sampler,120,FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain]；localOutputDirectory=WpfGfxShape/artifacts/resize-export-contract-x86-r1/sampler-readback；timeoutSeconds180，全回收。无参原FQN CaptureHistoricalControls.cs25；嵌套监督90秒、parent等待110、外层120/沙箱180，沿用原链不改。预占sandbox3/6。
Sandbox1原返回：沙箱执行完成，结果保存至 d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\resize-export-contract-x64-r1\official-readback。task=d35a30de53714ac0952ca17908b4e206；supervision全40行：child12400 exit0、exited、timeout/cancel/kill均false、output collected、drain无超时、SupervisorError=null；工具未提供supervisor OS exit。TRX72行，passed1/failed0精确原FQN；三次64/48/32×24均SyncFlush hr0且FF80，前置00563412、初态FF80两帧、mutation FF0080两帧全等。actual official AD1C完整hash与旧版一致；testSHA=C3D4F524400F5209C8DE44BD4621F2CA2431D1D7CF9BC0962FBE490FC57CD30F，MVID1989da39-bdf7-4b19-86b2-8decb666b6b9，runtime10.0.13/X64。world root13/transform14/child3，M11=M22=1、60byte、animation0；release world/contentroot deleted1，close/commit/destroy/disconnect/deinitialize全hr0；bitmap/surface/device/windows cleanup complete空错误。stderr空，diag148行session exit0。非历史隔离，无子角色TRX。backend仍未知，不是全WPF cookie链证明。
Sandbox2调用前：同clean/exe，arguments=[--supervise-exit,Results/x64-candidate-resize,120,FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive]；localOutputDirectory=WpfGfxShape/artifacts/resize-export-contract-x64-r1/candidate-readback；timeoutSeconds180、全回收。预占sandbox2/6；official已全过才执行。
Sandbox1调用前：sourceDirectory=WpfGfxShape/artifacts/resize-export-contract-x64-r1/clean；exe=WpfGfxShape.ComAcceptance.exe；arguments=[--supervise-exit,Results/x64-official-resize,120,FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive]；localOutputDirectory=WpfGfxShape/artifacts/resize-export-contract-x64-r1/official-readback；timeoutSeconds=180；outputRelativePath省略全回收，预占sandbox1/6。无参FQN已核HardwareClosure.cs20-21，无discovery。旧 full 23 红保留，不相减。HW target backend未知；真实loss、音频、资源自然收敛、finalfull均未证明。
