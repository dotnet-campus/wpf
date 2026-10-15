# S-20261008-190：笔画生产修正与覆盖前定向证据

## 本轮发布 #2

命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。

项目 `WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`，Release/win-x64，退出码 0。产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

日志共 60 行；已读取第 1–10、50–60 行。第 7 行明确原输出 411315 字符被截断，仅保留最后 20000 字符，不能声称保存了完整诊断。可核实 CA1416、Generating native code、创建 native lib/exp 和发布目录。SHA256 待同一发布的后续全量 TRX 获取；没有再次发布前不更换产物。

## 生产修正与原生合同

- `compactshapes.cpp:499–610`：简单圆形 Pen 的矩形 Widen 使用 REAL 半厚度、float 边界扩缩、Alternate 外/内轮廓；内边界退化时不添加内孔，最后施加矩阵。当前 `GeneratedStrokeGeometry` 改为 float 边界算术，coverage 保持实际外轮廓减内孔，不用包围框冒充覆盖。
- `swsurfrt.cpp:815–905`：共同 clip/lock 后先 FillPath，再 Widen，再笔画 FillPath，首错退出并解锁。`GeneratedVisualRenderer` 将笔画 Widen 放到填充捕获成功后，避免无依据抢先 Widen 失败。generated 帧快照与原生即时扫描仍不是同一 ABI。
- 修正 0.5 厚度仅笔画预期：2×1 矩形的内孔为 1.5×0.5，目标像素内孔移除 24/64 样本、笔画保留 40/64，预乘红色为 `0x9f9f0000`，不是此前错误预期 `0x40400000`。增加 −0.5 厚度等价、厚度 1 内孔退化对照。

## 覆盖前定向验收

直接生产 AOT、唯一 ComAcceptance csproj，筛选 `FullyQualifiedName~WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping`：87/87，0 失败/跳过，138ms。

TRX `0b7c8b79-d59e-4c9b-9ec2-6717def20cc1`；开始/结束 `2026-10-08T20:02:18.1928408+08:00` / `2026-10-08T20:02:18.7060175+08:00`。这是全量覆盖前读取的身份摘要，不是完整 TRX 副本。

覆盖真实 ImageBrush/作用域实色 Pen、仅笔画、内孔、负厚度、192DPI、锁定失败丢弃层及下一帧恢复。完整模块尚未关闭；一般几何 Widen、私有 ABI、一般曲线组合等差集保留。没有引入测试导出、Host、新项目或修改原 WPF。
