# wpfgfx C# NativeAOT 迁移测试与证据策略

> 状态：实施前权威测试基线  
> 上位约束：测试随迁移同步增长；没有预定义证据的翻译不得称为完成  
> 适用范围：项目机制、ABI、逐文件逻辑、协议、生命周期、渲染后端及真实 PresentationCore

## 1. 测试目标

测试体系必须同时回答五类问题：

1. C# 内部逻辑是否符合原文件语义；
2. Native AOT 产物是否提供正确的 Windows C ABI；
3. 原生资源、COM、线程、回调和失败路径是否等价；
4. 新实现与原 wpfgfx 对同一输入是否产生同结果/副作用；
5. 未修改的真实 PresentationCore 是否能逐步使用候选 DLL 完成可观察流程。

单一测试框架不能覆盖全部问题。托管单元测试、native caller、PE/resource 检查、原生差分 harness 和真实 WPF E2E 必须并列存在。

## 2. 证据分层

### T0：静态清单与来源证据

- 项目/文件/生成/ABI/资源 Ledger；
- 原路径到 C# 路径映射；
- 原 DLL、candidate DLL、SDK、RID、configuration、source revision 和 hash；
- 106 `.def`、107 唯一 EntryPoint、99 交集和 8/7 差异；
- 未运行项必须明确标记，静态搜索不能冒充二进制事实。

### T1：托管单元与布局测试

适用：纯算法、错误码、enum、状态转换、command parser、内部异常映射 helper。

必须覆盖：

- 边界、退化、溢出、NaN/Inf、checked/unchecked；
- `Unsafe.SizeOf`/`Marshal.SizeOf`/field offset；
- 固定宽度 `BOOL`、C++ `bool`、handle、pointer 和 `UInt64` 槽；
- 每个原文件的分支/失败前置，而不只覆盖 happy path。

T1 不能证明 PE export、native 栈、COM vtable、loader 或 unload。

### T2：Native AOT 产物与 native caller

至少两类 caller：

- 动态加载：绝对路径 `LoadLibrary`、`GetModuleFileNameW`、精确 `GetProcAddress`；
- 静态 import：若 publish 生成受支持 `.lib`，用 C header 正常链接。

验证：

- DLL/PE machine、精确导出名、大小写、装饰名、ordinal/kind；
- HRESULT/BOOL/void/out 参数；
- managed exception 封锁和调用后恢复；
- x86 stdcall/ESP，x64/ARM64 栈/寄存器/unwind；
- 并发首次调用与重复调用；
- 错误架构、缺失 DLL/symbol、加载路径；
- 所有 crash/fail-fast/卸载测试在隔离子进程运行。

### T3：原实现与 C# 差分

同一测试向量分别进入：

- 冻结的原 wpfgfx DLL/原生测试适配器；
- candidate AOT DLL/C# 实现。

比较：

- 返回值、out 参数和错误码；
- 完整结构/字节流/像素；
- AddRef/Release、创建/销毁、锁和事件序列；
- callback 次数、线程、顺序和 detach 后行为；
- 失败注入后的部分状态和后续可用性。

若原文件没有可独立调用入口，应构建不修改原产品源码语义的测试适配层或使用现有公开入口间接观测；无法建立原生基线时文件保持 `Blocked`，不得只与重新实现的数学公式自证。

### T4：组件集成

逐步覆盖：

- 初始化/错误/loader；
- connection/channel/batch/message；
- resource handle/factory/update；
- software render target/pixel output；
- hardware device/resource/fallback；
- AV state thread/event/surface；
- shutdown/线程 join/资源释放。

每个组件闭环都要有确定性输入、可观察输出和失败路径。

### T5：真实 PresentationCore E2E

原则：不修改 PresentationCore 的 MilCore P/Invoke 声明来“帮助”候选实现；通过受控部署/loader 隔离切换原 DLL与 candidate DLL。

首个强制真实路径：

1. 创建 `DrawingVisual`；
2. 绘制确定性、无字体/媒体依赖的图形；
3. `RenderTargetBitmap.Render`；
4. 读取规范化像素；
5. 与原 wpfgfx 基线比较。

现有 `PresentationCore.Tests` 未发现可直接复用的该贯通测试，因此需要新建专用 E2E harness；它不并入新生产项目。

## 3. 渐进 E2E 阶梯

E2E 不等待批次 14 才首次出现：

| 级别 | 首次能力 | 主要入口/结果 | 解锁含义 |
|---|---|---|---|
| `E2E-00` | Native AOT 机制 | 非生产 ABI probe | 只证明 shared-library/export/caller 机制 |
| `E2E-01` | 版本与最小对象 | `MilVersionCheck`；受控 IUnknown/factory spike | 版本/error/COM vtable 基础 |
| `E2E-02` | connection/channel | create/close/commit/destroy | 指针宽度 handle、线程/批次生命周期 |
| `E2E-03` | resource command | create/addref/send/release、消息失败 | 32 位资源 ID、protocol/router/factory |
| `E2E-04` | 软件像素 | 最小软件 bitmap/render-target 输出确定性像素 | SW/scanop/资源链局部贯通 |
| `E2E-05` | 真实 PresentationCore bitmap | `DrawingVisual -> RenderTargetBitmap -> pixels` | 第一条真实未修改 WPF 主链 |
| `E2E-06` | 硬件/HWND | 硬件 bitmap 和 HWND present、SW fallback | D3D9 设备/交换链/呈现 |
| `E2E-07` | D3DImage | front-buffer callback、detach、device loss | 用户 surface 与跨线程 callback |
| `E2E-08` | 媒体 | open/event/frame/close/shutdown/process-exit | AV/WMP/EVR/DXVA 状态机 |
| `E2E-09` | 生命周期收口 | 多 Dispatcher、显式关闭、process exit、受支持 unload/reload | 完整候选替换的生命周期证据 |

每一级必须保留原 DLL与 candidate 的结果。较低级通过不能替代后续级别。

## 4. 批次门禁

| 批次 | 最低新增证据 |
|---:|---|
| 0 | T0 + T2；`E2E-00`；隔离、架构和 P0 spike |
| 1 | 全基础 enum/error/handle/struct 的 T1 + native `sizeof/offsetof` |
| 2 | attach/detach/process-exit/heap/DebugLib 事件序列与失败注入 |
| 3 | COM/refcount/memory/lock/dynamic loader T1/T3 |
| 4 | 几何/scanop/effects 大量确定性和随机 T3 |
| 5 | FXJIT IR/code bytes/执行结果/W^X/CFG/cache/unwind |
| 6 | core math/loader/target stack/glyph 数据 T1/T3/T4 |
| 7 | 全 command/resource ID、布局、golden bytes、router/factory 负例 |
| 8 | `E2E-02`、`E2E-03`；resource/UCE 生命周期和线程 T4 |
| 9 | `E2E-04`、`E2E-05`；像素逐字节/容差规则 |
| 10 | `E2E-06`、`E2E-07`；设备丢失、fallback、shader/resource |
| 11 | `E2E-08`；线程/apartment/callback/三类结束动作 |
| 12 | factory/WIC/stream/meta backend selection T3/T4 |
| 13 | 完整 106/107 ABI、PE/resource/version/symbol 和全入口 smoke |
| 14 | `E2E-09` 与规定 configuration/RID 平台矩阵，状态收口 |

## 5. 逐文件测试契约

每个实现工作包编码前必须写明：

- 原生主文件和配对头；
- 原生可观察输入/输出/副作用；
- 正常、边界、错误和资源释放测试；
- 原实现基线取得方式；
- 需要的架构/configuration；
- 精确比较还是有依据的容差比较；
- 文件达到 `UnitVerified`、`DifferentialVerified`、`Integrated` 所需的分别证据。

工作包关闭时必须列出每条测试的实际执行结果。未运行不得写为“预计通过”。

## 6. 数值、几何与像素比较

- 整数、协议、命令、资源 ID、HRESULT、引用计数和大多数像素操作默认逐位/逐字节一致。
- 浮点和几何若原路径本身允许容差，先冻结原实现输出分布和边界，再定义绝对/相对/ULP 规则；不得为了让 C# 通过随意放宽。
- 颜色空间、premultiply、rounding、NaN、negative zero 和 overflow 必须作为显式向量。
- 图像差分保存输入、原图、候选图、差异图和统计；单个“看起来一样”的截图不是证据。

## 7. 生成协议测试

迁移前冻结：

- `MILCMD 0x01..0x8D`、Debug `0x8E`；
- resource ID `1..97`、`TYPE_LAST=98`；
- SDK version/fingerprint；
- 每个结构 size/offset/pack；
- 固定、resource-handle、UInt64、payload 和 render-data 命令 golden bytes。

负例必须覆盖：未知 ID、错误固定大小、截断 payload、错误 resource type、transport denied、数组长度不整除、render-data push/pop 失衡，以及原有特殊容错。

生成器只可输出到隔离目录。与冻结输出的差异未解释前不得覆盖基线。

## 8. ABI、COM 与 callback 测试

### ABI

- 逐入口比较名称、签名、调用约定、布局和错误；
- 8/7 差异保持显式状态；
- x86 每个代表签名覆盖栈平衡；
- 不通过修改 P/Invoke 或删除导出制造通过。

### COM

C++ caller 验证：IUnknown 三槽、完整 vtable 顺序、IID/QI、AddRef/Release、create/getter 所有权、null/error、apartment 和并发。GC 只能作为内部存储，不能替代 native refcount 证据。

### callback

覆盖：

- Stream descriptor 14 个 stdcall callback；
- EventProxy 长期 callback 与 AV thread；
- geometry 同步 `void CALLBACK`；
- D3DImage render/composition thread callback；
- CoreCLR token 在 AOT 内只作不透明值往返；
- detach 先阻止新 callback，再等待在途调用，再释放 owner/context；
- callback 异常不跨边界。

## 9. 生命周期和故障注入

必须独立测试：

- 首次任意入口、多线程同时首次进入、递归首次进入；
- 每个初始化阶段失败；
- 原实现没有回滚或始终返回成功的异常行为；
- 显式关闭、SafeHandle/finalizer、Dispatcher shutdown、ProcessExit；
- 进程终止时跳过完整 detach；
- 后台线程停止、唤醒、join 和 handle close；
- 官方支持范围内的 load/call/free/reload；不支持时记录兼容阻塞。

不要在同一测试进程中执行可能导致栈破坏、loader crash 或 fail-fast 的矩阵项。

## 10. 平台矩阵

- Debug/Release × x64/ARM64；
- x86 在支持裁决后加入，同步增加 stdcall/ESP；
- hardware、D3DImage 和 media 必须在具备真实设备/组件的受控机器运行；
- 软件路径应有稳定 CPU-only 基线；
- 远程桌面、强制软件、设备丢失和系统组件缺失作为独立环境维度；
- 交叉编译/PE 检查不能代替目标架构真实运行。

## 11. 证据存储

建议位于：

`WpfGfxShape/artifacts/test-results/<work-package>/<configuration>/<rid>/`

每次执行保存：

- manifest：工作包、Git revision、SDK/toolchain、OS、CPU/GPU、configuration/RID；
- 原/candidate 文件 hash 和加载绝对路径；
- 命令或 IDE 操作；
- stdout/stderr/exit code；
- 测试结果；
- PE/export/resource/layout 清单；
- golden/diff 工件；
- crash dump/PDB 引用（如适用）。

大型二进制是否提交仓库另行决定，但 manifest 和来源不能只存在于对话。

## 12. 完成定义

- `Translated`：代码已近似直译，但验证不足；
- `UnitVerified`：T1 满足；
- `DifferentialVerified`：T3 满足；
- `Integrated`：对应 T4 闭环满足；
- `EndToEndVerified`：该文件参与的最高已就绪 E2E 级通过；
- `Accepted`：该文件规定的全部 ABI/布局/单元/差分/集成/E2E 证据满足，且无未记录偏差。

任何工作包不得只以“build succeeded”关闭。
