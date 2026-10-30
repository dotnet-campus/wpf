# DxgKrnl 阶段C：既有离线证据回读与有限诊断终态

## 1. 终态与独立账

Offline=ExecutedPassed；Readback/Adjudication/Closeout=Completed；KernelBridge=NotEstablished；RootCause=Unknown；原HW内容、shader、实际resize及完整替换=Open。本次窄kernel诊断结束，不重采、不扩provider、不追加显示/微ABI探针；没有生产修正，不重复原失败用例。不存在本次收尾待办。

本接续仅原件读取与Docs修改：源码/项目修改、build/test/publish/沙箱、Native、采集、独立发现、终端脚本、再委派全部新增0。既有阶段C账独立保留：normal COM build1 exit0/133 warnings/0 errors/4.65秒，COM publish1 exit0，sandbox1/离线体1/TRX1 Passed，子exit0无timeout；额外采集/Native/独立发现/原业务体/official/x64/full/重试均0。运行内枚举1824项不是独立发现或执行1824体。A、B、旧TDH r1/r2及全部历史失败/双架构有限绿不合并、不冲销、不移绑。

## 2. 原件路径与阅读层级

以下C=`WpfGfxShape/artifacts/dxgkrnl-offline-x86-r1`，R=`C/readback/Results`。既有离线task=`879c0926f0554c3bb683d793b59bc2fd`，输入采集task=`e67641eec26c40d3bf78142d7dc107b5`，二者不混同。

- 前代理的全读/执行记录：C/build-log-receipt.json登记构建日志1–64，publish-log-preflight.json登记发布日志1–60；predefined-association.md登记执行前合同与归档偏差。B的全部原件到尾阅读由[stageBcloseout](replacement-original-present-dxgkrnl-stageBcloseout.md)记载，不冒本次重读B的TRX/diag/模块JSON等全部原件。
- 本次亲读：stageBcloseout全部72行；C根下predefined-association、build-request/return/log-receipt、publish-request/return/log-preflight、sandbox-request/return全文；readback/inputs/controller.json全部145行；R/aot-identity.json全文；R/dxgkrnl-offline/decoded.json全部165行（含两份SchemaHex、payload、属性及尾部）；R/dxgkrnl-offline-x86-r1/supervision.json全部40行、stdout.txt全部12行、stderr空、TestResults/child.trx全部35行、Diagnostic/log_261028082143535.diag全部75行。
- diag初读49–69返回截断，已分49–58、59–67、68–75补齐；decoded初读长范围截断，已分1–35、36–64、65–100、101–135、136–165补齐。TRX目录列表未列出，但按监督精确路径读取成功，不把漏列当缺件。无这些必要原件的未读尾项。
- 本次不读取ETL为文本，不重新执行decoder/哈希。327680 bytes/SHA由controller与既有decoder互证，不声称本次独立二进制复算。未重审整个package manifest、全部源码或所有历史任务。

构建stdout104987字符，仅尾20000可见，前84987不可得、line9残行；发布stdout52770字符，仅尾20000可见，前32770不可得、line8残行，发布warning总数Unknown。此为前代理即时日志回执所记，本次亲读其回执，不冒本次读完整构建发布stdout。两者无独立stderr段，不认证为空。沙箱子stderr原件确实为空；stdout中文显示乱码原样保留，结论依TRX/diag/监督交叉核对。执行工具即时返回未给数字exit/task/timeout，实际退出由监督和diag确证；文件工具原返回未全部逐字即时独立归档的既有偏差保留，不补造不可见日志。

## 3. 身份、监督与离线通过的含义

真实testSHA256=`91327D06EEBF5EE3127FA35DAF4B822BC5E7B14A4396296D977BA4A97A04A81A`；MVID=`07c74248-0136-4749-b34f-76a070b59698`；.NET10.0.13/X86/pointer4。监督PID12176、子PID5012，2026-10-28T08:21:43.3906789Z至08:21:44.3571361Z；Reason=exited/HasExited=true/ExitCode0，TimedOut/Cancelled/KillAttempted=false，OutputCollected=true，两drain timeout=false，SupervisorError=null。

唯一FQN=`WpfGfxShape.ComAcceptance.D3DImageTests.WhenRecoveredDxgKrnlEtlIsDecodedOfflineThenBoundedKernelEvidenceIsRecorded`，UID=`17ae2eb1-ff17-813b-8e7f-6c12aaa711f5`；TRX total1/executed1/passed1/failed0，duration0.0878717秒；diag会话371ec4db-8b80-4a75-aece-2e760895096c，terminal exit0。监督120秒/外层180秒，整任务回收。

candidate SHA=`15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`仅夹具文件初始化身份；TRX中的HWND owner说明也不能冒本体执行产品业务。离线Passed表示有界读取/断言成功，不是kernel桥成功、候选shader/resize或HW内容通过。

## 4. 实际轨迹、schema及关联

输入`inputs/original-present.etl`，327680 bytes，SHA256=`045B1CD110EBFB3A7945DF24633BE97954007C7AE5F590F868B8FC8D38DD7CB3`。Header clock1/frequency10000000/pointer4；既有RAW_TIMESTAMP合同下ClockComparable=true。总14事件：D3D9 2、DxgKrnl 10、trace元数据2。

D3D9 provider783aca0a-790e-4d7f-8451-aa850511c6b9，1v0/2v0、channel16/L0/opcode1/2/task1/keyword0x8000000000000002，唯一同PID5836/TID11732 Start/Stop QPC39957016165934/39957016171371，均在原调用[39957016165426,39957016171383]内。Start pSwapchain为4字节0x0D2C1EC0（InType16/OutType19），Flags uint32=0；Stop Result uint32=0；属性SizeStatus/ValueStatus和SchemaStatus均0，完整schema/raw保留。AssociatedStartStop=2/Matched.UniqueRuntimePair；runtime元数据负控Passed不移作kernel负控。

实际DxgKrnl完整descriptor按Id/Version/Channel/Level/Opcode/Task/Keyword：

|实际descriptor|条数|
|---|---:|
|552/0/17/0/0/351/4000000008000001|2|
|551/0/17/0/0/350/4000000008000001|6|
|501/0/17/4/0/309/4000000008000001|1|
|184/1/17/0/0/107/4000000008000001|1|

这些仅descriptor计数，未保存其payload语义/逐条PID/TID/QPC，不猜事件名称、同次提交或显示链。目标166/171/172任意版本在完整descriptor计数中均未出现，不只是所支持v0/v2未匹配；AssociatedBlit/HistoryStart/HistoryInfo均0。KernelNegativeControls=`NotEstablished.NoActualBridge`，不是Passed。没有精确HWND0x181049C桥或非零唯一token，不能建立kernel发起/history交接。

ProcessTrace0/CloseTrace0/CloseCalls1；OpenError/Error/CleanupError/ReportWriteError均null；schema/property errors0、unknown properties/descriptors0。这里的0仅按本decoder选择处理的目标事件/schema口径，不证明10个仅计数DxgKrnl事件的payload均已解析，更不认证缺失目标事件在运行机上的schema。Header EventsLost/BuffersLost及ConsumerEventsLost0不保证目标覆盖；BuffersRead报告0不改写为另有完整缓冲审计。

## 5. 采样选择与能力限制：不能由缺事件推出未提交

本次静态亲核controller：BroadKeyword.NoEventIdFilter，DxgKrnl level4、MatchAny=0x08000000、MatchAll=0、timeout500ms，EnableTraceEx2成功0；D3D9 level6/any0xf。原Complete返回39957020613121后StopRequested39957020622674，TruncatedBeforeComplete=false。不是event-ID专采；旧决策最初的ID过滤设计不冒实际部署。

controller RequestedDescriptors及既有决策所列166v0/171v2/172v2的keyword均0x4000000008000001，与MatchAny逐位相交为0x08000000（非0），MatchAll0无附加位要求；其level4/0/0也兼容level4。实际上述10条DxgKrnl同keyword事件已进入ETL，故没有“DxgKrnl全未启用”或这三个已知descriptor被当前keyword/level明显排除的证据。不能误以为MatchAny必须等于整个event keyword，也不能因多了高位/低位便判不匹配。

但这不是运行机缺失目标的manifest/版本全面认证：三种缺失事件没有实际TDH schema可亲核；mutable-main描述符不保证当前OS全部present路径/版本均产生相同事件。仅三个目标payload选择、短时自有session、严格同步PID/TID/runtime区间+HWND及token条件，不覆盖所有异步/批处理/其他kernel路径。当前未发现确定的keyword过滤不足；也没有排除provider版本/事件生成/路径选择覆盖不足。若该不足实际存在，它属于本实验能力限制，不是GPU故障证据；不以此自动扩provider/放宽关联/重采。

最窄裁决：在上述采样与关联前提下，确认原runtime唯一Start/Stop和10条其他DxgKrnl描述符；未观察到目标三类事件，kernel桥未建立。不能断言kernel没有提交、GPU未执行/已完成、DWM已消费/合成、scanout或像素成功/失败的原因。原native前置00F3F3F3≠00563412仍由B原件成立，未达候选shader/实际resize，根因Unknown，不归咎GPU或权限。

## 6. 完整计划与唯一下一门建议（本阶段不实施）

依据总计划包1–8及实施方案§1–10，主要未证能力不能由本ETL闭合：当前候选主要回归、真实WPF内容组合、HW内容/实际resize、真实device-loss恢复、持续媒体、循环资源趋势、随机差分与最后源码双架构最终矩阵仍Open；有限ABI/ownership/KN绿不替代这些。

唯一建议另授权**当前托管生产主要回归门，非最终full**，优先推进包1并为后续生产修正提供依据，不再接力微ABI或显示探针。可行合同：使用现有正式WpfGfxShape.Tests（net10.0、MSTest4.0.1、直接ProjectReference生产），Release当前依赖图；先固化本次源码范围/实际输出身份与适用测试分母，再执行该项目现有托管回归，不改测试/skip/阈值、不新建Host、不调用COM显示诊断。建议预算normal build≤1、该正式项目test调用≤1，publish/Native/COM沙箱/采集/独立发现/重试0；测试驱动自身编译不得再计成第二次独立build或隐匿额外执行，失败保留并停止。监督须在新授权中确定适用架构及执行工具能否准确满足该分母/预算；不支持则停止并明确待决定，不把本建议当已获授权或已具最终执行包。

该门通过只新增当前托管生产回归证据，不关闭完整替换，也不能关闭上述HW/loss/media/资源趋势等开放项。后续媒体非显示实资源或其他主要能力须独立决策，不在本阶段追加。当前阶段收尾已完成；开放产品能力与待监督决定的新工作不是收尾欠项。
