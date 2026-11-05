# HB 真实 surface consumer 整合验证

## HC 终态：唯一DPI纠正复测仍首帧失败，停止扩大

HC直接执行，沿用本报告。HB进行中及首例失败保留为历史；本节覆盖其“纠正未实跑”状态，不覆盖失败原件。

### 合同核验与改动范围
- 已读执行规则与完整替换方案1–120行。官方Common/Graphics/Generated/wgx_commands.cs 303–322：MILCMD_HWNDTARGET_CREATE为Explicit/Pack=1，92字节；DpiAwarenessContext偏移72/int，DpiX/Y偏移76/84、double。官方exports.cs 2195–2250：DpiX/Y来自dpiScale.DpiScaleX/Y，不是固定字面1；96 DPI的缩放比为1/1，不应写96/96。HB原0/0确属缺项；纠正后1/1已实跑，但未证明缺项是失败根因。
- 生产、共享VisualCache/historical、DRDS、NativeRun、ExitDiagnostics、捕获、WindowQ、项目/打包规则及默认并行均未修改。本轮仅在HB独有显式文件增加target合同记录，保留原DPI纠正、Present、清理和零阈值断言。未增Host/工程，未用终端/脚本。
- producer：D3D9 HAL CreateDevice成功，surface为32×16、D3DFMT_A8R8G8B8(21)、lockable render target；interop输入96/96。target包format=0是HWND默认格式请求，不是强制producer格式；flags实际0。官方初始化还设置PRESENT_IMMEDIATELY|RETAIN_CONTENTS，当前HB包未设置，属于未对齐字段，未擅自追加第二种改参或第三次官方运行。
- 实际窗口矩形(40,40)-(72,56)，32×16；绘制图像(0,0,32,16)，client采样(16,8)→screen(56,48)。专用STA窗口、cover HWND等于target、inputDesktop=Default、foreground相同。GetPixel(window DC)为独立于producer的窗口可见读回尝试，不是目标surface直接读回，也不是已证真实consumer内容。

### 唯一纠正组实际结果
- task **7614cdf951604723acfff73811c75ef2**，x64 official，1执行/0通过/1失败/0 skip；首红expected COLORREF=255，actual=13480843，与HB首例相同。未进入蓝/绿mutation；官方真实三色consumer不成立，不能归因候选产品。
- 监督120秒/外层180秒；子PID8648，监督PID10300；Reason=exited，ExitCode=2/0x00000002，TimedOut=false、KillAttempted=false、OutputDrainTimedOut=false，stderr空。无原生崩溃。专用执行工具未单列外层数字Exit；非MTP监督无父TRX。
- official actual模块：C:\CodingAgentSandbox\Tasks\7614cdf951604723acfff73811c75ef2\wpfgfx_cor3.dll，SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1；10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。
- actual test SHA C9310DFE22EAFD0B5D17479282FA2805907DE05949996D2CDC4C58BB29C686A9；MVID9246c92f-940f-4924-8e9f-23c5027b5267；runtime .NET10.0.13/X64/ptr8。WPF10.0.13-servicing.26504.104+同commit，PresentationCore SHA0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31。
- 包候选SHA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0由aot.sha256及初始化诊断确认，仅是包文件，不是本task实际候选消费；候选未运行，x86候选75D136F71223DB9EA0830BFBF9CDFC0EAAB748B5067397B682E42323B698EC30未重发/运行。
- HRESULT=0：producer device/surface、interop、partition initialize、async connect、channel、四resource、image update/draw/visual、target create/root、setup close/commit/flush、ColorFill、dirty、present separate send/commit、consumer flush及全部17次DwmFlush。
- 必要观测顺序：producer完成→dirty→DuplicateHandle1724转交→separate present成功→commit→event signalled=true→consumer flush→17次DwmFlush→首红像素失败→detach。事件只证明完成信号，不等于消费。该trace不是nativecallback轨迹。
- detach before/after均到达；倒序handle4/3/2/1 unregister HR0、Deleted=1；delete close/commit/flush HR0；destroy HR0、disconnect HR0、partition deinitialize HR0；bitmap/surface/device/factory Release最终全0；cleanup complete到达。调用DestroyWindow但其返回未独立记录。duplicate无测试侧直接CloseHandle，计数0为代码行为，不是已直接观测native exact-once-close；因首帧断言，后续原event reset/set自检未运行。Available仍未覆盖。

### 证据回收与隔离
绝对本地根：d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\hc-surface-consumer\readback\x64-official。
- task绝对TRX：C:\CodingAgentSandbox\Tasks\7614cdf951604723acfff73811c75ef2\Results\hc-x64-official\TestResults\child.trx。
- 本地TRX：上述根\Results\hc-x64-official\TestResults\child.trx，全文50行亲读，Counters与断言核实。目录枚举未展示该子目录不等于缺失，已用精确路径亲读。
- 同目录上级supervision.json39行、stdout29行、stderr空、hb-consumer.jsonl66行、stages.jsonl66行均全文亲读；Diagnostic/log_261023092338678.diag完整回收但未全文阅读，不声称所有诊断文件均亲读。目标合同、像素及所有HB事件/refs业务原件已全读。
- 正常分析器x64自包含10.0.13发布成功（61行日志，末段亲读）；obj/bin/publish显式隔离到artifacts/hc-surface-consumer，PublishDir显式指定。现有正式打包目标在独有根清洁生成com-acceptance-package，包清单419行，MTP/runtime与Contracts/aot.sha256存在；发布源与readback分离。未重发生产，也未将旧SHA绑定新生产源码。
- 最后测试源码修改后，default Release/win-x64正常build成功：98 warnings/0 errors；日志59–64亲读。HB文件整体#if HB_SURFACE_CONSUMER，默认build不含该场景；这是隔离构建验证，不是默认full运行或新生产NativeAOT产物。

### 收口与下一唯一切片
本轮新增actual task仅1；连HB历史总计2次官方x64（均首红失败）。纠正后四格预算仅消耗official x64一次；candidate x64及official/candidate x86均0，不以缺少其它三格声称四格通过。不再第三次官方乱改。
DPI缺项是经官方合同确认的输入问题，但修正对当前可见像素失败无已证改善；不得称已定位根因。producer ColorFill和GPU完成成功并未直接读surface像素；目标surface实际内容未知；窗口读回失败；独立consumer三色内容未证。consumer实际backend尚未独立观测，不能称HW或软件RTB通过，也不能从producer HAL推断consumer HW。未调用GetAsSoftwareBitmap冒充consumer。
下一唯一任务是共享HWND consumer夹具的内容链诊断，不是生产修复：先静态认证flags/update-window参数与官方合同，再在明确独有诊断切片记录真实consumer backend、目标内容/Present与桌面窗口读回的对应边界（producer校验只能另标控制），保留present/cleanup/严格颜色断言；不能靠重复改参追Green。本轮不实施该共享切片。
full/Available/HWloss/媒体及资源通知/独立x64Event0和最终双架构四格均未闭，完整替换未完成。

HC最后补记：本报告保留HB原文；三共享新增HC终态覆盖HC进行中与HB待跑状态，HA及更早历史不改。本报告全文与三共享本轮受改节、最后补记均需写后回读；长历史不宣称全文复核。

## HB 进行中（历史）

直接执行；主代理仅监督。先审执行规则、完整替换方案、HA/GZ、DS、DQ 与既有 VisualCache/historical 真正调用。仅增加独有 HB_SURFACE_CONSUMER 场景及同 exe 必要显式分派；共享夹具先只读。禁止终端/脚本/第三工程/Host/机器码探针。

保留 HA 已验收生产 Stdcall 修改；复用候选 x86 SHA 75D136F71223DB9EA0830BFBF9CDFC0EAAB748B5067397B682E42323B698EC30，x64 SHA 8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0，不重发生产，不重跑已验 DR，不运行官方 Event0 违规路径。

## 执行切片

1. 审核既有真实 consumer 与官方 channel/partition 初始化及异步所有权。
2. 确定初始色和两次 mutation 的独立 consumer 内容证据；事件不是内容。
3. 若 API/设施不支持独立证据，记录精确阻断，不伪造 HW/SW 成功。
4. 可行时仅独有测试、正常分析器、隔离 obj/bin/publish/package；先一架构官方/候选，再另一架构，最多四格各一次，原生崩溃停止扩大。
5. 读取监督/TRX/identity/内容/事件/释放原件并验证 default normal build 隔离。
6. 更新报告和三个共享入口并回读。

## 实际证据

首例task6264c60805ad4c5387fb3f446057fd4f已回读：x64官方1执行0通过1失败，Exit2/nonTimeout/noKill，无原生崩溃；真实surface事件完成、flush及cleanup/refs0，但首红窗口像素13480843而非255。未进入mutation，不算consumer通过。发现共享target包DpiX/Y为0，与官方exports.cs真实初始化1/1不符；仅独有HB包纠正两个double，允许一次有信息输入纠正，保留首例现场。未宣称环境失败或候选缺陷。Available、HW 消费恢复、device-loss、media/resource 通知、full 四格及独立 x64 Event0 仍开放；完整替换未完成。
