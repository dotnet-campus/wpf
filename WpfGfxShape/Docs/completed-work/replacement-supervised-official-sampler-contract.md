# IU official sampler 独立合同审计：不满足安全复跑门

## 终态

有限静态审计及default正常构建已完成；没有足够证据支持完整合法sampler/异步失败清理合同，故**不强跑，不交付猜测性修正**。有效officialSampler task0/1、部署纠正0、FH复现0、full0，无发布/上传/sandbox。原异常是本轮**未覆盖运行**，不是本轮不复现。生产、共享Historical/VisualCache、项目和现有helper均零修改；default没有新增未验收行为，但原同步Event0路径仍危险，未修复、不应原样复跑。

本报告创建后先前置handoff为IU进行中，再核证。已读取执行规则27行、完整方案分段、latesthandoff当前IT/IS/IR、IR报告分段到96行（部分长返回截断不冒全文审计）、当前调用链及下述官方原件。没有全库审计、随机shader变体、新Host、脚本或命令行。

## 旧IS原件、身份与边界

路径相对WpfGfxShape/。实际包artifacts/is-shared-consumer/x64/com-acceptance-package、publish及回收readback/x64存在。精确核x64/source、x64/snapshot、根source、根snapshot四目录均不存在；根仅列x64/readback，x64仅列包/publish。只排除这些精确位置，**不证明任意位置没有snapshot**，未用名称索引0作缺失证据。未进行旧包IL/PE方法和符号提取，不能将恢复后源码绑定旧IS二进制。当前仓库官方源未取得与4099f839 commit的来源绑定，下述为“仓库官方协议/实现对应”，不是同servicing ABI最终认证。

亲读supervision.json39行：task0727659920f44b25807d75918bc50d46，监督7880/子6940，filtered/120秒，UTC2026-10-25T08:21:57.4812026至08:22:05.1765994；Reason exited，子-1073741819/0xC0000005；OutputCollected=true，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，SupervisorError=null。子Exit不是监督或工具Exit；无first/second chance上下文，不能认异常线程/指令/首次坏写。

亲读stages519起及548–553完整尾段：official=True/geometry=False/cache=False，FF00FF00/expected0000FF80，native前置/两surface/bind后，flags12/context-2/dpi1，initialCommit0即EOF。无sampler首帧/后态、完成事件、cleanup/refs。diag368–380亲读：geometry原四帧0000FF00、00FF0000正确并Passed；sampler UID1c6be0d5-e7e9-8416-85f5-e4e394a6710c仅InProgress/Executing。五candidate Passed为IT已核历史，本轮未重读五节点，不升级为新TRX结果。精确Results/is-x64/TestResults/child.trx读返回文件不存在；TestResults本身有Deploy子目录。无完成TRX，不造最终总数、不拼full。最后Commit边界不认首坏写，不据官方AV改候选复制行为。

亲读实际module JSON，不用包hash代actual：
- official Actual=C:\\CodingAgentSandbox\\Tasks\\0727659920f44b25807d75918bc50d46\\wpfgfx_cor3.dll；SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1；Product10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。
- candidate Actual=同task\\Native\\wpfgfx_cor3.dll；SHA0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A；Product1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a。
- testSHA06587CD317704DC2DB7809A2189DED13BFB6B2C5594BA674F1CB26A327C59767，MVID476c92bb-2279-477a-8a28-61f8f56c2de0，X64/.NET10.0.13。实际托管WPF模块SHA/MVID未独立记录，不能补历史。

## 当前真实调用/资源图

CaptureHistoricalControls39行：officialSampler→RunHistoricalCaptureControl(true,false,false,FF00FF00,0000FF80)→RunVisualCacheConsumer(false,...geometry=false,official:true)→BindTwoInputEffect(visuals[0],visuals[1],false)。没有拿geometry/cache替代sampler。

VisualCache75–128先connect1，两组CreateRT/ColorFill/Create interop、resource97 image/43 renderdata/39 visual。0xA转交bitmap AddRef，0x18内0x47引用image，0x22绑定data。随后resource33 shader、34 implicit、82 VisualBrush、38 effect；发送shader→implicit→VisualBrush→effect→0x1D绑定visual0。46 HWND target以0x35根visual0。图为target→visual0→effect→implicit及VisualBrush→visual1→data1→image1，另visual0→data0→image0；可见链未发现visual1→visual0回边，不外推旧包全图无循环。

官方resource_types.h映射33/34/38/39/43/46/82相符。wgx_core_types.w285–291：HMIL_RESOURCE是UINT32，MIL_CHANNEL才是指针；x64 sampler handle仍4字节。VisualBrush native按TYPE_VISUALBRUSH、hVisual按TYPE_VISUAL取；动画/变换按double/rect/transform取并注册notifier。ShaderEffect按TYPE_SHADEREFFECT、shader按TYPE_PIXELSHADER、sampler按TYPE_BRUSH取并RegisterNotifier。声明类型相符不等于旧运行实际get/QI/refs全证；发送创建成功不是服务端已处理成功。

bitmap/surface/device局部原生指针及数组不代COM AddRef；0xA预AddRef的失败/丢弃责任仍须闭。静态Available非托管入口不存在实例delegate GC收集问题，但空回调不证明Available通知已验收。shadow/brush/effect资源的旧数值、实际native引用/删除轨迹无完整材料，不认资源生命周期通过。

## VisualBrush逐字段表（当前构造，非旧sendbuffer）

依据generated/wgx_commands.cs982–1008 Explicit Pack1、wgx_commands.h1030–1053、processed对应结构及marshal_generated.cpp6466起。new byte[148]零初始化，无tail。自然C++结构尾padding不能代协议：独立自然布局可能推到152，实际native打包上下文与同版二进制仍须证，不能擅自改148或拿sizeof自洽通过。

|offset/宽度|字段|当前值|
|---|---|---|
|0/4|Type|0x83|
|4/4|Handle|secondary，同channel resource82输出|
|8/8|Opacity|1 double|
|16,24/各8|Viewport X,Y|0,0|
|32,40/各8|Viewport Width,Height|1,1|
|48,56/各8|Viewbox X,Y|0,0|
|64,72/各8|Viewbox Width,Height|1,1|
|80,88/各8|CacheInvalidationThreshold min,max|0.5,2|
|96/4|hOpacityAnimations|0|
|100/4|hTransform|0|
|104/4|hRelativeTransform|0|
|108,112/各4|ViewportUnits,ViewboxUnits|1,1|
|116,120/各4|hViewportAnimations,hViewboxAnimations|0,0|
|124/4|Stretch|1|
|128/4|TileMode|0|
|132,136/各4|AlignmentX,AlignmentY|0,0|
|140/4|CachingHint|0|
|144/4|hVisual|source=visuals[1]，TYPE_VISUAL39|

这些是非动画常量，零动画handle不是缺资源。枚举语义尚未与同servicing原件逐项认证。generated_process_message.inl3327–3360的相等长度检查仅DEBUG，retail send0不表示长度和属性全验证。

## ShaderEffect全字段及tail

依据wgx_commands.h766–794、generated_process_message.inl2715–2754、marshal_generated.cpp3563–3865、ShaderEffect.cpp76–149。

|offset/宽度|字段|当前值|
|---|---|---|
|0/4,4/4|Type,Handle|0x70,effect(resource38)|
|8,16,24,32/各8|Top,Bottom,Left,RightPadding|全0 double|
|40/4|hPixelShader|shader(resource33)|
|44/4|DdxUvDdyUvRegisterIndex|-1 int|
|48,52/各4|FloatRegistersSize,FloatValuesSize|2,16字节|
|56,60/各4|IntRegistersSize,IntValuesSize|0,0|
|64,68/各4|BoolRegistersSize,BoolValuesSize|0,0|
|72,76/各4|SamplerInfoSize,SamplerValuesSize|16,8|
|80–81/2|float register SHORT|0|
|82–97/16|float4|0.5,0.5,0.5,0，末项零初始化|
|98–113/16|sampler INT对|register0/mode1，register1/mode1|
|114–117/4|sampler0 handle|implicitBrush(resource34)|
|118–121/4|sampler1 handle|secondary(resource82)|

80头+2+16+0+0+0+0+16+8=122。native从pCmd+1开始逐区copy/推进，SHORT/Float/INT长度模数与2/16/16/8对应，sampler数量8/4=2与info16/(2*4)=2相同。不能把FloatRegistersSize2误当两个register；也不能额外按4对齐补82处。两sampler是brush handle不是texture指针；texture/du/dv由HW实现派生，不是122字节额外字段。derivative=-1使ShaderEffect.cpp253–284非负分支不执行；不是缺少du/dv float register。本轮未完整审完SendShaderSamplersHw，纹理派生约束仍缺证。

PixelShader20字节头：0 Type0x6C、4 shader、8 mode2、12 bytecode304、16 CompileSoftwareShader0，20起76DWORD=304，总324。token FFFF0200=ps_2_0，末FFFF；s0/s1采样、c0定义及组合输出保留。旧diag的CreatePixelShader(76DWORD/304bytes)HRESULT0是D3D前置，不是WPF effect内容成功。

**没有旧IS原始148/122/324字节文件，未完成旧原始sendbuffer逐字节匹配；也未提取旧方法及编译符号。** 当前写包字段与仓库解码的对应只属于静态证据，不冒充实际字节或同版ABI通过。

## 所有权：已证问题与缺证

1. d3dimage.cpp249–287：实际执行ProcessPresent后，即使Present失败也SetEvent/CloseHandle副本；不证明未执行命令丢弃时也close。
2. apifunc.cpp499起仅转调；clientchannel.cpp329–366的separate模式暂存旧recorder，再Begin/End/Close。失败可能在记录或Close之前，单一HRESULT不能判定副本从未进入记录。CloseBatch520–544只将批次放入closed数组；Commit558–575逐批转交，后批失败前批可能已转交。
3. connection.cpp386–398、connectioncontext.cpp356–386：SubmitBatch连失败也转交批次内存，取serverchannel失败直接delete。cmdbatch.cpp49析构为空。crossthreadcomposition.cpp113–173：zombie拒收只signal channel flush并delete batch，未逐命令执行D3DImagePresent，仍返回S_OK。**批次内存ownership不等于内嵌event/COM引用ownership，Commit0不等于已处理event。** 这足以否定“批次会自动清理所有内嵌副本”的推论；基类析构及所有放弃路径未审完，不能扩称所有路径永久泄漏已证。
4. 恢复后显式IS_SHARED_CONSUMER_EXPERIMENT helper全68行亲读：SharedConsumerEvent.Dispose只Dispose原Completion；Duplicate字段没有回收。send失败即Assert，未形成安全副本回收；Submitted在send成功后设定，但没有丢弃后所有权状态；Wait未信号会失败。**此helper不满足安全门，不得当安全默认启用。** 该结论不是旧IS整包cleanup已提取的证明。
5. 不能超时就close副本：若native仍会处理，句柄复用会令后续signal/close命中别物。必须先证drain/销毁完成偏序再收未消费副本；send失败一律close或永不close均不足。0xA的interop AddRef及shadow资源也要同一处理/丢弃清单。
6. 当前default依旧connect1/Event0/同步提交；finally保留destroy→disconnect→bitmap/surface→device/factory→window，不新增绕过失败清理。HRESULT和逐资源delete/flush/refs并未全核，不能称资源门通过；旧IS sampler EOF没有cleanup，不能把当前finally映射成旧实验已执行。

## 实际验证及不改动范围

读项目net10.0-windows/UseWPF/Nullable enable/StartupObject NativeRun、项目SDK10.0.107/MTP、根SDK及Directory.Build.props。未改TFM/SDK/版本/NoWarn/分析器。

default ComAcceptance Release/win-x64专用build一次：Exit0，0 warnings/0 errors，6.24秒，17行日志全回读。普通增量非clean/rebuild，不能抵销历史117warnings，也不是sampler运行。输出正常artifacts/bin/WpfGfxShape.ComAcceptance/Release/net10.0-windows/win-x64，不覆盖旧IS包。无新增代码/公共API，未新增测试，未启动危险默认测试。

由于合法合同不足，未创建IU实验、独有obj/bin/publish/package、未发布或清理；未上传PublishDir/回收根/历史父。离线PE/入口/编译符号预检未执行，不谎称已排除零测试。生产COM/fill/通知/partition/DRDS、NativeRun/ExitDiagnostics、公共窗口捕获、项目打包及CF/CX/CY未改，Native未重发。slot6旧症状不与sampler混同。

## 下一唯一最小门

先以**旧IS已部署程序集和同版4099f839协议/解码证据**闭合“Send记录→CloseBatch→Commit→处理/丢弃→drain→回收”清单：离线提取旧BindTwoInputEffect/调用链/编译符号，核148/80+42及handle；旧原始bytes没有就明确不能追补，新独有实验记录新bytes。关键先取得提交失败、zombie拒收/丢弃时内嵌event和interop AddRef的释放责任，以及channel/partition drain完成证明。缺此清单不进入native复现。

清单闭合后才在现有exe独有IU_SAMPLER_CONTRACT私有接点，基于IR合法asyncgeometry控制保留真实VisualBrush、两输入shader、初始0000FF80/后态00FF0080和cleanup断言；正常依赖图10.0.13 x64 selfcontained、显式独有PublishDir与clean白名单包、离线入口预检后，一次有界监督official sampler，高风险隔离子进程。不是旧Event0复跑或geometry/纯色替代。这是下一门，不是本轮已实现修复。

最后补记IU：有限审计收口，没有可据以交付的合法最小修正；原AV未闭，本轮未覆盖运行。报告及handoff/session-summary/issues最新节写后回读。Available/HWbackend/loss/mediaresources/full四格仍开放，完整替换未完成。

## 前置历史（保留）

IU开始先创建本报告并前置handoff进行中：先核执行规则/完整方案、IT/IS/IR及实际源包，再核字段与所有权；共享Historical/VisualCache只读，默认Event0不复跑，最多1有效officialSampler task，证据不足不运行。此为阶段记录，不是运行证据。
