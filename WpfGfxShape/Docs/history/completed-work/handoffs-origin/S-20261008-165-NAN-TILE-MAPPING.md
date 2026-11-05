# S-20261008-165：ImageBrush NaN 矩形空画刷与恢复

## 本轮实际交付

完整 ImageBrush 遮罩、层消费和 Compose 模块中的生产合同修复；不关闭完整模块。没有确认外部阻塞。本轮没有实现私有路径创建者/消费者、笔画或一般曲线组合。

## 原生证据与修改

- `core/resources/TileBrushUtils.cpp:110-165,180-255`：绝对 Viewbox/Viewport 无效时设置空画刷并在映射计算前退出。
- `core/common/basetypes.h:130-169`：NaN 坐标或尺寸是无效矩形；该函数允许部分无限值，不能据此宣称所有非有限值为空。
- `core/resources/tilebrush.cpp:300-405`：空画刷返回 null realization；正常路径设置来源与 opacity 后释放临时来源引用。
- `Core/GeneratedImageMask.cs`：NaN Viewbox/Viewport、相对内容 bounds 和转换后的 viewport 作为空画刷，在来源 Acquire 和相对变换求逆前返回。保留已有零尺寸空画刷合同及无限值风险边界，没有扩大私有 ABI。
- `CompositionEffectsTests.ImageMask.cs`：在现有71组真实生产协议/直接AOT消费者中逐个更新 Viewbox/Viewport 的八个分量为 NaN，验证 Present 成功、透明输出，再恢复原值验证下一帧原像素。复用已有来源锁失败、丢弃恢复、tile/DPI、相对映射、geometry/scoped/solid 消费与最终释放链；未新增测试专用导出。

## ABI、所有权和错误语义

保持 Stdcall、HRESULT 首错及确定性清理；空矩形不 Acquire 来源，不把未知类型或无限值的失败隐藏为空画刷。无私有对象布局变更；非IUnknown对象仍不得用通用COM三槽访问。

## 每次发布和验收

共同命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
共同目标：生产 csproj，Release，win-x64；产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

1. 发布#1成功，工具报告11行日志；测试前未独立读取保存，不能宣称无警告。恢复279/279、0失败/跳过、267ms；TRX `d3326709-85d2-41c5-84b7-f478a602737e`，开始2026-10-08T18:35:34.5904853+08:00；对应SHA256 `769340DCA97902F559DB372C3A19F16967E20E691745D914985F12972FE7BB30`。
2. 修改验收、未修改生产DLL取得Red：定向71失败/0通过、90ms，NaN viewport宽度在Present处返回E_NOTIMPL（-2147467263）。完整Red TRX未独立保存，后续覆盖。
3. 发布#2成功、退出码0，可见CA1416。工具保留409681字符输出末尾20000字符，共59行，前389681字符不可见；已在后续测试前读取前12行，摘要另存 `S-20261008-165-directional-evidence.txt`，不是完整原始日志。
4. 修复后定向71/71、0失败/跳过、72ms；TRX `c79d5fc6-e9ab-402d-a3ee-6b7a45f8a685`，开始2026-10-08T18:38:58.9943341+08:00。仅宽度NaN断言阶段；完整TRX未复制，身份与结果已在全量运行前独立存入摘要。
5. 扩展八分量断言后全量279/279、0失败/跳过、293ms；TRX `def1e012-e9a5-4c0f-a759-1152c60a7ffc`，开始2026-10-08T18:39:58.7684162+08:00；对应发布#2 DLL SHA256 `507194BD8198723316D27F707B374A13F45A6E9E1E45F7FCFB5A55448EDDA50A`。完整TRX保留在 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，没有声称另存完整副本。可见既有MSTEST0037/MSTEST0044。

## 下一轮与未完成边界

继续完整模块：真实shape/pen/context及resource/immediate realizer创建者、MSVC布局和借用/持有合同→有限/无限路径全部参数→EnsureRealization/空画刷→fill/Widen/stroke共同扫描→Begin/End/Abort、mask/clip/opacity/Pop与Compose成功失败及释放生命周期→一般曲线精确组合/非轴保持操作数、动画和浮点/预滤原生差分联合验收。NaN与空线/空路径仅作为回归，不安排独立补测轮。

阶段4 Active，阶段1–3正式关闭、5–7 Pending；Ledger仍RepairRequiredPartial。未运行覆盖率、原生差分、PresentationCore E2E或Ledger修复/验证；不能替换原DLL。
