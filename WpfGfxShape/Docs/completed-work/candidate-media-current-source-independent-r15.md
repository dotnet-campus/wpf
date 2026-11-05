# r15 当前源码独立审查、最新 x86 对齐与恢复入口

## 权威结论：有限视频两架构通过，OverallOpen

先亲读 r14 清理报告、r14 x86 报告及原 handoff 顶部，再读实际生产相关方法、验收源码和真实收据。本轮生产代码/配置修改0；未改 default async、oracle、expected、颜色/时钟阈值、DPI、flags，没有原版加载/副本 fallback、终端脚本或生产测试耦合。

当前 x64 沿用清理修复后的 E158 身份两轮实证；当前 x86 从本轮冻结源码独立 NativeAOT 发布，并以同一新包两轮真实 consumer 通过。旧 x86 3D889C…50AD8 仅为历史身份，不认证当前清理修复。本报告不宣称异常注入、全部交错、refs0、HWND resize 或 full 完成。

## 1. 生产独立复核

实际阅读 IndependentVideoPlaybackOwner.CloseAsync/WaitForExitAsync/Run finally、IndependentMediaIdentity.BeginClose/BeginProcessExit/CompleteCloseAsync/Release/FreeIdentityAsync、BeginOpen/Notify/Completion、IndependentVideoFrameOwner.Dispose、MediaEventProxy 的 admission/Dispose，以及 MediaFoundationSourceReader.Close。

- owner finally 对 reader、Frames、commands、commandReady 分别捕获原 Exception，继续独立释放，清空 reader/pending，取消 play/preroll/seek，最后结算 _closed。单故障保留原异常类型与对象，多故障 AggregateException 保留内部 cause；WaitForExitAsync finally Join 保留线程退出要求。Frames.Dispose 只禁止新获取，不使已交付 lease 失效。没有把失败 HRESULT 改成成功。
- CloseAsync 的 _closing 门保证重复关闭返回同一 completion；BeginClose 先在锁内分离 owner/events，锁外 Dispose/release client，避免把客户端重入置于 identity 锁内。CompleteCloseAsync 直接返回 WhenAll，保留 pending operation、旧 cleanup、exit events 与 owner close 的完整故障集合，不用 await 再包装丢失多异常。立即 Open 可能覆盖当前 operation，但旧 cleanup 仍被后续 Close 累积；不能声称 Open 等待旧清理。
- Release 在 RegistryGate 内减引用并移除，锁外 BeginClose(true) 后 FreeIdentityAsync；后者读取 completion.Exception 并 Console.Error.WriteLine 完整异常，包含 HRESULT、堆栈和内部 cause，finally FreeIdentity。不再有仅保存而不观察的 _releaseCompletion。IUnknown.Release 返回引用计数，不应把稍后清理故障跨 ABI 同步抛给已释放客户端。本轮未发现需最小修改的确定性新增生产错误。
- 明确保留边界：reader Flush 失败或 WaitDrain 10秒失败会在内部释放之前返回，Dispose 经 ThrowExceptionForHR 抛出。owner 的独立资源继续尝试不等于 reader/callback/COM 已全部安全释放。MFShutdown 失败仍传递原 HRESULT。没有强制释放仍可能运行的 callback，也没有吞错伪造成功。
- Notify 的 owner 检查与锁外 admission 非原子；Dispose 允许已 admission callback 完成。任意旧 epoch 通知隔离尚未证明。stderr 宿主持久化及 stderr 自身 I/O 抛错后的二次 Task 故障仍未闭合。Run 外层异常等非本次 finally 正常退出路径不在动态证明范围。

上述故障与重入结论是静态核查，不是动态注入测试。现有真实视频只覆盖成功生命周期，不能以 exit0 替代多故障/引用计数证明。未新增生产导出、可见性、测试开关或失败注入接口。生产既有诊断机制未作扩大。

## 2. 源冻结及 x64 验收锁核查

证据根 `artifacts/candidate-media-current-r15/`。新增独立只读工具 `Tests/CurrentSourceAuditR15`，仅 .NET 文件、SHA/PE/metadata 读取，不加载候选 Native，不引用生产项目，也不改变生产构建。

最终冻结 `snapshot-final/Source`，800项清单 `snapshot-final-readback/Evidence/source.sha256`，SHA256 **3BFCEEE50FF1B6060FC679CE13F7FD4BA07413E7FF1887D390E781D93C6B8126**。范围包括 Code 全部文件、Tests 的 cs/csproj/targets/resx（排除 bin/obj）、局部 Directory.Build.*、global.json、两个直接嵌入资源；不是整个 WPF 上游仓库或 SDK/NuGet 的完整冻结。两个资源另保留相对生产项目所需路径，支持从快照直接编译。最终 Native、video、supervisor 实际均以此快照下项目为发布目标，不以发布后的清单倒推编译身份。

审计逐项重算 r14 的505项清单：**changedPreviousFiles=0**。其中 identity SHA D54165A10D6B2D8A54BC4C6CD6D8E99210B2600427D26B1BB7A3E6DFCE626C4F、owner SHA 67B563A57AABDEAB875A2CA32703D53AD3232F3DB966E4A09D6B89094FB1B2DD 与 r14 一致。该对比不声称505项包含所有本轮新增验收文件。

`audit.json:7543-7577` 重新计算旧 x64 Native E158…1073B、4895744、Amd64、无CLR，以及 x64 harness E61089A0A04DF598EF2CE05F0A9234DEB922710C7F373BA3D33BBC809E0B3B93 / MVID ac869081-a1bb-47f2-871c-a6f00d7978d6。对该实际 DLL 的 Resolve、Checkpoint IL 均检出 Monitor.Enter/Exit 引用；当前源码两处也确为同一 Evidence 锁。因此 x64 r14 包已包含验收证据文件串行化修复，无须无故重发 x64。这里只验证所述日志锁，不夸大为整个 Evidence 字典任意并发读写已形式证明安全。

初始冻结 `snapshot` 清单 D141…F261 已执行审计、505项也相同；发现资源需另保留编译相对路径后，只补审计 csproj 的资源复制路径，再重新冻结为上述 final。没有用初始快照发布生产，也没有隐去该两次审计执行。

## 3. 新 win-x86 当前身份与两轮实证

从 snapshot-final/Source/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x86/self-contained，专属 native-obj/native-bin/native-x86；没有设置 PublishAot=false。发布 exit0，亲读日志 Generating native code 及 lib/exp 输出。工具保留49行日志，但原 stdout 796407字符只返回最后20000字符；**不是完整构建 stdout 收据**，不把尾部称全日志。既存 CA1416 警告保留。

- Native **9FE6044FCCEE5C5A3D6E9452E8FF2506D4A67716942D11732CC60611BDA3E7FE**，**4333056字节**；runtime-audit.json:20622-20628 machine=I386、managed=false；harness PE 检查明确拒绝CLR header。
- harness **5EC368789BC237DF6AE0FE13C99174F615770D094DD35A421B5690CD816875A3**，58368字节，MVID **5a3dc746-eddc-473a-85f4-18b5d5f6dd0f**。
- 包为 package-x86，监督为 supervisor-x86。验收从同一冻结源码构建，保留 x86 架构负控与日志锁，PureVideoPublic 原断言未改。

|运行|task|正常监督 PID/exit|视频 PID/exit|正常三负控 PID（均 exit1）|
|---|---|---|---|---|
|readback|7b497e881479485c96b00b4a717d93a9|6020/0|11448/0|2928、10580、3424|
|recheck|78f2d8e65c8846098bc1e6bcc8f50022|1696/0|3852/0|1720、11756、10176|

亲读两轮 package-identity、actualNative、父子exit、stderr、负控结果、关键视频记录与唯一 graphics 模块：均 timeout=false/reaped=true/drained=true，视频和负控 killError=null、正常门 passed=true/failures=[]。父与视频 stderr 均空。actualNative 均为各自 task 的 Package/Native/wpfgfx_cor3.dll，SHA/长度一致，nativeBasenameHandleMatches=true，processArchitecture=X86；allWpfgfxModules 唯一该路径。外层 sandbox 工具不提供数值 exit，不伪造外层0。

首轮使用 verify-supervisor，还执行既有故意破坏拒绝阶段的 verify-failure-exit：监督12252 exit1，负控6332/2304/13116均exit1，其中 missing-digest 按设计 wrong-failure-stage/wrong-final-stage。此模式保存在 verify-failure-exit-process-results.json，并覆盖根 process-results.json；正常负控应读 **normal-process-results.json**。这是监督失败传播负控成功，不是业务失败后改包重试。第二轮 verify-video 只跑正常完整门。

两轮均保留原 paused seek 0.5/2.5 红蓝及原EOS；额外一次Play、中间无Pause/seek的连续0.8/2.8，sample=0000FFFF/FF0000FF、paused=false；第二EOS，随后同一player三次相邻 Close(); Open(uri)，两调用间无I/O/等待。reopenedState cycle2/opened4/ended2，video=true/audio=false、4秒、2x1，cleanupDiagnostics=[]。AVI F277087DA15898B6C96720AAEF17062A6E9D221856055CD2B51014FED323E7E4 /864字节及四份Pbgra32保留在 candidate-raw。亲读的是结果字段及实际身份；本轮没有另写像素原始字节复算器，不冒称独立逐字节复算。

视频上限120秒，监督180秒，回收/管道10秒，外层300秒，未加无限等待或 retry-to-green。

## 4. x64 沿用的亲核实证

原件 `artifacts/candidate-media-cleanup-r14/{readback,recheck}/Evidence`。两轮亲读 package/模块身份、监督及视频 exit、三负控、stderr和连续/重开结果，不仅转述报告。

Native **E158C6277E3B3043E25A98D808C9D7B5ACB81CFE0DE8EBE5D3187B8381C1073B** /4895744；harness 身份见上。

- task61dfbbe31e754638a474c57e01388457：监督9096/0，视频1608/0；三负控2656/10848/11820 exit1。
- task8b2f8139cab14f0286fff99f9afc776e：监督7084/0，视频7784/0；三负控2464/10940/4660 exit1。
- 均无timeout、已reaped/drained，视频killError=null、failures=[]；父/视频stderr空；actualNative及唯一graphics模块为各自Package Native且SHA/长度一致。连续0.8/2.8红蓝、第二EOS、三次立即Close/Open、cycle2/opened4/ended2、cleanupDiagnostics=[]与新x86有限范围一致。

本轮 x64 Native publish及x64 consumer执行0；这是旧独立实证回读与内容对齐，不编造新x64运行。

## 5. 执行账与交付改动

本轮 publish **5成功/0失败**：只读审计2（初始/最终），Native x86 1、video x86 1、supervisor x86 1。sandbox **4**：只读审计2、x86 consumer2；结果均已回读落文。消费两轮正常三负控合计6次，另首轮监督失败传播支路3次，不能混计为9次正常通过。

run_tests **1**：IndependentMedia|IndependentVideoPlaybackOwner|MediaEventProxy|SystemMediaSource，exit0，**83通过/0失败/0跳过，46ms**；未新增测试，未虚构本轮另一次2项运行（r14的83+2是历史账）。独立run_build0、x64 Native0、x64 consumer0、resize/desktop/full业务0、原版加载0。生产源码/配置修改0；新增独立审计两文件、报告，并更新handoff顶部。发布/审计产物全部隔离在本轮目录。

## 6. 精确未闭合与下一最小项

1. reader Flush/drain/MFShutdown、多pending close同时失败未动态注入；完整cause/聚合及独立资源继续释放仅静态。reader内部drain失败后的安全回收策略Open；refs0 Unknown。
2. 任意callback admission/旧epoch通知、全部重入与线程交错未证明；标准错误宿主持久化及其自身写入失败Open。不能以成功视频代替故障覆盖。
3. r13正确STA后 nativeControl 白像素 actual00FFFFFF/expected00563412 原断言失败，connect/shader/动态resize未进入。该历史阻塞不因视频通过而消失，未判定宿主硬限制或生产根因。
4. r12 RTB resize只保留其内容/生命周期有限结论，不等于HWND呈现；full缺可信oracle门保持，不运行full或伪造reference。
5. 下一最小项：基于r13已有父子收据/源码提出一个明确、可证伪的nativeControl白像素根因假设并收集所需独立诊断；只有该前置原断言通过才能继续原shader+resize全部断言与最终refs。不要再无目标改device/chain/DPI/flags。媒体故障回收另需不引入生产测试耦合的可安全复现路径/公开合同依据，未具备前不得冒称关闭。

总体状态 **OverallOpen**。本轮目标范围的最新x86有限门及x64实证对齐完成，不宣称产品全部完成。
