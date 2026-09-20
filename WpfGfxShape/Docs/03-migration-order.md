# wpfgfx 迁移顺序与工作包门禁

> 本文保存当前仍适用的迁移顺序原则和门禁，不维护当前工作切片。
> 当前阶段顺序见 [`remaining-gap-closure-plan.md`](remaining-gap-closure-plan.md)，唯一动作见 [`handoffs/current-work-item.md`](handoffs/current-work-item.md)。

## 1. 实施原则

- 初次迁移以原生文件为审计单位，保持路径、类型、函数、字段、分支、调用顺序、错误顺序、锁和释放顺序可追溯；
- 一次只处理一个可独立验证的迁移切片；
- 等价完成前不重构、不改算法、不改变并发/缓存/资源策略；
- 不使用假成功、空实现或批量 `E_NOTIMPL` 跨过缺口；
- 当前没有明确原生契约的能力保持未实现，不创建推测性 API；
- 每个生产切片必须有原生声明、实现、调用者、所有权和测试证据。

## 2. 调查与实现分离

调查可以围绕项目、ABI 家族、生成协议、生命周期或循环组进行。调查完成只表示事实可用，不表示应立即实现。

实现顺序由依赖和可验证闭环决定：

1. 项目、低层类型、ABI 和测试承载；
2. loader、display/device、resource/use-context 和直接设备边界；
3. HW render-target 与内容管线；
4. 软件呈现链；
5. generated protocol；
6. resources/UCE；
7. meta/API、生产 ABI 和 DLL 导出；
8. PresentationCore E2E 与替换验收。

当前正式阶段定义以 `remaining-gap-closure-plan.md` 为准。

## 3. 路径映射

原生实现：

`src/Microsoft.DotNet.Wpf/src/WpfGfx/<relative-dir>/<name>.<cpp|cxx|c>`

原则上映射到当前生产项目中的对应职责文件。现有项目已采用 `Code/WpfGfxShape/Core/` 为主要承载根，因此新增代码应遵守当前项目约定，同时保持原生职责可追溯；不得为了机械镜像而破坏已经稳定的项目结构。

要求：

- 实现文件有主要 C# 承载；
- 头文件类型和常量有明确映射；
- generated/processed 与手写实现分开；
- 外部直接文件保留真实来源；
- 多对一或一对多映射写入 `native-to-managed-file-map.md` 和 Ledger。

## 4. 编码前门禁

开始生产修改前必须确认：

- 当前切片符合 `current-work-item.md`；
- 不属于当前禁止扩展边界；
- 原生声明、实现和生产调用者已定位；
- 参数、HRESULT、COM 引用和失败清理已明确；
- 现有托管实现和测试覆盖已核对；
- 目标 Ledger/映射记录存在，或明确记录其缺口；
- 测试范围和完成条件已定义。

若只有声明而无定义/调用者，或完全属于当前禁止范围，则跳过，不创建无证据实现。

## 5. 编码规则

- 使用目标框架允许的现代 C#，但不改变原语义；
- Windows COM 调用显式使用 `Stdcall`；
- HRESULT 保持首错传播；
- 输出在原生要求的位置清零；
- 成功、非零成功、普通失败、device-lost 和 driver-internal-error 分开处理；
- COM 引用按取得顺序逆序释放；
- 释放后在进入底层调用前拒绝；
- async、线程、callback 和锁按原生证据实现；
- 不添加未使用参数、接口或包装层。

## 6. 验证门禁

每个生产切片至少执行：

1. 相关定向测试；
2. 全量主测试；
3. ABI 集成测试；
4. Debug 解决方案构建。

涉及 publish、导出、资源、架构、COM 或 E2E 时，增加对应发布后和隔离进程证据。

只通过 build 不能关闭切片；只通过 probe ABI 不能关闭生产 ABI；只通过单元测试不能替代差分或 E2E。

## 7. 状态含义

- `Investigated`：依赖、职责和测试点已记录；
- `Scaffolded`：只有签名/承载，不能当实现完成；
- `Translated`：生产代码已近似直译；
- `UnitVerified`：相关 T1 通过；
- `DifferentialVerified`：原/候选差分通过；
- `Integrated`：组件生产链闭环；
- `EndToEndVerified`：参与的最高已就绪 E2E 通过；
- `Accepted`：要求的 ABI、布局、单元、差分、集成和 E2E 均满足。

状态只能按真实证据提升。

## 8. 高耦合域规则

### Generated protocol

按类型/命令/资源族推进，先冻结 ID、布局、payload 和消费者，再实现 dispatch、factory、marshal 和 render-data。不得孤立手写单个 DTO。

### Resources/UCE

先建立类型和签名骨架，再按：

`connection → channel → resource → command batch → dispatch → render target`

推进，并覆盖线程、disconnect、zombie 和停止顺序。

### HW 内容管线

在 device/resource/render-target 生命周期稳定后推进 layer、mask/effect、glyph、shader 和 pipeline builder。

### 软件呈现

保持 software rasterizer、surface、DC/DIB、GDI presenter 和 dirty/present 的所有权边界；不得用 System.Drawing、Skia 或其它高层实现替换原算法。

### 生产 ABI

Native AOT 导出保持薄边界。不能用现有 8 个 probe 测试代替完整导出、COM、资源和 PresentationCore 调用链。

## 9. 证据触发型分支

以下能力只有在原生拥有者、实际调用路径、PresentationCore 可达性、状态语义和失败顺序明确后才能实现：

- `Reset/ResetEx`；
- registry WOW64 view；
- surface ScrollBlt；
- software dirty 通知；
- 普通 render-target dummy 解绑。

没有证据时保持未实现。

## 10. 例外流程

无法近似直译时：

1. 记录原文件和具体语义；
2. 记录无法低层表达的原因；
3. 列出最接近方案及行为差异；
4. 定义差分和回退；
5. 建立 Decision；
6. 涉及范围或强约束时等待用户批准；
7. 例外不得扩展为通用重构许可。

## 11. 会话收尾

- 停止扩大范围；
- 完成验证或明确阻塞；
- 更新 Ledger、映射和稳定基线；
- 更新 `current-work-item.md` 的唯一下一动作；
- 在 `handoffs/completed-work/` 创建一个新的独立历史文件；
- 只有导航变化时才修改 `next-session-handoff.md`；
- 不向冻结的 `progress-completed-work.md` 追加内容。
