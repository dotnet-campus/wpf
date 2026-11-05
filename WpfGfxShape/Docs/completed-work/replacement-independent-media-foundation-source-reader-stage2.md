# 独立 Media Foundation SourceReader stage2

## r2 正式终态：SourceReadClosureValidated / PlayerOpen
r2为当前终态；下文r1的ExecutedFailed原样保留。固定原创AVI未换：65284bytes，SHA256 `01BE17E50794DD8A076967E49D0C2CD6265A3908247748B4B95689AB2F07C872`。实际完整40视频帧→EOS→所有本owner引用释放→MFShutdown/COMuninit通过；不是独立player、音频或NativeAOT完成。

### r2 核证、before-after / 原因及回撤表
- 亲读本机SDK mfreadwrite.h270–316、330–430：IMFSourceReader直接继承IUnknown，slot4/5/6/7/9正确，ReadSample的DWORD/long/native指针及Stdcall、FIRST_VIDEO_STREAM=FFFFFFFC、flags0和四个同步out地址正确；SetCurrentMediaType reserved为NULL。没有修“错slot”或64位对齐，因为未发现此错误。属性ENABLE_VIDEO_PROCESSING GUID正确、类型UINT32，不是VT_BOOL；未设置DISABLE_CONVERTERS/HARDWARE_TRANSFORMS，不能归因过期GetLastError。
- 微软公开合同已抓取：https://learn.microsoft.com/en-us/windows/win32/medfound/mf-source-reader-enable-video-processing （有限YUV→RGB32/deinterlace），https://learn.microsoft.com/en-us/windows/win32/medfound/uncompressed-video-buffers （Lock最小stride/负stride行方向），https://learn.microsoft.com/en-us/windows/win32/medfound/video-processor-mft （不是所有输入输出组合均支持，不能凭格式清单推定转换有效）。生产原先强制DIB24→RGB32，超出有限处理明确承诺；r1协商成功不能证明实际转换有效。
- Before：ENABLE_VIDEO_PROCESSING=1、强制RGB32，首次Read ERROR/E_INVALIDARG。After：GetNativeMediaType(slot5,index0)→验证MAJOR_TYPE=video及RGB24/RGB32/ARGB32→明确SetCurrentMediaType原生类型→GetCurrentMediaType；不启用有限处理，不换文件。实际native RGB24，reader无需执行原RGB24→RGB32转换；在Lock期间按真实signed stride将RGB24展开为独立BGRA，RGB24补alpha255，RGB32/ARGB32保留源alpha而不声称任意RGB32alpha保证。拒绝未支持的原生类型，压缩/YUV解码协商后续Open，不冒通用reader/播放器。
- 确证边界：相同输入系统原生读取40帧及EOS成功，排除了“该系统MF完全不能读取固定AVI”；问题已定位到原先强制转换路径/不充分协商策略。具体系统转换器内部产生80070057的原因仍Unknown，未独立隔离某个属性/内部MFT，不声称唯一微观根因。没有随机枚举属性、换源、fallback或恢复原版wpfgfx。
- AVI生成器静态核对RIFF/LIST/strh56/strf40及PCM16、biSizeImage8、视频chunk8/音频chunk1600均偶数；无idx1且avih flags0不声称有索引。不修改素材补idx，不将初始化成功当容器证明；真实全部sample/EOS为本源可读取的运行证据，并非新增独立RIFF parser认证。

|文件|最小修正/回撤边界|
|---|---|
|Code/WpfGfxShape/Core/Av/MediaFoundationSample.cs|仅internal SourceReader原生RGB协商、major/subtype校验、RGB24逐行展开、完整Data1 stride helper、真实sample/length/negotiation及局部资源释放记录；不整体重写框架|
|Tests/WpfGfxShape.Tests/MediaFoundationResourceTests.cs|固定SHA/长度断言，pointer8，每sample真实格式/长度/hash/平台语义/flags记录；红蓝由四检查点加强为全部40帧双像素；40帧/时序/EOS/空帧与释放断言不降低|
|SystemMediaModule/SystemMediaSource/MediaExports|本轮不改，禁止恢复loader/委托，工厂仍E_NOTIMPL/null，不发假Opened|

无public测试接口/InternalsVisibleTo、无项目/SDK/TFM/包/Host/自动生成代码修改。r2未新增lease/player半成品，先完整保存并核读本条证据和入口。

### r2 实际运行与逐帧结果（原件全部回读）
证据根 `WpfGfxShape/artifacts/mf-stage2-r2/readback/Results/mf-stage2/`；task `2535a010e59c42968b1c1143e5e074f5`。业务JSON1–1006全部分段回读、TRX1–26、supervision1–40、stdout1–12及stderr空全文已读。单FQN仍 `WpfGfxShape.Tests.MediaFoundationResourceTests.WhenOriginalAviIsReadBySystemSourceReaderThenAllVideoFramesReachEosAndClose`；TRX1执行/1通过/0失败，866.2106ms；child OS exit0/HasExitedtrue，监督Reason=exited，无timeout/cancel/kill/drain超时/SupervisorError。工具未给父进程exit，不补造。

- 预注册64次/调用间30秒/源≤4秒、监督120/外180秒保持；40是本夹具40个未压缩视频帧的预期，并非所有系统文件固定帧数保证。实读41次，index0..39均HR0/stream0/flags0/sample非null，timestamp100ns依次 `0,1000000,...,39000000`，严格递增；无tick、无currenttypechanged。格式均RGB24 `00000014-0000-0010-8000-00aa00389b71`、2×1、stride=-8（bottom-up；单行只能验证该布局读取，不能冒多行反向运行覆盖）、packed rate42949672961=10/1；每次buffer length8/output8。
- index0..19 / time0..19000000：每帧两像素为红，BGRA `0000FFFF0000FFFF`，SHA256 `B978F97A664CC8FAE883C8185C0E72E4C16C3BE1FB8901B5306B20FDB24A34B4`。
- index20..39 / time20000000..39000000：每帧两像素为蓝，BGRA `FF0000FFFF0000FF`，SHA256 `42AF801193FAD22A2C6B98D9FE22F22D9C00F8F927539F65D98A41178FA31142`。
- hash为本次实际输出记录，不是跨pipeline bitexact门；全部40帧检查通道强弱平台语义，无按色skip。index40=HR0/flags2/EOS、timestamp0/nullsample/length0，EOS无sample时间不参加有效帧递增检查；唯一nullRead1就是EOS，最终completed=true/frameCount40/eos=true，没有不等EOS就报成功。
- Unlock40次均HR0；每帧先buffer.Release返回1（sample仍持有buffer），再sample.Release返回0，累计各40次，不将buffer返回1误报泄漏或HRESULT。currentType/nativeType释放返回0，attributes释放返回1（reader持有），最后reader.Release返回0；所有本owner引用已释放再MFShutdown HR0/count1及CoUninitialize count1。Close/重复Close/finally-Close HR0，重复Dispose及关闭后Read抛ObjectDisposedException断言通过。引用返回值不是HRESULT，不虚报attributes初次Release最终0。事件proxy从未创建/触碰，无空事件占位通过。

### r2 身份、预算与日志边界
- testSHA256 `338EAD247323239799C7B959C3C479D6F19BD5EE77FE16DC606208B7345291F8`，MVID `1bebaaef-2c88-468a-91ac-48727bf74668`，.NET10.0.13/X64/pointer8，implementation为托管COM source-link `WpfGfxShape.ComAcceptance`。
- 实际mfplat/mfreadwrite均 `C:/WINDOWS/SYSTEM32/`，10.0.26100.9278 (WinBuild.160101.0800)。clean/aot.sha256仍 `6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403`，仅fixture初始化文件身份，不认证新生产NativeAOT或删除链部署；runtimeconfig21行两framework10.0.13全文已核。
- 本轮预算normal≤3/tests≤2/COMpublish≤2/sandbox≤2/Native0；实际normal1（绝对COM项目Release/net10.0-windows，exit0/149warnings/0errors/5.66s）、tests1（4工厂negative passed/0failed/0skipped/18ms，隐式Debug compile1）、COMpublish1（Release/win-x64/self-contained10.0.13、显式fixture/clean/publish/ComAcceptanceSandbox=true，exit0、隐式compile1）、sandbox1、Native0。无终端/脚本/委派。
- build返回64行全读，stdout116898字符前96898截断；test返回61行读头1–10尾54–61，stdout1558945字符前1538945截断；publish返回60行读头1–12尾52–60，stdout58725字符前38725截断。test/publish未给warning总数，不补造0；未显示原文不声称已保存。每次准确参数/返回/路径及预注册见根 `.coding-agent/mf-stage2-r2-execution.md`，r1账保留。

### r2 下一直接实现与仍Open
本条固定源读取闭环已完成，不再把首次Read失败重复交接。下一直接现有项目internal独立帧/bitmap source对象及持有/释放lease（拥有复制像素、timestamp、真实布局，关闭reader后仍可由lease合法持有；不将IMFSample冒充IWIC/MIL）；再接SystemVideoSample/GeneratedMediaPlayerResource；随后同MTA专属调度的独立player状态/Opened-Failed-Ended/seek-rate/媒体时钟及clock lease、音频renderer同步和真正取消flush-drain。压缩格式decoder协商亦Open。默认成功工厂切换必须待正向能力完备，本轮未切换。

独立player/事件/clock/video lease/audio/在途取消=NotImplemented/Open，NativeAOT/真实WPF/双架构媒体/最终full未验证；旧委托绿仅撤销历史、r1ERROR失败保留。所有历史full红和HWbackend/productloss/audio/naturalresources/finalfull独立保留，memoryGreen=null，完整替换Open。以下r1原文仅历史，不覆盖r2终态。


## 实现前快照 / 预定义验收
Before：stage1 internal sample/buffer资源闭环有效；生产原版加载器及player/provider委托已删除，SystemMediaModule只有退役说明、SystemMediaSource.Create返回E_NOTIMPL/null。MediaExports保护不改。旧媒体委托绿仅历史；独立player NotImplemented，完整替换 Open。

本轮预算：normal Release build≤2，定向tests≤1（仅无系统依赖负向；工具隐式Debug编译独立列账），COM publish≤1（隐式编译独立列账），沙箱≤1（单FQN，监督120秒、外180秒），Native0；无终端/脚本/再委派。初始使用均0。

在现有MediaFoundationSample.cs及MediaFoundationResourceTests.cs内增加独立internal owner与正式测试，复用既有source-link，不改工程/包/SDK/TFM，不新增public生产API/InternalsVisibleTo。串行调用者必须MTA，同线程Create/Read/Close；不提供在途取消、不包装Task超时、不并发释放阻塞ReadSample。测试监督为进程边界，不是生产取消实现。

### 已亲读契约
本机Windows SDK `C:/Program Files (x86)/Windows Kits/10/Include/10.0.26100.0/um/`：mfreadwrite.h 270–316、441–535；mfobjects.h 441–533、550–645、828–880、1190–1276；mfapi.h 2610–2625、3070–3088、3274–3282。部分请求被4000字符截断，如实仅记返回段；实现前补读缺失的相关声明。IMFSourceReader IUnknown后SetStreamSelection=4/GetCurrentMediaType=6/SetCurrentMediaType=7/ReadSample=9，IMFAttributes GetUINT32=7/GetUINT64=8/GetGUID=10/SetUINT32=21/SetGUID=24，IMFSample ConvertToContiguousBuffer=41，IMFMediaBuffer Lock=3/Unlock=4；不是猜slot。

微软已抓取正文：https://learn.microsoft.com/en-us/windows/win32/medfound/processing-media-data-with-the-source-reader 及 https://learn.microsoft.com/en-us/windows/win32/medfound/mf-source-reader-enable-video-processing 。前者要求CoInitializeEx及MFStartup、RGB32类型协商后GetCurrentMediaType、未压缩视频每sample单帧、S_OK仍可null/EOS，后者说明属性UINT32及软件有限转换，不是通用实时播放器。RGB32并不保证输入RGB bitexact。

### Owner表
|资源|取得|关闭|
|---|---|---|
|COM MTA|CoInitializeEx成功含S_FALSE|同线程一次CoUninitialize；失败不反初始化|
|MF|MFStartup成功|所有COM引用释放后一次MFShutdown|
|attributes / requested type / current type|创建或Get输出引用|局部finally Release|
|reader|MFCreateSourceReaderFromURL输出引用|同线程Release，默认由reader管理其source；不返回旧vtable|
|sample / contiguous buffer|ReadSample / Convert输出引用|每次Read finally先buffer后sample Release|
|Lock指针|成功Lock|作用域内复制，finally Unlock；不逸出|

固定原创AVI：复核现有Pcore/MediaScenes.cs的DIB 2×1、24bpp、stride8、10fps、40帧/4秒及8kHz mono16 PCM；复用其纯C# RIFF写法生成固定红20帧→蓝20帧，音轨忽略而非音频渲染通过。预定义读至EOS最多64次/源时间不超过4秒/调用间墙钟30秒（不声称能中断同步阻塞）。官方未压缩单帧契约支持本夹具40视频sample预期，严格递增100ns时间；固定0/1秒红、2/3秒蓝弱通道语义，不按颜色挑帧追绿。保存每次ReadSample时间/flags/实际stream/格式/尺寸/stride/像素/SHA及输入身份、运行身份、系统mfreadwrite模块、释放记录，失败也保存completed=false。

## 执行与结果
### 正式终态：Implemented / ExecutedFailed / SourceReadClosureOpen
After：实际新增MediaFoundationSourceReader（internal，同MTA同步有限reader）及正式Tests/COM既有source-link场景；没有改工程、包、SDK、TFM、生成代码、MediaExports或旧工厂。复用stage1 Table/Release辅助；Create执行COM/MF初始化、属性及RGB32协商，Read实现flags/null/EOS/格式变化分支、timestamp顺序保护、contiguousbuffer/Lock/有符号stride逐行复制/Unlock/Release，Close幂等释放reader后Shutdown/CoUninitialize。每owner最多4096次读取、8192×8192及256MiB缓冲布局限制，仅有限提取器而非通用播放器；本测试更严格64次/4秒源时间/调用间30秒。未实现专用线程调度及在途取消，调用者必须串行同MTA；不是完整播放器。

真实场景失败，未完成用户要求的有效帧→EOS闭环，不能跳过后直接宣称lease/player可推进。第一次ReadSample本身返回0x80070057（E_INVALIDARG）、stream=0、flags=0x00000001（ERROR）、timestamp100ns=0、null sample；有效帧0，eos=false，completed=false。RGB32协商成功不是codec可用证明，当前证据不能区分AVI source/转换器限制或实现问题，不能宣称系统API普遍不支持。未改颜色预期、未换素材追绿、未加入codec库、未回退官方wpfgfx。预算沙箱1/1与publish1/1已用尽，不再重跑；下一先定位原失败再完成本闭环，不先开别的工作包。

### 原件 / 身份 / 清理（已全文回读）
本轮task `6d113d56d8d140dcb8b92dfc00a69dba`，证据根 `WpfGfxShape/artifacts/mf-stage2-r1/readback/Results/mf-stage2/`：业务JSON63行、监督40行、stdout20行、stderr空、TRX43行全部已读；TRX目录枚举遗漏，但按stdout精确路径 `TestResults/child.trx` 成功读取，末35–43补读至文件尾。没有将AVI/DLL当文本读取，也没有泛搜DLL。

- 单FQN `WpfGfxShape.Tests.MediaFoundationResourceTests.WhenOriginalAviIsReadBySystemSourceReaderThenAllVideoFramesReachEosAndClose`：TRX1执行/0通过/1失败，219ms；监督HasExited=true/Reason=exited/child OS exit2，无timeout/cancel/kill/drain timeout/SupervisorError。工具无父exit，未补造父exit。
- testSHA256 `8A4DABBCC58DF5D9F003712D71B34EE862F41D02790ECAFA7D3745B43475DC93`；MVID `aa74cf03-2274-4043-a6be-21c894845ae0`；.NET10.0.13/X64；implementation为WpfGfxShape.ComAcceptance托管source-link，不认证生产NativeAOT。
- 原创AVI 65284字节，SHA256 `01BE17E50794DD8A076967E49D0C2CD6265A3908247748B4B95689AB2F07C872`，生成源码在MediaFoundationResourceTests.cs。2×1 DIB24/stride8/10fps/40帧/4秒、前20红后20蓝、PCM mono8kHz16bit。输入仅按生成源码核验及实际文件身份记录，未伪称独立parser已验证。
- 已实际协商RGB32 subtype `00000016-0000-0010-8000-00aa00389b71`，2×1、stride8、packed FrameRate42949672961=10/1。只有Read index0的错误序列，上述timestamp0不算有效帧时间；像素/SHA均null，红蓝时间位置未获实证，没有“全部40帧通过”。
- 实际 `C:/WINDOWS/SYSTEM32/mfreadwrite.dll` 及 `mfplat.dll`，均10.0.26100.9278 (WinBuild.160101.0800)。仅实际调用系统MF源读取；旧Native6BDF只fixture初始化文件身份，不是新Native或部署认证。
- Create聚合HR0，finally-Close HR0；reader.Release返回最终引用计数0（不是HRESULT），MFShutdown HR0/count1、CoUninitialize count1。sample/buffer/Unlock均0次，因为Read未产出sample；因此未验证真实帧Unlock/Release路径、成功后重复Dispose断言或EOS路径，代码存在不等于已验证。测试失败后finally清理确已执行，不空绿。

### 实际预算账
详见仓库根 `.coding-agent/mf-stage2-execution.md`（完整参数按原工具回执保留）：
|类别|实际|原return/日志边界|
|---|---|---|
|normal Release build|1/2|绝对COM csproj/net10.0-windows，exit0/149warnings/0errors/4.57s；64行可用日志全读，stdout116898字符前96898截断|
|定向tests|1/1|绝对Tests csproj，filter FullyQualifiedName~MediaCreationUnavailableTests，exit0/4passed/0failed/0skipped/19ms；implicit Debug build1；61行日志头1–10尾54–61已读，stdout1558945字符前1538945截断|
|COM publish|1/1|绝对COM csproj/Release/net10.0-windows/win-x64/self-contained/runtime10.0.13/ComAcceptanceSandbox=true，显式AotDllPath/clean/publish；exit0，implicit compile1，60行日志头1–12尾52–60已读，stdout58725字符前38725截断，未提供warning总数不补造|
|沙箱|1/1|上述单FQN监督120/外180秒，child2/1failed；工具无父exit|
|Native/full|0/0|未新增Native认证或full|

clean/aot.sha256全文读取=6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403；runtimeconfig两framework均10.0.13全文读取。截断原文没有保存/读到的不冒称完整；本报告保存精确已知返回及范围。

### 安全边界 / 下一实现
Av目录源码搜索NativeLibrary.Load、PresentationCore.dll、wpfgfx_cor3.dll、MILFactoryCreateMediaPlayer无命中；SystemMediaModule空退役说明及SystemMediaSource E_NOTIMPL/null已亲读，MediaExports有效create保护保持，4工厂negative本轮通过，正向player仍Open / NotImplemented。源码不可达不等二进制部署认证。

补读SDK mfobjects.h 530–555覆盖中间方法；mfapi.h 3699–3710/4070–4086读取major及stride helper契约。没有新增用户可见产品字符串/资源项，日志为内部测试证据。没有终端、脚本或再委派。

下一直接调查固定AVI首次ReadSample 80070057：保留源与预期不变，核实原始媒体类型/转换器实际支持与错误来源，先完成全部40帧/递增100ns时间/固定红蓝平台/EOS及逐帧清理的真实闭环；通过后再做独立视频bitmap source lease及GeneratedMediaPlayerResource适配，不将IMFSample冒充旧IWIC/MIL接口；随后player状态/事件/seek/rate、媒体clock lease、音频renderer同步及取消flush/drain。当前不能默认成功创建或发Opened，不再申请独立后端许可。

旧委托媒体绿仅历史；stage1资源通过独立保留；历史full失败不扣减不迁移（controlled x64 1823/1822/1、x861823/1821/2，latest-default x64 1823/1821/2、x861823/1820/3）。HWbackend/productloss/audio/naturalresources/finalfull均Open，memoryGreen=null，完整替换Open。本报告及六份入口和当前执行规则已真实更新并逐份回读；无文档待同步，不代表本闭环完成。
