# r8 candidate-only resize：已实际运行，nativeControl 失败 / OverallOpen

## 执行与证据位置

本次已使用专用 Windows sandbox 推送现成 `artifacts/candidate-resize-r8-restored/Runner`，直接执行已编译 `CandidateResizeSupervisor.exe`，参数为空；未使用终端或脚本，未重新 publish。任务 `b073b965ca0b4b36b23ee1bf3ae97195`。回收原件：

`artifacts/candidate-resize-r8-restored/sandbox-readback/Evidence`

已亲读 frozen-identity、candidate stdout/stderr/result/process、supervisor stdout/stderr/exit、六项负控 assertions/exit 及相关 result/stderr、candidate-raw/stages.jsonl，并核查回收目录。沙箱“执行完成”不是通过：监督父 PID3076 实际 exit1，业务子 PID2840 实际 exit1，双方 timeout=false、reaped=true、drained=true、killError=null。外层无参数启动器自身的数值 exit 未由工具提供，不编造为0。父 stdout/stderr 为空。

## 前序错误接线与恢复（来源边界）

用户带入的前子任务账：曾发生错误接线失败，空 csproj 已恢复，并发布 `candidate-resize-r8-restored/{Runner,Package}`；publish4，2失败、2成功，此次接续前尚未执行 sandbox。此段是前子任务交接，不是本次亲见四次发布日志；当前 r8 报告原先为空，不能补造原接线错误的编译错误码或细节。旧 `candidate-resize-r8` 包不作本次运行证据。

本次亲读恢复后的两个 csproj、监督器源码、共享 VideoRun、bootstrap、SceneHeader、NativeControlHeader 及关联原件：Runner 以 Content 打包 Package；无参数入口只派生自身 `run` 并收取父退出。`run` 按 original/full/compare/missingdigest/wrongsha/wrongarch 顺序负控，恢复 native/digest 后启动 `CandidateResize.exe candidate resize`。真实运行路径及二进制身份在本次回收原件得到佐证。

## 入口、身份与合同

CandidateResize csproj 采用 net10.0-windows、win-x64、self-contained、运行时10.0.13。生成 bootstrap 仅把共享纯视频命令/场景/完成阶段替换为 resize；SHA、PE验证位于 EstablishNativeBoundary/InstallResolvers/ExecuteWpf 之前，ExecuteWpf 为独立 NoInlining 边界。原版角色在 bootstrap 直接返回64；AcceptancePaths.OriginalDll 明确抛错，不提供 fallback。

SceneHeader 先调用 RunCandidateNativeControl，再进入3轮原 RunCycle；RunCycle 源从 ReplacementStateSequenceWindowTests 的 NoInlining 方法起截取，不运行其先original再candidate的顶层测试。nativeControl 使用原 PresentNativeControl、PresentDiagnostics、SwapChainObservation、窗口线程及原 AssertCombinedPixel，保留颜色 `0x00563412`、逐通道容差1、原设备 flags0x26。本次没有修改这些源或 oracle，未以 producer 字节替代 consumer。

本次 sandbox 中监督器对 Runner/Package/Native 实际字节计算 SHA256，冻结身份（不是引用旧文档）：

- Native：`2E5DC1BCC9CC7DEF60B0EE05C622954DC9BC7C2C3A57888FEE5767ED5705AFEE`。
- CandidateResize.dll：`9A1A1CA8847A5C5462AE47AF0A9C635161C56865A81FE3D6EAA866FC2AD4EB2C`，50688字节。
- MVID：`4cc74411-8c4d-4e88-888e-45a450cfd266`。

已亲读 frozen-identity.json；candidate-resize.json 46–63 对 harness/candidate/actualNative 再次给出相同身份，Native4853248字节，nativeBasenameHandleMatches=true。stdout PresentCondition 同样给出实际加载模块路径与SHA。preSceneModules/finalModules 中 gfx 为该任务 `Package/Native/wpfgfx_cor3.dll`，无第二个原版gfx条目。本次未加载/复制原版或使用 fallback。

## 六项负控：原始退出与无Native副作用

每项保存 `{name}-exit.json`、`{name}-result.json`、`{name}-assertions.json`、stdout/stderr，不仅保留汇总。

|负控|PID|原始exit|阶段|断言|
|---|---:|---:|---|---|
|original|9676|64|bootstrap|passed=true|
|full|7388|64|bootstrap|passed=true|
|compare|1740|64|bootstrap|passed=true|
|missingdigest|4840|1|digest-required|passed=true|
|wrongsha|2672|1|sha-validation|passed=true|
|wrongarch|12080|1|pe-architecture-validation|passed=true|

六项均 timeout=false/reaped=true/drained=true/killError=null，noNative=true/noRequests=true。original/full/compare 的 result 未写业务 exitCode 字段，以进程原始64为准；不是运行了对应原版或full业务。missingdigest stderr 为 FileNotFoundException，wrongsha 为 InvalidDataException，wrongarch 为 BadImageFormatException；后三者 failureStage 与上述阶段一致。wrongarch 只在隔离任务篡改PE机器字段并重算对应digest，finally恢复原字节；正常业务重新记录了目标Native SHA。

## 真实失败：原nativeControl桌面可见性门，尚未进入RunCycle

candidate-resize.stderr.txt 1–9 与 stdout 1–11 已亲读：

- `nativeControl-before` 后，窗口 target/cover/foreground 均 `0x40C00E4`，ownerPid2840，inputDesktop=Default，session1/sessionState0，visible1/iconic0/cloaked0，cooperative=0。
- ActualSwapChain：32x16、format22、count1、swapEffect1、windowed1、DPI96；sourceRect/destinationRect均NULL，hDestWindowOverride为该窗口。
- 屏幕点(56,48)，实际像素 `0x00FFFFFF`；原预期 `0x00563412`，原逐通道容差1断言失败。
- 精确调用链：AssertCombinedPixel → RunCandidateNativeControl → CandidateResizeScene.Run → ExecuteWpf → Main。bootstrap result stage/failureStage 均 `execute-wpf-before`，业务 exitCode1。该阶段命名较粗，真正失败子阶段由 stdout/stages/异常栈定位，不能误写为尚未执行nativeControl。
- candidate-resize-process.json failures 为 business-process-failed、consumer-not-completed、reported-business-failure；监督正确返回失败，未发现仅收据误判的证据，未为此重新发布或重试刷绿。

原 PresentNativeControl 源先 Clear(0xff123456)、Present，断言返回成功，再 CompleteNativePresent；本次已到 DiagnoseNativePresent 与 AssertCombinedPixel，故失败范围是该native d3d9非Ex桌面呈现/观测前置，而不是已经复现 WPF resize 帧错误。窗口遮挡、桌面与尺寸诊断如上，但不足以唯一判定驱动/合成/窗口绘制或观测时序的根因；本次未采集GPU staging像素，不能证明后缓冲颜色正确或把原因直接归给candidate生产实现。不得通过改expected、容差、DPI、flags或跳过nativeControl解决。

## 帧与清理证据

candidate-raw 只有 result.json、stages.jsonl；无pbgra32/source帧、events.json或resources.json，因为 nativeControl 在进入 RunCycle 循环之前失败。3周期/12帧、Rendering回调收敛、HWND resize增长、解绑及producer弱引用回收均未执行认证；不能以缺失帧当通过。

stdout及stages记录4次nativeControl-release，各 refs=0：20205CB40C0、20205D15E38、20205D0C280、20205CC4B80；最后 nativeControl-released。源码 finally 释放device/factory、DestroyWindow、清空windowThread，using释放窗口线程，外层 finally 调用 Dispatcher.InvokeShutdown。已看到释放返回与真实子进程回收；原件未单独保存窗口线程终止/DestroyWindow返回/HasShutdownFinished值，不扩大为全资源释放证明。Native按既有进程生命周期持有，不人为卸载。

## 分账与后续

本次新增：sandbox1（含6个负控子进程、1个真实candidate子进程及受监督父）；publish0/build0/run_tests0；生产修改0/生产Native发布0。验收源码修改0，仅新增本报告及更新交接。前子任务publish4（2失败2成功）另账，不并作本次成功数。

r6两轮有限纯视频通过与r7原件审查继续保留，仅属有限视频；本次新视频0。resize真实尝试1、nativeControl失败1、RunCycle完成0；full/original/compare真实业务0（三个名字在本轮仅拒绝负控）。OverallOpen，不宣称resize或full完成。

下一步基于本次失败原件调查nativeControl桌面呈现/观测前置，必要时仅添加有界验收诊断并专用发布再实际运行；不改生产、不改消费者和原判据、不绕过失败，不运行会先original的正式顶层入口。本次已完成用户要求的实际sandbox、亲读结果及文档交接，但业务验收失败仍开放。
