# 测试建设线 A：真实 WPF 状态序列

## 交付与边界

新增 `Tests/WpfGfxShape.ComAcceptance/ReplacementStateSequenceWindowTests.cs`；未改项目文件、共享入口/生产代码、DesktopDuplication、InternalContextLayout/Realizer/FLOAT、golden、skip 或阈值。未发布生产 DLL。只使用专用发布和 Windows 沙箱运行已编译的 C# 测试。全部证据保存在 ignored artifacts，未写 tracked ReferenceData。

本包没有完成完整替换验收。新增测试成功发现双架构候选真实 WPF 窗口启动失败；保留失败而不换成软件窗口或删断言。

## 五维盘点与本包增量

|维度|已有代表覆盖|新增覆盖及剩余边界|
|---|---|---|
|二进制兼容|PublicAbi、COM、UCE、独立 context 布局|真实 PresentationCore/WindowsBase resolver 到官方/候选；保存实际模块路径、SHA、版本、指针宽度。不是全面 ABI 审计|
|功能结果|BitmapRenderTarget、Composition、shader、glyph、Pcore nested/images|真实 WriteableBitmap -> Image；红/蓝/绿/黄资源原位更新，每帧全像素非空/正确颜色/非旧帧检查，官方候选零阈值比较。候选首帧前失败，差分未完成|
|时序行为|AsyncUce、通知、媒体回调、Pcore dispatcher、旧 LifecycleSequence|3 轮创建/首帧/更新/窗口实际布局 resize/新帧/close；CompositionTarget.Rendering 实际回调，注销后 drain 不再增加，Closed 恰好一次。仅官方完成|
|资源行为|stream/media/channel lifetime；旧序列 weak refs|每轮记录 producer weak ref、句柄、线程、private bytes、managed bytes。官方 producer 三轮均回收；候选未到清理完成。不宣称 GPU/COM refs 或长期泄漏闭合|
|恢复能力|既有 HW resize 与独占 device-loss 探针|新增窗口实际大小从24x16到40x24；实跑既有真实独占探针，但没有发生首设备 loss。真实恢复未覆盖|

原有 `Pcore/LifecycleSequenceScenes.cs` 是 DrawingVisual/RenderTargetBitmap、新建 target 的 resize，不是 HWND 的 producer/consumer 关闭。新增选择有界窗口图像序列补该缺口；不重复开发捕获链。

## 新测试结构

两个描述性测试：`WhenWindowImageUpdatesAndResizesThenFreshFramesAndCallbacksConverge` 和 `WhenFrameIsBlankOrStaleThenContentOracleRejectsIt`。

沿用 NativeRun.RoleAsync 启动同一正式 exe，以 MSTest 精确过滤和 results-directory original/candidate 作为隔离角色；子测试在模块初始化完成后启动独立 STA。不修改共享 Main。子进程由既有三分钟 timeout 管理，STA join 60 秒。初期模块初始化启动线程/等待导致初始化锁超时，直接初始化线程又是 MTA；这些夹具问题已经从最终源码移除，早期失败目录保留，不计为产品问题。

官方和候选不互相转发。每轮 source-*.bin 保存4x4原始生产图像，frame-*.pbgra32 保存消费内容，events.json 保存资源更新/新帧/resize/close及回调计数。startup-identity.json、loaded-before-show.json 在 Window.Show 前落盘，避免原生崩溃使 finally 成为唯一证据来源。正常结束另有 report.json/resources.json。

backend 明确写为真实 WPF HWND/Image/WriteableBitmap producer + RenderTargetBitmap 软件内容 consumer。未强制软件 fallback；不声称 RTB 读回证明屏幕呈现或候选 HW 后端。RenderCapability.Tier 只标 capability，不作为 backend 通过证据。

## 最后源码实际构建与运行

ComAcceptance 双架构 Release 自包含发布均退出0，RuntimeFrameworkVersion=10.0.9、ComAcceptanceSandbox=true，使用已有架构 Native DLL；未关闭分析器。独立 publish/staging 名为 replacement-state-a-win-x86 / replacement-state-a-win-x64。

最终筛选：新增两个测试 + 既有 `WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover`。不是全量基线。

|架构|主 TRX 计数|官方隔离角色|候选隔离角色|
|---|---|---|---|
|x86|total3 executed3 passed1 failed2 notExecuted0|1测试通过；3轮12帧完成|进程退出 -1073740771；无首帧|
|x64|total3 executed3 passed1 failed2 notExecuted0|1测试通过；3轮12帧完成|进程退出 -1073740771；无首帧|

passed1 是内容检测器负控；两个失败分别是候选窗口序列和未能触发真实 device-loss。子角色运行不重复累计进主计数。两架构官方共24帧已独立验证颜色/新鲜性；候选像素比较没有执行，绝不计为零差异通过。

证据路径相对本工程根：

- x86 根 `artifacts/sandbox-readback/replacement-state-a-win-x86-final/`；TRX `TestResults/replacement-state-a-x86.trx`，计数第64行。
- x86 业务根 `Results/replacement-state-window/ec25fd4493a84918b02871a7d9835611/`。
- x64 根 `artifacts/sandbox-readback/replacement-state-a-win-x64-evidence/`；TRX `TestResults/replacement-state-a-x64.trx`，计数第68行。
- x64 业务根 `Results/replacement-state-window/d0f3ed1426924cf8a6ef3af0a6e5df92/`。
- 各业务根的 original/ 有12份原始 frame、12份 source、events、resources、report；candidate/ 有 startup-identity、loaded-before-show、events 和 uce-diagnostic.log。候选最终 report/资源/帧缺失是崩溃结果，不补造。

已回读 TRX、实际模块 JSON、官方事件和资源序列、候选原始 UCE 包。最终测试之后未再改测试源码。

## 实际身份

两架构官方 .NET/WPF 10.0.9；官方 FileVersion `10,0,926,27113`、ProductVersion `10.0.9-servicing.26270.113`，commit `901ca941248413c79832d2fdbd709da0c4386353`。

|架构|实际官方 SHA256|实际候选 SHA256|
|---|---|---|
|x86|0CC79CC937B17827344B8652556A4CFD774CFF64B7FD2709EF5284C480894047|747F307104A9E51E9689307BD85CAABC60EB60294F7F307E21CF687F77E7A142|
|x64|098D9CEFACA7B1E36460EB28F2725D7AFDB8E93855C29F8DD3627A1FB692AE8E|2BFC4AC24A83EBAF45A4B375CDC5FE1F9C90BD9370D73BD590C259091C5BF58F|

候选版本1.0.0.0，ProductVersion `1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685`。actual loaded-before-show 的候选路径分别为任务 aed4767e78ab41de8a292d22da66f427 / f1d432f823ff4bbebcdd80f765813aad 下 Native/wpfgfx_cor3.dll；官方位于同任务根 wpfgfx_cor3.dll。x86 SHA 再次不同于旧报告，不能合并旧全量；本包没有重发生产，来源调查交给身份/问题线，不阻挡测试建设。

## 提交问题线的最小复现

运行新增窗口测试即可：同一 exe 的官方隔离角色完整通过，候选在 Window.Show / HwndTarget 更新阶段 `UCEERR_RENDERTHREADFAILURE (0x88980406)`，调用栈包含 DUCE.Channel.SyncFlush -> HwndTarget.UpdateWindowSettings/UpdateWindowPos -> WndProc。两架构重复观察到，不判作 device-loss，也不据此断言根因已定位。

候选 uce-diagnostic.log 首个错误均为 `channel=3, thread=2, HRESULT=0x80070057`，packet 首字节 `33000000`。完整原始包在上述 candidate/uce-diagnostic.log，后跟两个空 packet 的 channel=0 失败。场景只需4x4 WriteableBitmap、Image和无边框真实窗口，尚未进入 resize。请问题线优先解码并定位该有效 WPF 命令。

官方资源原始样本：x86 handles 514/516/518、threads23/23/23；x64 handles495/497/499、threads22/23/23；producerAlive均false。三轮不足以证明句柄收敛，观察到每轮+2需后续预热和更长序列调查，不当作泄漏已确认或资源完全通过。

## 真实 device-loss 可触发性

未改既有 HardwareClosure 测试，直接实跑纯 C# D3D9 独占位移探针。双架构均 firstCreate=0、1920x1080 format22；secondCreate=0x88760868，而 firstCooperative=0。失败的是第二设备创建，首设备没有 D3DERR_DEVICELOST；不能用该错误声称设备丢失发生。未执行恢复后新帧，也未连到 WPF 的实际设备恢复链。

需要具备可验证实际设备丢失条件的交互式 Windows GPU 会话（允许独占焦点切换/真实 adapter reset），并在被消费的实际设备上确认 loss、随后重建并读回新内容。当前沙箱这条独占触发机制不满足条件；没有添加模拟通过、软件 fallback 或 skip。

## 下一批工作

1. 问题线用本最小窗口序列和 raw 0x33 UCE 包定位候选 render-thread failure，修复后双架构重跑，才能验证候选12帧及关闭收敛。
2. 在官方预热后扩大资源循环，调查每轮+2句柄，增加 COM/GPU refs和窗口/回调对象的明确所有权证据；不以三轮 GC 代替完整资源门。
3. 在允许真实丢失的交互 GPU 条件下执行 WPF/D3DImage 连续消费恢复，并记录实际 adapter/backend、loss确认和恢复后新帧；HW捕获由其专线处理。
4. 后续补多窗口/共享资源、媒体在途回调与重入关闭、必要偏序 oracle 负控；当前只完成有界建设包，不外推五维全覆盖。
