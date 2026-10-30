# HN 显示源与 DWM occlusion 有界调查

## 进行中：执行前登记
正式发布0/task0。保留HM首红08760878、HJ/HD/HE/HF历史成功与HL失败原始语义；基础9在HM未执行，不记候选失败。生产HAStdcall/fill、共享consumer、默认诊断与打包不改。

只读采集设计：HWND → MonitorFromWindow/HMONITOR → GetMonitorInfo设备名 → QueryDisplayConfig source GDI名称 → adapter LUID/sourceId/targetId；每个快照记录QPC，与submission关联。QDC_ONLY_ACTIVE_PATHS表示配置路径激活，不证明物理扫描或目标提交被DWM消费；allpaths仅补充配置，不冒充active。EnumDisplayDevices/current mode表示系统公开配置。WTS/desktop/input desktop/屏保/远程协议只按各字段合同解释；不能由Active或foreground推断可见。DWM cloaked、扩展边框、timing只证明相应属性或全局计数，不证明目标帧消费。系统电源若无同步只读当前显示状态API，则明确不可得，不注册改变状态的机制。

SDK布局必须精确验证，API失败保留error与不适用层级，不能输出自洽但伪造拓扑。仅HN_DISPLAY_STATE显式编译，最多一有效task，HM精确Ex owner首红一次；失败停止，不跑基础9/consumer/connect/Event0/full/变体。严格原expected，无skip/fallback。采集、实测、停止、边界阶段追加保留历史。

## 采集发布阶段
发布2次/task0；第一次误取现行依赖SHA2BFC4AC…，未上传消费；第二次改用HM留存HA依赖，实际aot.sha256=8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0，不重发生产。正常分析器、无NoWarn；独有hn-display-state obj/bin/publish/package、显式PublishDir、双framework10.0.13，419行白名单末Native/Contracts/aot亲核。离线API确认唯一HN精确方法存在，无HM/HI/HL方法。仅Ex owner首红一次。布局LUID8/source20/target48/path72/mode64显式union/sourceName84/targetName420/MONITORINFOEXW104/DEVMODEW220/DWM timing pack1；BOOL=int32，CCD直接返回LONG错误不读取LastError。WTSINFOEX level1 union按8字节对齐，失败原样分层。系统同步当前显示电源不可得明确记录。

## 实测与停止阶段（唯一task，已回收核证）
Task 3a45024c5717468daa6f2ee3174ae76c，UTC 2026-10-23 11:48:26.5353251–11:48:34.892774；监督9728/child1400，创建线程5336/owner984。发布2/task1，无重试。监督Exit2/00000002、exited，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，HasExited/OutputCollected=true，SupervisorError=null，stderr空。外工具未提供独立数值Exit；监督非MTP无父TRX。
绝对TRX C:\CodingAgentSandbox\Tasks\3a45024c5717468daa6f2ee3174ae76c\Results\hn-x64-native\TestResults\child.trx，本地映射artifacts/hn-display-state/readback/x64-native同相对路径，1–65全文亲读：total1/executed1/passed0/failed1/notExecuted0/timeout0/aborted0。普通断言失败，不skip。监督39/stdout44/窗口2行全文已读，业务282行中的身份、活动路径、模式、会话、电源不可得、DWM、首红/cleanup与模块节已读；allpaths重复非活动组合未逐行全文审，不冒全282行亲读。Diagnostic整task回收未全审。

### API真实性、关联图与错误层级
HWND 0x840570（8652144）→ MonitorFromWindow 0x10001 → GetMonitorInfoW成功\\\\.\\DISPLAY1 → CCD source名称成功、LUID0x4E71/sourceId0 → target同LUID/id5527112/HyperVMonitor/路径DISPLAY#MSH062E…UID5527112；同时D3D9Ex GetAdapterLUID=0x4E71、adapterMonitor=0x10001及DXGI outputName/monitor一致。不是仅凭结构大小自洽：原生GetDisplayConfigBufferSizes/QueryDisplayConfig与source/target device info均返回0，独立GDI设备/模式/monitor及D3D/DXGI交叉对应。
前后QDC_ONLY_ACTIVE_PATHS均1路径/2模式、pathFlags1/sourceStatus1/targetStatus1/targetAvailable1，sourceMode索引1=1024×768/像素格式4/位置0,0，targetMode索引0有效。QDC_ALL_PATHS均101路径，实际返回2模式；其余是非活动组合，不能把101当101个显示器。独立EnumDisplayDevices的DISPLAY1为Microsoft Hyper-V Video，flags5（attached+primary），monitor flags3；DISPLAY2–11 OrayIddDriver flags0，存在不证明影响本窗口。EnumDisplaySettingsEx current mode成功220字节/32bpp/1024×768/64Hz，与CCD 64/1对应。Technology=FFFFFFFF即OTHER，非错误码；targetFlags5按名称flags位含friendlyNameFromEdid与edidIdsValid，不是路径active位。CCD失败本轮未发生，unsupported/accessdenied未观察，不能泛称所有机器支持。
前快照QPC35771840496168–35771840994575，后35771914820506–35771915163814，夹住submission35771851616325；非原子事务，不能排除快照间瞬态。WTS session1 state0=Active、protocol0=console，WTSInfoExW level1/232字节/sessionFlags1=UNLOCK（当前Windows26100，不适用Win7反转语义）；input/current/owner desktop Default，屏保SPI_GETSCREENSAVERRUNNING成功且0。均无锁定/屏保/远程协议阻塞证据，但不证明物理扫描。当前显示电源未取得：未将SYSTEM_POWER_STATUS电池字段冒显示电源，也未注册通知冒当前态。
DwmGetWindowAttribute CLOAKED成功0；exStyle8/noRedirectionBitmap=false，只说明没有该style位，不证明重定向面已消费。DwmGetCompositionTimingInfo(NULL)成功/SDK pack1大小292，QpcVBlank/QpcCompose35771836562500→35771867187500，Frame/FrameSubmitted1→2、FrameConfirmed0→1，FrameDisplayed/Complete及QPC均0、Outstanding1。只代表全局timing字段，不推导本HWND已显示或DWM停摆。

### 精确HM首红结果
SDK32/adapter0/HAL1/behavior0x26，Format22/32×16/DISCARD1/count2/interval1/windowed1，其他创建字段0；实际descriptor逐项HR0。测试线程创建/owner提交，原窗口style94000000/ex8、40,40/client32×16、DPI96，foreground/cover自身、visible1/iconic0/uncloaked，与历史合同一致。Clear及staging acquire/descriptor/create/copy/lock/unlock均HR0，FFFF0000/Pitch128，staging/backbuffer refs0。
CheckDeviceState-before/真实PresentEx slot121/after全部141953144=08760878 S_PRESENT_OCCLUDED，不是S_OK。submission35771851616325，返回35771851620651，GPU完成35771856698282。window/screen00CDB38B/front FF8BB3CD（frontHR0），DXGI create0但initial887A0027及后续10次887A0027，无lastPresent/fresh；duplicated0是未采到，非黑帧。count/stats8876086C，输出零不证明零提交。仅首红一次，accepted=false，未跑余色/基础9/consumer/connect/Event0/FLIPEX/full。cleanup device/factory refs0、Destroy1 qpc35771915303714。

### 实际身份
actual task test SHA B8A09B8F3409196BA2A02DE93CC1F9701D95F5BFCABFE9AC7AD29BFA63DFBC0B；MVID50170539-8b25-467e-88d2-f64088a0c14c，.NET10.0.13/X64。task根PresentationCore SHA0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31，10.0.13-servicing.26504.104+4099f83977f4d29604a9aeb971fb0a7bc1d5f699。实际系统C:\WINDOWS\SYSTEM32：dwmapi.dll 10.0.26100.8875 SHA434EDD020C6EB30AEE968004641FCE6B21569E53F21695250B5055C4A93904C2；d3d9.dll 10.0.26100.9278 SHA87E157E78536C66FC413ED202296E39053CC9E14DC8D8381F8A3D18F444A4C0D；dxgi.dll 10.0.26100.9444 SHAB12AEBF0F077D6C1394B50E2EECDD5E16072C9A0C871F30887B5D170CBF2EAAE；d3d11.dll 10.0.26100.9278 SHAAE2777002F5A0FF543B5B9BEC5124746559FD01D6C7C0C216D3194632F084502。HA 8F74588…仅包依赖未消费，无候选责任判定。

## 边界与下一唯一行动
已证配置active/source attached/目标关联正确及采样时未锁屏、屏保关闭；未证实际scanout、显示电源、DWM目标重定向面消费与occlusion内部原因。没有明确topology/session缺项，故不提出武断系统修复，不自动归环境关闭产品门，不宣称Basic Render Driver或某驱动永久不可用。HJ/HD/HE/HF历史成功保留，HL/HM历史失败亦保留；本轮未覆盖基础9。
下一唯一行动：由具备Windows图形ETW采集权限/工具的环境提供同一目标HWND/PID/TID、adapter LUID/sourceId/targetId、QPC关联的DWM-Core与DxgKrnl原件：Present token/sequence及结果、surface/redirected HWND关联、DWM schedule/latch/consume/discard或occlusion状态及原因、VidPn source/target active与scanout/VSync/flip提交/完成、电源/session转换时间；具体event id/字段依该Windows版本manifest核实，不能臆造固定事件号。内核图形会话通常需管理员或获授权的trace权限及匹配符号/manifest，可能需宿主Hyper-V显示端证据。当前工具无现成ETW采集/解码能力，本轮不建ETW平台、不跑外部命令、不改显示/会话/电源/registry；只读API不足以替代该能力。若未来真实正锚成功，下一轮才consumer旧原场景，不能将相关快照当因果。
Available/HWbackend/loss/media/resources通知/full四格/x64Event0继续开放，完整替换未完成。

## 最后补记：默认隔离与证据覆盖
最后源码后default Release/win-x64正常分析器构建98 warnings/0 errors、10.06秒；离线正式默认DLL API无HN/HM/HI/HL方法。仅新增HN诊断文件与HI partial显式分支及四份文档，未修改生产/NativeRun/ExitDiagnostics/VisualCache/historical/WindowQ/打包/默认并行。上传只独有clean package，源与readback分离；原package-files.txt419行保存（首26及末20行已读），runtimeconfig/aot已核；当前专用文件工具未提供总字节统计，未伪填精确字节或宣称逐项全hash核验。历史HM业务26行存在截断尾、HJ仅读取首5行与HM报告中的合同引用、HL仅读报告相关节，未声称历史原件全文审。HN业务重复allpaths未全文亲读的范围已如实登记；这不改变已直接读取的active路径/session/帧结果，但完整逐项原件审计要求尚未全部满足。未新增第二task补跑。



