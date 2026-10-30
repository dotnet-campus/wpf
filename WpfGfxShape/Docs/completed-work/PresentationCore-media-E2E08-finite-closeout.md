# E2E-08 媒体方法族：真实公开路径有限收口

> 历史证据：用户已撤销“合法第二wpfgfx模块”提供生产媒体的许可及排程。下文委托模式视频绿不认证独立媒体。生产委托已删除，普通媒体 E_NOTIMPL；独立后端全部 Open / NotImplemented，完整替换 Open。见[删除报告](replacement-remove-original-media-delegation.md)。

## 身份、构建与本轮范围

2026-10-15 Windows x64 / .NET WindowsDesktop 10.0.1 建设版；不是目标10.0.12验收。本轮未改生产代码、未重新publish生产AOT或COM包。现有生产候选SHA256为 `9FDFE101BB27928DB76F2F1EBD2F25C7362CE3502303EE1AC305879897CA7EC8`；原版/系统媒体服务为 `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`。PresentationCore为 `605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654`。

专用Release build成功：`Tests/WpfGfxShape.PresentationCoreE2E/WpfGfxShape.PresentationCoreE2E.csproj`，net10.0-windows/win-x64。承载DLL SHA `CBB336814450E178FCF2E69F4525ABC13EADD4B5F74F4CEA42C1CDC7E429D410`。随后修正MediaAcceptance.proj：从本轮最新original复制Baseline、最新candidate复制ComparisonCandidate，并复制本次构建承载。该项目最后修改后已专用build成功；随后original、candidate及独立compare-media全部真实执行，没有未测源码尾改。未调用终端/脚本，没有Host、mock、跳过、放宽阈值或增加deadline。

## 双模块身份与生产因果

候选进程最终任务 `a32360efaf8b409f94d5befff6662b27`：requestedNative=frozenCandidate=actualNativeModules中的候选，PresentationCore PInvoke resolver在首次WPF调用前安装，并始终绑定候选绝对路径。`systemMediaServiceModule`另记录已加载10.0.1原版系统服务身份；合法第二wpfgfx模块不触发候选Red，也不伪称原版替候选合成。

`SystemMediaModule.Acquire`按已加载PresentationCore所在runtime定位系统服务，比较文件实体防止自递归，锁内缓存，按DEC-0004进程驻留。`SystemMediaSource`持有系统player/event proxy/client，系统服务负责解码、媒体状态与事件。候选`GeneratedMediaPlayerResource.ProcessCommand`仍执行UCE引用接管、订阅替换、RegisterVideo及NotifyChanged；`AcquireSystemFrame`获取真实provider sample；`GeneratedVisualRenderer`的DrawVideo/DrawVideoAnimate分支读取sample尺寸、持有source并生成候选绘制项；公开VideoDrawing→DrawingVisual→RenderTargetBitmap→CopyPixels由候选合成。这是系统服务委托，不是将UCE/RTB委托给原版。

## 方法族与实证

| 方法族 | 本轮证据及有限结论 |
|---|---|
| factory/event proxy、QI/AddRef/Release/provider | COM full内真实候选创建、provider独立引用在caller释放后存活及最后Release=0通过；非伪造native视频指针 |
| Open/Play/Pause/Position、HasAudio/HasVideo、尺寸 | 两个4秒AVI都含PCM，视频2×1；两角色四采样均HasAudio/HasVideo=true，宽2高1；真实Position向前推进 |
| Opened/Ended/Failed | 两角色各2次Opened、2次Ended（Position=4）；缺失asset触发真实MediaFailed，HRESULT -2147024894（0x80070002） |
| UCE引用/VideoDrawing替换 | 每asset暂停后替换VideoDrawing、旧drawing.Player=null，RTB全帧保持；COM真实callback中rebind/unsubscribe、channel销毁/connection断开/Close通过 |
| 并行、重复、失败恢复 | COM真实三色视频、重复创建关闭、并行player、并发factory、source替换、caller释放后消费、目标锁失败后解锁重试全部通过；公开承载两player的Opened回调分别Close、缺asset回调Close通过 |
| Stop/Close/Shutdown/process exit | 原有导出空参边界、原/候选Close后native尺寸/长度归零及候选重复Shutdown仅提交一次通过；公开承载正常Close、清空visual、Dispatcher shutdown、finalizers完成、退出0 |
| 拥有worker/callback清理 | full内AsyncUce的关闭/失败batch FIFO释放后，按实际callback记录的native worker ID验证线程消失；另一channel回调关闭无死锁。媒体callback内Close+返还最后caller引用时callback自持引用=1且无CallbackError；真实rebind/unsubscribe完成。不是所有系统服务线程退出证明 |

公开MediaPlayer.Close后HasAudio/HasVideo两角色均仍true，属于记录到的托管缓存状态；不得当作native未关闭或资源释放证明。清理结论由独立native尺寸/长度、引用、callback完成、candidate worker退出及公开Dispatcher正常收尾共同限定；未声称系统媒体服务所有线程退出，亦未卸载DLL。

## 四帧全字节独立比较

最终公开original任务 `de9f0f5a7ae944f79c373af6fe4abe4d`；候选任务如上。证据统一在 `artifacts/sandbox-readback/e2e08-latest-{original,candidate,compare}/Results/`。原版与候选四帧均32×16 Pbgra32，独立进程CompareMedia用SequenceEqual比较每帧2048字节，合计8192字节，threshold=0，4/4 equal，Green；不是只比较8字节sample或SHA。

| 文件 | SHA256（原/候选相同） | 原版Position | 候选Position |
|---|---|---|---|
| media-0-0.pbgra32 | 56E48108ACA0E07CF0D9F531679C1036E1980631DDF3A76C0409D7AAF9F6A8A4 | 0.8325013 | 1.0230011 |
| media-0-2.pbgra32 | 8F5F79953DF7C91E4BF56AC75222912DFE5E3C57B5B5E3164A63FA83F7FF07A5 | 2.5406823 | 2.5289005 |
| media-1-0.pbgra32 | D800E6BCB44942F4D4DEF110BE0625AA4C45DEE86DCB597940CDE163FD70B6A0 | 0.5411932 | 0.5407974 |
| media-1-2.pbgra32 | BD6FD4A7D9D5B84E82D5681BC174193A170A208B368BF794B1E86CFF6B4056A7 | 2.5293266 | 2.5293601 |

请求采样位置0.5/2.5秒，实际值如实记录，不伪称两角色在同一时刻；受控资产同色平台上的四帧全字节相同。asset0 SHA `01BE17E50794DD8A076967E49D0C2CD6265A3908247748B4B95689AB2F07C872`，asset1 `07FBF8F7BFBF80BC2832E714BBB374CAB8CB7E57F91D10AAAAA141E177DEF802`。输入与承载身份清单见 `artifacts/staging/PresentationCoreE2E/media-input-identities.txt`。最新两角色JSON的事件、像素、模块身份及最终comparison.json已当场回读。

## COM full、普通与GenProto

只上传 `artifacts/com-acceptance-package`，标准MTP exe参数为 `--report-trx --report-trx-filename e2e08-full.trx --results-directory Results`；仅回收同任务Results，含TRX和aot-identity，不回收整包。full本轮仅一次：**1446 pass /33 fail /1479，0 skip，退出2**，RunID `bb4c76c2-4751-4101-a56d-8557d93425b3`；路径 `artifacts/sandbox-readback/e2e08-latest-full/Results/`。identity同上述生产SHA；TRX Counters和exit2已回读。真实callback rebind测试为Passed，不是只运行了旧承载。

33失败保持原有HW/cache/shader/原版与native control可观测前提边界；未把背景像素或duplication timeout全归环境，未当作媒体差异。无生产/普通/GenProto代码或依赖变化，故不无谓重发生产、重跑普通/GenProto：普通真实历史4224/1/4225保留8007007E；GenProto历史34/34，RunID `778433c4-c65a-4025-b026-e896facfb993`，不冒称本轮新跑。同SHA strict最终报告仍缺，不拿旧报告代填。

## 两类player与正式剩余

`MILFactoryCreateMediaPlayer`及MILMedia*已经实际候选执行；不能继续将其历史缺export状态当作当前Red。PRERELEASE的record-packet player两缺export属独立协议/dispatch/ownership问题，不能拿媒体对象替代；官方MIL_REC_PACKET_HEADER/UCE_RDP_HEADER、destroy合同及匹配PRERELEASE oracle仍依唯一外部清单，不能混称媒体尚缺两export。

**有限Closed**：本10.0.1/x64受控两PCM AVI、四帧零差、引用替换、真实并行Opened关闭、缺asset失败、上述候选拥有链及有限worker/callback收尾。**正式E2E-08 Open**：HW媒体消费、长时间连续播放/压力、多Dispatcher和全架构/配置/能力矩阵、完整状态/事件线程与失败取消合同；不以两次4秒播放外推持续稳定性，不以audio flag证明扬声器输出质量。正式E2E-09及七阶段完整寿命也不由本轮关闭。

四外部input（authority binding、ARM64工具链与真实环境、完整10.0.12、record packet player官方协议）仍需用户补齐真正缺项。DLL进程驻留不卸载但资源清理必需。总门**Red**，没有全任务完成或替换资格。父级继续委派下一自主完整生产模块，不重领本有限媒体范围，不将纯复验作为默认下一模块。
