# Independent audio stage7 — 执行登记

## 同源 WAV/AVI 比较正式终态（当前权威；比较证据已收尾）
执行者亲自读写、主代理监督，无终端/脚本/再委派，不重跑比较。task674ba8d5bbe14840bbb66703a71d589f 原件位于 WpfGfxShape/artifacts/independent-audio-stage7/readback-wavcompare；stdout明确TRX路径映射 Results/independent-audio-stage7-supervision-wavcompare/TestResults/child.trx，1–34及35–43尾已读。监督40行/stdout20行，child2/1executed/0passed/1failed，非timeout/cancel/kill，输出排空无超时。container850–878及两result916–948尾补读。

|独立门|WAV|原AVI|
|---|---|---|
|结果|Passed|Failed，不能以WAV冲销|
|bytes/frames|64000/32000|63968/31984|
|匹配源offset/首PTS|0/0|32/20000|
|PCM SHA256|018817036867AC8AD146FC532BABBE027CCDEB89F66E786B4BBE78DF5B746CFD|73CE958AB2ED55F7535EE2BD9DC5532FDF5CA0480F3A06D4B475C61847B7E41B|
|EOS/首8000frames crossings|true/440|true/440|
|Create/末Read HRESULT|S_OK/S_OK|S_OK/S_OK|

源PCM64000bytes/32000frames/hash全等，canonical WAV44byte header/64044bytes，WAV SHA4457FE0B56CE1AEEA90DDC92ED2B6DAB20E0410949CC628E0A32562EE4552A02；AVI SHA01BE17E50794DD8A076967E49D0C2CD6265A3908247748B4B95689AB2F07C872。支持容器相关demux边界差异，不证明系统内部唯一root cause；原AVI expected不得削减或补零，gap仍Open。相同production internal reader可完整读取WAV，不需先修AVI才实现WASAPI。
原始comparison.json顶层Passed=true来自匿名对象wavResult.Passed简写，仅WAV门，**不是联合门**；真实联合断言wavResult.Passed && aviResult.Passed失败，与TRX一致。保留原始证据，不手改JSON冒充复验。
两reader末尾Lock/Unlock S_OK，buffer.Release1/sample.Release0、reader.Release0、MFShutdown S_OK、CoUninitialize:void；Release值是引用计数非HRESULT，全native引用总数Unknown。不是WASAPI cleanup或已证声卡输出。
wrapper SHA2E074D09B94B36666D23C2B15A04A68ED37AFC11FF1A2FE305CBC9AE37BA720D/MVID54b55c32-62b4-44dc-8730-bd125a5ac910；source-linked Tests SHAD32A3A523AF32C0848D26EDC20966773460BC3F6A0FF9C5D012B1E1B6F189178/MVIDe7f651ce-f9be-4271-8b68-c1daeaebbd09，.NET10.0.13/X64。旧6BDF仅fixture，不是新NativeAOT。
比较账normal1/tests1(23purepassed)/publish1/sandbox1/Native0；implicitDebug1/publishCompile1。build3767warnings/0errors；stdout3155321前3135321截断，tests1577956前1557956截断，publish1636933前1616933截断；详wav-comparison-receipt.md，旧NativePCM normal1/tests1/publish1/sandbox1及之前所有预算/失败/Unknown保留，不合并冲销。此次仅补读/文档新增执行0。
下一直接独立internal WASAPI共享事件输出完整WAV，新增授权normal≤3/tests≤2/COMpublish≤2/sandbox≤2/Native0，当前新增0。endpoint未测，不能造Unavailable；AVsync/publicfactory/完整player仍Open，E_NOTIMPL/null及禁止原版loader/fallback保持，memoryGreen=null/full红。后续stage7b报告独立登记，不把WAV decode当output。以下旧当前段落均历史。

## NativePCM 实测否定与 WAV/AVI 对照（当前权威）
NativePCM task65e990f3d35f422588de32c4533bafe6 已失败：仍63968bytes/31984frames，actual=source[32..64000)，首PTS20000/duration980000；seek0无效。避免冗余协商恢复首16frames的预测被否定，直通策略保留、slot37保持，不再seek或随机属性试错。原AVI严格64000门为Failed，不能合法化音频错误。旧轮normal1/tests1/publish1/sandbox1/Native0独立保留，旧警告/截断和历史失败不冲销；详见nativepcm-continuation-receipt.md。
本次独立授权normal≤2/tests≤1/COMpublish≤2/sandbox≤2/Native0，当前新增0。实际工具支持文件读写、build/test/publish及Windows沙箱，不泛称能力中断。先纯C#严格RIFF解析/关键AVI字段JSON及标准44byte WAV生成测试，再同实现两个独立reader读完分别落盘、统一硬门判定；WAV不替代AVI验收。WASAPI尚未实现/endpoint未测，AVsync/publicfactory/NativeAOT及完整替换仍Open，factory E_NOTIMPL/null和禁止原版loader/fallback保持。


状态：**ExecutedFailed / CloseoutCompleted（本次验证链已收尾，不代表音频功能通过）**。固定64000bytes/32000frames全音轨强门仍失败，不能标AudioDecodeValidated。下列旧InProgress/预算/待执行段均历史；以本节和末尾seek终态为准。独立WASAPI尚未实现、endpoint未测，不记Unavailable。不是完整播放器验收。

## Before / After 与预算
Before：stage6 ControlsEventsVideoValidated / CloseoutCompleted，33 unit passed 仅历史视频控制。HasAudio 未支持，默认 MediaExports 工厂 E_NOTIMPL/null，原版 wpfgfx loader/副本/fallback 禁止。
After：待真实执行登记，不预报成功。
本轮上限 normal build 4 / 定向 tests 3 / COM publish 3 / sandbox 3 / Native 0；当前均0。终端、脚本、再委派0。生产默认 AssemblyName/NativeLib/TFM 不变。

## 所有权及验证合同
|owner|合同|
|MF audio reader|专用MTA，逐实例 CoInitializeEx/MFStartup 配对；音轨选择、PCM协商、sample/buffer Unlock/Release、EOS与媒体类型变化；同步读取不得占用视频控制队列|
|有界PCM|固定容量、帧对齐，不无限堆积；格式不支持返回真实错误|
|WASAPI output（待实现）|公开 IMMDeviceEnumerator/IAudioClient/IAudioRenderClient/IAudioClock，event-driven shared；mix format/IsFormatSupported；stream-local gain，不改系统音量|
|player（本轮不接入）|现有Stopwatch视频时钟不冒audio master；后续采用audio clock position→100ns媒体锚点，pause/seek/flush/discontinuity重锚，EOS排空、Close取消须另验|

## 证据层级
1. 原创固定440Hz/8000Hz mono PCM输入：源SHA、PCM SHA、帧数/声道/频率、非静音、PTS和EOS，不以HasAudio认证解码。
2. 真实设备激活、Start、event、GetBuffer/ReleaseBuffer提交、padding、clock、Stop/Reset及释放：仅render设备证据，不证明物理听声。
3. loopback内容、物理扬声器、完整AV sync、public factory、NativeAOT：本轮未验，保持Open。
无endpoint以真实HRESULT记Unavailable/NotCovered，不skip绿，不换设备或改系统服务/音量。全系统资源未知不补0。

## 本续执行登记（优先于旧额度）
执行者：本执行子智能体；此前沙箱亦为前执行子智能体工具调用，不是外层主代理直接执行。
本续授权上限 normal3 / COMpublish2 / sandbox2 / Native0；pure run_tests拟用1，系统MF仅Windows沙箱。当前本续新增均0。
修改前 owner：audio reader持有sample至finally Release；GetSampleDuration错误slot38以long*签名调用实际SetSampleDuration(long)，会写入指针数值，不能忽略此副作用，也不能据此证明丢16frames的原因。
ABI before/after：38→37；共享已核实sample槽定义；视频只允许同根ABI去重，不改正常控制路径。回撤：仅回撤本续sample调用及共享定义/新增测试，不回撤原固定64000bytes/32000frames oracle，不降低expected。
sourceaudio oracle来源：MediaFoundationResourceTests.WriteAudioOracleAvi原创AVI及测试独立440Hz/8000Hz/mono/16bit/32000frames生成式；将读取源生成器及真实产物核对，不用MF输出反推expected。
本机权威 C:\Program Files (x86)\Windows Kits\10\Include\10.0.26100.0\um\mfobjects.h，IMFSampleVtbl 996–1275。IUnknown0–2；IMFAttributes3–32；GetFlags33/SetFlags34/GetTime35/SetTime36/GetDuration37/SetDuration38/BufferCount39/BufferByIndex40/Contiguous41/Add42/RemoveIndex43/RemoveAll44/Total45/Copy46。GetDuration1219–1223，SetDuration1225–1228，Time1208–1217，Flags1197–1206，Contiguous1243–1247。头文件读取截断部分已分段补读1086–1113及1197–1213；0–32列表继续登记。

## 权威依据（核查中）
本机 Windows SDK 10.0.26100.0 um/Audioclient.h、mmdeviceapi.h、mfreadwrite.h、mfobjects.h、mfapi.h及shared/mmreg.h；微软公开接口contract须逐项核实。

## 执行收据
normalbuild1：绝对Tests项目 Release/net10.0，exit1，1501 warnings/1 CA2208 error（ArgumentException参数位置）；stdout1230042字符仅尾20000，前1210042截断。已准确修正为ArgumentException(null,nameof(source))。
normalbuild2：相同参数exit0，3765warnings/0errors，62行日志末段已读。COMpublish1：绝对ComAcceptance Release/win-x64/net10.0-windows/self-contained，PublishAot=false、WpfGfxShapeManagedAcceptance=true、RuntimeFrameworkVersion10.0.13、ComAcceptanceSandbox=true、fixture指向stage6-r4/clean/Native，输出independent-audio-stage7/publish；exit0，stdout1636106前1616106截断，warning总数Unknown。旧6BDF仍只fixture。
sandbox1：task2238cf1954c545dcb01a93c33d5ac329，单FQN WhenAudioDecodesThenFixedOraclePassesOnDedicatedMta，监督120/外180，childexit2、1failed，初始化缺publish/aot.sha256，未触达MF业务，不能称解码失败或成功。监督40/stdout25已读，真实wrapper SHA868FB6EA1582D846A9C8C53067DEB3CC4FEAAC217DF218DCEE2AC8A5CC1481CD/MVID9c77b653-54f3-4196-a0e6-5548ce45ce78，.NET10.0.13X64。核对旧clean manifest后向本轮publish补同6BDF文本manifest，不绕过ValidateIdentity。待定根因明确后复验。
sandbox2（交接事实，待本轮原件回读）：task40b17910bea44412892ef656e8d7c117，已进入真实MF，首PTS=20000而原测试强断言0失败；不能记完整PCM/EOS通过。第一次使用publish根而非正式targets生成的clean，缺aot.sha256属于打包/入口错误。第二次手动sidecar来自历史clean的6BDF文本，尚未以本轮权威GetFileHash/实际PE认证，不构成包身份闭合；不能因目录名clean就认定身份齐全。本续不再手写hash或沿用该包，后续只使用正式PackageSelfContainedComAcceptance生成的新clean与运行时身份校验。
累计normal2/4 tests0/3 COMpublish1/3 sandbox2/4 Native0，publish隐式compile1。本续剩normal2/tests3/COMpublish2/sandbox2（含用户额外授权1），Native0；终端/脚本/再委派0。旧tool原return未见持久收据的字段维持Unknown，不伪造。

## seek0 最终实测与收尾（当前权威）
执行者为本执行智能体亲自实现/调用，外层主代理监督；此前也是前执行子智能体执行，非主代理直接执行，旧continuation-receipt已就地更正。无终端/脚本/再委派。精确收据见 ../../WpfGfxShape/artifacts/independent-audio-stage7/seek-continuation-receipt.md。

本次静态修正测试源解析：仅纯C#读二进制，按RIFF/LIST声明size、padding、父列表边界遍历，不扫描payload字节；唯一audio stream1，PCM1/8000Hz/mono/16bit/blockalign2/byteRate16000，audio initial0/scale2/rate16000/start0/length32000/sampleSize2；40video/40audio chunks，各audio1600bytes，合计64000bytes/32000frames，逐帧独立440Hz公式核对。源生成器及expected均未修改。没有用文本读取AVI/PCM。

本次normal1：绝对Tests Release/net10.0 exit0，3767warnings/0errors；stdout3155321字符，前3135321截断。COMpublish1：绝对COM项目Release/win-x64/net10.0-windows、自包含Core/Desktop10.0.13、PublishAot=false、WpfGfxShapeManagedAcceptance=true、ComAcceptanceSandbox=true，显式stage6-r4/clean/Native fixture，正式targets生成publish-seek/clean-seek/aot.sha256。exit0；stdout1636927字符，前1616927截断，warnings总数Unknown，implicitcompile1。没有手写sidecar。

唯一sandbox task010f946c57ab450d81cbca7366a64c4d：准确FQN WpfGfxShape.ComAcceptance.IndependentAudioTests.WhenAudioDecodesThenFixedOraclePassesOnDedicatedMta；监督120/外180，完整任务回收到WpfGfxShape/artifacts/independent-audio-stage7/readback-seek。监督40行、stdout19、stderr空、TRX1–42含尾段、business native/current/seek/sample/EOS/cleanup/identity全部关键亲读；旧r3和abi两份TRX1–44含31–44尾段亦已补读，不重跑取件。childexit2，1executed/0passed/1failed，非timeout/cancel/kill，output drain正常。

|门|seek前ABI轮|seek0后本轮|
|---|---|---|
|源AVI SHA256|01BE17E50794DD8A076967E49D0C2CD6265A3908247748B4B95689AB2F07C872|相同|
|源PCM SHA256/数量|本轮严格解析认证|018817036867AC8AD146FC532BABBE027CCDEB89F66E786B4BBE78DF5B746CFD / 64000bytes / 32000frames|
|actual数量|63968bytes/31984frames|相同，强门64000失败|
|actual SHA256|73CE958AB2ED55F7535EE2BD9DC5532FDF5CA0480F3A06D4B475C61847B7E41B|相同|
|首PTS/duration/length|20000/980000/1568|相同|
|seek0 HRESULT|未调用|S_OK，未补回16frames|
|native/current|旧未完整采集|两者均PCM mono8000Hz16bit/blockalign2，无格式变化|
|source内容匹配|此前未强解析定位|actual全段精确匹配sourcePCM[32..64000)，仅缺前32bytes=16frames=2ms，末尾不缺|
|频率/时序/EOS|不冒完整通过|首8000实际frames rising crossings440；40个data sample，后39个1600bytes/duration1000000，PTS1000000..39000000，连续至40000000；末ReadSample S_OK/flags2/length0|

根因分层：已确证原GetDuration slot38误调用setter是ABI错误，现为37且独立ComImport赋值/读取验证已先行并在本轮业务前通过；它不是已证的缺16frames根因。当前失败直接原因是系统MF链输出从源frame16开始，源payload完整、无显式2ms start，生产CopyTo无byteoffset。seek0成功仍完全相同，不能解释成随机读取或expected应减16；系统AVI source与冗余PCM协商转换哪个环节裁切的内部原因仍Unknown，不能声称真正内部根因已查明。合法source时序与全音轨exact不是同一门；读出PCM能力已有实测，全音轨exact仍ExecutedFailed，非AudioDecodeValidated。

cleanup：40次Lock/Unlock S_OK，buffer.Release=1为引用计数（sample仍拥有buffer），sample.Release=0；reader.Release=0，MFShutdown S_OK，CoUninitialize:void。CoInitializeEx S_FALSE也正确配对。全部系统native refs总数Unknown，不把Release计数当HRESULT。强门失败后显式重复Close/closed-read断言尚未执行，finally Dispose释放已执行。系统模块均Windows SYSTEM32 mfplat/mfreadwrite/mfcore/mfsrcsnk；音频生产reader仅系统MF，未回载官方wpfgfx。

wrapper SHA29EFA94EB96ADB52888DF0A0ED9FC01FAC5C40A9B971D930A90D2D07E30C0B6F / MVID37a0c492-79a0-4382-8cae-4f49ceee6838；source-linked Tests SHAA35BF0655F3D7C965365D05C7CD0B6BDB6A17EB1F4903C014EDC0D4246E8737F / MVIDff70dc21-aa0b-49e3-80c6-577b83bae88a；运行.NET10.0.13 X64。正式GetFileHash manifest与运行实际PE认证SHA6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403一致，仅旧fixture，非本次新NativeAOT/媒体实现。

预算终账：本次normal1/2、puretests0/2、COMpublish1/1、sandbox1/1、Native0；剩normal1仅编译错复验条件未触发，tests2不凑满，publish/sandbox已尽。stage7跨轮累计normal4（含初次失败）/puretests1（历史9passed）/COMpublish4（implicitcompile4）/sandbox5（全部失败）/Native0；历史测试implicitDebug1。首缺manifest、次zero-origin、r3/ABI/seek三次63968失败均保留。旧收据的Unknown/截断不冲销。

唯一后续代码方向：先在现有Create中把GetNativeMediaType/完整格式判断前移；当native本身已是支持的PCM时保留native/current pass-through，不做仅major/subtype的冗余SetCurrentMediaType；仅压缩源才请求PCM协商。这是受native=current PCM且内容只缺前16支持的单一待验修正，不是已证转换器根因，不继续seek/随机属性试错、不补零/拼接源payload冒解码、不改oracle；必须重新验证64000强门后才能称exact闭合。本轮不再修改生产reader制造未验修正。该闭环后下一直接WASAPI系统输出实现，再时钟同步，不再泛选门。

独立WASAPI尚未实现，endpoint未测（不是Unavailable）；公开factory E_NOTIMPL/null、原版loader禁止保持，NativeAOT/完整AVsync/publicfactory仍Open；旧full红、资源Unknown/memoryGreen=null、ProductionPlayerOpen/OverallOpen不变。当前验证链文档CloseoutCompleted，不等完整替换完成。
