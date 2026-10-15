# 无画刷 Pen 续接：发布与初始失败证据

## 发布 #1

- 命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
- 项目 Code/WpfGfxShape/WpfGfxShape.csproj；Release / win-x64；退出码 0。
- 立即读取全部可用 60 行日志。原输出 409681 字符仅显示末尾 20000 字符，不是完整原始诊断；可见 CA1416、CA1822，Native AOT 生成及发布成功。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 首次测试传目录被拒绝，未执行；随后直接按 csproj 全量验收：284 通过、1 失败、0 跳过，总计 285，325ms。
- TRX：`fad321e4-2a05-4a9c-afb8-05059db11b20`，start `2026-10-08T19:15:13.7662418+08:00`。
- SHA256：`06D06BAEE8977E032659F89FA27BAE09FA093FA84A21A8520667E751277F87AC`，取自该 TRX。
- 无画刷 Pen 的实色作用域场景不再在 Present 返回 E_NOTIMPL，但恢复像素预期 `(0, 0xffff0000)`，实际 `(0, 0x40400000)`，失败于 ImageMask.cs:229；普通 ImageBrush 场景通过。
- 本文件在后续测试覆盖默认 TRX 前保存摘要，不是原始 TRX 或完整日志副本。

完整 ImageBrush / realizer / Widen / 层 / Compose 模块仍未关闭；本轮先诊断当前失败，不把单个回归通过等同于模块关闭。
