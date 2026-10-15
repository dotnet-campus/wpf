# 原版 WPF 行为基准采集

用户本轮明确要求新建空 WPF 项目，通过 DLL Import 对接原版 wpfgfx_cor3.dll，为现有单元测试预期提供独立证据。新增 Tests/WpfGfxShape.WpfReference，不恢复旧 Host、不修改 .slnx、不修改原 WPF 或生产代码；生产 COM 验收仍仅 Tests/WpfGfxShape.ComAcceptance。

## 验证

新项目最终直接测试成功：2 通过，0 失败，0 跳过；采集 135 组 tile/flip/scaling/fill/stroke/mask 原版样本。初次构建发现 IO using 与 DrawingImage.Bounds 错误并修正；禁用并行的程序集属性位置错误亦已修正。最终无上述错误和 MSTest 分析器警告。

证据文件：Tests/WpfGfxShape.WpfReference/TestResults/original-wpf-cae82710033d408886b8f8833c9a2219.json。

- Runtime：.NET 10.0.12，X64。
- 原版 DLL：C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\10.0.12\wpfgfx_cor3.dll。
- SHA256：501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A。
- 版本：10,0,1226,42308，commit 95017c711e6afc1085133d440e42b4bd78155701。
- 直接导入 MilUtility_GetTileBrushMapping；顶层由原版 WPF RenderTargetBitmap 软件路径采集 Pbgra32。

本轮没有发布 Native AOT，不产生新的生产 DLL SHA256 或生产 TRX 基线；没有复验既有 890 项，也未修改预期值。样本文件并非 TRX。

## 未关闭边界

采集成功不等于生产行为一致。尤其 mask 层级还需逐项审核；Pbgra32 无法证明 float/HDR 的 <1e-7 断言。下一步将相同输入与同格式输出配对比较，差异须区分输入不一致、颜色转换/量化与生产算法错误，不可回填翻译实现自身结果充当原版 golden。

阶段状态和替换资格不变。本切片仅建立原版证据通道，不宣布原生中间面方法族或整体替换完成。
