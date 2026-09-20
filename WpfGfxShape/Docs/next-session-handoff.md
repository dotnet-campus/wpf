# 下一轮交接：继续推进 wpfgfx 代码翻译

> 本文件是当前唯一权威入口，只保存恢复顺序和文档导航，不再累积完成历史。

## 恢复顺序

1. 阅读 [`handoffs/current-execution-rules.md`](handoffs/current-execution-rules.md)，确认每轮执行规则与禁止扩展边界。
2. 阅读 [`handoffs/current-stable-baseline.md`](handoffs/current-stable-baseline.md)，确认当前阶段、稳定能力和最近验证基线。
3. 阅读 [`handoffs/current-work-item.md`](handoffs/current-work-item.md)，执行其中的唯一下一动作。
4. 需要中长期缺口顺序、阶段状态和剩余大项时阅读 [`remaining-gap-closure-plan.md`](remaining-gap-closure-plan.md)。
5. 需要新完成事实时查询 [`handoffs/completed-work/`](handoffs/completed-work/)；旧完成归档保留在 [`handoffs/progress-completed-work.md`](handoffs/progress-completed-work.md)，HW render-target 旧历史见 [`handoffs/progress-hw-rendertarget.md`](handoffs/progress-hw-rendertarget.md)。

## 维护规则

- 本文件保持短小，只维护导航和恢复顺序。
- 当前规则只写入 `current-execution-rules.md`。
- 当前稳定摘要和验证基线只写入 `current-stable-baseline.md`。
- 阶段状态、完成条件和剩余大项只写入 `remaining-gap-closure-plan.md`，不维护缺少稳定分母的整体完成百分比。
- 唯一下一动作只写入 `current-work-item.md`。
- 每个完成切片在 `handoffs/completed-work/` 中创建一个新的独立 Markdown 文件，不得继续堆积在当前入口、当前工作切片或单一长历史文档中。
- 旧 `progress-completed-work.md` 保留原位作为冻结历史，不拆分、不迁移、不再追加；新文件命名和内容规则见 `handoffs/completed-work/README.md`。
