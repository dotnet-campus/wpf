# IN：合法同步 HWND consumer partition 状态合同

## 进行中：源码前置核验

先核官方同步连接、通知注册、HWND target 初始化、SyncFlush/SameThreadPresent、资源及清理协议，再决定最小独立 C# 实验。禁止历史同步 Event0 PresentPacket；不得删除真实 consumer 冒充合法，不预设 async 1→0 或同步广播。

官方 x64 同版 self-contained 10.0.13 真实内容和 cleanup 通过才运行候选；官方前置失败即停，不随机调参。x86 仅 x64 合同成立后决定。仅使用现有 ComAcceptance 显式 IN_SYNC_PARTITION_STATE，生产/公共捕获/打包规则不改，IH Native 不重发。

当前尚无 IN 构建、发布、运行或状态差分证据。既有 IK/IM 默认异步双架构验收保留，不能替代本同步合同。Available、HW backend、非零 sync、长期资源、loss、media、full 四格、x64 Event0 均开放，完整替换未完成。

## 阶段历史

1. 建立本报告并开始读取执行规则、完整方案、latest handoff、IK/IM、IH、IF/HB 与官方同步源码。尚未裁定协议适用性。
2. 官方源码核到 SameThreadPresent 仅 Compose，不执行 HWND Present；SyncFlush 仅处理命令并等待 flush reply。候选同步实现也明确只 Render(signalPresent:false)。不开始可见 HWND 三色实验，继续核资源合同和真实调用方。
3. 核到真实 WPF 同步调用方使用 GenericRenderTarget/RenderTargetBitmap；D3DImage 同步 UpdateResource 发送软件副本，PresentPacket 仅 async。纯色或 brush 不改变 HWND 的 render/present 分离。停止于源码协议前置，不运行历史危险变体。

## IN 终态：指定同步可见 HWND consumer 合同不适用；不是官方运行通过

### 官方合法入口与阻断点（仓库官方源码，不冒充已加载 10.0.13 实测）

以下路径均相对 `src/Microsoft.DotNet.Wpf/src/`：

|环节|源码及行号|核定合同|
|---|---|---|
|同步连接|WpfGfx/core/uce/apifunc.cpp:186–201；connection.cpp:478–492|WgxConnection_Create(true) 选 SameThread；SameThreadPresent 只允许 SameThread，委托 PresentAllPartitions。|
|同步提交|WpfGfx/core/uce/samethreadcomposition.cpp:104–128|SubmitBatch 当前线程 ProcessPartitionCommand 后 FlushChannels(false)；ScheduleCompositionPass:171 为 no-op，无异步调度补呈现。|
|同步完成|WpfGfx/core/uce/clientchannel.cpp:812–842；connection.cpp:306–350|SyncFlush 发送控制命令、CloseBatch/Commit 后等待 flush reply 并检查 zombie；不是屏幕 Present 保证。|
|消息窗口|WpfGfx/core/uce/clientchannel.cpp:694–716|SetNotificationWindow 只存 HWND/message，在反向队列空→非空通知；无同步禁用分支，设置它不会触发呈现。|
|状态订阅|WpfGfx/core/uce/composition.cpp:1241–1291|注册频道 AddRef，向全部订阅者发送当前/当前 snapshot；禁用 Remove/Release。注册不要求窗口已呈现，也不能拿 snapshot 当消费证据。|
|同步呈现入口|WpfGfx/core/uce/connectioncontext.cpp:449–525|只对 root channel 的 composition 调用 Compose(&fPresentNeeded)，不消费 fPresentNeeded 去调用 Present。注释明确结果通过 generic render target 的 IMILRenderTargetBitmap 获取。|
|Compose 链|WpfGfx/core/uce/composition.cpp:650–750、775–825、842–866；samethreadcomposition.cpp:191–214|ProcessComposition 调 Render，OnEndComposition 为 no-op；Compose 返回是否需要 Present，不自行执行。|
|HWND 真实 consumer|WpfGfx/core/uce/hwndtarget.cpp:75–151、263–289；rendertargetmanager.cpp:325–352、636–649|HWND Render 明确不 Present，只更新后台内容/invalid regions；真正 target Present 只在独立 Present pass，随后 HandlePresentErrors。纯色 clear 和有 brush 的 visual 最终均受此分离约束，换内容不能补上缺失调用。|
|真实异步入口|WpfGfx/core/uce/partitionthread.cpp:145–181|worker 将 needsPresent 调度为独立 PresentPartition→partition.Present；SameThread composition 无该 worker 路径。|
|状态拥有者|WpfGfx/core/uce/rendertargetmanager.cpp:409–420、470–474；composition.cpp:2225–2272|正常成功渲染不在 HandleRenderErrors 更新 Normal；HandlePresentErrors 才按 Present 结果调用 ProcessRenderingStatus。状态还有 display/device-release 错误等入口，不能外推同步所有状态永久不变。|
|真实同步调用方|PresentationCore/System/Windows/Media/Imaging/BitmapVisualManager.cs:99–151；Media/Renderer.cs:5–8、74–116|同步 channel 用 GenericRenderTarget/PrintInitialize 和 bitmap target；Renderer 明确不是 HwndTarget 渲染入口。这是合法离屏消费，不是本任务要求的窗口像素消费。|
|D3DImage 同步资源|PresentationCore/System/Windows/Interop/D3DImage.cs:761–817|同步 UpdateResource 调 CopyBackBuffer，传 pSoftwareBitmap 并 AddRef；Present 仅 async，不能发送旧 Event0 PresentPacket，也不能只传 interop bitmap 假定同步 surface 消费。|

目标 92 字节初始化、flags、实际 DPI/context 和 invalidate 已从现有 HB 调用段核读，但它们只配置 HWND target，不能改变上述调用链。没有为拼出合法协议虚构新的 flags/context 值，没有进入无必要真实 consumer 的 control 变体。此结论仅针对本任务指定的 WgxConnection_Create(true) + SyncFlush/SameThreadPresent + HWND 可见像素消费协议；不声称所有原生 HWND/COM API 都不支持同步呈现。

### 候选对照与生产裁决

`Code/WpfGfxShape/Abi/UceChannelExports.cs:69–109` 的 SameThreadPresent 遍历 root partitions，检查失败并 `partition.Targets.Render(signalPresent:false)`；注释也明确官方同步不进入 CSlaveHWndRenderTarget::Present。这里没有证实新的真实外部差异，不能把 IH async 成功 Present 的 NoDevice→Normal 回调直接搬到 Render 成功处。

下一唯一生产决策是“不加同步状态回调”：此限定可见 HWND 合同按源码不适用停门。若以后获得同版官方明确合法、确实进入窗口 Present 的另一个入口，必须先以该入口完成 official x64 真三色/cleanup/状态原件，再候选比较，差异成立才提最小同步修复。本轮不修改生产。

### 本轮实际计数、身份与证据限制

|范围|x64|x86|
|---|---|---|
|official 沙箱任务/角色测试|0 / 0|0 / 0|
|candidate 沙箱任务/角色测试|0 / 0|0 / 0|
|实际监督 Exit/TRX/task 根|不适用，未启动|不适用，未启动|
|IN 实际 loaded SHA/test SHA/MVID/runtimeWPF|未采集|未采集|
|三色/原始64字节消息/逐 channel 数量/cleanup refs|未采集，不填零冒充正常|未采集，不填零冒充正常|

没有 official 前置失败红帧，也没有 candidate Passed 或状态 exact 结论；仅源码前置不适用。没有创建 parentTRX、没有 full/COM/旧Event0/HWloss 变体。由于没有合法目标协议，不创建零执行的显式测试、不发布空 IN 包、不做无意义的包类型发现或 x86 实跑。

IH Native 仍按交接历史登记 x64 `EE3F1DE1BC3192766ABE5ACBC94B2548B9EEDD5107B5E7BA8BF8F465DB148189` / x86 `01CF2206C3AE4327B6057A18AFA084629B49409AC812CFBD099E22CB66105ABC`；本轮未重新读取或加载包，不能称 IN actual hash。生产未改、未重发、未移绑旧 HA。

默认隔离验证：专用正常 Release 构建现有 ComAcceptance 一次，Exit0、117 warnings、0 errors、3.48s。最后日志63行已全部回读，但工具原始 stdout 92326字符只保留末20000字符，不能声称全警告全文审计。未设置 IN/IF/HB 编译符号，未修改 NoWarn/分析器/并行/测试默认行为；源码未变，无 IN 测试混入全量。本构建不是 self-contained 10.0.13 沙箱运行证据。

### 修改登记、保留范围与最后补记

仅新增本报告、更新 next-session-handoff/session-summary/issues 三份共享顶部；生产/测试源码修改0、publish0、沙箱0、新工程/Host0、终端/脚本0。共享历史原文保留，新状态覆盖原“同步门待做”，不覆盖 IK/IM 默认异步验收。

已核执行规则全文27行、完整方案120行分段、latest handoff/IK IM终态/IH IJ终态与 IF/HB 必要源码、官方及候选以上受影响调用段；未声称全部历史原件重新全审。Available/HWbackend/非零sync/长期资源/loss/mediaresources/full四格/x64Event0仍开放，完整替换未完成。

最后补记（IN）：报告全文及三共享最新 IN 受改节须写后回读；源码前置停门不是同版官方运行合同验证成功，不将缺少运行资料写成已跑未读。
