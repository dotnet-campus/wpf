# Sampler 协议现有产物真实全量复验（未关闭）

## 实际执行与产物身份

本轮未修改生产代码、未重新发布、未启动终端/脚本/自定义程序/沙盒执行器。现有发布由用户交接，发布命令与日志未在本轮取得，不虚构发布序号。

实际 COM 全量：Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj；退出码 1。产物 d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll。
SHA256：BB8B9B9B504C8903275C4C98D4F28119359201B79F2A339D3CFF96457B967934。
RunID：c4f48c60-b6a8-4100-b5b1-a736be341dd7。
TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx（会被后续运行覆盖）。
运行时间：2026-10-13 09:16:18 至 09:20:49 +08:00。
Counters 回读：1300 executed，1275 passed，25 failed，0 skipped，0 aborted，0 timeout。

实际普通全量：Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj；退出码 0；4197/4197，0 跳过。该测试使用 Debug 托管产物，不将其冒称 AOT 同 SHA 验证。

## 失败分类与独立报告

本轮不能沿用“八例锁屏失败加旧 FLOAT”的历史结论：TRX 中没有 WhenPublic* 失败，公开 HWND 参数实例实际有 Passed 记录。历史 LockScreenBackstopFrame/PresentEx Occluded 证据仍只属于 S-20261012-246，不证明本轮被锁屏阻塞。

25 个失败中，24 个属于 glyph 像素族（包含 WhenDirectDrawGlyphRunIsPresentedThenWholeSurfaceMatchesIndependentOriginal），另一个为 WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements。GlyphPixelTests.Run 第 290 行整面比较实际失败；典型索引 1302 expected 189、actual 151；半透明 expected 95、actual 76；重叠 expected 236、actual 211。根因尚未确认，不能称环境问题、既有已解释失败或修复。需要独立核对 glyph 原版 golden、实际 font/rasterization 状态与生产合成路径，不改 golden/阈值隐藏失败。

独立 calibration report：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-3d0305860a0f41339450544c113ee2a6.json。
已回读 report 完整 SHA 与上述 AOT 一致；Compared 450，Mismatches 57；DirectCompared 180，DirectMismatches 0；AotBitmapMismatches 0。旧 FLOAT 57/450 与 glyph 新发现失败分开记录。

## 当前生产缺口（源码实际读取）

GeneratedHardwareShaderSampler.Render 仍仅接受 GeneratedImageBrushResource + GeneratedD3DImageResource；TileMode 非 None、opacity 非 1、非正轴对齐 transform 返回 E_NOTIMPL。普通 bitmap/drawing ImageBrush、VisualBrush snapshot、BitmapCacheBrush 未完成通用 realization。
GeneratedHardwareShaderEffect.Render 在 bind 前创建 secondaryInputs，finally 逆序 Dispose；已有 shader/scene/texture cleanup 首错保留，但完整通用 ownlease、cache 身份/变更失效/替换释放、公开非恒等第二输入固定输出及真实资源寿命验收未完成。

下一工作必须复用 GeneratedImageMask 的真实映射/源租约；Visual snapshot 必须从实际 Capture/DrawContents 遍历构造，不假设 DrawingSnapshot.TryCapture 接受 Visual。BitmapCacheBrush 必须实现真实目标身份、失效与释放，空 target 使用 1x1 transparent。完成所有 realization 后才 bind，并验证失败逆序 cleanup 与原生 firsterror。

阶段4仍 Active，本轮只完成现有产物复验，不是 sampler realization 交付，不具有替换资格。未执行代码后 publish、定向非恒等 shader 验收，不能标 Closed。
