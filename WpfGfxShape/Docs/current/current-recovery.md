# 唯一当前恢复入口

1. 先读 [严格对照合同](current-comparison-contract.md)。
2. 完成报告：[同冻结包严格双进程重复](../history/completed-work/replacement-strict-dual-process-first-render.md)。
3. [当前严格替换验收矩阵与后续顺序](current-validation-matrix.md)；[任务边界](next-code-task.md)仅保留任务5边界，历史按需查[索引](../history/handoffs/README.md)。

## 当前状态

- **OverallOpen**。任务5用run2完整留存发布快照完成两次新增独立双进程重复，有限32×32 RTB门通过；不是桌面/full/x86或所有行为等价认证。
- 599个非Evidence文件相同且第二次运行前后不变，历史402个业务包manifest项全部匹配。四个新子进程exit0、完整4096字节差异0，与历史run2 raw相同；11项业务契约及runtime身份复核通过。
- 第二次父PID804真实exit0、stdout摘要通过、stderr空，子12080/824正常回收；首次父退出仍缺独立收据。证据见 `artifacts/strict-render-r28/task5-supervised-repeat/Evidence/summary.json`、`parent-process.json`、`readback.json`。
- run1缺模块/runtime证据且harness不同，不计同冻结包重复；旧红与历史缺口不抹除。模块采样非连续拦截，未比较字段与清理证明边界见报告；官方SHA已核不等于签名全链，审计最外层真实exit仍Unknown。

## 历史执行账与本轮文档任务

任务5报告的辅助审计 publish1、sandbox2 属于历史业务证据账，不是任务6新增。任务6只改文档与DocumentationMaintenance专用辅助，生产/业务验收改动0；历史维护记录见[原字节台账归档](../history/handoffs/documentation-maintenance-legacy.md)，该归档只覆盖原索引标明的阶段，不冒充任务6最终verify原件，终端/脚本/委派0。

**文档治理进行中**：任务10已全文治理5份历史报告；文档工作不是产品运行结果，不表示代码全完成。当前strict渲染仍仅上述x64固定32×32有限门，状态不变；下一代码任务仍待按[矩阵](current-validation-matrix.md)扩展路径→变换→裁剪→透明。旧234项为整理前历史盘点，不作迁后现状；目录整理检查见[当前维护检查](repository-organization-check.md)。

当前文档维护职责、已知归档范围、缺件与待审名单统一见[当前维护台账与迁后检查](repository-organization-check.md)；旧台账现行原件缺失，不由空输入副本重建，不以历史已读统计认证迁后全覆盖。产品当前状态仍只维护本页；历史累计文件不作为并行恢复入口。
