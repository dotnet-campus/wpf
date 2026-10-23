# EV：后置三调用同 DLL 运行时对照

## EW 独立核证结论（最新优先）
EW最终回读补记：四文档最终正文已全文小段读至末（补记前本报告37行、handoff523行、session-summary605行、issues622行）；所有超长截断均缩小补齐。本补记写入后再次读取。原件两TRX各26行及两DT各72行已精确补核，不遗留枚举误判。实验快照另补读245–315的前序/cleanup；direct根ev-generated.input精确读取不存在，generated标记内容已读。Code追加检索EV/ET/ER/EI/EG/EK及EA/EB指定标记/辅助零命中，仍仅限Code与所列模式。无源码改动/新业务任务/构建/发布/full；默认未修、未闭运行门不变。
EW仅修改本报告及handoff/session-summary/issues四文档；未改源码、未构建、发布或启动沙箱/业务/full任务。规则、完整实施方案120行及当前基线已读。下述亲核不承袭EV口述；历史过程全部保留。

### 原件与身份范围
证据根为 `artifacts/ev-adapter-samedll/`（相对WpfGfxShape项目根）。分别亲读 `run-direct/Results/ev-direct/` 与 `run-generated/Results/ev-generated/` 的 `supervision.json` 1–39、`TestResults/child.trx` 1–26、`dr-candidate.jsonl` 1–39、`stages.jsonl` 1–39、stdout 1–12及空stderr；两组 `Results/ev-adapter-samedll/stages.jsonl` 1–21、`Results/dt-interop-create/stages.jsonl` 1–72与 `Results/aot-identity.json` 单行均亲读至末。长日志截断已缩小补读。actual identity就是各业务JSON第1行，不虚构独立actualidentity文件。TRX先尝试监督中的远端绝对路径不可访问，随后精确读取上述本地回收件，TRX内task/codeBase与监督对应；不是以stdout替代TRX。
Direct task `4296dd31a8c04d65a0bcb0548e1ee9bd` 父9856/子1192；generated task `2c4456e90cf34d58958524635ac2fe78` 父4152/子3828：均Exit0、无timeout/cancel/kill/drain异常，各TRX 1/1 Passed。每组三轮Create Hr0/12x3、IUnknown同identity、AddRef2/Release1、GC dirty Hr0、Detach完成，各Bitmap最终refs0；末尾surface/device/factory均0，callback root freed、module resident、completed均有记录。
两组actualNative均 `6DCA9E5503D6A66B5334A5C9B1905815778D27A8348DE339F9B79C97E210FC5F`，与各aot.sha256及aot-identity一致；test均 `4365873DC3C71F363FE51E2405EBAF3A0FBA2A51AC3800E7535CE4A6E98DFFCB`，MVID均 `3e089fd7-3c70-40dc-9ce4-bb9f85613a1f`，.NET10.0.13/X86/pointer4。上述是实采记录互核，不是EW重算hash。

### 对照成立及不成立的推断
亲读实验快照 `source/ev-full-experiment.cs.txt` 的前序180–240与后置316–420：运行时仅 `ev-generated.input` 控制后置device slot6、factory QI slot0、factory Release slot2；两种分支共用输出地址、前序调用背景和日志，不分别重编译。generated标记内容已读；两组EV日志逐轮GetDirect3D Hr0、factory QI E_NOINTERFACE(-2147467262)/extended0、factory Release2。真实nonEx分支未走第二Creation、LUID、extended Release，不把未执行的显式Stdcall代码算覆盖。Available Count0/Notifications空且NativeInvocationCovered与ConsumerCovered均false，无消费证据。完整日志行本身不证明其writer/stream Dispose已返回。
因此generated后三调用本次也成功，不能支持它们单独导致旧失败；仅缩小疑点，不能据此称整个Silk库已修或全库错误，也不是失败后继续换全部的理由。此前EL Creation同DLL因果仅在其原背景内有效，不移植成当前唯一根因；本轮未新增EL运行核证。

### 默认恢复、偏差与安全边界
只读核当前 `Code/WpfGfxShape/Abi/InteropDeviceBitmapExports.cs`：默认Create前序与GetAdapterLuid仍为generated；后置283–305与 `source/default-before-ev.cs.txt` 对应段一致，DT_INTEROP_CREATE_TRACE/DX_INTEROP_COM_DIAGNOSTICS及原TraceCreate保留。`source/restore-sha256.txt` 两行记录当前源与default-before-ev均 `1C3E30CCB283A71FAE61286C582D7899A036911F474D3632614678F207FFFFB1`；EW无独立hash工具，未声称重算或逐字比较全文件。Code目录检索 `EvTrace|ev-generated|EV_ADAPTER|QueryCreationDevice|ReleaseCreationDeviceEx|creationBoundary` 零命中，仅支持该Code范围/这些标记和辅助无残留，不外推全仓全模式。EW未触及CF/CX/CY、EF或任何源码；既有EF/trace保持，不新增默认修复。
EV首次NativeAOT发布因既有PublishDir提前确定落到共享 `artifacts/publish`，随后显式专属PublishDir增量重发；此偏差保留，不能写成首次即完全隔离，也不是direct/generated各编一次。EV报告原记Native输出607229字符、仅末20000可回读，EW未亲核完整发布日志；不冒称所有警告已审计。任何默认增量build均非NativeAOT验收，EW不重建来覆盖此边界。默认问题未修，未执行新的默认AOT/双架构验收。

### 下一唯一实验设计（仅设计，须另授权）
选择“同一DLL切前序调用组，后置fixed generated成功基线”，暂不选择移除trace的default最小复现：EV已经给出有trace背景下后置generated成功，先保持这项正控制能隔离前序组合影响，避免同时换trace和AOT布局。
另授权时仅一份x86 NativeAOT/同正式DR程序集：固定后置slot6/QI0/Release2为generated、固定输出地址/布局/日志/真实能力判断；运行时标记仅切EV所用ER前序十调用组的SDK Stdcall与默认generated（以EV快照列出的调用组为边界，不切后置、不碰EF/CF/CX/CY）。先前序direct正控制，完整三轮成功且refs0才运行唯一前序generated组，最多两个有效task；控制失败即停，不继续替换更多调用。若只有前序generated失败，只能说明该前序组合在此固定背景相关，不能定位单一成员或宣称清栈根因；若两组均成功，则前序组本身也不足复现，保留trace/布局等后续疑点而不在本实验追加变体。两组均保存实际Native/test SHA/MVID、监督、TRX、三轮业务/释放及Ex/Available未覆盖情况；不full，不留默认实验补丁。
x64 Event0、Available真实通知消费、HW内容/resize/device-loss、持续media、resources通知/生命周期及final四格继续开放。文档闭环不等于完整替换。

## EV-4 两组均成功，开始撤回
第二且最后有效task2c4456e90cf34d58958524635ac2fe78 父4152/子3828 Exit0，无timeout/cancel/kill/drain异常，精确child.trx26行1/1 Passed。业务identity亲核与direct相同NativeSHA/testSHA/MVID；EV21行亲核三次Hr0/E_NOINTERFACE null/Release2，两组不是单独重编译。结论：在此次同DLL/同日志控制背景下，这三个实际调用的generated组也成功，旧失败不能由这组三调用单独重现；不归因唯一调用、不宣称整个Silk库错误，不猜栈破坏。任务预算已用完，不再运行业务任务。正在补核回收证据并恢复默认源码。

## EV-3 direct 控制成功，允许唯一 generated 组
Task4296dd31a8c04d65a0bcb0548e1ee9bd 父9856/子1192 Exit0；监督无timeout/cancel/kill/drain异常。精确绝对任务 child.trx 已直接读26行，1/1 Passed。业务39行和EV21行全文亲核：三次Create Hr0/12x3、QI同identity、AddRef2/Release1、GC dirty Hr0、Detach及各Bitmap最终0，surface/device/factory最终0。后置三次GetDirect3D Hr0、factoryQI E_NOINTERFACE/null、factoryRelease2；Ex三类调用未执行，Available0未覆盖消费者。
actualNativeSHA与包一致；业务/监督testSHA4365873DC3C71F363FE51E2405EBAF3A0FBA2A51AC3800E7535CE4A6E98DFFCB、MVID3e089fd7-3c70-40dc-9ce4-bb9f85613a1f，.NET10.0.13/X86/4。接下来仅同一清洁package增加ev-generated.input，不重编译，不上传回收根。

## EV-2 已发布，准备 direct 控制
仅 Interop 显式 EV 条件代码增加 ER 十调用背景及三调用运行时标记选择；两种模式共用输出变量地址表达式和日志，未执行 Ex 分支仍显式 SDK Stdcall。默认分支不读取标记。完整修改前/实验源已保存 ignored source；未改 DR/DS/监督及禁止区域。
NativeAOT x86 正常分析器发布成功；首次 PublishDir 被既有默认属性提前确定，落到共享 publish，立即以显式专属 PublishDir 增量再发布成功（非两组分别编译）。正式 DR selfcontained10.0.13 发布成功，仅同一 package 用于两组。未关闭分析器/压制警告；Native 输出607229字符只可回读末20000，不称完整警告明细。package aot.sha256为6DCA9E5503D6A66B5334A5C9B1905815778D27A8348DE339F9B79C97E210FC5F。

## EV-1 进行中
已开始读取最新 handoff、执行规则、ET/EU 与项目结构。目标仅在 ET 成功背景内以同 NativeDLL、同正式 DR 测试程序集运行时切换后置 GetDirect3D(slot6)、factory QI(slot0)、factory Release(slot2)，先 direct 控制，再按条件 generated，最多两个有效 x86 任务。不以 Ex 未执行分支充覆盖，不改变默认 ABI，不修改 DR/DS 预期及禁止区域。
当前尚无源码修改、构建发布或沙箱任务；历史证据不算本轮运行。后续阶段实时追加，保留历史。
