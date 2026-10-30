# 章程：目标、实现与 ABI 合同

> 用户已撤销原版 wpfgfx 生产媒体委托。不得保留、加载、复制改名原版作为候选媒体服务；仅测试独立 oracle 可加载官方。允许 Windows 公开媒体组件。普通媒体必须实现，但未实现时工厂明确 E_NOTIMPL/null，不能假成功。历史委托绿不认证独立媒体，全部独立媒体能力 Open / NotImplemented。见[删除报告](completed-work/replacement-remove-original-media-delegation.md)。

本页为[章程](00-migration-charter.md)§1–7原规范正文的主题承载，强制执行；不是摘要或状态源。

## 1. 目标

将现有 WPF `wpfgfx` 原生图形层逐步迁移为 C#/.NET 10 Native AOT 原生库，继续导出兼容 C ABI，尽可能不修改或仅最小修改现有 WPF 托管调用方。
首先追求可证明语义等价、ABI 兼容和完整流程可运行；现代化、抽象优化与性能改进属于等价迁移完成后的独立阶段。

## 2. 约束优先级

冲突时严格依次：
1. 正确性与可验证行为等价。
2. C ABI、二进制布局、调用约定和生命周期兼容。
3. 逐文件、可审计、可回退迁移。
4. 可构建、可测试、可跨会话继续。
5. 迁移速度。
6. 简洁度、现代化、抽象质量与性能优化。

不得以优雅、现代或性能损害前四项。

## 3. 最强实现约束：逐文件近似直译，禁止提前重构

本原则服从正确性/可验证等价与 ABI/生命周期兼容，不授权机械照抄已确认错误或绕过验证。
- 初次迁移以原生源码为基准，按文件逐个迁移；原则上一个原生实现文件对应一个主要 C# 文件。
- 保留原类型/函数/字段/常量名、分支、调用/错误处理/锁顺序、引用计数及释放顺序。
- C# 目录尽量镜像 `common/core/shared` 原始边界；单生产项目统一构建发布，不允许迁移期合并职责或重设计架构。
- 每文件记录原始/新路径、直接/未解析依赖、测试状态及已知差异。
- 一次变更只处理可说明的迁移单元，不顺带清理；完整模块可包含紧密耦合文件，粒度见[执行规则](handoffs/current-execution-rules.md)。
- 编码前必须有 Ledger 行、路径映射及预定义验证证据；没有台账与测试定义不得翻译。

完整等价验收前禁止：
- 重构、抽象重组、职责合并或拆分。
- 为 C# 习惯改变算法或控制流。
- 提前优化或改变缓存、并发模型。
- 删除未证明不可达的分支、状态、兼容逻辑、错误码。
- 多个原生文件任意揉成不可追溯实现。
- 无差分证据用第三方高层封装替代底层语义。
- 以测试困难跳过生命周期、失败、设备丢失、线程或释放。

无法逐字表达时采用最接近低层表达，台账记录偏差、原因、风险、验证，不得静默改语义。

## 4. ABI 与导出兼容原则

- 导出集合、名称、参数顺序、返回值、调用约定与结构布局均为外部契约。
- 托管 P/Invoke、原生头、def、生成导出、链接配置、实际 PE 都是事实来源；不得由单一文件推完整导出面。
- AOT 导出是薄边界：校验参数、转换 ABI 类型、封锁异常、调用内部等价实现。
- 托管异常不得跨 C ABI；HRESULT/错误码映射与原实现一致，或明确记录并验证。
- 逐项确认 COM/refcount、handle 所有权、callback 指针、编码、bool 宽度、enum 基础类型、对齐/pack。
- 替换前必须有自动化 ABI 清单与可重复兼容性验证。

## 5. 项目形态原则

初始方案有反证时必须形成书面决策，不得擅改：
- 唯一生产项目 `WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`，.NET 10，Native AOT 原生库及兼容 C 导出，承载全部迁移生产逻辑。
- 主测试 `WpfGfxShape/Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj` 使用 MSTest，不并入原 WPF 测试工程。
- 单项目仅指生产边界；独立测试、caller/harness、文档/基线不计入生产数量；不再要求 C++ NativeCaller，见 DEC-0003。
- 隔离 WPF 全局构建属性、包版本、TFM、生成流程；不得反改原源码掩盖缺陷。
- 项目骨架与最小导出闭环通过前不开始大规模翻译。
- 生产 COM 唯一 ComAcceptance 项目直接加载生产 AOT，标准 MTP（含沙箱 exe），禁止自建 Host；独立 PublicAbi 客户端不因“独立进程”被当作禁用 Host，见[方法](com-nativeaot-testing-method.md)。

## 6. DirectX 与 Silk.NET 原则

- 版本及已测生产方式只认[基线](handoffs/current-stable-baseline.md)；存疑 API 读对应版本源码。
- 旧本地仓库调查仅历史风险，不能覆盖当前包；Silk.NET 不免除 ABI/调用约定/AOT 验证。
- 使用前提是保持调用语义、COM 生命周期、布局、错误处理；只作低层互操作，不重写高层渲染架构。
- 未覆盖、版本不匹配或语义不透明可最小手写互操作，但需专题记录和测试。
- 分别盘点 D3D9/D3DEx/DWrite/WIC/DXGI/DWM/GDI/COM/Windows API，不将 DirectX 视作单一依赖。

## 7. 测试优先级与增量门禁

测试随迁移增长，每批先定义验收证据再翻译：
1. ABI/布局：名称、调用约定、参数、size/offset、enum、HRESULT。
2. 纯逻辑：数学、几何、颜色、像素、区域、解析、状态转换。
3. 同输入/状态的原生与候选差分输出和副作用。
4. refcount、释放、异常封锁、device loss、无效参数、低资源。
5. 通道、资源、SW/HW/呈现组件集成。
6. 真实或最小 PresentationCore 进入导出完成可观察渲染。

无测试或差分证据只能称代码草拟，不称完成。E2E 不全推迟最后：probe、版本/工厂、connection/channel、资源命令、SW 像素、真实 RenderTargetBitmap、HW/HWND、D3DImage、媒体、关闭/退出逐步扩展。后级未就绪不阻止前级存在，前级通过不替代后级。
DLL 驻留，unload/reload/热替换依[DEC-0004](decisions/DEC-0004-process-lifetime-module.md)不适用；资源/线程/callback 清理不减。
