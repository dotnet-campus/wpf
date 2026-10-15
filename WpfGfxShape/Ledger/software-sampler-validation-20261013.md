# Software sampler validation — 2026-10-13

状态：未收口；不是 Stage4 完成报告。

## 本轮实际修改

CompositionEffectsTests.SoftwareShader.cs 的 Drawing source 公共 COM 测试资源链修正：
- 原先将 RenderData 作为 DrawingImage.Drawing，且 DrawingImage 资源类型错误（87 为 GeometryDrawing）。
- 使用 ImageDrawing（89）承载 bitmap，DrawingImage（59）引用 ImageDrawing。
- DrawingImage 命令改为原生 0x71；原 0x88 非 DrawingImage 命令。
- expected 像素不变；未增加未执行测试；未 skip。

## 实际运行与回读

1. 初次筛选 SoftwareShader：没有匹配测试，不计通过。
2. WhenSoftware：35 项，34 通过、1 失败、0 skipped。Drawing source 在 commit 返回 -2003303421。
3. 修正后 WhenSoftwareGenericSampler：5 项全部通过、0 skipped；包含 null、Visual、Cache、普通 Image、DrawingImage 两 sampler 实际 offscreen 像素。
4. COM full：1339 项，1338 通过、1 失败、0 skipped，耗时 4m32s。
   - TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx
   - 实际回读失败：WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements。
   - executionId：43df26b3-1fdd-4175-9673-3376630a2fd2。
   - calibration：wpf-calibration-9cc068a9fef240a2a00cb07f4d89797a.json。
   - source=17 affine=3：45 comparisons / 31 mismatches。
   - source=18 affine=3：45 comparisons / 26 mismatches。
   - Direct Pbgra32：180 comparisons / 0 mismatches / 57 format verdict differences。
   - 未替换 golden，未将失败转换 skip。
5. 普通 full：4197 项全部通过、0 skipped，耗时 2s。

## 证据限制与 remaining gaps

本次没有重新 publish，因此上述 COM 结果不能宣称属于本轮新 publish SHA；没有生成同产物普通/COM full 独立 RunID/SHA 归档。TRX 为现有固定输出位置，会被后续运行覆盖。

仅本轮发现并修正 1 个失败用例的 malformed Drawing 资源链；不能声称完成所有新生命周期测试。

仍待完成：
- source mutation/delete/callerRelease、target lock failed retry 的软件 sampler 定向覆盖矩阵与独立固定 expected。
- padding/Ddx scope/nearest-linear 定向验证。
- 首 external COM 前软件/hardware 共用的 FreezeAllValues capture-then-realize 值冻结 contract；本轮代码文本搜索未找到 FreezeAllValues 名称，但不能据此断言不存在其他冻结实现。
- bitmap lease、绘制 locks、owned refs 的 finally reverse cleanup 与首错保存审计。
- shared resource generation/per-device shader cache、hardware malformed notify fallback/OOM 原生范围验证。
- 新 publish SHA、精确定向发现数、同产物 report RunID 独立归档与 mapping/stable work item ledger。
- 真实窗口与 lockscreen 尚未本轮分别审计，不计通过或 skipped。
- float57 / calibration 监督继续；Stage4 未完成。
