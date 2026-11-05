# 仓库迁后检查与当前维护台账

## 职责与边界

本页集中维护文档治理范围、已处理事项及待审责任，不维护产品状态或竞争排程。产品状态只见[恢复入口](current-recovery.md)，规则只见[严格双进程合同](current-comparison-contract.md)，产品验收顺序只见[当前矩阵](current-validation-matrix.md)。

本轮仅文档文件工具，委派、终端/脚本、程序执行及 build/test/publish/沙箱均0；生产/测试/工具源码修改0。artifacts 完全忽略，不枚举、不读写、不重组；正常构建机制不变。目录整理工具一次性非幂等，不再运行。

## 已确认迁移结果（承接上一检查，不重跑）

- 外层 Docs 已不存在；内层 Docs 根为 current、specifications、history、decisions、investigations 和 README。5份原 current 入口、36份规范已到目标位置。
- completed-work 原内层、outer-root、handoffs-origin 来源保留；handoffs 的 nested-docs-origin、maintenance-output-origin、handoff-output-origin、maintenance-input-origin、receipts-origin 及 maintenance-input-handoff.md 已落地。同名来源不合并覆盖。
- 固定清单7个兼容页、6个误发布文件及10个空壳目录已无旧路径残留；未发现未迁步骤或可定位中断点。这不是历史全文/字节/hash完整性认证。
- DocumentationMaintenance 已迁 Tools，Content/verify 已由前任务修为迁后路径，archive/pointer 旧入口退出；产品 solution/ProjectReference 无需改，正常输出机制未改。本轮未再审源码或执行 verify/inventory。
- 上轮单文件组织工具 build exit0；维护工程 build exit0、0警告/0错误。仅编译的既有结果，不是本轮执行或业务/归档验证。

## 当前维护台账：已知保全范围

- [历史归档索引](../history/handoffs/README.md)列出8个旧累计入口：next-session-handoff、current-work-item、current-execution-rules、current-stable-baseline、final-acceptance-remaining-matrix、remaining-gap-closure-plan、replacement-issues-and-plan-status、wpfgfx-complete-replacement-validation-plan。旧兼容入口删除不等于原文删除；legacy/audit 保留不改。
- 另有[旧维护台账原文](../history/handoffs/documentation-maintenance-legacy.md)，本轮183行分段全文读完，实际仅保留任务1–4，末次只完成首份累计入口归档；后续归档范围由历史索引记录，不能从此台账推出十轮完整收据。
- 原 `Docs/documentation-maintenance.md` 缺失；`history/handoffs/maintenance-input-origin/documentation-maintenance.md` 本轮读取仍无内容。该空副本及两份空 README 保留不改不删，不从空副本重建原件。
- 不复原迁后总数、已审数、待审数或hash。旧848/1102等盘点、任务8/10已读与任务11空写统计均只属原阶段，不直接认作当前覆盖；本页不复制十轮流水。

## 本轮已处理与链接核验

- 四份规范已逐份全文读后修导航：`com-acceptance-sandbox-operations.md` 冻结工程档转 history/completed-work/handoffs-origin，并修到真实 NativeCaller 历史诊断；`windowsdesktop-version-provenance.md` 修采集档及 PresentationCore 对照；`publicabi-family-delivery-plan.md` 修7份方法族完成档；`testing-comparison-and-acceptance.md` 修当前矩阵及 DEC-0004，分离旧缺失锚点。
- 上述迁后证据目标已逐个读标题，当前矩阵已全文读取；只证明目标/主题及导航可定位，不重新认证历史运行原物。既存历史报告的内部深链不改。
- 实际无 `history/README.md`，顶层入口是 `history/history-index.md`；已修该页与 history/handoffs、completed-work、completed-work/handoffs-origin、plans、handoffs/receipts-origin 的 README 当前导航。历史 README 逐一读取；maintenance-input-origin 的两份空 README 不改。原文证据 readme-before-task8、legacy 与 audit 不改。
- Docs/README 与 current/current-recovery 的当前维护链接统一到本页，无新建并行台账。修改文件分段/小段回读，仅核本任务涉及导航，不宣称全库链接清零或语义审完。

## Missing 与后续可执行责任（未审，不在本轮启动）

- `Docs/investigations/INCOMPLETE-HW-CACHE-CLOSEOUT/evidence-readback.md`：实际路径缺失且 Docs 名称搜索无原件，版本来源页已保留原路径文本及 Missing/不可核。后续须找回原件或独立补取加载因果证据，不用无关报告冒替。
- `Docs/final-acceptance-remaining-matrix.md#33项失败分账`：保留的历史矩阵未命中该标题，旧表不可核；现导航指当前矩阵而非伪造分账。需要该历史表时须恢复原件，不能从摘要造表；“七阶段正式完成条款”也仍缺可核历史标题。
- specifications 其余页的迁后深链及合同一致性尚未逐项审：先全文读 `06-testing-strategy.md`、`testing-levels-and-gates.md`、`governance-authority-traceability.md` 等，再逐目标/锚点核验；前任务的局部修复不代表整目录通过。
- decisions 与 investigations（含 active/audits/closed/superseded、GlyphProbe、INCOMPLETE-HW-CACHE-CLOSEOUT、RotatedFloatContract.Tests、TwoInputShader.Tests）仍待逐文件语义审核，先区分设计/实际运行/缺物，不改证据事实。
- history/completed-work（含 outer-root、handoffs-origin）、history/plans 正文及 handoffs/receipts-origin 长报告仍未迁后全审；可先核 `replacement-default-com-acceptance-baseline.md`、`replacement-default-com-acceptance-failures.md` 的身份/有限结论/链接。legacy/audit 只按原基准解释，不为修链改字节。
- 历史原件找回后再核缺失维护收据；未找回则保持 Unknown。本轮不计算SHA、不复验归档、不执行污染清理或产品专项。文档迁移完成不等于产品全完成，OverallOpen 不变。
