# S-20261008-194：直线笔画当前基线

## 发布 #1 与全量验收

- 命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
- 项目：WpfGfxShape.csproj；Release；win-x64；发布成功。工具返回日志共 11 行；随后测试覆盖最后日志，本轮未保存完整发布诊断，不声称诊断归档。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 唯一 ComAcceptance csproj 全量直接 DLL 验收：300/300，0 失败/跳过，322ms。
- TRX：`7dfd4e1b-df15-4a45-addb-0f2d173a9bb5`；start `2026-10-08T20:13:01.4444181+08:00`；finish `2026-10-08T20:13:02.1122138+08:00`。
- 测试输出 SHA256：`F6C29828097FF72CD2557894DBBB262142185CA145FCD545737E2DBC7EF51699`，对应本轮发布 #1。
- 固定 TRX 下一次覆盖前保存以上身份摘要，未另存完整 TRX 副本。

## 已验证能力与限制

承接 S-193 的共同多边形扫描、LineGeometry 当前动画值及 Flat/Square 直线轨道覆盖、实际绘制/内容 bounds 消费者；先前新增三个用例现全量 Green。本轮基线未修改生产代码。矩形与直线窄路径不等于一般 Widen；完整 ImageBrush 模块、非 IUnknown 私有 ABI、Round/Dash/零长度 Square 未关闭。阶段 4 Active，不能替换原 DLL。
