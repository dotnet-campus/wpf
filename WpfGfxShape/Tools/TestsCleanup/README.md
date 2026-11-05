# TestsCleanup

.NET 10 单文件一次性整理程序：`CleanupTests.cs`。固定产品根目录为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`，与启动工作目录无关。非幂等，无存在性或链接检查、备份、manifest、异常捕获；失败直接中断。仅创建迁移目标目录、移动文件和删除目录，不读取或重写被整理文件内容。不处理 artifacts，不改变现有 dotnet 输出机制。

## 执行范围

先原样移动 `Tests/WpfGfxShape.AcceptanceLifecycle.Tests/DispatcherWorkersTests.cs` 到 `Tests/WpfGfxShape.ComAcceptance/HarnessTests/DispatcherWorkersTests.cs`，保留四项测试。目标项目已有 MSTest 4.0.1、相同 net10.0-windows/WPF 框架、全局测试 using 和 DispatcherWorkers 源码；测试通过显式 using 访问 NativeAcceptance.Pcore，原 namespace 无须改变，无资源或工作目录依赖，也无新增 STA 要求。目标项目已有程序集级 DoNotParallelize。AcceptancePaths、AcceptancePathsTests、资源和 DispatcherWorkers 已在正式项目中，不重复迁入；TestDirectory.ModuleInitializer 不迁入。正式 AssemblyInitialize 的 Native 验收前置保持不变，迁入测试运行时也受该前置约束。

随后递归删除以下 14 个 Tests 子目录，预计项目数 **19 → 5**：

- CandidateMediaCallbackR20
- CandidateMediaFailureR16
- CandidateResize
- CandidateResizeDesktop
- CandidateResizeDesktopSupervisor
- CandidateResizeSupervisor
- CandidateVideoSupervisor
- CurrentSourceAuditR15
- DesktopByteAuditR17
- DesktopObservationReadbackR16
- DesktopWgcR17
- StrictRenderReadbackR28
- SupervisedNativeIdentity
- WpfGfxShape.AcceptanceLifecycle.Tests

最后删除 `Tools/RepositoryOrganization`。不重复删除已不存在的 DocumentationMaintenance，不删除当前 TestsCleanup。

## 保留的 5 个项目

- **WpfGfxShape.Tests**：正式托管测试；ComAcceptance 仍引用该项目及其中的媒体测试源码。
- **WpfGfxShape.ComAcceptance**：正式 COM/Native 验收，接收 DispatcherWorkers 四项测试。
- **StrictRenderR28**：保留严格渲染消费者及官方运行时隔离能力。
- **StrictRenderSupervisorR28**：保留独立父进程监督、模块身份门禁、零阈值像素及契约比较；Program.cs 自包含，不引用 CandidateVideoSupervisor 的 InitializationClosure/VideoRun。
- **CandidatePureVideo**：真实源码供体；StrictRenderR28 Compile 链接其 Program.cs 和 GeometryR27.cs，Supervisor Content 也引用两份源码。不能在不改现有项目路径的情况下删除，并非为保留历史而留存。

已核对 19 份 csproj 和 Strict Supervisor 实现，以上保留项目未发现对拟删项目的源码链接、Compile Include 或 Content 依赖；拟删项目内部相互引用不阻碍整体删除。没有扫描 artifacts 证据。

目前未实现最终 2 项目：合并 Strict 消费者/监督者的独立入口以及迁移供体，需要内容级入口、条件编译和项目路径调整，超出仅移动删除范围；本次仍实际提供完整 14 项删除清单。后续完成整合的工具应最后删除自身及本次遗留的 TestsCleanup 工具目录。

## 验证边界

仅使用 .NET 10 单文件 build 编译校验 CleanupTests.cs，不执行程序，不模拟移动删除。脚本编译不等于迁移后全部测试编译或运行通过；**没有产品编译保障**，Native、参考数据和验收环境仍须按原要求准备。
