# Z：独立资源/关闭与真实恢复调查

## 执行前登记

仅新增 ReplacementResourceCycle* / ReplacementRecovery* 测试和本报告；不修改项目、共享入口、生产、窗口通知、捕获、ABI、golden、skip 或阈值。生产不重发。候选选择 V 归档 `artifacts/replacement-window-v-2026/production/{x64,x86}/wpfgfx_cor3.dll`，不绑定最后生产源码 full。官方使用正式自包含包实际加载模块，运行后登记版本及 SHA。

选择真实离屏 `MILCreateFactory → BitmapRenderTarget(usage1, format16) → GetBitmap → Clear/CopyPixels → Release(target) → CopyPixels(bitmap) → Release(bitmap/factory)` 闭环。预热4轮、观测12轮，每轮红→蓝更新，独立颜色断言和官方零阈值比较。消费侧持有真实 COM 引用，关闭 producer 后再次读回验证剩余 owner 有效。并列真实 EventProxy 回调/最终 descriptor dispose，保存每次事件和进程资源样本。内存仅报告序列，不自设通过阈值；最终 COM 返回值与 descriptor dispose 是有界对象释放证据，不声称全局 GPU/缓存收敛。

遗漏/重复释放负控接入同一生命周期检查器，禁止在已释放原生指针上进行危险调用。另调查现有真实独占设备 displacement 探针；只有首设备 lost 和恢复后新帧均有证据才验证恢复。没有首设备 loss 时保留失败，不以 secondCreate 失败冒充。

状态：本节先于运行创建；最终结果见下文。预登记循环数未因结果调整。

## 最终交付与实跑

新增文件仅：
- `Tests/WpfGfxShape.ComAcceptance/ReplacementResourceCycleTests.cs`
- `Tests/WpfGfxShape.ComAcceptance/ReplacementRecoverySessionTests.cs`
- 本报告。

没有修改或重发生产，没有共享变更请求；没有终端、shell、直接/间接脚本、原生探针、外部重置或系统进程修改。只使用专用发布和 Windows 沙箱执行正式 C# 测试程序。两个现有测试项目边界不变，本包只改/运行 ComAcceptance；WpfGfxShape.Tests 无改动且未运行，不能据此宣称四格 full。

最后源码修改为把本批次 stackalloc 移出循环，消除新增 CA2014。此后重新发布 x64/x86 Release，自包含 .NET10.0.13、ComAcceptanceSandbox=true、各架构 AotDllPath 指向 V 归档，PublishDir/ComAcceptancePackageDirectory 均为本线独有 final-publish/final-package。两次发布均成功，分析器未关闭；既有其他文件警告保留，最终日志没有本批次两个文件警告。早期 x64 3项实跑和先前发布目录保留，不拼入最终数字。

最终筛选新增两类 + 既有 `WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover` + `ReplacementConstrainedSequenceTests` + `ReplacementDetectionControlTests`。通过专用沙箱运行 `WpfGfxShape.ComAcceptance.exe`，TRX 实际回读：

|正式配置|执行|通过|失败|timeout/aborted/notExecuted|结果|
|---|---:|---:|---:|---|---|
|Windows x64 Release|12|11|1|均0|退出码2，保留真实loss失败|
|Windows x86 Release|12|11|1|均0|退出码2，保留真实loss失败|

新增资源闭环与会话采样各1通过，既有随机/检测回归9通过；唯一失败为真实loss触发断言。不是full，不绑定最后生产源码，不将V旧归档局部Green外推到X/Y或其他批次。

## 可追溯证据与实际身份

统一根 `artifacts/replacement-resource-z-2026/`，所有运行数据在ignored artifacts。
- 最终 x64：`final-readback/x64/TestResults/replacement-z-final-x64.trx`，Counters第143行；任务 `d962879494ec49e69435597948a00d4f`。
- 最终 x86：`final-readback/x86/TestResults/replacement-z-final-x86.trx`，Counters第141行；任务 `a44f73e3bc0548d1aace6d94b3b23152`。
- x64资源根：`final-readback/x64/Results/replacement-resource-z/9955137b6c4c4fcda8e87520a5722a9c/`。
- x86资源根：`final-readback/x86/Results/replacement-resource-z/b0c89db746ce4402bb6f785cb2b1c80f/`。
- x64会话：`final-readback/x64/Results/replacement-recovery-z/bf2037f11a9b493295dd1ed185c52927/session.json`。
- x86会话：`final-readback/x86/Results/replacement-recovery-z/812eef9e36bf47a6ba18b77ee3662452/session.json`。

资源根 `identity.json` 用实际 NativeLibrary handle/GetModuleFileName 获取模块，不只记录预期路径。官方实际包根 wpfgfx_cor3.dll，候选实际 Native/wpfgfx_cor3.dll，两独立模块，官方不转发候选。实际 .NET10.0.13，PresentationCore10.0.0.0；官方 FileVersion `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。不外推其他 servicing。

|架构/指针宽度|实际候选SHA256（V归档）|实际官方SHA256|
|---|---|---|
|x64/8|7B491B79976CB1E1C8C24792E067CFD8948F0D47F6C174933C6B99F230D331E3|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|x86/4|508C123706484CF830C2448BEE34F3624AF3A74E02B9E8920A766AA296638A6E|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|

## 真实资源闭环与负控

每角色16轮（4预热+12观测），每轮8×8红帧、更新蓝帧、producer最终Release后consumer蓝帧，共48帧/角色/架构，双架构192份原始BGRA。每轮真实建立factory/target/bitmap/proxy；两次同步Raise把实际CopyPixels字节交给真实代理回调，回调累计sum65280。不是模拟COM对象，callback不是WPF呈现通知，不宣称异步关闭竞争。

官方与候选所有轮 target/bitmap/factory/proxy 的最终 Release 返回0，proxy descriptor Dispose恰好1次、Raise恰好2次；producer关闭后独立bitmap owner仍可真实读回256字节蓝帧。返回0依照COM合同表明这些接口对象没有剩余引用，descriptor回调为额外真实析构见证；不证明进程所有缓存/分配器/内部GPU资源释放。

每角色保存：
- `cycle-00-frame-0.bgra` 至 `cycle-15-frame-2.bgra`，紧密BGRA、stride32、无头；
- `cycles.json` 保存逐轮原始base64帧、所有Release返回值、Calls/Disposals/Sum；
- `events.json` 每个节点立即写盘（每轮14事件，共224事件），含创建/更新/消费/producer关闭/关闭后消费/最终释放/descriptor处置；
- `resources.json` 全16点 private/working-set/handles/threads/managed bytes。

两份comparison已直接回读 Failure=null、Threshold0、Cycles16、FramesPerRole48，两个负控均true。独立oracle逐像素断言红→蓝→蓝，防止官方/候选共同空白或旧帧；零阈值逐字节比较实际帧。回读cycles首尾原始base64以及events首尾，样本符合两次回调/最终释放0/处置1。原始二进制未通过文本读取接口做有损解码审计，逐字节验证由正式测试完成。

负控基于实际观测副本，将测量轮4的Disposals变成0或2，交给与正式运行相同Detect，均拒绝。仅证明该检查器能发现遗漏/重复处置信号，不是生产注入真实double-free；禁止对已释放指针再调用。Release返回非零也由同检查器直接拒绝。

## 缓存、增长及证据边界

已回读四份resources的完整12个观测点，不只比较最后值：

|架构/角色|private bytes，轮4→15（中间特征）|handles/threads（全部测量点）|managed bytes，轮4→15|
|---|---|---|---|
|x64官方|48,807,936→40,898,560；轮7下降后小台阶|368/18稳定|12,372,816→5,494,336；轮7 GC下降|
|x64候选|40,898,560→41,730,048；轮9/13/15台阶|368/18稳定|6,084,560→2,466,944；轮15下降|
|x86官方|43,307,008→33,734,656；轮15下降|388/18稳定|11,334,424→2,998,800；轮15下降|
|x86候选|33,734,656→33,976,320；轮12升、轮13略降|388/18稳定|3,563,880→7,719,804；本段持续增长|

这些是同进程先官方后候选的观察，受到先前测试、JIT、GC、日志序列化及保留帧/事件列表影响，不能精确归因到一个模块。尤其x86候选托管值上涨不能直接判生产泄漏，亦不能因COM最终0就宣布所有缓存合理。当前证据能闭合有限显式对象所有权及稳定句柄/线程，不足以裁决长期原生/托管堆、GPU缓存持续增长。没有随意设置内存容差或以阈值掩盖问题；扩大长期/堆分配归因仍是后续缺口。

## 真实 device-loss 调查结论

现有纯C#路径在 `D3DImageTests.HardwareClosure.cs:47-104`：真实D3D9非Ex HAL独占首设备→同适配器第二独占设备→首设备TestCooperativeLevel要求DEVICELOST→释放第二设备→要求DEVICENOTRESET→原线程Reset→要求S_OK。没有把托管状态位改成lost。既有其他device-state/generation测试不能替代真实触发。

两个最终TRX原始stdout均明确：
- firstCreate=0x00000000，mode1920×1080，format22；
- secondCreate=0x88760868；
- **firstCooperative=0x00000000**。

故第二设备创建失败并没有证明首设备loss；本轮在loss断言停止，未进入Reset，也没有恢复新帧。失败原样保留，不添加skip、不改断言、不以软件fallback替代HW。该探针直接调用系统D3D9，失败并非已证明候选恢复缺陷。

会话JSON两架构均SessionId1、UserInteractive=true、RemoteSession0、RemoteControl0、InputDesktopOpened/Closed=true。可访问交互桌面并不足以保证独占接管成功；现有证据不能断言远程/非交互是原因，也未获取完整adapter/驱动或判定sandbox虚拟显示能力。现有探针即使成功也仅验证lost/reset/cooperative，没有恢复后颜色消费链，故不能单独满足完整恢复合同。

所需条件与下一最小验证：具备可实际使首设备TestCooperativeLevel进入DEVICELOST、随后DEVICENOTRESET的适配器/驱动/独占接管条件；在同创建设备线程恢复，保存lost→not-reset→Reset→更新新颜色→真实HW内容读回及官方/候选身份和资源释放。若需外部人工显示/会话切换触发，应先提供合法可重复条件，不在本批次编写重置脚本、原生探针或修改系统进程。当前沙箱没有完成这条触发链，因此真实恢复仍未覆盖。

## 最终裁决

已交付可实跑的纯C#有界资源闭环，双架构官方零阈值及检测回归有实际证据，未发现本闭环候选差异。无生产修复。真实device-loss恢复未验证，长期内存/GPU/媒体及异步在途关闭不在本包闭合范围。最后源码两个项目×双架构full仍由集成线完成；本包不能宣布完整替换。

