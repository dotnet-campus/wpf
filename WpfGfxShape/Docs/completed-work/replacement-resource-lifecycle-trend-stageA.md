# Resource lifecycle trend Stage A — correction BuildPassed / SourceFrozen / Execution NotRun

## 追加终态：获授权最小修正与独立 normalbuild1

本次亲自实施，不再委派。计划：恢复与范围核对Completed → 最小修正Completed → 范围内静态复核Passed → 唯一追加normalCOMbuild1 Passed → 源码冻结Completed → 文档交接Completed。历史初build1失败 exit1 /145warnings /1error CA2208原样保留，不冲销；下文旧冻结/待授权属于历史状态。

实际追加编译：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`，`ResourceLifecycleTrendDiagnostic / win-x86 / net10.0-windows`；exit0，144 warnings /0 errors，2.80秒。只调用一次正常构建，无失败后修改重试，无分析器抑制；成功后同样停止并冻结源码。publish0、Native0、沙箱0、测试0、official0、candidate0、终端/脚本0、再委派0。Implementation范围内静态/编译通过，Execution NotRun，不是业务Green或资源回收实证。

### 具体修正与静态闭合

- 诊断partial的CA2208改为`ArgumentException(message, nameof(role))`；message来自现有AcceptanceResources.resx新增ResourceTrendRole，沿用英文资源及CurrentUICulture读取风格。ComAcceptance兄弟文件搜索`new ArgumentException(nameof(`，唯一命中为该处，未发现同形式兄弟误用。
- 专用NativeRun角色：所有Start之后的stdout/stderr采集启动和等待均在自有Process的try/finally；即使第二路采集启动抛错，仍尝试终止自己创建的子进程树。原异常通过ExceptionDispatchInfo保留，逐项收集cleanupErrors，finally不throw。
- Kill失败不再无期限WaitForExit：独立退出等待5秒（取消token与有限WaitAsync），stdout、stderr各自有限drain5秒，随后取消采集。KillFailed、Unreaped、forcedTermination、nullable exitCode分别记录；Kill失败/未回收/采集异常均让父失败，强制退出资源清理Unknown。退出后的有限等待/采集不是证明native资源已正常释放。旧RoleAsync180秒行为与公共通用入口零改。
- 轮/角色threadpool watchdog明确`watchdogHardRealtime=false`，TimedOut checkpoint保留资源清理Unknown；移除watchdog的finally内Kill，保存与终止分别尽力并记录错误。未执行轮、未完成轮仍分列，不会标成功。固定65样本、16轮4+12、同PID、无强制GC、official完整先绿才candidate、DescriptiveOnly、稳定标量DTO与WorkerExit NotProven均不变。
- AsyncSharedOwnership新增参数/轻量状态/真实callback/逐项清理及异常保全已亲读检查，本次未改该文件。alpha addressTaken对象未进入闭包。生产、旧单轮正常合同、Host、项目配置、SDK/TFM/包规则均零改。

### 边界与剩余未知

20秒只是尽力watchdog，不是硬实时承诺。同步native/WIC无法token中断，threadpool饥饿可延迟Timer、取消与continuation；角色360/父840/外900为分层监督边界，父840本身也是协作取消，不伪装内核硬时钟。有限回收预算最多退出5+两路各5秒，Kill/文件I/O/Dispose等系统调用仍可能受OS阻塞，只有未来独立外900秒沙箱才提供更外层进程边界，本次未运行验证。外层终止同样不算正常资源回收。未退出/未完成的轮、worker与native/GPU清理继续Unknown/NotProven，不为消除未知重构工程。

### 独立证据及保存限制

ignored `artifacts/resource-lifecycle-trend-stageA-normalbuild1/`：audit.md、build-request.json、build-return.txt、build-log-evidence.md、build-header.txt。构建真实参数事前持久化，返回即时保存，随后日志45–64与1–12即时保存summary/header；真实exit0/144warnings/0errors不是推测。服务stdout113441字符，只保留末20000，前93441不可恢复；不冒完整raw。早期上下文与全部读写工具未逐项逐字即时保存，部分4000字符截断，第一次1–1000读取超400行限制被拒；该逐工具保存偏差明确保留，不补造。非构建读写没有exit/warnings/errors，记N/A。完整读本stageA、规则、诊断partial与AsyncSharedOwnership（截断处补读）、专用NativeRun角色及必要旧路径；非全仓审计。

### 唯一下一建议（未执行）

另授COMpublish1（正常隐含编译另计）+外900秒沙箱1，父840/角色360/轮20，固定正式协调体1、official1完整成功后candidate1，Native0/额外测试发现重试0；先核包及实际模块/test SHA/MVID身份。当前成功编译不自动授权发布/运行，x64另授。完整替换仍Open。

## 历史初次交付：ImplementedDraft / BuildFailed / SourceFrozen

## 实际结论与预算

本代理亲自修改测试侧源码并调用唯一正常COM build。`ResourceLifecycleTrendDiagnostic / win-x86 / net10.0-windows`，exit1，145 warnings / 1 error，耗时2.92秒。首错且唯一汇总错误：`UceChannelTests.ResourceLifecycleTrend.cs(20,60)` CA2208，`ArgumentException(nameof(role))`把参数名作为message。按明确失败即停规则，不修后重试、不抑分析器；build后源码冻结。Implementation为草稿而非已验证完成，Execution NotRun，阶段A本次收尾完成但目标编译成功未达成。

normalCOMbuild1；publish0、Native0、沙箱0、测试0、正式协调体0、official0、candidate0、终端/脚本0、再委派0。没有实际角色PID/采样/业务Green。完整替换Open。所有历史报告与原失败保留。

## 实际修改范围

- `Tests/WpfGfxShape.ComAcceptance/UceChannelTests.AsyncSharedOwnership.cs`：私有helper有限参数化，旧无参仍一轮；角色提供module/轮根/轻量state/中途采样钩子。每轮新stack handles、resources、WIC与真实callback stream。callback方法只持轻量state，落盘后断开完整events队列。三个caller Release逐项记录pointer(long)/返回refs/index/name，关闭后caller指针、WIC Dispose及实际created/destroyed/released形成已知对象账。诊断新增硬门不改变默认旧硬门。主异常保留，原独立逐项清理保持。
- 新`UceChannelTests.ResourceLifecycleTrend.cs`：常量保护唯一无参协调体，显式环境opt-in；official先完整16轮并exit0、报告完成16和65样本才candidate。每角色一个PID、一份Load模块（不卸载），固定0–3预热、4–15测量。逐轮业务原件、实时events/callback JSONL、partial checkpoint；首错停止，完成轮数/未执行轮/未完成当前轮分列。
- `NativeRun.cs`：常量保护专用role派发及专用360秒等待，不复用/修改旧RoleAsync180秒。直接Process.Start同exe，UseShellExecute=false，stdout/stderr流写盘，linked父取消，终止只限自己创建的Process及其子树，等待/输出/监督逐项记录错误。
- csproj：仅非默认ResourceLifecycleTrendDiagnostic定义RESOURCE_LIFECYCLE_TREND、Optimize与x86既有候选路径；默认Release不启循环、不增加默认测试体。报告原属性方案被本次用户明确配置授权覆盖，未改SDK/TFM/包规则。
- 现有AcceptanceResources.resx：一条opt-in错误资源；文档和ignored证据目录。

生产、共享API、Tests、Host/库、SDK、TFM、依赖包版本、旧包规则均0改。未验证默认Release重编译（预算禁止额外build）。

## 静态架构合同

每角色固定65 = 起始1 + 16×(BeforeInitialize、A关闭/B新alpha更新flush后、完整cleanup后、固定1秒后)。不强制GC。记录phase/stage/round/PID/UTC/QPC/frequency、PrivateBytes/WorkingSet/handles/threads/managedBytes/naturalGC、累计与当轮callback/退出/disposal、真实callback线程ID、已知所有权账。指标读取失败为null+error，不写0冒未知。所有样本保留，轮3延后点基线；按阶段固定12轮OLS斜率/首末差/delta及4–7、8–11、12–15窗口min/max/median。软结论DescriptiveOnly，绝不内存Green；不按增长改预热/时间/阈值或删轮。

每轮原HRESULT、A/B非0不同、两个duplicate非0、caller refs、A前后dispose0、B更新、两个deleted1、真实destroy/disconnect/deinitialize、WIC Dispose与callback恰1退出须过。没有65536占位账；已知owner数只由实际caller指针与真实创建/销毁/duplicate释放事实得到，不代表所有native/GPU对象。

本次用户明确禁止临时给candidate加官方不具备的worker保证。现单轮只证明callback退出，未证明OS worker线程退出；报告与DTO始终WorkerExit NotProven。不会把本门成功称为包含candidate worker退出，也不会声称全worker/全GPU/长期无泄漏。结果validator负控仅复制结果，不是产品故障注入。DTO不写裸nint；alpha packet地址不进入闭包。

未来时间预算静态数值：role360×2=720 < 父840 < 外900；round20包含callback最多5与固定延后1。本阶段没有运行。同步public WIC/native不可Token取消；轮/角色watchdog直接终止角色进程，不能托管超时后继续工作并冒cleanup。父linked取消只作用本门子进程，旧180秒不截断本序列。

## 剩余风险 / 后续必须先审

编译失败本身阻塞发布与运行。还需独立静态复核：Timer/threadpool watchdog在进程极端阻塞/饥饿条件并非实时硬期限保证；父进程终止为外层界限。父输出收集在创建子进程之后、等待try之前存在极小异常窗口；Kill失败后的无期限WaitForExit仍可能超预算，启动/回收边界尚不能声称全路径无孤儿/取消端到端已证。不能因数值预算满足而宣称所有异常路径已安全。

checkpoint实时落盘与callback输出写入本身会分配和I/O；callback事件Exit在finally原子标量前，只有CallbackExited恰1断言是完成证据。延后样本与下一轮前验证已完成轮轻量状态，但最后验证后到进程退出仍有观测边界，不声称无限静默。模块requested/actual/hash与testSHA/MVID在每轮business内，角色初始加载失败则身份未知。采样Process.Refresh失败导致角色失败而非虚构metric0。部分失败轮可能无最终business，实时events/checkpoint为partial而非完成证据。每轮business记录已有完整事件，运行时identity/hash写入与采样自身开销需承认。需要后续授权纠正与审核，而非直接发布该冻结草稿。

## 证据与偏差

ignored `artifacts/resource-lifecycle-trend-stageA/`：snapshot.md（实施前）、build-request.json、build-return.txt、build-log-evidence.md。真实构建工具参数已存并回读；日志reader实际区间1–20、47–63，确认exit/warnings/errors。服务stdout115157字符仅保留末20000，前95157不可恢复；不冒完整raw。全部工具逐字即时日志要求未完整达到：早期读写/替换调用未逐一逐字持久化，部分返回发生4000字符截断；明确保留此偏差，不补造。读取请求1–1000超400行限制一次、文本替换唯一性/未读取前提失败若干，均非额外执行/编译。读过完整96行合同、恢复入口顶部/27行规则、完整单轮与NativeRun必要实现、csproj与SDK；非全仓审计。

## 下一阶段申请（未执行）

本次build预算已耗尽，不自动修复。先请求另授权最小CA2208修正与上述取消/回收窗口静态复核，并重新授normalCOMbuild≤1；不能把失败预算冲销。重新编译通过且源码冻结后，才申请COMpublish≤1（正常隐含编译另记）+外层900秒沙箱≤1，父840/角色360/轮20，正式体≤1、official≤1→candidate≤1、Native0、额外测试/发现/重试0。x64另授。publish1+900秒沙箱1均需外层明确授权，本次不执行。
