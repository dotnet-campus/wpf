# 当前候选 x64 KN baseline/resize 生命周期迁移收尾

## 终态裁决

**当前 x64 false/true 两个既存生命周期参数体有限通过，执行与文档收尾完成，停止执行。** 复用 `WpfGfxShape/artifacts/async-shared-ownership-x64-r1/clean/WpfGfxShape.ComAcceptance.exe`；不是publish根或旧KN包。先独立纯发现，完整亲读后执行false；false全部原件亲读及裁决落盘后才执行true，随后全部亲读。无重试、无方法整组运行、无额外采样。

本轮沙箱3/3=发现1（体0）+false1+true1。源码/项目/包修改、build、publish、Native发布、其他测试、full、显示、official、x86重跑、终端/脚本、再委派全部0。x86两个生命周期绿按其独立报告保留；当前双架构这两个切片有限关闭，不倒签旧KN17或完整替换。

## 调用、任务与原件索引

准确方法 `WpfGfxShape.ComAcceptance.D3DImageTests.WhenKnTargetResizesInPlaceThenProducerLifetimeSurvives(bool resize)`。执行前范围/预期/停止条件见 `WpfGfxShape/artifacts/kn-current-native-x64-r1/predefined-snapshot.md`。同目录每任务 `{discovery,baseline,resize}-call.json` 为准确调用对象，`*-tool-return.json` 为即时完整工具可见原返回（保留原乱码），完整回收至 `readback/{discovery,baseline,resize}`，outputRelativePath=null。

发现参数为 `--list-tests --filter FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenKnTargetResizesInPlaceThenProducerLifetimeSurvives`；体只用 `--filter-uid` 本次真实UID，附 `--report-trx --report-trx-filename child.trx`。每次独立 `Results/kn-current-LABEL-x64`、`--diagnostic --diagnostic-synchronous-write --diagnostic-output-directory Results/kn-current-LABEL-x64/Diagnostic --no-progress`。发现外120秒、两体各180秒，未用supervise-exit/默认run/Name猜测。

|顺序|真实task|参数/真实TestNodeUid|裁决|
|---|---|---|---|
|发现|5ed32b0437794075a6c895d0c9eff84f|Boolean false=10baaeb0-3ba8-84e6-8830-7058e6fca4ac；true=1f3e7305-18e0-806d-82fd-affa104bcbb4|恰两个Discovered节点，体0，会话0|
|baseline|613fca78d4724ad092c8198899280dc3|false / 10baaeb0-3ba8-84e6-8830-7058e6fca4ac|TRX total/executed/passed=1/1/1，同UID Passed terminal、MTP会话0|
|resize|78bea96021874783af3c87ddc0f96a5c|true / 1f3e7305-18e0-806d-82fd-affa104bcbb4|TRX total/executed/passed=1/1/1，同UID Passed terminal、MTP会话0|

UID虽与x86相同，但由本次x64发现原件独立取得，未套旧UID。diag的1822是程序集发现总数，不是执行分母。发现原件为 `readback/discovery/Results/kn-current-discovery-x64/Diagnostic/log_261028025924412.diag`，共51行；MTP session `4d3c3a94-ef5b-4aa0-be3d-a8aca7859583`。纯发现无TRX/业务identity，不补造。

false原件：`readback/baseline/Results/kn-current-baseline-x64/{child.trx,Diagnostic/log_261028030021224.diag}`；业务 `Results/kn-inplace-lifetime/baseline/{identity,lifetime}.json`；初始化 `Results/aot-identity.json`。session `9fe07ce7-d570-4adb-bece-1189422b100e`；TRX runId `92124533-eac9-468e-bc8e-d8b8bb3cc067`，executionId `4b701a5e-21ba-4a33-a673-37069941b4aa`。

true原件：`readback/resize/Results/kn-current-resize-x64/{child.trx,Diagnostic/log_261028030148846.diag}`；业务 `Results/kn-inplace-lifetime/resize/{identity,lifetime}.json`；初始化 `Results/aot-identity.json`。session `e7973c9d-6a8d-480f-852f-1ecf337e4d8d`；TRX runId `6d451418-1512-499c-8af3-7f26332b4064`，executionId `7563e867-822a-4821-aa8e-a0408d0bd43f`。

两体各TRX35行、diag75行、identity24行、lifetime320行、aot identity1行已逐段完整亲读，diag末段和源码读取截断已补齐后才继续。false的执行前后裁决另见 `baseline-verdict.md`。TRX testId各等对应MTP UID；业务identity没有独立业务UID字段，以Resize布尔值、任务路径、aot Arguments中filter-uid及同UID terminal交叉绑定，不把runId/executionId/COM指针称业务UID。

## 身份分层

clean的 `aot.sha256` 仅只读清单，不声称执行前重新散列。初始化aot-identity另记录文件校验及pointer；业务identity则由源码NativeLibrary.Load之后按handle匹配ProcessModule.BaseAddress获取实际模块路径，再散列该文件；不是把预检当实际加载证明，不声称内存hash、GetModuleFileName或已输出handle数值。

- false实际模块 `C:\CodingAgentSandbox\Tasks\613fca78d4724ad092c8198899280dc3\Native\wpfgfx_cor3.dll`；true实际模块 `C:\CodingAgentSandbox\Tasks\78bea96021874783af3c87ddc0f96a5c\Native\wpfgfx_cor3.dll`。
- 两体actual SHA256均 `54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD`。
- Test SHA256均 `4A8356201F062B01161744C6B3BBF7D9AB7DB5909F6F12A9389806FFB6B6ED43`；MVID `d41adfe8-8904-4c9b-887d-8d1f5979ba46`；Native/Test ProductVersion均 `1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a`。
- 两体runtime `.NET 10.0.13`、Architecture `X64`、aot PointerSize8；WPF均各任务内PresentationCore.dll，SHA `0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31`，版本 `10.0.13-servicing.26504.104+4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。

## 逐体业务与清理

亲读现有 `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.KjResizeLifetime.cs` KN分支及finally，未改expected/符号/阈值。原路径为connect(1)同步公共连接，不冒async管理全面证明。

|合同|false baseline|true resize|
|---|---|---|
|公共创建|initialize/connect/channel/resource46各0，target1；Flags12、DPI context-1/DPI1/1、Adapter0、32×16|同项各自0，不是跨进程同对象|
|提交/producer|send49、两次send51、close/commit/flush/same-thread compose各0；HAL/surface/fill/interop各0|同项各自0；不是consumer像素|
|真实对象|Window24642910；Producer2078953313184；Surface2078896571544；Bitmap2078898097840|Window29689016；Producer2371102137024；Surface2371102579352；Bitmap2370956462016|
|refs|caller1→interop后2→设置后2|caller1→interop后2→设置后2|
|snapshot/cooperative|前后S_OK且HasBitmap=true，两个临时Release0；cooperative前后0|同项各自满足，无真实loss|
|正常设置callback|Notifications=[]、Invalid=false|Notifications=[]、Invalid=false|
|尺寸路径|Resize=false；源码Settings32×16，不调用resize SetWindowPos|Resize=true；现有源码断言SetWindowPos64×24非零后Settings64×24并走完；日志send51不输出GetDesc，非独立尺寸认证|
|正常关闭|destroy/disconnect/deinitialize全0|同项全0|
|关闭callback|恰一次Available0/Version701|恰一次Available0/Version701|
|真实末释放|detach；bitmap/surface/device/factory Release0/0/0/0|同项全0|
|owner/cleanup|TRX dedicated STA owner；window destroyed、cleanup complete|独立dedicated STA owner；window destroyed、cleanup complete|

## 未证边界、账本与下一主线

独立GetDesc目标尺寸、GetDevice IUnknown同一性、target manager/device/backend均未知；producer HAL不证明target HWbackend。未开guard/诊断符号、未重发或补采样。两体ConsumerPixelsObserved=false、DesktopCaptureExecuted=false；snapshot/refs不证明resize后像素/真实WPF内容/HW/loss/media/长期趋势/合法随机差分/full。

工具仅给合并可见返回及回收路径，无独立OS数值exit、监督或分离stderr；不声称监督Exit0或stderr空。准确单体TRX1/1/1、完整业务/identity、同UID Passed terminal及MTP会话exit0仅支持本授权有限认证。

x86两个生命周期绿、双架构两partition/ownership/AsyncDrain、direct/public/WGC/KS及旧20B3/40D0 KN17/IO/JK/KD/full、所有历史失败和预算独立保留，不转绑、不相减；包1–8未整体关闭。显示冻结、整体不升、完整替换Open。

唯一下一主线建议：按主要缺口审查§6，由能力方提供真实交互桌面/目标适配器及producer→consumer原始内容证据环境后，另授权既存候选 `WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive` 与official `WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive` 的真实HW resize内容合同；绑定实际身份、target backend/尺寸、前后非空新内容和原FF80断言及cleanup。无环境则外部Blocked，不再发明微ABI/前置探针，不自动执行。real-loss/media/趋势与最后双架构Release full继续各自Open。

本轮原件无未读尾部、无重跑/收尾待办；预算已写终态，五共享入口已同步并回读，历史段保留且由最新终态取代其旧排程。
