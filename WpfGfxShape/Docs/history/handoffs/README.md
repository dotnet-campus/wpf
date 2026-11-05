# 历史索引与统一纠偏

当前状态只见[恢复入口](../current-recovery.md)，规则见[严格对照合同](../current-comparison-contract.md)，可用计划见[当前严格验收矩阵](../current-validation-matrix.md)。历史“唯一当前”、原版永禁、预算与有限通过均不覆盖当前合同；失败和未知保留。

## 原字节归档

| 原入口（相对Docs） | 完整历史 | 原行→短行 | 字节 | 审计 |
|---|---|---:|---:|---|
| next-session-handoff.md | [legacy](next-session-handoff-legacy.md) | 2484→10 | 614775 | [audit](next-session-handoff-audit.json) |
| handoffs/current-work-item.md | [legacy](current-work-item-legacy.md) | 867→10 | 180252 | [audit](current-work-item-audit.json) |
| handoffs/current-execution-rules.md | [legacy](current-execution-rules-legacy.md) | 219→10 | 60553 | [audit](current-execution-rules-audit.json) |
| handoffs/current-stable-baseline.md | [legacy](current-stable-baseline-legacy.md) | 581→10 | 133913 | [audit](current-stable-baseline-audit.json) |
| documentation-maintenance.md（任务1–4） | [legacy](documentation-maintenance-legacy.md) | 183→摘要 | 25599 | [audit](documentation-maintenance-audit.json) |
| final-acceptance-remaining-matrix.md | [legacy](final-acceptance-remaining-matrix-legacy.md) | 1006→11 | 135281 | [audit](final-acceptance-remaining-matrix-audit.json) |
| remaining-gap-closure-plan.md | [legacy](remaining-gap-closure-plan-legacy.md) | 162→11 | 30911 | [audit](remaining-gap-closure-plan-audit.json) |
| replacement-issues-and-plan-status.md | [legacy](replacement-issues-and-plan-status-legacy.md) | 1862→11 | 424198 | [audit](replacement-issues-and-plan-status-audit.json) |
| wpfgfx-complete-replacement-validation-plan.md | [legacy](wpfgfx-complete-replacement-validation-plan-legacy.md) | 532→11 | 124561 | [audit](wpfgfx-complete-replacement-validation-plan-audit.json) |

各原件UTF-8无BOM。归档先落地，再重新发布工作区未短化原文与落地归档，校验源SHA未变和完整字节一致后生成短入口；任务6独立最终verify见项目根`artifacts/documentation-maintenance/task6-final/verification.json`，任务5收据保留原路径。SHA对象为完整发布输入副本及沙箱归档读回，不冒称原位hash。程序全文读取/逐字节保全不等于助手逐条语义审核，人工读取范围只见[台账](../../documentation-maintenance.md)与矩阵。

## 任务8：README完整文本保留（文件工具）

[任务8前README完整文本](../../history/readme-before-task8.md)：原60行完整读取，文件工具写入后逐段全文回读，保留历史数字与首尾；现入口60→20行。此项为人工文本保全，不列入上表原字节/SHA审计，未核编码、换行字节或hash，不生成虚假audit。归档内链接按迁移前`Docs/`基准解释，不按`Docs/history/`重解；旧“当前权威”、预算和认证均为原时态，状态只见恢复入口。历史链接不保证原件存在，缺件不能拿无关页面冒替。

## 原路径与历史深链

四份任务6规划、next-session-handoff及维护台账内部链接均按迁移前`Docs/`基准解释；三份current-*累计原文按`Docs/handoffs/`解释。归档内部字节不改，不按history位置重解原链接。当前页面指向当前矩阵；历史引用按原路径基准人工定位，不伪造原文不存在的锚点。“七阶段正式完成条款”“33项失败分账”在此次保全原文均未命中，已修当前导航并披露，不认证缺失历史表。

## 剩余与副本责任

四份规划已保全短化，不是全库语义审完。任务8已全文审四份ABI/Ledger规范及README文本并短化入口；下一批为completed-work两份default-com-acceptance-baseline/failures主题归并及decisions，逐文件范围见[当前台账](../../documentation-maintenance.md)。D2其余规范、D4–D6与investigations继续按[旧完整台账](documentation-maintenance-legacy.md)责任索引；绝大多数文档仍未审。

任务7已纠正前账的污染漏盘：Docs实际1102，按结构分现行候选68、历史证据800、与既存artifact同SHA污染231、待判定3；不是1102份权威文档。完整逐文件SHA/来源匹配/显式引用在项目根 `artifacts/documentation-maintenance/task7-audit/Results/pollution-inventory.json`；231项人工复核候选见同目录 `manual-cleanup-whitelist.tsv`，不能把同SHA当创建来源认证。真实legacy/audit及原始报告仍是本索引上表权威落点，不删除。

任务11补记既有收据：累计空写32项、回读空32项，分布为 `Docs/Input/` 23项、`Docs/handoffs/Input/` 1项、误嵌套 `Docs/Docs/handoffs/history/` 8项。前24项沿用首任务收据，后8项已在后续任务实际回读为空，本次不冒称重读。原29项清单新增补记3项均在误嵌套目录：`replacement-issues-and-plan-status-legacy.md`、`wpfgfx-complete-replacement-validation-plan-audit.json`、`wpfgfx-complete-replacement-validation-plan-legacy.md`。本索引上表权威 `Docs/handoffs/history/` 归档原文未动。

原234位置减32得到202，仅为清单推算，不是实时非空数：193项runtime/工具 + 2项Output正文 + 4项工具配置 + 3项待判audit = 202，均未清空。3项audit为早期archive阶段记录，不同于权威pointer收据，保留待核。空写是截断不是删除，空文件及目录仍在，无实测磁盘释放量；可以清空内容，但后续须继续逐项来源、artifact保留及引用复核。

本次仅更新指定3份文档，无新清空，所有程序执行（含终端、脚本、build/test/publish、Windows沙箱）及委派均0；未扫描全库或复算实时hash。详细收据见项目根 `artifacts/documentation-maintenance/task11-cleanup-results.md`。876→1102为历史清单明细，848旧基准缺逐文件集合，不强行认证差额或当前路径集合；文档治理仍未全完。

专用辅助已隔离为静态12路径白名单 `DocumentationOutput/` 与审计 `Results/`；publish拒绝Docs。回收只指定上述子目录至artifacts，禁止执行器根/runtime/inputs回拉Docs。四份规划再次verify成功仅证明发布副本与审计一致，不是全库语义审核或最终现行页hash。四短入口/矩阵/本索引及台账回读不等于治理完成。下一规范批次与历史报告批次、Unknown未审数和手工清理边界见[当前台账](../../documentation-maintenance.md)。
