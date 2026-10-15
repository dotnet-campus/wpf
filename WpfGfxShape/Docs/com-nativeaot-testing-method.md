# 生产 COM / Native AOT 测试方法

> 用户最新约定：只写一个单元测试项目，直接使用 wpfgfx 生产项目发布的 AOT DLL。不创建 Host，不启动自定义子进程，不创建或修改任何 .slnx。

## 当前标准 MTP 与沙箱入口

当前唯一 COM 项目使用 MSTest 4.0.1、`OutputType=Exe`、`EnableMSTestRunner=true`。exe 是包生成的标准 Microsoft.Testing.Platform 验收应用，不是自建 Host；应用内直接加载生产 AOT，不能因 exe 形态误恢复已禁止的 Host。

本机与沙箱必须共用测试和原断言。沙箱构建使用 `ComAcceptanceSandbox=true`，AcceptancePaths 从 `AppContext.BaseDirectory` 解析 `Native/wpfgfx_cor3.dll` 和 `Results`，拒绝 rooted/越界路径，在加载前核对 PE 无 CLR、架构与 `aot.sha256`。本机继续使用显式绝对发布路径。包是框架依赖应用，远端需对应 .NET 10 Windows Desktop runtime。

完整命令、工作目录、显式文件白名单和上传/回收规范见 [`com-acceptance-sandbox-operations.md`](com-acceptance-sandbox-operations.md)。生产与包 publish 是独立显式动作，测试不自动 publish。本轮仅文档收口，没有新发布。

本机标准 MTP Test 参数说明（仅供专用能力使用，不授权终端，非本轮执行）：在 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/` 为工作目录执行 `dotnet test --project WpfGfxShape.ComAcceptance.csproj -c Release -- --report-trx --report-trx-filename local-final-full.trx`。结果以实际输出路径为准，禁止覆盖冻结证据。

项目级 MTP runner 的 SDK 查找与工作目录有关，传 csproj 路径不等于切换发现起点。历史包工程曾发生 VSTest target 失败且用例未启动；不计为行为 Red/Green。SDK paths/runner 的实际选择须执行代理保留原始证据，本轮未重新验证，不删改配置或升级 SDK。沙箱直接标准 exe 入口不依赖 SDK runner 选择。

运行身份、计数、strict适用版本及真实失败只见[唯一状态源](handoffs/current-stable-baseline.md)。同步perf等旧全量Green不代表当前回归；原始证据按[历史隔离索引](history-index.md)保留。本文不维护逐轮结果。

## 项目与依赖

唯一新测试项目：`Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`，使用 MSTest。

- 测试直接在测试运行器进程内用 NativeLibrary 加载生产 AOT DLL，取得真实导出并调用原生 COM vtable。
- 不引用生产托管程序集，不调用 internal 实现，不用 ABI probe 代替生产 API。
- 不创建 ComAcceptance.Host，不复制 Host 文件，不使用 Process.Start，不另建测试解决方案。
- 不恢复用户删除的旧测试。当前残留旧测试文件不由本项目包含；此项目没有任何 ProjectReference。
- 直接以 csproj 构建/测试，不依赖 slnx。

## 原生边界：生产 DLL 导出与 COM

所有生产 COM 模块验收均在唯一标准 MTP 测试运行器进程中加载生产项目发布的 Native AOT DLL，测试不得直接访问生产内部 C# 实现。独立 PublicAbi 客户端是公开 ABI 消费者，不是被禁止的自建 COM Host，也不替代唯一 COM 验收项目。“全走 COM”指使用真实原生互操作边界，而不是把原生所有入口改造成 COM 方法：

- 原生 COM 对象（如 IWICBitmap、IWICBitmapLock、IStream）通过真实 COM vtable 调用，遵守 QI、AddRef、Release 和调用约定。
- 原生 DLL 导出（如 connection/channel/resource、命令提交、位图创建）按原签名调用导出函数，使用原生规定的句柄与参数；不再包装自定义 COM 接口。
- 不引用生产托管程序集，不用反射/internal、ABI probe、fake COM 或测试专用导出替代被测生产路径。
- CopyForwardDirtyRects 等内部方法不需要为测试公开。测试从真实生产命令入口触发，由 DLL 内部完成分发与执行，再从真实生产消费者路径取得可观察结果。
- 内部方法不是公开 COM 方法，本身不是测试障碍或停工理由。缺少命令入口属于需要实现的生产依赖；只有超出已授权范围或存在无法解决的外部条件时，才记录具体阻塞并调整计划。
- 导出存在性仅是前置检查。必须补充真实行为、HRESULT、引用/句柄所有权及失败清理断言；不能因入口检查通过就报告模块 Green。

### 双缓冲端到端验收路径

加载生产 AOT DLL → 调用生产导出创建连接/通道/位图/资源 → 通过 IWICBitmap/IWICBitmapLock 写入后缓冲并释放锁 → 按原生时序标脏、保护并提交更新/CopyForward 命令 → DLL 内部分发、复制及完成通知 → 从真实生产消费路径读取结果并断言像素 → 按合同 Release/销毁资源、通道与连接。

前缓冲观察所需消费者必须通过原生合同确认并实现；不得为了断言新增 GetFrontBuffer/CopyForward 测试导出。测试无需也不得直接调用内部复制方法。完成事件只能证明通知发生，不能替代像素内容验收。

## 发布产物定位

先对生产项目 `Code/WpfGfxShape/WpfGfxShape.csproj` 发布 Native AOT 共享 DLL，测试不自动启动 publish 或脚本。

默认产物路径：`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

测试项目属性：

- `AotRuntimeIdentifier` 默认 win-x64，可指定其他已支持的 Windows RID。
- `AotDllPath` 可显式指定待测 AOT DLL；默认按本仓库发布目录计算。
- 路径在构建时写入测试程序集 metadata；改变路径后应重建测试项目。

不要求 WPFGFX_COM_AOT_DLL 环境变量。缺 DLL、普通托管 DLL、架构不匹配或缺导出都直接测试失败，不跳过。

测试记录产物绝对路径及 SHA256，并核对 PE 无 CLR header、架构匹配。它只能验证实际加载文件，不能证明其来自当前提交；发布日志/提交对应关系需单独保留。已有产物的失败不能冒称当前代码重新发布后的结果。

## TDD

1. 核对原生声明、导出及实现，先写测试。
2. 用生产 AOT DLL 运行测试，记录具体失败，而非把缺环境配置视为 ABI Red。
3. 根据合同失败实现生产功能；重新发布 DLL并启动新的测试运行验证 Green。
4. 保持同一组断言，不以新增测试专用入口或改变预期错误绕过失败。

## 首批 EventProxy 合同

证据：`core/api/exports.cpp:895-910`、`core/dll/wpfgfx.def`、`include/wgx_render.h:82,852-859`、`core/av/eventproxy.cpp`。

下表保存 EventProxy 必须验证的合同，不维护用例总数。首批测试的 Red/Green 只属其历史发布：旧产物缺导出的共同前置 Red 不等于方法体断言逐项 Red；生产实现后必须重新执行。

| 合同 | 测试覆盖 |
|---|---|
| 创建参数及失败所有权 | 空输出、空 descriptor、两者皆空；失败不 dispose 调用方 descriptor |
| QI 合同 | 空输出 E_POINTER；未知 IID 清空哨兵；规范 IUnknown identity |
| QI 引用所有权 | IUnknown/IMILEventProxy 查询引用在创建引用释放后仍有效；失败 QI 不保留额外引用；失败后仍可回调 |
| HRESULT | S_OK、S_FALSE、E_INVALIDARG、E_FAIL 精确回传；前次失败不屏蔽下一事件 |
| payload | 空指针/零长度；含 NUL 和高位字节的完整快照、长度、原指针及调用方数据不被修改 |
| descriptor | 创建后清空调用方 descriptor，原生副本仍可回调和 dispose |
| 对象生命周期 | 未调用即释放；额外 AddRef 防止提前销毁；最终释放一次；不同对象回调上下文及销毁隔离 |

原生依据用于约束预期，不用替身模拟待测 COM 对象。产物缺导出时初始化失败会阻止方法体执行；共同前置失败不代表所有行为断言有效，导出实现后必须再次运行全部用例。

IUnknown 三槽之后是 RaiseEvent。Shutdown 不是公开槽位，不能为测试虚构。最后 Release 后不能再调用对象。回调中不执行可能跨 unmanaged 边界抛出的断言，断言在测试方法中执行。

## 生命周期与运行限制

测试对象应在 finally 中释放；按[已生效产品合同](decisions/DEC-0004-process-lifetime-module.md)，生产模块保持加载到进程结束、不被运行期替换；unload/reload/热替换不适用（非通过、非阻塞），无需再批准。COM Release、channel/player关闭、线程退出、callback静止及初始化失败资源清理仍须验收。测试程序集禁止并行执行以降低共享本机状态干扰。

直接测试真实 DLL 意味着本机崩溃可能终止测试运行器，这是本方案的真实失败结果，不通过自定义 Host 隐藏或转换。死锁保护可由标准测试运行器/CI 的超时设施提供，不新增进程调度框架。

COM 跨 apartment、异步媒体通知、重入/并发关闭、真实播放、进程退出 SEH、原生差分及架构矩阵仍需后续测试；当前用例全部通过也不等于完整 wpfgfx 可替换。callback 内 QI/平衡 Release 重入已覆盖；后续在确认真实公开路径后，扩展发布架构矩阵；受控并发引用已覆盖非最终 Release 不等待回调及 2/4/8 工作者独立引用—回调重入—最终 dispose，但不代表穷举线程交错；不得虚构 Shutdown 槽位或对原生未承诺的无效函数指针要求 HRESULT。

## ManagedStreamWrapper 合同

ManagedStream 必须覆盖 MILCreateStreamFromStreamDescriptor、MILIStreamWrite、IUnknown/IStream/IManagedStream QI、descriptor副本、13个委托槽的参数/HRESULT及最终释放。按原生保留 QI 空输出 E_INVALIDARG、不支持 ISequentialStream QI，以及 Write/Seek/CopyTo 可空输出临时存储；Read输出原样转发。双流合同包括生产对象间复制、四种输出组合、EOF、真实Clone存活、读写定位/扩缩容、STATSTG无名称字段及最终释放。内存流descriptor仅为调用方来源，不等同真实PresentationCore/WIC集成；克隆采用快照与独立位置。历史执行计数不在方法页维护。

## 标准结果留存

测试 csproj 通过 RunSettingsFilePath 加载 acceptance.runsettings，启用标准 TRX logger。`TestResults/com-acceptance.trx` 保存成功用例的 TestContext 路径与 SHA256，后续运行覆盖同名文件；每轮将具体哈希、run ID 和结果记录到独立完成归档，不能把上轮哈希沿用为新发布身份。

## 维护

长期方法仅维护本文；逐轮结果放 completed-work，下一任务放 current-work-item。旧归档中的 Host 和环境变量做法已经被用户撤销，不再作为实施依据。
