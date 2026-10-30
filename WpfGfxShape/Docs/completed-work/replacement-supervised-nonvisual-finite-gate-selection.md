# 显示冻结后的非视觉有限门选择（仅静态调查，待授权）

## 裁决

推荐唯一最小门：**异步失败以 partition 为边界：同源 sibling 传播失败，同连接独立 partition 继续成功；关闭失败 partition 后健康 partition 仍可用。** 不是重跑 KN17、泛 COM refs 或模拟 device-loss。只验证错误输入触发的公开 ABI 可观察传播/隔离及有限关闭合同，不验证真实 HW、真实设备丢失、媒体或内容。

本阶段源码/配置修改、build、publish、沙箱、测试、终端/脚本、再委派均0；仅文档。未重算二进制 SHA，未亲核历史全部原件。用户本轮授权覆盖旧交接中“必须先由主代理取得显示环境才能继续”的停滞前提：显示线冻结，不等于全部工作阻塞。

## 依据和实际发现

已读完整替换方案1–120（长输出截断处以分段补读）、当前执行规则、当前覆盖报告§5/最终矩阵、public-control完整57行及恢复入口/事项/状态最新段。完整方案§3、§5、§6.4、§7要求 ABI 后续状态、必要时序、关闭和检测负控；错误输入允许有限独立用例，环境错误码不设等价门。

具体源码：
- `AsyncUceChannelTests.cs:68–118`：单通道 uint.MaxValue 命令后重复 flush 精确88980406；另有失败 batch callback 清理。`:150–202`：FIFO/worker退出/跨通道callback关闭，均不是故障 partition 隔离断言。私有 Api(:260起)硬绑定候选且不显式初始化 manager，不能原样当官方控制。
- `UceChannelTests.AsyncDrain.cs:140–205`：有真实 WIC source、失败后的排空和 sibling；官方参数仅0/1/2/6/7/8，失败参数3/4/5/9/10/11仅候选，且没有同时健康独立 partition 的成功断言。不重跑这18条充新覆盖。
- 官方 `connectioncontext.cpp:585–646`：source非零复用 composition，source零新建 composition。候选 `Abi/UceChannelExports.cs:138–180`同样显式共享/新建 SameThreadPartition。
- 官方 `partitionmanager.cpp:507–547`：普通失败对外映射 RENDERTHREADFAILURE，OOM/OOVM另论；`composition.cpp:2138–2190`通知已附着通道；`crossthreadcomposition.cpp:130–171` zombie后释放后续batch并完成flush。
- 候选 `PartitionScheduler.cs:157–184`保存 partition 首错并映射completion；`SameThreadChannel.cs:319–328`只是读取实际消息队列。这些是实现调查，不是20B3运行证据。
- **排除一个错误门**：不能直接要求 PeekNextMessage 返回type6。官方 `connection.cpp:439–458`将PartitionIsZombie在连接内部消费并写channel.Zombie，不把它走default公开消息队列；`clientchannel.cpp:814–839`通过SyncFlush暴露保存错误。`exports.cs:287–341`虽声明type6/offset8，单凭结构存在不能构造公开消费预期。
- `HyDeviceStatePayloadTests.cs:65–139`已独立断言非空type5初态NoDevice快照、注销/重注册计数和完整64字节；这不是缺新名字可复跑的空白区。

## 优先级候选（共3，只有第1推荐）

|优先级/有限合同|20B3/40D0现状与新增判别力|真实对象/事件、负控和视觉边界|
|---|---|---|
|1 异步partition失败共享/隔离|KN/KO绑定20B3/40D0各17/17仅有限resize/lifetime；当前覆盖映射没有这两身份下“失败A→共享S失败，但独立B成功且A关闭后B继续”的独立官方/候选证据。旧1DAA异步组/旧full不能移绑。新增能区分错误隔离过宽（连接/全局被毒化）、过窄（sibling不传播）、关闭连带损坏；不是只重复单通道88980406。|一个真实异步connection、A和S(source=A)、B(source=0)，显式manager初始化/反初始化。使用已存在CompleteNullSource错误输入：创建真实bitmap资源95，发送完整0x0c包但source=null，非任意指针/短包。原始每次HRESULT、合法资源更新、destroy/disconnect/deinit均保存。负控用结果副本将S失败改成功、B成功改失败，判定器须拒绝；不能冒生产故障注入。完全无HWND/D3D/GetPixel/Present/桌面捕获；不证明任何内容/HW/loss。|
|2 Available正常失效通知的准确一次/版本合同|当前20B3/40D0只KN正常target销毁Available0/701；direct另为官方Available1/Version7，不是候选。DR版本All断言在回调空集合仍成立，NativeInvocationCovered=false不等于通知通过。新增须真正收到预定非空通知，并区分版本/重复/注销后回调。|必须真实D3D9 device/surface/interop/消费绑定及可证明正常失效事件，不是只Create/Detach空跑或模拟loss。现有D3DImage业务有窗口取色前置，不能隐藏复用；需更多准备，非最小门。负控旧version/重复通知。可不取桌面像素，但HAL/Available仍不证明consumer HW或真实loss恢复。|
|3 device-manager资源通知/清理|`Direct3D9DeviceManagerTests.cs:1–65`已有MarkUnusable、重复不通知及通知先于partition move；managed4256绿不绑定20B3/40D0 native模块。不能以再跑这些注入factory测试称新增真实设备清理。新增实际adapter/device失效下通知和缓存收敛需公开可观测真实事件；当前不足选具体未覆盖公共路径。|内部delegate/fake device只能证明管理算法。真实device、adapter事件、surface/cache引用与后续更新才支持实际资源恢复；缺少负控资源账不得以最终refs0充替。真实HW/loss仍须事件与恢复新内容，故本轮不推荐。|

## 唯一门的精确最小实现范围（尚未实施）

建议仅新增 `Tests/WpfGfxShape.ComAcceptance/UceChannelTests.PartitionFailureIsolation.cs`，复用正式UceChannelTests partial。**不改现有私有Api、生产、NativeRun、ExitDiagnostics、csproj、resx、包依赖、TFM/SDK或任何Host；无新条件编译，Release默认可编入。** 新文件独立声明所需C# ABI，复用现有角色路径和监督约定，不复制整个测试框架。

拟新增两个准确FQN（当前不存在，不冒现成入口）：
- `WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives`
- `WpfGfxShape.ComAcceptance.UceChannelTests.WhenAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives`

两方法同一场景，分别只加载OriginalDll/AotDll，同exe不同进程，不自动启动第三重复角色。初始化manager(0)→connect(0)→A/source0、S/sourceA、B/source0。三通道先提交真实visual39和合法opacity更新，flush均0，排除S本来无效/B本来不可用。A另建bitmap95，先成功flush创建，再发完整BitmapSource null包触发异步错误。A/S各flush应88980406；B新一次合法opacity更新/flush应0。销毁A和S后B再一次合法更新/flush应0，随后destroy B、disconnect、deinitialize。

仅**1组参数**：async、同connection、A/S/B、一次CompleteNullSource错误；无OOM、随机、线程、resize或媒体扩展。包布局沿用`UceChannelTests.AsyncDrain.cs`的完整包，公开参数/路由依据来自上述官方源码，不使用候选类型生成expected。最小负控仅结果副本两种篡改，不新增生产注入。

官方实际版本合同仍需探索：同一官方方法就是一次有界控制，不另外加“基础设施控制”。先完整记录业务再在finally清理后断言。如果官方S不呈88980406、B失败、清理异常或身份不足，停止，不运行候选、不直接改expected；报告实际边界，再决定是否另授权缩减。官方安装DLL不凭仓库源码保证相同编译行为。禁止去执行同线程malformed失败路径：CL报告源级悬空batch风险不属于此async门。

每个角色必须在加载后用module句柄匹配进程模块BaseAddress，记录actual完整路径/SHA/版本，另记test SHA/MVID/runtime/架构/指针宽度和请求路径；官方不加载候选执行。AssemblyInitialize的20B3文件校验只属于文件身份。保存每阶段HR和参数、首错、原始业务JSON，finally逐项记录所有destroy/disconnect/deinitialize及异常；无返回计数的对象不杜撰refs0。模块驻留不FreeLibrary。仅真实业务cleaned=true且逐项清理结果可核才讨论关闭收敛；不声称GPU资源趋势或worker退出已证。

## 现存clean的准备程度（离线已查）

`artifacts/direct-consumer-clear-control/com-acceptance-package/`实存。离线DLL公开API已确认 `AsyncUceChannelTests` 及以下现成精确FQN：
- `WpfGfxShape.ComAcceptance.AsyncUceChannelTests.WhenAsyncPartitionFailsThenFlushPreservesFirstHResultAcrossLaterBatches`
- `WpfGfxShape.ComAcceptance.AsyncUceChannelTests.WhenShutdownDrainsClosedAndOpenFailedBatchesThenReleasesAreFifoAndWorkerExits`
- `WpfGfxShape.ComAcceptance.AsyncUceChannelTests.WhenReleaseCallbackClosesAnotherChannelThenQueuedCleanupDoesNotDeadlock`

它们无需guard，但候选硬绑定、没有本门A/S/B断言，也缺本门actual角色身份完整报告；**不是可直接零build完成新门的现成包**。现存public矩形方法不得当本门前置。clean只可复用打包/监督结构和已认证Native20B3文件；现成test CB821F7F…/MVID d947710e…并不包含拟新增方法。40D0需独立同架构包与实际加载核验，不能拿x86包认证。

DR全文件受`DR_INTEROP_ABI`，而`DtInteropCreateTests.cs`当前为空；不能猜默认clean有DR入口。`HW_NOTIFICATION_UNREGISTER`同样显式guard；HY默认可用但其group会额外official-repeat，不适合本次official≤1预算。主线不新增这些guard配置。

前置仅Windows相符架构、正式exe运行时/依赖、正确OriginalDll与Native DLL、官方manager初始化及真实UCE连接/通道；不用可见桌面、窗口、D3D device或取色。x86首门可使用同版10.0.13及现存20B3，runtime不形成永久产品限制。

## 待授权预算和结果纪律

执行全部留待主代理授权。本报告不授予运行。建议首门x86 Release：COM build≤1/publish≤1（只为新测试及clean），Native发布0；official≤1、官方合同与身份有效后candidate≤1；各精确FQN filtered监督120/外180。不跑发现、full、KN17、x64、public-control、direct/native/WGC或取色变体；不要把两角色组方法偷偷扩大运行数。x64是后续独立授权范围，本门不能关闭双架构。

clean包精确来源、实际参数、任务号、supervision/stdout/stderr/TRX/诊断/business JSON与所有清理记录都保存在新的ignored artifacts子目录并回读；计数和结果不拼历史。原始结果先落盘，断言失败也不覆盖记录；负控只作用副本，原件不删。仅清理本门创建的真实对象，不清旧artifacts或宿主目录。

## 账本与计划状态

public task03041eb56b8f4775a250759d0966e617 expected00563412/actual001E1D1C及actual official身份不足保留；public-control独立1失败。direct build4(1败3成)/publish3/official3失败/candidate0不变。WGC build4(1败3成)/publish4(1败3成)/沙箱3(发现1真实2)/真实体2失败、撤后default build1成功及通用打包未精确退场残项不变。KS build1/publish1/沙箱4/请求3/发现1/体1/A1/B1/native2另账不变。KN原地resize有限修复及两架构历史17/17保留，不冒内容/真实loss/full通过。

完整方案§4 ABI外部后果/§6时序资源有可推进的有限新门，但本阶段只选门和精确范围，尚无新增通过、无已证生产缺陷或修复。Available/HW backend/真实loss/media资源/最终四格仍Open。**完整替换未完成；显示冻结不等于问题关闭，也不等于所有工作阻塞。**
