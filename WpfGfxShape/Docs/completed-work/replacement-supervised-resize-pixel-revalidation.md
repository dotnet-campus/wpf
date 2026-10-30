# KP 原 shader resize 像素定向复验

## KP 启动进行中（历史记录）
已开始读取执行规则、完整替换方案、KO/KN/KL 与 KF 原件以及原测试方法。仅允许一次原 candidate x86 有效任务；尚未构建、发布或执行，不声明像素通过。生产与默认测试源码保持不变。复用已验收原地 resize 修复，双架构17/17为历史，不重跑。

## KP 终态：唯一 x86 原生内容前置失败，resize 像素未覆盖

### 唯一任务与实证裁决
2026-10-26 09:29:24.501–09:29:26.506 UTC，task **6dc44ac8fd3947b989353d9bda46b4a0**，监督PID4052/child9184。原ID **1eded079-5019-809c-bf7c-07ef4caf0e29**，child TRX Counters **total/executed=1/1、passed0、failed1、notExecuted/timeout/aborted0**，原精确filter命中，不是零测试。业务Exit **2**、Reason exited；BL filtered120/外180。TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut/OutputDrainCancellationTimedOut全false，HasExited/OutputCollected=true、SupervisorError=null。专用执行工具返回完成但未提供独立数值工具exit，不能与业务Exit混同。监督不是MTP，没有parent TRX。
绝对child TRX：`C:\CodingAgentSandbox\Tasks\6dc44ac8fd3947b989353d9bda46b4a0\Results\kp-x86\TestResults\child.trx`。本地证据根 `artifacts/kp-resize-pixels/readback/x86`；已亲读该TRX结果/定义/末Counters，supervision全部40行、stdout全部43行、stderr为空、stages全部21行，Results/aot-identity.json及hw-module-Native-d6c552cd402e4c30a9260e825b5f4a8c.json全文。TRX第一次读取输出截断，后续36–64补齐定义/计数，不称已逐字读所有Diagnostic。

### actual 身份（非包hash冒实际加载）
实际加载 `C:\CodingAgentSandbox\Tasks\6dc44ac8fd3947b989353d9bda46b4a0\Native\wpfgfx_cor3.dll`，GetModuleFileNameW实际路径与请求相等，实际文件SHA **20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D**，FileVersion1.0.0.0/ProductVersion1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a。本次实际test根程序集SHA **4C5229227BD7D024ED088136AF736A6363F76E990BFE18D4D56B47359A6F6FA9**、MVID **0e30de26-29e5-4cf8-99da-31e012e3c4a7**，.NET **10.0.13/X86/PointerSize4**。这是当前默认源码新发布，不拿KN旧实验test冒最新源码；原地Native含KI诊断但业务修复不受guard控制，未重发生产扰hash。
WPF runtime package includedFrameworks=Microsoft.WindowsDesktop.App10.0.13，包有PresentationCore及root官方模块；**本原方法不记录actual已加载PresentationCore路径/SHA/version，不能套KN的WPF实际身份充本次实测**，运行未进入WPF内容链，此项准确保留证据边界。没有为补身份修改源码/新增任务。

### 内容覆盖及 cleanup/refs 层级
真实nativeControl=d3d9.dll/Ex=false、cooperative0；实际swapchain32×16/format22/count1/swapEffect1/windowed1/dpi96。HWND0x1530412、owner9184/thread10424，visible1/iconic0/cloaked0，Default桌面/session1，cover就是本进程同窗口，client32×16。consumer位置(56,48)实际 **001E1D1C**，原前置expected **00563412**，AssertCombinedPixel原断言失败。没有改DPI/flags/expected、没有producer读回替代consumer、没有skip/fallback或禁并行。
最后业务stage为 DiagnoseNativePresent after; AssertCombinedPixel before，随后进入finally；未connect/channel/resource/bitmap/shader/target，**resize前FF80两帧、resize后64/48/32×24内容与target GetDesc/identity/refs均未覆盖**。旧KD的FF80→64×24→FF00仍为历史，不伪造本次FF00。不是已进入resize的新根因反例，不能据此前置失败判断原地修复失败。
21条stages含bitmap1/0与surface1/0清理占位、device/factory Release路径及windows清理后cleanup complete空失败列表；由于前置早退这些bitmap/surface/connection/channel尚为0，不能把占位写成真实四refs0。device/factory已创建且finally执行Release，但原Release不打印计数；本次没有可主张的COM refs数值/回调收敛证明，窗口仅清理路径完成而非独立DestroyWindow返回assert。KN既有真实四Release0/销毁callback0/701证据保留，不挪到本次。

### 交接、边界与下一唯一门
生产/默认测试/项目/打包/COMpartitionfill/DRDS/NativeRun/ExitDiagnostics/VisualCachehistorical/公共捕获0改，无新增Host/第三工程、脚本/终端/探针；仅本报告与三共享文档。正常小build1、正式COM publish1、Native publish0、沙箱唯一1；没有x64/full/17生命周期/WBCK/official前shader/KG/COM变体新任务。小构建正常成功是有限容量探测，空闲字节与包总字节未知；未diskfull、未置空DLL/广清历史。正常build133warnings/0errors，发布Exit0，分析器均正常；历史日志截断如前置节披露，不造新normal solution build或声明KN日志已独立补全。
**原像素缺陷仍开放，本轮resize像素未覆盖，不是通过、也不是修复后resize实证失败。KN双架构有限修复继续保留。下一唯一门是先由主代理落实同原参数nativeControl00563412可观察的真实consumer显示前提，再授权一次同ID currentNative原像素定向；本轮不重跑追绿、不盲改生产、不扩full。**
历史full1814/40和另4不相减推新full。Available/真实HWbackend/真实loss/mediaresources/final x86候选/x86官方/x64候选/x64官方四格仍开放，完整替换未完成。启动、实际前置及核验进度保留；没有覆盖旧报告/历史节。

## 最终核验阶段历史记录
唯一task 6dc44ac8fd3947b989353d9bda46b4a0 已完成，原ID TRX1执行/0过/1失败、业务Exit2；原生00563412前置实际001E1D1C，立即停止，没有第二任务。actual Native已核20B3F944…；正在收束身份/cleanup层级与三共享终态。以下保留实际前置阶段记录。

## 实际前置／运行阶段
正常 COM x86 Release 小构建成功（Exit0、133 warnings/0 errors、9.39秒），作为有限容量探测；空闲字节数未知，未假报容量。随后默认最新源码 COM x86 Release self-contained10.0.13 正常分析器发布成功1次，无NoWarn/关闭分析器；源码/打包0改。发布日志60行，stdout52781字符截前32781，不称完整日志审阅。独有 artifacts/kp-resize-pixels/{obj,bin,publish/acceptance-x86,com-acceptance-package}，PublishDir显式绝对。生产0发布，复用KN native-x86；包aot.sha256=20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D，actual待运行核。
离线精确原方法已读：RunVisualCacheConsumer(true,0xff00ff00,0x0000ff80,false,resizeWindow:true)，原生00563412前置、两原帧、64/48/32×24、提交协议及finally保留。新程序集API核原方法/KN默认生命周期存在；未用旧实验测试程序集。NativeRun监督角色源码闭合，filter无尾括号：FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive。manifest418行首尾已核，Contracts/Native/root官方/runtime10.0.13齐，source/readback分离。即将唯一沙箱任务，BL120/外180，前置失败即停。完整替换未完成。

## KP 最后补记
本报告全文及handoff/session-summary/issues最新KP受改节已写后回读，终态、实际SHA、TRX计数与覆盖边界一致；保留启动/前置历史，无旧历史覆盖。最后补记写后另行回读。没有追加任务或源码变更。
