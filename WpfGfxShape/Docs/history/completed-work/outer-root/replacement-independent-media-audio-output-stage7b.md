# Independent WASAPI output stage7b

## 当前终态：ExecutedFailed.EndpointUnavailable / CloseoutCompleted
仅证据收尾，不是音频完成。task e2189d578cc44d60b1541f6cd57f9320：stdout精确TestResults/child.trx已读全部41行，1 executed/1 failed/0 passed/0 skipped，child exit2；监督40行/stdout18行/stderr空/identity及output.json模块与cleanup尾已读。GetDefaultAudioEndpoint(render,console)=0x80070490；EnumAudioEndpoints(ACTIVE,render)=S_OK、GetCount=S_OK、ActiveRenderEndpointCount=0。不推论所有状态或capture无设备，其他角色及服务Unknown。
MF WAV create/read S_OK、EOS=true、32000frames；before/after/expected SHA256均018817036867AC8AD146FC532BABBE027CCDEB89F66E786B4BBE78DF5B746CFD。SubmittedFrames/clock/frequency/FinalPadding均0为未执行占位，不是drain/Stop/Reset已成功。IAudioClient未激活，actual audioclient模块路径/版本Unknown（modules未记录），Initialize/render/clock分支仅SDK实现未经系统实跑。
已创建collection.Release=0/enumerator.Release=1（不可谎称全refs0）；OwnedEvent.Close:void、CoUninitialize:void直接记录，reader.Release=0/MFShutdown=S_OK/CoUninitialize已记录，buffer.Release=1/sample.Release=0不等全部native refs0。未创建device/client/render/clock/mix/closest/id，不伪造释放记录。
MMDevApi.dll=C:\\WINDOWS\\System32\\MMDevApi.dll，10.0.26100.8875，SHA8550B6D33D543BAFC9596779317B3C41B11E4F6E103862786A4329693223B02C；mfplat/mfreadwrite=C:\\WINDOWS\\SYSTEM32，10.0.26100.9278。监督testSHA F4A3B9CB070400A214C4EA8A693CFBF08BD400A6D9FC6E0FF93AAFF27110A0F3/MVID49627e1d-ae73-4239-ab5a-c2d2745da2af；source-linked assemblySHA39667CE23293DD13D6C91B149D07A555696E6E7CD7FFC859E895A7ED57ECE29C/MVID1fb740d2-6a30-411e-bf8e-df7900f1cd38；runtime.NET10.0.13/X64。6BDF fixture仅初始化非newNativeAV。
历史执行normal1/tests1(40purepassed)/COMpublish1/sandbox1/Native0，隐式Debug1/publishCompile1；Release3767warnings0errors。AVI少16frames原Failed、WAV decode Passed、旧full红/资源Unknown/memoryGreen=null、factory E_NOTIMPL/null/禁原版loader、AVsync/NativeAOT/OverallOpen独立保留。
独立只读能力调查已收尾，见[报告](replacement-independent-audio-endpoint-capability.md)。task bddb917098d14aa4a2777710e17948d7：render/capture四状态全部S_OK/count0；六默认角色均0x80070490；AudioSrv/AudioEndpointBuilder及RpcSs/RpcEptMapper/DcomLaunch全部Running4/startType2/exit0；token session1/elevated1/high0x3000。诊断TRX1passed/child0仅ObservationCompleted，不是输出通过。无active不重播/不改系统。新预算实际normal1/tests0/publish1/sandbox1/Native0，publishCompile1，publish/sandbox已尽。具体clock映射设计和内部slot实际清单/下一GetMediaLength有限slice已写诊断报告；未新增生产或AVsync占位。

## Before / owner / budget（实现前登记）
Before：同源WAV完整64000bytes/32000frames/4s/440Hz/PCMhash全等；AVI63968/31984/offset32强门Failed，demux内部根因Unknown且Open，比较证据已闭链。WASAPI尚未实现、endpoint尚未测，不预报Unavailable或成功。
本执行者亲自执行，主代理监督；终端/脚本/再委派0。新增独立授权normal≤3/tests≤2/COMpublish≤2/sandbox≤2/Native0，当前0；历史比较normal1/tests1(23passed)/publish1/sandbox1及implicitDebug1/publishCompile1独立保留。
Owner：MF reader专用MTA独立读取完整WAV并先关闭；WASAPI在单一owner线程上创建/使用/释放COM、buffer/event/CoTaskMem，不共享视频控制队列。先核实SDK/docs，再固定mono8000Hz16bit shared EVENTCALLBACK|AUTOCONVERTPCM|SRC_DEFAULT_QUALITY，不自写resampler、不盲试格式、不改系统设备/音量/服务。IsFormatSupported只记录原HR/closest，不代表带自动转换Initialize合同；Initialize才是该固定策略实际支持结论。
目标证据：固定deadline、精确32000源frames，事件填充、尾静音标记、padding drain及audio clock进度、Stop/Reset/释放/close/CoUninitialize；成功最多RenderingSubmittedAndClockAdvanced，不认证听声/loopback或转换bitexact。无endpoint真实HR+active endpoint count+系统模块身份记UnavailableNotCovered，不能skip绿。公开factory E_NOTIMPL/null保持；AVI gap、AVsync、streamlocal volume/balance、剩playerAPI、NativeAOT和整体替换Open，mono不伪平衡，memoryGreen=null/full红。

## 实时执行收据
normal1：targetPath=d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.Tests\WpfGfxShape.Tests.csproj configuration=Release targetFramework=net10.0；原return执行成功/日志62行，1–12及52–62已读；exit0/3767warnings/0errors/21.81s，stdout3155321/前3135321截断/尾20000。当前normal1/tests0/publish0/sandbox0/Native0。

tests1：同绝对Tests项目；filter=(FullyQualifiedName~WpfGfxShape.Tests.IndependentAudioTests&FullyQualifiedName!~WhenRealAudioDecodesThenFixedOracleAndEosPass&FullyQualifiedName!~WhenWaveAndAviAreComparedThenBothFixedOraclesAreRequired&FullyQualifiedName!~WhenCompleteWaveIsRenderedThenEndpointClockAndDrainAreRequired)|FullyQualifiedName~WpfGfxShape.Tests.IndependentMediaControlsTests|FullyQualifiedName~WpfGfxShape.Tests.IndependentMediaIdentityTests。原return成功61行，1–9/48–61亲读，exit0/40passed/0failed/0skipped/35ms。stdout1577956前1557956截断，implicitDebugCompile1；系统MF/WASAPI未在本地测试运行。当前normal1/tests1/publish0/sandbox0/Native0。

COMpublish1：dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj -c Release -r win-x64 -f net10.0-windows --self-contained true -p:RuntimeFrameworkVersion=10.0.13 -p:PublishAot=false -p:WpfGfxShapeManagedAcceptance=true -p:ComAcceptanceSandbox=true -p:AotDllPath=d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\independent-media-stage6-r4\clean\Native\wpfgfx_cor3.dll -p:ComAcceptancePackageDirectory=d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\independent-audio-stage7b\clean-audiooutput\ -o d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\independent-audio-stage7b\publish-audiooutput\ 。原return成功60行，1–10/54–60亲读，exit0，stdout1636935前1616935截断，warnings总数Unknown，implicitPublishCompile1。正式targets生成clean-audiooutput/aot.sha256=6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403，非手写；runtimeconfig21行Core/Desktop10.0.13已读。当前normal1/tests1/publish1/sandbox0/Native0。
预登记唯一系统调用：sourceDirectory=WpfGfxShape/artifacts/independent-audio-stage7b/clean-audiooutput executableRelativePath=WpfGfxShape.ComAcceptance.exe arguments=[--supervise-exit,Results/independent-audio-output-stage7b-supervision,120,FullyQualifiedName=WpfGfxShape.ComAcceptance.IndependentAudioTests.WhenCompleteWaveRendersThenRealEndpointClockAndDrainAreRequired] outputRelativePath省略/全任务 localOutputDirectory=WpfGfxShape/artifacts/independent-audio-stage7b/readback-audiooutput timeoutSeconds=180。不原样endpoint重试。

## SDK contract核查
本机Windows SDK10.0.26100.0 um/mmdeviceapi.h478–510/815–855，Audioclient.h335–417/1118–1138/1364–1386，AudioSessionTypes.h109–141已亲读；官方IAudioClient Initialize/IsFormatSupported已读（Learn对应方法页）。IUnknown0–2是所有下列槽位前缀，Release返回ULONG引用计数不是HRESULT。
|API|slot/签名摘要|返回/所有权|
|---|---|---|
|IMMDeviceEnumerator EnumAudioEndpoints/GetDefaultAudioEndpoint|3(flow,state,collection**)/4(flow,role,device**)|HRESULT；成功输出COM引用由owner Release|
|IMMDevice Activate/GetId|3(IID*,CLSCTX,PROPVARIANT*,void**)/5(LPWSTR*)|HRESULT；接口Release；ID CoTaskMemFree|
|IAudioClient Initialize|3(shared,DWORD,INT64,INT64,WAVEFORMATEX*,GUID*)|HRESULT；shared事件duration/periodicity均0，一次固定策略|
|GetBufferSize/GetCurrentPadding|4(UINT32*)/6(UINT32*)|HRESULT；frames是输入流格式单位|
|IsFormatSupported/GetMixFormat|7(shared,format*,closest**)/8(format**)|HRESULT；S_FALSE不是exact支持；两返回格式均CoTaskMemFree|
|Start/Stop/Reset/SetEventHandle/GetService|10()/11()/12()/13(HANDLE)/14(IID*,void**)|HRESULT；GetService引用Release；event在client释放后关闭|
|IAudioRenderClient GetBuffer/ReleaseBuffer|3(UINT32,BYTE**)/4(UINT32,DWORD)|HRESULT；同线程每次成功Get必须配Release；指针不自free|
|IAudioClock GetFrequency/GetPosition|3(UINT64*)/4(UINT64*,UINT64*)|HRESULT（GetPosition可能S_FALSE）；时钟单位用frequency解释|
flags：EVENTCALLBACK0x00040000、AUTOCONVERTPCM0x80000000、SRC_DEFAULT_QUALITY0x08000000；系统插入矩阵/SRC，不保证bitexact。
