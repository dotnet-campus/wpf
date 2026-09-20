# 下一轮交接：继续推进 wpfgfx 代码翻译

> 本文件是当前唯一权威入口，只保存恢复顺序和文档导航，不再累积完成历史。

## 恢复顺序

1. 阅读 [`handoffs/current-execution-rules.md`](handoffs/current-execution-rules.md)，确认每轮执行规则与禁止扩展边界。
2. 阅读 [`handoffs/current-stable-baseline.md`](handoffs/current-stable-baseline.md)，确认当前阶段、稳定能力和最近验证基线。
3. 阅读 [`handoffs/current-work-item.md`](handoffs/current-work-item.md)，执行其中的唯一下一动作。
4. 需要中长期缺口顺序时阅读 [`remaining-gap-closure-plan.md`](remaining-gap-closure-plan.md)。
5. 需要已完成事实时查询 [`handoffs/progress-completed-work.md`](handoffs/progress-completed-work.md)；HW render-target 逐轮历史见 [`handoffs/progress-hw-rendertarget.md`](handoffs/progress-hw-rendertarget.md)。

## 维护规则

- 本文件保持短小，只维护导航和恢复顺序。
- 当前规则只写入 `current-execution-rules.md`。
- 当前稳定摘要和验证基线只写入 `current-stable-baseline.md`。
- 唯一下一动作只写入 `current-work-item.md`。
- 完成切片迁入 `progress-completed-work.md`，不得继续堆积在当前入口或当前工作切片中。
- `progress-completed-work.md` 文件末尾固定保留 `<!-- HISTORY -->` 标记；后续追加完成历史时，不读取或覆写整份长文档，而是用替换文本工具将该标记替换为“新增历史内容 + 原 `<!-- HISTORY -->` 标记”，以便持续快速追加。
