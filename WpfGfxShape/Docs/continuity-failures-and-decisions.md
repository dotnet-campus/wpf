# 连续性：中断、失败、决策与一致性

[连续性协议](10-session-continuity-protocol.md)§8–12正式合同。

## 8. 中断协议

不能完成时不开始新实现；保持可编译或标明未完成变更；不越证据提升；工作项记录最后动作/当前文件函数/未完检查/首个恢复动作；基线说明未运行；下步只恢复当前模块或唯一前置，不靠下一轮猜diff。

## 9. 失败与阻塞

保留错误/配置/log/artifact；标ExecutedFailed或Blocked；区分实现缺陷、环境能力、平台不支持、计划假设错误。
只修当前根因，不suppression/假成功/绕过；影响范围/架构/强约束则建Decision并请求必要裁决。

## 10. Decision

位置Docs/decisions/DEC-<NNNN>-<short-title>.md。
必须有ID/状态、背景/范围、确认事实/未验证假设、选择/拒绝方案、ABI/正确性/追溯/测试影响、重新评估触发、用户批准要求、supersedes/superseded-by。
改变生产范围/目标架构、章程例外、COM/callback/unload/x86机制、generator/frozen source长期策略、C#承载偏差、测试框架/candidate加载/大基线存储，须建立或更新Decision。
Decision不能自行授权违反章程或删除兼容范围。DEC-0004已接受驻留，不重复请求批准。

## 11. 进度报告

仅报告切片是否完成、Active阶段、正式Closed/Pending数量、硬阻塞、替换资格；这些当前值只从[基线](handoffs/current-stable-baseline.md)读取。
分母不可确定计算不报总百分比，单切片100%不等项目100%。

## 12. 一致性检查

每轮确认工作项/阶段条件一致，Ledger/ABI/Evidence/映射一致，NotRun不写pass，唯一下步，独立完成档，无断链，未改原WPF，未混无关重构/优化/清理。
历史SHA与数字仅对应原身份，不拼当前；方法页不复制流水；外部四输入只维护[唯一清单](all-completion-external-prerequisites.md)。
