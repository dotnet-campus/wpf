# 用户 Video.mp4 沙箱诊断播放：r2 ExecutedPassed / CloseoutCompleted

## 结论与证据边界
已实际完成用户文件15秒无声诊断播放：独立系统MF SourceReader → 内存帧 → UI线程WriteableBitmap → 可见WPF窗口。不是MediaPlayer，不借旧Native媒体，不证明生产MediaPlayer ABI、NativeAOT媒体接入、音频同步、全片或完整替换。生产MediaExports仍E_NOTIMPL/null；整体Open。Rendering事件/WritePixels不是具体像素物理scanout证明。

## r1失败及修复来源
r1 taskaf363ec6586842c7aae67cc96d882eb9：TRX1执行0过1败、child OS2。首帧RGB32已解码；CURRENTMEDIATYPECHANGED后640x360→640x368/stride2560，旧bitmap360高断言失败，updates0、elapsed0.116065秒，不能称播放通过。cleanup正常。保留首次publish duplicate CS0111失败及其后未验证源码变动的历史账；旧clean存在部分程序集刷新风险，不作为r2身份。
本轮精确删除RefreshVideoMp4DiagnosticAssembly目标，未覆写其余打包规则。仅VideoMp4SandboxDiagnostic保留专用Content与inputs白名单，改用全新r2/publish和clean正式复制。现有动态RefreshFormat保留，补subtype精确失败及格式/aperture日志。

## 官方格式合同与真实结果
官方MFVideoArea=MFOffset X/Y + SIZE；MFOffset=WORD fract + short value，value+fract/65536。SDK26100 mfapi.h及Microsoft Learn结构/属性文档已核，IMFAttributes.GetBlob slot15读取16-byte结构。
最初coded640x360无aperture；首读flags32/currenttypechanged重新取RGB32/stride/size，coded640x368、stride2560。MinimumDisplayAperture与GeometricAperture均真实返回(0,0,640,360)。完整buffer942080bytes=2560×368，读取时核current<=maximum及pitch×codedHeight，按真实signed stride展开；展示另取合法aperture640×360、rowpitch2560，不把368冒充源可见高度，不盲裁padding。UI按实际display尺寸重建bitmap。分数aperture当前明确不支持，返回InvalidMediaType；无原版fallback。

## 唯一r2执行
- taskd80388edc11f4edea0b5eecd52a21d58，完整回收WpfGfxShape/artifacts/user-video-mp4-r2/readback。
- 输入已正式进包：WpfGfxShape/artifacts/user-video-mp4-r2/clean/inputs/Video.mp4（publish/inputs亦有）；47255617bytes，实际SHA928410B89545B2DF0AA7C8A46756D512D2BE4879209282F90D92D578A30CA40A。
- 原生H264、AAC仅探测，25/1fps，源duration7438266666×100ns=743.8266666秒；无音频输出。
- decoded376；呈现375（UI WritePixels计数），frameCount375；首末呈现PTS0..149600000×100ns=0..14.96秒；额外一帧读取用于15秒边界判定，未呈现。wall elapsed15.0010117秒；stopReason PlaybackLimit、partialtrue、EOSfalse，绝非全片播完。
- renderEvents435，visibletrue（WPF+IsWindowVisible），closedtrue、uiThreadExitedtrue、uiErrornull。帧日志首末已抽读核clock，初始PNG保存会产生启动迟到，不能冒严格零抖动/逐帧scanout。frames.jsonl375行完整回收，未声称人工逐行全审。
- 自身首帧PNG：readback/Results/user-video-mp4-r2/user-video-mp4/first-writeable-bitmap.png，640×360，已加载核内容；不是桌面截图。
- 业务result.json1233行已全部分段回读（含完整cleanup）；监督40行、stdout12行、stderr空、TRX26行全读。TRX准确路径readback/Results/user-video-mp4-r2/TestResults/child.trx：1执行1过0败0跳过，测试duration15.6557308秒。child OS0；监督Reason exited，timeout/cancel/kill/drainTimeout均false、SupervisorErrornull。工具原return未给父进程exit，不能补造父exit0。
- Unlock/sampleRelease/bufferRelease各376；全部Unlock HR0，buffer.Release1（sample仍持有）、sample.Release0；reader.Release0、MFShutdownHR0一次、COMuninit一次，窗口close/UIjoin完成。

## 身份分层
.NET10.0.13/X64；testSHA51DB584E83B5374254F7A39AEB4325A05E2AC79F6774EA3182D4CFBD3B2F7C49，MVIDb3692885-885e-4eb2-92d5-6f15d56ac713，监督和业务一致。SourceReader源码链接进新托管测试程序集，不是新Native导出。
实际系统mfplat/mfreadwrite为SYSTEM32/10.0.26100.9278；mfcore10.0.26100.1、mfmp4srcsnk10.0.26100.8875、mfperfhelper10.0.26100.1150。仅该模块过滤清单，不冒全decoder枚举。
正式clean/aot.sha256=6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403，仅既有验收fixture副本，不认证新媒体NativeAOT实现。

## 新授权预算实际账
normal1/2：绝对COM项目，VideoMp4SandboxDiagnostic/win-x64/net10.0-windows，exit0、152warnings/0errors/8.25秒；日志64行全读，stdout119130字符前99130工具截断，未抑分析器。
正式COMpublish1/1：同配置正常发布，无no-build，self-contained，core+desktop10.0.13，显式fixture AotDllPath及全新r2/publish、clean；exit0，11行日志完整无截断、无warning输出，含正常编译。runtimeconfig双framework已核。
Windows sandbox1/1：源码核唯一FQN，--supervise-exit Results/user-video-mp4-r2 120，工具timeout300，完整目录回收；无重复追绿。Native0。本轮执行账见WpfGfxShape/artifacts/user-video-mp4-r2/execution-ledger.md，历史r1账保留不重写。

## 下一生产交付
直接接独立bitmap-source/frame lease持有释放及GeneratedMediaPlayerResource；实现串行player状态、事件/clock/seek-rate与音频同步、取消flush-drain，再独立NativeAOT/真实生产WPF/双arch/full验证。不再查已解决的r1尺寸故障，不把本次诊断绿迁移为完整替换绿。旧full失败及memoryGreen=null保持。
