# S-20261009-231：DrawingImage设备映射分辨率

## 生产推进

DrawingSnapshot尺寸由viewbox→viewport stretch→device矩阵基向量长度计算，不再直接按内容DIP分配。相对viewbox先解析为内容坐标；Fill/None/Uniform/UniformToFill按相应映射缩放。中间bitmap坐标及drawing消费映射仍一致，延迟realization和失败释放保持不变。修复source递归深度拒绝时先插入活动集合导致残留的问题：先检查256深度，再Add。

原生证据：WpfGfx/core/resources/DeviceAlignedIntermediateRealizer.cpp 215–325（工具显示至317），CalculateSurfaceSizeAndMapping/1D按采样空间bounds floor/ceil计算尺寸，随后原生最大尺寸调整。本次基向量尺寸是软件source的分辨率推进，并非上述设备aligned裁剪算法完整直译；尚未实现目标clip裁剪与原生max-size mapping。

## 验证

新增8项真实COM验收：DrawingImage source 1e8内容缩小与1e-8内容放大、画刷/遮罩，及Tile/FlipX/FlipY/FlipXY。后四项仅在基础tile内采样，不能证明跨tile翻转缝完整合同。精确红色/透明断言保留。定向及全量通过；最终686/686，零失败/跳过（1s）。既有source回调/关闭/首错重试全量通过。

## 发布

两次命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始成功，11行日志，678项通过；hash/TRX未独立保留。
2. 设备映射尺寸修改后成功，60行日志，编译存在CA1416警告。最终686/686；SHA256 `CD2BCD99F8E0EE4AB75B6C8798D149BEFA05508A8B2EF5BC6AD919ADA26AC801`，TRX `5878dc14-aa6b-4199-9f1d-f51e1749dd7d`。由Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx确认。中间定向TRX被覆盖。

## 未收口合同

目标clip驱动的中间面裁剪、绝对viewbox子区域限制、跨tile接缝/flip及资源遮罩回调专项尚未闭合；原批准模块保持Active。当前仍可能按全部映射内容分配巨大中间面，不能宣称内存优化完成。下一轮必须继续生产clip与mapping方法族，而非仅补测。

阶段4Active、Ledger RepairRequiredPartial；完整MSVC ABI、曲线恢复、层栈/UCE与PresentationCore E2E未关闭，不能替换原库。
