# 完成工作历史目录

> 本目录保存启用新归档规则后完成的生产翻译切片。当前工作入口仍是 [`../../next-session-handoff.md`](../../next-session-handoff.md)。

## 归档规则

- 每个完成切片创建一个新的 Markdown 文件，不再把历史持续追加到单一长文档。
- 文件命名使用 `S-YYYYMMDD-NNN-<topic>.md`；无法可靠取得日期时使用 `S-UNDATED-NNN-<topic>.md`。
- 每个文件只记录一个可独立验证的完成切片，包括原生证据范围、托管差集或修改、所有权与错误语义、测试和构建结果。
- 历史文件创建后保持不可变；只允许修正明确笔误，并在文件内记录修正原因。
- 不要求维护集中式逐项索引；需要历史时按目录文件名检索，避免 README 再次增长为长历史文档。
- 旧归档 [`../progress-completed-work.md`](../progress-completed-work.md) 保留原位作为冻结历史，不拆分、不迁移、不再追加。
- HW render-target 旧专题历史 [`../progress-hw-rendertarget.md`](../progress-hw-rendertarget.md) 同样保留，不把新完成事实继续写入其中。

## 单文件建议结构

1. 标题与完成切片名称；
2. 原生声明、实现和生产调用者证据；
3. 托管实现差集及修改；
4. ABI、HRESULT、所有权和释放顺序结论；
5. 定向测试、全量测试、ABI 测试和构建结果；
6. 明确未扩展的边界。
