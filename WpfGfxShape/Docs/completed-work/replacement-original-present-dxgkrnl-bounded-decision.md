# 原 Present：有限 DxgKrnl 采集与离线解析决策

## 1. 唯一裁决与当前账

**值得实施一次，但仅作为“当前普通 D3D9 原调用到 kernel Blit 发起／history 交接”的有限证据门；不承诺完成链或根因。** 唯一推荐新增 DxgKrnl 166/171/172 三种事件，不新增 DWM、Win32k、Queue、DMA、scanout 或 PresentMon 状态机。保留 D3D9 原 runtime Start/Stop 作为同一新轨迹内的锚，不再单独重采 D3D9-only。下一动作仅申请阶段 A 实现及 normalbuild1，不自动发布采集。

本阶段亲自只读调查、网络官方文档／可信源码及文档更新；源码、配置、资源、build、publish、沙箱、测试、Native、official、x64、full、终端、脚本、CLI、再委派全部新增0。Design=Completed.ReadyForStagedAuthorization；Implementation/Build/Publish/Capture/Offline=NotStarted。没有取得新运行机 schema、权限结果或事件。本裁决不是再次原用例授权。

旧 ETL SHA256 `4085C964310A28A8FCEB686326F401B4405F8B8512C408307B89F81F050214E6` 131072 bytes 保留；原 PID2748/TID4736、QPC39911863251053/39911863255835、Result0 已有 runtime 配对，不再质疑或重做这一结果。00F3F3F3≠00563412及 shader/实际resize未入、原失败原件全部保留。**新采集只能关联新任务中的原位置、原参数那一次 Present，绝不能回溯补证旧 PID2748 那一次调用。** 完整替换 Open，双架构有限绿及所有历史独立账不冲销、不移绑。

## 2. 本次实际核查及来源边界

已读最新恢复入口、current-work-item、plan状态、baseline的当前头部，当前规则全部27行、continuity-recovery-and-closeout全部66行；r2的原ETL裁决及最终边界、raw-etl-stage1实现合同、raw-etl-x86-r1-closeout时序／失败／清理段、system-trace-decision白名单与旧HT/HQ来源。部分长范围工具返回截断，未冒全文重审；历史事实依已登记报告，不冒本次重读全部task JSON/TRX。

亲核现代码的关键路径：OriginalPresentSystemTrace Before/After、Start临时权限及Enable、StopCore/Dispose；OriginalPresentOfflineTdh DTO、输入硬编码、callback/TDH属性与关联器；SwapChainObservation全文；OutputWindow Complete全文；VisualCache原scope及原像素前置；csproj诊断属性与Package.targets自有输入clean复制；HP的Enable／filter结构。不是全仓源码或全部失败路径重新认证。全仓AGENTS名称查询无命中。

本次实际取得、只读不运行：

- https://github.com/GameTechDev/PresentMon/blob/main/PresentData/ETW/Microsoft_Windows_DxgKrnl.h ：Intel MIT生成描述符，下面三个事件的完整descriptor来源。
- https://github.com/GameTechDev/PresentMon/blob/main/PresentData/PresentMonTraceConsumer.cpp ：HandleDXGKEvent的字段名／读取类型；HandleDxgkBlt、HandleDxgkPresentHistory、HandleDxgkPresentHistoryInfo的关系语义。FindOrCreatePresent还存在“同进程最老deferred present”启发式，本门明确**不采用**。不能把PresentMon推断的Displayed等价为像素证据。
- https://learn.microsoft.com/en-us/windows/win32/api/evntrace/nf-evntrace-enabletraceex2 ：Level、Any/All keyword、非零Timeout、EVENT_ID过滤、过滤不能消除全部provider生成成本、真实API状态及权限边界。
- https://learn.microsoft.com/en-us/windows/win32/api/tdh/nf-tdh-tdhgeteventinformation ：实际EVENT_RECORD取schema，122容量协议，1168/schema与resource缺失均可能失败。

main是可变网页，不是已固定commit；不是运行机manifest认证。可信代码足以选择事件和字段，**实际版本／长度／InType最终以同机TDH为准**，不复制C++实现、不执行PresentMon CLI。旧HT报告亲读目标桥接段：仅旧x64 Ex首红成功，166精确HWND/PID/TID/调用区间、17标量、Token0；不能挪给当前x86基础9失败。旧HQ成功仅说明已有实现与权限途径，不保证新任务权限。无工具session参数不构成ExternalBlocked。

## 3. 仅三个 kernel 事件的合同

provider `Microsoft-Windows-DxgKrnl`，GUID `802ec45a-1e99-4b83-9920-87c98277ba9d`。以下十进制ID，keyword十六进制；三者 channel17，keyword均 `0x4000000008000001`。

|事件|Version / Level / Opcode / Task|必要字段及来源|最大语义|
|---|---|---|---|
|166 Blit_Info|0 / 4 / 0 / 103|`hwnd` uint64语义、`bRedirectedPresent` uint32，可信consumer直接读取；`bSubmit`、`PresentHistoryToken`、source/dest allocation及rect只在实际TDH存在且类型受支持时保留，旧HT曾见但本轮不假定布局|kernel Blit发起／路径标志，非GPU完成|
|171 PresentHistory_Start|2 / 0 / 1 / 6|`Token` uint64、`Model` enum32、`TokenData` uint64，HandleDXGKEvent/HandleDxgkPresentHistory|同线程在途Present产生history token|
|172 PresentHistory_Info|2 / 0 / 0 / 6|`Token` uint64，HandleDxgkPresentHistoryInfo|同token向DWM交接／ready观察，非DWM已消费／合成／扫描|

D3D9原GUID `783aca0a-790e-4d7f-8451-aa850511c6b9`，1v0/2v0/channel16/L0/opcode1/2/task1/keyword `0x8000000000000002`；字段pSwapchain/Flags、Result继续由实际TDH解码。x86原件pSwapchain实际4字节，不因consumer读uint64就猜8字节。

新配置D3D9 EVENT_ID过滤[1,2]，DxgKrnl EVENT_ID过滤[166,171,172]；Dxg Level4，MatchAny=0x08000000（Present），MatchAll=0，Timeout500ms；D3D9沿原level6/mask0xf但加ID过滤即可，不另改runtime合同。保存请求和实际每个descriptor。仅offline选择这三种kernel语义；不因keyword相交就声称只产生三种provider内部事件。无PID scope过滤：history Info可能由系统／DWM上下文触发。filter不支持／Enable失败无宽采fallback，原体不改，标诊断失败并停止该门。

不依赖先跑独立metadata任务。新采集的原ETL完整保存；阶段C在同运行系统版本的正式exe里对**实际回调**调用TdhGetEventInformation/PropertySize/Property，保留schema/payload hex、descriptor全七项及headerflags/PID/TID/rawQPC、字段flags/InType/OutType/count/length/实际bytes/API状态。若需运行前验证filter新增ABI，只读本机SDK声明并加布局守卫，不用旧x64偏移。未知版本、schema失败或字段不符：原始记录仍保存，Association=UnsupportedSchema/Unknown，不以事件名称近似匹配，不扩版本或provider重试。缺171/172不阻止166有限结论；缺166精确窗口桥则不称完整目标history链。

## 4. 最多两个可证伪假设及关联算法

**H1：新任务原普通9调用期间能观察到目标HWND的kernel Blit发起。** 接受标准：同新任务controller内唯一原调用；新D3D9 Start/Stop唯一同PID/TID，RAW_TIMESTAMP且clock1/frequency吻合，Start/Stop位于原Before/After，Result原样记录；166必须处于这对runtime边界内、headerPID/TID同时等于原实际调用线程、payload hwnd精确等于存活的原窗口。记录window创建/生命周期及owner PID/TID（owner可与调用线程不同，不混用），只限该scope、不跨销毁复用；若同线程多次/嵌套Present或多个相冲突166则Ambiguous，不挑最近一条。此为有可信同线程发起语义、精确窗口及唯一调用锚的桥接，不是仅时间相邻。headerPID不同或仅HWND相同，不降格接受为同次原Present。

H1未观察到，只能否定“本窗口本过滤可观察的同步目标Blit”，不能否定真实提交。外线程deferred/batched提交、不同present路径、优化/取消、provider/schema/filter差异、enable生效／事件生成、窗口截断均可解释；lost0也不排除这些。**不采用PresentMon同进程最老Present的补绑启发式。**

**H2：已按H1绑定的同次调用还有可唯一建立的history Start→Info交接。** 171须实际受支持schema、原PID/TID、在同一runtime Start/Stop内、唯一非零Token；与H1在途调用一致，无第二个竞争Start。172只能由同provider同语义`Token`精确相等、rawQPC不早于171且不晚于固定Complete边界关联，允许headerPID/TID不是目标（它是事件执行上下文）。在有限文件内若token重用／多个竞争Start／冲突序列则Ambiguous；零token/缺Start不匹配Info。Model保留原值；consumer对Model=REDIRECTED_GDI(1)另行处理，本门记UnsupportedModel，不套通用交接；未知Model不推路径。TokenData只是payload，不能代替Token。允许171在166之前，不能人为要求Blit先于history。

H1成立但无有效H2：只报 `Matched.KernelBlitInitiatedOnly`。H2成立：最多 `Matched.HistoryHandoffObserved`。171虽同线程但无166窗口桥：报 `SameCall.HistoryCandidateWithoutHwndBridge`，不冒完整HWND→history。泛系统172、仅时间接近、不同PID的166都仅保留context。

**禁止等值越界**：D3D9 pSwapchain、controller device、allocation、pDmaBuffer、PresentHistoryToken、Token/TokenData并非因为都是地址/整数就同命名空间。可信consumer并未用166.PresentHistoryToken直接等于171.Token建立此桥；本门不新增该等值假设。这里runtime→kernel依赖同步同线程+精确HWND+唯一在途调用，不是共享swapchain ID；跨线程时缺可靠桥键就Unknown。H2也不是kernel队列完成；没有hContext+SubmitSequence、没有DMA完成、没有目标DWM/scanout桥，绝不称完整runtime→kernel→显示因果。

## 5. 固定采集边界、异步不足及扰动

现有After在原Present finally调用StopCore("PresentReturned")；其后才Assert(result)、CompleteNativePresent。因此当前窗口不覆盖event query、消息泵和两次DwmFlush。history Info可在其他线程异步发出，Present返回前Stop对H2明显不足。

唯一改法：新默认关闭kernel符号下After**只内存记录原After/Result，不Stop**；仍原Assert(result)、原CompleteNativePresent(device,window)一次，不改内部5秒query期限、GetData flags、消息处理或两次DwmFlush。在PresentNativeControl现有原体外层finally，于Complete正常返回或异常退出的第一处记录CompleteExitQpc、Returned/Aborted及错误状态，然后请求一次按handle Stop；原Present抛错/Assert失败同样finally Stop，不跳过原异常。Complete前后仅内存标记；Stop逻辑复用原一次所有权锁。VisualCache的using Dispose仍是异常安全后备，不另做第二次ControlStop。

正常语义终点是**原Complete返回后一次Stop**，不是按像素／token／事件数判断。保留现有Start成功后2秒watchdog，实际采集截止为CompleteExit请求与watchdog请求先到者；query可能合法运行至5秒，若watchdog先停明确记Truncated.BeforeComplete。既有外监督120秒／任务180秒复用；Stop API及调度不是硬实时承诺，保留请求/完成QPC及timeout事实。不能悄悄把watchdog延长以追事件。

离线目标异步匹配上限取CompleteExitQpc（watchdog更早则取更早截止）；Stop排空到ETL不意味着边界后新发事件也属于目标。Complete中的query只约束既有GPU事件，DwmFlush约束其文档意义的合成调度，不保证该目标token的Info一定已发出或某色已显示。因此即便正常到Complete后停止仍可能截断目标交接，缺172不证明未交接；本门不额外sleep/flush/present，不等到色对、不循环续采。

开启ETW前置成本、系统wide provider开销、锁／QPC、Stop从Complete之前移到之后及摘要写盘都会扰动原观察时序，明确记录为新诊断条件，不能称零扰动／旧失败同次复现。原参数/线程/Clear/NULL rect/override/format/DPI、原00563412容差、前置顺序不改。Stop/Dispose落盘仍在Diagnose及原像素断言之前；本门不移动观察者以“改善”内容。若原前置意外通过仅同体自然进入业务，不能由trace成功跳过断言。

## 6. 小改可行性及精确最小文件修改表（未来，不是已修改）

只用既有ComAcceptance工程／正式exe，无新Host、库、平台或包。collector已有x86成功文件session、权限恢复、唯一handle Stop、错误DTO、hash；offline已有跨provider TDH保存能力，不必再建reader。新增filter的自然布局、x86指针零扩展须核SDK；HP `Pointer = (ulong)filter.ToInt64()`不能原样搬到x86，应按nuint零扩展。新配置不污染既有r1/r2配置或原文件硬校验。

|文件（均位于Tests/WpfGfxShape.ComAcceptance）|最小变更及阶段|
|---|---|
|D3DImageTests.OriginalPresentSystemTrace.cs|A：复用partial，在额外ORIGINAL_PRESENT_DXGKRNL诊断条件下新增一provider Enable、[166,171,172] ID filter及D3D9 [1,2]过滤；provider请求／状态DTO、Complete边界/窗口owner生命周期标记；After不Stop，独有CompleteExit Stop入口；原watchdog/权限/owned-stop/错误保全保持。未开新符号原D3D9-only行为不变|
|D3DImageTests.SwapChainObservation.cs|A：只在新诊断条件下围原Present/assert/Complete加finally边界，原inner finally After仍紧邻调用；原业务异常保留，不吞原断言；仅一次原调用|
|WpfGfxShape.ComAcceptance.csproj|A：新增OriginalPresentDxgKrnlDiagnostic，定义原collector符号+新符号、独有obj/bin、win-x86、10.0.13 pin及既有15A545 Native路径；不改TFM/SDK/C#或原配置。C：新增OriginalPresentDxgKrnlOfflineDiagnostic定义离线符号+新离线分支，独有输出及固定新ETL/controller输入，输入路径须据B真实回收确定|
|D3DImageTests.OriginalPresentOfflineTdh.cs|C：保留原r2硬编码golden分支；新诊断分支只接B固定ETL/controller、expected SHA/bytes/任务身份清单；同一TDH reader和清理边界，新增166/171/172严格schema gate及最多两步关联/负控。现有descriptor七项/schemaHex/payloadHex/属性flags等已经足够保真，不重写DTO平台；修正新分支Evidence/Conclusion不可继续叫RuntimeBoundaryOnly|
|ComAcceptance.Package.targets|C：仅新offline配置的两固定inputs加入现有clean白名单/Exists门，不复制整个历史artifact树|

VisualCache、OutputWindow、NativeRun、生产、公共owner/capture及原expected **无需修改**。AcceptanceResources.resx预期0修改，复用现有OriginalPresentTraceContract；若必须新增用户错误文本，仅该资源增键，不散落字符串。没有无关身份扫描／模块枚举任务。

A建议新增/变更约100–160行，C约100–180行；不是承诺统计。若必须新实时消费者、版本推测布局、struct/array通用解码或跨线程状态机，或总新增明显超过约350行，即PreBuild.Stop并交接，不扩成平台。A编辑前核实际SDK filter结构与清理；本阶段只调查未完成该新增ABI认证，不能以旧x64绿替代。

C负控须覆盖：错误HWND/PID/TID、runtime区间外、未知descriptor/version、错误clock、竞争Start、零/错token、token复用、Info跨PID允许但无Start拒绝、Info超固定终点拒绝；关联器自检不是运行端到端故障注入。原r2单文件结果保持不变，旧ETL不塞新kernel结论。

## 7. 分阶段预算与明确停止

|阶段|授权范围与上限|停止点|
|---|---|---|
|当前调查|仅报告和恢复导航；执行全部0|Completed，待A新授权|
|A 实现+normalbuild|仅表内collector/hook/新采集配置，normal COM build≤1：该csproj / OriginalPresentDxgKrnlDiagnostic / win-x86 / net10.0-windows；publish/采集/test/Native0|静态ABI/cleanup失败即编译前停；build失败保留完整可见错误，不修后重试；成功只交接B，不自动执行|
|B 发布+采集|另授权COM publish≤1，selfcontained NETCore/Desktop10.0.13、显式既有15A545 native、独立publish/clean；正式exe原精确FQN `WpfGfxShape.ComAcceptance.D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive`，--supervise-exit独有Results目录120，外180；沙箱≤1/原体≤1|new ETL/controller/setup、原TRX/业务失败、stderr/stdout/supervision/identity全回收；Stop/watchdog/权限/过滤API状态真实记录；不在B同任务塞decoder，不重试原用例。业务失败不是collector失败，反之亦然|
|C 复用B轨迹离线|单独授权源码/配置表内C行、normalbuild≤1、COMpublish≤1、离线沙箱≤1/离线体≤1；固定B输入，禁止再StartTrace和原业务采集|有效文件零目标事件亦输出Unknown；未知schema／桥键不足即收尾，不另重采；默认关闭纯C#同exe，不独立发现|

各阶段Native0、official0、x64full0（x64及full均0）、生产publish0、额外等待0、独立发现0、再委派0；发布自身编译成本计入publish，不偷加normalbuild。A/B/C各自执行账独立、不借额度重试，不把这张未来表记成已消耗。只允许当前阶段到指定停止点，失败完整保留、不能看到事件少就不停。

## 8. 结果能改变的决策，及终止后的主线

- H1真：反驳“该新任务原调用只有runtime边界、连目标kernel Blit发起都未观察到”的证据空白；停止围绕是否到达Blit入口的重复runtime诊断。即使token0，只获kernel发起仍有**一次有限价值**：同当前普通9对象的正证据，而不是借旧HT。但不能判提交到GPU或完成，更不支持任何产品修正。
- H1+H2真：把已观察断点推进到history交接／ready；不再把“没有runtime→kernel/history”当解释依据。若像素仍错，仍不能在DWM、观察者、driver、沙箱中定根因，本门无目标显示完成证据。
- H1无／H2无、schema/API失败、截断、ambiguous：保留对应NotObserved/Unsupported/Unknown与具体替代解释，**lost0不推没提交**；结束该有限诊断，不追加Queue/DWM/睡眠/第二个原用例。仅泛系统事件不算目标桥。
- 仅kernel Blit发起但无完成：足以作为本门有限成功与停止，不值得为“必须完整完成链”继续扩provider。若实施审查发现连上述同步桥也不可可靠建立，则在A编译前裁决NotWorthFurtherImplementation，而不是先盲跑原用例。

任何结果都不降低原shader/resize、real-loss/media/最终双架构矩阵条件。采集完或无法实施即以所得证据结束此显示诊断切片；无可验证生产缺陷依据时不猜修生产、不重开paint/owner/capture/格式/DPI。不推外部管理员或从无返回权限归环境。其他主要验收仍按原完整方案另授权，原HW项和完整替换保持Open。
