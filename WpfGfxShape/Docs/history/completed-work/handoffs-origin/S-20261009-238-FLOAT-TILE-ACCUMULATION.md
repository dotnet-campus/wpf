# S-20261009-238：共同浮点精度tile累加

## 生产修复与验证

恢复共同精度缺口：三分之一alpha内部viewbox四tile分数交界新增精确0x55550000断言，旧DLL失败；不是放宽接缝预期。AddVisibleTileMask在32bpp目标上也使用浮点renderer生成子tile和累加结果，浮点bitmap交给原有消费者，在最终目标格式转换时统一量化。不再把每个子tile量化到byte后相加。浮点临时renderer只用于realization，其destination为空但不执行目标draw；Dispose释放surface，不释放调用方callback。

新增四项参数化真实COM验收：1/3、0.1、0.2、0.8 alpha，普通和交界像素相同，预期分别0x55550000、0x1a1a0000、0x33330000、0xcccc0000。旧0.5和整数/1e10等断言保留。最终721/721通过，0失败/跳过（1s）。

读取原生ViewportAlignedIntermediateRealizer.cpp 709–795：实际像素范围floor(min-0.5)/floor(max+0.5)、非tile范围裁剪和offset调整。本轮尚未完成该全方法和padding直译，只关闭共同精度导致的已证明舍入错误。

## 发布记录

两次命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；共同产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始成功，11行日志，全量717/717，起始独立hash/TRX未保留。
2. 浮点共同realization修改后成功，59行日志，仍有CA1416警告；定向及全量Green，测试增强后最终721/721。SHA256 `EA3264907A6F71E8B3F72A882862587D3EE64BBC2D3D39799678E73FD5ADE9A3`，TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，Run ID `65d2f42a-3e3c-49a8-bfeb-c612093a193e`，TestContext确认实际加载产物；中间TRX已被覆盖。

## 未关闭与下一完整动作

仅保证多tile消费贡献的共同浮点精度：DrawingSnapshot原始source中间面仍可能先32bpp量化；完整float/HDR、旋转parallelogram、source过滤padding、可见跨度超过viewport路径及像素相位尚未闭合。不得将四项alpha成功宣称整个生产模块关闭。

下一轮应直接完成DrawingSnapshot source及最终消费者的统一格式/采样mapping与过滤边界生产方法族，合并HDR、旋转、fill/stroke/mask、回调首错验收。阶段4Active、Ledger RepairRequiredPartial，不能替换原库。
