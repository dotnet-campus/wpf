# Independent audio endpoint capability observation

## 最终结果：ObservationCompleted / CloseoutCompleted（非AudioOutputPassed）
唯一新增task bddb917098d14aa4a2777710e17948d7，回收WpfGfxShape/artifacts/audio-endpoint-capability/readback。stdout精确TestResults/child.trx全部26行、监督40行/stdout12行/stderr空、identity1行/capability.json全部136行均已亲读。TRX1passed/0failed/0skipped、child0，timeout/cancel/kill均false，仅证明本次观察完整；原stage7b声音用例1failed不抹。

|事实|实际公开API结果|
|---|---|
|CoInitializeEx MTA / CoCreateInstance IMMDeviceEnumerator|S_FALSE(0x00000001) / S_OK|
|render ACTIVE/DISABLED/NOTPRESENT/UNPLUGGED|各枚举及GetCount全部S_OK，各Count=0|
|capture ACTIVE/DISABLED/NOTPRESENT/UNPLUGGED|各枚举及GetCount全部S_OK，各Count=0|
|render console/multimedia/communications defaults|均0x80070490(ERROR_NOT_FOUND)|
|capture console/multimedia/communications defaults|均0x80070490|
|device ID/name/GetState/Activate IAudioClient|无Item，分支NotExecuted，不造device或接口返回|
|OpenSCManager CONNECT / OpenService QUERY_CONFIG+QUERY_STATUS|均成功/error0|
|AudioSrv|RUNNING(4), pid2640, type16, accepted193, win32/specificExit0, startType2(AUTO_START)|
|AudioEndpointBuilder|RUNNING(4), pid2404, status type48/config type32, accepted193, exit0, startType2|
|RpcSs|RUNNING4,pid1076,type32,accepted192,exit0,startType2|
|RpcEptMapper|RUNNING4,pid1076,type32,accepted0,exit0,startType2|
|DcomLaunch|RUNNING4,pid708,type32,accepted128,exit0,startType2|
|依赖|AudioSrv→AudioEndpointBuilder,RpcSs；RpcSs→RpcEptMapper,DcomLaunch；其余无公开依赖|
|全部service checkpoint/waitHint/flags|0|
|QueryServiceConfig sizing|各false/error122(预期容量探测)，第二次全部true/error0|
|token|ProcessIdToSessionId成功/session1，TokenSessionId1，ElevationType1(Default)，Elevation1，Integrity RID0x3000(High)；不是以管理员身份推论有硬件|
|token sizing|12/18/25错误122，20错误24，随后Get全部成功/error0；原值保留不改写|

系统最少所需能力：该验收进程所在Windows环境必须向MMDevice API暴露至少一个ACTIVE render endpoint；现默认路径还要求console默认解析成功，设备可激活IAudioClient并接受固定shared conversion/event合同，有事件/clock/drain进展。当前所有查询状态render/capture数0是环境实质结果；服务已Running，不支持通过启动服务解决，更不能归因原版wpfgfx缺失。未证明音箱听声/loopback，不擅装驱动/虚拟声卡或改变默认/注册表/音量/服务；本轮无active故不选固定endpoint、不重播。远程音频重定向/可用物理端点的实际供应机制由宿主决定，不能保证任一种自动有效，提供后须重新实测MMDevice可见性。

### 身份与资源关键证据
MMDevApi.dll C:\\WINDOWS\\System32\\MMDevApi.dll /10.0.26100.8875 (WinBuild.160101.0800)/SHA8550B6D33D543BAFC9596779317B3C41B11E4F6E103862786A4329693223B02C。actual audioclient.dll/AudioSes.dll未列出，未激活client，版本路径Unknown，不把SDK实现当实跑。
监督与business一致testSHA5F76B7B968941A28D3EF0197518A3FEC77329EAAAD262BABCFA5DF42BBCA2603/MVIDadb19960-1302-43bd-afbd-2a7ff45ea304/.NET10.0.13/X64。aot identity6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403仅fixture/初始化非newNativeAV。所有8个collection.Release=0；enumerator.Release=1（系统引用不可冒充0），对应CoUninitialize已记录。SCM+5service CloseServiceHandle均true/error0、5config FreeHGlobal、4tokenbuffer FreeHGlobal、token.CloseHandle true/error0；audio clients/buffers/events NotCreated，不是Close成功0。全系统native refs/handle趋势Unknown。无需CoTaskMemFree ID/PropVariantClear实跑，因为没有device/property。

### 执行账与边界
normal1/2（实际目标COMAcceptance绝对csproj Release/net10.0-windows，非Tests/net10.0；3917warnings0errors/9.89s）；定向dotnet tests0/1；COMpublish1/1、implicitpublishcompile1；sandbox1/1（内部系统诊断1passed）；Native0。未新增Tests项目normalbuild，原stage7b Tests Release3767warnings0errors独立保留；本轮第二normal及定向tests未用，不为文档再执行。publish与sandbox已尽。准确参数/return/截断见WpfGfxShape/artifacts/audio-endpoint-capability/execution.md；publish selfcontainedCore/Desktop10.0.13/PubAotfalse+managedacceptance true/ComAcceptanceSandbox true，clean/aot fixture已回读。
WAV decode32000frames/64000bytes/hash/EOS Passed、原AVI31984frames/63968bytes/缺16frames Failed/demux根因Unknown、原stage7b真实输出Failed.EndpointUnavailable、旧full红/资源Unknown/memoryGreen=null分别保留；factory E_NOTIMPL/null/禁原版loader、AVsync/NativeAOT/ProductionPlayerOpen/OverallOpen不变。

## 生产音频时钟接入设计（设计完成，未实现/未验证AVsync）
当前IndependentVideoPlaybackOwner仍用Stopwatch与_origin，WasapiSharedOutput只有同步Render完整固定WAV；不得在无输出时接个fake clock宣称同步。下一复用internal具体类型/不可变快照，不新增无必要接口或public API：owner线程取得IAudioClock frequency、position/QPC、epoch和已提交源帧快照，仅有效initialized/started且frequency>0时发布。以流锚点(clock0,PTS0)做100ns映射：PTS=PTS0+floor((position-clock0)*10000000/frequency)，用UInt128中间值并检查long边界；例freq48000/delta48000→10000000，freq8000/delta32000→40000000；delta0→原PTS。S_FALSE/时间戳错误/位置回退/设备失效不当可靠新clock；仅同epoch比较，未知时返回NotAvailable而不是0时钟成功。
Pause固定已确认媒体PTS，恢复重锚；seek/Stop先取消旧数据/flush/reset，在owner完成后epoch++、PTS锚点重设、排除旧clock/视频帧；Close先停止新快照并等待owner自清理，不在callback同步Join。源submittedframes不是已听到帧，padding/input-frame与clock frequency单位不可直接相减；尾静音不增源32000frames；EOS要保留clock进展和drain证据。单元先覆盖非整频率舍入、大数溢出、回退/无freq、pause/resume/seek epoch、close不可读。只有可用endpoint的真实4s输出后才接视频deadline并验AVsync，设计不计通过。

## 当前公开factory与内部24slots实际清单 / 下一确定slice
源码核实IndependentMediaIdentity.cs:90–168,228–361/IndependentVideoPlaybackOwner.cs:1–112；公开SystemMediaSource.Create仍E_NOTIMPL/null，所以公开factory全部player能力未开放，不把内部24slots冒充公开完成。
内部slots0QI/1AddRef/2Release、3Open、4Stop、5Close、6Position、7Seek、8Rate(仅0/1)、16HasVideo、18Height/19Width已有有限实现；22Shutdown/23ProcessExit仅Close别名完整语义未证。Position仍最近视频PTS不是音频clock。9Volume/10Balance/11Scrub/12IsBuffering/13CanPause/14DownloadProgress/15BufferingProgress/17HasAudio/20GetMediaLength/21NeedUIFrameUpdate仍E_NOTIMPL（out先清0），其他rate也E_NOTIMPL。不要沿用stage5“Stop/Pause全未实现”的旧表。
下一确定可开发且不依赖音频端点的slice为GetMediaLength(slot20)：先核实SystemMediaModule现reader duration公开属性合同，在既有OpenAsync owner读取/保存duration，identity在合法状态返回真实100ns值，unknown duration保持精确不可用结果，Close后RO_E_CLOSED；纯单元覆盖null/out、unknown/negative/overflow/state，真实现有4s视频fixture验证40000000。再用真实duration约束seek负值、终点及超界（先对齐官方合同，不盲截断）；0/1 rate已有限支持，补状态回归而非重复实现。progress不能因本地文件一概填1或冒充缓冲完成，留待reader能力明确。当前只完成设计/盘点，没有未经本轮可执行验证的生产改动。后续执行需新独立授权，不能挪用已尽publish/sandbox。本轮未新增AVsync骨架或音频占位。

## Before / 新增预算
stage7b已ExecutedFailed.EndpointUnavailable / CloseoutCompleted，见replacement-independent-media-audio-output-stage7b.md；真实声音未证，不抹原1failed。
本执行者亲自实施，不委派/终端/脚本；程序仅Windows沙箱。新增normal≤2/tests≤1/COMpublish≤1/sandbox≤1/Native0，当前全0。仅新增COMAcceptance测试C#公开Win32/COM查询，不改production/公共API/TFM/SDK/第三包。不调用StartService/ChangeServiceConfig/音量/默认设备/驱动/注册表。
查询：CoInitializeEx MTA、CoCreateInstance IMMDeviceEnumerator实际HR；flow0render/1capture×state1ACTIVE/2DISABLED/4NOTPRESENT/8UNPLUGGED全8集合GetCount/Item/GetState/ID/friendlyname；各flow×role0console/1multimedia/2communications默认HR。仅存在设备才GetState/ID/property；未存在不造字段。SCM仅CONNECT、service QUERY_CONFIG|QUERY_STATUS，AudioSrv/AudioEndpointBuilder递归公开依赖status/config starttype；token查询SessionId/ElevationType/Elevation/Integrity RID。全部原始HR/Win32error、接口/配置内存/token/SCM释放、模块SHA/version、testSHA/MVID/runtime记录capability.json。无audio client/buffer/event创建，不以0伪关闭。
预注册唯一沙箱：sourceDirectory=WpfGfxShape/artifacts/audio-endpoint-capability/clean；executableRelativePath=WpfGfxShape.ComAcceptance.exe；arguments=[--supervise-exit,Results/audio-endpoint-capability-supervision,120,FullyQualifiedName=WpfGfxShape.ComAcceptance.AudioEndpointCapabilityTests.WhenEndpointCapabilityIsObservedThenRawStatesAndServiceStatusAreRecorded]；全任务回收readback；timeout180。诊断Passed仅观察完成，不是音频输出Passed，无active不得fake/null播放。
