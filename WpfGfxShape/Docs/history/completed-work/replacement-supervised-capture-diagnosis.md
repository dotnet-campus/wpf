# 问题处理线 B：Desktop Duplication 与历史 HW 有界裁决

## 结论

确认并复验两个 control 的测量夹具根因：`AcquireNextFrame` 成功不等于有桌面内容帧。旧包在 Present 之后新建 duplication，取得 `LastPresentTime=0 / AccumulatedFrames=0 / LastMouseUpdateTime>0` 的鼠标-only 首帧，将零内容当作呈现结果。当前提交感知捕获先创建 duplication、释放初始帧，再执行真实 Clear/Present，并只接受非零且不早于提交起点的 LastPresentTime；相同 CopyResource/Map/RowPitch 读回链返回 FF123456。原两个像素断言未改，未更换捕获方式。

最终源码 x64 Release 42/42；x86 Release 隔离捕获 6/6。历史32项不能整体关闭：x64 全部候选用例 Passed，但官方同场景对照未完成，内部 backend 未证明；x86 扩展历史组未完成。捕获所见 adapter 为 **Microsoft Basic Render Driver**，绝不记作物理 GPU 或候选内部硬件 shader/cache 路径验收。

## 变更与所有权

本包写入：

- `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.DesktopDuplication.cs`：增加 DXGI adapter description/LUID、output 名称/bounds/rotation/monitor 诊断；核对窗口 monitor 和 identity rotation；提取帧选择谓词，明确拒绝 LastPresentTime=0，增加四项边界回归（pointer-only、旧帧、等于提交点、新帧）。
- `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.CaptureHistoricalControls.cs`：本包独有；五个候选历史 scene 身份回归，官方 geometry/sampler 各一个独立可筛选入口。复用既有场景及断言，不以候选算法构造官方预期。
- 本报告。执行摘要另外写 ignored artifacts，非另一份报告。

首次读取工作区时，DesktopDuplication.cs 和 PresentContract.cs 已存在提交回调、首帧 drain、有界新鲜度等待、PPM 保存及提交日志（不同于交接报告描述的旧编译版本）。这些预存修改没有被本包覆盖或冒认是本包编写；本包在此基础上验证并补强。`PresentContract.cs` 本包未写入；提交感知逻辑与其调用必须一并保留。没有修改生产、项目文件、InternalContextLayout、Realizer、FLOAT、其他代理状态序列或主交接文档；没有重发生产 DLL、清理共享目录、增加项目/Host/原生探针/脚本。

## 实际诊断与因果证据

证据公共根（以下目录均相对此根）：`artifacts/sandbox-readback/replacement-supervised-capture/`。

1. `preexisting-x64/TestResults/capture-preexisting-x64.trx`：实际执行集成旧包，2 executed / 0 passed / 2 failed。swapEffect1/3：staging/front FF123456，屏幕00563412，duplication00000000；frame lastPresent0、accumulated0、protectedMasked0，鼠标时间戳非零。失败不经候选绘制。
2. `fresh-x64/TestResults/capture-fresh-x64.trx`：提交感知诊断实际2/2。仍先收到 pointer-only 初始帧，提交后 LastPresentTime 变为非零，再经原读回链获取正确像素。
3. 最终 `final-v2-x64/TestResults/capture-final-x64.trx` 534–581行：swap1 initial lastPresent0/accumulated0；submissionStart32021971739104；内容 lastPresent32021972111668、accumulated3；swap3 submissionStart32021973608897、内容 lastPresent32021974303852。两个内容帧均 FF123456。
4. `capture-isolated-x86/TestResults/capture-isolated-x86.trx`：swap1 submissionStart32021661935191 → lastPresent32021662393697；swap3 32021663980301 → 32021664427913；同样得到 FF123456。

两架构：output `\\.\DISPLAY1`，bounds(0,0,1920,1080)，attached1，rotation1(identity)，output/window HMONITOR均10001；screen(56,48)，local(56,48)，纹理1920×1080、BGRA87、RowPitch7680/DepthPitch8294400、featureLevel B000、protectedMasked0。故本次不是 output/坐标/rotation 错配，也没有保护遮蔽证据；不适用于所有多显示器/旋转布局。保留 adapter0/output0，遇不匹配直接失败，没有擅改成任意选择策略。新旧读取链相同且新帧能正确读回，不支持 Map/CopyResource 根因；未加任意 sleep/Flush 追 Green。

adapter description=Microsoft Basic Render Driver，vendor1414/device008C，LUID00000000:00004E71。此为 duplication 设备的实际身份，不代表候选内部 backend。不能仅凭 HAL 参数、成功创建 pixel shader 或非空像素认定物理 HW。

## 构建与真实运行

使用专用发布接口，最终两架构均发布成功。项目 net10.0-windows、Nullable enabled，根 SDK10.0.107。参数 Release、win-x86/win-x64、自包含、RuntimeFrameworkVersion10.0.9、ComAcceptanceSandbox=true、对应 AotRuntimeIdentifier、DebugType=None、DebugSymbols=false、ProduceReferenceAssembly=false。唯一输出 `artifacts/publish/replacement-supervised-capture-win-<arch>/` 和 `artifacts/staging/replacement-supervised-capture-win-<arch>/`。没有生产发布。

全部实际执行标准现有 `WpfGfxShape.ComAcceptance.exe`，MSTest4.0.1 / .NET10.0.9；每次完整回收独立任务目录，不执行终端或脚本。构建工具返回成功；执行接口没有暴露数值退出码，不能补造。完成运行以 TRX 为准。

|运行目录|结果|边界|
|---|---|---|
|preexisting-x64|2执行、2失败|旧包实证，不是最终回归|
|fresh-x64|2/2|首次新鲜帧验证|
|final-x64|37/37|加历史对照前；历史32+四项谓词+普通官方control|
|controls-final-x64|未完成，无TRX|初版混合官方对照到首个官方geometry时中断，不计任何完整通过|
|final-x86|未完成，无TRX|扩展历史组，不能计失败数或成功率|
|capture-isolated-x86|6/6，failed/error/timeout/aborted/notExecuted均0|最终两control+四谓词；TRX113行|
|final-v2-x64|42/42，failed/error/timeout/aborted/notExecuted均0|最终历史32+四谓词+普通官方control+五候选身份场景；TRX851行|
|official-sampler-x64|未完成，无TRX|最终源码独立官方sampler进程，身份已落盘|
|official-geometry-x64|未完成，无TRX|最终源码独立官方geometry进程，身份已落盘|
|historical-public-x86|未完成，无TRX|进一步切分x86 public/shader组仍无完整结果|

未完成进程的接口只返回启动/运行片段和回收目录，没有异常类型、栈或数值退出码；称“进程中断/未完成”而非猜测 AccessViolation、超时或环境错误。没有为了制造Green删除/skip官方入口。官方两个入口需按本报告单独 filter 执行；目前不能安全宣称它们全套执行会完成。

## 实际 identity 与原始证据路径

- 最终x64任务 `14a4c83ae05a4e68b639fc5abfba4d43`，X64/pointer8，候选实际 `C:\CodingAgentSandbox\Tasks\14a4c83ae05a4e68b639fc5abfba4d43\Native\wpfgfx_cor3.dll`，SHA `2BFC4AC24A83EBAF45A4B375CDC5FE1F9C90BD9370D73BD590C259091C5BF58F`。`final-v2-x64/Results/aot-identity.json` 及五份 `hw-module-Native-*.json`；实际fileVersion1.0.0.0、productVersion1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685。最后测试程序集SHA `5FA826386266E43D2518F19E94E34CCCEB8928B94A59ADA0E64EB1621448621E`。
- 最终x86捕获任务 `4efe303ece35405aa92d57c9d07d6cb0`，X86/pointer4，候选实际同任务Native目录，SHA `747F307104A9E51E9689307BD85CAABC60EB60294F7F307E21CF687F77E7A142`，见 `capture-isolated-x86/Results/aot-identity.json`。不沿用集成x86 SHA、不与历史full拼接；本包未追SHA来源。
- 官方geometry `official-geometry-x64/Results/hw-module-2c6b6cba9e1643d9ba72cc382cc2809c-9bf42fb332244fa0b53e02390167474b.json`；官方sampler `official-sampler-x64/Results/hw-module-1df388c7f67e4ef49a9ba2d00b9a33df-57c6db9c3f034efca1dc8e3c3519c25f.json`。均为实际任务根wpfgfx_cor3.dll（非Native候选），SHA `098D9CEFACA7B1E36460EB28F2725D7AFDB8E93855C29F8DD3627A1FB692AE8E`，fileVersion10,0,926,27113、productVersion10.0.9-servicing.26270.113、commit901ca941248413c79832d2fdbd709da0c4386353，X64/.NET10.0.9，测试程序集SHA与最终x64一致。
- 原始完整桌面RGB PPM：`final-v2-x64/Results/desktop-6052f8dcea1b4a47917d5afa5d7c79df.ppm`、`desktop-a7b2dc40a8d14b388cb8303b18334477.ppm`；x86图像在 `capture-isolated-x86/Results/desktop-*.ppm`。PPM为原映射逐行写出，未改golden。像素核查来自TRX实际采样，未声称人工检查全图。
- 摘要JSON：公共根 `execution-summary.json`；TRX同时保存原始stdout帧/窗口/像素日志。无TRX的中断仅有identity JSON，不伪造原始终止日志。

## 历史32项有界裁决（编号沿用上包）

|编号|最终x64实际结果|裁决|
|---|---|---|
|1–2 CacheGeometry|两组Passed，另两组带实际候选模块identity回归Passed；绿/蓝更新原断言|已尝试同场景官方geometry，但官方未完成；不闭合官方差分/backend|
|3–4 ConfigurationRecovery|两组Passed|配置拒绝重试不是实际device-loss；官方与backend仍缺|
|5–6 WicCacheReplacement|两组Passed|更新像素原断言保留，官方WIC同序列/backend未补|
|7–11 LazyWicReentry|五组Passed|不是官方重入/流寿命差分闭合|
|12–13 WicWriteLockRetry|两组Passed|官方同锁冲突/提交帧/重试证据缺|
|14–15 TwoDeviceCache|两组Passed|官方双设备键/释放顺序和内部backend缺|
|16–23 PublicHwnd|八组Passed|普通官方实色control不是同D3DImage/共享surface/callback/shader场景；未闭|
|24 ShaderGenerationReplacement|Passed|缺官方同bytecode代替换及内部shader backend|
|25 MalformedShaderFallback|Passed|fallback合同通过不能记HWshader闭合|
|26 ChannelDeviceCloseRecreate|Passed|重建不等于真实device-loss，官方同序列缺|
|27 NativeGpuTopmost|Passed|仅系统D3D9测量control，不代表候选HW内容|
|28–29 NativeGpuFrontBuffer|两组Passed，x86也Passed|首帧测量夹具异常已闭；不推导物理HW兼容|
|30–32 SecondSampler|三组Passed，另三组带候选identity回归Passed|官方sampler独立运行未完成；非空及更新不替代官方/backend|

x86历史扩展两次未完成，不外推x64结论；当前只有捕获六项正式完成证据。x64五项新增候选身份增强了#1–2/#30–32可追溯性，不是新独立官方对照通过。

## 未闭与下一包

1. 已关闭：本场景鼠标-only首帧误用及两个control的零像素异常；双架构原断言实跑、新鲜度负控、同读回链原始内容均有证据。未确认任何候选生产缺陷。
2. 首要阻塞：共享 `D3DImageTests.VisualCache.cs` 的 `RunVisualCacheConsumer(... official:true)` 在官方geometry和sampler均无法完成；x86历史组也不完成。需要下一包逐调用持久化阶段/HRESULT及进程终止诊断，先审计合法wire/ABI及官方同场景夹具。没有足够证据指定生产根因；本包不改共享consumer或跨域ABI。
3. 真实HW：需要非Basic Render Driver设备以及逐场景候选内部backend证据；不能以当前软件adapter模拟HW成功，也不能把尚未诊断的官方中断笼统归环境。
4. 尚需官方各cache/WIC/锁/双设备/callback/shader同参数独立完成、同场景官方重复稳定性及内容零阈值比较；实际device-loss另有责任包，本包不借关闭/重建冒充恢复。
5. 多显示器/rotation本包仅验证当前identity布局，未实现通用output枚举或旋转坐标变换；不改阈值、golden、skip。整体完整替换与历史32项HW独立验收仍未完成。
