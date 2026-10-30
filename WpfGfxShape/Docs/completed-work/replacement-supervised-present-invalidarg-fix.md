# JL Present E_INVALIDARG 根因簇

## JS 最终裁决：有限保留 JR 安全矩形 fill 修复
JS 独立核证结束。保留 NativeShapeReader.CaptureSafeRectangle 与专属 Jl 回归；没有发现必须撤回的具体合法反例，不以尚未实现通用 clip 另设撤回门。JS 新生产/测试源码修改0、发布0、业务运行0/full0；没有重跑已通过组或重新发布 Native。下方 JR/JQ/JO 的“进行中、尚未修复”均为当时历史，不代表本终态。完整替换未完成。

### 拒绝根因及有限合同
亲读 artifacts/jr-safe-fill/readback/trace-x64/Results/jr-trace-x64/jp-brush-6464.jsonl 第6行：task8cfa654553014750842fa123c4276b8a，Capture.TryRound 输入(1,0)，ScaleX/ScaleY=1e10，raster=(1.6e11,-8)，clip[0,0,4,1]；4x1/16字节/stride16 的 intermediate 创建与 Open 已成功，随后 FillShape 首负并传播 DrawingSnapshot.Draw action3。不是窗口交换链故障，也不是错误的目标 clip。原实现先转换 28.4，合法几何在安全裁剪前超出量化范围而拒绝。
官方源码 src/Microsoft.DotNet.Wpf/src/WpfGfx/core/common/rtutils.cpp:380-440 已直接读取：ClipToSafeDeviceBounds 将 shape 变换到设备空间，再 ClipWithRect 到 ±SAFE_RENDER_MAX；JR 使用524287。修复只在原 TryRasterPoint 失败时重新读取完整单 figure，证明是直线轴对齐矩形后，设备几何与安全框求交，再交给既有 coverage 量化/clip。没有扩大8388608量化限、clamp控制点、修改 viewport/stroke/曲线算法或将旋转矩形 bbox 当几何。
严格性静态核：只接受一个 figure；每一段(type&3)==1，曲线即使端点凑成矩形也拒绝；First 重置遍历，3条边的隐式闭合或显式第5点等于首点可接受。fill 原路径本就隐式闭合，因此未要求 Closed 标志不是擅自填充 stroke。其余重复点、零长边、共线退化、跳角/自交顺序被相邻三点的横竖交替与不等式排除；正反绕向均可，单矩形两种 fill rule 等价。所有四顶点及变换后四个界限均检查 finite，NaN/Inf 不会变成安全矩形。零 scale 产生空交集；负 scale 通过 Min/Max 正确反序，偏移参与设备坐标。M12/M21 非零一律原拒绝，含90度换轴的更宽合法集合也未扩展，不谎称所有设备轴对齐变换已支持。
整数/half-pixel：安全框交集仍用原 SoftwarePathCoverage.TryCreateFigures/Quantize，(device-0.5)*16、Floor(x+0.5) 未改；±524287 可落在旧限内。clip 仍原样传入，coverage 与会话保持原 clipbox 消费语义，未把 clipbox 当源几何或扩大 viewport。此次只审有限矩形合同；一般超限曲线仍原拒绝，多 figure/非矩形/带剪切旋转未测，不宣称通用裁剪完成。边界、反向、负 scale 等以上为源码推理，不冒充新增运行用例。

### 直接原件核证与退出层级
readback 目录枚举不显示 TestResults，但按 stdout 实际绝对路径映射到本地直接读取成功；三份 child.trx 均真实存在，不能据枚举漏失说缺件。没有恢复运行、跨 task 文件访问或上传 readback。
本地绝对根 d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\jr-safe-fill\readback；双架构原件分别 family-x64/Results/jr-family-x64/TestResults/child.trx 与 family-x86/Results/jr-family-x86/TestResults/child.trx。两份各62行已分段亲读 Results/Definitions/Entries/Counters，均 Completed、7 executed/7 passed/0 failed/0 notExecuted。stdout各12行、supervision各40行、空stderr已读。BL120；子Exit0，无timeout/cancel/kill/drain/SupervisorError；监督器PID/子PID为7076/9408、4432/6536。JS未执行沙箱，因此没有JS外层工具数值exit可宣称。
x64 task6bd19d227a5f4c038ccba8ecb532986f：actual C:\CodingAgentSandbox\Tasks\6bd19d227a5f4c038ccba8ecb532986f\Native\wpfgfx_cor3.dll；SHA1782797EA3271F9426BFEDFC67F30D42834858C778C3ECADF6A40C1AC692CF9A；testSHA2A1A4C860DFE07FE6600E132BFA49A136B32BD4F3F5EE5CF732793163369C933，MVID52283a1c-7912-46b5-bbe5-e3c1085fb178；.NET10.0.13/X64，Native版本1.0.0.0。来源为同进程 Modules 枚举后写出的 Results/jp-actual-identity.txt，不只是文件预期身份。
x86 taskc76ee482bc514a32b1c6394886395e27：actual C:\CodingAgentSandbox\Tasks\c76ee482bc514a32b1c6394886395e27\Native\wpfgfx_cor3.dll；SHA1DFB33772ACC5FFCD3A5D9D865867ACA9F0EA33A6DD7A8F59113056098E744DF；testSHADF126DBFB368FCC14A9FB833CBCF116B1817860BA8D1D94D5AF3A2ACF314D70F，MVIDddef0e33-7cec-465b-9fa8-d6373c4a24e7；.NET10.0.13/X86，Native版本1.0.0.0。同上实际模块证据已读，各架构身份不得互换。
x64首修 taskd0e7ac25d4f84f609060f1e3b7fea3f3：fixed-x64/Results/jr-fixed-x64/TestResults/child.trx 全26行、stdout/监督已读，1/1 Passed、Completed/Exit0，BL120无异常终止；testSHA241C6BA57694024A7677D22514327F71B9751A166C6F6FB5AB99133EED8D5806/MVID3570f45c-49fd-4a87-aa98-d706b107af41。这是先行单例，不与后续7例累加成新full，也不以其testSHA绑定最后测试源码。
官方合法 sourceMode7 对照沿用JL task727631f123f24264bba20e940efdbd47：历史监督/stdout1/1 Exit0，actual AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1、10,0,1326,50504/.NET10.0.13/X64，原始官方模块非候选转发。来源身份详见下方JL记录；JS按精确本地路径读取官方TRX仍不存在，不冒充JS亲读官方TRX或新增x86官方差分。该有限合法对照加候选精确内容断言，不等于7例均做了独立官方新运行。

### 七场景逐项与内容证据
以下前六项完整方法均为 WpfGfxShape.ComAcceptance.JlPresentInvalidArgumentTests.WhenSafeFillRectangleIsClippedThenCompositionPixelsRemainExact；末项为同类 WhenCandidateDrawingImageUsesLargeViewboxThenSynchronousCompositionSucceeds。两架构各七项ID相同且逐项Passed。
共同调用原公开 CompositionTests.WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked：mode0/stroke=false/firstPixel=secondPixel=0xffff0000/relationship0/cap0/join0/dash0/curves=true/drawingImage=true，其余原默认，差异见表。
| sourceMode,tileMode,coordinateScale | 新独立 testID | 原矩阵/93表可证范围 |
|---|---|---|
|7,0,1|19e156fe-40f7-8a3c-a812-fb04a35990bd|原DataRow见BooleanContours:136；JL/JO已核双表旧ID1bbc964c-b6e5-8d5d-8d59-fba9c3c3ca06，JS未重新取得原93 JSON全文，不提升为JS逐表亲读|
|8,0,1|1b0de0d9-64d4-87c4-a216-39f72ba0cb2b|原DataRow:137；JL/JO已核双表旧ID1308b204-c717-8327-88c1-71486c5f582a，同样历史映射非JS新审全表|
|7,4,1|1b7b309a-a209-8a55-9359-f5835019b56f|原DataRow:145可证相同参数；原93表ID/是否失败本轮未取得，不推已修失败数|
|1,0,1|1dad025a-f440-845a-b9b1-873cde6f965e|原DataRow:112可证参数；原93表ID/是否失败未证，作为回归非新增修复数|
|2,3,1|11b1bba7-0f92-850b-bca9-33f7fd65030a|原DataRow:125可证参数；原93表ID/是否失败未证，不能泛归Present簇|
|1,0,1e8|1a6665d0-aca0-810e-b7fc-6f7ed1830bc1|原DataRow:119可证参数；原93表ID/是否失败未证，不能计为原失败已修|
|7,0,1（独立旧红方法）|1567885e-7f12-88d6-885a-8044b734f9b9|与第一行同场景，非第七个不同输入；旧红JO曾到present E_INVALIDARG，此次Passed。原93对应仍仅1bbc964c…，不可重复计修复|
原93清单历史路径 artifacts/jk-readonly/readback-complete/Results/jj-full-inventory 本任务本地不存在；准确保留上述逐项可证/不可证，不编造四项旧ID或“七项全簇”。JR七个测试实际只有六种输入。sourceMode7/8使用大viewbox映射，1/2分别覆盖已有 brush/scale、tile分支；没有将方法名中的curves=true误称超限曲线裁剪已完成（内部再实现失败的是直线矩形）。
原 CompositionTests:721 的 present S_OK、bitmap Lock/GetData/GetStride 均断言成功，非浮点路径:821-823直接从uint像素缓冲取两点与(0xffff0000,0xffff0000)精确比较，无容差，证明预定两点为非空不透明红。sourceMode非0取第一点x1，第二点sourceMode>=5取x2，否则x3；本组height1/y0。无observer早返回、无修改golden/skip/阈值。这是公开API原断言覆盖的真实两点内容结果，不是整4像素全表对比、独立离线二进制复算或所有生命周期长期收敛证明。finally释放lock、channel/connection及相关对象仍保留。
七例TRX成功项为自闭合Passed结果，全文不含精确像素expected/actual文本或原始像素附件；原测试只失败时输出断言差异，成功后没有导出像素文件。没有伪造像素文件，不因未独立人工二进制复算自动撤销真实修复；证据等级为运行Passed加保留的精确非空断言。

### 默认源码构建、安全及后续唯一门
JS只改本文及共享handoff/session-summary/issues自有最新节。NativeShapeReader/Jl测试保留JR源码不再改；JN resolver、JD、NativeRun/ExitDiagnostics/公共隔离/打包/默认并行/无关COMpartition未动。JP trace由Conditional("JP_BRUSH_TRACE")控制，默认未定义；三个历史JL/JO诊断guard保持显式关闭，原Composition默认场景保留。
最终normal默认Release solution build仅一次：专用构建 WpfGfxShape/WpfGfxShape.slnx，只有configuration Release，Exit0、3803warnings、0errors、14.48秒。未传NoWarn、未关闭分析器/改TFM/SDK。日志62行全部增量读取：工具只保留标准输出3185037字符中的末20000，前3165037字符已截断不可恢复；以上warning数取末尾构建汇总，不沿用历史129，不谎称无警告/原完整输出已审。构建不是新矩阵或发布，未扰已验Native hash。
确定裁决：有限保留JR；JS没有新增反例修复需求，不把尚未执行通用曲线clip当额外撤回门。下一唯一工作是同最后源码/双架构实际身份扩大原CombinedGeometry+DrawingImage的相关sourceMode/tile/scale方法族回归，先恢复原93表逐项映射，再由集成协调确定full准备门；本轮不默认重复full。旧两架构full1802/1709/93/0/Exit2保持，不相减造新full。Available/HWloss/mediaresources/final四格继续开放，真实HW恢复/资源/媒体与全量最终身份闭包未完成，不能宣称完整替换。


## JR 进行中；JQ 阶段终态/待核
JR已亲读JQ原始jp-brush-6060.jsonl：task f262332feeeb4c49ab2a34cf6d14b441，4x1 intermediate/Open S_OK，Path.FillShape在clip[0,0,4,1]返回E_INVALIDARG，随后DrawingSnapshot.Draw action3传播。JQ阶段仅定位Capture边界；具体拒绝坐标、完整actual SHA/TRX/监督待核，不算修复或验收。JR已核真实NativeShapeReader/SoftwareFillBezier与官方rtutils.cpp:380-440；官方SAFE_RENDER_MAX=(1<<19)-1，先设备空间ClipWithRect再量化，候选TryRasterPoint先量化。正在以至多一次失败点trace核坐标；不改共享stroke/整数Bezier，不重复旧红或full。未经关键有限验收的JR业务补丁不保留。

## JQ 进行中；JP 已发生实证补记
按监督提供记录：JP task9bcd5e65da9846da87a0157f154b40e0，候选actual SHA前缀227885；intermediate 4x1/Pbgra32/size16/stride16创建与Open均S_OK，DrawingSnapshot.Realize action3的session.DrawPath返回首负E_INVALIDARG。实际Viewbox4x1、Viewport1e10x1e10；前述JO“大viewbox”描述不适用于此已实测内部动作。以上暂为监督提供，JQ尚未亲读原件，不冒充完整SHA或TRX验收。
JQ正在沿真实DrawPath/reader/coverage/clip/brush/export拒绝条件定位，先精确路径读取既有childTRX；不因名称枚举漏项断言缺失。不新增重复报告，不动JN/JD及公共设施。根因未证前不改业务；额外trace至多一次、默认关闭。尚无JQ发布/运行或生产修复。

## JL 已发生证据补记；JO 接管进行中
JL 已修改 JlPresentInvalidArgumentTests.cs 并实际发布/执行，原“尚未修改/发布/运行”仅为阶段历史，不再代表现状。官方合法同步 bitmap sourceMode7 task727631f123f24264bba20e940efdbd47 已运行；监督全文亲读：Exit0、120秒、无timeout/cancel/kill/drain，testSHA E8E3E2C84E351812191ADB36FE4C36DD63629695674A69F77E5201EC88E28FFF、MVID 4b75486c-3861-4fad-8723-76070b70cc72。官方 actual C:\CodingAgentSandbox\Tasks\727631f123f24264bba20e940efdbd47\wpfgfx_cor3.dll，SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，10,0,1326,50504，.NET10.0.13/X64。用户提供1/1，TRX待JO补亲读；保持现有完整像素原断言。

候选 task e160f57c37dd4409b655cd806b37ac65 监督全文亲读：filter尾括号未转义，Exit8，零测试，不是有效旧红，禁止复用该表达式。两任务原件在 artifacts/jl-present-invalidarg/readback/{official-x64,candidate-x64}。工具层数值exit未从原件取得，不混同子进程exit。

JO 正在接管：先隔离历史inputs清单测试，使用无DataRow括号完整方法过滤取得一次有效候选旧红，再核真实bitmap/geometry/brush下层返回；未知根因不盲改生产。仅Present相关必要文件；保留JM/JN并行节，不动resolver、NativeRun、ExitDiagnostics、公共打包及默认并行。完整旧full双架构各1802/1709/93/skip0/Exit2保持，全部93条因果待定，不相减构造新full。

## JO 有界终态：有效首旧红成立；最早下层返回尚未闭合

### 实际执行与身份
- JO专属正式项目正常依赖图x64 Release selfcontained10.0.13发布1次Exit0，无NoWarn/关闭分析器。独有artifacts/jo-present-invalidarg/x64/{obj,bin,publish,com-acceptance-package}，显式PublishDir，只上传正式com-acceptance-package，回收另存readback/candidate-x64/Results。白名单419行首尾、runtimeconfig双framework10.0.13、aot.sha256已读。复用JL包Native，SHA确为0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A；生产修改0、Native重发0。
- 仅一次JO执行：task c74f4e622ed1419da9a26c43c39ca43a；完整方法等值过滤FullyQualifiedName=WpfGfxShape.ComAcceptance.JlPresentInvalidArgumentTests.WhenCandidateDrawingImageUsesLargeViewboxThenSynchronousCompositionSucceeds，不含DataRow括号。父/子PID6924/6940，BL120、外180。TRX全文分段、监督40行、stdout20行及空stderr已亲读：1 executed/0passed/1failed/0skip，ResultSummary Failed，子Exit2；无timeout/cancel/kill/drain。专用执行工具只返回完成，无数值工具码，不将工具完成冒充Exit0。
- 绝对TRX：C:\CodingAgentSandbox\Tasks\c74f4e622ed1419da9a26c43c39ca43a\Results\jo-candidate-x64\TestResults\child.trx；本地d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\jo-present-invalidarg\readback\candidate-x64\Results\jo-candidate-x64\TestResults\child.trx。新独立testID1567885e-7f12-88d6-885a-8044b734f9b9；testSHA C3EB8F3C58A850C780BFCD5FD8678DCA81F2D9FB69C7060D2AAB0FD36F540484，MVID2071c81d-f6f8-4491-81a3-70bbcde337da，.NET10.0.13/X64。
- aot-identity.json记录Native绝对路径及同0C16A66C SHA，但这是ValidateIdentity文件身份，**不是实际已加载模块枚举证据**；本轮未补actualmodule枚举，不冒充满足actualmoduleSHA门。
- JL官方监督、actual identity与stdout12行已读，stdout确认1/1/skip0，绝对TRX为C:\CodingAgentSandbox\Tasks\727631f123f24264bba20e940efdbd47\Results\jl-official-x64\TestResults\child.trx。本地JL回收目录未找到child.trx；官方TRX原件未亲读，此缺口保留。未重跑官方。

### 已证最窄边界与下一唯一根因
真实参数(0,false,4294901760,4294901760,0,1,0,0,0,true,true,false,0,7)。失败在原CompositionTests.BooleanContours.cs:721的present(connection)，Expected0/Actual-2147024809，完整像素原断言与原方法完全未改，尚未到像素读回，不能说内容已验收。

静态真实链：夹具MILFactoryCreateSWRenderTargetForBitmap与TargetPacket创建4x1软件bitmap target；GeneratedTargetRegistry.Render→GeneratedTargetResource.Render在非Hardware分支→GeneratedVisualRenderer.Render→Capture/CaptureInstructions→CaptureGeometryBrush→SoftwareBrushRealization.CaptureFrozen/Capture→GeneratedImageMask.TryCaptureFrozen→DrawingSnapshot.TryCapture；消费再经SoftwareImageRenderSession.DrawPath→NativeResourceBrushRealization.Ensure→SoftwareBrushRealization.EnsureRealization→session.RealizeBrush→GeneratedImageMask.RealizeDrawingSource→DrawingSnapshot.Realize→CreateDrawingIntermediate/Open/内部draw。不能把名为present的同步入口归成窗口交换链错误。

sourceMode7合法大viewbox宽高1e10、viewport1x1；默认tileMode0，走deviceAligned。静态已看到DrawingSnapshot.TryCaptureDeviceAligned按clip裁到有限尺寸；其CaptureInstructions负值被bool化false，调用方通常转E_NOTIMPL而非直接E_INVALIDARG，因此不能仅凭大viewbox断言这里是最早E_INVALIDARG。DrawingSnapshot.Realize各Create/Open/draw、SoftwareImageRenderSession.Initialize缓冲区检查、Direct3D9SoftwareImageRenderer.ImageMask.cs DPI/mapping检查均仍可能传播；没有运行时首返回点证据，不选择其中之一盲修。下一唯一调查：在**同一合法场景的brush realization/嵌套DrawingSnapshot消费链**取得首个负HRESULT来源，再与官方对应实现核对，不运行更多Present变体或显示红帧。生产无诊断、新阈值、fallback或参数改动。

### 完整失败表与未满足事项
JK summary两架构各93失败/1802唯一ID已读；完整JSON只有定向首条及匹配项读取，**未完成两份93条全文逐条核读或共同链簇精确计数**，不得拿命中截断结果推总数。确认两架构首旧ID均1bbc964c-b6e5-8d5d-8d59-fba9c3c3ca06，原名WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked，参数如上，stack同原方法；sourceMode8旧ID1308b204-c717-8327-88c1-71486c5f582a亦在两表，但同根因未证。完整所有testID/name/参数/stack仍保留于原JSON，分类待定，不新造已修簇计数。本轮没有确定生产根因、没有修后双架构族回归、修复数0，x86新运行0。任务未完整完成，不以本节或build冒充收口成功。

### 最终源码安全与构建
仅JlPresentInvalidArgumentTests.cs必要改动：历史inputs测试完整代码/93断言保留在显式JL_PRESENT_INVENTORY；独立旧红方法保留在JO_PRESENT_REPRO。官方控制需要原模块并会修改进程级WPFGFX_CANDIDATE_DLL，本类未DoNotParallelize，故不适合作默认；完整原代码/像素断言保留在显式JL_PRESENT_OFFICIAL，不加无role空返回。三个guard默认均关闭，无测试空过；原CompositionTests场景仍在原默认矩阵。未修改生产、Rounded、resolver、JM、NativeRun、ExitDiagnostics、公共打包或默认并行。没有未验收默认业务补丁留存。

正常Release默认构建共2次均成功，最终guard修改后第二次Exit0/129warnings/0errors/2.79秒，未关规则。最终默认源码未新发/运行矩阵；第一次专属旧红包身份只绑定当时测试源码，不能冒最后源码验收。默认构建结果不等测试通过。共享handoff/session-summary/issues仅自有JO节更新，JN/JM历史保留。旧full两架构1802/1709/93/0/Exit2不变，未运行新full、未推新full绿。

## JL 阶段历史（保留）
已读执行规则、完整替换方案入口、最新handoff、JH/JJ/JK convergence报告及完整清单summary。双架构full各1802/1709/93/skip0/Exit2保留，不重复full。原始清单位于artifacts/jk-readonly/readback-complete/Results/jj-full-inventory；不将所有E_INVALIDARG合并归因。

调查范围仅Present相关生产/测试；JM resolver并行保留，不修改公共设施。阶段记录曾为“尚未修改生产/测试，未发布/运行”；已发生事实以上方补记为准。不运行同步Event0危险输入。
