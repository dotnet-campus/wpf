# S-163 验证证据摘要

## 发布 1

完整可见日志独立保存于 S-20261008-163-publish-01.log。
Release/win-x64，退出码 0；初始全量 271/271、257ms。
TRX 0e542b9b-6777-4f67-a5fa-6e8389da2785，开始 2026-10-08T18:11:02.4138131+08:00。
DLL SHA256 0DE97F7F7F075E2A1B3B54560E6C6B705F3ABC0CF1C1696FC9BB49EAC2FD3267。

## Red

发布 1 DLL；定向 filter FullyQualifiedName~WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping。
63 通过、4 失败、0 跳过、72ms，退出码 1。
operandKind=6 四种组合模式均在 present(connection) 期望 0、实际 -2147467263 (E_NOTIMPL)。
测试编译可见 MSTEST0037 / MSTEST0044；没有编译错误。
Red TRX 未独立保留，未取得其运行 ID。

## 发布 2

完整命令: dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64
项目 WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，TFM net10.0，配置 Release，RID win-x64。
退出码 0、成功，日志总计 59 行；工具输出截断：409681 字符仅保留末尾 20000 字符，不能视作完整诊断归档。
读取了全部 59 行可见日志；可核实诊断为 CA1416（ImageMask.cs、SolidMask.cs、GeneratedVisualRenderer.cs 等 Windows-only API）。未声称无警告。
可见末尾：Generating native code；创建 native/wpfgfx_cor3.lib、wpfgfx_cor3.exp；发布成功。
产物: d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll。
本文件仅保存诊断摘要，不冒充完整原始日志。

## Green 定向

同 filter，退出码 0，67/67、0 失败/跳过、68ms。
TRX 6ec2f3e1-3cb0-41bd-9803-a31c9ef7f8ad，开始 2026-10-08T18:14:11.5341630+08:00。
定向 TRX 完整文件没有独立保存；全量前保存此身份摘要。SHA256 由随后同一发布产物的全量验收取得。
