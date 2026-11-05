# BI 曲线 fill 判别实验与实现审计

## 裁决与修改范围

**新增判别实验已实跑：geometry 与 drawing/visual 不能统一量化阶段。续派已定位原失败的实际 NativeShapeReader fill 消费链：仅补本地 float 保存不足以修复 identity frame9；消费入口已丢失 cubic/arc/rounded 来源，不能无条件改所有 cubic 且承诺 arc/rounded 原样。本次不强修，无生产修改，完整替换未完成。** 主代理只监督。本执行者仅专用文件/发布工具与 Windows 沙箱既有正式纯 C# exe，无终端或直接/间接脚本，无 Host/第三项目/原生探针。

实际修改仅 `Tests/WpfGfxShape.ComAcceptance/CurveDeviceQuantizationExperiment.cs`、本报告；创建 ignored `artifacts/bi-curve-fill/scope.md` 及本根产物。未改 WindowQ、项目/入口/打包、硬件/捕获、共享交接、golden/threshold/skip/fallback、共享stroke/arc/HFD。

先读指定入口与报告。执行规则/完整方案最初按根简写路径读取失败，随后定位 Docs/handoffs/current-execution-rules.md 与 Docs/wpfgfx-complete-replacement-validation-plan.md 并全文读完。入口与BH报告有长行截断，不声称这两份全文无遗漏。

## 实验实施

沿用正式曲线 parent、既有 Render/Compare，保留 A/B/C/D、原十帧 exact、负半格 below/exact/above、line/stroke 和旧控制干预。

- E：原点 FloatTransform(local)，保存 float 点，再 identity 栅格。
- F：local/outer元素先float，各两个乘积分别落float再相加，平移最后加，float合成矩阵再栅格。
- 增加 B/E、D/F 专属半格搜索桶（513个double偏移及相邻double/float ULP），如实输出found；增加显式E/F cubic、官方B/E与D/F互差、控制点/矩阵/IEEE各阶段/28.4整数JSON。
- 同角色记录实际测试程序集 path/SHA256/MVID/runtime/architecture/pointerSize。

## 发布与真实运行

当前实验源码正式 ComAcceptance 项目双架构 Release self-contained 发布工具均返回成功。WpfGfxShapeArtifactsRoot、BaseIntermediateOutputPath、BaseOutputPath分别独占 `artifacts/bi-curve-fill/{x64,x86}/`；publish/package也在该根，未覆盖公共路径，无 --no-dependencies。存在原有MSTest分析器warning，未改其他夹具清警告。x86末次日志58–59确认独有bin/publish；未将完整发布日志保存为长期文件。

**Native候选复用BE separator-v4归档；生产未改，不重发NativeAOT。本轮不是当前生产源码修复验收。**

|架构|sandbox task|本地回收根|业务根（回收根下）|parent执行/通过/失败/退出|
|---|---|---|---|---|
|x64|08c3f932dfa942b990e777870452a73e|artifacts/bi-curve-fill/run-x64|Results/curve-an/fa671b8394a6408f9ffad6a953d21c8a|1/0/1/2|
|x86|e678ab0938384db0861d6c77c876313b|artifacts/bi-curve-fill/run-x86|Results/curve-an/d140094b12e34552b85922fb56e30c05|1/0/1/2|

两份 `bi-parent/parent.trx` 全文41/39行亲读，TestRun x64=98a80ee8-f42d-4a72-91db-40234097be4d，x86=b0391031-d16c-4e9c-a92a-c276c043251a。timeout/aborted/notExecuted均0，失败在exact，非子角色异常。角色成功不算父通过。

comparison.json两份全文亲读：frame1/3/5各44字节、frame9为10，其余0；threshold0、exact=false、hardwareVerified=false。首差696/B36→40、2432/B171→179。没有删改原失败。

## 官方判别结果与亲核范围

separator-search.jsonl双架构各8行全文：A/B负4正4；C/D负0正4；B/E负0正4；D/F负0正4。20样本×4placement×2fill/stroke，summary各160行；每角色8模式共1280张图（循环推导，未逐文件盘点）。不将正桶外推负桶。

两架构summary亲读65–72（截断末行另补71–72）、133–134，以及1–2、97–100、135–136；不是160行全审或人工全图重算。

### B/E：本地保存阶段不能忽略

`separator-be-positive-0-*`，P1=(-0.4442859342857142,-4)，官方B/E fill互差30字节、非空，两架构一致：

|真实placement fill|对B|对E|候选真实差|候选line差|
|---|---:|---:|---:|---:|
|geometry|30|0|36|44|
|drawing|0|30|22|0|
|visual|0|30|22|0|

**geometry选E，drawing/visual选B。** BE旧负样本B/E本来相同，故旧“geometry匹配B”不能排除E；本轮以实际30字节互差解除混淆。geometry line差44同样揭示本地float保存差，不能全部归细分。

亲读x64 `original/separator-be-positive-0-geometry-fill.json:1–110,761–846,1019–1104`：P1 double BFDC6F2E45751DDD、float BEE37972；B栅格x24.499998/41C3FFFF，28.4整数24；E本地保存x2.03125/40020000，identity栅格x24.5/41C40000，整数25；y整数均-7。

stroke不是fill同一模型：geometry对B108/对E126；drawing与visual对B/E均216。保留负控，不能共享加量化。

### D/F：排除本实验的float矩阵先合成

`separator-df-positive-0-nested-fill`：官方D/F互差8，真实nested对D0/对F8，同时对C8，两架构一致。候选真实差38、line差28。stroke对D224/对F218，不能修stroke。

亲读x64 `original/separator-df-positive-0-nested-fill.json:933–1018,1105–1190`：P1=(-1.89500015,-4)，原x double BFFE51EBAD62A43A、float BFF28F5D；D本地保存x0.7618750333786011/3F430A3E、y-0.14687514305114746/BE166670，outer栅格x23.5/41BC0000→整数24；F合成offset=(3.9574999809265137,3.638124942779541)，栅格x23.499998/41BBFFFF→整数23。支持本地MilPoint2F保存后outer，排除本次F。

旧A/B负样本本轮也实跑：summary1–2官方fill互差18，drawing对B0/对A18，候选真实64、line12。没有用新样本替换原失败。

## 普通 cubic 与 rounded 新增审计

官方本轮亲读：

- resources/pathgeometry.cpp:40–78：GetMatrixCurrentValue→SetPathData保存local，不先对double点变换。
- resources/PathGeometryWrapper.cpp:635–720：普通Bezier三个点先ConvertToSingle再TransformPoints，line同样；quadratic另走SetQuadraticBezier，poly后续未全读。
- resources/CurrentValue.cpp:187–249：rect double→MilPointAndSizeF，radii→FLOAT。
- resources/rectanglegeometry.cpp:57–144：等半径且无非identity local走CRectangle；其他AddRoundedRectangle后CShape.Transform(local)。
- geometry/CompactShapes.h:315–365,445–477：CRectangle.Set转交figure.Set，float XYWH构CMilRectF再转交；compact不是另一套double点生成。
- geometry/compactshapes.cpp:105–145：fabs(radius)，零用rectangle，否则调用共用InitBufferWithRoundedRectanglePoints。
- geometry/figure.cpp:986–1000,1038–1074：REAL dimension/2夹半径；rBezier=REAL((1.0-ARC_AS_BEZIER)*REAL radius)，然后float LTRB±radius/bezier赋16个MilPoint2F。
- geometry/shape.cpp:787–797与figure.cpp:754–760：逐figure，matrix.Transform直接修改MilPoint2F缓冲，确有float保存。

候选 SoftwarePathCoverage.cs:190–246普通cubic先parent.Prepend(local)合并矩阵，对double点Apply，未量化控制点交共享Flatten，仅量化输出；quadratic同分支double升阶。:312–365 rounded的rect/radius/Bezier偏移全double，合并矩阵后Quantize仅out _检查，仍未量化控制点进入Flatten。:366起group继续Prepend。差异包含float生成、本地保存、设备量化顺序，不能仅修输出Round冒充闭环。

尚非所有rounded转换辅助函数/矩阵SIMD/CPU、poly/quadratic与候选全部调用点全审；rounded点生成double/float尚缺独立官方有差像素分离。源码审计不能冒充运行二进制每条分支唯一证明。

## 实际身份与快照隔离

四份角色identity.json全文亲读：requested=actual path，官方task根/wpfgfx_cor3.dll，候选task根/Native/wpfgfx_cor3.dll；均RTB软件、hardwareVerified=false。.NET10.0.13，官方10,0,1326,50504，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。

|SHA256|x86|x64|
|---|---|---|
|官方actual|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|归档候选actual|0FCA4287C3E44CB7A7B0801D297C6D4E85F4AD84402D8B9580D0725E8449741B|C8C35CE4872FA659B02F8731F34F1F0492775D30879DEC17E54779F06228BE01|
|本次ComAcceptance actual|6BBBCA3386028D144E7B0111E0629461AB45F3A92E7E076405B22F4525901927|49A7B4CED67FBC6A70D0DE22AFB91702ECEF734B8825BE9826DB94EB631FEAFA|
|程序集MVID|c7ea250c-7116-4f4f-ab7b-a1ce413971c5|21edce72-842f-4663-9e96-a93aa281b56b|

四份 original/candidate/bi-experiment-identity.json全文亲读，同task两角色测试SHA/MVID一致；X86/4、X64/8。没有取得NativeAOT候选或官方托管MVID，不造值。业务根aot-identity.json读取不存在，不声称已读，模块以角色identity为准。

control-alpha.json双架构各100行全文亲读：官方frame7/8无alpha变化、frame9七点；候选frame9两点，负控保留。comparison、identity、search、parentTRX全文已读。大型每控制点JSON仅关键段亲读，未声称2000余行全文；be-device-summary及其他causality大JSON本轮未全文核完。

## 已闭与未闭

已闭（仅有限有差样本）：B/E不再不可区分，geometry选择E、drawing/visual选择B；nested选择D而非本次F。证明placement阶段必须保留，不宜统一合并矩阵。

仍开：B/E、D/F负桶未找到；F仅覆盖本次乘加/平移顺序，未穷尽其他SIMD/FMA/矩阵组合顺序；rounded float生成/变换缺独立有差官方像素；poly/quadratic、group/clip/primitive全调用链未闭。完整HFD/GPU及最终四格full仍未覆盖。

因此不猜修生产，没有生产修复后双架构验收，不拿AY/BE归档证明新源码修复。本包完成两个新增替代分支的有判别力实跑，但整个普通cubic/rounded审计与fill实施仍部分完成，不能标根因全闭或完整替换完成。

## BI 续派收口：原失败路径的最小修复可行性

续派先立即回读本报告原1–103行；首次返回1–70被截断，随后65–103补齐，含86–103全部身份及结论。未使用终端、脚本或新执行器。仅修改本报告；未修改生产、实验、入口、捕获、WindowQ、项目、打包或共享文档。

### 本轮亲读原始证据（历史运行，不是新源码验收）

业务根仍为本报告表中run-x64/Results/curve-an/fa671b8394a6408f9ffad6a953d21c8a和run-x86/Results/curve-an/d140094b12e34552b85922fb56e30c05，均位于WpfGfxShape/artifacts/bi-curve-fill。两份parent.trx全文41/39行、comparison.json全文、两份aq-summary.json全文、四份角色identity.json全文本轮重新亲读。父运行均执行1/通过0/失败1/退出2，timeout/aborted/notExecuted均0；仍是exact断言失败。差分仍frame1/3/5各44字节、frame9为10，其余0，threshold=0。

四份identity实际SHA与上表相符：官方x64 AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，候选x64 C8C35CE4872FA659B02F8731F34F1F0492775D30879DEC17E54779F06228BE01；官方x86 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834，候选x86 0FCA4287C3E44CB7A7B0801D297C6D4E85F4AD84402D8B9580D0725E8449741B。均.NET10.0.13、官方10,0,1326,50504，同原task实际路径；这是归档候选，绝非当前源码新发布身份。本轮无构建/发布/运行，无并发构建快照或新程序集身份可报告；不创建空的新产物根冒充验收。

### 确切调用链及此前审计纠偏

1. RoundedRectangleContractTests.Render:117–126：frame9是StreamGeometry普通Bezier，点(4,28)、(4.04,4)、(36,4)、(36,28)，蓝色fill、pen=null，无geometry/drawing/visual变换。parameters-9.json原文亲读确认firstX=4.04及28.4整数57。frame5是四段普通Bezier加line的显式路径；frame1是DrawRoundedRectangle(-80,60)，frame3是DrawEllipse。故原44/10并不全是一个普通PathGeometry分支；保持rounded/ellipse原样时不能承诺原十帧全部exact。
2. GeneratedVisualRenderer.CaptureInstructions:790–798把primitive转换成DrawGeometry；:844–861强制GeneratedNativeStroke.TryCaptureFill，成功后将NativeStroke=nativeFill、Mask=null。:853的SoftwarePathCoverage路径只在!hasNativeFill时调用，而:847–848已拒绝该情况。因此只改SoftwarePathCoverage.TryCreate(path, parent)不能修原正式fill。
3. GeneratedNativeStroke.TryCaptureFill:23–26转TryCapture；:32调用SoftwareGroupStrokeContours.TryRead(preserveGaps:true, deviceTransform:transform)。SoftwareGroupStrokeContours.Append:75–77读取path，:98–123保存geometry父级点和控制点为float；SoftwareStrokeContours.TryRead:33–40在local变换后保存float控制点。NativeGeneratedPath构造:65–68、:82–90再存float点/控制点，并仅以三控制点标type=2。
4. BitmapRenderTargetExports.Drawing:179–198将snapshot.Transform（drawing/visual父变换）转float Matrix4x4并调用正式内部绘制；ConsumeSoftwarePath:230–239调用NativeShapeReader.Capture(shape, WorldToDevice, clip)。该入口:55–61先在本地控制点上调用共享double SoftwareBezierFlattener（父矩阵仅参与误差），然后Apply父变换；:72才通过SoftwarePathCoverage.TryCreateFigures量化输出。
5. 官方aarasterizer.cpp:1312–1333在float矩阵上计算并Round；:2053先对四个控制点TransformRasterizerPointsTo28_4，:2061–2063再CMILBezier.Flatten。差异不是仅输出Round，亦不是只在geometry入口缺一次float。

local对应geometry.Transform，parent对应drawing/visual及其外层；geometry本地float保存后identity消费对应E，drawing/visual父矩阵消费对应B，nested本地保存再outer对应D。该对应关系仍采用已证实的有限有差分离器，不推翻B/E、D/F结论。当前SoftwareStrokeContours局部矩阵仍用double Apply后cast，与官方先ConvertToSingle再local的阶段也非完全一致；但修此点并不能解释identity frame9。

### 精确裁决与阻塞点

**不能将“fill入口保留float”作为有依据的原失败最小修复。** frame9的local与parent均identity；4.04已在SoftwareStrokeContours.CaptureSegment和NativeGeneratedPath保存float。它仍有10字节差异，因此再次cast或调整本地保存无法改变根本的先细分后量化顺序。两架构aq-summary均显示frame9 double对官方差10、prequantized-double与integer样本差0；frame5分别44/0。注意这些intervention经CurveCausalityExperiment.Render:155–157生成的是line polygon，仅证明此有限输入的因果干预有效，不是生产新代码验收，也不证明所有曲线可以用预量化double替代整数CMILBezier。

**可隔离点在NativeShapeReader.Capture的fill消费，而不是共享flattener；但此处无法辨认普通cubic来源。** SoftwareStrokeContours.Segment仅含Points/Smooth/Gap；普通Bezier、SoftwareStrokeContours:93–96转换的arc、SoftwareCurvedStrokeContours:100–104生成的rounded均成为三控制点segment。NativeGeneratedPath:84只保留type=2，NativeShapeReader:51只看(type&3)==2。直接在:55前统一预量化会同时改arc/rounded，违反本次保持这些路径原样的约束。按控制点值或smooth标志猜来源会误判合法普通路径，不能采用。

这不是“普通fill原则上不可独立修复”：需要在来源尚未擦除的fill专属捕获阶段保留可选择的来源信息，再由fill消费选择正确的float设备变换、28.4控制点及细分，而不影响stroke/arc/rounded。现有快照和ABI figure没有该选择信息；本轮未实现来源传递及fill专属细分，不能把它描述为已经完成的最小补丁。具体阻塞就是上述Segment→NativeGeneratedPath type=2→NativeShapeReader.Capture的信息丢失与错误细分阶段，不是group/poly/硬件等其他路径未穷尽。

**交付：精确不强修结论，无生产补丁、无新双架构发布或回归通过声明。** 原44/10失败、B/E和D/F有差负控、负半格及stroke控制全部保留，golden/threshold/skip不变，无软件fallback。后续若实施来源传递和fill专属细分，必须用当前源码独有正常依赖图双架构Release重新发布并记录实际程序集身份；本报告旧候选不能证明该实现。本轮没有以继续扩大实验替代这一结论。
