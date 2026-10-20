# CJ：异步 Present 所有权审计与可控最小验证

## 1. 裁决与范围

**静态正常路径合同已梳理；真实 surface 离屏消费、发送失败后的全生命周期及堆损坏根因未闭。没有运行验收。** 按用户允许的证据不足分支收口，不冒险运行旧 sampler，不重跑桌面 fresh、不盲 full。不因空事件返回 E_HANDLE 判为产品错误。

先创建本报告后持续写入。唯一受跟踪新增为本报告；机器可读审计状态为 `artifacts/cj-present-ownership/audit-status.json`。未改生产、原夹具、VisualCache/historical、NativeRun、ExitDiagnostics、曲线、捕获、WindowQ、项目打包或共享文档。没有新增 Host、脚本或终端操作；本轮没有构建、发布、沙箱执行。未拿官方冒充候选，也未把并行 CI 源码绑定 CF/BD DLL。

本报告的“官方”静态依据是仓库 `src/Microsoft.DotNet.Wpf/src/` 源码，**不是对已安装官方二进制的反编译或构建配置认证**。候选源码观察同样不是 CF/BD 二进制行为证明。历史运行只引用 BN/BP/BS/CG/CH 报告，不冒称 CJ 亲读这些历史任务的全部原始 JSON/TRX。

## 2. 已读规则、结构与证据

已读执行规则27行、完整替换方案120行、current-work-item31行；BN、BP、BS、CG、CH 报告主要节及尾节，BL监督报告全文。历史报告部分大段首次返回截断，关键结论/尾节补读；不声称全部历史长报告逐字无截断。没有发现 AGENTS.md。

已枚举解决方案及正式 ComAcceptance 文件清单；读取正式 csproj、global.json、Directory.Build.props/targets。net10.0-windows、Nullable enable、SDK10.0.107/latestFeature，MSTest4.0.1，未修改。读取官方 D3DImage 创建/更新/提交/回调/注销段、InteropDeviceBitmap 主要生命周期、CMILCOMBase 和 bitmap 继承、UCE发送/提交/flush/销毁、生成命令；读取本地 VisualCache 初始和 mutation/cleanup、独立 packet/Release、候选 interop/resource/丢弃包处理。

历史边界：BN记录双架构官方首次 Commit E_HANDLE，随后 DestroyChannel 中0xC0000374，无完成 sampler TRX；BP无surface初始化后4/4退出0只证明最小事件序列；BS真实内容未进入connect；CG backbuffer已更新而desktop交付失败；CH仅CF软件有限修复验收，不关闭同步Event0/堆损坏/HW/loss/full。上述不是本轮运行。

## 3. 官方创建、更新、提交合同表

以下路径相对官方源根；线程标签来自源码注释和调用链，不声称已测实际OS线程。

|阶段与依据|所有者/线程|必要偏序与释放责任|
|---|---|---|
|PresentationCore/.../InterOp/D3DImage.cs:60–89|UI/Dispatcher对象持有 ManualResetEvent(true)、实例delegate|原柄由SafeWaitHandle生命周期管理；finalizer Detach，不能让原柄直接成为native待关闭句柄。|
|SetBackBuffer:145–212；InteropDeviceBitmap.cpp:29–78、400–475|UI创建bitmap，返回一个调用方引用；native构造对用户surface AddRef|旧bitmap先Detach、取消pending commit、清dirty；新版本递增，注册async UpdateResource；用户指针_pUserSurfaceUnsafe本身不是额外引用。|
|D3DImage.cs:544–575、580–610|UI锁/commit订阅|Lock等待canWrite并撤销待提交；只在主async channel在场时订阅。换bitmap后须先UpdateResource，SendPresent检测等待位并推迟，不可Present旧bitmap。|
|UpdateResource:761–817|UI生产channel命令|对interop AddRef后写入24字节命令；同步channel另CopyBackBuffer并对软件位图AddRef；只有异步更新清等待位。|
|resources/d3dimage.cpp:180–237|消费端持有传输引用|先UnRegisterNotifiers并Release旧引用，再偷取interop引用；软件指针包装后释放传输引用；失败Release已接管interop。不是消费端再次AddRef。|
|D3DImage.cs:657–727|UI复制event|DuplicateHandle SAME_ACCESS、不可继承；只转交duplicate；独立batch必须进入本次commit；send后Reset原event阻止下一次写入。|
|UCE clientchannel.cpp:315–369、550–575|channel录制/提交|Send只录制；separate batch关闭自身并恢复原batch；Commit提交closed batches，不代表异步处理结束。|
|resources/d3dimage.cpp:249–289|合成线程ProcessPresent|调用bitmap.Present、更新software fallback及NotifyChanged；cleanup即使Present失败也SetEvent(duplicate)再CloseHandle(duplicate)。|
|D3DImage.cs:745–759|合成回调→UI Dispatcher|availability回调只排Dispatcher操作；UI按version过滤过期消息。它不是每帧Present完成回调。|

事件正常路径责任：原柄始终由托管对象持有；duplicate在成功录入后随命令转交；ProcessPresent关闭一次。观察原event被置位只能证明该命令到达其cleanup，**不能证明像素成功消费**。Present失败也唤醒UI；无interop的资源也照样唤醒。不能要求availability每帧恰好一次：官方明确可多次同值，未发生adapter状态转换也不保证每帧触发。

官方Reset在SendCommand之后是依赖MediaContext尚未Commit的顺序；独立诊断应先Reset、再发并Commit，不能在工作线程可能已SetEvent后Reset而丢完成信号。此差异只能用于新诊断设计，不据此更改真实WPF或原夹具。

## 4. 注销、flush与cleanup合同

|节点/源码|责任与限制|
|---|---|
|InteropDeviceBitmap.cpp:459–475 Detach|UI在m_cs内Release用户surface并清callback；不释放front realizations，也不立刻注销device manager listener。|
|同文件490–553 NotifyAdapterStatusInternal|render在锁内处理状态，再调用callback；fallback=false时无效状态释放用户surface并ReleaseResources；fallback=true保留surface及软件副本。|
|同文件436–445析构|UI或render；RemoveAdapterStatusListener，Release软件bitmap/surface，DeInit锁。Detach≠最终Release。|
|D3DImage.cs:835–851 ReleaseOnChannelCore|资源最后channel引用释放时注销async committing handler；resource删除是排队动作，不是返回即已消费。|
|同文件869–879 shutdown及finalizer|Detach阻止native后续通知；已经排入Dispatcher的操作仍靠version/对象生命周期处理，不能把Detach理解为撤销已排队消息。|
|resources/d3dimage.cpp:34–43|Duce析构UnRegisterNotifiers，Release interop/software；它不负责托管原event。|
|UCE apifunc.cpp:73–117|InitializePartitionManager(int)在建异步连接前建立scheduler/workers；所有channel/connection收尾后Deinitialize。外部专属进程避免与其他测试争全局manager。|
|clientchannel.cpp:768–840；connection.cpp:306–355|Destroy先SyncFlush，忽略其HRESULT，再DestroyChannel、Release自身；SyncFlush发送控制命令、CloseBatch/Commit并等待处理，channel非线程安全且不可重叠flush；内部WaitForSingleObject为INFINITE。|
|connectioncontext.cpp:386–440|强制关闭提交destroy resources+flush，转交batch后无限等待；外层监督是最后有界性，不是业务成功。|
|apifunc.cpp:212–225|Disconnect释放connection引用；不得在仍有待完成的客户端使用时先断开。|

新诊断成功路径应同一owner线程：初始化→连接/通道→更新/dirty/Present→事件完成→SyncFlush→停止新提交→Detach→删除资源并flush→DestroyChannel→Disconnect→释放调用方bitmap/surface/device/factory→Deinitialize。保留callback根直到Detach及消费排空。每一返回均记录HRESULT和线程，若内部阻塞由BL监督终止并判失败，不把kill视为正常释放。该顺序是待验证设计，不声称已跑；真实WPF换buffer允许Detach后仍显示旧front，不能强加“Detach必先销毁front”的错误合同。

## 5. COM/vtable与双架构布局核对

|对象/结构|x86|x64|裁决|
|---|---|---|---|
|HMIL_RESOURCE、HMIL_CHANNEL内部ID|4字节|4字节|processed/wgx_core_types.h:60–66为UINT32；不是MIL_CHANNEL客户端指针。|
|MIL_CHANNEL/HMIL_CONNECTION、surface、bitmap指针|4|8|导出用nint，不能把资源ID扩成nint。|
|MILCMD_D3DIMAGE|24，Type0/Handle4/Interop8/Software16|相同|wgx_commands.cs:47–54与.h:73–79；指针槽始终UINT64。|
|MILCMD_D3DIMAGE_PRESENT|16，Type0/Handle4/Event8|相同|本地Pack1 ulong匹配；x86不能用4字节事件字段缩包。|
|D3DPRESENT_PARAMETERS本地Sequential|预期56，HWND28/Windowed32|预期64，HWND32/Windowed40|本地前七UINT后nint，自然对齐；这是静态计算，未本轮sizeof/Marshal实测。不能把命令Pack1套给D3D结构。|
|Create的version/尺寸与fallback/callback参数|UINT32、BOOL32|相同|本地int availability/uint version宽度匹配BOOL/UINT；两个double保持8字节。|

真实bitmap返回类型是CInteropDeviceBitmap*，不是可以任意当IWICBitmapSource用的指针。继承已追到 CInteropDeviceBitmap→CDeviceBitmap(BitmapOfDeviceBitmaps.h:26)→CWGXBitmap→CBaseWGXBitmap(bitmap.h:65)，首基类CMILCOMBase(milcom.h:24)继承IUnknown；DECLARE_COM_BASE覆盖QI/AddRef/Release。由此本地首vtable[1]/[2]用Stdcall对bitmap AddRef/Release有源级依据，不能误套非IUnknown的IMILRefCount或凭空改槽。多继承的IWGXBitmap接口可能需要this调整，**不能把首表slot7当CopyPixels**；本地snapshot返回的是另一个IWICBitmapSource再用slot7，必须保持此区别。

官方托管用MILAddRef/MILRelease导出，exports.cpp:16–29内部转IUnknown；是独立诊断可优先复用的引用入口。当前候选源码interop首表仅3个IUnknown槽且QI仅支持IUnknown，不是IWGXBitmap完整表；其resource靠IsInterop识别私有对象。这是内部实现差异，不能由此单独判产品不兼容，也不能以候选表反推官方期望。

callback typedef在InteropDeviceBitmap.h:24没有显式调用约定；官方托管delegate无UnmanagedFunctionPointer标记、走平台默认Winapi，本地显式Stdcall。可核对签名宽度，但未获得实际x86 native构建的最终calling convention证据，**不从裸typedef推断cdecl后改全局Available**；x64统一ABI亦不能替x86验收。本轮不声称直接验证ESP。

## 6. 原夹具具体差异与失败责任

1. VisualCache.cs:83 connect(1)是同步连接；100–125真实surface初始Present，以及402–405 mutation Present均Event=0。官方仅async Present；同步UpdateResource走CopyBackBuffer。两个兄弟提交点同偏差，不能只修其中一个。send separate标志0也不同于真实MediaContext提交中的1；若独立显式CloseBatch/Commit，不必机械认为0必错，但不可忽略真实deadlock约束。
2. 原夹具没有partition初始化；同步本身不能据此判错，若改async则必须补初始化/反初始化并保留owner线程。SameThreadPresent(connection)不能充当async消费完成栅栏。
3. bitmap AddRef在每次ImagePacket前、调用方引用保留到Commit后，正常ProcessUpdate偷引用匹配；Commit成功在async只表示接受，释放调用方引用可以依靠传输引用，但不可无证据在失败分支重放Release传输引用。
4. finally是DestroyChannel→Disconnect→bitmap/surface→device/factory→window；未显式Detach、未观察availability、未独立记录每个引用最终收敛。静态Available为空，不能给出回调发生/线程/注销后无回调证据。静态callback不会被GC并不等于对象/资源已释放。
5. 官方ProcessPresent cleanup无条件SetEvent/CloseHandle；Event0导致E_HANDLE有明确源级解释。BN首次Commit失败不是候选有效输入行为证据，更不是官方bug。E_HANDLE与堆损坏的因果关系未证实。
6. 候选当前源码GeneratedD3DImageResource.cs:130–149 finally Complete，Complete忽略SetEvent/CloseHandle返回，故可能吞掉空事件错误；这仍是无效/不符合调用方合同输入差异，不据此改生产。

### 发送/提交失败不是“非零即调用者关闭”

DuplicateHandle本身失败时没有可转交句柄，保留原event。若已复制但尚未交付且能够证明命令没有录入，调用者应回收duplicate以及尚未交付的AddRef；一旦录入，就不能在finally无条件CloseHandle或Release传输引用，否则与消费端双关/双释放。

关键源码：clientchannel.cpp:568调用SubmitBatch后才将closed-batch槽清NULL；connection.cpp:390–395明示**即使失败也转交batch所有权**，connectioncontext.cpp:350–383在Submit后将局部清NULL，composition.cpp:495–500处理后delete batch。因此Commit失败不等于“未接收/未执行”。这条跨层失败路径尤其值得核查：源代码存在失败返回前closed-batch槽未清的可疑窗口；它是否参与BN重试/Destroy堆损坏，尚无实际二进制、首次free或dump证据，**仅列根因候选，不宣称已定位或修复**。

cmdbatch.cpp:49析构为空；crossthreadcomposition.cpp:125–167 zombie丢batch会唤醒transport flush，但本轮未找到逐个D3DImage event/指针补偿合同。generated_process_message.inl:233–255先验证resource，再进入ProcessPresent；若验证失败，不能依赖ProcessPresent finally。不得凭Send返回值推定duplicate恰好关闭，也不得在失败后查询已可能关闭并复用的裸HANDLE来证明所有权。失败实验需单独隔离并先证明录入/消费状态；本轮不注入OOM/坏指针/失效资源。

## 7. 为什么不执行“真实surface+事件+snapshot”伪闭环

决定性静态证据是InteropDeviceBitmap.cpp:700–818：CreateFrontBuffer只有当MIL device manager已存在windowed HW device时才创建color source；注释明确composition没有创建render targets时device不存在。生产者自己的D3D9 HAL device不满足这个前置；无设备时通知false并可能禁用HW。

Present:634–695允许无内容或D3D失败后成功返回，ProcessPresent依然置位event。UpdateFrontBuffer:880–917仅在m_poDeviceBitmapInfo存在时复制并清dirty。因此BP最小序列加一张真实surface后，仍可能只是“没有front的Present完成”。

GetAsSoftwareBitmap:1032–1050调用CopyToSoftwareBitmap，从用户backbuffer读取；注释明确不读front（UI线程安全原因）。它可以独立验证生产者内容，不证明合成消费。用它的red/blue/red与event并列，仍不能建立front内容闭环；RTB/software fallback、DwmFlush或DWM全局timing同样不能替代。

当前未建立公开、线程合法、可独立读取MIL硬件front的离屏consumer合同。直接猜C++成员slot/thiscall、反射私有layout或跳过现有sampler内容断言都不安全。故本轮选择停止，不新增一个默认发现即可能触发堆损坏的测试，也不启动没有决定性消费观测的四格实验。不是断言离屏方案永远不可行。

## 8. 下一轮可执行的最小验证规格（尚未实施）

A. 先仅在正式ComAcceptance新增独有 `CjPresentOwnershipTests.cs`，全部以 `CJ_PRESENT_OWNERSHIP_DIAGNOSTICS` 包围；不改默认入口、基础设施、原断言或打包。不新增Host。独立声明24/16字节命令、自然对齐D3D参数；先用纯C# sizeof/OffsetOf断言上述双架构尺寸。诊断标签不作为产品门。

B. 在调用真实surface之前，必须找到并审查现有导出可建立的**MIL硬件target/离屏consumer**及其合法读取接口：能建立windowed HW device，能区分producer与consumer像素，读取不靠desktop可见性、不走RTB软件fallback、不猜内部vtable。若只能创建HWND target但不能证明真实front内容，仍停止在“事件/生命周期诊断”，不得命名为surface消费验收。

C. 两模块×两架构四个独立受监督进程，先官方再候选；每格一次有界序列，三帧red→blue→red，全dirty、每帧一份新duplicate。owner线程只在完成事件之后再写surface。分别记录producer staging原始像素/哈希、consumer独立原始像素/哈希、event等待结果与QPC、callback可用性通知及version/OS线程、每个导出HRESULT、资源创建/转交/Detach/Release和最终退出。事件完成与availability分开，不能要求无状态变化时有availability通知。

D. 原柄由EventWaitHandle持有；SafeWaitHandle在复制期间保活；Reset先于可执行提交；AddRef/update在Present之前；消费成功、SyncFlush后才注销。对发送失败只按可证明状态回收，不在已转交分支盲CloseHandle。若无法建立失败态所有权，先只运行正常路径、明确失败态未覆盖，不能声称完整合同已验收。

E. 正常依赖图发布到独有 `artifacts/cj-present-ownership/{x86,x64}/{obj,bin,publish,package}`；显式AotDllPath选择可追溯CF或BD旧候选并保存路径/hash/来源快照；测试程序集是新诊断快照，不冒称旧测试。候选不可用则该格阻塞，官方包只能标原生/官方诊断。不得重发并发生产或把源码改动映射到旧SHA。

F. 复用BL正式exe `--supervise-exit` 的filter角色（不改NativeRun/ExitDiagnostics），内部30秒、外层75秒；监督把结果根规范化为绝对路径，必须亲核Arguments中 `C:\CodingAgentSandbox\Tasks\<task>\Results\cj-<module>-<arch>\TestResults\child.trx`。这是将来的路径模板，不是已存在文件。回收整个任务，完整分段读取父TRX、supervision、stdout/stderr、stages及内容/事件/实际模块JSON。

G. 门分别裁决：布局；真实producer更新；真实consumer更新；事件/必要偏序；Detach及资源释放/正常退出。要求父TRX完成、预定全部数据执行、Exit0/exited且非timeout/cancel/kill/drain timeout、SupervisorError null；实际模块路径/hash/版本、测试SHA/MVID、.NET/WPF runtime与架构匹配。任何一项缺失即该项未覆盖或失败，不能用双边零像素/零句柄或仅事件成功判绿。最小循环不证明长期资源趋势、loss或screen。

## 9. 本轮验证、产物与未闭项

本轮验证方式为逐段源级核对和报告回读；**没有编译或测试，所以没有CJ父TRX、监督进程、实际模块identity/runtime、内容或事件运行JSON**。状态JSON把这些明确设空，不伪造文件或复用历史身份。因未改代码，不进行无意义构建来替代运行。产物根相对工程为 `artifacts/cj-present-ownership/`，绝对根为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\cj-present-ownership\`。

完成：正常创建/传输引用/复制event/处理/回调/注销/flush/销毁的静态表，双架构主要结构核对，原夹具初始/mutation偏差、Commit失败转交窗口和无设备伪完成的明确证据，以及具体的最小诊断规格。

未完成：真实surface consumer读回合同及四格实跑；x86 callback最终native调用约定；发送失败/丢弃包的全所有权；首次堆损坏位置与Commit失败窗口因果；资源/线程长期收敛；真实screen交付、物理GPU、loss恢复和full。**没有生产修复；完整替换仍未完成。**
