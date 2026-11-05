# HI Present完成→桌面fresh交付边界诊断

## HJ历史阶段登记：专属格式设施修复，登记时正式task尚0
只改HI_NATIVE_DELIVERY partial：GetBackBuffer/GetDesc实际尺寸/Format创建SYSTEMMEM staging，逐调用记录acquire/descriptor/create/copy/lock/unlock/release；实际Format22按X8R8G8B8解释，X非alpha，RGB仍零阈值，expected及其余桌面断言未改。Present调用布尔与返回/提交QPC单列，保持原occluded正状态后仍Present调度。正常分析器发布已成功，未加NoWarn/禁规则；独有artifacts/hj-present-control/{obj,bin,publish,com-acceptance-package}显式PublishDir。
离线发现首发布误引用HA现行包的x86依赖SHA75D136…，正式task前已纠正为HI保存的HA x64候选8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0并正常重发布；未部署错误包、不计新task。该候选只依赖，不声称actual消费。两次发布调用均成功；runtimeconfig 10.0.13双framework、419行clean白名单首尾及aot.sha256已亲读，MTP/Contracts/runtime/Native齐全，source与readback分离。已核NativeRun正式MTP入口及BL filtered120合同；精确filter为FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenHiNativeDeliveryRunsThenBlitAndFlipOutputBoundariesAreRecorded，HI符号显式发布。即将唯一正式task，外180秒，完整回收，不追加普通断言重跑。SDK26100 shared/d3d9.h2004/2016–2017已核：INVALIDCALL MAKE_D3DHRESULT2156=8876086C，MODE_CHANGED MAKE_D3DSTATUS2167=08760877，OCCLUDED2168=08760878，正状态非S_OK。历史HI以下原样保留。

## HJ实跑/清理阶段：唯一task已回收，停止追加
Task80257012a4c640019c6eaf83b1b34024，监督5944/child7456，UTC11:03:12.6755987–11:03:14.3830494。filtered120/外180；childExit2/00000002、exited/nonTimeout/noKill、OutputCollected=true/drain正常、SupervisorError=null，stderr空。外工具仅报告完成，未提供独立数值Exit；非MTP监督无父TRX。绝对task路径映射直接读取Results/hj-x64-native/TestResults/child.trx1–12、85–105：Failed，total1/executed1/passed0/failed1/notExecuted0/timeout0/aborted0，RunInfo Error exit2。六帧不是六tests，普通断言失败不是部署缺项。仅1正式task，不再跑变体。
六帧Clear/acquire/GetDesc/staging/copy/lock/unlock均HR0，Pitch128，staging/backbuffer Release均refs0。实际32×16/Format22/Pool0/MS0/Q0；DISCARD Type1/Usage1，FLIPEX Type3/Usage8396801(0x802001)。staging精确同尺寸格式/Pool2；全部backbuffer红FFFF0000/蓝FF0000FF/红FFFF0000。Format22 X字节非alpha，RGB完全相等，无阈值放宽。两控制device/factory refs0/DestroyWindow1；DISCARD cleanup QPC35744709528432早于FLIPEX window35744709638147。公共读回/窗口/生产/VisualCache/historical/NativeRun/ExitDiagnostics/DRDS/项目打包/CF/CX/CY均未修改。

### HJ真实Present与内容对照
|模式/帧|submission QPC|completion QPC|PresentEx真实HR|CheckDeviceState前/后|window/screen COLORREF|front ARGB|duplication ARGB / fresh lastPresent|
|---|---|---|---|---|---|---|---|
|DISCARD red|35744704008475|35744704541851|00000000 S_OK|0/0|000000FF/000000FF|FFFF0000 HR0|FFFF0000 /35744705642302|
|DISCARD blue|35744706042502|35744706406172|00000000 S_OK|0/0|00FF0000/00FF0000|FF0000FF HR0|FF0000FF /35744707004362|
|DISCARD red|35744707475636|35744707912136|00000000 S_OK|0/0|000000FF/000000FF|FFFF0000 HR0|FFFF0000 /35744709019666|
|FLIPEX red|35744709749184|未完成(原值0)|8876086C INVALIDCALL|0/未调用(int.MinValue)|未采样(FFFFFFFF)|未采样(FFFFFFFF),HR哨兵|未采样(0),无fresh观测|
|FLIPEX blue|35744710003995|未完成(原值0)|8876086C INVALIDCALL|0/未调用(int.MinValue)|同上|同上|同上|
|FLIPEX red|35744710071285|未完成(原值0)|8876086C INVALIDCALL|0/未调用(int.MinValue)|同上|同上|同上|
六帧presentCalled=true；FLIPEX不是未Present，而是slot121真实调用返回失败，原断言在该边界终止每帧回调，后态/完成/桌面样本均未执行。保持原预注册red/blue/red循环，未新建API变体；失败后没有追加task。Present flags0、source/destination NULL、override真实HWND、effect1/5、format22/count2/interval1/behavior26/owner提交均保留。各次Present返回QPC依次35744704013908、35744706044283、35744707477176、35744709749210、35744710004017、35744710071304。两模式countResult/statsResult均8876086C，lastCount及stats0是失败输出，不能称零帧或consumer计数。DISCARD三次accepted=true，FLIPEX三次false，最终allAccepted=false。
DISCARD duplication初始HR0/lastPresent0(鼠标帧)，提交起点35744703823945/35744706023038/35744707458217；三次目标lastPresent均晚于对应起点、积累9/6/8，实际独立readback红/蓝/红。FLIPEX仅初始HR0/lastPresent0/鼠标活动，提交后回调因Present失败退出，不能说取得旧色或等待后无fresh。没有观测到不同色/旧色，不造这种样本。
H1：本task不支持“FLIPEX改善交付”，DISCARD已真实成功而FLIPEX在PresentEx API边界失败，无法比较FLIPEX成功提交后的内容。H2：本task六次stateBefore全S_OK，DISCARD后态亦S_OK；HI的S_PRESENT_OCCLUDED未复现。跨HI/HJ只有状态与结果相关性，不能因果归于格式修正消除occlusion，不能把HI状态回填HH/HD或把失败归整个环境。D3D9/DXGI LUID4E71、monitor10001匹配，简单错输出排除仍限本task。

### HJ身份与现场
actual test SHA5F1828CEA6A725729A9B7EA08E17345CF98360A87EE18F55DF115677AB548B2F，MVIDf5f29290-df11-4bbc-9819-37093387422f，task根dll，.NET10.0.13 X64；actual PresentationCore SHA0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31，10.0.13-servicing.26504.104+4099f83977f4d29604a9aeb971fb0a7bc1d5f699。候选8F74588…仅包身份/初始化hash，consumerEntered=false，不声称actual候选模块消费。actual SYSTEM32 dwmapi26100.8875 SHA434EDD020C6EB30AEE968004641FCE6B21569E53F21695250B5055C4A93904C2；d3d926100.9278 SHA87E157E78536C66FC413ED202296E39053CC9E14DC8D8381F8A3D18F444A4C0D；dxgi26100.9444 SHAB12AEBF0F077D6C1394B50E2EECDD5E16072C9A0C871F30887B5D170CBF2EAAE；d3d1126100.9278 SHAAE2777002F5A0FF543B5B9BEC5124746559FD01D6C7C0C216D3194632F084502。
实际session1/Active0/WinSta0/Default/High S-1-16-12288/remoteMetric0/protocol0/DWM HR0 enabled1。HWND17170680/17236216，style90000000/ex8/DPI96，screen56,48/client32×16，foreground/cover自身，visible1/iconic0/cloaked0，测试tid2088/owner及Clear/Present tid1528，monitor65537；DXGI Microsoft Basic Render Driver1414/8C，DISPLAY1 1024×768 LUID00000000:00004E71。并未切换HWbackend或软件fallback来追绿。
回收artifacts/hj-present-control/readback/x64-native；supervision全39、业务74行分段、双窗口各4、双流/childTRX关键Counters/RunInfo均已亲核。Diagnostic/log_261023110312829.diag大事件日志未全文审，不宣称事件消费或Event0。下一唯一动作不是扩consumer或迁移：先离线核FLIPEX真实descriptor与PresentEx调用合同/override限制，定位本次INVALIDCALL边界；不得随机改flags重跑。DISCARD成功可作为恢复共享迁移的前置证据，但FLIPEX门未通过；HDHEHF成功反证、HA生产修复保留。Available/HWbackend/loss/media/resources/full四格/x64Event0继续开放，完整替换未完成。默认正常分析器构建已最终新跑：Release/win-x64，98 warnings/0 errors，2.30秒，日志58–64亲读；无HI显式符号，未通过双模式诊断不进入默认行为。源码最后修改后未再改源码。业务74行/双窗口各4行、stdout1–84分段、stderr空、supervision39及childTRX1–12/85–105已核；childTRX中段13–84已补核，重复输出及ErrorInfo与原始双流一致；现childTRX105行全读。大Diagnostic未全审，仅登记路径；未声称full/Event0覆盖。

## 历史阶段登记：原件审计，正式task=0（终态见末尾）
遵循本轮授权，仅可增加 HI_NATIVE_DELIVERY 条件隔离的纯C#诊断；不改production、共享断言/捕获/VisualCache、NativeRun/ExitDiagnostics和项目打包。共享async迁移保持撤回。最多一个正式x64任务，必要两个单变量控制同task顺序独立释放；不重复HH失败门、不运行consumer/Event0/COM/full。

已读执行规则全文、latesthandoff最新HH/HG状态、HG报告HH静态差异及终态主体；完整方案与HDHEHF历史原件继续分段核证。历史长返回截断未读部分不计全审。

已知HH同task两前置backbuffer正确/Present0/GPU完成但桌面无fresh；HDHEHF真实三色成功为保留反证，不得解释为驱动永久不支持。当前审计actual session/windowstation/desktop/完整性/远程、style/DWM、adapter/output/HWND、creation/实际swapchain/Present状态及绘制时序。实际JSON无字段即缺项，不用代码推导冒充实际观测。

上述为初始历史登记。

## 审计结论与预注册（task仍0）
HD成功原件 bj-fresh-1.json 1–55及Diagnostic/log_261023093442877.diag53–70：session1/Active0、Default、未cloaked、foreground/cover自身、DISPLAY1 1024×768、monitor10001，DXGI BasicRenderDriver LUID00000000:00004E71。actual format21/count1/swapEffect1、HWND override同目标，Present0，三色client/screen/front/duplication真实通过。HH旧window JSONL全文2行同session1/Active0/Default/monitor65537；其身份与像素见HG/HH报告。历史未记录windowstation/完整性/remote protocol/D3D9 adapter LUID，不能臆造同一；style旧HH实际已记录，成功侧仅同源码推定。DWM composition/目标present stats历史缺项。BX和CG报告相关节已读：GDI可更新；paint计数不在D3D提交后变化；不重跑GDI覆盖/paint强制刷新。

H1（先验中等，非已证）：windowed blit重定向路径不交付，但FLIPEX呈现链仍交付。控制唯一变量SwapEffect1对5；均D3D9Ex、adapter0/HAL type1、behavior0x26、format22/count2/interval1、32×16、owner PresentEx、同类独立HWND。count2/format22是两控制共同合法配置，不与HH声称一变量因果；仅两控制内部可比较。预测：blit失败flip成功且同monitor/LUID，支持模式特异交付边界；双失败则未支持，不可称根因。
H2（先验低至中）：硬件呈现源与读取输出不属同链，或设备明确报告occlusion/mode-change。只读GetAdapterLUID对DXGI LUID、GetAdapterMonitor对HWND/output、CheckDeviceState前后与PresentEx所有正状态，GetLastPresentCount/GetPresentStats仅目标swapchain。LUID/monitor匹配排除本task简单错输出，不能证明DWM内部consume。统计不可用HR如实记录；全局刷新不作consume证据。

新增仅HI_NATIVE_DELIVERY显式文件，不加载候选/官方wpfgfx，HA仅包依赖。正常分析器正式self-contained10.0.13发布已成功，独有artifacts/hi-desktop-delivery/{obj,bin,publish,com-acceptance-package}，显式PublishDir。日志54–60、runtimeconfig全文、清单400–419及aot.sha256已核：HA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0。clean白名单含Contracts/runtime/Native/aot.sha256，无NoWarn/禁规则。仅新package上传，readback分离；即将唯一x64监督task，filter精确HI方法，120秒/外180秒，两控制顺序独立释放，不加载wpfgfx候选。

## HI停止阶段：唯一task已跑且亲读，诊断前置无效，不追加task
Task **7d92e64668684b28a669a4c59bd6511f**，监督10888/child10772，UTC10:55:32.795969–10:55:39.7917439。child Exit2/00000002、Reason exited；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，HasExited/OutputCollected=true，SupervisorError=null，stderr空。外工具未单列数值Exit；监督非MTP无父TRX。child TRX按stdout绝对task路径映射直读 Results/hi-x64-native/TestResults/child.trx，total1/executed1/passed0/failed1/notExecuted0/timeout0/aborted0，RunInfo Error exit2。两控制六帧是内部记录，不是六test。

### 身份与实际会话
actual test task根WpfGfxShape.ComAcceptance.dll SHA98D5ED8E9C3314A02BBF87489C3B9696018139D0CD2C0195CEF5E3888367A3C9，MVID8fd15929-e5ab-4881-b5d9-6a04730655d3，.NET10.0.13 X64。actual PresentationCore.dll同task根，SHA0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31，10.0.13-servicing.26504.104 commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。未调用NativeLibrary.Load候选/官方wpfgfx，不把包依赖称actualconsumer。
实际session1/Active0，WinSta0、测试和owner/input均Default，integrity S-1-16-12288（High），SM_REMOTESESSION0、WTSClientProtocolType0；DWM composition HR0/enabled1。两个HWND14419212/14484748，style90000000/exStyle8、DPI96、client32×16、screen56,48，foreground/cover自身、visible1/iconic0/cloaked0，owner2848。正常窗口可见性并不否定D3D内部occlusion状态。
D3D9Ex factory/device两控制创建HR0，实际swapchain32×16/Format22/count2/interval请求1、swapEffect1及5、HWND及override一致。D3D9 LUID20081=0x4E71，DXGI00000000:00004E71匹配；D3D9 adapter monitor/HWND/DXGI output monitor均65537，DISPLAY1(0,0,1024,768)，BasicRenderDriver vendor1414/device8C。这排除本task简单错adapter/output，不证明内部合成consume。

### 确定前置错误与两假设裁决
本HI选择Format22，但复用 ReadNativeBackBufferPixel 在GpuReadback.cs55固定创建Format21 staging，随后GetRenderTargetData(slot32)要求格式一致，六帧均HR **8876086C / D3DERR_INVALIDCALL (-2005530516)**。未改公共函数；这是HI调用不适用造成的诊断前置失败，不是已证产品失败，也不能据此推翻HH Format21已正确读回或HD成功。
H1：**无法判别**，不是“两种模式呈现都失败”。六帧Clear之后在backbuffer读回阻断，submitted/completed全0，Present字段int.MinValue是未调用哨兵、不是HRESULT；backbuffer/duplicated0、client/screen/front FFFFFFFF均未取得有效样本。不能报告本taskPresent0/GPU完成/桌面固定像素。
H2：**简单输出错链被本task实测排除；occlusion分支获得明确API证据但根因未闭**。六次提交前CheckDeviceState(slot128)均 **08760878 / S_PRESENT_OCCLUDED (141953144)**，即使窗口前台无遮挡。stateAfter均未执行。GetLastPresentCount/GetPresentStats均8876086C，数值0是失败输出，不是目标帧计数；无消费证明。该状态仅属于HI的D3D9Ex配置，不能倒推HH普通D3D9或HD成功期间也occluded。

|控制/帧|frame记录QPC|stateBefore|提交/完成及内容|
|---|---|---|---|
|DISCARD red|35740113291055|08760878|未Present；读回格式错误|
|DISCARD blue|35740123413264|08760878|同上|
|DISCARD red|35740133518831|08760878|同上|
|FLIPEX red|35740144013921|08760878|同上|
|FLIPEX blue|35740154226759|08760878|同上|
|FLIPEX red|35740164365393|08760878|同上|
两控制cleanup均deviceRefs0/factoryRefs0/DestroyWindow1；第一释放QPC35740133755864早于第二window35740133860938，资源独立。duplication各次初始Acquire887A0027，回调在读回时终止，未等到目标内容；不伪造fresh帧计数。系统actual SHA：dwmapi434EDD020C6EB30AEE968004641FCE6B21569E53F21695250B5055C4A93904C2(26100.8875)；d3d9 87E157E78536C66FC413ED202296E39053CC9E14DC8D8381F8A3D18F444A4C0D(26100.9278)；dxgi B12AEBF0F077D6C1394B50E2EECDD5E16072C9A0C871F30887B5D170CBF2EAAE(26100.9444)；d3d11 AE2777002F5A0FF543B5B9BEC5124746559FD01D6C7C0C216D3194632F084502(26100.9278)，均SYSTEM32。

### 停止与下一唯一动作
有效task预算1已用，发布1成功；无部署缺项，无授权条件允许补task。保留显式诊断为失败原件源码，不在本轮暗改后重跑。最早已证HI状态仅到设备创建/Clear及CheckDeviceState；交付最深历史仍HH backbuffer/Present0/GPU完成→无fresh，HDHEHF成功反证保留。下一唯一可执行动作须另授权：只在HI专属实现增加按actual Format22分配staging并GetRenderTargetData的读回（不改公共GpuReadback），然后一次原配置DISCARD/FLIPEX对照，记录PresentEx真实返回及occlusion前后变化。先修确定诊断输入缺项，不能继续泛比较成功失败，也不能先迁移async。

回收根artifacts/hi-desktop-delivery/readback/x64-native；supervision39、业务hi-delivery20行（首长返回截断已按帧分读）、window1全文、stdout1–22/26–60、stderr空、TRX1–15/57–81亲读；其余未读范围随后补核。历史只相关HD/HG/HH/BX/CG节与HD原件，不冒HEHF全原件/方案中段全文审计。默认安全构建已新跑：normal Release/win-x64 ComAcceptance build98 warnings/0 errors，5.34秒，日志58–64亲读；无HI符号，整新增文件不进入默认编译。补核stdout23–25、双window全文、首帧长行分别单读、TRX40–57，关键结论无改变。Available/HWbackend/loss/media资源通知/full四格/x64Event0保持开放，完整替换未完成。
