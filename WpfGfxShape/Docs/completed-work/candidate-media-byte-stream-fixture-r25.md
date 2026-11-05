# r25 独立 MF 外部 byte stream fixture / 有限合同实测 / OverallOpen

## review-run3 取证收尾（本节为当前结论，覆盖下文 run2 监督器待办）
本次只回读与落文，生产/fixture源码修改0，build/test/publish/sandbox均0。没有终端脚本、原版加载/复制fallback，也没有重复发布试验替代原件读取。此 slice 的证据交接完成；总体仍 OverallOpen。

### 分账与权威原件
- 前账：publish3（首次CA2201失败、后2成功），sandbox2（首次拉取失败、业务执行未知，后一次 run2 有效）；不能删去首次不明执行。
- review账：publish1、sandbox1，产物及正常/错误/负控原件在 artifacts/candidate-media-byte-stream-r25/review-run3/Evidence。
- 本次取证收尾：publish0、sandbox0、build0、test0，仅更新本文及 next-session-handoff。累计 publish4、sandbox3；不是本次重新执行4/3次。
- review程序集身份由 parent.json 与 negative-parent.json 一致记录：SHA256 A0C2E30BB1C863F04D01DA1FFB684E83C43D2178B13E26C5CDBE642108946E8C，MVID 72fc5194-c557-4e6e-9fde-f9cc46ac00b9。不是exe SHA、源码冻结或生产认证；本次没有重新计算hash。

亲读所有7份JSON（normal/io-error/invalid-scenario/parent/negative-parent 的 exit.json，以及两个父汇总）和 outer.json；亲读正常1172行/错误891行日志中的初始化、Open/首样本、held BeginRead、完成/Flush/drain及最终cleanup段，截断尾已补读；并读两份父stdout。未冒称逐字读完两份日志全部探测读。normal、io-error、parent、negative-parent原始stderr均空；invalid-scenario.stderr原文为 `Invalid scenario parameter.`。负控stdout无业务收据不是正常业务运行。

|被监督进程|监督父PID / 子PID|真实exit|timedOut / reaped / drained|startedMs → elapsedMs（监督者时钟）|
|---|---|---:|---|---|
|normal|11872 / 2984|0|false / true / true|6 → 383|
|io-error|11872 / 3416|0|false / true / true|418 → 793|
|正常父|4900 / 11872|0|false / true / true|6 → 869|
|invalid-scenario|4840 / 12740|64|false / true / true|3 → 62|
|负控父|4900 / 4840|1|false / true / true|903 → 1061|

全部 killError=null，无本次timeout/kill。负控父 passed=false 是预期失败传播，不能改写为exit0；supervise 以 negative.Collected && negative.Exit==1 验收负控。parent.json exit0/elapsed794；negative-parent.json exit1/elapsed96；outer.json exit0/elapsed1062、normalPassed=true、negativeExit=1。外层exit0是文件所记结论，未另获其上级OS exit收据，不将文件自报冒充更外层真实进程exit。

### 70秒共享预算与计时
实际 Program.cs 24–90：Main 首先 Clock.Restart，不再由首次Log才延迟起表。Parent 两组共享同一绝对 deadline=70000；每次启动前余额必须>6000ms，主等待 min(30000, remaining-6000)，kill后 WaitForExit(min(remaining,4000))，输出 Task.WaitAll(min(remaining,2000))；所有显式等待均从同一余额扣除，不再每子另加4+2秒形成72秒。未drain会取消输出复制，任何timeout/未reap/未drain/killError/非零exit/越deadline均失败。elapsedMs为监督者从Main开始的累计时刻，不是每子用时；正常377ms、错误375ms，绝非首receipt零耗时。两组父总794ms有效。

这是共享有界等待预算的静态实现核对与正常完成动态验收，不是硬实时证明。Process.Start/Kill、文件写入/Flush/Dispose、调度和同步系统调用没有可证明最大耗时；代码和收据明确 hardRealtime=false。kill/reap/drain分支的余额计算已读，但本次未触发kill，不能声称动态验过kill-tree。Supervise正常父预算90秒、负控阶段绝对100秒，不与正常父70秒混为一谈。

### ABI / asyncResult所有权有限复核
本次重读本机 Windows SDK 10.0.26100.0：mfobjects.h 3710–3745、4880–4970，以及 mfreadwrite.h 356–435、933–960；对照实际 Program.cs P/Invoke、ByteStream18槽、ReaderCallback6槽、SourceReader的5/9/10槽。DWORD/ULONG/BOOL32位，QWORD64位，Seek offset有符号64位；IMFAsyncResult GetStatus槽4返回HRESULT、SetStatus槽5接收HRESULT，与源码一致。项目为net10.0、Nullable/unsafe开启；没有改SDK/TFM。

重新取得 MFCreateAsyncResult/MFInvokeCallback 官方正文：创建输出引用须由调用方Release，Invoke从MF工作队列执行，Shutdown后不保证调用。本次源码 BeginRead 使用真实系统result，借用callback/state交系统创建；保留result owner到Complete，SetStatus后Invoke，再Drop自己的result；EndRead借用result取status，不Drop借用引用。每次成功BeginRead额外持有stream到EndRead，结果字典移除后outstanding归零并释放该引用。日志有Invoke S_OK、result owner Release返回1、实际EndRead；没有证据支持“此处必须再AddRef一次”或“EndRead应释放借用result”。此有限检查未确定已执行路径存在ABI/所有权缺陷，因此不为求绿改fixture，也不新增发布复验。未调用失败路径、任意多并发/Seek取消/非法参数和永久不回调不在认证范围；Invoke失败时的通用回收不获本次保证。

### 有限MF合同结论（不是生产负Flush回收）
|事件，子自身ms|正常2984|I/O错误3416|
|---|---|---|
|CoInitializeEx-MTA / MFStartup|36 / 39|31 / 33|
|Open request / return S_OK|142 / 190|140 / 188|
|首样本 / held BeginRead|196 / 196|193 / 193|
|放行完成 / EndRead|198 S_OK / 198|196 0x8007001F / 196|
|Flush request / return S_OK|199 / 199|195 / 195|
|OnFlush / drain|207 / 207|202–203 / 203|
|reader owner Release|210|204|
|callback / stream owner Release|210 / 210|204 / 205|
|MFShutdown request / return|210 / 307|205 / 308|
|原MTA线程2 CoUninitialize / result|308 / 308|308 / 308|

两组Open完成且确认首样本后真实BeginRead held=true/count262144/outstanding1；错误状态由SetStatus写入（SetStatus调用本身返回S_OK），并由真实EndRead返回0x8007001F，终止后清理成功。两组Flush均S_OK、Flushes=1、drain Active/AsyncOutstanding/SyncActive均0，reader和两个自定义identity owner在原MTA释放后MFShutdown/CoUninitialize配对。Samples最终均2：pending是源预读，同一个ReadSample仍可由缓存完成，不能证明产品样本请求被源I/O阻塞。正常Release reader内部触发stream Close的error=true是关闭标志，不是额外负Flush。

仅能据此确认这两次受控MF运行；negativeFlush=false、lateDrain=false，未触发目标故障即停止。reader Release返回0、两个自定义identity refs0、asyncResult Release返回1均只是对应调用观测，绝不推断系统所有COM refs0或生产COMrefs0。OnFlush/active0也不是所有未来OnEvent永久栅栏。

### r23产品故障责任设计：独立结论与下一具体动作
本次重读产品 MediaFoundationSample.cs 692–730 与 IndependentVideoPlaybackOwner.cs 280–335，r23根因仍在：早返回之后finally销毁队列并退出原MTA；ClosedReader强持资源但没有可执行收尾责任。

1. **正Flush但晚drain**：可以在关闭状态中保留原MTA、reader/callback及其事件/队列，禁止新业务和样本准入；晚OnFlush/最后callback退出只通知，随后由原MTA执行一次Release和MF/COM配对。条件是确实观察到所需终止/排空，并有明确的后续回调生命周期合同；不能在超时处先销毁执行器，也不能由MF worker直接Shutdown。当前生产没有实现此责任续存。由于成功Flush也无最大完成时限，此方案只在最终完成确实发生时条件安全，不能证明有界退出；永久保留前台MTA不是合格产品恢复。
2. **负Flush且无OnFlush**：可以暂时保留原MTA/资源避免活动对象被强释，但无法从HRESULT、瞬时active0、重试次数或经过时间推导终止。没有官方shutdown/取消并join所有相关执行的合同，就没有可验证的通用进程内恢复；不能等一个未承诺的OnFlush后宣称必能收尾。若另有经核准的终止合同，必须先实际完成，再在原MTA收尾；本fixture并未提供它。
3. **下一具体建议是产品边界决策，不再追加外部流试验**：对不可信/可能永不回调源，将MF初始化、reader、callback及原MTA整体置于独立媒体worker进程；主进程仅持IPC和可回收句柄。关闭正常走已确认drain后配对；不收敛则报告关闭失败，预算到期终止整个worker并确认进程退出、收集诊断，再释放主进程句柄。进程终止是故障隔离，不冒称MFShutdown/COM正常配对或资源合同成功。实施前先明确跨进程帧/错误/取消协议与worker退出验收；若拒绝隔离，必须明确“不保证任意源故障下有界恢复”的产品限制，并取得官方可用shutdown合同才承诺通用恢复。
4. 不选择无限前台reaper、后台隐藏泄漏或超时强释。若后续获准生产改动，应统一处理playback/cancellable/Create失败责任入口，并分开业务关闭结果、资源清理结果、worker真实退出结果；r22旧冻结认证不得移绑新版本。此处仅责任设计建议，生产修改0。

full可信两JSON、桌面原nativeControl失败、生产负Flush/晚drain/永不回调、URL强取消、产品源pending及生产COMrefs0继续Open；不把清单未闭合解释为本slice未取证。下文为run2历史账，其“先补监督器/另行执行”待办已由review-run3覆盖，不再派发。本文与恢复入口更新后均需回读。

## 以下为前账 / run2 历史记录

## 交付与范围
新增 Tests/CandidateByteStreamR25/CandidateByteStreamR25.csproj 和 Program.cs，net10.0、win-x64、自包含托管控制台，无新增包、无生产项目引用。生产源码修改0，既有测试源码修改0。没有终端/脚本、原版 gfx 加载/复制/fallback、生产钩子/开关、defaultasync、oracle、阈值、DPI 或 flags 改动。

这是独立系统 MF 合同实验，不是产品入口测试，也不是生产资源问题修复。已跨过 r24 缺绑定门：真实外部 IMFByteStream、系统 MFCreateAsyncResult/MFInvokeCallback、真实异步 Source Reader，Open、媒体识别、首样本、两组有界实验均实际执行。没有无限试探直到失败 Flush。

## ABI 与所有权核准
本机 Windows SDK 10.0.26100.0 um/mfobjects.h：3717–3740 IMFAsyncResult、3841–3854 IMFAsyncCallback、4885–4968 IMFByteStream；mfreadwrite.h：281–289 创建函数/async 属性、356–434 reader、933–958 callback。读取时较长工具结果会截断；随后补读实际调用相关部分，不把网页按字母排序当 vtable。IUnknown 三槽后按头文件声明顺序；STDMETHODCALLTYPE 对应 unmanaged[Stdcall]，DWORD/ULONG/BOOL 32位、QWORD 64位，Seek offset 有符号64位。

- IMFByteStream IID ad4c1b00-4bf7-422f-9175-756693d9130d，继承 IUnknown；完整18槽：QI/AddRef/Release、GetCapabilities/GetLength/SetLength/GetCurrentPosition/SetCurrentPosition/IsEndOfStream/Read/BeginRead/EndRead/Write/BeginWrite/EndWrite/Seek/Flush/Close。只读、可定位，flags=1|4；写入方法返回 E_NOTIMPL。
- IMFAsyncResult IID ac6b7889-0740-4d51-8619-905994a55cc6，继承 IUnknown；GetState/GetStatus/SetStatus/GetObject/GetStateNoAddRef。只调用已核准槽4 GetStatus、槽5 SetStatus，不自造 result/vtable/MFASYNCRESULT。
- IMFAsyncCallback IID a27003cf-2354-4f2a-8d6a-ab7cff15437e，继承 IUnknown，GetParameters/Invoke。fixture 不实现它，由 MF 调用者提供，传给系统结果对象。
- IMFSourceReader IID 70ae66f2-c809-4e4f-8915-bdcb406b7993；调用槽5 GetNativeMediaType、9 ReadSample、10 Flush。IMFSourceReaderCallback IID deec8d99-fa1d-4d82-84c2-2c8969944867，6槽，复用既有生产 unmanaged identity/GCHandle 的风格而非复制生产实现。
- MFCreateAttributes 返回一个 owner ref；SetUnknown 槽27持有 callback；创建 reader 后释放 attributes。media type 输出单独释放。
- stream 和 callback 创建各1 owner ref，QI/AddRef/Release全部打印。reader owner 由创建函数返回，drain 后在原MTA Release；再释放 callback/stream owner。
- BeginRead 参数 callback/state 借用，仅交给 MFCreateAsyncResult 创建真实结果；结果持有其依赖，调用者拥有结果输出引用。待完成时保留 result、原始 buffer/count，另对 stream AddRef 至 EndRead。buffer 在放行前不访问。完成时写buffer、记录count、SetStatus、MFInvokeCallback，再释放结果owner；MF队列/回调持有其执行引用。EndRead 使用调用方借用 result，不 Release 借用指针，取状态和count，减 outstanding 并释放 BeginRead 所加 stream ref。
- observer 不保留 OnReadSample 的借用 sample，不对借用 sample Release。callback active进入/离开及OnFlush分别记录。MFStartup/Shutdown、CoInitializeEx/CoUninitialize只在每个子进程原MTA线程2配对，不在MF worker关闭平台。

本轮直接取得 Microsoft 正文：
- https://learn.microsoft.com/en-us/windows/win32/api/mfapi/nf-mfapi-mfcreateasyncresult
- https://learn.microsoft.com/en-us/windows/win32/api/mfapi/nf-mfapi-mfinvokecallback
- https://learn.microsoft.com/en-us/windows/win32/api/mfobjects/nf-mfobjects-imfbytestream-beginread
- https://learn.microsoft.com/en-us/windows/win32/api/mfobjects/nf-mfobjects-imfbytestream-endread
创建结果输出须Release；MFInvokeCallback经MF work queue，不得Shutdown后期待完成。FromByteStream/Flush/OnFlush/Shutdown合同另见亲读的r24/r23；本轮不冒称重抓全部页面。

## 实验输入与握手
在内存构造既有纯视频 AVI 写入结构的较大变体：320x240、24位 DIB、120帧、10fps，无音频，27649184字节；SHA256 82EFA68A4704D06B0CCD9F136D6BD3F856D826EFDBF523CB5E335E251E56D94C。每子峰值含MemoryStream/ToArray副本，不是无限流；顺序执行两子。没有生产 fixture 注入。

Open阶段所有读正常完成，GetNativeMediaType成功、实际收到首个非空样本后才Arm。后续最多128次请求且子Clock<11秒，用 Entry 和 ReadDone 明确等待源入口；不是暂停OnReadSample冒称源pending。真实命中的是BeginRead，count262144、held=true、outstanding1。正常组放行成功完成；错误组先Flush，再将受控源结果以HRESULT_FROM_WIN32(ERROR_GEN_FAILURE)=0x8007001F完成。错误状态保持，后续读不继续成功，Close释放所有pending。同步Read也有3秒预算错误出口，但本次门控不是同步Read。

**关键负结果：源端pending是预读；第二个ReadSample仍能由缓存完成。** 两组均Samples=2。因此没有证明样本请求依赖这个挂起I/O，也没有把系统预读全部与真正阻塞样本混为一谈。没有触发负Flush或超过10秒晚drain，停止本轮，不扩大试探。

## 实际执行账
专用publish共3次：首次CA2201失败（显式new COMException），改为检查原HRESULT；第2次成功；第3次在固定工作目录修正后成功。独立build0/test0，publish内含实际编译。最后发布日志11行已读，exit0，无警告行。完整命令：
`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\CandidateByteStreamR25\CandidateByteStreamR25.csproj -c Release -r win-x64 --self-contained true`

Windows sandbox调用2次：首次指定Evidence拉取失败，工具只返回FileNotFoundException，拉回run目录的发布文件，没有父子收据；不能断言首次未执行业务或其exit，不计已核准有效实验。将工作目录固定AppContext.BaseDirectory后第2次拉取整个任务，获得下列正常/错误各一次有效记录。首次不明调用不能藏成执行总数1。

权威原件根：artifacts/candidate-media-byte-stream-r25/run2/Evidence。
- normal.stdout.jsonl 1172行，io-error.stdout.jsonl 891行；两份stderr实际为空，已读。
- parent.json 27行已全文读，父9772；正常子12896、错误子3100，各exit0、timedOut=false、reaped=true、drained=true，父JSON exit0。sandbox返回两份父stdout child-receipt；没有另存原始父stderr文件，不能声称其文件已核空或给出工具未返回的外层exit数值。
- 程序程序集SHA256 D0B31213111CA89C1A80445C9321D1C18E7865B3CD4DFEED59E4DDEC3F529ECA，MVID 2f852d42-cbf5-42c1-b266-be0b735f8c04。这是DLL身份，不是exe SHA、源冻结或生产认证。

子时间线(ms，分别以各子Clock为准)：
|事件|正常12896|错误3100|
|---|---:|---:|
|COM/MF初始化|33/35|29/30|
|Open request/return S_OK|139/185|132/176|
|首样本确认|190|182|
|held BeginRead/source观察|190/190|182/182|
|Flush request/return S_OK|193/193|183/183|
|受控I/O完成/EndRead|193 S_OK/193|184 8007001F/185|
|OnFlush/active0+outstanding0+sync0|201/201|191/191|
|reader/callback/stream owner Release|204/204/204|193/193/193|
|MFShutdown request/return|204/298|193/298|
|原线程CoUninitialize/最终result|298/299|298/299|

本轮亲读首段、Open/首样本/门控搜索命中、正常850–882及1130–1172、错误840–891（截断尾再875–891补读）和parent/stderr，不冒称逐字读过两份日志全部探测读。引用日志可见两个自定义identity最终refs0，reader owner Release返回0；不外推系统内部全部COMrefs0，更不外推生产refs0。

## 静态限制与精确剩余
- 父每子WaitForExit30秒，kill后reap4秒/输出收集2秒，顺序两组最坏72秒加启动开销；外层90秒。**尚未实现此前70秒父总硬deadline**。kill算失败，绝不是清理成功；本次无kill。
- 父Clock是beforefieldinit延迟初始化，正常receipt elapsedMs=0、错误403不表示首子零耗时；子时间线有效。父严格总预算与可靠起始计时、原始父stdout/stderr独立持久化尚待补齐，不能称监督器完整验收。
- 异常时不强释活动COM，不伪造清理通过；子返回非零后由进程隔离结束，不能将此失败路径当生产回收。BeginRead挂起由driver有界放行，非通用永久不回调源实现。Seek flags取消扩展/多并发请求/非法指针防御不是本最小fixture认证范围。
- active0/outstanding0是观察点，不证明未来永久无callback；本次reader Release后自定义refs归零并完成MFShutdown只是这次系统实验。未知API失败的异常通路不保证MF/COM正常配对，不声称已测。
- 当前最重要目标故障（负Flush、晚drain、样本请求依赖源pending）均未触发；有控I/O错误确已完成并终止，不以此支持强释生产COM、删早返回或改默认异步。

后续若继续，应先仅修独立监督器总deadline/计时/父输出证据，另行授权再执行；不要重跑直到Flush失败。生产最小修复仍需独立因果证据，不把本次系统实验当产品回归。r22冻结四业务历史身份不变，不重派、不移绑。full缺可信两JSON、桌面原nativeControl失败，均Open；内部负Flush/晚drain/永不回调、URL强取消、产品源pending和生产COMrefs0仍Open。本文与恢复入口写后回读。
