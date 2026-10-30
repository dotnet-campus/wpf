# HD 官方 HWND consumer 内容链合同诊断

## HF实时登记：official成功，准备同包唯一candidate
唯一official task e0036f3dbf3347bb8891145835f4c9df已回收，监督子Exit0/nonTimeout/noKill/drain正常；TRX1执行1通过0失败。actual官方5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；testC61C02D7A3A6F1B4785920037C8CF0E81C1A18EE51352950D9CE99B2A614A12E/MVIDe37ffa74-0079-4cf6-af2d-c1e0671cc2f2/.NET10.0.13 X86 ptr4。native同任务fresh及consumer三色255/16711680/65280均accepted，flags12/context-2/DPI1/1，三事件true，detach/四删除/cleanup/四refs0已亲读。剩余阶段/TRX截断处继续补读，不追加official；现在仅同一包candidate一次，不重编译。

### HF发布留痕
零源码修改，正常分析器win-x86 Release self-contained .NET/WPF10.0.13正式发布成功，独有artifacts/hf-x86-consumer/obj/bin/publish及com-acceptance-package，显式PublishDir。仅DefineConstants=HB_SURFACE_CONSUMER；正式包离线公开API核得两个准确方法，源码分别RunHbConsumer(true/false)。Contracts、MTP、runtimeconfig与418行package-files清单首尾已核，aot.sha256=75D136F71223DB9EA0830BFBF9CDFC0EAAB748B5067397B682E42323B698EC30，复用HA Native不重建生产。上传源仅新clean com-acceptance-package，readback分离，不上传PublishDir或历史父目录。实际包文件字节可直接取得，未另造清单。当前task0；即将official一次，成功才candidate一次。120秒filtered/外180秒，无变参追Green。

## HE实时历史登记（已完成；以末尾HE终态为准）
下列进行中/尚未调用描述是执行阶段留痕，非当前状态；唯一任务现已完成且核心原件核证结束，见末尾HE终态。
唯一task333b412deb784bc3bdea920f2a519ba7已整task回收。监督39行亲读：子PID836 Exit0/nonTimeout/noKill，test SHA/MVID与HD完全相同；业务77行与native60行已分段亲读，候选actual SHA匹配，红255/蓝16711680/绿65280全等、三事件true、四refs0/cleanup完成。继续核TRX/stages/stdout/stderr及官方旧原件；不追加任务。
同HD正式com-acceptance-package复用，aot.sha256/Native/Contracts/self-contained runtime10.0.13已核；实际包DLL公开API可发现WhenHbCandidateSurfaceMutatesThenConsumerWindowMustChange，源码[TestMethod]直接RunHbConsumer(false)。同exe --supervise-exit过滤角色，内部120秒/外180秒；同参数仅换candidate filter，独有回收artifacts/he-candidate-consumer/readback/x64-candidate整task。零源码修改、不重build/重发布。HD官方旧监督原件39行已亲读，其余原件继续分段核证。唯一candidate任务尚未调用；前置失败早停，不重跑。

## 状态：HD 发布完成，即将唯一 official x64 正式任务
正常分析器首发被CA2219拒绝（finally内多余throw表达式），复用既有DestroyWindow owner封装消除根因后发布成功；未关规则。最终发布日志58–60、package aot.sha256、runtimeconfig全文、Contracts及清单头已读。self-contained .NET/WPF10.0.13；候选包SHA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0，仅包非actual消费。独有obj/bin/publish/package在artifacts/hd-hwnd-contract；只上传com-acceptance-package，回收另存readback。至此有效task0，即将消耗唯一official预算。
先完成官方调用方与 HB 显式夹具的聚焦合同对照；一次性修正已证输入缺项，再仅运行一次 official x64。生产及 HA Stdcall 修复不改，不重发候选，不扩 candidate/x86/full。HB/HC 历史不覆盖。

已读 current-execution-rules 全文、完整方案1–120（中段需分段补读以排除返回截断）、latesthandoff 最新HC/HA节、HB报告HC终态、独有 HbSurfaceConsumer 全文。长历史保留，不声称全文亲读。HB/HC 已知首红失败不归候选；目标内容与 backend 均未直接观测。

## 审计/运行纪律
只改 HB_SURFACE_CONSUMER 独有显式测试、必要独有只读诊断与本报告/三共享。默认不得发现未验收实验。禁止脚本/终端/新Host/第三工程/机器码；保留 expected、Present、cleanup。producer 软件读回不是 consumer 证明。

## 字段合同表（审计中）
| 项目 | HB/HC | 官方来源/HD裁决 |
|---|---|---|
| HWND CREATE | packed92，DPI double76/84=1/1 | 待逐字段认证 |
| flags | 0 | 已知官方 PRESENT_IMMEDIATELY、RETAIN_CONTENTS，待完整上下文认证 |
| client/dirty/invalidate | 32×16，SendOutputWindowSettings | 待核 |
| 绘制与资源关联 | image→drawing→visual→target root | 待核 |
| submit/闭批/Present | setup close/commit/flush；separate Present/commit | 待核 |
| 消息泵/线程 | 复用现有窗口 API | 待核 |
| connection | connect(0)异步 | 待核 |
| DuplicateHandle | 原事件持有，duplicate交命令 | 待核失败所有权 |
| 显示前置 | 未有本轮 native fresh | 必须同正式任务先验证，失败早停，不归候选 |
| consumer target/backend | 无独立目标读回 | 不以event/flush/GetAsSoftwareBitmap伪证 |

## 审计完成补记（正式运行前）
完整方案已补读38–120，HB报告25–56补读；三共享HC节已读，旧历史不宣称全文。仓库官方源码是合同参考，不冒称与servicing二进制精确源码校验绑定。

|项目|官方合同认证与HD修正|
|---|---|
|CREATE全部字段|generated/wgx_commands.cs303–322：0 type/4 handle/8 HWND/16 section/24 master/32 width/36 height/40 RGBA/56 flags/60 bitmap/64 stride/68 format/72 context/76、84 double；section/master/bitmap/stride/format=0符合普通窗口，clear黑alpha1、32×16不变|
|flags|exports2180–2250及wgx_core_types90/97：0x4加0x8=0xC；HD一次纠正，不加HARDWARE_ONLY掩盖backend未知|
|context/DPI|Shared/MS/Utility/DpiAwarenessContextValue全文：0 Invalid，合法-1/-2/-3/-4；HD查询window context并用AreDpiAwarenessContextsEqual映射，DPI用GetDpiForWindow/96.0，不把opaque handle硬转协议|
|窗口update|generated329–348/exports2385–2425：72字节、屏幕client rect，alpha1/renderingEnabled1；无边框popup rect即client屏幕rect，layer/transparency/child/RTL/cookie/gdiBlt0，不强制软件|
|dirty/invalidate|HwndTarget1532–1551及exports2427–2454：WM_PAINT发TARGET_INVALIDATE并close/commit；共享WndProc仅ValidateRect，HB缺等价命令。HD独有setup flush后发送0x37/24字节/client0,0,32,16并close/commit。真实WPF用双channel duplicated target；HB单async在目标已flush后顺序提交，避免未创建目标竞态，不声称完整MediaContext复刻|
|资源图|image97→drawing43→visual39→target46；renderdata header12+record48，draw0x47矩形0,0,32,16，SETCONTENT0x22、SETROOT0x35；generated133–139及exports2360–2380、既有packet对应；d3dimage.cpp ProcessUpdate偷取入队AddRef，保留|
|批次/Present|setup close/commit/flush保留；producer填充/GPU完成/dirty/reset/duplicate/separate Present/commit/wait/flush保留。D3DImage653–729说明独批必要性；d3dimage.cpp250–287无论Present失败仍SetEvent/CloseHandle，故event不是内容证据|
|连接|MediaSystem197–210：false为async，首channel null创建partition；HB匹配，不跑旧同步Event0|
|线程|WindowThread全文：专用STA GetMessage/DispatchMessage持续泵；测试线程等待不阻owner；D3D MULTITHREADED保留；独有销毁记录owner执行返回|
|duplicate所有权|原事件SafeWaitHandle持有，仅转交同进程duplicate，成功send后native关闭、测试不close；send失败所有权未由公开合同独立认证，不造重试或危险探针|
|显示前置|复用CheckFreshDesktopDelivery(1,false)，native backbuffer红/蓝/红、Present HR、GPU完成、window/screen/front与DXGI fresh QPC分层原件；前置失败ConsumerEntered=false早停；actual官方身份先记录，非已消费|
|目标/backend|无已认证公开API返回HWND consumer目标surface；producer GetRenderTarget/GetAsSoftwareBitmap不能替代；backend未知，window DC只为桌面可见层|

## HD 终态：唯一官方任务三色通过，停止本轮执行
上方进行中表与发布前账本为实时历史；本节覆盖当前状态。有效official task恰为1：**9e32d79eddaf4449a0c8b936f3831604**，official x64 1执行/1通过/0失败/0skip；candidate/x86/full均0。子PID9876、监督9572；Exit0/0x00000000，Reason exited，TimedOut=false、KillAttempted=false、OutputDrainTimedOut=false；无原生崩溃。工具未单列外层数值Exit；非MTP监督不造父TRX。

### actual identity
- 官方actual：C:\CodingAgentSandbox\Tasks\9e32d79eddaf4449a0c8b936f3831604\wpfgfx_cor3.dll；SHA **AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1**；10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。
- test actual SHA **878F8815345E61268526C957EF43D1D195979298C10D1C5216450234C9072E6C**；MVID **e148d839-2fd7-44d4-88b5-512acdf3d919**；.NET10.0.13/X64/ptr8。WPF同servicing/commit，PresentationCore SHA0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31。
- 候选包SHA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0只是复制的HA产物，未actual候选消费。native控制旧诊断PresentCondition的production字段也仅打印包路径/SHA，不是候选加载证明。

### 内容、时序与资源分层
|层|本task独立原件与裁决|
|---|---|
|native producer backbuffer前置|红/蓝/红分别FFFF0000/FF0000FF/FFFF0000，逐帧ReadNativeBackBufferPixel完全相等|
|native Present|三次HR0；submit线程=owner1904，GPU完成QPC在submit后；该前置与consumer窗口是两个先后创建的HWND|
|native desktop/window/front|COLORREF255/16711680/255，window/screen一致；front HR0且ARGB相等|
|native duplication fresh|三帧ARGB同上，LastPresent35691604326453/35691607068874/35691609154217分别晚于submissionStart35691602773629/35691604566382/35691607270622，非mouse-only/旧帧；passed=true|
|consumer producer surface|ColorFill HR0及GPU完成，不直接读该interop surface；native前置backbuffer不能冒充此surface内容|
|consumer target参数|packed92，flags12，context-2（raw24593），Dpi1/1，format0、32×16，window23855136，rect40,40–72,56，采样screen56,48；Client序列化为{}（默认不含字段），尺寸由已通过client断言和窗口日志补证，不伪造字段输出|
|consumer Present/事件|每帧dirty、separate send、commit、event=true、flush HR0；duplicate1808/1800/1836转交，测试直接close0；原event reset/set自检三次到达，不证明native exact-once-close或Available callback|
|consumer独立可见内容|红COLORREF expected255 actual255；蓝16711680=16711680；绿65280=65280，零阈值全部通过，不是两边同空白。native前置最后红与consumer红可能重色，但后续蓝/绿更新消除单首帧旧红证据不足；不凭颜色反推根因|
|consumer render-target内容/实际backend|目标surface无独立读回API证据，consumer backend仍未知。DXGI前置adapter日志Microsoft Basic Render Driver、vendor1414/device8C/LUID4E71/flags0只属于duplication，不证明consumer硬件或软件；不把HAL创建或内容通过写成HW完整门通过|
|cleanup|detach前后到达；handle4/3/2/1 unregister HR0/Deleted1；delete close/commit/flush、destroy、disconnect、deinit HR0；bitmap/surface/device/factory最终refs均0；owner DestroyWindow返回1，cleanup complete|

### 因果边界与停止点
已证夹具合同缺项为flags0、invalid context0、缺初始化invalidate；DPI改为真实窗口比例来源（本场仍1/1）。集中纠正并且同任务显示前置成立后，官方可见三色基线首次通过。没有逐字段消融，不能单独断言flags或invalidate/context之一就是HB/HC servicing失败唯一根因；更不能把旧UAF源码当servicing根因。未改共享VisualCache/historical、生产COM/CF/CX/CY、DRDS、NativeRun/ExitDiagnostics、捕获/WindowQ、打包/项目或默认并行。保留HA Stdcall修复。

### 证据路径、亲读与默认安全
绝对回收根：d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\hd-hwnd-contract\readback\x64-official。
- 绝对taskTRX：C:\CodingAgentSandbox\Tasks\9e32d79eddaf4449a0c8b936f3831604\Results\hd-x64-official\TestResults\child.trx。
- 本地相对回收根：Results/hd-x64-official/TestResults/child.trx（86行）；同级上层supervision.json39行、hb-consumer.jsonl77行、stages.jsonl77行、stdout.txt12行、stderr空；前置Results/bj-fresh-1.json60行。以上按分段回读；诊断文件完整回收未全文，不冒称全部诊断已审。
- publish/package输入与readback回收分离；仅上传独有com-acceptance-package，不上传PublishDir或历史父目录；正式打包目标回收源独立且clean生成，Contracts/runtime/aot.sha256已核。未运行删除历史产物操作。
- 最终测试源码后default Release/win-x64正常build成功，98 warnings/0 errors，末段58–64亲读；无NoWarn/禁分析器/skip/放宽golden/fallback。文件整体#if HB_SURFACE_CONSUMER，默认不发现此实验。生产未改/未重发。仅保留ignored独有产物，不扩大清理范围。

### 下一唯一实证任务（本轮不执行）
复用本次同一HD包、相同flags/context/DPI/invalidate、native fresh前置与原红/蓝/绿断言，仅candidate x64显式测试一次，绑定实际候选8F74588B…模块SHA并亲读三色/事件/refs；前置失败早停，candidate内容失败精确定位，不立即扩大COM替换。若需要真正HW验收，仍必须补独立consumer backend与目标内容/API证据，不能将本次桌面内容成功替代。
Available真实callback、HWloss/恢复、media/资源通知、full最终四格与独立x64Event0仍开放；完整替换未完成。

HD末次补记：报告与三共享当前受改节/末次补记写后回读；旧历史保留，未宣称千行历史全文复核。最终源码后无额外测试改动，正式task不重跑。

## HE终态：同HD包唯一candidate x64三色、事件、refs通过
唯一新task **333b412deb784bc3bdea920f2a519ba7**。正式纯C#同exe --supervise-exit，root Results/he-x64-candidate、120秒、filter FullyQualifiedName~WhenHbCandidateSurfaceMutatesThenConsumerWindowMustChange，外180秒。仅上传artifacts/hd-hwnd-contract/com-acceptance-package；未上传PublishDir或回收根。实际包公开API及源码[TestMethod]均核可发现，RunHbConsumer(false)无猜角色。与HD仅角色/filter和独有结果root不同；flags12/context-2/DPI1/1/初始invalidate/expected/Present/cleanup均相同，未改并行。

### 计数、Exit与实际身份
- 子TRX Completed：total1/executed1/passed1/failed0/notExecuted0；stdout skipped0。监督PID6332、子PID836；Reason exited，Exit0/0x00000000，TimedOut=false、KillAttempted=false、OutputDrainTimedOut=false、SupervisorError=null。工具仅返回执行完成及回收路径，没有外层数值Exit；监督非MTP，不存在应计父TRX。没有原生崩溃，不按末阶段猜根因。
- actual候选路径 **C:\CodingAgentSandbox\Tasks\333b412deb784bc3bdea920f2a519ba7\Native\wpfgfx_cor3.dll**，SHA **8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0**，版本1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299。identity代码用NativeLibrary.Load返回句柄与Process.Modules.BaseAddress相等定位actual再hash；原件未序列化数值模块句柄，不编造数值。不是包根官方DLL冒候选。
- test actual路径同task根WpfGfxShape.ComAcceptance.dll，SHA **878F8815345E61268526C957EF43D1D195979298C10D1C5216450234C9072E6C**，MVID **e148d839-2fd7-44d4-88b5-512acdf3d919**；与HD完全相同。runtime .NET10.0.13/X64/ptr8；WPF10.0.13-servicing.26504.104+4099f83977f4d29604a9aeb971fb0a7bc1d5f699，WpfSha256 0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31。
- 对照HD task9e32d79eddaf4449a0c8b936f3831604官方actual包根wpfgfx_cor3.dll SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，旧原件重新亲读而非重跑。HE新增candidate task恰1；新增official/x86/full/重复COM变体0。

### 前置、真实内容与生命周期
- native前置passed=true，red/blue/red backbuffer和duplicated/front ARGB分别4294901760/4278190335/4294901760；window/screen COLORREF255/16711680/255。三Present与frontResult HR0，submitThread=owner8848。fresh LastPresent35694816082519/35694818238305/35694820866031均晚于对应submissionStart35694813714739/35694816310221/35694818605569；GPU completed均在submitted后，无旧帧冒充。
- consumer窗口3211722（0x3101CA），rect40,40–72,56，32×16，screen sample56,48，window DC client16,8；packet92，flags12，DPI1/1，context-2/raw24593。Client序列化{}，不补造字段。consumer独立GetPixel结果红 **255=255**、蓝 **16711680=16711680**、绿 **65280=65280**，全部Accepted=true；后两帧证真实内容更新，不仅eventsignalled。
- 各帧producer ColorFill/GPU完成、dirty、separate send、commit、consumer flush/DwmFlush到达，所有所记HRESULT0；三event Signalled=true；duplicate1644/1616/1728，测试直接close0。原event每轮Reset/Set自检通过不证明native duplicate exact-once-close，不证明Available callback。
- interop create/partition initialize/async connect/channel及四resource创建HR0。Detach前后到达，resource4/3/2/1各unregister HR0、Deleted1；delete close/commit/flush、destroy、disconnect/deinitialize HR0；Bitmap/Surface/Device/Factory最终refs **0/0/0/0**；DestroyWindow Result1，cleanup complete。
- actualconsumerbackend仍未知，HardwareConsumerProven=false；native duplication Microsoft Basic Render Driver仅前置adapter证据，不能冒consumer HW验收。consumer target surface未直接读；producer surface也未直接读，窗口可见三色成功不升格为HW完整闭合。consumer未单独记录screen GetPixel，screen三色仅native前置；不冒称consumer有双读回。

### 原件路径与实际审阅范围
整task绝对回收根 **d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\he-candidate-consumer\readback\x64-candidate**。
绝对task TRX **C:\CodingAgentSandbox\Tasks\333b412deb784bc3bdea920f2a519ba7\Results\he-x64-candidate\TestResults\child.trx**；本地同根Results/he-x64-candidate/TestResults/child.trx。
HE监督39行、childTRX86行、业务hb-consumer.jsonl77行、stages.jsonl77行、stdout12行、stderr空、Results/bj-fresh-1.json60行全部分段亲读（初次截断TRX1–30已以1–18/19–26/27–47补齐）。完整Diagnostic/log_261023094003957.diag已回收，仅目录登记，未读正文，不宣称全审。
HD旧原件监督39/TRX86/业务77/阶段77/native60/stdout12/stderr空本轮同样分段亲读。完整方案1–120与HB独有232行读取，长返回截断处补范围；历史共享仅最新HD/HC/HA相关节，不宣称500余行历史全文。HD报告最终全文将分段写后回读。

### 修改边界与下一唯一任务
本轮零源码修改、零编译/发布，生产HA Stdcall保留。未改生产COM、共享VisualCache/historical、DRDS、NativeRun/ExitDiagnostics、捕获/WindowQ、项目打包、CF/CX/CY；未改expected/flags/refs/Present/cleanup。仅本报告及handoff/session-summary/issues更新，不新增重复报告。引用HD默认Release/win-x64 build成功98 warnings/0 errors为旧构建，本轮未新跑。
下一唯一任务：**x86同场景official/candidate对照**，先核可用同合同正式包与架构身份、同任务native fresh前置，逐帧三色/事件/refs；本轮不执行、不扩full。Available0/真实callback、native duplicate exactclose、consumer HW backend/目标内容、HWloss、media/resources、最终四格及独立x64Event0仍开放；完整替换未完成。

HE末次补记：唯一候选真实任务及核心原件已读后收束；报告全文和handoff/session-summary/issues最新HE受改节及本补记写后分段回读，旧HD/HB/HC历史保留，未宣称历史全审。

## HF终态：x86同包official/candidate真实可见内容对照通过，停止执行
HF实时登记是阶段历史；本节为当前状态。新增恰official1+candidate1，无重跑/full/额外控制/COM变体；未出现部署纠正。零源码修改，未重建生产，保留HA Stdcall；未改生产COM/CF/CX/CY、DRDS、NativeRun/ExitDiagnostics、共享VisualCache/historical、捕获/WindowQ、项目打包或默认并行。仅正式测试包新发布一次。

### 两格身份与计数
|角色|task|监督PID/子PID|子TRX total/executed/passed/failed/notExecuted|子Exit|
|---|---|---|---|---|
|official|e0036f3dbf3347bb8891145835f4c9df|2128/4868|1/1/1/0/0|0 / 0x00000000|
|candidate|71daf6c78a9c4cdabc67748c1c9d8e98|1336/8808|1/1/1/0/0|0 / 0x00000000|
两份监督均Reason=exited、TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，HasExited/OutputCollected=true，SupervisorError=null。filtered内部120秒、工具外180秒；工具没有单列外层数值Exit，不编造。非MTP监督无父TRX；子Completed，error/timeout/aborted等Counters均0，stdout skipped0，无Failed条目或原生崩溃。

- 两格同一测试DLL SHA **C61C02D7A3A6F1B4785920037C8CF0E81C1A18EE51352950D9CE99B2A614A12E**，MVID **e37ffa74-0079-4cf6-af2d-c1e0671cc2f2**；位于各task根WpfGfxShape.ComAcceptance.dll。actual .NET10.0.13 / X86 / pointer4。
- official actual **C:\CodingAgentSandbox\Tasks\e0036f3dbf3347bb8891145835f4c9df\wpfgfx_cor3.dll**，SHA **5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834**；10.0.13-servicing.26504.104 @Commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。来源为同包self-contained WPF x86 runtime根官方模块。
- candidate actual **C:\CodingAgentSandbox\Tasks\71daf6c78a9c4cdabc67748c1c9d8e98\Native\wpfgfx_cor3.dll**，SHA **75D136F71223DB9EA0830BFBF9CDFC0EAAB748B5067397B682E42323B698EC30**；1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299。确为HA已验收Native，不是1893370失败产物或官方充候选。
- 两格WPF ProductVersion **10.0.13-servicing.26504.104+4099f83977f4d29604a9aeb971fb0a7bc1d5f699**，PresentationCore SHA **D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D**。来源为正式自包含包，不借系统未部署runtime。identity通过Load句柄与Process.Modules.BaseAddress相等选actual再hash；**数值基址及WPF完整加载路径未序列化，本轮未直接取得，不编造**。

### 内容/时序/资源证据
两格均packet92、flags12、context-2/raw24593、实际DPI1/1、format0、producerFormat21、32×16，target rect40,40–72,56，client采样16,8对应screen56,48；Client对象输出{}不补造字段。初始化invalidate/close/commit、async connect、独批Present、DuplicateHandle及原expected/cleanup全部原样。

|层|official|candidate|
|---|---|---|
|native backbuffer/duplicated/front ARGB|FFFF0000/FF0000FF/FFFF0000逐帧全等|相同|
|native Present/frontResult|三次均HR0|三次均HR0|
|native window/screen COLORREF|255/16711680/255，全等|相同|
|native提交线程=owner|5152|2848|
|native GPU completed>submitted|三帧成立|三帧成立|
|DXGI fresh LastPresent>submissionStart|35700157058442>35700154700583；35700160179770>35700157657018；35700162785610>35700160526307|35700692515950>35700690482570；35700695014903>35700692732830；35700697714476>35700695240918|
|consumer HWND|10355990|8062454|
|consumer 红 expected/actual|255/255|255/255|
|consumer 蓝 expected/actual|16711680/16711680|16711680/16711680|
|consumer 绿 expected/actual|65280/65280|65280/65280|
|consumer三event|true/true/true|true/true/true|
|duplicate转交数值|1988/1984/1492|1644/1644/1744|
|最终Bitmap/Surface/Device/Factory refs|0/0/0/0|0/0/0/0|
|DestroyWindow|1|1|
两格native passed=true，fresh非mouse-only旧帧，前置HWND与consumer先后不同。consumer ColorFill/GPU complete、dirty、separate send、commit、event、consumer flush/DwmFlush及三色accepted完整链成立；后续蓝/绿证明真实更新，不仅事件。所有记录HRESULT0。Detach前后、resource4/3/2/1 unregister HR0且Deleted1、delete close/commit/flush、destroy、disconnect/deinit与cleanup complete均到达。

consumer读回仅window DC，不是consumer screen双读回；native window/screen/front/duplication属于显示前置。没有独立consumer target surface读取API证据，不把producer读回冒consumer证明。**actualconsumerbackend未知/HardwareConsumerProven=false**；Microsoft Basic Render Driver vendor1414/device8C/LUID4E71是native duplication前置，不外推consumer HW或SW。Available0/真实callback未证；测试侧duplicate直接close0及原event自检不证明native exact-once-close，候选重复句柄值可复用，不推断关闭次数。

### 正式包、路径与亲读范围
正常分析器发布成功，未NoWarn/关闭规则，仅显式HB_SURFACE_CONSUMER；独有obj/bin/publish，显式PublishDir；runtimeconfig includedFrameworks.NET/WPF10.0.13。正式clean source=**artifacts/hf-x86-consumer/com-acceptance-package**，未上传PublishDir/回收根/历史父目录。aot.sha256、Contracts、MTP、418行package-files清单首尾及正式DLL离线API已核；清单和实际包字节保留可得，未额外上传自包含包外无关资产。
绝对本地根 **d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\hf-x86-consumer**；两格整task分别回收readback/x86-official、readback/x86-candidate，与source分离。
绝对taskTRX分别：
- **C:\CodingAgentSandbox\Tasks\e0036f3dbf3347bb8891145835f4c9df\Results\hf-x86-official\TestResults\child.trx**。
- **C:\CodingAgentSandbox\Tasks\71daf6c78a9c4cdabc67748c1c9d8e98\Results\hf-x86-candidate\TestResults\child.trx**。
本地各回收根下Results/<同名run>/包含supervision.json39行、TestResults/child.trx86行、hb-consumer.jsonl77行、stages.jsonl77行、stdout.txt12行、stderr空；native Results/bj-fresh-1.json60行。两格以上核心原件均分段亲读，截断处补读；诊断/PPM整回收，未全文或逐图审，不冒全审。HB显式文件原合同已核，不改源码也不为补基址扩跑。历史HD/HE范围仅读同HD报告全部及共享最新HE/HD/HA相关节，未重读旧task全部原件，未冒称全历史复核。

### 默认安全、下一唯一门及未完成声明
本轮零源码修改；引用HD既有default Release/win-x64正常build成功、98 warnings/0 errors（非HF新跑），HB文件整体#if隔离，默认不发现；此次显式发布不污染默认obj/bin。HA生产修复保留，无full扩跑。
**下一唯一共享夹具迁移门**：将HD集中认证且x64 HD/HE、x86 HF有限验证的flags/context/实际DPI/初始化invalidate/async+duplicate所有权合同，先逐项映射到共享真实consumer夹具并保留旧断言/cleanup，再设计独立backend/目标消费证据；此轮仅登记，不迁移源码、不扩COM替换。不能直接将HB实验默认化或用新场景成功关闭旧x64Event0/EHANDLE异常。
consumer实际HW/backend/目标内容、Available callback、native duplicate exactclose、HW loss/恢复、media/resources通知、最终full四格及独立x64Event0/EHANDLE线仍开放；**完整替换未完成**。

HF末次补记：两格已跑且核心原件已读后停止；本HD报告全文及handoff/session-summary/issues最新HF受改节和末次补记写后分段回读，旧历史保留不冒全文。
