# KQ 原参数 native consumer 下一门独立有界审查

## 裁决：阻塞，未启动构建或执行

本轮为独立文件审查及旧证据回读，不是新的显示校准成功。当前执行规则允许在正式 ComAcceptance 中新增必要纯 C# 测试，故不存在一般性的“禁止新增测试”规则；但本次授权同时禁止重复历史失败探针、换名重跑和扩大共享入口。综合现有设施与预算，未找到可在这些限制内直接启动、具有新判别条件的单次精确执行身份，因此停止，不新增测试源码。

不是断言该沙箱永久无法显示，也不是将所有硬件失败归环境。历史成功反证仍保留。缺的是与原参数相容且已说明发生了什么变化的真实显示消费前提；当前仅有再次观测同一失败边界的建议，没有已证的条件变化或具体 API/坐标/窗口根因。新加提交前基线可排除旧色假通过，但不能使原本没有交付的帧得到交付，亦不能单独证明这次重跑不同于历史失败探针。

## 已读规则与仓库设施

已分段读取完整替换方案1–120行、current-execution-rules全文、current-stable-baseline全文、恢复入口最新KP/KO及KP报告、HH终态相关段落；长历史入口未声称全文读取。方案允许两个正式项目，真实consumer内容不可由producer读回、事件完成、软件fallback或空白相等替代。仅有界审查本次门，不重新展开完整矩阵。

正式项目 `Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj` 为net10.0-windows/WPF/nullable/MSTest4.0.1，StartupObject为NativeRun；其现有MTP入口与ExitDiagnostics精确filter、BL120机制已读取，未修改。文件索引工具用于确认项目文件范围；没有新Host、工程、脚本或委派。

### 原参数匹配与已有失败设施

- `D3DImageTests.VisualCache.cs:55–94`：CreateOutputWindow("Two input shader acceptance",40,40)，EstablishObservableWindow，Direct3DCreate9(32)，adapter0/HAL1/behavior0x26，32×16/count1/DISCARD1/windowed1/requestedFormat0，PresentNativeControl(false)→DiagnoseNativePresent→AssertCombinedPixel(00563412)，然后才connect。
- `D3DImageTests.OutputWindow.cs:30–65`：专用窗口线程Invoke创建，同位置，exStyle8/style90000000、无背景刷。原CompleteNativePresent有既有有界GPU完成逻辑；本轮未加循环或等待。
- `D3DImageTests.SwapChainObservation.cs:23–32`：一次Clear FF123456，普通Device9 Present slot17，NULL src/dst/dirty、同HWND override，然后原CompleteNativePresent；未变完成顺序。
- 已有 `D3DImageTests.HhNativePrecondition.cs`（全文分段读）由HH_NATIVE_PRECONDITION默认关闭，旧窗口段48–89复用上述窗口、设备、Present及consumer采样，记录client/screen并释放真实device/factory和销毁窗口。这不是不存在独立native前置设施。
- HH方法 `WhenHhNativePrerequisitesRunThenBothOriginalContentContractsAreRequired` 先加载官方模块、运行CheckFreshDesktopDelivery(1,false)三帧控制，再执行旧单Clear前置；因此整个现有测试不符合本轮单提交1及不追加其他变体的预算。不能直接用该完整filter冒本轮合法身份，也不能把条件编译未进入视为通过。
- HH历史报告 `replacement-supervised-shared-consumer-contract-fix.md:42–74` 记task4b901b8e7b094574bf51400213e9cb41，旧段client/screen00CDB38B而非00563412，资源0/0及销毁1；这是报告引用，旧HH原始产物未在本轮重新实跑或全文回读。HH没有提交前基线，且旧段增加trace/front观察，不夸为字节级无扰时序复现。

结论：已有原参数观察设施，但现有整体入口超预算；仅抽出旧段、加基线并更名，没有新的已证显示条件，会重做历史已失败边界。无需新增重复partial。可选独立GDI标记未添加：它至多验证观察链，不能建立原native消费前提，且增加窗口/呈现活动对本次最小门没有必要性证据。

## KP 实际 readback 复核及调查结论纠正

证据根：`WpfGfxShape/artifacts/kp-resize-pixels/readback/x86/Results`（相对工作区）。本轮亲读stdout全部43行、supervision全部40行、hw-module JSON全部10行、stages全部21行；TRX直接读取1–12及36–64行，未称全文逐字审查。

**child.trx实际存在。** 目录枚举未列TestResults，不构成缺失证明。按stdout给出的task绝对路径映射直接读取 `kp-x86/TestResults/child.trx` 成功：原ID1eded079-5019-809c-bf7c-07ef4caf0e29、方法WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive、Failed；Counters total/executed1/1、passed0、failed1、timeout/aborted/notExecuted0；RunInfo明确exit2。调查中“当前KP readback缺child.trx”不成立，不为补件重跑。原KP报告的存在性记载得到本轮直接读取支持。

task6dc44ac8fd3947b989353d9bda46b4a0的实际模块JSON记录Requested=Actual，均为该task的Native/wpfgfx_cor3.dll，actual加载文件SHA **20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D**，不是用包aot.sha256替代actual；本轮没有另行对二进制重新计算hash。实际testSHA **4C5229227BD7D024ED088136AF736A6363F76E990BFE18D4D56B47359A6F6FA9**、MVID0e30de26-29e5-4cf8-99da-31e012e3c4a7、.NET10.0.13/X86。actual PresentationCore身份未在本原方法记录，不移植KN身份。

stdout实证：普通d3d9/ExFalse，32×16/actualFormat22/count1/swapEffect1/windowed1/DPI96，screen(56,48)，同进程同cover窗口可见，cooperative0；client与screen均001E1D1C，expected00563412。原AssertCombinedPixel错误文本包含既有每通道<=1逻辑；本轮没有变阈值，不将原方法夸为新严格零阈值校准。未进入connect/shader/resize，不能判原地resize修复失败，也不能判像素已通过。

业务ExitCode2/Reason exited、BL120，TimedOut/Cancelled/KillAttempted/KillReturned/两项drain timeout均false，HasExited/OutputCollected=true、SupervisorError=null。原执行工具无独立数值exit；本轮只回读，不产生新的工具exit或运行数字。stdout报告1失败0成功0skip，与TRX闭合。

cleanup证据仅到原finally路径：stages到cleanup complete空失败列表；未创建bitmap/surface的零占位不能冒真实四refs0，device/factory无Release返回计数。窗口是清理路径完成，非独立DestroyWindow返回值证明。没有本轮新增对象、窗口、refs或cleanup实测。

## 最小外部前提与下一授权门

1. 监督方先提供可实际消费此原参数普通Device9 Present的可见、未锁定且输出持续更新的交互显示会话，并说明相对于KP/HH失败现场的具体变化或提供具体根因修正证据；仅Default/Active/visible/cooperative0不充分。无需承诺物理GPU，亦不以Basic Render Driver名称推导永远不可用。
2. 在此条件已明确后，再授权一次精确native-only入口的复用/最小收窄方案：不得完整调用HH的三帧控制，不修改NativeRun、共享捕获、生产、项目或打包。先确认可识别方法实际编入并可由现有MTP精确选择，不能只给编译符号或零测试通过。
3. 该门仍须提交前client/screen基线非00563412、保持原窗口/线程/参数/Present完成顺序、仅一次原Clear/Present，之后两真实consumer样本严格00563412，真实Release与窗口销毁分别记录；未建对象写未创建。基线已等于目标不判新交付。无producer冒consumer、变expected/flags/DPI、skip/fallback或额外等待追绿。
4. 校准任何结果先停并回报；原ID currentNative shader/resize仍需另行授权。任何前置失败/参数漂移/身份不闭合/cleanup失败/timeout即停，不追加x64/official/full/17条或其他变体。

## 本轮预算与计划推进

正常COM x86 Release小build **0/1**；正式COM x86 self-contained10.0.13发布 **0/1**；Native发布 **0/0**；Windows沙箱任务 **0/1**；新精确测试执行 **0/1**；新原native提交 **0/1**；终端/脚本/委派 **0**。没有消耗BL120/外180运行预算，没有新TRX。生产、测试源码、默认原方法、公共helper、NativeRun、项目、打包均零修改；仅文档追加，保留历史证据。

KN双架构17条有限保留仅沿用既有KO/KN裁决，本轮未独立重新核其全部原件，更未重跑；原像素、真实loss/HWbackend/Available/mediaresources及最终四格仍开放，完整替换未完成。本轮推进的是排除重复探针并纠正TRX缺失误判，不是校准或产品验收通过。
