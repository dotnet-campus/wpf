# KA：窗口 sequence 专属角色路由修复

## 最终裁决：有限保留，两个原 ID 双架构真实内容通过
已实际执行，不是待裁决或仅角色采集通过。两个原 ID 的错误64在本次 x86/x64 有界过滤验收中均实证消除；原业务 original/candidate 各自在独立进程完成，父执行原12帧零阈值比较。保留修复。不宣称硬件窗口呈现像素/device-loss/完整替换通过。

## 原始失败与根因
亲读 JV/JY 报告及 JX 分类 TSV42–43、61–62，失败JSON x64:563–572/577–586、x86:213–222/227–236，原父TRX x64:3843–3862、x86:3671–3692。
- `170ff4ac-8c18-8c83-8b35-77294231a78e`：WhenWindowImageUpdatesAndResizesThenFreshFramesAndCallbacksConverge。
- `1e7cbafa-f206-8321-a8c4-2c890e8cc69e`：WhenWindowSolidRectangleUpdatesAndResizesThenOfficialContentMatches。
四条原断言都是 RunIsolated(arguments[index+1]) 实际64（第二方法 rectangle:true），不是原生进程 Exit64 的泛化归类。ExitDiagnostics 为测试父设置 WPFGFX_RUN_ROOT，并传绝对 results-directory；旧测试据通用输出根误判业务角色，把绝对路径交给只接受 original/candidate 的 RunIsolated，直接64，未进STA。

## 最小修改与合同
只改 ReplacementStateSequenceWindowTests.cs，新增内部 WindowSequenceRoleIsolation.cs；新增默认回归 `13baa1a6-edce-89fd-892d-6ad5de576148`。另改本报告及三共享自有短节。
显式 KA_WINDOW_SEQUENCE_ROLE + KA_WINDOW_SEQUENCE_METHOD 在子 StartInfo 设置；两标记都没有才走父协调，部分标记/错误角色/方法不符直接断言失败。没有读取 results-directory 作角色，没有 catch64/无role空Pass。父协调两角色后检查有界退出、监督、真实方法receipt、业务PID和单条Passed TRX，0tests/缺TRX/非零退出/错误角色不能通过。
复用同exe --supervise-exit 和现有receipt/TRX模式，不改 NativeRun/ExitDiagnostics/公共隔离/JM/JD/默认线程/项目/生产。父只协调，不设置WPF resolver。原 RunIsolated/Run/RunCycle/内容oracle/resize/Rendering/close/unsubscribe/producer GC断言均未改；仅将角色完成输出复制到原比较布局，仍实际读取两边原始pbgra32比较，非golden或fallback。
子监督65秒，父等待75秒，kill后wait5秒，drain3秒及取消后drain3秒均有限；finally写parent-role-exit，错误不吞为Pass。保持全局并行设置；无终端、脚本、新Host/工程、NoWarn、skip、阈值/expected修改。JZ VisualCache三处包长及独立回归未动。

## 正式构建与离线发现
两次专用publish成功，Release self-contained net10.0-windows / runtime10.0.13，正常依赖及分析器，无探索define。独有 artifacts/ka-window-role/{x64,x86}/{obj,bin,publish,com-acceptance-package}；显式PublishDir，既有包target clean仅各自com-acceptance-package，不从PublishDir父或历史readback上传。AotDllPath复用 artifacts/jv-postfix-full/{arch}/com-acceptance-package/Native/wpfgfx_cor3.dll；ReferenceData同源。生产没改没重发；JR源码闭包沿用已核历史，不冒称本轮逐字节全源码核验。
两次正式exe --list-tests过滤该类都发现4方法，discovery task x64 469a33b0ea5946068696b3d0f6040c49 / x86 b42ef2fbabe247b9b0474ec97bbf40bb；未执行业务。之后每架构仅一次组，BL120/外180。

|架构|业务组task|父PID / 监督PID|总/执行/通过/失败/skip|监督Exit|
|---|---|---|---|---|
|x64|8e3fe0045dd24c5a9ca940a26e3a617f|11016 / 2752|4/4/4/0/0|0|
|x86|90cdc21626a5408bb613ab3db08a5f1e|6188 / 9232|4/4/4/0/0|0|
父TRX完整末尾44行均直接读，两个原ID及新回归和原空/旧帧负控Passed，ResultSummary Completed。工具返回“沙箱执行完成”及回收路径，不暴露单独数值工具码，不编造；监督JSON实际ExitCode0，角色parent receipt exit0，业务receipt exit0分别核验，层级不混。

x64 testSHA `0FD01DF02892214AC7E3B7C31F1E4B76A5AE2FAEA75B5309475D30E722F739E9`，MVID `bf2160c7-144c-494f-900d-dcd9ea47cc76`。
x86 testSHA `44E8185DC052D67DB9BB3A9C406D05729281DCC654CAF9FF8D57E1D4804811A6`，MVID `d52cca44-f8fa-4b42-9c23-738980463c7e`。
真实候选 loadedNative/resolvedNative：x64 `1782797EA3271F9426BFEDFC67F30D42834858C778C3ECADF6A40C1AC692CF9A`；x86 `1DFB33772ACC5FFCD3A5D9D865867ACA9F0EA33A6DD7A8F59113056098E744DF`。实际官方根模块 x64 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`；x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`，WPF10.0.13-servicing.26504.104。已直接读四个场景两边report尾部实际module，不只是aot.sha256。

## 原件定位与实际业务/清理
以下相对 artifacts/ka-window-role/readback/{arch}/Results：
|架构|image根|rectangle根|稳定控制根|
|---|---|---|---|
|x64|replacement-state-window/f3181295cf604429b64abd618237ece3|replacement-rectangle-window/a35444f123fc4d7c8e061028ccc05b06|ka-window-x64/ka-window-control/9c547bd5d9c34f37b42d0f792a6940bb|
|x86|replacement-state-window/245d87a1af8c4ad4b7d2125cdc630961|replacement-rectangle-window/3b044df192bb4e439e717c6e66e81158|ka-window-x86/ka-window-control/4e9b32a4473a4f93abbcf88a5606a4ed|
父TRX绝对路径为 `d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/ka-window-role/readback/{arch}/Results/ka-window-{arch}/TestResults/child.trx`；相邻supervision.json已全文读。每个场景根/{original,candidate}/TestResults/child.trx 为roleTRX，相邻window-role.json、parent-role-exit.json、supervision.json，业务report/events/resources与frame/source原始文件同目录；未flatten的原始业务副本还在角色同名子目录。
image candidate业务PID x64 1816 / x86 6400，rectangle candidate2464/10164；control original2968/5500，candidate5792/7576，均不同父及各自监督；receipt记录正确方法、executed=true和exit0。role TRX image候选两份全文亲读单条1/1Passed，其余由父Validate强制验证，不能冒称逐行读完所有角色TRX。
两场景comparison.json各12/threshold0/exact=true；rectangle完整12项partial-content-evidence亲读，1536字节(24x16)和3840字节(40x24)，四颜色SHA不同、exact/nonempty均true。测试真实逐字节比较，不声称人工逐字节阅读二进制。原新帧非空/非旧帧及实际HWND尺寸增长断言执行，三轮每轮红蓝绿黄。事件原件已读首轮和最后轮：ActualWidth/Height40/24，close-drained closed1、callbacks9=unsubscribe9。resources三轮producerAlive均false，report dispatcherShutdown=true/exitCode0；没有把短三轮数据外推任意时长无泄漏。
稳定控制在父已有通用绝对输出根下仍启动两正确方法，真实加载对应模块，独立PID；parent-state两架构dispatcherExisted=false，shutdown=null，owners维持PresentationCore/WindowsBase集合，通用outputroot未变，专属role/method未写父环境。父没有调用SetDllImportResolver，业务resolver仅在独立STA角色中调用；未增加直接探测父resolver注册状态的公共设施。父/角色receipt无timeout/kill/wait/drain取消超时，drained=true/errors空，父监督亦无timeout/cancel/kill/drain错误。没有native crash，未扩跑full或随机显示重试。

## 默认构建与下一唯一任务
最后源码后正常默认ComAcceptance Release build成功，专用日志Exit0，130warnings/0errors/1.46秒，正常分析器。构建不是内容运行替代，上述实际双架构组才是有限运行证据。路由及原内容关键门已过，保留本轮修改，无需撤回；旧JV失败原件保留，不改旧full44/19、不相减推新full。
原合同本来就是真实HWND producer + RenderTargetBitmap软件内容consumer；HWND呈现像素和HW backend没有验证，device-loss未触发，renderTier2仅capability。本轮不是HW显示恢复，不用此Green覆盖JZ原11项显示前置失败。
下一唯一生产根因任务：ShaderBounds `105b96c7-ae45-8225-b7ac-be58d2f59b51` 的真实资源上界/BOOL恢复语义：1048580 disabled0官方白/候选红，1048576两边红；不改golden/阈值。不在本轮执行。JZ保留其可见首帧恢复后的11项条件验收交接。Available/HW恢复/mediaresource/full四格继续开放，完整替换未完成。
