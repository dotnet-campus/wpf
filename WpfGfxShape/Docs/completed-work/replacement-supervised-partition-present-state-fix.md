# IH：合法窗口 Present 状态最小闭包

## IJ 独立最终裁决：有限保留通过，完整替换未完成

保留 IH 的最小 NoDevice→Normal 实现及 IA 初态、ID 全监听注册快照广播、既有 COM/fill。核心有限门满足，没有读到要求撤回的新语义错误；不因 II 恢复测试 guard、旧报告未收尾或未完成全文件 manifest 审计撤回真实通过。下方 IH/II 进行中及“尚未实施/修改0”均为历史，不是当前裁决。本轮仅四文档读写；生产/测试修改、build、publish、沙箱、新 Host/工程及脚本均0，不重复已完成任务。

### 原件及身份（IJ 独立读取范围）

根路径相对 WpfGfxShape。核心根 `artifacts/ih-partition-present/readback/`，每格结果 `Results/ih-{role}-{arch}/`；已亲读 hb-consumer.jsonl 的实际身份、初态/转换/后快照、三色事件与清理关键段，精确 `TestResults/child.trx` 的用例和 Counters、supervision.json 退出段。没有把目录索引漏出的 TRX 判作缺失。IF/IG 历史见同目录报告 replacement-supervised-partition-render-state.md；本轮读其核心证据及权威定义节，不冒称重新审完所有历史原件。

|核心格|task|TRX executed/passed；监督 Exit|actual Native SHA256|
|---|---|---|---|
|candidate x64|e61e5fa0956c43308e90470d74e6da45|1/1；0|EE3F1DE1BC3192766ABE5ACBC94B2548B9EEDD5107B5E7BA8BF8F465DB148189|
|official x86|9e6286ff04614adf805aa360ff393baf|1/1；0|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|
|candidate x86|aeda7991921e42cd9cc186167505cc1a|1/1；0|01CF2206C3AE4327B6057A18AFA084629B49409AC812CFBD099E22CB66105ABC|

三监督均 Reason=exited、HasExited/OutputCollected=true、TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut=false、SupervisorError=null。TRX 均 failed/error/timeout/aborted/notExecuted=0。核心 test x64 SHA `856010B12EB5783FEA73261A6D4899F010475B9E499D43E07A59B2AF3C8A333C`、MVID `24951844-b76a-4e4c-874d-8dd56c6a600e`；x86 两侧 SHA `1FF7E338B65AAC126BF18290AC2D364CB90AD7B176C799D3CD103AD9097C0F45`、MVID `3995b811-3ef4-4f95-a411-abdf4fe10d55`。这不是 guard 恢复后测试身份。

actual 由业务 identity 指向 task 下实际模块：候选 `Native/wpfgfx_cor3.dll`，官方 task 根 `wpfgfx_cor3.dll`，不以官方进程中 prerequisite/aot-identity 所记候选身份替代官方实际模块。实际 .NET10.0.13、X64/8 或 X86/4；WPF10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；候选版本1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a。官方 x64 AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1 来自既有 IF 官方基线及本轮 II 官方 role 的 actual 记录，不是新增 IH 官方 x64 运行。只可称历史 x64 旧红、双架构新绿；x86旧红本轮未核，不称新旧全矩阵。

### 真实状态与内容验收

三核心格均：A 初态 type5 old/new=1/1一条；frame0独立窗口 consumer 红色255确认后 drain 真1/0一条；B由A作source共享partition后，B注册向A/B各0/0、A disable后重注册向A/B各0/0，共六条完整64字节消息，type在0、old/new在8/12，其余零。frame1/2 drain各0，verdict RealTransitions=1/Current=0。蓝16711680、绿65280实际窗口像素更新，三帧 Signalled=true；不是producer回读、空内容或只看Passed。

关键尾部亲读：资源 unregister/delete、delete commit/flush、destroy、disconnect、partition deinitialize均成功，Bitmap/Surface/Device/Factory refs全部0，window destroy1、cleanup complete。consumer HardwareConsumerProven=false、ActualBackend未独立观测；不称真实HW覆盖。消息仅限定转换发生于初态采样后至frame0内容后drain前，不断言必是红帧那次Present；setup/initial invalidate未分点。B转换时未订阅，动态同时向多个listener投递未实证。

### II 最终测试身份与32兄弟（不是32项默认产品闭包）

根 `artifacts/ii-present-state-acceptance/`：独立输入 `x64|x86/com-acceptance-package/`，测试发布目录 `publish/x64|x86/`，回收 `readback/x64|x86/`，输入与结果路径分离。已读输入 package-files.txt开头及aot.sha256、回收 supervision/actual业务/aot-identity，确认关键身份对应；II记录恢复guard后重新发布测试、Native未重发，与核心及兄弟actual Native相同相符。未独立重算包内每个文件hash、未做全文件manifest集合/复制来源逐项审计，不冒全审，也不把它自设为撤回有限修复门。

|兄弟架构|task；主child PID|TRX executed/passed；监督 Exit|最终 test SHA256 / MVID|
|---|---|---|---|
|x64|62eb98f92bdc4bf6a36e7142c475c48c；8708|32/32；0|B84B35125789717EB88E32294944210B91D5E9CDFB01FE9862E7E3C6E88210A7 / 090b609d-35ed-4eba-a2be-7b5542a219b4|
|x86|98c05771852d46a097c6ce9b12d4eb85；6748|32/32；0|400714294930D5322C96A2C8835F9AA52E499A37EB7AFD21D74A203A0F9F7457 / e8ca1b93-a055-4b70-b58f-7e713482c081|

两精确 Results/ii-siblings-{arch}/TestResults/child.trx Counters均32执行32通过、failed/error/timeout/aborted/notExecuted=0；两监督均exited/Exit0，收集完成，无timeout/cancel/kill/drain失败或SupervisorError。actual Native由HW candidate业务及HU False-6业务核到，分别EE3F1DE1…/01CF2206…，与输入aot.sha256及核心相同；官方HW角色actual为根DLL AD1C9DBB…/5EBD18E0…，.NET/WPF同上述版本。aot-identity是末次HU角色记录，不冒充所有官方角色身份。来源核到实际部署模块路径/版本/SHA及关键包声明，未独立核到安装源复制链全部文件。

每架构32组成：3通知/锁、2 HY、2 ListenerBroadcast、12候选排空、6官方排空、1有限HW注销group、6 HU外部流关闭。含显式探索角色，不是32完整默认产品回归，更不是full；HY未渲染初态、静态listener广播不替代真实动态门。

必要业务亲核：双架构HW candidate dispatch-enter→unregister-return hr0→callbackexit released=true，retained队列1；禁用后drain0；windowless新backchannel仍有1/1快照；最终drain0/owner barrier、destroy/disconnect/deinitialize hr0、window-dispose、ownerJoined=true/callbacks1/windowError=null。官方/候选四角色exit.json均exited/Exit0，四角色身份及清理尾已读。这里HW只是历史组名，业务scope明确nonHW class5 message-only HWND，无HWtarget/SyncModeStatus。

HU双架构candidate False-6业务207行全部亲读：closeReturn时disposals0→externalRelease前0→callback.enter→callback.exit/后disposals1→sibling destroy→deinit后等待均finished且仍1；callbackExited/closeReturned=1、externalReleaseReferences0，Disconnect.ReferenceOnly/Destroy.Drain/Destroy.Sibling/Deinitialize均0。各role TRX1/1 Passed、Exit0。两架构False/True-6/7/8合计12个HU退出记录均exited/Exit0（包含上述两个False-6）；其余HU业务未逐项全文核。其余兄弟以父TRX及监督为计数证据，不继承“所有业务全审”。

### 五生产文件受改段与边界

1. `Abi/HardwareRenderTargetExports.cs:163-195`：out默认false；绘制/CopyPixels成功不直接认证。软件分支经过Presenter.Present，精确hr0且非零宽高才合格；这次传完整非空rect、无dirty列表，已亲读SoftwareGdiPresenter:201-267，其内部资源检查/裁剪/取得窗口DC/CopyToDib/BitBlt，失败返回负HR；当前调用不是空rect/空dirty成功假Present。D3D分支由Renderer.Present(out)资格独立传回，PerformanceInstrumentation不是资格来源。
2. `Core/Direct3D9SurfaceRenderTarget.cs:3735-3850`：合格值来自真实_device.Present的PresentProcessed && HResult==0；disabled/无swapchain/异常前置均false。并亲读既有Direct3D9Device的PresentProcessed默认精确HResult0及Occluded处理：归一返回0同时显式PresentProcessed=false，不可误置Normal。模式变化/失败不靠hr>=0认证。该窄成功门不复制完整窗口错误归一、device-loss/cache副作用。
3. `Core/GeneratedVisualTargetResources.cs:327-354`：仅有效enabled/非空target/非零尺寸进入，保持root和target引用；HW包装分支还要求资源类型HwndRenderTarget；generic/WIC/no-target成功不认证。
4. `Core/GeneratedTargetRegistry.cs:24-43`：snapshot按既有list逐target顺序持引用；逐target合格且result0立即回传，负result首失败停止，finally逆序release；不自造any/all/max聚合，前target真实成功不因后失败抹掉。
5. `Core/SameThreadChannel.cs:29-52,116-145`：仅Compose接回调，仅精确0且旧NoDevice才赋Normal；赋字段后type5 old/new广播全部_notifications，RegisterNotifications快照读同一_deviceState，保留IA/ID。没有扩loss/occluded/最后target删除算法。

附核PartitionScheduler:118-180：队列_gate只保护取任务，释放后在同一scheduler线程执行CommitSubmitted/Compose或scheduled.Compose；不是持_gate整段渲染。异步命令注册与状态更新在该串行上下文，消息使用既有PostMessage队列；不因此宣称全线程安全、HashSet官方数组枚举顺序等价或多监听同时变化已测。UceChannelExports:70-99同步SameThreadPresent仍直接Targets.Render(signalPresent:false)、未传状态回调；同步状态合同不在本次成功门内，不能冒称已接入。

### 最终保留范围及下一唯一门

IF_PARTITION_RENDER_STATE条件保护已恢复、原动态断言仍在，显式核心三格的真实通过不撤销。默认独立动态回归尚缺：本次32兄弟及默认构建不能证明默认发现/执行真实转换。下一唯一生产回归门：在现有正式ComAcceptance内把合法异步HWND真实内容→1/1→1/0→后0/0合同纳入默认独立动态回归，绑定当前双架构Native，继续保留显式探索隔离；不扩第三Host、不重跑已完成核心任务冒新证据。本轮不实施该下一门。同步状态合同单列后续，不擅自接回调扩大修复。

真实HW target、非零SyncModeStatus、Available、长期资源、loss恢复、mediaresources、full四格、x64Event0全部开放；完整替换未完成。状态来源及适用范围仅已验收的合法asyncCompose成功窗口Present最小NoDevice→Normal，不称全状态机。II所记默认build数字保留为历史记录，本轮未新build也未继承完整构建日志审计。

最后补记（IJ）：最终有限保留裁决已落本文及三共享最新节，历史原样保留；完成写后回读以实际读回记录为准，不以“待裁决/初稿/已跑未读”交付。

## II 接续进行中（纠正 IH 初稿时效）

II 已恢复 IfPartitionRenderStateTests.cs 的 IF_PARTITION_RENDER_STATE 条件保护，未改原断言。IH 初稿“生产修改0/沙箱task0”不是当前事实：当前已有五生产文件修改，以及 candidate-x64 e61e5fa0956c43308e90470d74e6da45、official-x86 9e6286ff04614adf805aa360ff393baf、candidate-x86 aeda7991921e42cd9cc186167505cc1a 三格已执行产物。II 尚未亲核原件，不把已执行等同已验收；不因初稿未收尾撤回补丁。恢复 guard 后测试源码身份已变，旧 test 包不代表最终源码。正在逐格核身份、完整消息、consumer、监督、清理与权威状态链；最终裁决尚未作出。

## II 阶段补记：核心已核，兄弟已实跑

三格精确 TestResults/child.trx 均1/1 Passed、监督Exit0，无timeout/kill/drain失败；全部核心状态消息、三色consumer、事件及refs/cleanup已亲核，确为1/1→1/0→四次0/0，不只是采集Passed。x86实际候选01CF2206…，test1FF7E338…，非旧SHA。II新独立包双架构兄弟task62eb98f92bdc4bf6a36e7142c475c48c / 98c05771852d46a097c6ce9b12d4eb85均32/32 Passed、监督Exit0；恢复guard后的新测试身份已记录，Native未重发。默认正常分析器Release工程build Exit0/108warnings/0errors，solution build Exit0/3664warnings/0errors。继续补核必要业务及包来源，随后提交有限保留终态，不追加核心重跑。

## IH 历史进行中（以下保留，不代表当前计数）

已读执行规则、完整替换方案、最新 handoff 与 IG 独立核证。保留 IA 初态、ID 广播及 COM/fill。历史 x64 official ea7ec0df7fbe4e12ada417e42bc04768 / candidate 833a25829cf442f7856693d159786a6c 不重跑旧红，不计为 IH 新运行。

## 生产改动候选清单与语义（尚未实施）

1. HardwareRenderTargetExports.Render：仅真实软件 Presenter.Present / D3D Renderer.Present 的合格窗口结果分离于绘制 HRESULT。
2. GeneratedTargetResource / GeneratedTargetRegistry：窄转发逐 HWND target 结果，保留迭代顺序、首失败停止及同步路径。
3. SameThreadPartition：唯一拥有状态；同 scheduler 内仅认证精确 S_OK 使 NoDevice→Normal，更新后广播 old/new，注册快照读取同字段。

不得由 Render 成功、hr>=0、event 或 HasPresentableTargets 设置 Normal；不实现正 occlusion/loss/cache/删除最后 target 等未覆盖行为。只有双架构新产物核心门及必要兄弟通过才保留新增行为，否则撤回。

## 当前实测

IH 生产修改0、测试修改0、构建0、发布0、沙箱task0。正在阅读真实调用链和 IF 专属回归；不声明已确定官方转换发生于 setup 还是 frame0。
