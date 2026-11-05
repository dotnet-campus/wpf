# S-20261008-196：Triangle 定向 Green 与发布 #2

- 发布命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`；Release/win-x64，成功。
- 可用日志 60 行；实际输出 411315 字符，工具保留末 20000 字符、截断前 391315 字符。本轮读取 1–10、54–60 行，核实 CA1416 警告、Generating native code、lib/exp 创建和发布目录；不声称完整诊断归档。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`；SHA 待后续全量输出取得。
- 修改 GeneratedStrokeGeometry：允许非退化直线 Triangle start/end cap，轮廓通过半厚度切向尖顶连接法向轨道；共同多边形扫描与仿射 bounds 消费者保持一致。无 COM ABI/所有权修改，零长度非 Flat、Round、Dash 保留拒绝。
- 原生证据及 Red 见 S-195。
- 定向 92/92，0 失败/跳过，117ms；TRX `fda6a6d7-bc76-4477-ac0f-dff759be8a46`，start `2026-10-08T20:16:15.7203745+08:00`，finish `2026-10-08T20:16:16.2143106+08:00`，对应发布 #2。
- 两项新增参数用例沿真实实色/ImageBrush、作用域、锁定遮罩、失败丢层、释放锁后重试、像素及最终释放路径通过。端帽尖顶位于当前输出裁剪外，不能把这些像素断言视作已验证尖顶边缘差分；仍需合并到完整模块验收。
- 覆盖固定 TRX 前保存身份摘要，未保存完整 TRX 副本。
