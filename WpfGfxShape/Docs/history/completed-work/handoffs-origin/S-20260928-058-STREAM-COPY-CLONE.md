# 双生产 IStream 复制、克隆与生命周期验收

## 合同与差集

复核 core/api/exports.cpp:748-890：CopyTo/Clone/Stat 业务由 descriptor 来源实现，wrapper 负责 ABI 和委托；可空输出适配不变。生产代码未修改。

将 ManagedStreamTests 改为 partial，新增 ManagedStreamTests.Lifecycle.cs。调用方 MemoryStream descriptor 使用 GCHandle 保活，生产对象最终 Dispose 释放流与句柄；所有 COM 对象均由真实 MILCreateStreamFromStreamDescriptor 创建，CopyTo 调用目标真实 vtable，Clone 再次调用生产导出。无 fake COM、内部调用替身、Host 或新测试项目。

新增7项：CopyTo四种输出组合、超过EOF的部分复制、读写/多次定位/扩缩容/STATSTG字段、原对象释放后真实克隆存活与最终释放。含 NUL/高位字节；STATSTG由原生布局写入、BCL定义读取，并检查末尾哨兵。仅 STATFLAG_NONAME，未验证名称分配释放。克隆语义由测试来源明确为内容快照与独立位置，不宣称由wrapper实现或匹配真实PresentationCore来源。

## 验证

新增测试在既有生产DLL上直接Green，无行为Red、无生产修复。随后执行当前源码Release/win-x64 Native AOT发布，退出码0；这是增量发布，日志没有重新生成native code，不声称全量重编。该次日志无警告输出，不代表既有CA1416已经修复。

唯一测试项目全量52通过、0失败、0跳过，133ms（EventProxy28、流24）。标准TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx；run ID 13309c46-2a0c-4c19-a7e6-f2141a3a99c8。

DLL：WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
SHA256：9A1248D757EAB6E3E44D00D75A0FDDBD53383605E75BA6BCD41025250F215148，与上一轮一致，已由本轮TRX重新核实。

## 边界及后续

没有修改slnx或原WPF。仅win-x64已执行；真实PresentationCore/WIC集成、跨apartment、并发、低资源、异常dispose、进程退出未验收。exports.cpp仍Partial，阶段4 Active，不能替换wpfgfx。

下一轮关闭双流失败传播与所有权生命周期：目标部分写入/失败、克隆创建失败与调用方清理、失败后继续使用以及最终释放；明确这些失败由descriptor来源控制，不添加测试专用生产导出。
