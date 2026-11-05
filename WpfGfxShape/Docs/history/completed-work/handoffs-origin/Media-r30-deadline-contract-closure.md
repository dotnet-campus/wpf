# Media r30：八项 deadline 根因与同步软件消费闭环

## 根因及权威合同

已完整读取 r29 两轮全部 media JSON 和 TRX（截断部分另次补读）。minimal 为2通过/1失败；sample 为原版/候选诊断2通过，不是八项验收通过。相同fixture SHA为00181531947B7F4EAAD21C4935ECC1DC7C249D8B67DA33A52373FC792A26E004；两边均事件1→11→7、HasVideo=1、2×1。Position槽位6修正后两边100000000，成功采样ready=1、render/copy=0、pixels=1110EBFF1110EBFF。

权威源码：wgx_av_types.w:52–68：1 Opened、11 Prerolled、7 Ended、13 NewFrame。CompositionNotifier.cpp:118–160：仅注册CMilSlaveVideo或outstanding UI frame时发送13；没有这些条件不会因解码自动给客户端发送13。SystemVideoSample.cs:42–66使用provider→renderer→BeginComposition(syncChannel=true)→BeginRender读取真实软件sample；它不是原生通知注册。不得伪造CMilSlaveVideo指针。

失败helper SolidFrame.cs原141行每次present前硬等任意媒体通知，原145行却要求输入纯RGB逐字节相同。系统解码色彩转换实际红像素为0xFFEB1011，原expected=0xFFFF0000永不能匹配；有限生命周期通知耗尽后第二次等待满15秒，误报“No real system media notification”。不是无效AVI/codec故障，也不能把7当frame-ready。AutoResetEvent合并通知本身合法，无法用于计数帧；回调finally始终Set，不存在按7过滤。生产SystemMediaSource:82–96快照后释放gate再调用监听；GeneratedMediaPlayerResource的锁仅处理绑定和调度，不持锁调用原生renderer。

历史JSON只有事件顺序，无逐事件时间戳，不能追溯证明事件在注册/等待之前到达。代码open/rate在资源commit之前，存在合法提前送达；根因不依赖这一时序假设。未声称已证明丢通知或callback异常。新测试显式检查CallbackError。

## 修复边界

- 生产GeneratedMediaPlayerResource.AcquireSystemFrame不再把任意媒体事件_pending提升为NotifyFrameReady；仅原生BeginComposition返回newFrame才通知，候选source也适用。
- 真实公开上层helper保留15秒deadline，用20ms有界通知等待唤醒同步软件present/pixel读取，不要求原生未承诺的每帧事件。不增加deadline、不skip、不mock、不probe、不Host。
- 每种输入独立系统原版播放器按同一WriteSolidVideo生成fixture，严格要求ready=1、render/copy成功和两个非零同色像素，再将候选公开visual输出逐字节与原版解码值比较；没有放宽阈值。原版sample不注入候选，候选仍消费自己的真实sample。
- 三色、重复15次、并行9次、来源替换、锁失败0x88982F0D及释放后重试、callerRelease拥有寿命全部真实执行。

## 发布与原始证据

发布序号r30-1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，成功，60行工具日志。产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

r30-2标准包：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true`，成功，37行工具日志。只上传artifacts/com-acceptance-package；每次outputRelativePath=Results，TRX通过--results-directory Results落在回收范围内，不回收整包。

最终SHA：6FDF81D7F8CA98588DB4C5CCB3FAFB4C2904C4225B89D4FC725D7B07CD3F239D。最后代码修改先于两次publish，测试后无源码尾项。

| 证据（Investigations/下，TRX均在Results） | pass/fail/total | RunID |
|---|---|---|
| Media-r30-lock/media-lock.trx | 1/0/1 | 7ac0c8d4-2c65-4fab-b052-e6148c68371c |
| Media-r30-eight/media-eight.trx | 8/0/8 | ed9b7b45-8fd4-4108-94a4-171290d211a3 |
| Media-r30-full/media-r30-full.trx | 1374/32/1406 | 43e1e204-efde-4e41-978e-76fbda5c96de |

均零跳过；定向/full identity同SHA已回读，full Counters/退出2及八项Passed原始行已回读。新增两项r29诊断使full分母1404→1406。full其余32项HW/cache/shader/呈现失败仍保留，包含原生GPU呈现对照失败；不宣称全量Green、不全归环境。GenProto现有25/25未运行也未覆盖为旧23。

## 裁决

八项同步软件媒体deadline按真实公开像素与独立原版合同Green。此有限闭环不证明异步持续逐帧UI通知、HW媒体兼容或全项目替换资格；上述合同仍独立Open。阶段4/5 Active、6/7 Pending，UCE已Closed条款不重开。
