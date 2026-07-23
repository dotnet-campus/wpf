# wpfgfx 迁移决策索引

稳定架构/范围/例外决策使用 `DEC-NNNN-<short-title>.md`。决策格式和触发条件见 [`../10-session-continuity-protocol.md`](../10-session-continuity-protocol.md)。

## 当前决策

| ID | 状态 | 主题 | 位置 |
|---|---|---|---|
| `DEC-0001` | Accepted | “整个 wpfgfx”的生产完成分母 | [`DEC-0001-production-scope.md`](DEC-0001-production-scope.md) |
| `DEC-0002` | Accepted | Silk.NET 按 API 家族经 spike 裁决采用形态 | [`DEC-0002-silknet-adoption-boundary.md`](DEC-0002-silknet-adoption-boundary.md) |

## 尚待裁决的 P0

- Windows x86 Native AOT shared library；
- Native AOT shared library unload/reload；
- AOT-safe COM-like vtable/object。

上述不得在没有官方和运行证据时标记 Accepted。