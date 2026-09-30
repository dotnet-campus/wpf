# 当前会话执行约束

> 本文只保存每轮恢复工作都必须遵守的执行约束，不记录完成历史或下一目标。
> 当前入口见 [`../next-session-handoff.md`](../next-session-handoff.md)。

## 每轮执行规则

- 用户明确要求按可独立验收的生产大模块规划和开展工作，而不是连续小切片。一个模块应合并生产方法族、实际消费者接线、成功与失败路径、生命周期、AOT发布验收及文档收口；这些是模块内部步骤，不单独占下一轮。纯覆盖增强或失败分支补测不得作为默认的独立下一轮；仅在已记录的真实技术阻塞下缩小交付范围，并保留原模块完成条件。

- 先扫描当前入口与工作切片，再选择一个可独立验证的原生大块切片，完成近似直译、测试、构建和文档同步。默认以完整原生方法、紧密耦合的方法族、完整生命周期或可端到端验证的生产闭环为一轮粒度；除非存在明确技术阻塞，不再把同一闭环拆成仅定位、仅计数、仅分配、仅转换、仅接线等连续小切片。
- 交接文档中的“唯一下一动作”必须持续遵守上述大块粒度。若因原生证据缺失、风险边界或外部阻塞而缩小任务，必须同时记录阻塞证据、当前可交付边界以及后续如何重新合并成完整闭环；不得只因实现方便而规划小任务。
- COM 测试只写一个 `Tests/WpfGfxShape.ComAcceptance` 单元测试项目，直接加载生产 AOT DLL。禁止创建/恢复 ComAcceptance.Host；禁止创建或修改任何 .slnx。不得恢复用户删除的旧测试方案。
- 生产 COM 导出采用 TDD：单元测试进程直接用生产项目的 Native AOT DLL，不引用生产托管程序集、不启动自定义子进程、不调用内部实现冒充验收。直接按 csproj 构建/测试，不自动启动发布命令。完整测试方法统一维护于 [`../com-nativeaot-testing-method.md`](../com-nativeaot-testing-method.md)。
- 原生证据只从 `src/Microsoft.DotNet.Wpf/src/WpfGfx/` 枚举和读取；每次最多读取 400 行。
- Silk.NET ABI 或类型存疑时读取本地 Silk.NET 2.23.0 源码。
- 保持显式 Windows `Stdcall` COM ABI、HRESULT 首错传播、COM 引用计数、失败逆序清理、确定性释放和释放后保护。
- 不修改原 WPF/wpfgfx 源码；不以低优先级文档或验证阻塞生产翻译。
- 原生直接路径没有 `Reset/ResetEx` 证据，不得新增就地 Reset。
- 进度报告必须区分“本轮切片进度”和“当前阶段状态”；单个切片达到 100% 不得表述为项目总进度 100%。在生产分母不可确定计算时，不报告整体完成百分比，只报告已正式关闭的阶段、当前 Active 阶段、Pending 阶段、硬阻塞项和替换资格。
- 每轮完成事实必须在 `completed-work/` 中创建一个新的独立历史文档；不得追加 `progress-completed-work.md`，也不得拆分或迁移该旧文件。
- 每次实际发布 Native AOT DLL 都必须在本轮 `completed-work/` 文档记录：发布序号、完整命令、目标项目/配置/RID、成功或失败及可核实的诊断、产物路径；测试取得的 SHA256 和 TRX 必须对应具体发布，未取得时明确注明。不得只记录 build 或测试结果而遗漏发布动作。

## 当前禁止扩展边界

没有明确原生契约前，不得虚构或扩展：effects/UCE/资源协议、生产 ABI、完整 glyph 私有模型、shader bytecode、GDI presenter、software-DC present context、兼容 DC/DIB、`Reset/ResetEx`、registry WOW64 view、surface ScrollBlt、software dirty 通知、普通 render-target dummy 解绑、完整 layer stack、几何 mask/effect、普通 scene 生命周期。

Dummy back-buffer 的初始化、失败释放、析构顺序与测试隔离已闭合；当前 render-target 非拥有身份和 scene/失败清理状态机也已有明确原生证据。后续只可按独立验证切片逐步实现当前身份、`ReleaseUseOfRenderTarget` 与 present 失败解绑，不得脱离对应原生调用顺序扩展普通 scene 或 render-target 生命周期。
