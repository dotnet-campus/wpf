# S-20261008-176：开始发布及覆盖前基线

## 发布 #1

- 命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
- 项目 WpfGfxShape，Release，win-x64；发布工具报告成功，可用日志11行。本轮未在测试前读取发布日志，测试已覆盖最后日志，不能声称已归档完整发布诊断或无警告。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 随后直接按 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj` 测试成功：286/286，0失败/跳过，304ms。
- TRX：`0bcf8a58-25ea-4d0a-a8e2-dc199e53727d`；开始 `2026-10-08T19:21:45.2506665+08:00`。
- TRX实测SHA256：`06D06BAEE8977E032659F89FA27BAE09FA093FA84A21A8520667E751277F87AC`。
- 当前TRX默认路径 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；本文件在下一次测试覆盖前保存摘要，不是完整TRX副本。

## 工作范围

原生证据核实 `resources/pen.cpp` 43–110、`geometry/cpen.h` 73–112、`geometry/shapebase.cpp` 179–200、`sw/swlib/swsurfrt.cpp` 836–885：厚度先由动画覆盖再转float，零宽/高Pen的Widen为空，填充先于Widen/stroke。当前generated绘制与内容bounds仅按Brush非空拒绝Pen，未区分float零厚度。下一步TDD验证真实协议零厚度（包括float下溢及动画覆盖）与遮罩层失败恢复。

这不是完整私有ABI或非零笔画闭环；不得据此关闭完整模块。阶段4 Active，Ledger RepairRequiredPartial，不能替换原DLL。
