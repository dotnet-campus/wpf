# IF：真实 partition render state 官方基线与候选差分

## 当前阶段：进行中，合同调查

先核执行规则、完整方案、latest handoff、IC/ID（含 IE）/IA 与 HD/HB 原件及官方状态拥有者；不修改生产状态机，不复活 HY 映射。当前保留 IA NoDevice 初态与 ID 新 listener 全广播。

当前候选登记（未重新取证）：x64 `1AFFBE40F24D35DC317FE05D3D1F64C62AC136700F5D8D50B41A9B7252D68863`；x86 `C3D357371E070F3A81CCA3F955F50DC45A1A1F56A46680AE118930432C424AF7`。

## 执行门

仅正式 ComAcceptance 纯 C#，不新增 Host/工程，不使用终端或脚本。优先 x64 official 唯一显式 IF_PARTITION_RENDER_STATE；必须真实独立 consumer 非空更新及至少一条 old != new 消息，采集通过不等合同通过。官方内容前置失败或状态未变则停止 candidate/x86。最多四格各一次，明确部署缺项仅可纠正一次。

合法合同待源码确定：同 partition 注册 A → 初态 NoDevice 快照 → 合法 HWND target/有内容 frame → Present → drain type5 → 新增 B/重新注册快照 → 清理。不得硬编码 Normal 预期；不使用 invalidtarget/人工错误设 state，不将 WIC/no-target 成功、producer backbuffer 或 event signalled 当消费证据。

## 实证阶段：官方真实转换成立，候选同内容缺转换；停止 x86

新增独立 IF_PARTITION_RENDER_STATE 诊断及 HB 专属方法条件编译接点，生产修改0；正式自包含10.0.13发布1，沙箱实际task2。official ea7ec0df7fbe4e12ada417e42bc04768 / PID8812 / 监督Exit0；candidate 833a25829cf442f7856693d159786a6c / PID5204 / 监督Exit2。官方stdout1执行1通过，候选1执行1失败（待继续回读原件）。两监督无timeout/kill/drain失败。并非工具返回Exit，也无MTP父TRX。

官方初态1/1→独立consumer红帧后真实1/0→B注册A/B各0/0→A重新注册A/B各0/0，三色255/16711680/65280实际更新全部通过、四refs0、cleanup完成。候选已实际加载当前1AFF…；失败为消费后无old!=new，不作为部署错误、不重跑。候选具体逐阶段原件继续核读。x86本轮停止，无稳定双x64绿路径，不追加full。

实际test SHA D1AA4EA526A964873DA5DD849571285363A849BDB0827CA3AD9EC99D8D838E44 / MVID39cda456-05c3-4a54-8639-217c762e1b74；官方SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，WPF10.0.13/.NET10.0.13 X64/8。consumeractualbackend未知，不能称HW消费触发。官方stdout宣称绝对task路径child.trx生成，但整task回收尚未发现文件，必须披露未亲读，不能补造TRX。

## 开放范围

真实 HW target、非零 SyncModeStatus、Available、长期资源、loss、mediaresources、full 四格及 x64 Event0 仍开放；完整替换未完成。保留既有历史证据，不以本文登记替代实际验收。

## IG 独立终态：新动态差异成立，生产更新仍未实施

以上为 IF 阶段原文历史；其中“继续核读”“TRX未找到”不是当前终态。IG 亲读原件后确认：合法异步 HWND 内容消费序列中，官方真实 NoDevice→Normal 及渲染后同源快照成立，当前候选内容通过但状态不更新。该差异独立于已修的 IA 初态和 ID 新 listener 广播，不撤回这些修复。本轮只补本文与三共享文档，生产/测试源码修改0、构建0、发布0、沙箱运行0；IF 历史发布1/task2不计为IG新运行。x86未运行，不追加full，不新Host/项目，不使用终端或任何脚本。

### 原件定位与 TRX 补正

证据根为 `artifacts/if-partition-render/readback/`。下述 O 表示 `official-x64/Results/if-official-x64/`，C 表示 `candidate-x64/Results/if-candidate-x64/`。两侧 `hb-consumer.jsonl`（O 124行/C 122行）、全部业务阶段、全部 type5 完整64字节、事件/refs/cleanup、`supervision.json`、stdout/stderr及 child TRX 核心结果已亲读；`stages.jsonl`关键偏序及尾部已核，重复阶段另有完整业务JSON逐行对应。不将诊断diag全集、所有辅助图像/包全集合hash或他人历史full冒充本轮全审。

精确已读文件是 O `TestResults/child.trx` 与 C `TestResults/child.trx`，不是根目录child.trx。目录枚举和名称索引均漏出该子目录/文件，但直接读取成功；不得据索引缺项推断回收丢失。保留上文“整task回收尚未发现”的历史记录，现明确补正为“文件存在且已亲读”。官方TRX run `5492f2bb-8594-4ad8-a657-150463ada63e`，唯一 WhenIfOfficialHwndPresentsThenPartitionTransitionAndSnapshotsShareState Passed，Counters total/executed/passed=1/1/1；候选run `538e5d88-c61a-4a63-9a09-3961c36c4e93`，对应 Candidate 用例 Failed，total/executed/failed=1/1/1，无skip/timeout/aborted。候选ErrorInfo明确 `_transitions > 0` 失败，不是部署失败。

O监督task `ea7ec0df7fbe4e12ada417e42bc04768`，supervisor3168/child8812，Exit0；C task `833a25829cf442f7856693d159786a6c`，supervisor9144/child5204，Exit2。两侧Reason=exited、OutputCollected=true，TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut=false、SupervisorError=null；两stderr为空。stdout分别1执行1通过/1执行1失败，与TRX和监督一致；这里是监督记录的child退出码，不是沙箱传输工具退出码，也没有MTP父TRX。

### 实际身份与 consumer 边界

两侧test SHA `D1AA4EA526A964873DA5DD849571285363A849BDB0827CA3AD9EC99D8D838E44`，MVID `39cda456-05c3-4a54-8639-217c762e1b74`；实际.NET10.0.13/X64/指针8，WPF10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。业务identity由实际模块BaseAddress查询路径并算SHA，不能由配置替代。

O实际加载task根 `wpfgfx_cor3.dll` SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`；C实际加载task下 `Native/wpfgfx_cor3.dll` SHA `1AFFBE40F24D35DC317FE05D3D1F64C62AC136700F5D8D50B41A9B7252D68863`。两侧 `Results/aot-identity.json` 也亲读，但它只登记候选包；O的TRX/BJ prerequisite内“production=Native/1AFF…”同样不是O consumer所加载官方模块的身份，不混淆。

真实 HWND 包92字节、DPI偏移76/84、比例1/1、flags12、pixelFormat0、32×16、专属STA窗口/no background brush/WM_PAINT仅validate；O consumer HWND66455230，C525650，均不是BJ prerequisite窗口。setup/target/root命令及flush成功。三个独立consumer读数（window DC GetPixel client16,8，不是producer backbuffer）两侧都为255、16711680、65280，Expected=Actual且Accepted=true。每帧先producer更新/dirty及独立D3DImagePresent event，再consumer flush/DwmFlush/像素断言；三帧event均Signalled=true。event不是消费证明，像素才是有限内容证明。native fresh prerequisite的桌面duplication/BJ读数只是前置对照，不能代替HB consumer；更不能以Microsoft Basic Render Driver或命名IsHardware断言consumer backend。两业务均明确ProducerReadback=false、HardwareConsumerProven=false、ConsumerActualBackend=not independently observed。

### 原始消息、转换与 snapshot 证据层级

所有已记录消息均type5/64字节，uint32 type在0，old/new在8/12，其余字节全0；完整Hex已逐条核对原件。下表列的是所有实际type5，空项是有界drain至空，不是省略消息。A=channel0，B=channel1；B通过CreateChannel(connection,A)共享partition，不是独立partition。

|阶段/接收channel|O old/new及数量|C old/new及数量|
|---|---|---|
|初次注册A/A|1/1 ×1|1/1 ×1|
|content accepted frame0/A|1/0 ×1|0条|
|注册B/A广播|0/0 ×1|1/1 ×1|
|注册B/B快照|0/0 ×1|1/1 ×1|
|disable A/A|0条|0条|
|重新注册A/A|0/0 ×1|1/1 ×1|
|重新注册A/B广播|0/0 ×1|1/1 ×1|
|frame1/A、frame2/A|各0条|各0条|

O共6条，C共5条。1/1完整布局为 `05000000000000000100000001000000` 后接48个零字节；1/0为 `05000000000000000100000000000000` 后接48个零字节；0/0为 `05000000000000000000000000000000` 后接48个零字节。逐条原始128个十六进制字符序列保留在各hb-consumer.jsonl，不以格式缩写替代原件。

O verdict RealTransitions=1/Current=0；C=0/1。候选已向A/B广播，因此不是RegisterNotifications广播未实现；其快照自洽于过时字段，但不等价于官方渲染后0/0。测试AssertSnapshot以各侧_current检查内部同源，不直接强制候选为0；候选在这些断言全部通过后才在真实转换门失败，报告另以官方0/0和候选1/1建立外部差分。

实证层级：已证明“初态采样之后、frame0内容确认后drain之前发生官方1→0”，以及其后注册快照共享新状态；未证明状态变化恰由红帧那次Present触发。setup commit/flush、initial invalidate、后续Present之间未设消息检查点，也未采集真实consumer Present HRESULT；setup已可能触发合成，不进一步指定转换时刻或HW因果。B尚未在转换时登记，未实证同一动态转换同时投递A/B；只实证之后注册时全监听快照。

两侧listener disable/B destroy成功；detach→四资源4/3/2/1各Deleted1→delete close/commit/flush→A destroy→disconnect→partition deinitialize→refs(Bitmap/Surface/Device/Factory全部0)→window destroy1→cleanup complete。候选断言失败仍走完finally。此为本次有限COM返回值/清理证据，不等长期资源趋势或Available回调验收。

### 官方权威定义：不能把 Render 成功当 Normal

以下均为IG亲读工作区官方源码，不是从历史报告继承的全审；路径前缀 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/`。

- `rendertargetmanager.cpp:587-658`：Present按manager数组遍历，仅TYPE_HWNDRENDERTARGET调用pTarget->Present，紧接HandlePresentErrors(hr)，IFC失败则停止；非“所有target Render返回0→Normal”。
- `hwndtarget.cpp:263-300`：CSlaveHWndRenderTarget::Present在needsPresent/target/enabled/screenAccess/nonZombie条件下调用底层Present，Cleanup总经HandleWindowErrors；软件窗口目标同属HWND类型，不能只接D3D swapchain。
- `hwndtarget.cpp:1034-1133`：HandleWindowErrors可将部分底层失败转换为S_OK，DISPLAYSTATEINVALID保留上抛，正S_PRESENT_OCCLUDED触发InvalidateInternal。故manager的权威输入是窗口target处理后的HRESULT，不是任意原始D3D/BitBlt HR，更不是命令输入错误。
- `rendertargetmanager.cpp:406-452`：HandleRenderErrors仅FAILED时上报状态，成功Render不报告Normal；`468-519` HandlePresentErrors先ProcessRenderingStatus(RenderingStatusFromHr(hr))，再执行manager错误返回策略（如NOTAVAILABLE/DISPLAYSTATEINVALID可返回S_OK）。必须保留“状态输入HR”和“最终返回HR”区别。
- `composition.cpp:885-913`：仅精确S_OK→NORMAL；S_PRESENT_OCCLUDED→DEVICE_OCCLUDED（正成功码也不是Normal）；NO_HARDWARE_DEVICE、DISPLAYSTATEINVALID及default未知值→DEVICE_LOST。不要用hr>=0映射Normal；也不能把无效命令/ABI输入错误直接喂入这个渲染状态入口。
- `composition.cpp:2225-2271`：状态属于CComposition，old/new同源读取；NORMAL→Normal，LOST/RELEASED→NoDevice且NotifyDeviceLost，OCCLUDED→Occluded且ScheduleCompositionPass；赋字段后仅old!=new调用NotifyHelper。`2284-2296` NotifyHelper遍历全部已登记channel。`1241-1286` 新注册快照使用同一m_deviceState的old=new，再NotifyHelper。
- 多target不是自行求max、投票或any-success聚合：manager按既有顺序逐个HWND的经处理Present结果上报共享partition，保持失败停止和通知偏序；多个channel只是同partition的订阅集合。`ReleaseTargets:166-180` 与 `RemoveRenderTarget:909-925`有官方DEVICE_RELEASED调用，后者确有移除已登记最后target的规则；但不能直接套候选Release/Remove作补丁，须核生命周期等价及cache副作用，本轮1→0不扩到删除规则。

### 候选真实接点与唯一下一生产任务（设计，不是已实施）

根因静态闭合：`SameThreadChannel.cs:7-15`只有NoDevice初态，`Compose:28-50`调用Targets.Render，`RegisterNotifications:121-133`从同字段填payload；没有任何动态写入。`GeneratedTargetRegistry.cs:24-42`逐target Render汇总int、首负返回，缺Render/Present分阶段结果，不能从最终0推断有HWND Present。

真实路径为SameThreadPartition.Compose→GeneratedTargetRegistry.Render→GeneratedTargetResource.Render (`GeneratedVisualTargetResources.cs:327-345`)→HardwareRenderTargetExports.Render (`Abi/HardwareRenderTargetExports.cs:163-189`)。IsHardware:160仅识别本地vtable，owner.EnsurePolicy可切软件backend：SoftwareTarget分支先画图/Lock/CopyPixels，最后Presenter.Present；另分支先GeneratedHardwareVisualRenderer.Render，最后owner.Renderer.Present。`PerformanceInstrumentation.Presented:16-21`原样返回输入HR但只负责perf信号，不能作为partition状态所有者。D3DImage资源Present的event/producer/interop更新也不是窗口target Present接点。

D3D路径还经 `Direct3D9SurfaceRenderTarget.cs:3735-3850` Present/Core→device.Present→状态归一/HandlePresentFailure；软件路径 `Direct3D9SoftwareGdiPresenter.cs:201-267`则是真实窗口DC/BitBlt及返回HR。底层前置失败、无swapchain、禁渲染、空rect成功、是否执行底层Present均需区别；不能仅以名字或hr0认证路径。`UceChannelExports.cs:69-107` 同线程Present调用Targets.Render(signalPresent:false)，已有官方同步不进入HWND Present的注释/有限合同；signalPresent目前是perf通知开关，不可直接重新解释成“状态权威资格”。

唯一下一任务：在现有这条窗口target路径内补“经窗口策略处理的Present结果→所属partition共享状态→变更广播→后注册同源snapshot”的1→0最小闭包，不重写整个状态机，不恢复HY逐target映射。

1. 先用现有显式IF夹具作有界细化：初态drain后，在setup flush、initial invalidate flush及每次Present/consumer flush后分别记录并drain消息；候选同一路径记录partition/channel/target身份、compose序号、backend分支、Render HR、Present是否进入、底层HR与窗口处理后HR。现有1次官方/1次候选证据没有此关联，不能声称已经确定哪次Present触发。只补必要诊断，不添Host/项目，不做full；需能区分setup与frame0，而不是泛化采整个状态机。
2. 最小函数/传参边界：复用HardwareRenderTargetExports.Render，在两个真正窗口Present表达式处分离Render失败与Present结果，增加内部结果输出（如out bool hasAuthoritativePresentResult/out int presentResult；或小型内部结果值，不新增接口层）；保留原int返回/资源try-finally/perf信号。GeneratedTargetResource.Render增加/转发结果并保留ResourceType==HwndRenderTarget资格；generic/WIC/no-target/disabled早退不伪造Present。
3. GeneratedTargetRegistry.Render在每个合格target结果产生时立即转交partition处理（可由Compose传入内部Action<int>或等价窄回调，并显式传入异步Present上下文资格）；不能等全部targets成功后只报一个0，也不能把signalPresent=false简单等价于“没实际Present”。同步调用默认不新增状态通路，待其官方合同单独核实。依据官方逐target上报语义，保留顺序与首失败停止；不要把任意单target绘图/参数错误升级为全局loss。
4. SameThreadPartition仍唯一状态所有者，增加Normal=0及只处理已认证S_OK Present的窄入口；捕获old、更新new、构造64字节type5及向_notifications广播必须和RegisterNotifications/注销在同一串行上下文完成。异步当前PartitionScheduler.Run:112-180在同一worker执行CommitSubmitted、Compose、Destroy，_gate只护工作队列、不是状态锁；不得在设备callback线程直接改字段。确需外线程上报则先排队到同partition worker；若新增锁，snapshot/订阅集合/状态一起用同锁，避免只锁字段而乱序广播。已有消息队列锁不代替partition串行约束。
5. 初始保持NoDevice1；只在已认证的窗口Present S_OK权威结果上使1→0，old==new不发变化通知，新注册继续全监听snapshot0/0。已有IA初态、ID新增广播、重复enable/disable、COM/fill不动。不硬编码每Render成Normal、不以HasPresentableTargets/非空root/event完成代替Present。生产修改必须在关联采样通过后进行，本轮没有实施这些建议。

正occluded/无设备/未知HR的完整映射虽有官方源码，但本轮未触发；不能把这些状态伪装成Normal，也不宣称loss/recovery实现。将来实现需连同HandleWindowErrors/HandlePresentErrors策略、cache loss副作用、重调度、target释放规则及真实恢复内容一起闭合。若当前窗口策略后的真实HR无法取得，下一步止于上述单条路径的分阶段结果采样，不以任意单target HRESULT推测全局算法。

### default 安全与下一验收门

IF文件整体 `#if IF_PARTITION_RENDER_STATE`；HB文件整体 `#if HB_SURFACE_CONSUMER || IF_PARTITION_RENDER_STATE`，四个IF插点只在显式符号内创建/观察/裁决/释放observer。csproj与Directory.Build.props/targets没有默认注入这些符号；net10.0-windows/nullable enable/global SDK10.0.107保持不变。故源码条件隔离成立，默认不新增IF/HB测试或observer路径；本轮未反射默认DLL验证类型集合，不冒二进制全审。

已回读最后一次默认正常分析器Release ComAcceptance build日志全部可用64行：Exit0、108warnings/0errors、2.74秒；原stdout85078字符前65078已被工具截断，不能宣称全85078字符亲读。这是IF后的既有build记录，不是IG新build，不覆盖旧IE的3772warnings/9.16秒历史。因只改文档且既有build已成功，不新增build/发布扰hash。生产未改，IA初态/ID广播/COM fill原样保留；default安全仅指条件编译及既有编译证据，不等状态缺陷已修或全默认回归通过。

下一验收须独立绑定新test/MVID、各架构旧/新Native SHA和actual官方来源：x64/x86各先合法官方IF内容+真实old!=new+后snapshot基线，再旧候选红→最小生产闭包→新候选绿；x86旧红目前尚无，不预填。IF应覆盖新增监听与重注册同源新快照、无额外变化消息及cleanup，保留三色独立consumer读回/原始64字节。随后运行双架构默认HY DeviceStatePayload（不恢复已撤HY映射）、ListenerBroadcast及受影响通知锁/唤醒/注销/排空/关闭兄弟；逐项回读父子TRX、实际identity、原始payload、关键偏序与资源证据，保留IA/ID/COMfill修复。绘制成功但Present失败、非HWND/no-target、正occlusion及同步通路不得错误变Normal；未真实触发的恢复用例仍记开放，不用模拟冒真恢复。未过核心门不保留未核生产补丁。

真实HW consumer、有效HWtarget/nonzeroSyncModeStatus、Available、长期资源、loss、mediaresources、最终full四格、x64Event0仍开放；此次x64真实转换差异成立，不等完整替换。最后补记（IG）：本文全文和三共享文档最新独立受改节及最后补记已写后回读；原IF/IE及更早历史不覆盖。

