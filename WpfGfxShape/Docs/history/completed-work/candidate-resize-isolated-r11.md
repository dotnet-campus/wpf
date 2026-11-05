# r11：resize 前置静态依赖核实；原 RunCycle 为 RTB consumer / OverallOpen

## 本轮范围与结论

已亲读 latest handoff、r10、CandidateResize 项目生成规则、SceneHeader、NativeControlHeader、原 ReplacementStateSequenceWindowTests.cs，以及当前生成的 ResizeScene.cs；回读 r10 原件。遵守监督者仅分派读文档的范围，本轮只新增本报告并更新恢复入口。生产修改0、测试修改0、oracle/expected/DPI/flags/阈值修改0，无终端脚本、原版加载、复制 fallback 或删除用户文件。

**发现任务A的实际前提冲突：当前入口所称“原 RunCycle”不是 HWND 像素 consumer，而是 RenderTargetBitmap 软件内容 consumer。不能既运行未削弱的原 RunCycle、又不使用 RTB。** 本轮不通过执行被明确禁止的 RTB 路径来制造独立候选 resize 结果，也不替换原 oracle。原组合仍失败，独立真实 HWND resize 诊断尚未执行，OverallOpen。

## A：静态依赖与有限判定

- `Tests/CandidateResize/SceneHeader.txt:15` 在任何周期之前调用 `RunCandidateNativeControl()`；`:21-30` 才进入3周期 RunCycle 和 producer 弱引用 GC 检查。nativeControl 抛出时循环确实不进入。
- `Tests/CandidateResize/CandidateResize.csproj:15-22` 从 `ReplacementStateSequenceWindowTests.cs` 的首个 NoInlining 方法起截取原方法体，拼接 SceneHeader。没有把 RTB 改成桌面 consumer。
- 原 `ReplacementStateSequenceWindowTests.cs:197-283` 的 RunCycle 不调用 nativeControl，也不读取其 device、factory、HWND 或校准结果。它自行创建 WriteableBitmap/Image/Window、订阅 Rendering、更新内容、请求尺寸增长，并在 finally 解绑及关闭窗口。原 Run 外层在 `:155-168` 直接调用 RunCycle，并无此 D3D9 校准前置。
- `NativeControlHeader.txt:20-25,51-57` 的校准有独立 PresentationWindowThread、局部 factory/device/window，finally 释放并清空 windowThread。未见 RunCycle 静态依赖校准成功的证据。这支持将独立校准与候选诊断分开报告的设计，但不能仅据静态分析宣称所有失败类型后都能安全继续。
- 决定性证据为原 `ReplacementStateSequenceWindowTests.cs:251-258`：创建 RenderTargetBitmap，调用 Render(content) 与 CopyPixels，写出 frame 文件后传给 IsExpectedFreshFrame。当前生成 `artifacts/obj/CandidateResize/Release/net10.0-windows/win-x64/ResizeScene.cs:93-98` 同样如此。该生成文件只读，未修改。
- 原源码 `:129` 的 backend 描述本就明确写有“RenderTargetBitmap software content consumer”与“HWND presentation pixels and HW backend are NOT verified”。不能把真实窗口存在、ActualWidth/ActualHeight 增长和 Rendering 回调误当作桌面像素 consumer 已验证。
- 原 `:259-282` 保留尺寸增长、回调、close/unsubscribe 收敛及 producer WeakReference；这些也不等于 COM 对象全部 refs 已认证。不能把 nativeControl 的 COM Release 证据转记到 RunCycle。

因此，**前置在控制流上额外阻断一个原本无校准数据依赖的内容/窗口生命周期测试，但尚不能据此说它阻断了现存独立“真实 HWND 全像素 resize 验收”**：当前链接方法没有那种 consumer。新增独立诊断覆盖在原则上不等于绕过原门；不过本轮未实施，不能把设计建议当已运行结果。

在“不改 oracle / 不用 RTB”的当前边界下，不能直接运行此方法满足任务A。也不能擅自把其采样替换为 GetDC/截图、缩减全量像素或以 producer 字节验收。下一执行授权应先明确一个已有且不含 RTB 的真实 HWND RunCycle/consumer 入口，或另行授权新增独立 HWND consumer 诊断；后者不能冒称未修改的原 RunCycle，更不能追认原组合通过。不要继续无目标 device/chain 实验。

## r10 原件回读（历史运行，不是 r11 新执行）

路径：`artifacts/candidate-resize-r10/sandbox-readback/Evidence/`。

本轮亲读 `supervisor-exit.json`、`candidate-resize-process.json`、`frozen-identity.json`、`candidate-resize.stderr.txt`、`candidate-raw/stages.jsonl`：

- 父 PID676 exit1，子 PID12176 exit1；两者 timeout=false、reaped=true、drained=true、killError=null。子 passed=false，failures 保留 business-process-failed / consumer-not-completed / reported-business-failure。
- frozen Native SHA256：`2E5DC1BCC9CC7DEF60B0EE05C622954DC9BC7C2C3A57888FEE5767ED5705AFEE`。
- frozen harness SHA256：`3CF053E1A5688D1F9626FACA038CB633447F2B7E5DD5841B99FCAA7FE0147C6E`；MVID `d282e8e7-ff7c-4164-879f-47e2663c7d6c`。这里是读取历史冻结身份，不是本轮重新计算或新进程加载认证。
- stderr 精确失败在 AssertCombinedPixel：actual `0x00FFFFFF`，expected `0x00563412`，原逐通道容差1断言失败；经 RunOwnedNativeControl → owner Invoke → RunCandidateNativeControl → CandidateResizeScene.Run 退出，尚未调用 RunCycle。
- stages 共18行，16条 release；5条中间 refs 为4/3/2/3/1，其余记录为0，最后 surface/chain/device/factory 的相关释放均到0，尾部 nativeControl-released。不能写成所有 Release 都为0，也不能延伸为候选 producer 已清理。
- 历史 RunCycle 未进入、完成帧0、完成周期0、candidate producer refs/弱引用结果未产生；无真实 candidate resize 通过或失败结果，只有“未运行/被前置阻塞”。未发现可归因生产 candidate 的确切缺陷。

r10 其它模块列表和逐阶段像素细节仍以 r10 报告及原件为准，本轮未重新遍历全部 finalModules，不能声称做了新一轮模块完整认证。

## B：桌面状态仍 Unknown

本轮没有新增专用程序、截图或执行，也没有切换桌面、更改配置、录制无关区域。r10 的单点 DC/GDI 结果不等于 DWM 实际消费证明；未新增原 HWND owner-thread desktop 与 input desktop 同时对照、锁定状态、DwmIsCompositionEnabled 或仅窗口区域截图的同一单调时间证据。因此环境硬限制仍 **Unknown**，不能把白像素直接定性为锁屏、遮盖、会话限制或生产缺陷。

若获执行分派，B 应与原窗口存活期绑定并记录同一 QPC/Frequency，只抓测试窗口区域；不重新进行 device/chain 参数实验。原 HWND 已销毁后另一个程序的当前桌面状态不能追认 r10 当时状态。

## 本轮账与停止点

r11：build0、test0、publish0、sandbox0、新运行0；生产修改0、测试修改0、Native 发布0、原版真实业务0、fallback0、新截图0、独立 resize 帧0。只更新2份文档，不复制历史大包。

历史 r9+r10 的 publish12/sandbox6 仍按原报告保留，不将本轮静态核实算成运行。不报告整体完成；组合失败未解除，独立真实 consumer 诊断未完成，任务B未执行。停止于已明确定位的 consumer 合同冲突，而非 nativeControl 单点无限实验。
