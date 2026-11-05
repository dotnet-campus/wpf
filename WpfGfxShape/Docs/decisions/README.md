# wpfgfx 迁移决策索引

稳定架构/范围/例外决策使用 `DEC-NNNN-<short-title>.md`；格式与触发条件见[连续性协议](../10-session-continuity-protocol.md)。任务9实际枚举本目录：本索引及DEC-0001–0006共7份，均全文审读；没有将所有决议一概废弃。

决策Accepted是合同成立，不是产品/运行Accepted。当前状态只见[恢复入口](../handoffs/current-recovery.md)，执行规则只见[严格双进程合同](../handoffs/current-comparison-contract.md)，覆盖与唯一顺序只见[当前矩阵](../handoffs/current-validation-matrix.md)。用户最新澄清覆盖旧执行政策，不撤销不冲突的技术设计。

## 按性质拆分的决议

| ID与位置 | 技术设计仍有效 | 历史执行政策被覆盖 / 部分待核 |
|---|---|---|
| [DEC-0001](DEC-0001-production-scope.md) | Accepted：最终DLL完整生产闭包、旁支及移出批准边界 | 项目数量/直接编译数为历史调查，未重新求值；不认证当前完成 |
| [DEC-0002](DEC-0002-silknet-adoption-boundary.md) | Accepted：Silk.NET按API家族裁决、唯一绑定、loader/ABI/许可合同 | commit/覆盖及WP-00G前Blocked不是当前执行状态认证，当前实现待核 |
| [DEC-0003](DEC-0003-managed-pinvoke-abi-integration-tests.md) | Accepted：托管发布后P/Invoke主证据、真实身份、危险场景隔离，不要求C++ NativeCaller | “以本决策为准”仅测试承载选择；不覆盖用户严格双进程要求，不禁止原版；宿主实施状态待核 |
| [DEC-0004](DEC-0004-process-lifetime-module.md) | Accepted：模块进程寿命驻留，不要求卸载/重载/热替换；对象/线程/callback仍须清理 | 历史外部清单/执行政策不作当前排程；x86有限核验链接原件缺失，EvidenceUnavailable |
| [DEC-0005](DEC-0005-x86-x64-acceptance-scope.md) | Accepted：正式仅x86/x64；ARM64为NotApplicable-UserScopeDecision，非通过 | 旧baseline撤当前权威；GenProto Blocked/Red及overlay读取者等仅原调查，本轮未核机器数据 |
| [DEC-0006](DEC-0006-installed-official-runtime-contract.md) | Accepted：来源可核的官方完整runtime、同版同架构独立对照；10.0.12仅历史身份 | 历史测试修复待办不争夺下一顺序；源码行号/安装完整性/修复状态本轮待核 |

## 冲突裁决与证据边界

- 原版独立业务进程对照允许且必须；禁止同进程双版、fallback、只测原版冒验收、候选专属补偿。同上层逻辑仅DLL不同，完整像素及业务契约严格比较；历史禁原版、预算耗尽/另授权不再约束后续分派。
- DEC-0003的隔离测试与DEC-0004驻留并不冲突：每个业务进程只加载一版，模块进程寿命驻留，但真实对象清理和退出证据仍需验证。DEC-0005限定架构，DEC-0006限定runtime来源/版本，不缩减适用门。
- 本轮不改媒体默认async、模块进程寿命或其他生产实现，不重新认证历史full/有限绿，不把历史机器Satisfied或决策Accepted当产品通过。
- 原索引和DEC-0004曾引用 `../investigations/audits/x86-shared-unload-actual-verification-20261014.md`；任务9实际枚举audits为空、名称查找未找到，故撤可点击失效链接并标EvidenceUnavailable，不冒替证据。这不影响用户驻留合同生效，也不证明x86不支持或当前已通过。
- x86完整ABI/配置/E2E、AOT-safe COM-like vtable/object及GenProto等历史未决不因治理关闭；当前可证范围和未证项以矩阵为准。重复scope overlay不重建，Ledger机器数据未改。
