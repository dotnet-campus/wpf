# Consumer 可观察合同、x86 real-loss 与默认 full 收尾

状态：CloseoutCompleted（本轮实际执行与证据收尾）；整体替换 Open。七入口已同步最新实际基线，旧历史保留。

## 1. 亲读与合同裁决

本轮亲读嵌套 next-session-handoff、执行规则、章程实现约束、完整方案关键规范、旧 exclusive-foreground-and-cache-evidence-closeout（实际在根 Docs/completed-work）及 present-occlusion receipt。旧 df9e/3ab full 数字引用既有报告/receipt，不冒本次重读全部原件。亲读 official-cache-8836-7970572.jsonl 1–16关键内容：source两份12×1、dirty0、mode/brush/root/world提交、flush0、expectedFF80/actual0及事后peek0；超长读取截断后补读11–16，未把peek0解释为从未呈现。

本轮直接检查官方源码：
- core/resources/d3dimage.cpp 90–117：CMilD3DImageDuce::GetBitmapSource 根据software或interop且未disabled返回source；这是渲染资源内部方法，不是现成公开COM getter。
- core/resources/BitmapCacheBrush.cpp 70–165：更换mode/target会注销/注册cache；realization调用PreCompute→UpdateCaches→CacheSet.GetBitmapSource，null则无brush，再GetSize。现有fixture显式mode/target及矩阵，无确定合法缺项，不盲改。
- core/hw/InteropDeviceBitmap.h 及 cpp 1015–1050/1100–1162：GetAsSoftwareBitmap调用CopyToSoftwareBitmap，后者读取m_pIUserSurface；源码明确back buffer，UI线程不能随意读取front buffer。即便得到IWICBitmapSource及尺寸/像素，只能证明producer snapshot，不能认证cache texture或HWND consumer。GetSoftwareBitmapSource是内部render-thread最后software copy接口，不是公开消费观察合同。

结论：本轮没有证明可从公开IWIC/interop snapshot/MilResource句柄合法取得该cache consumer的非空、尺寸和内容；不能将内部C++ public成员当COM ABI，也没有私有布局硬读、修改官方DLL或PDB断点实跑。所有未证消费状态仍Unknown。未增加诊断信号，不重复单独officialcase；转用户授权的x86对称loss与修正后默认full。

## 2. 执行范围、参数与预算

本轮源码/工程/资源/生产修改0，Native publish0。保持expected00563412/FF80/FF0080及原比较、尺寸序列、事件、默认并行；没有skip/fallback/循环等色/Sleep/多flush/隔离求绿。原AssertCombinedPixel源码逐通道<=1是既存合同，本轮未改，不误称已经改成threshold0。

执行账：`../../artifacts/consumer-contract-x86-closeout/execution.md`。COM publish1（Release/win-x86/self-contained，隐含编译1），显式normalCOMbuild0/3、COMpublish1/2、sandbox2/4、Native0/0。余normalbuild3/publish1/sandbox2；不是耗尽停工，而是唯一full已执行、无确定新修正不重复。

发布绝对COM csproj `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj`，明确RuntimeFrameworkVersion10.0.13、ComAcceptanceSandbox=true、AotRuntimeIdentifier=win-x86、显式AotDllPath=`...\artifacts\final-candidate-major-x86-r1\clean\Native\wpfgfx_cor3.dll`，独立PackageDirectory=`...\artifacts\consumer-contract-x86-closeout\clean\`、PublishDir同根publish。exit0；完整返回日志61行已读，stdout57561字符只保留末20000，前37561截断，warning总数Unknown。clean aot.sha256亲读147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304。没有发布根项目或默认Aot漂移。

## 3. 新x86真实loss控制：ClosedLimited

沙箱task **b7fd380ab87d42b08b0042c53464824b**；source上述clean、exe WpfGfxShape.ComAcceptance.exe，参数 `--supervise-exit Results/loss-x86 120 FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover`，外180秒。

原件根 `../../artifacts/consumer-contract-x86-closeout/loss-readback/Results/loss-x86/`：亲读supervision.json40行、TestResults/child.trx48行、stages.jsonl6行、stdout12行和空stderr。目录漏列TestResults但按精确路径读取成功，没有判缺失。

- supervisor12532/child10840，exit0；TRX total1/executed1/passed1/failed0，其余0。无timeout/cancel/kill/drain/error。
- .NET10.0.13 X86；test SHA **5BBE8377EC439A53BF4C47CC339D3D84167345125B9780B25DBB6EB15B1AAA5D**；MVID **759e2b50-271a-46b3-9061-8caafc530a1a**。
- system D3D9 adapter0/type1/windowed0，三次SetForegroundWindow1，双向Allow1，session1。
- displaced Present/cooperative88760868 → beforeReset88760869 → Reset0；初始FF123456与恢复FF654321实际相等，均见stages；trigger exit0、device/factory/window complete。

此为新x86对称system控制闭环，与旧x64历史控制互补；**不是产品consumer实际device-loss恢复**，这个case未实际加载产品消费device。TRX记录candidate包SHA不等于证明产品消费。旧历史full失败不因本项被删除。

## 4. 同包修正后x86默认full：1823/1821/2红

沙箱task **2a5cb30fa5214ca58e891858eb17ad97**；同clean/exe，参数 `--supervise-exit --full Results/default-full-x86 1500`，外1800秒，无额外诊断配置。唯一一次完整full，保留全部默认适用用例。

原件根 `../../artifacts/consumer-contract-x86-closeout/full-readback/Results/default-full-x86/`：supervision38行、TRX关键结果/definitions/Counters/尾、stages关键现场与清理尾、stdout失败内容/空stderr已亲读。不是逐字重读14087行所有Passed结果。

supervisor5116/child2496，exit2、无timeout/cancel/kill/drain/error；runtime/test SHA/MVID与定向相同。TRX14080 Counters：total1823/executed1823/passed1821/failed2，skip/notExecuted/timeout/error/aborted等0，14087终尾完整。全部失败如下：

|TRX行|失败方法|实际业务及界限|
|---|---|---|
|2309–2340|WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive|official actual5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；native00563412通过，初始expectedFF80/actual0；后续resize未到达。|
|2341–2362|WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover|初始SetForegroundWindow0前置失败；未创建exclusive device、未触发真实loss。stages1330明确foreground PID6252/thread7720/session1，caller2496，sameProcessFalse。不得称已修复完整full前台资格。|

candidate同场景实际模块147B已加载，TRX2275–2308读证：初始FF80、64×24/48×24/32×24 resize后FF80、资源更新FF0080均实际相等，native00563412通过；不外推official消费或backend身份。

official stages1280–1329：两source创建、资源提交、world单位矩阵、commit0、两ProcessPresent完成、初帧present后；world/root释放、close/commit/destroy/disconnect/deinitialize HRESULT0，surface/device/windows完成，cleanup末字段空。产品完整自然资源收敛仍未证。stages末1710–1728另为HB完整清理，不误归official。

**新的判别信号**：相同test包同x86 runtime，定向系统loss真实通过，但full初始前台资格被另一PID持有而失败；故双向child授权修正只覆盖已经取得初始资格后的loss链，没有解决sharedfull启动资格。真实前台owner无资格合作证据，不能盲AttachThreadInput/SendInput或认为BOOL0可忽略。该失败真实保留，不重复full、不改expected。官方cache根因仍Unknown。

## 5. 当前整体与下一具体工作

本次真正关闭：x86 system D3D9真实loss/Reset/两色/正常清理有限控制；新取得当前Release x86默认full（仍红）。没有关闭official cache主红，也没有关闭共享full的初始foreground前置；产品consumer loss、HWbackend身份、实际音频、自然资源/worker趋势、双架构最终full及完整替换均Open，memoryGreen=null。production x64仍632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671，x86仍147B；本轮未更新x64默认full。

下一具体修正路线：在已有supervisor/测试partial内核实合法初始foreground资格转交契约（已具资格的实际owner→测试PID，不能只在child激活后授权），必须先确认owner可协作；当前full现场owner6252可作为定位输入，未知其程序身份不补造。如没有合法owner协作，不做伪修复。cache路线只在匹配官方符号与既有只读dump能力确立实际消费断点/状态观测后增加信号；producer snapshot不得代替。不得重复本轮已否定的公开consumer getter假设或原样追绿。未完成全替换，不宣布完成。
