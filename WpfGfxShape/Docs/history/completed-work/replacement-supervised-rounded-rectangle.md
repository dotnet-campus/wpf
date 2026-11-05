# AK 圆角中间代码质量门与像素差异根因

## 接手状态（AH，未验收）

AH 留下 WindowRoundedRectangleGeometry.cs、GeneratedHardwareRectangle 圆角 GPU 分派及独有 Round 测试。支持范围拟为有限 SourceOver、单位缩放平移、不透明 solid；未支持输入明确失败。最后 bounds 合并后未构建。既有 x64 managed 4/4 仅覆盖 coverage，不证明 GPU 执行。

AH COM RTB 独立对照官方 36、候选 40，首差字节 696；证据 `artifacts/rounded-ah/readback/com-x64/Results/rounded-ah/a92f0ed762d5432298eaf602e7faf7e7`。这是软件 RTB 合同差异，尚未归因，不能归于新 GPU 实现，也不能声明 GPU 通过。

## AK 执行边界

只修改圆角/覆盖相关生产与 AH 独有测试；不修改 AJ 的 BitmapRenderTargetExports.Drawing.cs、NativeImmediateSolidRealization.cs，不修改通知、上传、捕获、项目、入口、skip/golden/阈值/fallback。纯 C#，两个正式测试项目，x86/x64 Release；专用工具构建/发布与 Windows 沙箱运行。输出使用独有 ignored artifacts/rounded-ak，保留其他执行者产物。

## 本轮状态

AK 已有证据位置：`artifacts/rounded-ak/readback/com-x64-final`。按 AN 接手输入，矩形、普通圆角及其对应 Bezier 一致；clamped 圆角、椭圆及其对应 Bezier 均有 44 字节差异，首差像素 (14,4) 的 B/A 为官方 36、候选 40。原始差异必须保留，不能更换参数追绿。以上为交接事实，AN 尚需回读原始证据独立核实。

AN 仅调查共享曲线软件 coverage 与 AK 独有测试，不修改 AJ/AM Bitmap 生产/夹具、通知、图像、捕获、入口或项目，不修改 skip/golden/阈值/fallback。控制点先转 28.4 再整数 flatten 仅为待验证方向，不是根因结论；不能确定则保留失败。x86 必须使用已验证 .NET 10.0.13 self-contained 正式包（含 identity），不可依赖沙箱 runtime。最终须记录双架构 Release 构建、实际定向回归、TRX/identity/原始差分回读。

新 GPU 圆角尚无 backend 内容证据，仍未覆盖；任何软件回归结果不得代替 GPU 证明。

## AN 有界实验与最终结果（未修复，保留失败）

仅修改本文及 `Tests/WpfGfxShape.ComAcceptance/RoundedRectangleContractTests.cs`。未修改生产算法、项目、入口、AJ/AM 文件或 auto-generated 文件，未修改 skip/golden/阈值/fallback，未清理其他产物。

读取官方 `core/sw/swlib/aarasterizer.cpp:2053-2063`：先将四个控制点 `TransformRasterizerPointsTo28_4`，再 `CMILBezier.Flatten`。`core/geometry/bezier.h` / `bezier.cpp` 使用固定点 HFD（初始 18.14、稳定态 15.17、1/4 tolerance）。候选 `SoftwarePathCoverage.cs` 的 Arc 与 Bezier 分支先调用 double `SoftwareBezierFlattener.Flatten`，然后量化输出点。该顺序差异是源码事实，但尚未独立证明它完全解释原 44 字节差异，不能将 double 算法前简单取整当作已证实修复。

新增独有测试 `WhenCurveControlPointsCrossQuantizationBoundaryThenIndependentOfficialPixelsMatch`，独立官方与候选角色仍通过真实 RenderTargetBitmap 输出。原六帧参数和断言保留；额外四帧为闭合单 cubic：起点 (4,28)，第一控制点 (x,4)，第二控制点 (36,4)，终点 (36,28)，x 分别为 4、4.001、4.02、4.04。前三者的第一控制点量化 X 为 56，第四为 57。交替红蓝保证新内容，alpha 单独比较避免颜色干扰。参数、原始 pbgra32、逐字节差分及 control-alpha.json 均落盘。

两架构独立结果一致：新增帧 6/7/8 官方与候选逐字节相等；帧 9 有 10 字节差，首 (8,15) B/A 官方 171、候选 179。官方帧 6→9 有 7 个 alpha 像素变化，候选仅 2 个；帧 6→7/8 两边都不变。这是可区分合法控制点实验，支持调查栅格离散化，尚不能排除整数 HFD 舍入/步长等贡献，也未建立原曲线的完整因果闭环。因此未进行猜测性生产修复。

### 最终构建、运行、回读

- 最后测试源码后，两正式测试项目均经专用发布工具 x86/x64 Release 编译发布成功；全部 self-contained，RuntimeFrameworkVersion=10.0.13。生产项目随后单独 win-x86 / win-x64 Release 构建成功。无生产改动，正式 COM 包复用 AK 原生 DLL，不重发扰 hash。
- 正式 COM 包：`artifacts/curve-an/package-x64`、`package-x86`，包含 Native DLL、aot.sha256、package-files.txt；实际角色 identity 已回读。
- 实际沙箱：managed 圆角各 4/4 通过；COM 原圆角合同与新增实验各架构 0/2 通过、2 失败。TRX Counters 已回读。managed 通过不证明官方像素相等或 GPU。
- 最终回读根：`artifacts/curve-an/readback/{com-x64-final,com-x86-final,managed-x64,managed-x86}`；各 TestResults 内 `curve-an-*.trx`。
- x64 原合同：`Results/rounded-ah/7abe002e15a64cf8996258eae11369ca`；实验：`Results/curve-an/0ab5ca3a8f574d02af80b35bce448500`。
- x86 原合同：`Results/rounded-ah/7f7c28e06db148f1846ff9dab797e862`；实验：`Results/curve-an/b69b43aadf1f48c6ac9ae626ad49f53a`。
- 两架构原帧 1/3/5 仍各 44 字节差，首 offset 696、(14,4) B/A 官方 36、候选 40；帧 0/2/4 零差。comparison、difference-9 全部 10 字节明细及 control-alpha 已回读；原始 pbgra32 随包回收，由 C# 直接读取生成差分，不是候选自产预期。
- 实际运行 .NET 10.0.13，官方 WPF DLL 10,0,1326,50504，commit 4099f83977f4d29604a9aeb971fb0a7bc1d5f699。
- x64 候选 SHA256 `C8C35CE4872FA659B02F8731F34F1F0492775D30879DEC17E54779F06228BE01`；官方 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`。
- x86 候选 SHA256 `0FCA4287C3E44CB7A7B0801D297C6D4E85F4AD84402D8B9580D0725E8449741B`；官方 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`。

过程中解决方案级 RID 构建因 NETSDK1134 失败，改为项目级构建；首次 managed 发布设置全局 StartupObject 导致被引用库 CS2017，去掉该设置后使用既有自动入口发布成功，未修改项目。初次 x64 实验回读保留在 com-x64，正式结论仅以上 final 两架构结果。

**结论：共享曲线软件覆盖差异未闭；无生产修复、无完整替换结论。下一步需要独立对照控制点量化后复用既有 HFD 与官方整数 HFD 的输出，分离预量化、整数舍入与自适应步长贡献，并仍保留原 44 差场景。新 GPU 圆角继续未覆盖，不能由本轮软件结果替代。**
