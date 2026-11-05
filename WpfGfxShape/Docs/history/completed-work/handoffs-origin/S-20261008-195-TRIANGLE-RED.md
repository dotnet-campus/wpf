# S-20261008-195：Triangle 直线端帽 Red 证据

使用 S-194 发布 #1 生产 DLL（SHA256 F6C29828097FF72CD2557894DBBB262142185CA145FCD545737E2DBC7EF51699），未另发布。定向唯一 ComAcceptance 验收新增实色正向与 ImageBrush 反向负厚度 Triangle 两项：90 通过/2 失败，0 跳过，111ms；Present 实际 `0x80004001`，预期遮罩 WIC 锁定首错 `0x88982F0D`。

TRX `542e8ec7-b0ab-4df7-b77a-333ab04c3e8c`；start `2026-10-08T20:15:04.7139515+08:00`；finish `2026-10-08T20:15:05.1643441+08:00`。覆盖前保存身份和错误摘要，未复制完整 TRX。日志可见既有 MSTest analyzer 警告，未作为本任务修改范围。

原生证据：仅读取 WpfGfx/core/geometry/strokefigure.cpp:1880–1980，CSimplePen::DoBaseCap 的 Triangle 分支从两个轨道端点经过 ptCenter±GetPenVector(m_vecRad) 尖顶连接；原生 HRESULT 首错传播保持。下一发布验证共同多边形扫描及 actual content bounds 消费者，不代表一般 Widen 或 private ABI 已实现。
