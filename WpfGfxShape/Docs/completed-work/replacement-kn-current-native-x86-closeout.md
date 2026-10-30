# 当前候选 x86 KN baseline/resize 生命周期迁移收尾

## 裁决

**当前 x86 的两个既存 KN 生命周期参数体有限通过，收尾完成，停止执行。** 先纯发现，明确真实参数 UID，再独立 false，亲读其全部原件通过后才独立 true；无重试、无方法 FQN 执行整组。沙箱3/3（发现1/测试体2），源码/项目/包修改、build、COM publish、Native publish、终端/脚本、再委派、official、x64、full、显示及其他回归均0。复用 `WpfGfxShape/artifacts/async-shared-ownership-x86-r1/clean` 正式 `WpfGfxShape.ComAcceptance.exe`。

范围仅为公共target46/producer创建、尺寸更新路径和正常销毁的现有合同；未倒签旧KN17全部迁移。完整替换Open、整体基线不升级、显示冻结，所有历史独立保留。成功后的唯一建议为另授权x64同两参数及发现预算，不自动执行。

## 执行前合同与实际调用

执行前已落盘预定义快照：`WpfGfxShape/artifacts/kn-current-native-x86-r1/predefined-snapshot.md`，包含先验身份、验收、停止条件和证据分层。准确调用及完整工具可见原返回分别为同目录 `{discovery,baseline,resize}-call.json`、`{discovery,baseline,resize}-tool-return.json`；乱码保持原样，没有将中文解码猜测成新原件。`budget-status.md`记录预算状态。

准确方法：`WpfGfxShape.ComAcceptance.D3DImageTests.WhenKnTargetResizesInPlaceThenProducerLifetimeSurvives(bool resize)`。发现使用 `--list-tests --filter FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenKnTargetResizesInPlaceThenProducerLifetimeSurvives`；两体仅 `--filter-uid` 真实节点。各次独立Results及Diagnostic，均同步诊断/no-progress、outputRelativePath=null完整回收；发现外120秒，两体各外180秒。没有supervise-exit、没有猜Name/Id/UID。

|顺序|真实任务ID|真实TestNodeUid / 参数|结果|完整回收（相对工作区）|
|---|---|---|---|---|
|1发现|779cf22aa87f4ec4a4a16e6e2b0db44d|恰两System.Boolean Discovered节点；false=10baaeb0-3ba8-84e6-8830-7058e6fca4ac；true=1f3e7305-18e0-806d-82fd-affa104bcbb4|体0；会话0|WpfGfxShape/artifacts/kn-current-native-x86-r1/readback/discovery|
|2 baseline|23a5b2c6805a411a825bb1afb0aba44d|10baaeb0-3ba8-84e6-8830-7058e6fca4ac / false|TRX total/executed/passed=1/1/1；同UID Passed terminal；会话0|WpfGfxShape/artifacts/kn-current-native-x86-r1/readback/baseline|
|3 resize|9d53b485c4dd4f5a85a39af1c8805c3b|1f3e7305-18e0-806d-82fd-affa104bcbb4 / true|TRX total/executed/passed=1/1/1；同UID Passed terminal；会话0|WpfGfxShape/artifacts/kn-current-native-x86-r1/readback/resize|

前代理记录：每任务返回后即时保存准确调用/完整返回及回收路径，再亲读原件才继续；本接续不倒签执行顺序或即时归档行为。本次文档收尾亲读执行前predefined-snapshot、budget-status、baseline-verdict及三任务各call/tool-return原件；发现diag共51行，false/true各child.trx35行、diag75行、lifetime.json320行、业务identity.json24行和aot-identity.json1行均已亲读。报告首次读取40行处及两diag末段的截断已分段补齐，未只看摘要。discoverer提到1822为程序集发现总数，不是本次执行分母；实际筛选发现恰2参数节点、每体只执行1。本接续仅改文档，新增执行0；本门历史沙箱3=发现1+测试体2，其他独立账不合并。

## 原始证据索引

所有下列路径均在上表各自readback任务内，不把回收任务上传成clean：

- 发现：`Results/kn-current-discovery-x86/Diagnostic/log_261028024252409.diag`；末会话 `686b48ac-4ebb-45a3-b50e-0e26e99bddbf` exit0。纯发现未运行业务，没有TRX或业务identity，不补造。
- false：`Results/kn-current-baseline-x86/child.trx`、`Results/kn-current-baseline-x86/Diagnostic/log_261028024357795.diag`、`Results/kn-inplace-lifetime/baseline/{identity,lifetime}.json`、`Results/aot-identity.json`。MTP会话 `5047ea59-bccb-4d5c-866c-976cdbab1b82`。
- true：`Results/kn-current-resize-x86/child.trx`、`Results/kn-current-resize-x86/Diagnostic/log_261028024551992.diag`、`Results/kn-inplace-lifetime/resize/{identity,lifetime}.json`、`Results/aot-identity.json`。MTP会话 `355ac1e3-fa62-423f-a0db-8e4d50f49042`。

false的TRX runId `6626dd82-ea48-4be4-8dce-c8e8901ec138`、executionId `fff71cf5-8870-4d5e-ac9a-99fdcc2475a5`；true的runId `e13a0e5a-aa71-4af2-9e2d-592bcad1262b`、executionId `abb9cf16-6498-48bc-b656-529463b65070`。各TRX testId恰等其MTP TestNodeUid。业务identity不输出独立业务UID，不发明一个；aot-identity的Arguments记录相应filter-uid，与Resize布尔值、任务路径和同UID terminal交叉绑定。不要把runId/executionId/窗口或COM指针叫业务UID。

## 实际身份分层

执行前只读clean的aot.sha256清单值，不声称重新散列clean二进制。实际两任务另有测试在NativeLibrary.Load之后，按handle匹配ProcessModule.BaseAddress获得ActualNative路径并散列该文件的业务identity，不以aot文件预检替代实际加载。没有输出handle数值或GetModuleFileName调用证明，也不声称内存镜像hash。

- 两体实际模块路径分别为 `C:\CodingAgentSandbox\Tasks\23a5b2c6805a411a825bb1afb0aba44d\Native\wpfgfx_cor3.dll` 与 `C:\CodingAgentSandbox\Tasks\9d53b485c4dd4f5a85a39af1c8805c3b\Native\wpfgfx_cor3.dll`。
- 两体actual SHA256：`15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`。
- Test SHA256：`D8A5F57FE4A61CC28459A89DCB50DC75E0ADE80E1D0AEDC35D7E18AC5069A7BB`；MVID `e207b681-4c15-453e-b0a4-a1690fa3db78`。
- native/test ProductVersion均 `1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a`。
- 两体实际runtime `.NET 10.0.13`、Architecture `X86`，aot-identity PointerSize=4。
- 各任务自带PresentationCore.dll SHA `D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D`，版本 `10.0.13-servicing.26504.104+4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。

## 逐体既存合同验收

|合同项|false baseline独立结果|true resize独立结果|
|---|---|---|
|正常初始化/连接/target|initialize、connect synchronous(源码connect1)、channel、resource46均S_OK；target handle1|同项各自S_OK；target handle1，非跨进程同对象|
|公共创建与提交|Flags12、DPI context -1、DPI1/1、Adapter0、32×16；send49及两次send51/close/commit/flush/same-thread compose各S_OK|相同创建，后续更新路径各S_OK|
|producer真实对象|HAL、surface、fill、interop create各S_OK；Producer188921856 / Surface75282656 / Bitmap43028120非零|各S_OK；Producer195999872 / Surface78760064 / Bitmap194947040非零|
|surface持有|caller1→interop后2→baseline设置后2|caller1→interop后2→resize设置后2|
|软件snapshot临时对象|前后S_OK/HasBitmap=true；两次临时Release0|前后S_OK/HasBitmap=true；两次临时Release0|
|cooperative|前0、后0|前0、后0；并未触发device-loss|
|正常设置通知|Notifications=[]，Invalid=false|Notifications=[]，Invalid=false|
|尺寸路径|Resize=false，不调用resize SetWindowPos；源码Settings32×16|Resize=true；现有源码Assert SetWindowPos(64,24)非零后Settings64×24，测试成功走完；日志只记send51而未输出更新尺寸/GetDesc，不能冒独立实际GetDesc尺寸证明|
|正常关闭管理结果|destroy/disconnect/deinitialize均0|destroy/disconnect/deinitialize均0|
|关闭callback|cleanup target callbacks恰1，Available0、Version701|cleanup target callbacks恰1，Available0、Version701|
|末真实释放|detach后bitmap/surface/device/factory Release0/0/0/0|detach后bitmap/surface/device/factory Release0/0/0/0|
|owner窗口/清理|窗口37291404；cleanup window destroyed及cleanup complete；TRX记录dedicated STA owner|窗口22348790；cleanup window destroyed及cleanup complete；独立dedicated STA owner|

前代理报告已亲读原测试源码 `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.KjResizeLifetime.cs` 的KN分支与cleanup；本接续不读改源码，该源码行为说明为引用前代理，不冒本次独立源码复核。没有隐含取色前门或显示能力阻塞；未更改expected/阈值/等待/取色。正常设置不提前unavailable、销毁后通知与最终真实Release按各自日志核验，不混两体。原测试是合法同步公共连接路径，并非新增async管理全面证明。

## 未证明项与停止边界

- 当前产物业务日志没有真实GetDesc尺寸、GetDevice IUnknown同一性、target manager/device/backend诊断；这些独立观测未知。未启用默认关闭诊断符号，未重发Native，也不从历史KI_RESIZE_TRACE移绑。producer HAL与producer指针不证明target device或consumer HWbackend。
- 两体ConsumerPixelsObserved=false、DesktopCaptureExecuted=false。snapshot非空/refs不是shader像素、resize后新内容、HW、device-loss、媒体、长期资源趋势、合法随机差分或full验收。
- 工具只给合并可见文本及回收路径，未提供独立OS数字exit、监督文件或分离stdout/stderr。本门有正确单体TRX1/1/1、完整业务/identity、同UID Passed terminal及MTP会话exit0，按授权有限认证；不造监督Exit0，不声称stderr为空。
- 旧20B3/40D0 KN17、旧失败像素/真实loss未触发、direct/public/WGC/KS、所有旧full身份/分母和预算账全部保留，未相减或倒签。

## 恢复与下一步

本轮三任务均已回收与审阅，不存在待重跑或待补读任务。仅建议另授权当前x64 `async-shared-ownership-x64-r1/clean` 同样先发现→false→true（最多3任务、2体、首错停，build/publish/Native0），独立绑定54BC62/test4A835620/MVIDd41adfe8；本轮没有执行或预猜其UID。显示冻结、完整替换Open。更正：此前本段“已最小追加”共享文档的表述早于实际同步，不能作为完成依据。本接续已实际最小更新并逐份回读 `Docs/next-session-handoff.md`、`Docs/handoffs/current-work-item.md`、`Docs/handoffs/current-stable-baseline.md`、`Docs/replacement-issues-and-plan-status.md`、`Docs/final-acceptance-remaining-matrix.md`，并将 `artifacts/kn-current-native-x86-r1/budget-status.md` 从待回读改为真实终态。只新增当前x86 false/true两个生命周期切片，历史段保留，旧KN17不转绑新hash。
