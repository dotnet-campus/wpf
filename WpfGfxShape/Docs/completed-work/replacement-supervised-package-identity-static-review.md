# 正式包 Native 身份修改：监督分派下的独立静态复核

## 本轮结论与授权边界

IndependentStaticReview / StaticInspectionOnly / NotCompiled / NotTested / OverallOpen。

采用用户指定的监督者分派模式：监督者只读文档、负责分派，不亲自探索代码；本承接者承担独立源码复核和交接，不以实施者自检替代审查。本轮先读最新恢复入口、Pcore handoff 与适用执行/连续性规则，再直接核对两文件及必要的被调用实现、dispatcher 和共享 RoleAsync。用户本轮要求监督者持续分派；本轮子任务由监督者限定静态，因交接要求核实新运行授权而尚未启动执行。这不是用户明确禁止全部执行。

终端调用=0；脚本执行=0；构建=0；测试=0；发布=0；程序运行=0；COM 执行=0；Native 执行=0；沙盒执行=0。未加载 candidate 或原版，未制作原版副本，未引入或使用 fallback。没有编译器诊断、测试发现或运行通过证据。

## 本轮实作与独立审查结果

审查对象是本轮已有两文件修改，不声称由本承接者实施：

### Tests/WpfGfxShape.ComAcceptance/ComAcceptance.Package.targets

- 第 47、80 行两处 GetFileHash 的 Files 均为 `$(ComAcceptancePackageDirectory)Native\wpfgfx_cor3.dll`，Algorithm 均为 SHA256；各自输出随后写入最终包根的 aot.sha256。
- 普通包分支先将 AotDllPath 复制到最终包 Native 目录，再计算该目的文件摘要；self-contained 分支先通过带 PackagePath 的复制列表将 PublishDir/Native/wpfgfx_cor3.dll 放入最终包，再计算相同目的路径摘要。相比仅散列来源文件，这使清单计算对象明确落在最终包 Native 文件上。
- self-contained 的 Stage17CandidateRoot 条件复制仍是既有 candidate 根目录复制，不是本轮新引入的原版副本；两处 hash 并未改成散列该根目录文件。本轮不修改或运行该历史分支。
- 未见两处修改中的明确静态错误，因此不修改 targets。上述只确认源码顺序与路径，不证明实际打包结果、来源可信性、清单冻结、实际加载路径或完整包身份；目标求值及复制执行均未验证。未核算新 Native SHA，数值为 Unknown。

### Tests/WpfGfxShape.ComAcceptance/AcceptancePathsTests.cs

新增四例均已直接读取并与 AcceptancePaths.ValidateIdentity / ValidateCandidateModules 对照：

1. WhenFailureInspectionFindsNoCandidateModulesThenItAcceptsThem：require=false、空数组；实现的数量与 Any 条件均不触发，静态对应允许零模块。
2. WhenFailureInspectionFindsOnlyDifferentPathModuleThenItRejectsIt：require=false、一个 other 路径；Any 路径不等条件触发，静态对应拒绝异路径，不把 require=false 当作放宽身份。
3. WhenCandidateHasClrHeaderThenIdentityIsRejected：匹配当前 machine；将节数设为 1，在 PE32/PE32+ 的 directory 起点 96/112 加第 14 项写 CLR RVA 0x2000、大小 72；节表位于 optional header 后，VirtualSize/VirtualAddress/RawSize/RawPointer 为 512/0x2000/512/512。1024 字节容纳该节，原始偏移 512 写 COR header，metadata 指向 RVA 0x2048、大小 1；摘要在修改后计算，流复位。静态意图是让 PEReader 解析至 CorHeader，再由 native-only 门抛 InvalidDataException，而不是被错误摘要拒绝。
4. WhenCandidateHasNoOptionalHeaderThenIdentityIsRejected：从 0x84 提取 20 字节 COFF header，将 SizeOfOptionalHeader 的 16/17 字节清零；摘要计算与复位自洽。意图是 COFF-only 输入得到 PEHeader=null，再由身份门抛 InvalidDataException，而非截断 PE 的解析异常。

两个模块判定例可由当前布尔条件直接对应。两个 PE fixture 的偏移、范围及构造意图静态自洽，未发现可据以修正的明确错误；但目标 System.Reflection.PortableExecutable.PEReader 对合成 CLR/metadata 和 COFF-only 输入的接受程度、是否先抛 BadImageFormatException、能否到达断言指定的 InvalidDataException，正确性结论仍为 **Unknown**。未编译、未测试，不能宣称四例通过，也不能把静态自洽提升为 fixture 已认证。未凭猜测改 fixture。

这些是判定函数测试，不是 candidate 两个真实入口的端到端负控。既有 AssemblyInitialize 仍依赖外部合法 matching-architecture candidate；本轮未绕过此前置或创建新产物。

## dispatcher 生命周期：两次独立确认，未修改

既有 [Pcore handoff](../handoffs/pcore-candidate-identity-minimal-fix-awaiting-authorization.md) 已记录独立审查确认的异常展开提前释放风险；本轮重新读取 DispatcherScenes.cs，构成第二次独立确认，不是一次风险陈述被当作两次运行证据。

DispatcherAcceptance 的 candidateLease 位于 try 内 using 作用域。RunDispatchers 的后台线程在 ready.Wait(20 秒) 或逐线程 Join(60 秒) 超时后，不保证全部已退出；异常展开即释放只读句柄，然后进入托管失败取证。故不能断言失败路径租约覆盖所有仍可能发生的 native 活动。正常路径的 join/清理与异常路径必须分账；两次均为静态确认，无实测。DispatcherScenes.cs 本轮未修改。

可选的进程寿命 lease 能延长保护，但涉及资源释放契约：何时移交所有权、正常/认证失败/线程失败时何时释放，是否允许持有至进程退出；还涉及整体清理期限，不能把无界等待当作有界收尾。共享 NativeRun.RoleAsync 当前是三分钟等待，取消后 Kill(true) 再无超时 WaitForExitAsync；它被多种角色共用。若调整回收期限、进程树回收与失败语义，影响不限 dispatcher，需明确共享 RoleAsync 的回收范围及授权。保持句柄不等于线程/COM/native 资源收敛，杀进程也不证明业务清理成功。本轮未擅自选择进程寿命方案、调整释放契约或修改共享回收代码。

## 历史 resize 证据边界

引用用户要求保留边界的 [resize 像素定向复验报告](replacement-supervised-resize-pixel-revalidation.md)：历史 task 6dc44ac8fd3947b989353d9bda46b4a0 的 consumer 前置期望 00563412、实际 001E1D1C，TRX 1 执行/0 通过/1 失败，业务 Exit 2。报告记载未进入 connect/channel/resource/bitmap/shader/target 及 resize；resize 前后内容、target identity/refs 未覆盖。

本轮仅阅读该报告，并未重新审阅其原始执行产物；不能称 resize 通过，不能称这是已进入 resize 的修复后反例，更不能用其历史 SHA 或结果认证当前包。

## 实际修改文件与交接

本承接者没有修改两个审查对象或其他源码，没有修改 Pcore handoff、执行规则、基线、工作项或其他历史文档。实际写入仅：

- `Docs/completed-work/replacement-supervised-package-identity-static-review.md`（本报告，新建并回读）。
- `Docs/next-session-handoff.md`（只在顶部加入本轮简短摘要/链接和历史边界，下方原文保留，并回读）。

精确交接根：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`。恢复入口为该根下 `Docs/next-session-handoff.md`；本轮完整交接为该根下 `Docs/completed-work/replacement-supervised-package-identity-static-review.md`。

## 下一动作与未闭合项

必须先取得明确的新执行授权及各类次数，并明确 dispatcher 生命周期取舍、资源释放契约、整体清理期限和共享 RoleAsync 回收范围，再决定实施/验证；本静态复核不是自动运行许可。后续需将新 Native 来源、架构、正式包 Native 实体、重新核算的 SHA、清单和当前源码产物明确绑定，再按授权验证真实 candidate 入口负控及公开 WPF 路径。禁止原版加载、原版副本和 fallback 不变。

本轮未核算新 Native SHA，未运行真实入口负控；不能绑定 stageAsync、旧 Native 发布、旧测试或 resize 的任何历史通过结果。两个 PE fixture 解析正确性 Unknown；源码未编译未测试；dispatcher 异常生命周期风险仍开放；整体状态 **OverallOpen**。
