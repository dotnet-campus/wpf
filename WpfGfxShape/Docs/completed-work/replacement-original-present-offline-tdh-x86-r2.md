# 原 Present 回收 ETL 离线 TDH x86 r2

## 授权与静态审核

亲自执行，不再委派；禁止终端/脚本，新运行仅Windows沙箱。r1 build1 exit1/133 warnings/1 error（CA2219）历史原样保留，r1 publish0/sandbox0。r2仅离线partial及诊断配置输出路径、文档修正；生产/collector/原shader/入口/SDK/TFM/包零修改。原ETL/controller只读，不以文本读取或复制ETL。

已读r1报告全文、恢复入口当前头部、规则全文、基线/工作项/plan当前头部、连续性收尾全文，partial全356行、csproj相关配置与targets完整诊断打包路径。原controller全119行已读。SDK10.0.26100.0 tdh.h EVENT_PROPERTY_INFO 466–513、TRACE_EVENT_INFO 529–645（中段说明超长截断已明确，结构所用字段均可见）、PROPERTY_DATA_DESCRIPTOR 651–660；evntrace.h TRACE_LOGFILE_HEADER1073–1115、EVENT_TRACE_LOGFILEW1611–1648；evntcons.h EVENT_HEADER/EVENT_RECORD/flags150–225已直接核准。

x86自然Pack8：EVENT_DESCRIPTOR16、EVENT_HEADER80、EVENT_RECORD104（BufferContext80/length86/pointers88,92,96）、TRACE_LOGFILE_HEADER272（End16/Frequency248/Start256/clock264/lost268），LOGFILE416（CurrentEvent24/Header112/lost396/callback400）。TDH前缀112、property24，count100/top104、type8/10/count16/length18；有长度和容量守卫，无猜offset。所有离线兄弟finally（CloseTrace/schema/property分配）已检查；CloseTrace仅一处且有效handle时一次，delegate活至同步ProcessTrace和CloseTrace后。callback保存EDI并继续有限文件消费，不跨native抛出；finally不重抛，清理/写盘异常保存，finally外主异常优先且EDI保堆栈，次错附Data。DTO不含IntPtr，schema和payload保留hex，类型/数组未知不猜。仅明确Present任务或Present_Start/Stop名称可关联。RAW_TIMESTAMP与clock1、实际frequency一致后才比较原QPC。负控仅关联器元数据行为。

## 执行预算和实时账

r2 normalbuild最多1：OriginalPresentOfflineTdhDiagnostic/win-x86/net10.0-windows。成功后COMpublish最多1：selfcontained NETCore/Desktop10.0.13，显式既有15A545 source-rebuild-x86-r1/native，独立r2 publish/clean；成功后唯一离线sandbox1，正式exe --supervise-exit Results/offline-tdh-x86-r2 120 精确FQN，外180，整目录回收。任一失败立即停止，不修后重试。

当前：StaticReview Completed；Build ExecutedPassed，targetPath=WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，configuration=OriginalPresentOfflineTdhDiagnostic，runtimeIdentifier=win-x86，targetFramework=net10.0-windows；exit0/133 warnings/0 errors/2.63秒。stdout104874字符仅尾20000可见，前84874不可见；64行可见日志已完整回读，非原stdout全文归档，参数摘要即时存 artifacts/original-present-offline-tdh-x86-r2/build-result.json。Publish ExecutedPassed：正式COMpublish1 exit0，完整命令即时存 artifacts/original-present-offline-tdh-x86-r2/publish-result.json，selfcontained/runtime10.0.13、显式AotDllPath及独立r2 publish/clean。stdout52796字符仅尾20000可见，前32796不可见；60行可见日志全读。publish未提供warning汇总（不可报0；可见有warning），errors0。clean runtimeconfig已核NETCore/Desktop10.0.13，aot.sha256为15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620，inputs目录确有ETL/controller；targets逐依赖Exists门通过。Sandbox ExecutedPassed；r2 build1/publish1/sandbox1/离线体1（Passed），累计含离线r1 build2（失败1+成功1）/publish1/sandbox1/离线体1。Native/独立发现/原业务测试/重采/x64/full/终端/脚本/再委派均0。TRX含公共夹具HWND owner初始化日志，不将其称为新增显示实验，也不宣称整个进程窗口0。完整替换Open，产品基线不升。

## 最终原件回读与来源边界（本次文档收尾）

Readback/Adjudication/Closeout=Completed；本次新增源码/配置/build/publish/沙箱/test/Native/重采/终端/脚本/再委派全部0。上文静态审核及构建发布日志回读是前代理记载，不冒本次重新全文审核。实际artifact根为 `WpfGfxShape/artifacts/original-present-offline-tdh-x86-r2`（R），不是工作区根artifacts。

本次亲读：本报告原15行全文，R/{build-result,publish-result,sandbox-result}.json各全文；readback/Results/offline-tdh-x86-r2/{supervision.json全40行,stdout.txt全12行,stderr.txt空,TestResults/child.trx全35行}；Results/aot-identity.json全文；readback/inputs/controller.json全119行（1–60/61–119）。Results/original-present-offline-tdh/decoded.json共607行，本次亲读1–60、62–74、275–310、395–607，非本次全文：首段因长SchemaHex截断，补读62–74保证header/payload元数据可见；未以不可见的61行完整SchemaHex冒全文，原hex仍保留。主要结论所需summary、Present两事件/全部字段、unknown数组及另外两事件descriptor均已亲核。前代理全读主张不替代本次范围。源码仅亲读离线partial的1–65、70–124（125尾截断）、126–230、275–337（338尾截断），用于核RAW_TIMESTAMP、同步唯一ProcessTrace/CloseTrace路径及负控；不冒完整394行复审。原ETL不作文本读取、不重新执行哈希工具；摘要由既有decoder读文件计算并与controller硬校验，二者记录一致。

build-result是即时摘要，不是完整stdout归档；其Publish/Sandbox=0及publish-result的Sandbox=0是阶段快照，被后续sandbox-result及原件补全，不是最终未运行。build133 warnings/0 errors；publish warnings总数null且可见有warning，不报0。缺失构建输出前84874字符、发布前32796字符仍缺失，本次未补造。

## 已有唯一离线运行及身份

任务dc69a9a07c4544afbf3b8f88b6eec5cf。正式exe `--supervise-exit Results/offline-tdh-x86-r2 120` 精确筛选 `WpfGfxShape.ComAcceptance.D3DImageTests.WhenRecoveredOriginalPresentEtlIsDecodedOfflineThenRuntimeBoundaryEvidenceIsRecorded`，外层180秒。监督pid9604/子12052，exited/exit0，TimedOut/Cancelled/KillAttempted=false，OutputCollected=true，drain超时false，SupervisorError=null。TRX total1/executed1/passed1/failed0；stdout中文乱码原样保留，stderr空，不能仅靠乱码摘要裁决。

test SHA256=C349CAB2A0532C501ADACDD2AE3A87D6BBA0FE1DA1663DF9D5488594CAD93CD4，MVID=84e7d121-25b4-4485-a13e-88b2c636b02a，.NET10.0.13/X86/pointer4，supervision与decoded互证。aot-identity仅记录夹具文件初始化的候选路径/SHA15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620；TRX同样只是artifact/公共owner日志，不证明候选产品模块实际业务加载或shader/resize覆盖。离线测试体只调用Decode，不创建图形设备或新trace session；不得把夹具初始化升级为产品验收。

## 原ETL解码裁决

输入131072 bytes，SHA256=4085C964310A28A8FCEB686326F401B4405F8B8512C408307B89F81F050214E6。原controller PID2748/TID4736，调用闭区间[39911863244854,39911863255850]、frequency10000000、PresentResult0。实际header clock1/frequency10000000/pointer4，源码消费mode0x10001000（EVENT_RECORD | RAW_TIMESTAMP，非实时）；ClockComparable=true，因此以下原始QPC可与controller比较，不能把header的StartTime/EndTime当同尺度QPC。

TotalEvents4：系统EventTrace Header（Id0/v2/opcode0）及PartitionInfoExtensionV2（Id0/v2/opcode80）各1，provider68fdd900-4a3e-11d1-84f4-0000f80464e3；另有D3D9两事件，provider783aca0a-790e-4d7f-8451-aa850511c6b9。两D3D9事件均channel16/level0/task1/keyword9223372036854775810、SchemaStatus0/DecodingSource0、HeaderPid2748/HeaderTid4736、HeaderFlags544；TaskName=Present，实际OpcodeName保留尾空格。

|事件|descriptor及原QPC|实际TDH字段（InType/OutType，schema长度/实际bytes）|
|---|---|---|
|Start|Id1/v0/opcode1，39911863251053，payload8 bytes|pSwapchain：16/19（POINTER/HEXINT64），4/4，count1，0x0CC78FE0，hex E08FC70C；Flags：8/8（UINT32/UNSIGNEDINT），4/4，count1，0，hex00000000|
|Stop|Id2/v0/opcode2，39911863255835，payload4 bytes|Result：8/8（UINT32/UNSIGNEDINT），4/4，count1，0，hex00000000|

三属性Flags元数据均0，SizeStatus/ValueStatus均0，State=Decoded；不因pSwapchain OutType19就改猜8字节，实际x86 pointer长度4。两事件都落在上述闭区间，Association=Matched.RuntimeBoundaryOnly，AssociatedStartStop2。只能确认这一次原调用内确有D3D9 runtime Present Start/Stop且runtime Result0；不证明kernel提交、GPU执行、DWM消费、scanout或像素呈现。

UnknownDescriptors2是上述系统元数据事件未纳入Present关联，不是TDH失败。UnknownProperties1是Header.TimeZoneInformation数组：InType4/OutType301/count176/schema length1/bytes176，Value=null但hex保留，SizeStatus/ValueStatus0、State=UnknownTypeOrArray.RawHexPreserved；不猜数组语义。TdhSchemaErrors0/TdhPropertyErrors0；OpenError/Error/CleanupError/ReportWriteError均null，ProcessStatus0、CloseStatus0、CloseCalls1。源码同步ProcessTrace调用仅一处，本次成功路径一次；CloseTrace在有效handle finally仅一处，报告实际计数1。callback/processing线程均8。header EventsLost/BuffersLost及consumer EventsLost均0，BuffersWritten2；BuffersRead0是原记录，不用它外推无消费或所有系统会话/句柄/线程全清零，也不推广失败清理路径认证。

NegativeControlsPassed=true只认证纯Matches谓词：正确元数据在Before边界匹配；错误PID/TID、Before−1、After+1、known=false、comparable=false均拒绝。源码用>=与<=；此负控没有独立测试恰好After端点，没有篡改ETL/TDH真实schema或模拟native失败，不冒端到端负控或kernel/呈现验证。离线Passed不要求所有事件都已知，更不是产品Passed。

## 最窄结论及下一授权决策

r1构建失败CA2219与r2构建成功133warn0err、publish1、sandbox1/离线Passed账全部保留；原始ETL采集轮、旧轨迹/显示失败、双架构有限绿各自独立，不冲销、不移绑。旧文档中的“ETL未解码/Present事件Unknown”在该单文件范围由本次runtime观测取代，旧失败事实仍成立。当前原shader/resize仍被系统native前置像素00F3F3F3≠00563412阻挡；本轮未修任何产品问题，未改变原失败前置，也没有可验证的生产修正依据，根因Unknown，完整替换及最终矩阵Open。

因此不建议立即重复原失败用例或最终矩阵，不把runtime Start/Stop当解除前置。真正缺少的是能将同次Present关联到kernel提交/队列/完成及后续DWM/scanout或最终可见内容的证据层，以定位runtime返回与实际显示之间断点；本ETL只有D3D9及系统元数据，不能提供该层，也不能据此归因驱动/沙箱或缺权限。

待监督另行决策，仅建议不执行：独立审核是否值得一项有界kernel证据任务（先说明可观测对象、同次关联键、成功/失败与停止条件、预算及其能否改变修复决策；kernel证据本身仍非DWM/像素证明）；或转向主要其他未完成验收并明确原HW/shader/resize保留Open。此阶段不扩provider、不重采、不建解析平台、不追加显示变体，不转外部维护者。文档收尾已完成，无收尾待办；任何后续工作均需新授权。
