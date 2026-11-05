# Stage8 LimitedDurationControlsValidated / CloseoutCompleted

## 预注册 / Before（尚未实现、未验证）
亲自执行，不委派、不用终端或脚本；程序仅 Windows 沙箱。授权 normal≤3 / directed tests≤2 / COMpublish≤2 / sandbox≤2 / Native0，当前全部0。历史账不冲销。
Before：IndependentMediaIdentity slot20 Length 为 E_NOTIMPL/0；SourceReader 仅 enableCompressedVideo 的诊断 ReadSourceMetadata 查询 duration 并存字符串，无生产 duration；owner 无 duration。slot13 CanPause 未实现。factory 保持 E_NOTIMPL/null，禁止原 wpfgfxloader/fallback。

## 权威合同
本地 src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_render.h IMILMedia 顺序（IUnknown 3）确认 slot20 STDMETHOD(GetMediaLength)(LONGLONG*)；WmpPlayer.cpp 469–486 返回 S_OK、单位100ns。Microsoft MF_PD_DURATION 文档：UINT64 存储、按 LONGLONG 处理、不得负数；IMFSourceReader::GetPresentationAttribute(MF_SOURCE_READER_MEDIASOURCE=0xffffffff, MF_PD_DURATION) 可取 presentation descriptor 属性，VT_UI8=21。无需另创建 presentation descriptor。超过 long.MaxValue 是非法 metadata，不截断，不参与乘除故无 UInt128 转换需求。
https://learn.microsoft.com/en-us/windows/win32/medfound/mf-pd-duration-attribute
https://learn.microsoft.com/en-us/windows/win32/medfound/processing-media-data-with-the-source-reader
https://learn.microsoft.com/en-us/windows/win32/api/mfreadwrite/nf-mfreadwrite-imfsourcereader-setcurrentposition
MFMEDIASOURCE_CAN_PAUSE=4；只有真实 characteristics 且成功打开才报告，未知不固定true。尚须核对 characteristics GUID 与查询合同后实现。

## Owner / 状态与边界预注册
reader 在唯一 MTA Open 中查询、清理 PROPVARIANT；成功非负 VT_UI8 存 long?，属性不存在 MF_E_ATTRIBUTENOTFOUND=0xc00d36e6 保持 null 且保存原HR，允许未知长度源打开；其余属性失败/非法类型/越界使 Open 真实失败，不吞错。owner 保存不可变 metadata 值，identity 不跨线程调用 reader。
Opened/Playing/Paused/Stopped/Seeking/Ended：已知 Length S_OK/原100ns不随暂停seek改变；未知返回属性原HR/0，不冒 known0。未Open/Opening/Failed：Length 非成功且输出0；Closing/已Close：RO_E_CLOSED=0x80000013/0。null outpointer 优先 E_POINTER，已失效self 不解引用。
seek 负数 E_INVALIDARG，无 Clamp；已知时长的超尾策略不先猜测：须核实原控制语义，再登记实施。未知不按0拒绝。所有 Flush/SetCurrentPosition 仍 MTA串行，成功后 epoch 隔离旧pending，失败不冒成功。Ended的Length保留，Position须到 duration（已知时）且 replay 须显式 seek0/重启；最终具体策略须在代码前补齐。
CanPause 按真实源能力及可用状态；未source/未知false，不宣称audio。progress/HasAudio不改假1。

## 代码前补充裁定
MF Start 官方列明超出presentation可返回 MF_E_INVALIDREQUEST；本slice对已知 timestamp>duration 同步返回0xc00d36b2，不Clamp、不发命令、不改epoch/state；等于duration允许交给reader并消费EOS。未知交给原reader，保留精确失败。Ended Position查询返回已知duration，不改历史LastTimestamp（最后实际发布PTS）；Ended后 rate1显式MTA seek0再SetPlaying，重新消费新epoch。未open/Opening/Failed Length用0x8007139F/0；unknown Length用原MF_E_ATTRIBUTENOTFOUND/0。CanPause slot13依据实际characteristics VT_UI4及CAN_PAUSE=4，成功打开的6状态有效；缺属性记录原HR、false，其他错误使Open失败。GUID6d23f5c8-c5d7-4a9b-9971-5d11f8bca880已按公开header索引核对；官方属性合同 https://learn.microsoft.com/en-us/windows/win32/medfound/mf-source-reader-mediasource-characteristics 。

## 验证预注册
纯unit：真实 COM slot20 函数指针、空输出、未知源、关闭、状态表、负数及 long 边界；保留原33 controls/provider 回归，不改 expected/skip/public/visibility/工程依赖。
系统第一场景固定4秒AVI：真实metadata=40000000，全部支持状态length不变，Pause/seek/productionDrawVideo红蓝/EOS/事件/close/ref0/registryRemoved；第二场景仅第一全过后有界 Video.mp4，使用正式Content和源SHA，实际duration而非固定743.826；不强制40帧/颜色、不声称AAC输出。可只执行第一场景。系统身份、原HR、metadata/PTS/epoch、模块、TRX/监督/清理必须回读，Unknown不填0。

## 回撤与范围
若验收失败保留原件和精确原因，不启用默认factory；只撤回本slice的具体改动，不还原历史修正、不恢复原版服务。音频 endpoint render/capture四状态count0、服务Running/高权限session1结论保留，不重试设备或服务；WAV Passed、AVI音频缺16frames Failed、旧full红/资源Unknown/memoryGreen=null、AVsync/强cancel/公开factory/NativeAOT双arch/OverallOpen仍Open。

## 最终结果：LimitedDurationControlsValidated / CloseoutCompleted
本次仅亲自读写已有证据和文档，主代理监督；新源码/build/tests/publish/sandbox/NativeAOT运行全部0，终端/脚本/再委派0。以上Before、预算和预注册为历史，不是当前待执行项。MP4不启动；发布2/2耗尽，不凑用剩余沙箱。

### 原件、阅读归属与来源限制
准确回收根：`WpfGfxShape/artifacts/independent-media-stage8-r1/readback`；远端task4491db439c904e8eb66568a9d3126dd6，非其他task。以下路径以回收根为基准。
- 前代理全文阅读记录（本次交接引用，不能冒称本次全文亲读）：`Results/independent-media-stage8-supervision/TestResults/child.trx` 26行、`supervision.json` 40行、`Results/independent-media-stage6/result.json` 469行、`Results/independent-media-stage8/duration-replay.json` 95行。
- 本次亲读：stage8草稿31行及执行receipt10行；supervision 1–40、duration-replay 1–95；controls 1–65、60–143、340–400、420–469；TRX 19–26。诊断`Results/independent-media-stage8-supervision/Diagnostic/log_261030141118332.diag` 1–35和48–85的工具回显有4000字符截断，不冒称全文；另单独亲读63–65完整尾，明确TRX产物路径及session退出0（3.2465319秒）。即时账追加见receipt。
- sandbox工具原return未有可定位的逐字原始存档：sandbox1及成功回收据用户/前代理交接引用；本地监督、TRX、业务、identity原件交叉支持task和child0。不得将此描述为原return全文已读，不补造工具stdout或外层exit。

### 真实结果与边界
纯unit 56passed/0failed/0skipped，implicit Debug1独列；未选系统测试，原工具stdout未给unit TRX路径。真实AVI系统1executed/1passed/0failed/0notExecuted，child exit0；这是source-linked生产identity/owner/DrawVideo有限证据，非NativeAOT或完整player通过。
- metadata duration HRESULT0、VT_UI8(21)、40000000（100ns）；characteristics HRESULT0、VT_UI4(19)、134。GetMediaLength HRESULT0/40000000，CanPause HRESULT0/value1；Opened/Playing/Paused/Stopped/seek完成Paused/Ended实测不变，不把瞬时Seeking状态表unit冒称现场逐态观测。
- PauseBefore/After position5000000、count6不变；wallTicks6162831→11458359，暂停528.4796ms，原下限515ms不变；HasVideo1/NaturalSize2×1。Stop/Seek25000000 HRESULT0，epoch2后仅新蓝区帧；production DrawVideo软件RTB红蓝与Opened/NewFrame/Ended内部通知保留，非公开MediaEventProxy packet ABI闭环。
- Ended length/position40000000，最后实际PTS39000000；超尾HRESULT0xC00D36B2，Ended/epoch1不变，不Clamp。Replay HRESULT0，epoch1→2，PTS0→1000000（wallTicks13234→1150391），重锚后推进；ReplayPause HRESULT0。
- Close HRESULT0，Closed/Failure0/COM finalReferences0/registryRemoved=true；controls descriptorDisposed=true。关闭后Length/CanPause HRESULT0x80000013，输出0/false。全native refs仍`Unknown; individual backend counters not exported`，不把identity ref0推广到所有原生资源。

### 身份（各层不可互换）
.NET10.0.13/X64；监督PID12040、child10068，Reason=exited、TimedOut/Cancelled/KillAttempted=false、OutputCollected=true、drain两超时false、SupervisorError=null。
- wrapper COMAcceptance SHA256 `D446EDCA322656E0BEEA250EC632672F4FA699FA18BD4FF96A165F0D0DE09A28`，MVID `106a3b6f-9d15-41e7-8b5e-678e5aa5661e`。
- source-linked ManagedAcceptance SHA256 `10CB9C6C575BC15A4ED1F95BB1E3F32030FD49449970441461F2352EAB62F0F7`，MVID `9ea0f3fa-139b-49a4-b267-80c1d2938dc7`；duration源AVI SHA256 `01BE17E50794DD8A076967E49D0C2CD6265A3908247748B4B95689AB2F07C872`。
- 旧Native夹具SHA256 `6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403`：仅aot-identity.json所列路径/身份，不认证stage8源码NativeAOT。
- 实际模块mfplat/mfreadwrite/mfsrcsnk位于Windows System32，10.0.26100.9278；mfcore为10.0.26100.1，原件保留大小写路径。

### 执行总账（历史实际，不是本次新增）
normal2/3、directed tests1/2、COMpublish2/2、sandbox1/2、Native0；implicit Debug1、publishCompile2独列。本次追加normal/tests/publish/sandbox/Native均0，不猜额外codebuild。首publish exit1/MSB3030（历史Native路径不存在），第二次改用已认证同SHA副本exit0；保留失败，不抹为一次成功。normal1 stdout3166826字符前3146826截断；tests stdout1583710前1563710截断；normal2 stdout2062863前2042863截断；publish1 stdout1646856前1626856截断；publish2日志13行未截断但无warning汇总，不猜count。原工具缺完整raw保持限制。

### 源码只读核对：IMILMedia槽位（含IUnknown，0起算）
本次读取`WpfGfxShape/Code/WpfGfxShape/Core/Av/IndependentMediaIdentity.cs` vtable及相关实现，并对照`src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_render.h` 715–840；未修改源码。
- 支持的有限实现：0 QueryInterface、1 AddRef、2 Release、3 Open、4 Stop、5 Close、6 GetPosition、7 SetPosition、8 SetRate（仅0/1，其他E_NOTIMPL）、13 CanPause（真实能力）、16 HasVideo、18 GetNaturalHeight、19 GetNaturalWidth、20 GetMediaLength、22 Shutdown和23 ProcessExitHandler（均映射Close）。异步Open/Seek/Close的返回不是所有后台工作强取消已闭证据。
- 仍E_NOTIMPL（有效未关闭identity）：9 SetVolume、10 SetBalance、11 SetIsScrubbingEnabled、12 IsBuffering、14 GetDownloadProgress、15 GetBufferingProgress、17 HasAudio、21 NeedUIFrameUpdate。boolean/progress输出初始化0；null输出先E_POINTER；关闭后RO_E_CLOSED。不得把HasAudio=0当成功能力查询。
- 公开factory仍E_NOTIMPL/null，禁止原版wpfgfxloader/fallback；内部事件记录不等公开packet ABI通知完成。

### 未闭项与下一直接生产任务
真实player audio/AVsync、公开MediaEventProxy packet ABI通知、factory启用门、强cancel、首次当前源码NativeAOT及双arch整体仍Open；OverallOpen。旧stage7/7b Failed、旧audio endpoint render/capture四状态S_OK/count0及六默认80070490、WAV完整Passed、AVI缺16frames Failed、旧full红/资源Unknown/memoryGreen=null全部保留。stage8视频AVI通过不覆盖音频AVI失败。
下一直接生产任务固定为公开MediaEventProxy packet ABI通知及工厂启用门，随后当前源码NativeAOT首次验证；必须按未闭合同满足门才启用，不因本报告放行。之后由外层连续派发剩余生产任务，不再泛选门，不在本次文档闭链中启动执行或沿用剩余额度。
