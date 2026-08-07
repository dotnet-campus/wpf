# wpfgfx 测试证据与渐进 E2E Harness 契约

> 状态：权威 harness/evidence 契约；`E2E-00` 托管 ABI harness 已落地并执行通过；Ledger evidence 索引已启动；原 Release win-x64 DLL 已定位但尚未形成 hash/loader 运行 golden  
> 测试层级与门禁：[`06-testing-strategy.md`](06-testing-strategy.md)  
> ABI schema：[`08-abi-manifest-spec.md`](08-abi-manifest-spec.md)

## 1. 目的

本文把 `T0–T5` 与 `E2E-00–E2E-09` 从原则表转为后续可执行的项目、进程、输入、输出和证据契约。当前 MSTest 项目已经创建并引用 `MSTest` `4.0.1`；其还原、测试发现和运行结果仍需由后续执行证据确认。

测试的目标不是证明“C# 代码看起来合理”，而是持续证明：原实现与候选实现的 ABI、字节、状态、副作用、线程、所有权和真实 PresentationCore 流程等价。

`WP-00I` 已创建 `Ledger/evidence.jsonl` 作为证据 ID 索引。预定义但尚未执行的证据必须显式标记 `Planned` 或 `Deferred`，`satisfiedEvidenceIds` 只能引用真实已执行/已取得的证据；人工 JSONL 复核不得冒充机器 schema 校验。

## 2. 预期承载边界

```text
WpfGfxShape/
  Tests/
    WpfGfxShape.Tests/
      WpfGfxShape.Tests.csproj         # T1：MSTest；纯逻辑、布局辅助、manifest/schema
    WpfGfxShape.AbiIntegration/        # T2：MSTest 发布与子进程编排
    WpfGfxShape.AbiIntegration.Host/   # T2：独立托管进程、resolver、真实 P/Invoke ABI
    NativeBaselineAdapter/             # T3：原实现可观察适配；不改原产品源码
    Differential/                      # T3 编排与结果比较
    ComponentHarness/                  # T4 子系统闭环
    PresentationCoreE2E/               # T5 未修改真实 WPF 调用链
    TestAssets/                         # 小型确定性输入；大型媒体另行管理
  artifacts/test-results/
```

约束：

- 一个生产项目仍是唯一生产构建边界；上述均为独立测试/工具承载。
- 测试不得 `ProjectReference` 原 WPF项目，也不得递归构建原 WPF。
- 原实现只能通过冻结的原生 DLL/独立 adapter 进入差分。
- ABI/loader/crash/unload 测试必须在独立托管子进程中通过真实 P/Invoke 进入发布 DLL，不能仅调用内部普通方法。
- 测试使用 `NativeLibrary.SetDllImportResolver` 将固定库名解析到发布 DLL 绝对路径，并校验实际模块路径和 SHA-256。
- PresentationCore E2E 原则上不修改其 MilCore P/Invoke 声明；通过隔离部署和进程边界选择 original/candidate。

## 3. 测试框架与包版本决策

托管测试框架锁定为 MSTest。`WP-00A` 必须根据独立还原证据锁定其包版本。执行规则：

1. 当前测试项目显式引用 `MSTest` `4.0.1`；是否还需拆分声明 adapter/SDK 依赖，以实际还原、测试发现和项目求值证据为准；
2. 不继承根 `eng/Testing.targets` 的包注入；
3. 与 .NET 10、AOT 项目引用和当前 IDE Test Explorer 兼容；
4. 不因追求统一而修改 WPF 原测试项目；
5. 版本和来源写入局部 package policy 与 evidence manifest；
6. 托管测试项目和测试辅助代码位于 `WpfGfxShape/Tests/WpfGfxShape.Tests`，其它 harness 位于 `WpfGfxShape/Tests` 的独立子目录。

## 4. 测试运行单元

每次运行必须是一个可重放 run：

```text
artifacts/test-results/<work-package>/<run-id>/
  manifest.json
  environment.json
  commands.jsonl
  tests.trx-or-equivalent
  stdout.txt
  stderr.txt
  artifacts.jsonl
  original/
  candidate/
  diff/
  dumps/
```

`run-id` 建议：`<utc>-<configuration>-<rid>-<short-revision>-<sequence>`。

### 4.1 `manifest.json` 必填

- schema version；
- work package、Ledger/ABI/evidence IDs；
- OriginWpf 与 WpfGfxShape revision/dirty 状态；
- Silk.NET revision（若使用）；
- SDK、MSBuild、C/C++ toolchain、Windows SDK；
- OS/build、process architecture、CPU/GPU/driver、session/RDP 状态；
- configuration、RID、original/candidate role；
- DLL绝对路径、回读模块路径和 SHA-256；
- test filters、seed、timeout；
- start/end、exit code、result；
- skipped/not-run 项及精确原因；
- golden update 是否发生及 Decision ID。

### 4.2 `commands.jsonl`

每个 build/publish/test/inspection 操作记录：工具、完整参数、工作目录、环境覆盖、开始/结束、退出码和输出文件。若通过 IDE 执行，记录等价操作名称、配置和产物路径。

## 5. 角色与进程隔离

每项运行标明角色：

- `Original`：冻结原 `wpfgfx_cor3.dll`；
- `CandidateProbe`：只有非生产 ABI probe；
- `CandidateProduction`：按工作包实际已实现能力；
- `Harness`：托管 ABI 集成测试宿主/PresentationCore app；
- `Comparator`：不加载被测 DLL，只比较落盘结果。

以下必须一 case 一子进程：

- 错误架构或缺失依赖加载；
- 可能栈破坏的 x86 ABI；
- unmanaged/managed exception 边界；
- fail-fast/crash/dump；
- COM final release/线程终止；
- callback detach race；
- `FreeLibrary`/reload；
- device loss/driver reset；
- process-exit 与强制终止。

父进程只判定退出码、超时、dump 和结果文件，不在同进程继续其它测试。

## 6. 原/候选切换协议

每个 harness 接受显式参数或环境文件：

- role；
- DLL绝对路径；
- expected SHA-256；
- architecture/configuration；
- output directory；
- deterministic seed；
- timeout；
- optional feature/environment labels。

加载后必须回读实际模块路径和 hash。禁止依赖 PATH、当前目录或覆盖原 WPF artifacts 来“切换” DLL。

PresentationCore E2E 的部署策略必须在首次实现前由 spike 裁决，候选包括：

1. 每个子进程使用独立完整部署目录，目录中只有指定 role 的 `wpfgfx_cor3.dll`；
2. 受控 host/loader 环境，在不改 P/Invoke 声明的前提下隔离搜索路径；
3. 若运行时明确支持且不改变生产声明，可使用测试进程级 resolver，但必须证明真实名称、路径和加载时机仍等价。

不允许修改 PresentationCore 源码或声明使 candidate 更容易加载。

## 7. 结果模型

每个 case 输出机器可比较 JSON：

- `caseId`、`role`、`status`；
- 输入/seed；
- 返回值、HRESULT、Win32 error（仅原语义使用时）；
- out 参数与结构原始字节；
- callback/lock/refcount/resource/thread 事件序列；
- 像素/字节/文件工件 hash；
- exception/crash/dump；
- elapsed/memory 仅作诊断，不作为等价优化目标；
- nondeterministic fields 及其规范化规则。

Comparator 独立读取 original/candidate 结果，应用预先批准的比较规则，不让 candidate 测试代码自己判定等价。

## 8. Golden 与差分规则

- 整数、ID、HRESULT、布局、命令、引用计数事件和默认像素路径逐位/逐字节比较。
- 浮点容差必须先从原实现分布冻结，记录 absolute/relative/ULP 规则及适用字段；无依据不得放宽。
- 图像保存 original、candidate、normalized、diff 与统计；截图肉眼相似不算证据。
- 线程时间戳可规范化为逻辑序号，但线程身份、相对顺序、锁和 callback 数不能删掉。
- 地址、PID、时间等不稳定值只在 schema 明确标记后规范化。
- golden 更新必须由专用操作触发，附旧/新 hash、原因、审阅人/Decision ID；普通测试失败不得自动覆盖。
- 原始原 DLL基线只读；candidate 不得写入 original 目录。
- 当前已定位的 `.NET 9.0.5` Release win-x64 `wpfgfx_cor3.dll` 只有来源/版本/配置归因；在 SHA-256、模块回读和 PE 检查完成前只能作为 original baseline candidate，不能作为不可变 golden。

## 9. E2E 阶梯可执行合同

### `E2E-00` Native AOT probe

- Harness：MSTest 编排的独立托管 ABI 集成测试进程。
- 入口：固定 P/Invoke 库名通过 `NativeLibrary.SetDllImportResolver` 解析到指定 publish DLL 的绝对路径。
- 输入：probe protocol v1 的 success/error/exception/unknown；并发首次和重复调用。
- 结果：精确 export、PE、预期/实际路径、SHA-256、HRESULT、out、异常封锁。
- 明确不证明：任何生产导出、COM、渲染或 PresentationCore。

### `E2E-01` version / minimal COM

- `MilVersionCheck` 正确/错误版本。
- 独立受控 IUnknown-like object spike；只有机制通过后，才在真实 factory 工作包增加 `MILCreateFactory`。
- 验证 vtable、QI/AddRef/Release、空参数、所有权和双运行时边界。

### `E2E-02` connection/channel

- create SameThread/CrossThread connection；create channel；marshal type；close/commit/destroy/disconnect。
- 覆盖错误顺序、重复关闭、部分初始化和线程身份。

### `E2E-03` resource command

- 最小 resource create/addref/duplicate/send/release；固定命令、payload、malformed、back-channel message。
- 校验 32 位 resource ID、packet bytes、router/factory 和失败码。

### `E2E-04` software pixels

- 直接组件 harness 建立最小 software bitmap/render target。
- 输入为无字体/媒体依赖的确定性几何、颜色和变换。
- 输出规范化像素；与原实现逐字节或经批准规则比较。

### `E2E-05` 真实 PresentationCore bitmap

- 独立 WPF app：`DrawingVisual -> DrawingContext -> RenderTargetBitmap.Render -> CopyPixels`。
- 图形至少含填充矩形、线/几何、透明混合和变换；首版避免字体、媒体、用户 GPU surface。
- 同一 app 分别在 original/candidate 独立进程运行；比较像素、HRESULT/异常、事件与退出。

### `E2E-06` hardware/HWND

- 真实 HWND 呈现与硬件 bitmap；明确 GPU/driver/RDP 标签。
- 覆盖 device create/reset/lost、SW fallback、呈现和关闭。

### `E2E-07` D3DImage

- D3D9 user surface；front-buffer callback；version；dirty rect；detach；software fallback；device loss。
- callback 来自 composition/render thread，UI 更新经 Dispatcher；detach 后禁止新 callback。

### `E2E-08` media

- 受控小型媒体资产与系统组件清单。
- open/event/frame/position/stop/close/shutdown/process-exit；COM apartment、state/event thread、超时与 callback。
- 缺少系统组件是明确环境结果，不与实现失败混淆。

### `E2E-09` lifecycle closure

- 多 Dispatcher；重复创建/关闭；显式关闭；finalizer/SafeHandle；进程退出；受官方支持范围内 unload/reload；失败注入和后台线程静止。
- 只有所有目标架构/配置和批准环境矩阵通过后才可称完整替换。

## 10. 环境标签与能力检测

每个测试声明 `requiredCapabilities`，例如：

- `WindowsDesktopInteractive`；
- `RealArm64Host`；
- `D3D9Hardware`、`D3D9Ex`；
- `WIC`、`DWrite`、`WMP`、`EVR`、`DXVA`；
- `RdpSession`、`ForceSoftware`；
- `SupportsNativeAotUnload`（只能由官方/运行证据设定）。

缺少能力的结果是 `NotRun-CapabilityMissing`，不是 pass。工作包 done 条件若要求该能力，则仍保持 blocked。

## 11. 失败注入

失败点以稳定 ID 建账，至少覆盖：

- allocation/heap；
- lock/init；
- loader DLL/symbol；
- COM QI/create；
- channel/service channel；
- resource handle/factory；
- callback registration；
- worker thread/event；
- device/resource；
- media state transition。

失败注入不得通过修改原产品源码语义实现。原实现若需 adapter，应保持产品函数不变，记录 adapter revision 和可观察范围。

## 12. Evidence ID 与状态

格式：`EVID-<work-package>-<level>-<case>`，例如：

- `EVID-WP-00B-T2-PROBE-X64-DEBUG-PINVOKE`；
- `EVID-WP-04A-T3-EXACT-ARITHMETIC-RANDOM`；
- `EVID-WP-09X-T5-E2E-05-RTB-PIXELS`。

每个 Evidence 记录状态：`Planned`、`ExecutedPassed`、`ExecutedFailed`、`NotRun`、`Invalidated`。只有 `ExecutedPassed` 可满足 Ledger/ABI requirement；SDK、源码、golden 或规则改变时相关证据标记 `Invalidated` 并重跑。

## 13. 每个文件编码前的测试合同

工作包必须预先写明：

- 原文件/头/生成关系；
- observable 输入、输出、副作用；
- normal/boundary/error/release cases；
- original baseline 取得方式；
- exact/tolerance 规则；
- target configuration/RID/environment；
- T1、T3、T4、T5 分别需要哪些 Evidence ID；
- 尚未就绪的高层 E2E 为什么不阻塞当前较低状态；
- 禁止用测试便利改变生产控制流。

## 14. 超时、重复与随机测试

- 每个 case 有硬超时；超时后保存 dump并终止子进程。
- 并发/生命周期测试记录重复次数和 seed。
- 随机/属性测试必须可重放；失败保存最小化输入与原 seed。
- 不能只运行一次 happy path 关闭有状态工作包。

## 15. 当前已锁定与未锁定项

已由 `WP-00B-MANAGED-PINVOKE-ABI-INTEGRATION` 锁定：

- MSTest 包版本为 `4.0.1`；
- T2 采用 `WpfGfxShape.AbiIntegration` 编排项目与 `WpfGfxShape.AbiIntegration.Host` 独立子进程宿主；
- 固定库名由 resolver 解析到 publish DLL 绝对路径；
- Debug/Release `win-x64` 协议、并发、缺失 DLL、缺失导出、无效 PE、错误架构、模块路径和 SHA-256 已执行通过；
- 本轮全量结果：T1 10/10、T2 8/8 通过。

仍留给后续工作包裁决：

- PresentationCore candidate 隔离加载的具体机制；
- 大型原 DLL/媒体/图像工件是否提交仓库或保存在受控外部存储；
- ARM64 与硬件/媒体测试机器供给；ARM64 ABI 测试代码已就绪，但真实 ARM64 Windows 运行证据仍缺失；
- 目标 .NET 10 SDK 的 Windows x86 Native AOT shared-library 支持证据；当前状态为 `BlockedByPlatformEvidence`；
- 官方支持范围内的 Native AOT unload/reload 与 crash/fail-fast 场景。

内部 `NativeAotAbiProbe.Invoke(...)` 测试只计 T1，不得登记为 ABI 通过。