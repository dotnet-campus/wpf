# S-20261008-191：矩形笔画续接恢复 Green

## 本轮结果

本轮生产修改：`GeneratedStrokeGeometry` 按原生 REAL 边界扩缩计算外轮廓及内孔；`GeneratedVisualRenderer` 按 fill→Widen→stroke 顺序捕获。测试修正独立推导的内孔像素，增加 −0.5 厚度和厚度 1 退化内孔对照。真实消费者包括 generated 绘制和已有内容 bounds 消费者；不新增 ABI、Host、项目或修改原 WPF。

原生依据和定向结果见 `S-20261008-190-STROKE-TARGETED-EVIDENCE.md`；初始失败及覆盖前身份见 `S-20261008-189-INITIAL-STROKE-BASELINE.md`。这些是同一完整生产模块的内部验证记录，不能当作一般 Widen 或整个 ImageBrush 模块关闭。

## 逐次发布关联

本轮实际发布共两次，均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，生产项目、Release、win-x64。

- #1 成功，工具报告 11 行；未在测试覆盖日志前读取发布诊断。SHA `227610A11B1C733EEC1165A5A6C002386FC688A1D37F94031FB37E47BE1DE78D`；全量 294 通过/1 失败、322ms，TRX `90263279-71e5-432c-81ff-9cf68856f1c0`。
- #2 成功、退出码 0；日志 60 行、原输出截断，读取首尾可核实 CA1416、native 代码生成及 lib/exp 和发布目录；未保存完整发布诊断。SHA `E7202E9C314F37712B2E5975DD9A92DF29A2A540C6EC92B48409753EB77FD6A9`，来自同一发布后全量 TRX 第 13 行。定向 87/87、138ms、TRX `0b7c8b79-d59e-4c9b-9ec2-6717def20cc1`；随后全量 297/297、0 失败/跳过、350ms，TRX `224ddcd7-1909-4168-9c86-c2d8231a6a1e`，开始/结束 `2026-10-08T20:03:05.3117510+08:00` / `2026-10-08T20:03:06.0253070+08:00`。

两次产物均为 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`；#2 覆盖 #1。固定 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx` 覆盖前已保存身份/结果摘要，未独立复制完整 TRX。最终测试日志 16 行已完整读取。现有 MSTEST0044/MSTEST0037 和发布 CA1416 未修复，不声称无警告。

## 能力与未完成边界

矩形无圆角、无 DashStyle、Miter/受支持 miter limit 的笔画可消费 ImageBrush 和实色；覆盖 fill+stroke、stroke-only、内孔、退化内孔、负厚度、DPI、作用域遮罩，以及源锁定失败后恢复。现有空 Pen/空位图、重入和引用释放等全量用例保持 Green。本轮未增加资源所有权或修改释放顺序。

一般线/曲线/圆角/其他 join/dash Widen、private shape/pen/context/realizer ABI、finite/infinite 全参数、EnsureRealization/effects 合同、一般曲线精确组合及原生差分仍未完成；不能将受支持矩形能力当作完整非零 Pen 合同。一般模块未关闭，没有已确认外部阻塞。覆盖率、PresentationCore E2E、Ledger 修复验证未运行，不能替换原 DLL。

## 下一轮模块计划

继续完整 ImageBrush 遮罩映射、层消费和 Compose 生命周期模块，而非另开纯补测轮：先发布/测试并在覆盖前保存诊断及证据；从原生真实创建者接通完整 shape/pen/realizer 方法族与 finite/infinite 参数；合并一般 Widen、fill/stroke 共同扫描、空 brush/effects 首错；合并层 Begin/End/Abort 和逆序清理、映射与精确区域消费者；联合重入、删除替换销毁、帧快照保活、失败恢复及最终释放；直接 AOT 定向/全量验收并更新 Ledger 实际差集，满足完整条件后才关闭模块。
