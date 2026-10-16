# wpfgfx 迁移决策索引

稳定架构/范围/例外决策使用 `DEC-NNNN-<short-title>.md`。决策格式和触发条件见 [`../10-session-continuity-protocol.md`](../10-session-continuity-protocol.md)。

当前正式架构仅 x86/x64；ARM64 编译/运行/布局按用户裁决不适用，不是通过。Ledger原ARM64阻塞字段的显式解释覆盖见DEC-0005，x86/x64父门仍Blocked。

## 当前决策

| ID | 状态 | 主题 | 位置 |
|---|---|---|---|
| `DEC-0001` | Accepted | “整个 wpfgfx”的生产完成分母 | [`DEC-0001-production-scope.md`](DEC-0001-production-scope.md) |
| `DEC-0002` | Accepted | Silk.NET 按 API 家族经 spike 裁决采用形态 | [`DEC-0002-silknet-adoption-boundary.md`](DEC-0002-silknet-adoption-boundary.md) |
| `DEC-0003` | Accepted | 发布后 ABI 验证以托管 P/Invoke 集成测试为主，删除 C++ NativeCaller | [`DEC-0003-managed-pinvoke-abi-integration-tests.md`](DEC-0003-managed-pinvoke-abi-integration-tests.md) |
| `DEC-0004` | Accepted（用户明确需求） | 生产DLL进程寿命驻留；unload/reload/热替换不适用 | [`DEC-0004-process-lifetime-module.md`](DEC-0004-process-lifetime-module.md) |
| `DEC-0005` | Accepted（用户最新明确范围，无需再批准） | 正式验收仅x86/x64；ARM64为NotApplicable-UserScopeDecision | [范围决议](DEC-0005-x86-x64-acceptance-scope.md) |

## 尚待裁决的 P0

- Windows x86完整ABI/配置/E2E矩阵（[Shared发布与有限生命周期已实测](../investigations/audits/x86-shared-unload-actual-verification-20261014.md)，不是支持未知）；
- AOT-safe COM-like vtable/object。

上述不得在没有官方和运行证据时标记 Accepted。