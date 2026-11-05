# HP 纯 C# 有界 DWM/DxgKrnl 事件采集可行性

## HQ终态补记：一次短会话实证，目标关联仍开放
本节由HQ授权补入同一HP报告。以下原HP阶段1全文作为历史保留；其中“未执行Start/Enable/消费”描述仅适用于原task40a30d3916c44149a9df78beb22bfe50，不再代表HQ终态。HO报告与其handoff节未覆盖。

### 实际执行、身份与包
仅新增HP条件内 HpGraphicsTraceTests.Control.cs，原文件改partial及ACL读取接受234/122；生产修复0。未新增包/项目/Host，不改生产COM、公共捕获、VisualCache/historical/DRDS、NativeRun/ExitDiagnostics、项目打包或CF/CX/CY。无脚本/终端/WPR/logman，无渲染/HM/consumer/full/Event0运行。
正式ComAcceptance Release/win-x64自包含10.0.13，发布1次成功（正常分析器，ErrorsOnly只限制输出，不禁规则）；源码预运行核正消费模式0x10001100及manifest控制EventSource，无失败部署或重试采集。独有 artifacts/hq-graphics-trace/{obj,bin,publish,com-acceptance-package,readback}，显式绝对PublishDir；仅clean白名单package上传，输入/回收分离。实际C#集合expected419/actual419/missing0/extra0；Contracts包含wpfgfx.def和wgx_sdk_version.h；实际Native SHA与aot.sha256一致，HA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0仅包依赖未加载。
离线API预检新方法存在；唯一有效采集task cac9e239b5e44fc28f4839a3e07ac7b4，PID6548/session1，UTC2026-10-23T12:20:22.0851856Z，QPC35790988894836/frequency10000000。actualtest SHA A2DB4D3BDB6C32EF68BA5B6127B7D5C221496842FEB209B52C1E669C9C249637，MVID980e70c6-6df1-4ed7-93fd-0318449eccf0，.NET10.0.13/X64。
绝对TRX C:\CodingAgentSandbox\Tasks\cac9e239b5e44fc28f4839a3e07ac7b4\TestResults\hq-control.trx，26行全文亲读：total1/executed1/passed1/failed0/notExecuted0/timeout0/aborted0，测试duration1.8237005秒，工具整轮2.136秒。直接MTP测试，无新增监督角色/父TRX；工具未给独立数值进程Exit，不伪填Exit0。发布独立exit0与测试进程Exit不是一层。

### SDK、ACL与临时权限
亲读本机Windows SDK10.0.26100.0 shared evntrace.h/evntprov.h/wmistr.h及um evntcons.h对应声明：WNODE union/history/timestamp48，properties age/flush union与HANDLE对齐120，ENABLE_TRACE_PARAMETERS含GUID/指针48，filter descriptor16，EVENT_FILTER_EVENT_ID为BOOLEAN/UCHAR/USHORT/USHORT数组，LogFile mode/callback union及嵌入EVENT_TRACE/TRACE_LOGFILE_HEADER推导x64总448/callback424；EVENT_HEADER80、EVENT_RECORD112/UserData96/length86；实际大小/offset日志一致并实际回调验证。不把不同union分支相加，TRACEHANDLE为ulong，BOOL32、BOOLEAN8、status ULONG32。
DWM与Dxg EventAccessQuery sizing均234/292bytes，正式读取均status0并取得完整SDDL：O:BAG:BAD:(A;;0x1800;;;WD)(A;;0x120fff;;;SY)(A;;0x120fff;;;LS)(A;;0x120fff;;;NS)(A;;0x120fff;;;BA)(A;;0xee5;;;LU)(A;;LC;;;MU)(A;;0x1800;;;AC)(A;;0x1800;;;S-1-15-3-1024-3153509613-960666767-3724611135-2725662640-12138253-543910227-1950414635-4190290187)。ACL可读不等于enable权限，后者另由实际Enable证明；未执行AccessCheck或修改ACL。
OpenProcessToken成功/error0（仅本进程QUERY|ADJUST_PRIVILEGES）；LookupPrivilegeValue成功/error0，SystemProfile LUID11。AdjustTokenPrivileges成功/error0/returned16，保存previous Count1/attributes0；finally恢复成功/error0，CloseToken成功。没有SYSTEM提权或全局配置/权限更改；不外推普通用户也可enable。
本task TDH再次核DWM67与Dxg166/501 v0：sizing122，读取0，大小548/3194/398；它们仍是schema检查，不是运行事件计数。

### Controller/consumer实际status与原始内容
唯一session HQ-1c332e40a1944003bf5adbe0bae7730a，handle32，StartTrace0，QPC35790989649017。实时无ETL文件，64KiB buffer/min2/max4；事件上限2048/payload2MiB/输出4MiB。1.7秒定时请求结束，controllerEnd实际1.7160697秒；该时间在Stop调用前记录，不能当成精确Stop完成QPC或严格调度最坏界证明。测试总长不足2秒，整MTP启动开销不属于采集时间。
EnableTraceEx2三次均0，Version2/一条EVENT_ID filter，无filter回退：自有4163cfa5-c286-510a-16e4-f9816997cfb4仅id1/L5；DWM仅67/L4/K8000000000000001；Dxg仅166/501/L4/K4000000008000001。只有控制provider keyword全位，不是全图形泛采。OpenTrace handle257/error0，真实消费线程ProcessTrace0；StopTrace0，EventsLost/LogBuffersLost/RealTimeBuffersLost均0，BuffersWritten4；CloseTrace0，joined=true。finally仅停止本任务成功创建的命名session，权限恢复与缓冲释放完成，无其他session操作。未另行QUERY停止状态，停止证据为STOP成功及ProcessTrace退出，不冒二次查询。
controllerEnd时controlObserved=false，事件在STOP排空阶段才送达；最终callbacks4/control1/graphics3（DWM67三条，Dxg0）/TDH metadata decoded3/callbackErrors0/budget0/payload44bytes。不能把启动成功当消费成功，本次有实际回调原件证明消费，但不是低延迟实时交付保证。
自有控制事件：PID6548/TID3880/ID1/v0，QPC35790989778763，原始payload DA5A3B3D8D200000，按自有固定Int64合同还原35790989777626，与写入marker完全相等。TdhGetEventInformation对该自有provider sizing1168 ERROR_NOT_FOUND，read未调用（4294967295为哨兵，不是实际API返回）；所以这是已知控制合同解码，不宣称自有TDH解码成功。
三条DWM67：PID1312/TID1340/v0，QPC35790991281931、35790997568704、35791002611901；各12bytes原始payload 000000000000000001007A08。TDH sizing122/read0，真实字段hwnd input10/output19/count1/length8、hr input8/output18/count1/length4，flags0。按该平面schema与原始字节可读hwnd0/hr0x087A0001；诊断尚未调用TdhGetProperty逐字段取值，decoded3仅表示TDH事件元数据成功，不冒完整动态payload解码器。hwnd0不是本任务呈现目标，未创建目标。Dxg0不证明没有系统提交，也不扩大采集找事件。

### 已证与未证、下一唯一目标
已证：此管理员令牌临时权限范围内可Start、窄filter enable、Open/Process实际回调、自有payload吻合、DWM事件TDH元数据可读、Stop/Close与线程收敛。未证：目标Present/token/surface桥接、目标消费/occlusion原因、adapter LUID/source/target/scanout；无拒绝API，当前没有新增外部权限要求。
下一唯一显示采证任务（须另轮授权）：保持原固定呈现合同，仅一次首红，与最多2秒窄ID trace同task记录PID/TID/HWND、LUID/source/target及提交前后QPC；先完善TdhGetProperty及已知控制provider TDH元数据缺失处理，并使超时STOP独立于setup等待、记录STOP完成QPC。若没有目标hwnd/token或surface桥接立即交接关联不足，不追加系统全量trace或随机重试；仍occluded即停余色/基础9/consumer/full。成功也只支持关联范围，不承诺取得内部原因/scanout。具体替代路线保留HO一次成对有/无duplication加独立GDI两内容标尺，不在HQ执行。
HO双轨推荐写入三共享终态：显示调查与非显示解耦；静态协议/经合法性确认的无surface控制、ABI/通知/媒体生命周期、托管Tests及full准备可独立推进。共享原颜色门及旧失败必须保留，仍约束旧内容验收，但不是全仓开工门；无surface/事件成功不得替代surface内容。
最后源码后default正常Release/win-x64 build98 warnings/0 errors/3.00秒，默认DLL公开类型HpGraphics为0；显式诊断隔离成立。生产修复0，HA/fill/HDHEHF/HJ历史保留，Available/HW恢复/backend/loss/media/resources/full四格/x64Event0仍开放，完整替换未完成。

### 亲读范围登记
HQ业务control.jsonl33行与TRX26行全部亲读；identity/runtime/ACL/权限/六类API/controller/回调原始payload/TDH字段/cleanup均已读。包runtimeconfig/aot全文，419项由实际C#集合核验，不宣称人工逐项hash。HP原报告通过分段补读含末节，HO全报告分段读取但40–86首次返回末尾被截断后已补读78–116；原HP自有handoff终态段已读。历史证据范围仍按原HP/HO登记，不宣称重新审计历史task原件。SDK只核本次相关定义，未宣称全SDK/所有版本认证。


## 结论与范围
纯 C# 调用系统 TDH 在本机确实可行，不需要第三 Host、WPR/logman、脚本或新增包。已实际读取两个 graphics provider、keyword/level 及 1052 个 manifest 事件描述符，得到可用于下一轮设计的真实 HWND/token/Present/source/target/QPC 字段。不能再以“没有现成采集工具”认定做不了。

但本轮只有只读 metadata/令牌探测：StartTrace、EnableTraceEx2、OpenTrace、ProcessTrace、运行事件消费、payload 解码、目标关联全部未执行；没有证明已能短采集，更没有证明 HM occlusion 原因。metadata 是 schema，不是发生过的事件。交付为阶段1实证及下一次实施边界，不是完成采证或完整替换。

## 修改与隔离
仅新增 Tests/WpfGfxShape.ComAcceptance/HpGraphicsTraceTests.cs（整个文件受 HP_GRAPHICS_TRACE 条件保护）、本报告、next-session-handoff.md 自有 HP 短节。生产修复0，现有 HA/fill 等修复保留；未动 NativeRun/ExitDiagnostics、共享consumer/VisualCache/historical/DRDS、公共窗口捕获、项目/打包/版本、CF/CX/CY。未新增项目/包。HO内容保留。

现行 net10.0-windows、nullable/unsafe 开启、MSTest4.0.1/MTP，项目 global.json SDK10.0.107/latestFeature。正式项目仅 MSTest 直接 PackageReference；未发现现有 Tests 中 TraceEvent/TDH/StartTrace 实现，故直接调用 tdh.dll/advapi32.dll，未安装 TraceEvent。未以“查不到现有库”取代系统API实测。

## 实际发布与执行
发布调用4次：前三次编译失败，完整初始warning日志输出被工具截断，第三次 ErrorsOnly 定位 CA1838 StringBuilder PInvoke；修为 UTF16 char[]，第四次正常分析器发布成功，无 NoWarn/禁规则/skip/fallback。ErrorsOnly仅改变日志显示，不关闭分析器。没有失败包上传或渲染补跑。

自包含 Release/win-x64，Microsoft.NETCore.App 与 WindowsDesktop.App 均10.0.13；独有 artifacts/hp-graphics-trace/obj、bin、publish、com-acceptance-package。显式绝对 PublishDir，正式原包目标产生 whitelist，小包 sourceDirectory 只选 com-acceptance-package，不上传publish或历史父目录。Native来自HN留存 HA SHA 8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0，仅满足打包依赖，HP未消费，不重新发布生产。

离线 GetDotNetTypeApi 确认唯一方法 HpGraphicsTraceTests.WhenGraphicsMetadataIsQueriedThenActualApiStatusIsRecorded 存在；在同包内正式MTP精确filter执行一次。Task 40a30d3916c44149a9df78beb22bfe50，PID5808/session1，UTC2026-10-23T12:07:27.2957372Z，QPC35783241000343/frequency10000000。
actualtest SHA 8CE653825CF711A777089B6FC97B02A7B78952D473205D1AEF5AFC2873DB89C4，MVID af664ba1-66a0-426f-8eb3-6097a342c13c，.NET10.0.13/X64，OS API报告10.0.26200.0；不能把HN的26100系统模块身份移植到本task。

实际包预检expected419/actual419，missing/extra均空，断言逐项集合相等。runtimeconfig/aot亲读；白名单仅首28行人工阅读，全部集合由实际C#比较，不冒人工419行逐读或全部文件hash。绝对TRX C:\CodingAgentSandbox\Tasks\40a30d3916c44149a9df78beb22bfe50\TestResults\hp-metadata.trx，26行全文亲读：total1/executed1/passed1/failed0/notExecuted0/timeout0/aborted0，duration约1.039秒。这是metadata诊断通过，不是图形显示通过。专用执行返回成功文本，未给独立数值exit，不伪填Exit0；无监督父TRX、无自造TRX。全部task回收至 artifacts/hp-graphics-trace/readback；业务 Results/hp-graphics-trace/probe.jsonl 与源包分离。

## API status、签名与权限
TdhEnumerateProviders sizing122/103622bytes，正式read0。枚举返回的名称匹配后才使用GUID，不凭记忆硬编码：
- Microsoft-Windows-DxgKrnl：802ec45a-1e99-4b83-9920-87c98277ba9d，schemaSource0；keyword1884bytes/level204bytes，sizing122/read0；manifest11384bytes，711描述符，711解码0错误。
- Microsoft-Windows-Dwm-Core：9e9bba3c-2e38-40cb-99f4-9e8281425164，schemaSource0；keyword1006bytes/level160bytes，sizing122/read0；manifest5464bytes，341描述符，341解码0错误。

关键词筛选仅决定保存schema条目，不是事件filter：Dxg选86保存60省26；DWM选93保存60省33。无运行事件计数。每缓冲16MiB硬限、事件描述符上限4096，越界/无终止符显式异常。

全部TDH/EventAccessQuery直接返回 ULONG status，不读LastError；GetTokenInformation/LookupPrivilegeNameW为BOOL32且SetLastError，Unicode W/char[]；Guid16、EVENT_DESCRIPTOR16（ushort/4个byte/ushort/ulong），provider表8字节头+24字节项，field表8字节头+16字节项；TRACE_EVENT_INFO112字节头，EVENT_PROPERTY_INFO24字节，union仅记录原始uint16，不将struct union误解释为固定类型。按返回缓冲边界验证UTF16偏移及终止符。TOKEN_PRIVILEGES头4/LUID_AND_ATTRIBUTES12。真实TDH名称、descriptor及相互一致的字段作为可用性证据，布局大小不是唯一依据。

令牌administratorEnabled=true；GetTokenInformation成功/error0，sizingError122；LookupPrivilegeName各项成功。SeSystemProfilePrivilege存在attributes0（未启用），SeDebugPrivilege2；本轮未 AdjustTokenPrivileges，也没改变令牌或系统权限。管理员组已启用不等于StartTrace/EnableTrace必成功，且不是普通非管理员访问测试。

两个 EventAccessQuery(NULL)均234 ERROR_MORE_DATA/292bytes。当前探测实现只在122时继续读，因此ACL未读、未AccessCheck；这是探测分支不足，不是访问拒绝，不是provider不可用。保留实际SHA对应源码，不在唯一task后悄改代码再冒同产物已验；下一轮须支持234再读取292字节SDDL。没有观察到ERROR_ACCESS_DENIED，不能要求用户先提升到SYSTEM或修改ACL。是否需额外管理员/Performance Log Users/provider授权/系统logger权限仍待具体StartTrace与EnableTrace结果裁决；不以通用规则替代本机实证。

cleanup记录sessionsCreated0/sessionsStopped0/traceHandlesOpened0/renderingStarted=false；所有已分配TDH/token缓冲finally释放，身份令牌using释放。cleanup行在provider finally之前写，随后正常测试完成说明执行了finally；不是创建会话后的stop确认。未启动/停止别人的session，未注册provider/改注册表/显示设置，未采用户运行事件或长期GPU数据。

## 真实字段能回答什么
以下id/version/level/keyword均由本task TDH返回，不是跨Windows固定承诺，下一次使用前须重核相同descriptor：

| Provider/event | 本机descriptor | schema真实字段及边界 |
|---|---|---|
| Dxg Blit |166/v0/L4/K4000000008000001|hwnd、PresentHistoryToken、pDmaBuffer、bSubmit、bRedirectedPresent、allocation及rect；有目标关联入口，但未证HM会发出此事件|
| Dxg BlitCancel |501/v0/L4/同上|hwnd；可查目标cancel，不单独说明原因|
| Dxg VSyncDPC |17/v0/L0/K4000000008000001|pDxgAdapter、VidPnSourceId、VidPnTargetId、FrameQPCTime、FlipFenceId；指针不是adapter LUID，仍需桥接，不能与HN LUID4E71直接相等|
| DWM CheckDeviceState |67/v0/L4/K8000000000000001|hwnd、hr；可能直接记录DWM侧目标状态，不能预设一定触发|
| DWM Occlusion |43/44/45 v0，L4/5/4，K8000000000000001|首尾data/Id，中间Z/rect/rectType；没有明确HWND或原因文本，不把data冒HWND或Id冒目标Present|
| DWM PresentHistory |64/65 v0 L5 K8000000000000001|hr/cTokenCount与总token/iteration计数；是全局/线程处理字段，非单目标已消费证明|
| DWM scheduling BLT |91/v0/L4/K8000000000000080|hLogicalSurface/hDxSharedSurface/EventId/dirty rect，缺直接HWND；必须另证surface桥接|

DWM Composition=1、Scheduling=80，Dxg Present=08000000、Base=1；原始descriptor还含高位channel keyword，不能忽略其语义直接用全开FFFFFFFF。DWM 196的luidSurface是surface结构，不可偷换为adapter LUID。即使捕获上述schema对应事件，仍需证明确实来自目标PID/TID/HWND并有token/surface关联；缺桥接只能报告时间相关。Occlusion矩形不自动给出内部拒绝原因，VSync也不证明目标帧scanout。

## 下一唯一可执行采证任务（本轮未执行）
在同一正式项目HP显式条件中，先完善234安全描述符读取并核Windows SDK原型/layout，做一次最多2秒的独有会话控制与真实消费验证；若权限/结构/filter能力不确定，停止在具体API/status，不启动渲染。该任务只验证trace链，不为验证trace又重跑红帧。成功后才允许另轮按原合同一次原呈现场景关联；不是本轮追加渲染，也不是全仓开工门，HO其它轨道不受强制ETW限制。

最小collector而非ETW平台：StartTraceW(out ulong TRACEHANDLE, Unicode唯一HP-随机GUID名, EVENT_TRACE_PROPERTIES)；Wnode.ClientContext=QPC，Wnode.BufferSize包含名称buffer，先核x64 WNODE_HEADER48、EVENT_TRACE_PROPERTIES120及指针对齐。EnableTraceEx2使用ref GUID/ulong handle/byte level/ulong keyword与正确ENABLE_TRACE_PARAMETERS/过滤descriptor布局；仅本机TDH上述provider与必要event-id白名单，过滤失败不回退全系统泛采。Dxg是否要求系统logger及DWM是否支持event-id filter必须以实际API返回证明，不能强设kernel/systemwide logger。

优先仅DWM 67/43-45及Dxg166/501，是否增17须有source/target桥接必要性。DWM必须在DWM进程发事件，不能一律按测试PID过滤；因此若不能provider侧event-id过滤或不能限制数据量就停止。每provider数十事件白名单、2秒墙钟cancel、最多2048事件、2MiB保留payload/4MiB文件预算；预算触发立即本任务stop，不丢事件继续跑冒完整轨迹。ETW实时短会话有固定buffer内存，不能把文件写上限冒采集零开销。

OpenTraceW返回64位TRACEHANDLE，INVALID_PROCESSTRACE_HANDLE是全64位-1；PROCESS_TRACE_MODE_EVENT_RECORD/RAW_TIMESTAMP模式与EVENT_TRACE_LOGFILEW内union必须按SDK核对，callback持有delegate且不得跨native抛异常。EVENT_RECORD/HEADER及UserDataLength/指针宽度独立验证；ProcessTrace读取专用handle，有界取消由本任务ControlTraceW(STOP)并CloseTrace解除等待，finally等待consumer结束。只关闭本任务已成功创建且身份匹配的session；ERROR_ALREADY_EXISTS重新唯一名或停止，绝不stop他人。记录Start/Enable/Open/Process/Stop/Close各API原始status和lost events，finally之后再次确认本任务停止，拒绝仅“start成功”关门。

payload用TdhGetEventInformation/TdhGetProperty等按实际版本动态读取，只处理必要HWND/token/hr/QPC/source/target，struct/array/map不硬按平面偏移；本轮schema union只是描述，不是解码器。运行期保存实际测试SHA/MVID/runtime/task/时钟及cleanup，再区分session成功、callback事件、解码成功、目标关联四层。无事件不冒0次提交，事件丢失/字段缺失不作根因结论。敏感地址仅任务内关联，不进tracked报告原始指针。

若实际Start/Enable返回5/1314等权限错误，最小外部需求是同环境获授权的ETW session/provider访问令牌（以及该API明确要求的system logger权限），不是安装WPR/logman或新Host。若授权后仍缺字段/桥接，只需该版本manifest/map语义或宿主Hyper-V显示端关联证据；不以笼统“系统问题”关闭产品缺口。当前未观察到这些错误，不预填。

原呈现场景的下一次关联须重新记录同task PID/TID/HWND、monitor、adapter LUID/source/target、QPC start/submission/return/GPU完成。HN历史HWND840570/monitor10001/LUID4E71/source0/target5527112及QPC35771851616325只为历史定位，不绑定HP task或未来窗口。精确首红若仍08760878则立即停止余色/基础9/consumer/full，禁止随机参数或FLIPEX追绿；只有目标事件与surface/token桥接成立才能解释DWM消费/拒绝，scanout另有自己的链。

## 默认隔离、审阅范围与未闭
最后源码后正常default Release/win-x64构建98 warnings/0 errors/1.62秒；离线默认DLL类型搜索HpGraphics为0，HP未默认启用。未修改默认build配置。四次发布只是编译尝试，唯一provider探测task1，session0/渲染0；没有HM正锚/基础9/consumer/full/Event0重跑。

本轮已读执行规则、完整方案、最新handoff相关节、稳定基线、ComAcceptance项目/包目标/入口/SDK/本地构建配置、HN报告全部相关段（工具首段截断与后续节分别读取）。HN原始hi-delivery与TRX未全文复审，引用历史结果仅来自HN已登记报告；没有把历史全部原件宣称亲读。HP业务233行中APIstatus/provider/token/权限/identity/summary/cleanup全读；120个大metadata条目保存于ignored原件，分段读取多处被4000字符上限截断，未宣称233行全文人工审计。核心引用Blit、BlitCancel、VSyncDPC、DWM43-45/64-65/67/87-91已直接读取，summary计数为诊断实算非人工事件计数。TRX26行全读，包runtimeconfig/aot及实际集合检查已读；工具缺独立数值exit如实保留。

Available/HW恢复/backend/loss/media/resources通知/full四格/x64Event0仍开放；HA/HD/HE/HF/HJ历史成功与HL/HM/HN失败保留，不更改裁决。生产修复0，完整替换未完成。
