# BE 曲线设备量化根因审计

## 续轮实做：separator-v4（保留失败，非生产验收）

本节优先于下文旧轮“下一最小实验”的状态。仅修改 CurveDeviceQuantizationExperiment.cs 和本独有报告。未修改 BF/WindowQ、项目、入口、打包、共享交接或生产；未发生生产同文件冲突。通过专用发布工具、Windows 沙箱正式现有 C# 测试 exe 完成实验，无终端或脚本。新产物均位于 artifacts/be-curve-device-quantization/ 的 separator-v2/v3/v4 独有子目录。

### 实施与执行

新增半格搜索、A/B/C/D 四条显式预量化 cubic、真实 geometry/drawing/visual/nested cubic 与 line、fill/stroke 对照。每个样本 JSON 记录 double/float 输入位型、矩阵、float 乘积/和/平移后值的 IEEE 位型、28.4 整数。separator-summary.jsonl 独立比较官方 A/B、C/D 像素，不以整数不同代替像素判别力。

v2 仅相邻 double/float ULP，v3 增加半格附近 ±256×1e-8 搜索；两次 x64 正式执行均因强制要求每个搜索桶成功而失败，保留全部结果。实际缺失的是 C/D negative 桶，不是所有搜索失败。v4 将每桶 found 明确写入 separator-search.jsonl 后继续渲染已有分离样本，不改 parent exact 断言：A/B negative=4，positive=4；C/D negative=0，positive=4。每角色产生 12样本×4 placement×2 fill/stroke×6模式=576张分离实验原始图及96份控制/位型JSON；比较汇总96行。这些数量由循环/结构确定，尚未逐文件检查全部图像。

当前实验源码分别独有 Release/self-contained win-x86、win-x64 发布成功。但 Native 候选明确复用旧归档，未声称当前生产源码依赖图重发；因未生产修复，没有修后验收。

### 已亲核的实际像素判别

两架构亲读 separator-summary 的1..8及65..72行（x64初次读1..5，随后补齐6..8）；不是全文96行跨架构审核。下列为这些行内可直接成立的结论，单位为不同字节：

- ab-negative-0：P1=(-3.8728574428571427,-4)。官方 A/B fill 差18；真实 geometry、drawing、visual fill 对A差18、对B差0。两架构相同。候选真实fill差64；对应line fill差12，stroke line差0。这说明新输入也暴露真实line的变换/舍入差，不能把所有差只归为曲线细分。
- 此P1在A的栅格x=-23.500004，IEEE C1BC0002，28.4整数=-24；B栅格x=-23.5，IEEE C1BC0000，整数=-23。A本地double变换后的x位型 BFEF00008CEDB9AC、转float BF780004；原始P1 x位型 C00EFB9CAED92208、转float C077DCE5。已亲读x64 JSON 1..100及470..650行，确有中间IEEE与整数分离，不是模型命名。
- cd-positive-0 nested fill：官方C/D差8；真实官方对C差8、对D差0，两架构一致。候选真实差38；对应line fill差28。C/D的stroke差48，但真实stroke对C差218、对D差224；stroke不能用fill预量化曲线替代。此样本的完整位型JSON已生成，尚未逐段亲读，不能声称与上条相同的位型亲核深度。
- cd-positive-0 非nested fill的A/B本来相同，不能用它区分A/B；ab-negative-0 nested的C/D本来相同，不能用它区分C/D。C/D negative 未找到，不宣称负半格全覆盖。

有限样本已排除“先本地double预变换”的A像素模型，嵌套样本已在像素层选择D而非C。但仍不能据此给整个曲线族选定一个通用算法：B是原点进入带×16矩阵的一次float运算；真正 geometry 本地float变换、保存MilPoint2F再交栅格identity的中间分支尚未作为独立E模型，与B逐半格分离验证。D胜C也未穷尽“两个float矩阵先合成”的替代分支。正是这些上游分支及新line差异阻止生产闭合；不能用零差反推所有内部运算顺序。

### 本轮 parentTRX、业务与模块身份亲读范围

v4 x64任务3cea3b588a674696980033f9a7ccda0b，业务根 Results/curve-an/606269f7f75049eebbaab712cfe210ce；x86任务940ff07be0b142648f81f97a4cd2ad58，业务根 Results/curve-an/982c21e1286a4054ad2c345a61dec949。两份 be-parent/parent.trx 已各全文亲读（x64 41行、x86 39行）：TestRun分别565b703c-2a72-4be7-8dfe-8d4ef66d2985、94ee48e7-6655-43b7-b6d0-9636a5a8ecd4；均total/executed=1、passed=0、failed=1、timeout/aborted=0、outcome=Failed、exit2；失败在原exact断言，不是渲染子角色失败。

两份comparison.json全文亲读，仍frame1/3/5各44，frame9为10，其余0；首差offset696 B36→40、offset2432 B171→179，threshold0，exact=false。没有将原失败转成通过。

两架构两角色的identity.json四份均全文亲读，Requested=实际加载path；官方在各任务根/wpfgfx_cor3.dll，候选在Native/wpfgfx_cor3.dll。运行时.NET10.0.13；官方version10,0,1326,50504、commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。官方SHA x86=5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；x64=AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1。候选SHA x86=0FCA4287C3E44CB7A7B0801D297C6D4E85F4AD84402D8B9580D0725E8449741B；x64=C8C35CE4872FA659B02F8731F34F1F0492775D30879DEC17E54779F06228BE01。均hardwareVerified=false。本轮没有重新取得测试程序集SHA/MVID，不借用下文旧轮程序集SHA绑定新实验。

### 普通 cubic 与 rounded 上游新增亲审

- resources/pathgeometry.cpp:40..76，GetShapeDataCore取得CMILMatrix，SetPathData保存矩阵，并非直接对double点先预变换。PathGeometryWrapper.cpp:1441..1457保存矩阵指针；639..715普通Bezier分支先对Point1/2/3 ConvertToSingle，再TransformPoints；line同样ConvertToSingle后TransformPoint。quadratic另有SetQuadraticBezier再TransformPoints，不能只凭arc或普通cubic推它。
- resources/rectanglegeometry.cpp:57..144：rect/radii的当前值已是FLOAT；等半径且无非identity矩阵走CRectangle.Set；其他走CShape.AddRoundedRectangle，再由CShape.Transform施加本地矩阵。geometry/compactshapes.cpp:285..357直接以MilPoint2F返回line/Bezier段。
- geometry/shape.cpp:1219..1268：半径0转rectangle，否则fabs半径后InitAsRoundedRectangle。figure.cpp:1180..1239预留17个MilPoint2F，InitBufferWithRoundedRectanglePoints，8段Bezier/line交替并闭合。因此rounded并非arc入口，它有独立float控制点生成合同。

尚未亲审InitBufferWithRoundedRectanglePoints具体算术、CRectangle.Set的全部点生成和候选普通cubic/rounded所有调用点；没有完成用户要求的全族上游闭合。这是明确未完成项，不是工具阻塞。保留生产不改，stroke合同未动。完整HFD/GPU与最终四格仍未完成，本轮只提交实做判别实验及失败证据，不宣称最终验收。


## 本轮可信收口（非生产验收）

仅专用工具及 Windows 沙箱正式 C# exe；未运行终端或脚本。未修改生产、项目、入口、打包、共享交接文件，未改 golden/threshold/skip。所有新产物在 artifacts/be-curve-device-quantization/。AY 归档候选不是当前源码生产验收；完整 HFD/GPU、最终四格 full 仍开。

旧 readback-x86/readback-x64-trx 目录未找到实际 parent TRX（不能以目录名证明存在）。因此本轮从已发布独有 x86/x64/com-acceptance-package 执行现有正式测试入口，重新取得完整 parent TRX 并全文亲读：

- compact-x86/be-parent/be-x86-parent.trx：任务 f449ce6f089d46a69b147399f28e1702，TestRun 74c50fe6-8f42-4ec7-a980-4d3f52f259d3，total/executed=1，passed=0，failed=1，outcome=Failed，exit=2；不是中断。
- compact-x64-parent/be-parent/be-x64-parent.trx：任务 f6c43b8a76194fbbb6f4add69fdcf39b，TestRun 8efc4fed-9b9f-468c-88f5-cf25105b5406，同为1执行、1失败、exit=2。
- 两次 official/candidate 子角色成功仅代表像素生成成功，不能覆盖 parent exact 断言失败。
- 业务根分别 Results/curve-an/cc910a0fa28e44ee9976c87e5bd06893、Results/curve-an/64dc07f1b1644d6dbdce49f054075139。原十帧 comparison.json 两份全文亲读：frame1/3/5各44字节差，frame9为10，其余0；threshold=0，exact=false，hardwareVerified=false。首差分别 offset696/(14,4)/B=36→40 和 offset2432/(8,15)/B=171→179。正式失败完整保留。

## 实际身份

复用现有 WhenDesktopCaptureOfficialIdentityIsRecordedThenModuleIsNotCandidate，无新增入口/Host：identity-x86、identity-x64 中各正式测试实跑通过，Results/hw-module-*.json 已亲读。记录的是实际加载官方路径与实际测试程序集 SHA，不以源码或发布目录名代替身份。

|架构|归档 Native 候选 SHA256（曲线 Results/aot-identity.json）|实际测试程序集 SHA256（identity 实跑）|实际加载官方 SHA256|
|---|---|---|---|
|x86|0FCA4287C3E44CB7A7B0801D297C6D4E85F4AD84402D8B9580D0725E8449741B|099C3C83B76FD3A4E614DB927649256BC17F82BE1EEFD1A5BF580884B7893282|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|
|x64|C8C35CE4872FA659B02F8731F34F1F0492775D30879DEC17E54779F06228BE01|0CC68268FDD7063450FDDA009644423462E5DCEFB9BD14F479BE8ADB2ED138DF|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|

identity 任务 x86=0a38fb2f6d7c418bbc1ba36e52f626f8，x64=78f621de361c488f9fdfb5c910bf1c02。官方实际路径均任务根/wpfgfx_cor3.dll，Requested=Actual；版本10,0,1326,50504，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699，运行时.NET10.0.13。候选路径为曲线任务 Native/wpfgfx_cor3.dll；IntPtr.Size分别4/8。程序集SHA由同一发布包另一次现有身份测试取得，未声称在曲线进程内重新哈希；未取得MVID。Native SHA与AY归档一致，绝不绑定当前源码生产修复。

## 已取得真实 WPF 对照

每根有 be-device-compact.jsonl（180项）和完整 be-device-summary.json（逐差字节）、original/candidate 原始 pbgra32 及控制点/矩阵 JSON。40×32、Pbgra32、96DPI，软件 RenderTargetBitmap，全部非空；不证明GPU。以下是本轮 x86 compact 全部读数；x64本轮已亲读前30项（drawing/float-above），其他x64项尚未逐项复核，不能据此声称180项跨架构完全相同。

模式 R=真实WPF cubic，E=显式设备double控制点cubic，Q=float设备预量化cubic，P=Q后double浮点细分折线。差异单位字节，参考为官方真实cubic；line-sibling独立对官方line-sibling比较，所有x86行两角色均0。

|场景|候选fill R/E/Q/P|官方fill E/Q/P|候选stroke R/E/Q/P|官方stroke E/Q/P|
|---|---|---|---|---|
|negative-below -0.0312501|14/14/0/0|0/0/0|138/138/138/126|0/16/128|
|negative-exact -0.03125|30/30/0/0|0/0/0|138/138/136/126|0/38/126|
|negative-above -0.0312499|30/30/0/0|0/0/0|138/138/136/126|0/38/126|
|float-below -0.03125000001|30/30/0/0|0/0/0|138/138/136/126|0/38/126|
|float-above -0.03124999999|30/30/0/0|0/0/0|138/138/136/126|0/38/126|
|geometry|46/46/0/0|0/0/0|120/120/156/150|0/128/150|
|drawing|46/46/0/0|0/0/0|104/218/218/218|216/214/218|
|visual|46/46/0/0|0/0/0|104/218/218/218|216/214/218|
|nested|20/20/0/0|0/0/0|158/210/214/200|206/204/200|

真实变换不是只有手算：geometry.Transform=local；drawing.PushTransform(local)；visual.Transform=local；nested为geometry.Transform(local)加ContainerVisual.Transform(outer)。local=[0.875,0.125,-0.0625,0.75,2.17,3.09]，outer=[1.125,-0.0625,0.125,0.875,1.13,1.07]，combined=local.Append(outer)。stroke宽1.25，因此父变换stroke与显式变换控制点/原宽pen不等价，官方216/206差本身就是证据；不得向共享stroke盲加量化。

Q模型先把矩阵/控制点转float，矩阵线性项乘16，平移减0.5后乘16，float乘加，最后floor(value+0.5)/16+0.5；不是简单double设备坐标Round。以上零差只支持有限fill样本，尚不能区分geometry本地double预变换再float与父设备float变换，也不能外推完整HFD或所有JIT运算顺序。

## 未覆盖与下一最小实验

未实施生产修复。必须先构造使两舍入路径跨越28.4半格的控制点，现有变换数值没有证明能区分这些路径。下一实验限定本专属文件及既有Render/Compare：同一local矩阵分别施于geometry、drawing、visual；比较 A=double local.Transform后转float，B=原始点和矩阵各转float后乘加，C=local/outer先合成double再float，D=local后float再outer分阶段float。记录各中间IEEE位型、量化整数及真实官方像素；先确认A/B或C/D整数确实不同且对应官方两条预量化曲线像素不相同，否则该样本没有判别力。用负/正半格、相邻double及float ULP邻域，各做fill/stroke/line对照。

本轮已亲审调用链切片：官方 src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/swlib/aarasterizer.cpp:1273..1327 的 TransformRasterizerPointsTo28_4 接收 MilPoint2F，矩阵含×16，以float计算两次乘法和加法；2053处先将四控制点转28.4，随后构造CMILBezier并Flatten，再InitializeEdges。候选 SoftwarePathCoverage.cs:179..189 的arc兄弟路径却是transform.Apply后只用Quantize做有效性检查（out _），把未量化控制点交给SoftwareBezierFlattener.Flatten，再量化输出；408..415的Quantize本身采用double设备计算。此切片支持先量化控制点与仅量化输出的差异，但尚未逐段审完普通cubic/rounded所有调用方及geometry本地矩阵进入MilPoint2F之前的GetShapeDataCore。SoftwareStrokeContours.cs:174..180明确先保存float REAL再widen，Curve方法另调共享flattener；这是不改共享stroke的额外理由。尚未证明所查官方源码与运行二进制每条上游分支严格对应，不能只凭模型命名推定官方float顺序。若独立官方仍无法选择分支，保持不修并提交具体分支。若证据充分再做fill专属最小生产修复，当前源码双架构正常依赖图独有根重发布，保留原44/10、负半格、真实WPF变换和fill/stroke兄弟回归的完整parentTRX与实际身份；本归档包不可替代。
