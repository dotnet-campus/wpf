# BW curved fill precision：已有实跑收口

## 1. 范围与结论

- 根目录：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`。证据全部位于独有 `artifacts/bw-curved-fill/`；下文路径均相对此目录。
- 本次只亲读已有执行证据并收口，不重新执行实验，不恢复生产。当前本次生产改动为零；未改 BX 捕获、入口、共享交接或生产实现。
- 双架构、BR 与 BU 撤回基线均有真实非空反例：rectangle 64、ellipse 32 个不同字节；不是像素数，阈值始终为 0。
- 同一架构内使用同一当前测试程序集比较 BR/BU：8 项 stroke 逐字节未变，但它们仍与官方存在差异，不能称 stroke 已修复。
- 已亲读此前未核 x64 补强 oracle 的五份 evidence.json：5/5 nonempty=true、differences=[]、threshold=0；监督退出 0，未超时。此结论只覆盖五个样本，不等于完整 HFD、GPU 或最终四格验收。

## 2. 双架构真实参数与反例

源码：`Tests/WpfGfxShape.ComAcceptance/BwCurvedFillPrecisionTests.cs`，`RenderContent`。

- RectangleGeometry 与 EllipseGeometry 使用同一 Rect：X=1、Y=1、Width=2^-20=0.00000095367431640625、Height=16。
- local Matrix(M11,M12,M21,M22,OffsetX,OffsetY)=(1.000000059604644775390625,0,0,1,-1,0)，即 M11=1+2^-24。
- parent Matrix=(1048576,0,0,1,8,0)，即 M11=2^20。
- geometry.Transform=local；DrawingContext.PushTransform(parent)；Brushes.Blue、pen=null。
- RenderTargetBitmap：32×24、96×96 DPI、Pbgra32、stride=128，完整缓冲区 3072 字节；以 alpha 非零检查非空。软件路径，hardwareVerified=false。

| 架构 | 候选身份 | rectangle 不同字节 | ellipse 不同字节 | 两项 nonempty |
| --- | --- | ---: | ---: | --- |
| x64 | BU baseline | 64 | 32 | true |
| x64 | BR archive | 64 | 32 | true |
| x86 | BU baseline | 64 | 32 | true |
| x86 | BR archive | 64 | 32 | true |

原件：`run-x64/br-x64/Results/bw-precision/comparison.json` 和 `run-x86/br-x86/Results/bw-precision/comparison.json`，两份均 exact=false、threshold=0。各目录 official/baseline/br 下保留原始 .pbgra32、参数 .json、identity.json 和子进程 supervision.json；非空反例来自真实官方渲染，不是候选算术自证。

## 3. BR/BU 同程序集 stroke 八项逐字节对照

原件：两架构各自 `Results/bw-stroke/comparison.json`（位于上述 run-x64/br-x64、run-x86/br-x86）。两份均 unchanged=true、threshold=0；下表数字双架构相同。changes 是 BR 与 BU 候选完整 Pbgra32 缓冲区的不同字节数；officialChanges 是两次独立官方缓冲区的不同字节数。

| 场景 | BR/BU changes | officialChanges | BR 对官方 | BU 对官方 | 四份缓冲区 nonempty |
| --- | ---: | ---: | ---: | ---: | --- |
| arc-stroke-identity | 0 | 0 | 128 | 128 | true |
| arc-stroke-nested | 0 | 0 | 122 | 122 | true |
| cubic-stroke-identity | 0 | 0 | 108 | 108 | true |
| cubic-stroke-nested | 0 | 0 | 120 | 120 | true |
| ellipse-stroke-identity | 0 | 0 | 190 | 190 | true |
| ellipse-stroke-nested | 0 | 0 | 200 | 200 | true |
| rounded-stroke-identity | 0 | 0 | 104 | 104 | true |
| rounded-stroke-nested | 0 | 0 | 94 | 94 | true |

同一比较中的八项 fill，BU 对官方依次 arc identity/nested=22/4、cubic=10/12、ellipse=8/40、rounded=24/32，BR 对官方均 0，官方重复输出变化均 0、全部非空。这只能证明该有限样本的历史 BR fill 改善，不能推翻本报告新的窄图元反例。

## 4. 父执行、TRX 与监督

所有执行使用正式纯 C# exe；本次收口未使用终端或任何脚本。

| 执行 | 父监督目录 | UTC 起止 | supervisor/pid | 测试结果 total/pass/fail/skip | 退出 |
| --- | --- | --- | --- | --- | --- |
| x64 BW | run-x64/br-x64/Results/bw-x64 | 2026-10-20 05:28:39.3771816—05:28:50.3184342 | 5476/7444 | 2/1/1/0 | 2 |
| x86 BW+oracle | run-x86/br-x86/Results/bw-x86 | 2026-10-20 06:27:26.2839059—06:28:32.5953031 | 6260/6852 | 7/6/1/0 | 2 |
| x64 oracle 补强 | run-x64-oracle/Results/bw-x64-oracle | 2026-10-20 06:29:49.2822801—06:29:50.2386858 | 2752/8816 | 5/5/0/0 | 0 |

- 三份 supervision.json 均 Reason=exited、TimedOut=false、Cancelled=false、KillAttempted=false、HasExited=true、OutputCollected=true、OutputDrainTimedOut=false、SupervisorError=null；timeout=120 秒。
- 父 filter 分别为 `FullyQualifiedName~BwCurvedFillPrecisionTests`、`FullyQualifiedName~BwCurvedFillPrecisionTests|FullyQualifiedName~BrFillIntegerTests`、`FullyQualifiedName~BrFillIntegerTests`。
- 双架构失败均为 `WhenNarrowPrimitivesAreTransformedThenIndependentOfficialContentMatches` 的 exact 断言；归档 stroke 对照通过。x86 另外五个 oracle 通过。
- 父 stdout 与监督 Arguments 均记录 `--report-trx --report-trx-filename child.trx --results-directory <父监督目录>/TestResults`。实际远端前缀如下节。
- 证据缺口：本地回收的这三个父目录及现有双架构 Results 中未找到 .trx 文件。不能将 stdout 声明的生成位置冒充亲读 TRX；上述计数来自亲读父 stdout，退出/身份来自亲读监督，业务结果来自逐项 JSON。父 TRX 尚未本地核验，不宣称已完成该项。

## 5. 实际加载身份

共同 runtime=.NET 10.0.13，MSTest=4.0.1；x64 pointerSize=8、x86=4。每个角色 identity.json 的 requested 与 actual 完全相同，均为实际加载模块路径，不以磁盘目标代替加载身份。

远端任务前缀（C:\CodingAgentSandbox\Tasks\ 下）：

- x64 BW：`882b796e0c5a4e5a8a7fc901fa87b7cf`。测试程序集 `<任务>/br-x64/WpfGfxShape.ComAcceptance.dll`。
- x86 BW：`b5b7dd6aa2b0446aa688c50e6709f4f1`。测试程序集 `<任务>/br-x86/WpfGfxShape.ComAcceptance.dll`。
- x64 oracle：`6cc3e263568545dcab509d284237359b`。测试程序集 `<任务>/WpfGfxShape.ComAcceptance.dll`；实际官方 `<任务>/wpfgfx_cor3.dll`。

| 身份 | 实际模块相对任务路径 | SHA256 |
| --- | --- | --- |
| x64 官方 | br-x64/wpfgfx_cor3.dll | AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1 |
| x64 BR | br-x64/Native/wpfgfx_cor3.dll | 2C7529F5BC1DB2BA851DFA58703F2FEDC0815E6FBD96604BE9050B009ACAB57C |
| x64 BU | baseline-x64/Native/wpfgfx_cor3.dll | 8D48ADBB30D24BD9CEAF4505969126450B52034BCD3726204B15C6EF126FBB79 |
| x86 官方 | br-x86/wpfgfx_cor3.dll | 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834 |
| x86 BR | br-x86/Native/wpfgfx_cor3.dll | 6A681983165861B1B5C2596EBC35A7C8A4832F5A938C41E4A6C74152185C38F4 |
| x86 BU | baseline-x86/Native/wpfgfx_cor3.dll | E301EED4262C3709AB7B8B604117067CFD28E24CB674551D0C9DBD07099CE111 |

官方版本均 `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`；BR/BU 均 1.0.0.0。x64 补强官方 SHA 与 x64 BW 官方相同。

- x64 测试 SHA256=`0594938549E60CE4895EF3272183C82A37667F31FD6E5DF40CC7812D1E7D4F87`，MVID=`fa73203b-5210-458b-b515-3a4ebd8290e2`；BW 与补强 oracle 相同。
- x86 测试 SHA256=`59B6717D7C33B508AEE429E51856D9EEE271642BF7AC0DFC0F959451C8A8DA12`，MVID=`20dee6c8-0cc2-4b41-b062-64c617d27c1f`。
- “同程序集”指同一架构内 BR/BU 对照，不指 x64/x86 二进制相同，也不指下节收口后源码已经重新发布。

## 6. 亲读 x64 补强 nonempty oracle

`run-x64-oracle/Results/br-integer-oracle/` 下逐份读取 evidence.json 的结论和身份字段，而非只读 stdout：

| sample | 独立执行目录 | differences | nonempty | threshold |
| --- | --- | --- | --- | --- |
| 0 | 0/e9c0983d076c40bdb9b92169da4f6ab9 | [] | true | 0 |
| 1 | 1/a585cf6767fb487199c48ee701b2a4c8 | [] | true | 0 |
| 2 | 2/ec64ce5523bf4b878da77546da165bfb | [] | true | 0 |
| 3 | 3/09e0646520514ff5a59503ee1169326f | [] | true | 0 |
| 4 | 4/06f4ef51c6cb454f841d463bc91364f0 | [] | true | 0 |

源码 `BrFillIntegerTests.Compare` 的五组原始 cubic 控制点依次为：

1. (4,28),(4.04,4),(36,4),(36,28)。
2. (4,28),(14,24),(24,24),(36,28)。
3. (-2,24),(-0.03125,-4),(30,-4),(34,24)。
4. (4,28),(-1024,-2048),(2048,-1024),(36,28)。
5. (4,28),(4.03125,4),(35.96875,4),(36,28)。

这是独立官方 cubic 与 integer-lines 的完整字节比较，不是 BR/BU 窄 rectangle/ellipse 的修复验证；五份均实际官方加载、hardwareVerified=false。

## 7. 默认 MSTest 门检查与本次测试收口

- 已读 csproj（net10.0-windows、Nullable enable、MSTest 4.0.1）和 acceptance.runsettings；SDK 默认编译 .cs，runsettings 无排除过滤。原 Bw 类的两个 [TestMethod] 会被默认 full 发现：一个精准已知失败，加一个重复归档批量实验。不能称默认 full 不受影响。
- 本次仅移除 `WhenArchivedStrokeRunsThenPerSceneChangesAreRecorded` 及只供其使用的 Copy 辅助方法：这是依赖相邻历史 BR/BU 包、再启动整组 sibling 实验的归档比较，不应成为默认 full 的重复批量错误门。历史实跑文件与失败证据全部保留，未删除精准失败。
- 保留 `WhenNarrowPrimitivesAreTransformedThenIndependentOfficialContentMatches`，明确为已知产品失败；保留完整官方/BU/BR 内容比较、nonempty 与 threshold=0，未 skip、未放阈值、未删除失败断言。默认 full 仍应如实暴露这个精准失败，不宣称 full 通过。
- 保留回归的后续输出改到包目录内 `artifacts/bw-curved-fill/precision/`，不修改共享 Results 规则、BX 或入口。旧已跑证据保持原位，不覆盖。
- 本次收口后的测试源码与上述已跑程序集不是同一发布版本；历史 2/7/5 个计数不得冒充删去归档实验后的新跑计数。此次不重新实跑。已用专用构建验证该测试项目：构建失败，92 个警告、1 个错误；唯一错误为未改文件 `ExitDiagnostics.cs(24,37)` 的 CA2208（RunAsync 的 ArgumentException 参数用法）。未修改该共享诊断文件、未压制规则；不能宣称本次构建通过。

## 8. 后续修复精确入口（本次不修改）

- 复现：`Tests/WpfGfxShape.ComAcceptance/BwCurvedFillPrecisionTests.cs` 的 `WhenNarrowPrimitivesAreTransformedThenIndependentOfficialContentMatches`、`RenderContent`，使用第 2 节原值；rectangle64/ellipse32 在双架构及 BR/BU 均已实跑。
- 首查 `Code/WpfGfxShape/Core/GeneratedImageTransform.cs`：`TryResolve`、`Prepend`、`Apply(double,double)` 的 local/parent 合成及 float/double 精度边界；当前 Prepend/Apply 使用 double，不能未经实跑便随意全局改 float。
- 椭圆入口 `Code/WpfGfxShape/Core/SoftwarePathCoverage.cs`：`TryCreateEllipse`、`AppendEllipse`（local/parent 合成、控制点生成、Quantize 前精度）；同时核查该文件 rectangle 路径及 `SoftwareBezierFlattener.Flatten`，避免只改一个图元掩盖共同精度问题。
- 当前证据定位的是变换/图元精度排查范围，不是已确认唯一根因。修复必须保持独立官方非空 oracle、双架构完整内容比较及八项 stroke 对照；未通过前不得恢复生产。
