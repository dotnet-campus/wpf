# S-20261008-192：直线 Widen 续接基线与发布记录

## 模块状态
完整 ImageBrush 遮罩映射、层消费与 Compose 生命周期模块继续 Active，本文件不声明关闭。

## 已发生的发布
1. `dotnet publish WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`：失败，MSB1009，项目不存在，9 行日志未完整归档，无产物。
2. `dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`：失败，同上，无产物。
3. `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`：成功，退出码 0，Release/win-x64。完整可用日志 11 行已读取；还原最新，输出 bin/net10.0/win-x64/wpfgfx_cor3.dll 与 artifacts/publish/WpfGfxShape/Release/win-x64/，未显示重新 native 编译。

## 覆盖前测试证据
发布 #3 全量 297/297，失败/跳过 0，298ms。TRX `58247440-4d44-4a3a-a30c-fde731ac645b`，开始 `2026-10-08T20:06:22.0832026+08:00`，结束 `2026-10-08T20:06:22.7148188+08:00`。SHA256 `E7202E9C314F37712B2E5975DD9A92DF29A2A540C6EC92B48409753EB77FD6A9`，产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
固定 TRX 已覆盖前轮结果，覆盖前仅有前轮归档身份摘要；本轮再次运行前保存当前身份摘要，未保存完整 TRX 副本。

## 本轮原生证据
WpfGfx/core/resources/linegeometry.cpp:1–97：当前值、动画、几何变换。
WpfGfx/core/geometry/CompactShapes.h:965–1026：CLine 非矩形及虚析构。
WpfGfx/core/geometry/shapebase.cpp:100–220：WidenToShape→WidenToSink，空笔画早退与首错。
WpfGfx/core/geometry/strokefigure.cpp:1880–2035：两侧轨道、Flat 连接及 Square 延伸半笔宽。
每次原生读取不超过 400 行。一般曲线 Widen、Dash、Round/Triangle、private ABI 尚未闭合。
