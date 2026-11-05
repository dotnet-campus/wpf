# 当前源码 x64 source-rebuild 两有限合同最终收尾

## 裁决与实际预算

**本轮 x64 postfailure/isolation 两个有限 ABI 合同通过，当前修正可在该有限范围保留；限定来源链闭合，完整替换仍 Open。** 不把这两个合同当作全部二进制兼容、资源、时序或恢复能力验收。

工作区为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape`；下述 R 为工作区相对 `WpfGfxShape/artifacts/postfailure-source-rebuild-x64-r1/`。本报告接续原执行，不重新构建、发布或重复已经完成的 official postfailure。

|独立账|本轮累计|本次接续新增|实证结果|
|---|---:|---:|---|
|normal COM Release/win-x64/net10.0-windows build|1|0|Exit0，133 warnings / 0 errors|
|Native Release/win-x64/net10.0 publish|1|0|Exit0；记录可见 Generating native code 及创建 lib/exp|
|正式 COM self-contained publish|1|0|Exit0；显式新 Native、win-x64、两框架10.0.13、正式 clean|
|Windows 沙箱|4|3|postfailure official→candidate→isolation official→candidate，各1/1/1通过|
|源码/项目/测试/资源/依赖/SDK/包规则修改|0|0|只写文档和 ignored artifacts|
|发现/full/x86重跑/显示/Native追加/单元追加/重试/终端脚本/再委派|0|0|未执行|

所有沙箱均以 R/clean 为 sourceDirectory，执行 WpfGfxShape.ComAcceptance.exe，不从 publish 根运行；独立任务/独立进程/唯一 Results，内部120秒、外部180秒。使用 --supervise-exit 的精确 FullyQualifiedName 等值筛选。每一任务完成后立即保存工具原返回，完整读取 TRX、监督、全部 business、identity、stdout/stderr 后才进入下一角色。工具“执行完成”没有数字 exit；以下 Exit0 均来自 supervision.json，不由完成文字推导。

## 来源链与日志审阅边界

执行前限定输入快照为 R/source-before.md，已有 x86 快照作为范围对照；包含三处生产修正、四单元、两个 COM partial 合同与项目/包规则关键范围。它不是全仓库加密冻结，本次接续没有重读全仓库或认证所有输入的字节哈希。源修正为管理 Register/Destroy completion 与 partition 业务 Failure 分流、注册所有权及失败回滚，业务首 Failure 不清。

真实参数和即时返回在 R/01-com-build.md、02-native-publish.md、03-com-publish.md；日志持久压缩摘录分别在相应 *-log.md。三个记录全文已在接续阶段亲读；原阶段的分段日志阅读只按记录引用，不冒本次亲读原 stdout 全文。

- normal build：参数 Release/win-x64/net10.0-windows；64行工具日志记录 Exit0、133 warnings/0 errors。原 stdout104998字符，工具仅末20000可见、前84998缺失；持久文件是诊断位置/代码摘录，不是逐字完整日志。
- Native：精确命令见02；输出 R/native/；60行工具日志 Exit0，stdout615466字符，前595466缺失。可见 native code/link 输出，不声称全部警告或全部编译输入已审，无 warning/error 总数则不补造。
- 正式 COM：精确命令见03；显式 AotDllPath=R/native/wpfgfx_cor3.dll、AotRuntimeIdentifier=win-x64、ComAcceptanceSandbox=true、ComAcceptancePackageDirectory=R/clean/，输出 R/publish/；self-contained NETCore/Desktop10.0.13。stdout52792字符，前32792缺失，Exit0；不造 warning/error 汇总。
- R/04-clean-preflight.md 是原阶段静态读取记录：runtimeconfig全文、deps1–48、package-files1–90及600–622。接续重新亲读 clean/aot.sha256、runtimeconfig全文、deps1–48、manifest600–622，并实际枚举 clean/Native/wpfgfx_cor3.dll；不声称读取全部622行 manifest 或完整 deps。

正式 clean 的 aot.sha256 是 `54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD`，实际 candidate 角色加载再次认证该值。两 official 角色实际加载各任务根 official DLL，初始化候选文件不是 official actual 身份。限定链为：执行前可读范围快照→真实工具参数/Exit记录→Native输出→显式正式COM/clean复制链→真实进程实际模块身份/合同/清理。日志缺失如实保留，不补造完整 warnings，不因此否定已通过 actual ABI，也不以此要求重新发布。

## 共同身份

四角色测试程序集 SHA：`4FDFB7CD9D5D6B94678D5E3C753E2450EA4239150E0C2C71C72E9A585C43A15D`；MVID `aa7e49cc-633d-4b0e-aed8-560763268099`；运行时 `.NET 10.0.13`，X64，pointerSize8。runtimeconfig includedFrameworks NETCore/Desktop 均10.0.13，deps runtimeTarget win-x64、MSTest4.0.1。

两 official actual SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，version `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。actual/requested 均 `C:\CodingAgentSandbox\Tasks\<task>\wpfgfx_cor3.dll`，requestedEqualsActual=true，moduleHandle=actualBaseAddress=140720669196288。

两 candidate actual SHA 为上述54BC62完整值，version1.0.0.0；actual/requested 均 `C:\CodingAgentSandbox\Tasks\<task>\Native\wpfgfx_cor3.dll`，requestedEqualsActual=true，moduleHandle=actualBaseAddress=140720477831168。全部 identity 文件的候选初始化 SHA 相同，但角色认证以 business.identity.actual 为准。

## 四角色原件索引与标识（不能混用业务 UID 与 TRX ID）

以下 readback 路径均从 R 起；每个 Results 子目录都有 supervision.json、stdout.txt、stderr.txt、TestResults/child.trx；每个 readback/<角色>/Results/aot-identity.json 为初始化记录。TRX 的 TestResults 目录在枚举中可能漏列，四份均按精确路径直接读取成功，不能宣称原件不存在。

|角色 / Results 后缀|task / PID|business UID|TRX testId / executionId|
|---|---|---|---|
|postfailure-official / source-rebuild-x64-postfailure-official|ace38922f9164fd2909c12e8b75f4f63 / 2748|7f9b2d405d4746d599305a0733d545ad|1492e97c-4a0e-8e0a-b34c-d1512cc86f05 / 0f7c4b1b-cff9-4e45-bc08-e4a8e28c4c10|
|postfailure-candidate / source-rebuild-x64-postfailure-candidate|f888136b0f3b47908de98b6160cf8be2 / 12104|48821fe4206348efa3c4c115a38c8dd6|1c192168-0326-8ff6-be42-a78cea1f8ece / ead0b3ad-125f-459b-9ae0-3b71deb7e8b3|
|isolation-official / source-rebuild-x64-isolation-official|3aa3bab5e7714577a8d9a607519eeaa0 / 984|fcb37e774d3d4547866d0078ca4f7491|1e3a533f-c51b-8855-b58a-2f84cc5b1ade / 05ccaf18-875c-459e-968f-469148b0aea9|
|isolation-candidate / source-rebuild-x64-isolation-candidate|ea9e604fe881458c8e4e80a97614b135 / 11408|f889213a141f4ab9858972044b089527|18b58946-d629-8139-8233-224beea22724 / 053f20a1-d804-4e5e-aada-5a74c7e03d1f|

精确 business 路径规则：`readback/<角色>/Results/<Results后缀>/PostFailureAttach/<UID>/business.json`（前两项），或 `PartitionFailureIsolation/<UID>/business.json`（后两项）。工具即时返回与参数分别 R/05-postfailure-official-tool-return.md、06-postfailure-candidate-tool-return.md、07-isolation-official-tool-return.md、08-isolation-candidate-tool-return.md。05为原阶段即时记录；06–08为本次接续实际调用后立即落盘。

四份TRX全文各26行，total/executed/passed=1/1/1，failed/error/timeout/aborted/notExecuted等0，outcome Passed；监督全文各40行 ExitCode0、Reason=exited、HasExited/OutputCollected=true，TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut/OutputDrainCancellationTimedOut=false、SupervisorError=null。stdout各12行已读，中文部分存在编码替换字符，保留原件不擅自修复；stderr各空。TRX与业务而非乱码摘要承担通过裁决。

## postfailure 合同与清理

两个 business 均267行，接续完整亲读1–140、141–267（包含原official剩余及完整身份/清理）。共同23次真实调用；Async0/manager0，Visual39、Bitmap95，alphaCommand32/16bytes/setup0.75，bitmapCommand12/16bytes/source0。

- order16 A flush = -2003303418 (`0x88980406`) 后，order17 Create.L=0，L真实非零且不同A，source=A；不是创建前就失败或在成功partition创建。
- order18 L flush、order20 source销毁后的L flush，均88980406；order19真实Destroy.A=0，order21真实Destroy.L=0，order22 Disconnect/order23 Deinitialize均0。
- official A=2639142145552、L=2637244436592、connection=2637240025296；candidate A=2、L=3、connection=1。
- created=destroyed=destroyAttempted，stillKnown=[]；completed/cleaned/disconnected/deinitialized=true，unknownRegistration=false，cleanupClosure=known objects closed，primary=null、cleanupErrors=[]。
- negativeRejected五项均true，仅结果副本断言负控，不是生产故障注入。

这与已认证 x86 同一有限合同语义一致：管理创建成功不清除业务 Failure，source关闭不终止共享L，真实资源正常回收；不把进程退出等同正常清理。

## isolation 合同与清理

两个 business 均402行，逐段完整亲读1–160、161–300、301–402，共42次调用。A(source0)、S(sourceA)、B(source0)；三次初始setup flush0。order28 A、29 S flush均88980406。

- 独立B在A/S关闭前以alpha0.5真实Send/CloseBatch/Commit/Flush（orders30–33）全0。
- 真实Destroy A/S（34–35）均0；之后B以alpha0.25真实新更新（36–39）全0，不是只复读旧flush。
- B真实Destroy40、Disconnect41、Deinitialize42均0。official created=destroyed=[2395709699376,2395709945984,2395709948448]、connection2394576562176；candidate [2,3,4]、connection1。
- completed/cleaned/disconnected/deinitialized=true，primary=null、cleanupErrors=[]，siblingNegativeRejected/independentNegativeRejected=true；副本负控不冒产品故障注入。本partial没有unknownRegistration字段，不补造其值；正常清理由真实三对象销毁及清理记录认证。

## 历史独立账与未证边界

x86当前来源限定闭合的15A545两合同通过及旧四绿继续保留，见[x86收尾](replacement-postfailure-source-rebuild-x86-closeout.md)，不因本次x64另hash冲销。旧20B3 postfailure Create88980406/output0、unknownRegistration=true/cleaned=false是真实红历史，不能改写；旧40D0 x64隔离绿只绑定其当时产物，不冒含本修正。KN17 resize/HW/media/full及direct/public/WGC/KS/Ledger等历史账不转绑54BC62；显示、Native-only和此前所有冻结保持。

真实Register失败回滚/极端异常所有权、composition线程重入Destroy仍无本轮ABI实证；结果副本负控不能证明这些路径。本门不认证device-loss、显示新帧、resize、HW、媒体、全量或任意.NET版本。完整来源审阅有限于上述快照/参数/Exit/正式clean及实际加载链，不声称全repo或完整stdout审阅。整体包1–8不因有限门自动关闭。

## 下一唯一门建议（仅建议，不追加执行）

另授权一次**非显示、零执行的剩余资源/时序覆盖选门审查**，只针对“真实Register失败回滚及异常所有权”能否通过现有纯C#公开ABI合法确定触发：给出官方/候选入口、可观察清理与停止条件；若不存在合法确定触发点，明确未证边界而不造注入器或成功结论。不得自动build/publish/沙箱/full/显示，不追跑本轮四绿；具体后续执行必须由外层另授权。本轮已用完4沙箱并终止执行。
