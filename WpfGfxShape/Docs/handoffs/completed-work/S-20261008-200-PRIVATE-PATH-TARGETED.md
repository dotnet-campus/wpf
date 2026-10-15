# S-20261008-200：私有 finite/infinite 路径定向验证

这是完整 ImageBrush 模块的内部验证，不是模块完成。

## 发布 #2

- 命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
- 项目 WpfGfxShape，Release/win-x64，退出码 0；读取全部可用 59 行诊断。
- 工具将 414927 字符截断到尾部 20000 字符，前 394927 字符不可取得；尾部可核实 CA1416、Generating native code、lib/exp 创建、发布路径，不声称完整诊断无警告。
- 产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 定向筛选 `FullyQualifiedName~WhenNativePathUsesRealizerThenPixelsAndFirstFailureArePreserved`：10/10，0 失败/跳过，40ms。
- TRX `7019a0c3-4193-47b6-8266-c5916aef0f91`；start `2026-10-08T20:39:28.2194576+08:00`，finish `2026-10-08T20:39:28.6320357+08:00`。
- 本次定向用例不输出 SHA256；待同一发布的全量测试取得。覆盖固定 TRX 前保存身份摘要，未复制完整 TRX。

## 修改与验证边界

生产 FillInternalShape 将原生 RGBA 显式转换为托管 ARGB；没有修改像素预期以掩盖通道问题。

验收通过已有公开 COM QueryInterface 得到生产 internal target，分别调用 finite 槽10/infinite 槽11。有限 shape/figure 为按原生声明建立的测试边界对象，不是真实原生生产创建者。有限矩形位于更大的层裁剪中，三个像素仅中央红色，确认按 figure 覆盖绘制而不是整层填充。两条路径均执行 WIC 锁失败早退、Ensure 首错、effects 首错（含 null brush）、重试、Pop、再次成功锁定和最终释放。

原生证据：`core/geometry/ShapeBase.h:43-75`、`ShapeData.h:38-123` 及 `figure.cpp:154-221` 确认虚析构、shape 槽4/5/6、figure 槽8/15/16/17/22 和遍历终止合同；`api_basebrushes.h:18-44` 确认 CMILBrush 为 IUnknown 三槽后 GetType、BrushSolid=1。各次读取均不超过400行。

一般 Widen、私有 bitmap brush 布局及 mapping、真实创建者、完整层/Compose生命周期仍未关闭，不将10个用例通过当作完整 ABI/E2E 证据。
