# BX desktop delivery differential：已有证据收口

## 1. 结论、范围与证据口径

BX 证据已收口，**交付根因仍未闭，验收仍 Red**。证据根为工程根下 `artifacts/bx-desktop-delivery/`（下称 B）。本次只用专用工具读写及一次专用 Release build，没有终端、脚本、新捕获测试、发布或生产修改。

同窗口 GDI 绿色能被 client/screen/front 独立读到；随后 D3D9 backbuffer 按 red/blue/red 改色、Present=0、EVENT query 与 DwmFlush 完成，client/screen/front 仍为绿色，duplication 没有 fresh content。这把问题定位在“已有 backbuffer/提交完成”到“桌面可观测输出”的边界，**不是证明 Present 没被调用，也不是证明 GPU 真实显示已完成**。GDI 可见不等于 duplication 成功，不以 GDI 冒充硬件交付。

保留 BJ 成功反证；不得据此宣称 Basic Render Driver 永远不能产帧、无证归因环境、物理 GPU 已验证、loss 恢复或 full 已完成。未改生产、异步 surface 合同、阈值、golden、skip、fallback、并行、入口、项目打包、其他代理文件或共享文档。

## 2. 五份父级执行结果与退出

下表是各次执行的顶层业务测试 TRX（监督子进程输出名为 child.trx），不是监督器自身另有一份通过 TRX。未把内部三帧计入父测试分母，也不把多次复跑合成一次验收。

|B 下任务目录|task|TRX 相对任务目录|total/executed/passed/failed|Counters 行|
|---|---|---|---|---|
|run-x64|4f5dc9702e5247d9a7095e9d7e558e3f|bx-native/TestResults/child.trx|2/2/0/2|213|
|run-x86|f980d3954f2240089b66f7bb3abf84bb|bx-native/TestResults/child.trx|2/2/0/2|202|
|control/run-x64|44774f5fcbbd490c89d1049b58d04d03|bx-control/TestResults/child.trx|2/2/0/2|245|
|control/run-x86|26472b6be6f4424ebacd6028faf11242|bx-control/TestResults/child.trx|4/4/0/4|318|
|control/final-x64|86719c1ecbe145c094d1363d3ec0f4cc|bx-control/TestResults/child.trx|4/4/0/4|329|

已亲读五份 ResultSummary/Counters/RunInfo：均 Failed；error、timeout、aborted、inconclusive、passedButRunAborted、notRunnable、notExecuted、disconnected、warning、completed、inProgress、pending 全为 0；RunInfo 均明确失败退出 2。内容等待截止导致断言失败，不是 TRX timeout。

五份 `bx-native/supervision.json` 或 `bx-control/supervision.json` 已全文亲读：Reason=exited，ExitCode=2/0x00000002，TimedOut=false、Cancelled=false、KillAttempted=false、KillReturned=false、HasExited=true、OutputCollected=true、OutputDrainTimedOut=false、SupervisorError=null。前两份监督上限 60 秒，后三份 90 秒。最终 x64 UTC 06:41:33.5038062 至 06:42:44.9253589，监督 PID9464、业务 PID2388；不是崩溃或监督杀进程。

前两格筛选 owner native gate（swapEffect 1/3）；control/run-x64 仅同窗口 GDI 两数据用例；control/run-x86 与 control/final-x64 另含两项非 owner alternating 对照，四项全失败。最终 stdout 284–291 行也明确 4 total/4 fail/0 pass/0 skipped，不能遗漏 final-x64 或用先前两项结果替代。

## 3. actual 测试身份

以下来自五份监督 JSON，并与各 `Results/bj-fresh-1.json` 的实际 testPath/SHA/MVID 对照；不是根据本地 publish 目录推定。共同 runtime 为 .NET10.0.13；X64 pointerSize8，X86 pointerSize4。实际程序集路径统一为 `C:\CodingAgentSandbox\Tasks\<task>\WpfGfxShape.ComAcceptance.dll`。

|任务|testSha256|testMvid|
|---|---|---|
|4f5dc970… X64|C6D47BAC4F209983C461A24BF74A0EA09E019B9E10FF8CD07BD973EA2BC9621A|10d9ab8c-d57a-4cfb-bc74-47dc2637175c|
|f980d395… X86|E51684CC4CD5B7E8A485C0A4D760799CFCCE117C35A5ACE0421BA1B75AE6ACBC|96a7169a-e695-42b1-9d29-9ba67701404a|
|44774f5f…、86719c1e… X64|6813EB788E0A25A75EA9383C40D8C06FDF34A5E8A9364A2F2BA66E2BA30BC2DC|78965af3-d0a3-4328-8e62-91b8074c7035|
|26472b6b… X86|57DCE17B205651A62F3C0EDFDD4DC0A6E94CC6DD07D2C393F0509E1042A9DA07|5d1ce80a-aac7-4b28-93f9-87251ae2d936|

亲读 final-x64 与 control/run-x86 的 aot-identity：所选候选分别为本任务 Native/wpfgfx_cor3.dll，SHA256 x64 `1C381539E7C82DDD75908C0563F0AC062E583166BDB23AEE066BDD21E838752F`、x86 `E0D3F5EFEDFACFD5CC3491DD43B194E19028E8B541A462123C8F73C73D6E25B5`，仍是 BD 旧快照。该身份记录不等于 native gate 实际加载并验收了候选 backend；本次 native gate 使用系统 D3D9，没有同任务 candidate PublicHwnd 成功链。

## 4. 业务像素、duplication 与窗口轨迹

亲读各任务 `Results/bj-fresh-1.json` 的身份与首帧；final-x64 的 `bj-fresh-1.json`、`bj-fresh-3.json` 三帧均已分段读完。初始无 GDI 两架构首帧 backbuffer=4294901760(FFFF0000)、Present=0、frontResult=0，client/screen=13348487、front=4287082187，不是目标红色，duplication 等待失败。

三个 GDI 任务首帧均记录 backbuffer=FFFF0000、Present=0、client/screen=65280(0000FF00 COLORREF)、frontResult=0、frontPixel=4278255360(FF00FF00)，accepted=false。最终 x64 两种 swapEffect 的六帧均为 red/blue/red backbuffer 对应正确，Present=0，client/screen/front 始终绿，duplicated=0、accepted=false，failure 为 `No desktop content frame belonging to the current submission arrived.`。这里 duplicated=0 是未取得结果的初值，不是成功捕获黑帧。GDI observation 自己也失败、pixel=0，不能写成 duplication GDI gate green。

最终 x64 swapEffect1 首红帧提交/完成 QPC 为 32995783156478/32995788450901，owner=submitThread=9380；swapEffect3 首红帧为 32996070726575/32996075954881，同 owner9380。stdout 记录 Microsoft Basic Render Driver、vendor1414/device008C、LUID 00000000:00004E71；DISPLAY1，1920×1080，attached=1、rotation=1，同 monitor0x10001。duplication create=0，初始及 acquire 返回0x887A0027，未得到 fresh content；不要把 API 创建成功写成内容成功。

已亲读五任务 swapEffect1 的 created/shown/before-present（GDI 任务另含 gdi-flushed），最终 x64 追加 completion/capture/destroy 轨迹与 swapEffect3 首次 Present 完成：session1、sessionState0；input/thread/owner desktop 均 Default；visible1、iconic0、cloaked0；rect=[40,40,72,56]、client=[0,0,32,16]、采样屏幕点[56,48]，cover 等于目标且 class=WpfGfxShape.HardwareOutput。原生任务 owner 分别4452/7160，对照任务9964/7532，最终9380；未把 MSTest 调用线程当作 owner。

最终 swapEffect1 HWND27591964，swapEffect3 HWND27657500。swapEffect1 六个关键 Present 前后/采样阶段窗口仍可见无遮挡；before-destroy 后的消息71/6/130与 after-destroy 才出现 visible0、owner0/无效窗口及桌面 cover，不能把正常销毁尾迹倒推成呈现时被遮挡。轨迹是离散时刻，不证明整个等待区间无任何瞬时变化；未声称其余所有历史 JSON 全文已核。

## 5. BJ 反证与已排除边界

亲读 BJ `artifacts/bj-capture-fresh-frame/stages-run-x64/TestResults/bj-stages-x64.trx:410–413`：Completed、16/16/16、failed及异常计数0。实际 task d664443b1dfd4315a14422dcea75aed8；测试 SHA `7FFDF4F38E97BB2E67ECBFE3219D2D8BA19E7A3B9AA6F2ACDCE653A3498220C7`，MVID `a185491a-3e71-4a5d-88a6-bb93c71db7b9`，不是本轮 BX 测试。

BJ fresh-1 三帧实际 red/blue/red、duplicated/front 与 expected 一致、client/screen COLORREF 为255/16711680/255、accepted=true；fresh-3 首红帧同样成功、passed=true。bj-public-stages.jsonl 三条依次 native-gates-passed-official-start、official-passed-candidate-start、candidate-passed。BJ 的数值进程退出码本次没有可回读监督文件，不能将 Completed 偷换为亲核 ExitCode0。

因此排除“颜色常量根本错误”“Basic Render Driver 绝不产帧”的绝对结论。BX 当前失败已在无候选生产参与的原生前置复现，不能用来证明异步 surface 合同有错；BS 两个完成任务5693314defb646fbab11ed92f55b56c1、3863b247efc74696b9b194e5cddd1c39 的报告同样止于 native gate，不能声称已到 sampler connect/Present。BS 数据此处是报告交叉引用，不冒称本次重新亲读 BS 原始 TRX。

本轮已排除或收窄：backbuffer 未改色、Present 未调用/返回失败、未等 EVENT/DwmFlush、首帧 owner 不匹配、采样坐标不在32×16 client、记录时窗口隐藏/最小化/cloaked/被其他窗口覆盖、duplication 选错 monitor、仅 GDI 读屏路径失效，以及监督超时/崩溃导致没有结果。不能据离散轨迹排除所有驱动/DWM/会话状态变化，也不能排除诊断时序扰动；没有把角色隔离或关闭并行当修复。

## 6. 默认行为审查、精确恢复与最终文件清单

本次修改仅下列三文件：

1. `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.CaptureFreshFrame.cs`：同窗口 GDI 方法原来无条件带 TestMethod/DataRow，会进入默认发现，多占用相同位置窗口并增加等待，属于行为性诊断。现仅在显式编译符号 `BX_DESKTOP_DELIVERY_DIAGNOSTICS` 定义时编译该测试；不定义时不是 skip，而是不引入诊断测试。需要对照时在独有诊断构建显式定义该符号，并仅筛选 `WhenSameWindowGdiControlPrecedesNativeFramesThenBothDeliveryBoundariesAreRecorded`；本次未改任何项目默认 DefineConstants。
2. 同文件：原有 owner gate 仍调用 CheckFreshDesktopDelivery(swapEffect,false)，不执行 FillRect/GdiFlush。仅 gdiControl=true 注册轨迹文件；默认路径不再执行窗口状态采集、JSON序列化和同步文件写入，已有 Trace 调用仅查空注册表返回。保留专属诊断阶段 created/shown/gdi-flushed/before-present/after-completion/capture-ended/before-destroy/after-destroy，均只读状态，不绘制。GDI 是显式对照唯一有意覆写绿色的分支，绝不作为生产验收替代。未改三帧顺序、颜色比较、截止、释放、断言或失败传播。
3. `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.OutputWindow.cs`：移除 BX 在 WndProc 对0x0F/0x14/0x18/0x47/0x06/0x82调用 TraceDesktopWindow 的两行。该记录会查询窗口/desktop/WTS并同步文件I/O，虽不直接绘制，仍会阻塞 owner 消息处理，且托管异常可能越过 unmanaged callback，不是零行为影响。恢复无诊断回调的原处理链。WM_ERASEBKGND(0x14)返回1、WM_PAINT(0x0F) ValidateRect并返回0、其余 DefWindowProc 的原行为完全保留；这些是既有 HW 自有窗口策略，历史 `HW-dedicated-STA-presentation-current.md` 已记载，不是本次 BX 新加，不应误撤回为默认背景重绘。未新增背景刷、InvalidateRect、重绘或额外消息泵。
4. `Docs/completed-work/replacement-supervised-desktop-delivery-differential.md`：本独立报告（以上源码两文件加本报告共三文件）。没有改 DesktopDuplication、ExitDiagnostics、NativeRun、其他代理或共享报告。

因此本次代码只做**诊断隔离及撤销诊断对默认路径的扰动**，不是生产修复。旧捕获证据属于修改前测试身份，不移绑到当前源码，也不宣称当前默认时序已经实跑验收。源文件变更后已回读关键条件编译段及完整 WndProc。

## 7. 验证与最小后续假设

唯一验证是专用工具 Release build `Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`。失败；构建日志唯一 error 为未修改文件 `ExitDiagnostics.cs(24,37)` 的 CA2208（ArgumentException 参数名作为 message），未触及该代理文件、未降低分析级别或隐藏失败。未取得最终成功构建；显式诊断符号配置也未独立构建，不能声称双配置已绿。TFM net10.0-windows、Nullable enable、SDK global.json10.0.107 均未改。

最小可执行后续不是再次重复相同超时：先由入口负责人处理其 CA2208，再分别编译默认与显式诊断配置，核对默认发现不包含 GDI 方法。若以后获准做新实验，只选一个32×16窗口、一个 swapEffect、一个红色提交，用既有专属诊断记录同窗口 GDI 可见→D3D9 backbuffer/Present/query→front/screen及duplication的同任务链；对比有无诊断阶段I/O的输出差异以检验“观测扰动/桌面交付状态”假设。不得改变等待阈值、skip、并行或生产合同来制造通过。若未提供新的状态观测手段或可区分变量，不再重复捕获。当前足够报告事实，不足够确定 DWM/驱动/会话根因。
