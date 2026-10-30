# 原用例单次 Present 系统轨迹：唯一下一门（待监督授权）

## 1. 决策与本阶段账

选择 **原 candidate x86 resize 精确用例、原 native 前置单次 Present 的窄 ETL + 同进程 TDH 离线解码**。不重跑 NativePaintBaseline A/B，不重开 owner/override/format/DPI/capture，不改驱动/RDP/CCD，不修生产，不新增微 ABI 验收。此门是最后一次显示诊断；关联不足仍未定并停止，不再接同义报告或延长等待。

本阶段只有阅读/网络文档核验和本报告/三个入口文档更新；源码/项目配置/build/publish/沙箱/测试/Native 新增全部0，无终端、脚本、CLI或再委派。Implementation/Build/Publish/Sandbox=NotStarted，Design=Completed.ReadyForAuthorization。不是运行成功或根因结论。

依据：最新 next-session-handoff；native-paint-baseline-final-closeout 全部60行（首段截断部分由后读40–60补齐，部分中段依报告摘要，不冒原JSON重审）；native-observation-closeout 相关身份、H1/H2、hook、清理段；完整替换方案§1–10及当前执行规则27行；HP/HQ、HR/HT、HO、BX审计与HN/HL报告。历史task原件本阶段没有重新全文复核，以下运行事实为已登记报告引用。全仓未找到AGENTS命名文件；不冒全仓指令内容全文审阅。

## 2. 历史认证审查：不能移绑的连接

|记录|已认证范围|不能绑定最新 task8803fd5b02f84af882ec19bc8c30fa41 的内容|
|---|---|---|
|最新8803…|x86 child4972/owner5116，test947B0DBC/MVIDf21a1c7b，.NET10.0.13；A/B各Clear1/Present1/hr0、00F3F3F3；B真实WM_PAINT；清理成功|adapter0/HAL1、GetPP/creation、HWND/monitor不是完整adapter identity、UMD identity或scanout；候选产品没有调用，actualNative=NotObserved。报告未认证完整D3DADAPTER_IDENTIFIER9、实际UMD路径/SHA或ETW提交链，均记未证，不补旧值|
|87d… native-observation|actual15A545/testA48736C8 x86；owner/test/input desktop、session1、同DC坐标/裁剪合同及DISPLAY1上下文成功|不能将其身份、1920×1080、会话快照移给8803…；未证物理surface/帧；不是ETW提交证据|
|HL/HN|x64 Ex GetAdapterLUID=4E71、GetAdapterMonitor=10001，DXGI Basic Render Driver vendor1414/device8C/同LUID/output DISPLAY1；HN CCD source0/target5527112/HyperVMonitor、Hyper-V Video、WTS session1 Active/console/UNLOCK，API0|基础9只有monitor相符，不冒直接取到基础9 LUID；Basic Render Driver是所枚举DXGI adapter，不自动是候选内部backend。output与会话快照不是持续连接/物理scanout；Oray条目存在不证明影响目标|
|HL/HN 实际模块|x64实际System32 d3d9.dll v10.0.26100.9278/SHA87E157…；dxgi/d3d11/dwmapi分别有路径版本SHA|这些是runtime模块，不是已认证具体用户态显示驱动；不能把d3d9.dll叫UMD。旧x86 QI/栈审计SysWOW64 d3d9 SHA F4757445…也不能移绑最新task。未见已读记录给出本task实际UMD→具体adapter的充分连接|
|BJ/BX/CC/CG|BJ真实多色成功反驳Basic永不产帧；BX backbuffer正确而GDI/front旧色，GDI污染/dup观察混杂；CC/CG否定已测owner/override/后置paint充分性|这些内容成功/失败不认证最新包、原format0单帧或候选内部HW路径；禁止再做同矩阵|
|HP→HQ|HP task40a…真实TDH枚举Dxg711/DWM341描述符；HQ taskcac9…Start/Enable/Open/Process/Stop/Close均0，control1/DWM67三条，lost0，临时SeSystemProfile恢复成功|权限和schema只属于该任务/版本；但已足以否定“沙箱工具没有会话参数所以无法ETW”。HQ不是目标Present关联|
|HR→HT|HR旧task20b…因未ClassInitialize而Present0，不能算目标bridge失败；HT task0c1… x64 PresentEx首红通过，同PID5296/TID1448/HWND9110896，Dxg166在调用QPC区间内，17标量TDH成功，token0/lost0|仅目标Blit提交桥成立，DWM67/501未回调；没有队列完成/目标合成/scanout链，更不是当前基础9 x86失败解释|

更小GetAdapterIdentifier/GetCreationParameters/GetAdapterMonitor或模块枚举只能补身份，无法回答此处HR0后的提交/优化取消边界；因此不把它们另立一个门。作为本次同task身份旁证一次记录即可。普通D3D9不换成Ex以取得LUID；不创建替代device、不用producer成功顶consumer。

## 3. 精确来源与事件白名单

### 已亲取可信代码/官方API

以下GitHub网页正文实际取得；raw.githubusercontent多次404，不用失败抓取冒来源。只参考公开描述符/字段与语义，不运行/安装PresentMon，不复制整套算法，不新增包。

- https://github.com/GameTechDev/PresentMon/blob/main/PresentData/ETW/Microsoft_Windows_D3D9.h ：Intel MIT，头注生成器etw_list public 1b19f39ddb669f7a700a5d0c16cf079943e996d5。D3D9完整descriptor及GUID如下。
- 同路径 Microsoft_Windows_DxgKrnl.h、Microsoft_Windows_Dwm_Core.h：实际返回的生成描述符如下。
- https://github.com/GameTechDev/PresentMon/blob/main/PresentData/PresentMonTraceConsumer.cpp ：亲取HandleD3D9Event、HandleDXGKEvent、HandleDWMEvent相关正文；字段名及层级关联来源。main为可变网页，不是已固定commit，也不是本机manifest认证；实施时把本次参考与**运行机TDH返回的原始schema**保存，运行机为最终结构权威。不猜新版本偏移。
- Microsoft Learn：StartTraceW、EVENT_TRACE_PROPERTIES、TdhGetManifestEventInformation官方页面已亲取。其余声明复用仓库HP经SDK10.0.26100.0核对并实际运行的调用；实施新增file-mode字段/布局须对同SDK evntrace.h/evntcons.h/tdh.h核对，不凭本报告杜撰x86 offset。

### 必需白名单（十进制ID；完整descriptor由运行机枚举取得并比对）

D3D9 GUID **783aca0a-790e-4d7f-8451-aa850511c6b9**：1 Present_Start/v0/channel16/L0/opcode1/task1，2 Present_Stop/v0/channel16/L0/opcode2/task1；keyword均8000000000000002。Start字段 **pSwapchain**（注意大小写，不写pSwapChain）、Flags；Stop字段Result。可信consumer按uint64读取pSwapchain、uint32读取Flags/Result；实际仍须TDH验证类型和长度，不能按本进程IntPtr.Size截断事件字段。

DxgKrnl GUID **802ec45a-1e99-4b83-9920-87c98277ba9d**，channel17：

|事件ID/版本|Level/opcode/task/keyword|必要字段与判别层|
|---|---|---|
|166 Blit/v0|4/0/103/4000000008000001|hwnd、bSubmit、bRedirectedPresent、PresentHistoryToken、allocation/rect；目标提交入口，不是完成|
|501 BlitCancel/v0|4/0/309/同上|hwnd；可信consumer解释为blit优化消除，不等于驱动故障/物理掉帧|
|184 Present_Info/v1|0/0/107/同上|hWindow；kernel Present尾端，不是目标显示完成|
|171 PresentHistory_Start/v2|0/1/6/同上|Token、Model、TokenData；与同线程在途Present桥接|
|172 PresentHistory_Info/v2|0/0/6/同上|Token；与Start相同token可证交接ready边界，不等于显示|
|178 QueuePacket_Start/v1|0/1/9/4000000000000001|hContext、SubmitSequence、PacketType、bPresent；同线程调用区间候选提交|
|180 QueuePacket_Stop/v1|0/2/9/同上|hContext、SubmitSequence；须双键精确相等，不能只按时间/sequence推完成|

Dwm-Core GUID **9e9bba3c-2e38-40cb-99f4-9e8281425164**，channel16：64 GetPresentHistory_Info/v0/L5/op0/task63/K8000000000000001；15 SCHEDULE_PRESENT_Start/v0/L4/op1/task13/K8000000000000080；67 CheckDeviceState/v0/L4/K8000000000000001（HP实际TDH来源，字段hwnd/hr，完整opcode/task由枚举保留）。DWM64/15是全局/线程阶段，**没有目标token/surface桥接不得标记目标已合成**；67精确HWND/hr仅状态观察。

不启用DWM43–45矩形、91匿名surface、196 surface LUID、VSync17等来硬凑目标合成/扫描链。特别是FlipChain 69/70/101在可信代码中标“manually added”、keyword0，本门不采不猜；它们不能与HP中的其他PresentHistory编号混用。不新增Win32k/完整PresentMon状态机。若必要schema不存在或字段类型不符，在同一次任务内落盘并停止，不开一个独立metadata门再接力。

## 4. 最小实现范围、原hook与成本

仅正式 Tests/WpfGfxShape.ComAcceptance：

1. 新增 `D3DImageTests.NativePresentSystemTrace.cs`（默认关闭NATIVE_PRESENT_SYSTEM_TRACE）：内部scope、文件session/离线reader、固定白名单和TDH标量读取、身份/清理报告。复用HP现有TDH边界检查/Descriptor/PropertyDescriptor/原型，不再造通用ETW框架。
2. `D3DImageTests.VisualCache.cs`：仅在resizeWindow && !official的原native前置外围建立scope，try/finally包围现有PresentNativeControl→DiagnoseNativePresent→AssertCombinedPixel；无catch绕过断言，原异常保留。scope在该区域finally停止采集/离线解码；成功则原测试自然继续shader/resize，不跳前置、不把诊断成功当产品成功。
3. `D3DImageTests.SwapChainObservation.cs`：实际slot17/121调用前后仅内存QPC/PID/TID/窗口/device标记，result原样断言；Clear、NULL rect、window override、参数、线程、CompleteNativePresent全不变。scope只激活原目标首次调用，拒绝第二次arm。不要用旧NativeObservation同DC/owner Invoke/screen采样hook；不启用NATIVE_OBSERVATION_CONTRACT/HP_GRAPHICS_TRACE/NATIVE_PAINT_BASELINE。
4. csproj增加一个非默认诊断配置NativePresentSystemTraceDiagnostic，Optimize、独有obj/bin与10.0.13 framework pin沿既有诊断惯例，RID win-x86，显式复用15A545 Native；AcceptanceResources.resx仅必要新增诊断错误文本。正常Debug/Release不定义新符号，TFM/C#版本/包不动。

已有HP collector约426行但硬编码x64：TraceProperties120、LogFile448/callback424、EventRecord112/UserData96及Assert IntPtr8，**不能直接在x86复用**。选择ETL文件模式→停止后同进程OpenTrace(file)/ProcessTrace(TDH)，去掉实时consumer/stopper竞争和同步callback写盘；不是复用旧x64执行结果冒x86 ABI。预计新增约350–500行C#，原hook约20行、配置/资源小改；超过约550行净新增、需要struct/array/map解码或新增状态机即停止实施并报不适配预算，不转大平台。layout核对属于实施同一工作，不新增运行门；无可信x86 SDK布局则不publish运行，不降到猜偏移。

运行前以SDK声明建立x86自然对齐结构（WNODE union只取最大成员，TRACEHANDLE永远ulong，BOOL32/BOOLEAN8，指针字段IntPtr）；用Marshal.SizeOf/OffsetOf核已核SDK值，且必须由实际Start/Enable/ETL消费/TDH结果认证，单布局断言不够。模块枚举可用现有Process.Modules/.NET FileVersionInfo/SHA256；原factory GetAdapterIdentifier(0,0)与GetAdapterMonitor读身份，无新D3D对象。Driver字符串是报告身份，不等于该文件实际加载；保留**全部实际进程模块**路径/base/size/version/SHA，不能靠名字筛选声称找到UMD。未有明确module↔adapter连接仍UMD attribution=Unknown。只读原scope session/monitor，复用既有CCD只读辅助若可直接调用；不重做全环境扫描，不配置显示。

## 5. 一次任务的精确API顺序与原始证据

1. 现有正式exe/原MSTest ClassInitialize和owner照常；新scope在原native前置前创建，仅首次目标arm。先写任务/test SHA/MVID/runtime/OS/arch/QPC频率/native实际加载身份；保存本进程session、HWND owner PID/TID、实际D3D creation/swapchain/adapter报告和模块快照。身份查询不得改变device。互不原子如实标注。
2. `TdhEnumerateProviders`按名称核三个GUID；`TdhEnumerateManifestProviderEvents`→所选实际descriptor→`TdhGetManifestEventInformation`（sizing122/read0）保存原始TRACE_EVENT_INFO二进制和可读字段表/来源模块版本。校验完整descriptor/必要字段存在、非struct/array/param-count、预期长度类型；不同版本只保存并UnsupportedSchema停止本门，不用旧offset解析。
3. `OpenProcessToken(QUERY|ADJUST_PRIVILEGES)`→LookupPrivilegeValue(SeSystemProfilePrivilege)→AdjustTokenPrivileges，保存previous；BOOL成功仍检查ERROR_NOT_ALL_ASSIGNED。EventAccessQuery支持234/122再读，ACL可读不冒Enable权限。当前API失败只记对应status，不提前归沙箱或缺权限。不开SYSTEM、不改ACL/组/注册表。
4. `StartTraceW`创建且仅创建一会话：描述性名称WpfGfxShape-OriginalPresent-<task标识>，独有绝对Results路径original-present.etl；GUID_NULL让ETW生成会话GUID，ClientContext1/QPC，WNODE_FLAG_TRACED_GUID。FILE_MODE_SEQUENTIAL=1，MaximumFileSize16MiB，BufferSize64KiB、min4/max16（实际ETW可按CPU调整，记录返回，不谎称硬内存上限），FlushTimer0。不用NT Kernel Logger/SystemTraceControlGuid/SYSTEM_LOGGER_MODE，不改其他会话。183冲突即停止，不能stop冲突者或改名重试。
5. `EnableTraceEx2` Version2、EVENT_FILTER_EVENT_ID（0x80000200），逐provider精确白名单；Dxg any=4000000008000001/L4、DWM any=8000000000000081/L5、D3D9 any=8000000000000002/L0，all=0。后端先启Dxg/DWM，最后D3D9；任一失败无全开fallback。当前环境是否需额外特权/系统会话由Start/Enable真实status认证；**不因kernel provider名字强上系统logger**。失败清理自建会话即停，无第二会话升级实验。
6. 同会话启用既有manifest EventSource控制marker，仅id1；写唯一开始/结束QPC marker（自有固定Int64合同可按自有schema解码，不冒TDH成功）。用同个独立计时控制线程保证从Start起2秒请求STOP，原区域正常完成/断言失败则立即请求STOP，禁止等目标事件或颜色延长窗口。线程不执行Present、不改owner。Stop API自身可能阻塞，不声称2秒硬完成，独立4秒join及监督120秒/外180秒保底；无二次采集。
7. 原Clear和一次原Present照常，调用点前后内存记录QPC/PID/TID/参数/result；不在调用之间写盘/查询/同步callback。原Complete/Diagnose/单点断言照常。scope finally无论断言如何先请求停止，保全主异常，逐项记录诊断/cleanup错误。
8. 唯一controller `ControlTraceW(QUERY)`保存本会话统计→`ControlTraceW(STOP)`记录请求/完成QPC/status、EventsLost/LogBuffersLost/RealTimeBuffersLost/BuffersWritten。计时线程与finally用原子所有权保证STOP不并发、不重复释放。仅成功Start所得handle/name/GUID；QUERY不能确认自建身份则不碰其他session。STOP成功后按名称QUERY应4201（not found），原status保留；若其他失败记cleanup未证，禁止用空列表冒成功。令牌previous恢复/CloseHandle在finally，借用句柄不关闭。
9. ETL关闭后保存长度/SHA，再 `OpenTraceW` 设置LogFileName、EVENT_RECORD|RAW_TIMESTAMP（不设REAL_TIME），`ProcessTrace`一文件离线读取→`CloseTrace`。LogFile/EventRecord按**x86 SDK**，delegate保持活跃，不跨native抛异常。回调先复制header/extended data/payload成托管记录；不能只存UserData指针等待回调后用。TDH可在回调内调用但本门采集已停止，不干扰Present。TdhGetEventInformation→TdhGetPropertySize→TdhGetProperty，按实际header/version/flags验证字段；未知结构仅原始保留，不猜解码。
10. 单文件≤16MiB；离线≤8192事件/8MiB payload/16MiB JSON、5秒处理预算，超界标记Truncated，保留原ETL且不继续推完整性。写manifest.json、controller.jsonl、events.jsonl（完整provider/descriptor/header flags/PID/TID/QPC/ActivityId、原payload/字段status）、correlation.json、cleanup.json、原TRX/监督/stdoutstderr。原ETL优先于摘要，不因解码失败删除。原生指针仅ignored原件内关联，tracked报告不粘整地址。

不启动logman/wpr/PresentMon/PowerShell/cmd或新工具；上述全由同一正式C# exe中的系统API完成。TDH离线解码是本次同task收尾，不要求新工具/新任务。若文件模式Start失败，不自动回退实时；如实记本门采证失败并收敛。

## 6. 关联、负控与裁决

- 原调用标记只表示测试确实调用；D3D9 Start/Stop在同PID/TID及调用区间形成单一配对，保存pSwapchain/Flags/Result。ETW的runtime内部swapchain值不自动等于GetSwapChain公开COM指针，不硬断言二者相等；HWND由原调用记录及Dxg166/184独立桥接。
- Dxg166：同PID/TID/精确HWND且QPC在原调用区间，才称目标kernel blit提交入口；501若同目标/线程/区间可称优化取消观察，不能叫丢帧原因。跨线程仅时间邻近一律Unlinked，不套用“进程唯一所以肯定同帧”。
- Queue178在该在途Present中且bPresent等合同成立，随后180的(hContext,SubmitSequence)精确相等才称队列完成；多候选/键复用歧义不挑最近。token171→172非零精确相等才称history交接，Token0不能链接。
- DWM67必须hwnd精确匹配；DWM64/15仅保存全局阶段，不赋目标完成。此窄门**不承诺得到目标合成/丢弃全链，更不承诺物理scanout**。PresentStart/Stop、队列完成、交接、DWM合成、显示硬件扫描五层分别记Observed/Unlinked/NotObserved。
- 自有marker验证记录/时钟/消费；事件在STOP落盘后才出现不是丢失。控制marker缺失、任何lost>0、schema/TDH失败、超预算均使否定性结论无效。即使lost0也不能由无事件推没提交（provider发射/过滤/版本/时间窗均有边界）。
- 同体内纯托管负控：给关联器复制记录改PID、TID、HWND、QPC越界、token/context/sequence错值，均不得命中；截短payload/未知version不得固定偏移解码；loss>0必须降级。无额外真实Present、无伪系统事件混入原始事件表；synthetic另文件标识，不冒新测试体。仅证明关联器，不证明真实driver行为。
- 新增判别力：8803只有API0/paint/像素；HT只x64成功Ex + Blit166。此门首次在**当前原candidate x86失败前置**连接runtime Start/Stop→精确HWND kernel提交/优化取消→可匹配队列/history边界，并保存原生ETL。不是重跑HT首红或观察者变体。不能据此通过产品内容/resize；若原前置成功，后续产品断言仍原样执行并独立登记。

## 7. 监督可直接授权的执行包与退出策略

授权范围严格按§4四处源码/资源/配置，禁止其他生产修改；**normal COM build≤1、COM publish≤1、Windows沙箱≤1、原测试体≤1、Native0**，独立发现0、official0、额外x64/full0、外部CLI0、重试0。复用postfailure-source-rebuild-x86-r1/native/wpfgfx_cor3.dll（15A545完整SHA见原closeout），不重发候选。build失败即停止，不修后再build；publish失败亦停，不运行旧包。

唯一配置NativePresentSystemTraceDiagnostic/win-x86/net10.0-windows，自包含10.0.13，ComAcceptanceSandbox=true，独有artifacts/original-present-system-trace-x86/{obj,bin,publish,clean,readback}，显式AotDllPath指向既存原件，正式PackageSelfContained目标核clean。工具完整可见返回即时保存，超长截断边界如实登记。核test SHA/MVID/runtime/native SHA、原FQN存在及默认符号隔离，不新增发现调用。

唯一正式exe参数沿既有监督合同：--supervise-exit、Results/original-present-system-trace-x86、120、FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive；外180秒，整任务回收，上传clean而非源码父目录。诊断默认配置不是最终Release矩阵认证；不因此追加默认build挤预算。

停止标准：schema/权限/会话/过滤/布局/文件/清理首错即本门终止；原业务断言失败保留主失败，仍完成自建session finally和原件收尾。没有可靠目标链则结论Unknown，不归沙箱/驱动/权限根因，不要求外部维护者，不再延时、加provider、换API/颜色/格式或新增微门。即便出现目标queue完成且像素错，也只收窄到后续消费/观察边界，不作“GDI错面已证”或驱动定因。存在明确系统状态事件也只按其公开合同解释，不自动改系统设置。

**若此门仍不能分辨，停止这一显示诊断路线，显示内容根因保持未定；不再提议下一报告接力。** 只有新形成可验证产品缺陷依据，才另授权产品修复及原最终矩阵，不把未定当通过。保留8803方向Closed.Negative、paint累计build3/publish2/沙箱2/体2，87d独立build1/publish1/沙箱1/体1及全部历史预算；双架构候选有限通过不撤销、不移绑；显示内容未定、完整替换Open。
