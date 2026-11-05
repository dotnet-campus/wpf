# AB：真实异步 Present 已触发，双架构通知布局差异最小复现（合同未闭）

## AB 已知阻塞登记及最终裁决

执行前已登记：Y 的 SameThreadPresent 不能证明真实异步 Present。现已完成有界替换和实跑：**官方 x64/x86 均实际触发 Present，返回两条非空 0x0A；候选两架构均返回两条，但字段比官方提前4字节，零阈值比较失败。** 不再是双方空消息的触发阻塞；这是归档候选的可重复通知布局缺陷。本包不抢修生产，不扩展 FrameTime 变化。

仅修改 `PresentNotificationContractYTests.cs` 与本报告。未改生产、入口、项目、其他代理文件、skip/golden/阈值；未重发 NativeAOT，未清理他人产物。所有构建/执行使用专用工具及 Windows 沙箱，未执行终端、shell、脚本或模拟回包。

## AB 最小真实调用链

复用现成 `PublicAbi/PerfAsyncPresentContract.cs` 的 CrossThread HWND 通道机制：Initialize → WgxConnection_Create(0) → CreateChannel → GetMarshalType=2 → SetNotificationWindow(0x8123) → SetMilPerfInstrumentationFlags(2) → 类型46真实 STATIC HWND target、92字节0x31（96 DPI）、72字节0x33、类型39 visual和0x35绑定 → 两份 packed12 `060000007B00000000000000` → CloseBatch → Commit → SyncFlush → 有界1500ms泵消息并读取64字节原始通知。

先官方独立运行，必须存在官方正确布局的非 NoPresent 0x0A才运行候选；两角色 compose 阶段均要求至少一条实际 WM_USER(0x400,123,456)呈现仪表消息。不是 SameThreadPresent，也不是纯模拟通知。官方 x64/x86 实测呈现仪表数量分别2/1，候选各1，不把这个调度数量当严格等价合同。

随后空 flush 并观察300ms，两边没有重复0x0A；注销通知窗口（SetNotificationWindow(channel,0,0)），提交在途0x06并销毁通道，创建替代通道、flush再观察300ms，其队列均空，最终 destroy/disconnect/deinitialize。events 保存注册、提交、flush、实际WM消息、注销、在途销毁及最终释放，HRESULT均0。注销窗口不是注销0x06一次性监听者，替代通道空也不证明已销毁通道不存在迟到消息；不夸大生命周期覆盖。

本场景两份请求 FrameTime 均123；正式独有测试保留一个 DataRow。官方成功触发但候选合同已失败，故不增加变化场景。不把本次观察到的结果2、refresh60、time123外推为一般算法或版本合同。

## AB 原始差异（双架构相同）

|64字节消息偏移|官方实测 DWORD|候选实测 DWORD|
|---|---|---|
|0|0x0A|0x0A|
|4|0（Reserved）|2|
|8|2（PresentationResults）|60|
|12|60（RefreshRate）|123|
|16|123（PresentationTime低32位）|0|

每角色两条完全相同通知。官方前24字节 `0A00000000000000020000003C0000007B00000000000000`；候选前24字节 `0A000000020000003C0000007B0000000000000000000000`。其余原始字节保存于各角色 compose-0.bin、compose-1.bin及samples/comparison JSON。已回读两架构完整原始hex和0x06发送包，而不是只看断言摘要。

独立声明依据：官方 `include/exports.cs:338–343` 的 Type@0、Reserved@4、Presented@8，Presented字段相对偏移0/4/8。官方 `core/uce/composition.cpp:2401–2421` 构造消息并逐监听者发送、一次性清空。工作区 `Core/SameThreadChannel.cs:65–69` 当前写结果@4、刷新@8、时间@12，与归档实际包一致；仅作为定位线索，不宣称并行最新源码等同归档构建。最小待修是消息布局，交生产所有者；未改该文件。

比较保留原始包，仅对官方合同 PresentationTime@16的8字节绝对QPC不作跨运行相等断言，其他字节零阈值。该排除不会掩盖本次@4/@8/@12差异。没有比较墙钟全序、VBlank或仪表消息次数。尚未增加QPC必要偏序断言。

## AB 最后源码构建和实际运行

根：`WpfGfxShape/artifacts/replacement-present-ab-2026/`（以下相对此根）。两个正式测试项目均 Release、win-x64/win-x86、self-contained 发布成功。COM最终包为 `final-package/{x64,x86}`，Managed为 `managed/{x64,x86}`；Managed实际构建生产项目依赖，不发布替换Native。最终源码之后无测试代码修改。

|最终定向实跑|执行/通过/失败|实际TRX|
|---|---|---|
|COM x64|1/0/1|readback/final-x64/TestResults/present-ab-x64.trx|
|COM x86|1/0/1|readback/final-x86/TestResults/present-ab-x86.trx|
|Managed x64|5/5/0|readback/managed-x64/TestResults/present-ab-managed-x64.trx|
|Managed x86|5/5/0|readback/managed-x86/TestResults/present-ab-managed-x86.trx|

四份TRX已直接回读：COM Counters第36行、Managed第64行；error/timeout/aborted/notExecuted均0，COM退出2，失败在CollectionAssert而非触发门槛。Managed各5/5仍不验证官方消息布局，不抵消COM失败。不是四格full验收。

早期 x64 `readback/x64` 只打印不支持过滤参数帮助，不计测试；`readback/x64-run` 是加最终实际Present门槛前的探索复现，最终数字只取final-x64/final-x86。原始产物保留，没有删除。

## AB 身份与证据定位

已直接回读四份现场 identity，Loaded取 Process.Modules实际模块并现场算SHA；同架构官方/候选同 .NET10.0.13进程先后运行，不是隔离角色进程。官方FileVersion `10,0,1326,50504`、commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`；指针宽度8/4。

|架构|官方SHA256|候选SHA256|
|---|---|---|
|x64|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|7B491B79976CB1E1C8C24792E067CFD8948F0D47F6C174933C6B99F230D331E3|
|x86|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|508C123706484CF830C2448BEE34F3624AF3A74E02B9E8920A766AA296638A6E|

候选明确来自既有 `artifacts/replacement-window-v-2026/production/{x64,x86}/wpfgfx_cor3.dll`，现场实际Loaded为沙箱任务 `Native/wpfgfx_cor3.dll`；不是AA并行生产或最新源码重发。官方实际Loaded为self-contained正式包根的 `wpfgfx_cor3.dll`。

完整业务目录（每个含comparison和official/candidate的identity、packets、events、samples及实际通知bin）：

- `readback/final-x64/Results/present-contract-y/2ec6158048434b25a7d02be9bbb7cefd/`
- `readback/final-x86/Results/present-contract-y/2fa5958605824c68be2dc8631ba74b0a/`

已回读两架构comparison、四identity、官方双架构和候选x86最终events、x64官方完整发送包、四最终TRX。原始bin是实际peek返回，不是预制包。测试没有像素consumer、GPU adapter/backend确认，不称HW/VSync/真实屏幕内容验收；但真实异步通道提交和Present仪表路径已实触发。

## 历史 Y 同步轮次（以下非当前结论）

### 原裁决

**本批次未建立有 target 的官方通知合同；不可报成功。** 新增纯 C# 测试实际运行于 Windows x64/x86 Release，两架构官方与候选同步通道均没有产生呈现通知。测试明确因官方触发未覆盖失败，未修改 skip/golden/阈值，未让双方空队列相等变成通过。

未改生产实现，未重发 NativeAOT。没有足够官方行为证据允许修改当前 VSyncUnsupported/60/minFrameTime 算法。没有触碰 GeneratedVisualTargetResources、GPU 渲染、捕获、ABI、随机、项目、入口或主交接；没有清理共享目录。

## 修改范围

- `Tests/WpfGfxShape.ComAcceptance/PresentNotificationContractYTests.cs`：独有测试，独立声明原始导出函数指针；创建真实 STATIC HWND 和类型46 target，发送92字节0x31与packed12字节0x06。两组 FrameTime 为 `[0,123,123]`、`[987654321,123456789,123456789]`，另有销毁前在途请求。
- 本报告。

测试分别记录 close前、commit后/调用前、SameThreadPresent后、第二次SameThreadPresent后、销毁后新通道队列。销毁在途通道后再次调用呈现、新建通道并读空队列，最后销毁/断连/反初始化。无注销 callback 注册或注销用例，不能声称已验证该项。

官方与候选分别直接加载真实模块，不转发、不引用候选消息类型生成预期；本版依次在同一测试进程执行，不是隔离角色进程。原始 HWND 数值各次不同，仅保存而不对包地址作相等断言。只比较阶段内原始消息序列，不比较两次运行的墙钟时间。

## 实跑与构建

使用专用发布工具编译两个正式项目，Release、win-x64/win-x86、self-contained；COM设置ComAcceptanceSandbox及独有PackageDirectory，复用V归档Native。托管发布EnableMSTestRunner=true并构建实际项目依赖。所有执行通过Windows沙箱直接运行编译后的现有测试exe；无终端、shell或脚本。

根：`artifacts/replacement-present-y-2026/`。测试源码修正局部函数捕获取地址编译错误后，四份最终发布均成功；没有之后的源码更改。

|当前定向回归|实际执行/通过/失败|TRX（相对根readback）|
|---|---|---|
|COM x64 Y官方触发|2/0/2|`x64-run/TestResults/present-y-x64.trx`|
|COM x86 Y官方触发|2/0/2|`x86-run/TestResults/present-y-x86.trx`|
|Managed x64现有通知测试|5/5/0|`managed-x64/TestResults/present-y-managed-x64.trx`|
|Managed x86现有通知测试|5/5/0|`managed-x86/TestResults/present-y-managed-x86.trx`|

已回读四份TRX Counters：COM第51行、Managed第64行；error/timeout/aborted/notExecuted均0。COM ExitCode=2。没有把无timeout等同成功。第一次x64沙箱执行用了不支持的过滤选项，只打印帮助，保留在`readback/x64`，不计测试；真正执行为`x64-run`。

**不是两项目双架构full。** 此次只有通知定向回归；没有最终全量回归证据，不把旧full拼接到当前源码。

## 当前Native身份与源码绑定边界

已回读现场Process.Modules实际Loaded路径及SHA，不以请求路径代替实际加载：

|架构|官方SHA256|候选Native SHA256|
|---|---|---|
|x64|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|7B491B79976CB1E1C8C24792E067CFD8948F0D47F6C174933C6B99F230D331E3|
|x86|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|508C123706484CF830C2448BEE34F3624AF3A74E02B9E8920A766AA296638A6E|

官方/.NET10.0.13，官方FileVersion10,0,1326,50504，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。指针宽度分别8/4。候选源为`artifacts/replacement-window-v-2026/production/{x64,x86}/wpfgfx_cor3.dll`，现场实际加载任务包`Native/wpfgfx_cor3.dll`，SHA与V记录一致。它是既有归档，不宣称与并行修改中的整个最终工作区逐文件一致。本批次生产零修改，故没有新的生产SHA。

托管测试使用当前源码实际依赖构建；本轮未新增托管identity记录，不将其与Native SHA混同。

## 原始证据回读

官方/候选各自`identity.json`、`packets.json`、`events.json`、`samples.json`，以及同场景`comparison.json`位于：

- x64 `[0,123,123]`：`readback/x64-run/Results/present-contract-y/e36c115bd65f43a3b8bee1a3dc4e08aa/`。
- x64第二组：同根`23e3d27fa9a34926bd993a0b8dd8088e/`。
- x86第一组：`readback/x86-run/Results/present-contract-y/471bb60e8bff4d309f089086cc9e332c/`。
- x86第二组：同根`a9f08ca7984641e79372316a550646c9/`。

已直接回读第一组x64原始包：`060000000000000000000000`、两份`060000007B00000000000000`及销毁前请求，target包完整保存。第一组x64官方事件全部HRESULT0，包含commit、两次呈现、destroy-pending、呈现后新通道与最终销毁/断连。comparison中双方全部五阶段Messages为空。此时没有任何通知字节可保存为bin；测试仅当实际取到消息时保存64字节bin，不伪造原始消息。身份两架构官方/候选四份已直接回读。

现场Backend字段明确：`UCE synchronous channel + real STATIC HWND; renderer/backend not directly observed; no HW claim`。未记录真实GPU adapter，也没有真实像素消费证据；不能称HW呈现验证。

## 触发路径定位与未闭项

此次失败根因首先是测试触发路径不对，不是已证实生产字段差异：

- `Abi/UceChannelExports.cs:69–98`的WgxConnection_SameThreadPresent只调用`Targets.Render(signalPresent: false)`，不调用`SameThreadPartition.Compose()`，所以无法触达本批次关注的合成后通知状态。
- 官方源码`core/uce/composition.cpp:1684–1694`逐请求Add监听者，`2380–2421`构造一次性通知并清空；`750–758`仅在不需要Present时发送NoPresent；真正呈现后的Notify在1033行。源码用于定位触发条件，不替代安装官方模块行为证明。
- 有真实target并不等于执行Present。此前PerfPresentContract中的同线程只Compose、不进入HWND Present注释也与此边界一致。

本轮尚未实现异步通道+实际Present的有界官方触发；也没有在有target场景中独立证明恰好一次、不同FrameTime语义、VSyncUnsupported、refresh字段、销毁/注销callback语义或必要VBlank偏序。未因双方空结果一致放宽门槛；新增测试仍为失败状态以暴露缺口。

后续最小工作应在现有正式项目独有文件内，用异步通道真实调度，记录官方0x0A原始消息和有界队列完成条件；先复核官方重复请求/FrameTime与退出订阅行为，再确定候选最小根因修复。若涉及共享入口或GPU渲染，先协调所有权。不得直接把同步触发的空队列当作“应删除通知”的官方合同。

## 最终状态

有界实现、双架构构建实跑与证据落盘完成；Y目标合同未完成。生产未改，不重发；正式替换不能关闭。保留四份最终TRX和独有ignored产物，不清理共享。报告完成不代表测试通过、HW触发成功或全量验收完成。
