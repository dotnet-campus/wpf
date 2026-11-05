# 真实 HW shader resize 主线恢复决策

> 后续用户纠正（本次仅文档、源码及全部执行新增0）：本报告以下B/ExternalBlocked与“当前工具无桌面参数故必须外部维护者”是过早历史裁决，保留原文但撤销其当前排程效力。可执行沙箱C#即可直接调用实际令牌允许的Windows管理API，权限/管理能力尚未被API结果充分核实。当前为**显示内容失败根因未定，沙箱内能力尚未充分核实**，不证明沙箱故障或环境/外部阻塞。[后续唯一原用例内单次调查设计](replacement-sandbox-native-observation-contract-design.md)给出两个可证伪假设、具体API顺序、清理、预算和主线恢复；未经授权仍不执行。所有历史失败、身份和预算原样保留。

## 确定裁决：B / ExternalBlocked

本阶段仅调查与文档。源码/项目修改、build、test、publish、Native 发布、沙箱运行、命令行/脚本、再委派全部 **0**。公开 API 元数据读取不是执行测试；清单读取不是重新散列。双架构当前 KN 两生命周期有限绿保持，完整替换 Open、显示冻结。所有历史任务、预算、失败及身份独立保留，不合并、不重置。

**现有正式设施存在，但没有可由当前工具落实的必要会话控制，也没有相对于既有失败可证明的运行条件变化。故不授权一次试环境、不重开显示探针，主线明确 ExternalBlocked。** 这不是证明沙箱永久不支持显示，也不是把像素失败唯一归因环境。主代理不需要再探索代码或重新选门。

## 1. 能力与运行入口核验

- 当前正式 Windows 沙箱执行能力仅接受 sourceDirectory、executableRelativePath、arguments、outputRelativePath、localOutputDirectory、timeoutSeconds。可以推送工作区包、以包内应用执行并完整回收；没有 sessionId/desktop/RDP/显示器/adapter 选择、登录/解锁、连接保持、窗口保留、驱动或显示电源操作参数，也没有对应独立工具。不能用 arguments 冒充会话配置，不能通过另写 runner/脚本或调用系统程序绕出这项边界。
- `com-acceptance-sandbox-operations.md` 明确包根运行正式 `WpfGfxShape.ComAcceptance.exe`，不是 publish 父目录或历史 caller。现存 `NativeRun` 提供 MTP 及 `--supervise-exit`，`ExitDiagnostics` 提供独立子进程、filtered120秒、原始 TRX/诊断/阶段/监督回收；**不提供跨任务保持窗口或配置桌面**。延长外层超时不是显示修复。
- 实读 `artifacts/kn-current-native-x86-r1/discovery-tool-return.json`：正式 exe 在 `C:\CodingAgentSandbox\Tasks\779cf22aa87f4ec4a4a16e6e2b0db44d` 启动并返回 MTP 文本/回收路径，只证明已有执行与回收能力，没有远程连接或会话恢复能力。
- 仓库根 README、`Documentation/getting-started.md` 仅一般 WPF SDK/创建新应用/外部 samples 指引；Documentation 内未找到 RDP/桌面会话/窗口保持配置说明。它们不是本任务现成显示执行入口。WpfGfxShape 范围实际仅生产库与两个正式测试 csproj，无 App.xaml；ComAcceptance 是 UseWPF/NativeRun 测试应用，并非已记录可恢复显示的独立交互应用。public HwndSource 入口确实存在，但已失败且被冻结。没有发现可直接替代的、已记录 actual 身份和官方可见内容的真实 WPF 应用入口。此结论限于已核项目范围，不声称仓库所有代码均已穷举。
- HN `replacement-supervised-display-source-state.md` 已记录 Session1/console/Active/UNLOCK/Default、屏保0、visible/uncloaked、CCD active 路径与 Hyper-V DISPLAY1/LUID 关联，却仍失败；当前显示电源、实际 scanout/指定 target 消费未证。不能再把“切 RDP/解锁/启动窗口”当已定位修复，更不能把旧 Active 或 KN 创建窗口成功说成新条件变化。

## 2. 冻结依据与角色顺序

直接依据均亲读相关裁决段：

1. `replacement-supervised-public-hwndtarget-control.md`：官方 public 矩形 expected00563412/actual001E1D1C，真实体失败；actual 官方模块身份不足。终态冻结当前 window-DC 验收，禁止追加 direct/native/WGC/等待/消息探针，要求真实应用/内容入口前提。
2. `replacement-supervised-direct-consumer-clear-control.md`：actual official5EBD，同 target 空 root 绿清屏 HR0，DC001E1D1C而非0000FF00；红内容未到达，target 输出/观测共同边界未闭，backend未知，不唯一归环境或 interop。
3. `replacement-supervised-consumer-wgc-retirement-and-coverage.md`：WGC 有据退场，透明原因未决不阻主线；旧“先恢复 fresh-native”不是当前排程。其后 direct/public 实际失败约束仍有效。
4. 最新双架构 KN 收尾与主要缺口审查 §6：下一主线是既存 shader resize 内容，不再造微 ABI/native-only/WGC/等待/线程探针；无环境 ExternalBlocked。

**因此现在 neither official nor candidate：执行0。** 外部条件确实改变且另获一次授权后，优先恢复原 candidate x86 resize 单例，不先跑 public 官方矩形、direct clear 或原 native-only 前门。理由是当前唯一主线/原 KP 恢复身份为 candidate；official 控制已实际失败，现无理由无条件重跑另一官方控制。原候选方法内部 native00563412断言必须保留，在唯一原用例内验证，不把它抽成先验独立门。official resize 是后续独立角色、独立授权，不是本次第二体；其旧首 shader 帧失败仍保留。

## 3. 原用例与当前两 clean 静态可用性

本次读取两 clean 的 `D3DImageTests` 公开 API，均存在下面两个无参数 public 方法；读取两 aot.sha256 与当前收尾报告吻合。未执行发现、未核二进制 IL 与源码逐字节等同，也未重新计算包散列。

|角色|准确 FQN|历史 UID/testId，仅索引|
|---|---|---|
|candidate|`WpfGfxShape.ComAcceptance.D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive`|`1eded079-5019-809c-bf7c-07ef4caf0e29`|
|official|`WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive`|`14e862b3-9b71-843a-9ff5-67f779272c33`|

当前包：`artifacts/async-shared-ownership-x86-r1/clean` 和 `artifacts/async-shared-ownership-x64-r1/clean`。两者入口均为正式 `WpfGfxShape.ComAcceptance.exe`。API 元数据只能确认方法可静态引用，**不能认证当前 MTP TestNodeUid 未变**。旧1eded…仍为历史 KP 身份，不能无发现地直接当新程序集 filter-uid；无参数精确 FQN 可合法限定一体，执行后的真实 UID 必须由实际 TRX/diagnostic核定，不必为此另跑发现。

身份预期（来自当前两收尾报告与本次清单，不冒本轮加载）：

- x86 Native `15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`；test `D8A5F57FE4A61CC28459A89DCB50DC75E0ADE80E1D0AEDC35D7E18AC5069A7BB`；MVID `e207b681-4c15-453e-b0a4-a1690fa3db78`；.NET10.0.13/X86/pointer4。
- x64 Native `54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD`；test `4A8356201F062B01161744C6B3BBF7D9AB7DB5909F6F12A9389806FFB6B6ED43`；MVID `d41adfe8-8904-4c9b-887d-8d1f5979ba46`；.NET10.0.13/X64/pointer8。
- official 独立角色历史实际模块 x86 5EBD18E0…、x64 AD1C9DBB…，不由 candidate 执行认证。

## 4. 原步骤、身份与清理的真实观察范围

亲读 `D3DImageTests.HardwareClosure.cs:15–42`、`D3DImageTests.VisualCache.cs` 原参数、前置、target/frame/resize/mutation/finally及现存 NativeRun/ExitDiagnostics；不改代码。

- 两角色都传 cache=true/colorFF00FF00/expected0000FF80/geometry=false/resizeWindow=true，仅 official角色不同。
- 实际加载请求模块后 GetModuleFileNameW取得 actual路径，断言请求/实际路径相同，散列实际模块文件，输出版本、runtime、architecture及 test SHA，写 `Results/hw-module-*-<guid>.json`；resize前再次记录。不是内存hash。该业务 JSON不记录 test MVID、actual PresentationCore 或独立业务 UID；MVID取监督记录，UID取TRX/diag，不能发明字段或移绑 KN 的 WPF 身份。
- 原窗口(40,40)/32×16；普通 Direct3D9 adapter0/HAL1/behavior0x26，count1/DISCARD/windowed；PresentNativeControl(false)与 DiagnoseNativePresent 后 consumer DC **00563412** 前置。失败即原断言/清理，不跳过。
- 随后才 connection/channel、两 producer surfaces/interop images、visual/cache/shader/effect和resource46 HWND target。candidate connect1同步/same-thread；official initialize0/connect0异步，initial flush/逐 image 独立 JhPresentImage；不能称完全同一调度夹具。
- 初始两消费帧均要求0000FF80；随后 width64、48、32且height24，owner SetWindowPos + GetClientRect独立断言窗口client尺寸，SendOutputWindowSettings/close/commit/present/DwmFlush后逐次原FF80取色。再secondary mutation绿→蓝、dirty/提交/释放caller bitmap，末两帧要求00FF0080。不存在把 FF00 作为成功 expected 的授权。
- GetClientRect只证明 HWND client尺寸，**没有 target surface GetDesc、target GetDevice/IUnknown同一性、target manager/device/backend 的独立观测**。producer HAL、HardwareOnly字节码、HR0及原像素成功都不能补这些格；即使原像素通过也不关闭完整真实HW合同，不顺便加测试。
- finally记录 destroy/disconnect及适用 deinitialize HRESULT，释放bitmap/surface/device/factory，销毁窗口，记录cleanup complete；仅businessCompleted才断言cleanupFailures为空。Release辅助调用没有本合同独立四资源最终ref返回值记录；DestroyWindow调用没有此处独立返回断言。前置失败时未创建对象的零占位不是四真实refs0，原异常之后必须亲读清理阶段，不能借 KN 关闭回调/refs认证此体。

## 5. 最少外部动作及唯一原用例验证

责任人是**Windows沙箱会话/宿主显示设施管理员或用户指定的既有可见桌面维护者**，不是继续代码探索的主代理。

最少动作：在正式沙箱任务实际所属会话上，提供一个能持续接收该任务 HWND 内容的已连接显示端，并维持到一次用例正常结束；说明执行服务如何进入这个会话，以及相对 KP/public/direct失败现场具体改变了哪个连接/显示消费条件、何时改变。若现服务不可绑定到该桌面，须由设施方配置正式任务的交互会话映射或提供其已有受支持入口；当前工具不能选择另一主机/任意RDP会话，提供一个与任务无关的可见桌面地址不够。**不要求安装驱动、切换RDP、解锁或显示电源操作中的任意特定一种：尚未定位哪一种必要，不可臆造修复。** 若管理员无法提供该映射/变更，保持ExternalBlocked，不运行。

确认映射及变更并另授权后，使用既有clean，唯一原用例直接验证，不要求额外校准探针：

- 新增预算上限：沙箱1 / 测试体1；build/publish/Native/发现/official/x64/full全部0；首错停、无重试。不是复用或覆盖任一历史预算。
- sourceDirectory=`WpfGfxShape/artifacts/async-shared-ownership-x86-r1/clean`；executableRelativePath=`WpfGfxShape.ComAcceptance.exe`。
- arguments逐项为 `--supervise-exit`、`Results/real-hw-resize-resume-x86-r1`、`120`、`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive`。
- timeoutSeconds=180；outputRelativePath=null；localOutputDirectory=`WpfGfxShape/artifacts/real-hw-resize-resume-x86-r1/readback/candidate`。只上传clean，完整回收，准确调用及完整可见原返回即时归档，再回读原件。本报告没有创建该运行目录或执行此调用。
- 核实际 Native/test身份及runtime/arch、真实单体TRX/UID、原始像素与HardwareResize client记录、阶段/监督/cleanup。zero/multiple体、身份不符、00563412前置或FF80/后态内容失败、timeout/崩溃/cleanup异常或关键原件缺失均立即收口；不改expected、不拿producer读回顶consumer、不软件fallback、不追加official/第二任务。
- 原前置仍失败＝本次真实失败、shader/resize内容未覆盖；不自动恢复ExternalBlocked后再试。通过只新增本原像素/窗口resize有限内容证据，target GetDesc/device identity/backend仍未知；不得宣布完整HW/full/完整替换通过。

**唯一必要问题：谁能把正式沙箱任务映射到一个持续连接的可见桌面会话，并说明相对既有失败改变的具体显示连接条件；该维护者是否有权限完成这项映射？**

## 6. 显示阻塞期间仅一项有价值的非显示建议

另授权 **当前源码正式 `WpfGfxShape.Tests` Release正常依赖图构建1次**（目标 `WpfGfxShape/Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj`，configuration=Release），失败停止，不publish/test/Native/沙箱，不抑制分析器、不改SDK/TFM。依据：该项目 net10.0/MSTest4.0.1直接 ProjectReference 当前生产库；主要缺口工作表包1仍缺最后源码正式完整验收，历史IO绑定旧托管产物，当前KN只覆盖Native/COM有限合同。此项可检验当前托管源码及依赖图编译面，不依赖显示，属于正式构建验收而非微ABI门；**不声称新运行、当前双架构覆盖或最终full**。本轮不执行，也不把它替代显示主线。
