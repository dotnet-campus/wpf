# 测试策略：分层、E2E 阶梯与批次门禁

[测试策略](06-testing-strategy.md)§1–5完整主题合同；状态只见[基线](handoffs/current-stable-baseline.md)。

## 1. 测试目标

同时证明：内部文件语义、发布 Windows C ABI、资源/COM/线程/callback/失败等价、同输入的结果与副作用差分、未修改真实 PresentationCore 可观察流程。
T0–T5是证据分类，不授权额外项目或恢复历史GenProto/Ledger治理门；仅Tests托管与ComAcceptance原生两项目，最新用户组织裁决优先，产品行为目标不变；真实消费者为托管 P/Invoke，不要求 C++ NativeCaller、静态 import caller、lib 链接或纯原生消费者，见[DEC-0003](decisions/DEC-0003-managed-pinvoke-abi-integration-tests.md)。

## 2. 证据分层

### T0：静态清单与来源证据

- 项目/文件/生成/ABI/资源 Ledger，原路径到 C# 映射。
- 原/候选 DLL、SDK、RID、configuration、revision、hash。
- def 与托管 EntryPoint 的交集/差异；静态历史数量由 ABI manifest 复核，不是运行分母。
- 未运行明确标记，搜索不冒充二进制事实。

### T1：托管单元与布局测试

纯算法、错误码、enum、状态转换、command parser、异常映射 helper；覆盖边界、退化、溢出、NaN/Inf、checked/unchecked、Unsafe.SizeOf/Marshal.SizeOf/offset、固定 BOOL/C++ bool/handle/pointer/UInt64 槽及每文件失败前置，不仅 happy path。
不能证明发布 DLL、导出、实际加载路径或进程失败行为。

### T2：Native AOT 发布后托管 ABI 集成测试

主链：publish → 独立托管测试进程 → resolver → 真实导出 → P/Invoke。
- 每 Configuration/RID 先取得对应生产发布产物，仅加载对应 publish DLL；发布是显式独立动作，COM 测试不自动发布。
- DllImport/LibraryImport/unmanaged pointer 调用真实 UnmanagedCallersOnly 导出。
- NativeLibrary.SetDllImportResolver 指向绝对路径，不依赖默认搜索；核验预期/实际模块路径、SHA256。
- 专用检查器验证 PE machine、export ordinal/kind、资源、版本。
- 覆盖 HRESULT/BOOL/void/out、异常封锁与恢复、并发首次/重复进入、错误架构/缺 DLL/symbol/路径。
- x86 支持矩阵用真实 WPF 一致声明验证代表签名与连续调用。
- crash/fail-fast 用标准运行器或独立编译验收应用隔离；生产 COM 不建 Host 或自定义子进程，见[方法](com-nativeaot-testing-method.md)。独立 PublicAbi 客户端不是被禁 COM Host。
- 驻留 DLL unload/reload/热替换依 DEC-0004 不适用，不记通过/阻塞，资源清理仍验收。

### T3：原实现与 C# 差分

同一向量分别进入冻结原 DLL/原生 adapter 与候选 AOT/C#。
比较返回/out/错误码、完整结构/bytes/像素、AddRef/Release/创建销毁/锁事件、callback 次数/线程/顺序/detach 后行为、失败部分状态及后续可用性。
无独立入口时用不改原产品语义 adapter 或公开入口间接观察；无法建立原基线保持 Blocked，不用重写公式自证。

### T4：组件集成

初始化/错误/loader；connection/channel/batch/message；resource handle/factory/update；SW target/pixel；HW device/resource/fallback；AV state thread/event/surface；shutdown/join/release。
每个闭环有确定输入、可观察输出与失败路径。

### T5：真实 PresentationCore E2E

不修改 MilCore 声明帮助候选；受控隔离部署选择原/候选。
首个强制真实路径：DrawingVisual → 无字体/媒体依赖确定图形 → RenderTargetBitmap.Render → 规范化像素 → 原版比较。
现有 PresentationCore.Tests 的初始调查未找到可复用贯通测试，现由ComAcceptance/Pcore辅助源码及同exe角色承载，不独立建第三测试工程，不并入生产。

## 3. 渐进 E2E 阶梯

| ID | 必须新增能力/结果 |
|---|---|
| E2E-00 | 非生产 probe，仅 shared/export/PInvoke 机制 |
| E2E-01 | MilVersionCheck、受控 IUnknown/factory；version/error/vtable |
| E2E-02 | connection/channel create/close/commit/destroy；pointer handle、线程/批次寿命 |
| E2E-03 | resource create/addref/send/release、消息失败；32位 ID、protocol/router/factory |
| E2E-04 | SW bitmap/target 确定像素，SW/scanop/资源链 |
| E2E-05 | 未修改真实 DrawingVisual→RenderTargetBitmap→pixels |
| E2E-06 | HW bitmap/HWND present/SW fallback；D3D9/交换链 |
| E2E-07 | D3DImage front-buffer callback/detach/device loss，surface/跨线程 |
| E2E-08 | media open/event/frame/close/shutdown/process-exit，AV/WMP/EVR/DXVA |
| E2E-09 | 多 Dispatcher、显式关闭、退出、资源释放/线程退出/callback 静止；卸载不适用 |

各级保留 original/candidate 结果，低级不代高级，不等待最后批次才开始。
逐项输入输出及矩阵见[E2E 可执行合同](e2e-executable-contracts.md)。

## 4. 批次门禁

| 批次 | 最低新增证据 |
|---|---|
| 0 | T0+T2、E2E-00、隔离/架构/P0 spike |
| 1 | 全基础 enum/error/handle/struct T1 与 native sizeof/offsetof |
| 2 | attach/detach/process-exit/heap/DebugLib 事件与失败注入；模块卸载不适用 |
| 3 | COM/refcount/memory/lock/dynamic loader T1/T3 |
| 4 | geometry/scanop/effects 确定及随机 T3 |
| 5 | FXJIT IR/code bytes/执行/W^X/CFG/cache/unwind |
| 6 | core math/loader/target stack/glyph T1/T3/T4 |
| 7 | 全 command/resource ID、布局、golden bytes、router/factory 负例 |
| 8 | E2E-02/03，resource/UCE 寿命/线程 T4 |
| 9 | E2E-04/05，像素字节/容差 |
| 10 | E2E-06/07，device loss/fallback/shader/resource |
| 11 | E2E-08，thread/apartment/callback/三类结束动作 |
| 12 | factory/WIC/stream/meta backend selection T3/T4 |
| 13 | 完整 ABI 集合及差异、PE/resource/version/symbol、全入口 smoke |
| 14 | E2E-09、configuration/RID 矩阵、状态收口 |

## 5. 逐文件测试契约

编码前写原主文件/配对头、可观察输入输出副作用、正常/边界/错误/释放、原基线取得方式、架构/配置、exact 或有依据 tolerance，以及 UnitVerified/DifferentialVerified/Integrated 各自证据。
关闭列每条实际结果，未运行不能预计通过。
