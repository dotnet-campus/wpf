# CG：paint / DWM 桌面交付有界诊断

## 1. 裁决与范围
**两项控制均未恢复交付，未证实paint时序根因；无生产修复。** 已撤回填绿/强制重绘实验，仅保留 `CG_DWM_DELIVERY_DIAGNOSTICS` 显式只读观测。默认与显式Release x64发布成功；最终只读诊断失败，默认fresh选择谓词四例通过。不是候选、物理GPU、loss恢复或产品full验收。
工程根 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`；B=`artifacts/cg-dwm-delivery/`。本报告先创建后更新。无终端/脚本、新Host/项目；不改生产、曲线、VisualCache、historical、NativeRun、ExitDiagnostics、WindowQ、项目打包、共享交接、默认并行或CF文件。

## 2. 审计与两个控制
已读执行规则27行、完整实施方案1–120、CE/CC/BX/BJ报告主要内容和补读尾段；历史报告部分首次长返回截断，不声称其所有原件重新全文亲核。核对OutputWindow、WindowThread、PresentDeliveryControls、DesktopDuplication、CaptureFreshFrame、csproj/global.json/目录属性/打包规则。
OutputWindow独有class、Background0/class Style0；exStyle8 TOPMOST、style0x90000000 POPUP|VISIBLE、32×16；无NOREDIRECTIONBITMAP/LAYERED。WM_ERASEBKGND返回1不绘制，WM_PAINT仅ValidateRect，其余DefWindowProc；owner为STA GetMessage/DispatchMessage。未发现默认GDI填充、隐藏主动重绘或paint覆盖；BX已移除WndProc同步I/O，本轮不加回。
仅两个数据行，每行独立新CG窗口/设备，swapEffect1、D3D9 HAL type1、flags0x26、32×16/format21/windowed1、owner提交、目标HWND override、red/blue/red：
1. 独立无GDI窗口：无背景刷，paint仅验证，整个窗口不执行GDI绘制。
2. paint填绿窗口：每帧Present前RedrawWindow INVALIDATE|ERASE|UPDATENOW，经WM_PAINT BeginPaint/FillRect/EndPaint填绿，GdiFlush/DwmFlush后提交；其后不主动重绘。
WndProc仅128条有界内存事件，超限计数，无文件I/O或桌面/WTS查询。逐帧前/完成后/等待后采集paint、坐标、覆盖、style、DWM timing；现有duplication机制增加仅显式线程局部内存钩子，每帧最多32条，结束才写JSON。既有捕获Console日志保留，不声称诊断零时序扰动。

## 3. 两控制实际业务结果
任务 `66f4376505d04a8f990af4a1abf0df46`，B/run-x64。`Results/cg-dwm-False.json`726行、True762行已全部分段回读。六帧expected=backbuffer red/blue/red、Present0、完成QPC晚于提交、accepted=false。

|控制/帧|提交/完成QPC|paint/erase 前→完成后→等待后|client/screen；front|
|---|---|---|---|
|无GDI red|33059758588924 / 33059763724194|1/0 → 1/0 → 1/0|13348487；4287082187|
|无GDI blue|33059834870159 / 33059840011213|1/0 → 1/0 → 1/0|同上|
|无GDI red|33059911422378 / 33059916599375|1/0 → 1/0 → 1/0|同上|
|GDI red|33059990513391 / 33059995704095|2/1 → 2/1 → 2/1|65280；4278255360（绿）|
|GDI blue|33060069695347 / 33060075007096|3/2 → 3/2 → 3/2|同上|
|GDI red|33060148755923 / 33060153897574|4/3 → 4/3 → 4/3|同上|

frontResult均0。HWND83952896/28181154，owner2744。18个观测点rect[40,40,72,56]、client[0,0,32,16]、screen[56,48]、cover=目标、visible1/iconic0/cloaked0、实际style0x94000000（含CLIPSIBLINGS）、exStyle8。离散点不证明全区间连续无遮挡。消息/acquisition未丢弃。
六帧各10次Acquire均 `0x887A0027 DXGI_ERROR_WAIT_TIMEOUT`，无resource/像素；duplicated=null、acquiredPixel=false、blackFrame=false。LastPresentTime/lastMouse/AccumulatedFrames零是失败调用占位，不是有效元数据；无成功pointer-only帧，不能称黑帧。
DwmGetCompositionTimingInfo(NULL)全部0，pack1/size292；vblank/compose递增，全局frame/submitted=1…18、confirmed=0…17，displayed/framesDisplayed/displayedQpc/completeQpc均0。全局timing不是目标重定向surface消费事件，不能证明消费提交，也不能单凭零字段断言DWM故障。
TRX实际backend Microsoft Basic Render Driver vendor1414/device8C、LUID00000000:00004E71、DISPLAY1 1920×1080、rotation1、output/window monitor10001。BJ历史同adapter red/blue/red成功保留，不能断言Basic永不产帧。
判别：独立无GDI窗仍失败且没有后置paint，否定“必须先有GDI才失败”和“Present后WM_PAINT覆写”的充分解释。预paint绿真实生效但不恢复D3D桌面色或fresh duplication。WM_ERASE仅计数/返回1，未独立切换背景刷，不伪装第三控制。

## 4. 父TRX、监督与身份
三个父业务TRX均从监督的绝对TestResults路径直接读取并完整回收，不因目录检索漏失宣称没有。帧数不作父测试分母。

|task / B下本地路径|total/executed/passed/failed|监督|
|---|---|---|
|66f437… run-x64/cg-control/TestResults/child.trx|2/2/0/2，Failed，Counters156行|Exit2，08:28:16.8968027–08:29:04.5714745Z，业务PID9932/监督9688|
|efa7d62d8b9f43349bef9870f38cc113 readonly-run-x64/cg-readonly/TestResults/child.trx|1/1/0/1，Failed，Counters90行|Exit2，08:31:10.5410689–08:31:34.4866905Z，6368/10684|
|0142efbb93d8447088ef6c66f637bf61 default-run-x64/cg-default/TestResults/child.trx|4/4/4/0，Completed，Counters51行|Exit0，08:31:54.3130853–08:31:55.0951812Z，1480/10708|

绝对远程路径 `C:\CodingAgentSandbox\Tasks\<task>\{cg-control,cg-readonly,cg-default}\TestResults\child.trx`；绝对本地路径=上述工程根+B+表中路径。三个监督JSON各38行全文亲读：Reason exited；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，HasExited/OutputCollected=true，SupervisorError=null。TRX异常、timeout、aborted、notExecuted等全0。失败为内容截止断言，不是监督timeout。关键结果/身份/计数亲读；两份失败TRX重复stdout未逐字全文重读，不冒称全日志复核。
实际runtime均.NET10.0.13、X64/pointer8，testPath各task根WpfGfxShape.ComAcceptance.dll：
- 两控制SHA `CABE525EFE49B45A30291DD5316B8886089A83DE9C35BDBAA5DF38FFE9EE8CDA`，MVID `a23e5f5b-9c81-4d44-a9bb-b66f70db023d`。
- 最终只读SHA `194356F2CE86808ACC4B9063CF0C61479CDD15EEF10660BD65D92DFDCA539CE0`，MVID `7dbe3159-eb9c-4d92-b3b7-5b6afefa43d8`。
- 默认SHA `DC36B90284606B4FC400F6B2CFC0F866095F7983F887CA2A474BD77E7F1BED79`，MVID `0ca1a06c-5f14-4abc-b953-4b674938fd21`。
前两份业务identity与监督一致、aot-identity全文亲读；默认身份取监督/TRX。依赖明确选工作区 `.dotnet/shared/Microsoft.WindowsDesktop.App/10.0.13/wpfgfx_cor3.dll`；task Native实际官方SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，不是候选。诊断backend是系统D3D9/DXGI，不把官方依赖作候选验收，不重发生产。
CC限制原样继承：仅x64、父TRX缺失、官方依赖非候选。CG完整TRX不倒填CC；CE/CD软件nested双架构不替代桌面交付；BJ成功不移绑本次SHA。

## 5. 最终源码与验证
仅三个文件：新增 `D3DImageTests.DwmDeliveryBoundary.cs`；`D3DImageTests.DesktopDuplication.cs`仅显式内存钩子；本报告。无其他代码/共享修改。
未证实根因，因此最终已删除GDI字段、BeginPaint/FillRect/EndPaint分支、RedrawWindow调用和相关结构/声明，只剩独立无GDI窗与只读事件/状态/timing。默认不编译诊断测试/钩子。未修改获取、截止、fresh筛选、颜色、清理或默认并行。
正常依赖图，独有B/explicit、B/readonly、B/default根派生obj/bin/publish/package。初次CA2219通过提前捕获owner修正；CS1686通过createdDevice局部取地址修正。一次截断日志不能冒称成功；临时WarningLevel0仅用于显示错误，最后成功发布均未带此开关、未降代码分析。默认与最终显式Release win-x64 self-contained发布成功，有既存warning，不称零warning。
撤回后最终只读实跑1/1失败。CH授权补审 `readonly-run-x64/Results/cg-dwm-False.json` 全文726行（分段1–70、71–155、156–240、241–310、311–395、396–480、481–560、561–645、646–726），补齐三帧：expected=backbuffer red/blue/red；Present均0；提交/完成QPC分别33061495370351/33061500421546、33061571858024/33061577012703、33061648280290/33061653436672；全部accepted=false、duplicated=null、acquiredPixel=false、blackFrame=false。九观测点paint1/erase0未变，client/screen13348487、front4287082187且frontResult0；每帧10次Acquire均-2005270489（WAIT_TIMEOUT），无像素、无有效LastPresentTime/AccumulatedFrames，acquisitionDropped0。DWM全局frame/submitted1…9、confirmed0…8，displayed/completeQpc等0；这些不证明目标surface消费或paint因果。身份194356F2…/7dbe3159…与最终只读监督一致。CH亲读三监督全文及三父TRX计数，确认两控制2/2失败Exit2、只读1/1失败Exit2、默认选择谓词4/4 Exit0；未重读旧两控制全部JSON，旧控制细节沿用CG亲核记录。默认fresh选择谓词0/99/100/101只验证时间选择，不是硬件新帧交付。旧两控制完整JSON绑定旧SHA，不移绑最终源码。
未跑x86：本轮隔离窗口paint变量，x64已否定其充分解释，无交付恢复/候选变化，不重复无信息双架构/全矩阵。x86本变量仍未覆盖；未跑PublicHwnd/full。

## 6. 精确剩余边界
两控制实际给出：**Present后没有paint覆盖；独立无GDI与预paint绿均在D3D9提交完成之后、桌面可观测输出之前失败；DWM全局timing可读，但无有效duplication帧。** 没有证据支持修改默认WndProc、设备线程、override或异步surface合同。
未直接观测目标重定向surface像素/更新事件及其与提交关联；query/DwmFlush/global timing不等于该证据。没有成功Acquire，LastPresentTime/AccumulatedFrames不可用，不能由占位零推断DWM未提交。未独立切换class背景刷、composition policy、无重定向style、会话/驱动或捕获生命周期，不将未知项称根因。
行为实验已撤回，保留显式只读探针，普通验收不增加绘制/重绘。最终桌面交付仍Red，候选硬件验收、x86控制、物理GPU、loss恢复及产品full未闭，无生产修复声明。
