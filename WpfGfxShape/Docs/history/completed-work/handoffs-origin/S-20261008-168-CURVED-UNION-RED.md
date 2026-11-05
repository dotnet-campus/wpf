# S-20261008-168：曲线 Union 修复前证据

本文件在后续发布与测试覆盖日志/TRX前保存；仅为可用证据摘要，不是完整原始日志或TRX副本。

- 发布 #1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`；项目 net10.0，Release/win-x64，退出码0。立即读取完整可用11行日志：还原最新、托管输出与发布目录，无可见警告。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。本次未独立取得SHA256，不沿用历史哈希冒充本次结果。
- 初始直接生产AOT全量：281通过、2失败、0跳过，共283，305ms。两项operandKind=8的普通/外层仿射Union在Present返回-2147467263（E_NOTIMPL），失败行398。
- TRX：`79b16068-6ff9-4709-a5fb-832c8951e710`；开始 `2026-10-08T19:03:10.4244389+08:00`。默认文件 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx` 随后将被覆盖。
- 根因：GeneratedGeometryBounds将所有CombinedGeometry强制交给仅支持矩形区域的提取器，拒绝真实椭圆操作数。
- 原生证据：`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/combinedgeometry.cpp:35-135`，取得当前操作数、null解释为空、取得共同矩阵并Combine，清理临时shape；本次读取101行。
- 修复范围：Union递归取得两操作数共同仿射后的已有曲线极值；不扩展Intersect/Xor/Exclude为包围框近似，不修改原WPF、ABI、导出或引用所有权。
