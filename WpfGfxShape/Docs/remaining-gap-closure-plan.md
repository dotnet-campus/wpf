# wpfgfx 剩余缺口关闭计划

> 状态：当前中长期推进计划。
> 当前每轮唯一动作仍以 [`next-session-handoff.md`](next-session-handoff.md) 为准。
> 当前文件实现事实以 [`native-to-managed-file-map.md`](native-to-managed-file-map.md) 为准。
> 本文不记录逐轮完成历史和测试流水。

## 1. 目标与职责

本文负责把当前尚未覆盖的能力组织为有依赖关系的关闭阶段，直至候选库具备替换原 `wpfgfx_cor3.dll` 的条件。

本文回答：

- 哪些缺口阻塞完整替换；
- 各缺口应在什么前置满足后进入；
- 每个阶段以什么事实判定完成；
- 哪些兼容分支只能由原生可达性证据触发。

本文不替代：

- `next-session-handoff.md` 的唯一当前工作切片；
- `native-to-managed-file-map.md` 的当前文件映射事实；
- `06-testing-strategy.md` 和 `09-test-evidence-and-e2e-contract.md` 的测试契约；
- `00-migration-charter.md` 的最高约束。

## 2. 当前基线

- D3D9 loader、display/device manager、device/resource/use-context、状态缓存、surface/texture/swap-chain 已有大量可运行切片。
- HW display/window/surface render target、软件 3D fallback、geometry renderer、部分 shader/fixed-function pipeline、bitmap color source/cache、software rasterizer/render target 和 glyph/text 窄路径已有实现。
- 当前全部原生到托管映射仍为 `Partial` 或 `Not started`，没有完整原生文件族可标记为替换完成。
- 生产 ABI、generated protocol、UCE/资源协议、完整内容管线和 PresentationCore E2E 尚未闭环。
- 当前不能替换原 `wpfgfx_cor3.dll`。

## 3. 缺口分类

### 3.1 完整替换硬阻塞项

- generated protocol；
- UCE connection/channel/resource/command/composition；
- 生产 ABI 与完整导出；
- meta/API/factory；
- scene、render-target 和 dummy back-buffer 的完整生命周期；
- PresentationCore E2E。

### 3.2 渲染覆盖缺口

- 完整 layer stack；
- 几何 mask/effect；
- effects；
- 完整 glyph 私有模型；
- 真实 shader bytecode 加载链；
- GDI presenter、software-DC present context、兼容 DC/DIB。

### 3.3 证据触发型兼容分支

以下内容不得仅因当前缺失而直接实现：

- `Reset/ResetEx`；
- registry WOW64 view；
- surface ScrollBlt；
- software dirty 通知；
- 普通 render-target dummy 解绑。

进入这些分支前必须确认原生拥有者、实际调用路径、PresentationCore 可达性、状态语义、失败顺序和所有权。没有证据时保持未实现，不创建推测性 API。

## 4. 阶段总览

| 阶段 | 目标 | 当前状态 | 是否阻塞完整替换 |
|---:|---|---|---|
| 1 | 完成 HW backend 生命周期和直接设备边界 | Active | 是 |
| 2 | 完成 HW 内容管线 | Pending | 是 |
| 3 | 完成软件呈现链 | Pending | 是 |
| 4 | 冻结并实现 generated protocol | Pending | 是 |
| 5 | 实现 resources/UCE 最小到完整链 | Pending | 是 |
| 6 | 收口 meta/API、生产 ABI 和 DLL 导出 | Pending | 是 |
| 7 | 渐进 PresentationCore E2E 与替换验收 | Pending | 是 |

`Active` 只表示当前优先推进方向，不表示该阶段的全部前置已经完成。每轮仍只领取 `next-session-handoff.md` 指定的一个可独立验证切片。

## 5. 阶段 1：HW backend 生命周期和直接设备边界

### 范围

- 完成 `Direct3D9Device` 尚缺的原生直接委托、状态和错误边界；
- 完成 scene 配对、render-target 使用与释放生命周期；
- 按原生证据建立 dummy back-buffer 的真实所有权和解绑行为；
- 补齐 device-lost、失败恢复、释放顺序和释放后保护；
- 保持 D3D9/D3D9Ex COM vtable、Windows `Stdcall` 和 HRESULT 顺序。

### 进入规则

- 每个切片必须先定位原生方法、调用者和所有权；
- 不以相邻功能为理由引入 `Reset/ResetEx`、GDI presenter 或 UCE；
- dummy back-buffer 相关切片只有在完整创建、借用、替换和释放链明确后才能进入。

### 完成条件

- 直接设备方法和当前 HW render-target 承载面没有已知的生命周期断口；
- scene/use-context/render-target/dummy 资源能够按原生顺序配对；
- driver error、device-lost、部分初始化失败和释放后调用均有回归；
- 定向测试、全量主测试、ABI 测试和解决方案构建通过。

## 6. 阶段 2：HW 内容管线

### 顺序

1. 完整 layer stack 与嵌套恢复；
2. 几何 mask；
3. effect list 与 effect 消费链；
4. glyph run/bank/painter 私有模型；
5. shader 资源定位、bytecode 加载、创建和设备释放；
6. bitmap/path/glyph/video 的完整生产绘制链；
7. pipeline builder、waffling 和 expanded vertex 消费链。

### 前置

- 阶段 1 涉及的 render-target、scene 和资源所有权稳定；
- effects 先具备 COM AddRef/Release 和消费者证据；
- glyph 先具备数据布局、缓存拥有者和软件/HW 回退边界；
- shader bytecode 必须来自原资源或可验证生成链，不使用占位数据。

### 完成条件

- HW backend 可通过生产对象完成确定性 bitmap/path/glyph/video 绘制；
- layer、mask、effect 和 fallback 不依赖测试替身绕过核心逻辑；
- shader 和 glyph 资源具备成功、失败、设备释放和重建证据；
- 组件级输出可与原生实现进行差分。

## 7. 阶段 3：软件呈现链

### 顺序

1. software surface/HWND target 的剩余状态；
2. software-DC present context；
3. 兼容 DC/DIB 生命周期；
4. GDI presenter；
5. dirty region 与 present；
6. 仅在原生可达性成立时补 ScrollBlt 和 software dirty 通知。

### 前置

- software rasterizer/render-target 的当前像素和锁定语义稳定；
- HWND、HDC、HBITMAP、DIB bits 的所有权和线程边界明确；
- 不使用 System.Drawing、Skia 或其它高层光栅器替代原实现。

### 完成条件

- 直接组件 harness 能完成最小 software bitmap/render-target 绘制和呈现；
- dirty rect、stride、DPI、pixel format、GDI present 和资源释放有确定性证据；
- 满足 `E2E-04` 的软件像素要求。

## 8. 阶段 4：generated protocol

### 顺序

1. 冻结 PresentationCore 当前实际消费的类型和命令副本；
2. command/resource ID；
3. 显式结构布局、payload 起点和固定宽度字段；
4. processed core/AV types；
5. generated dispatch 签名；
6. resource factory、marshal 和 render-data；
7. producer/consumer 字节级校验。

### 规则

- 按生成族推进，不把单个生成输出当作孤立 DTO 手写；
- 不运行生成器后直接覆盖冻结基线；
- 不改变命令编号、资源编号、布局、SDK fingerprint 或 malformed packet 语义。

### 完成条件

- 当前消费副本、原生布局和托管镜像可逐项追溯；
- 代表命令和资源更新具备 golden bytes、size/offset 和失败码证据；
- generated dispatch、factory 和消费者签名能够支持阶段 5。

## 9. 阶段 5：resources/UCE

沿用既有 `8A`～`8F` 依赖拆分，但状态以本文和当前交接为准。

### 顺序

1. `8A`：类型、签名和布局循环骨架；
2. `8B`：handle table、connection 和 channel；
3. `8C`：command batch 与资源生命周期；
4. `8D`：resource update 与 generated dispatch；
5. `8E`：composition、partition、线程和 scheduler；
6. `8F`：UCE ABI 家族与最小集成链。

### 完成条件

以下链路使用生产实现闭环：

`connection → channel → resource → command batch → generated dispatch → render target`

并覆盖：

- create/duplicate/AddRef/release；
- begin/append/end/commit；
- malformed packet；
- same-thread/cross-thread；
- disconnect、partition zombie 和部分初始化失败；
- 线程停止与逆序释放。

完成后应满足 `E2E-02` 和 `E2E-03` 的生产能力要求。

## 10. 阶段 6：meta/API、生产 ABI 与 DLL 导出

### 顺序

1. meta backend selection 和 target wrappers；
2. factory、bitmap、stream、WIC/codec 及外部对象桥接；
3. `MilVersionCheck` 与最小 COM/factory；
4. connection/channel/resource 导出；
5. render-target、bitmap、geometry、HW 和媒体导出；
6. DLL 初始化、资源、异常封锁和进程退出；
7. `.def`、PresentationCore P/Invoke 和实际二进制导出差分。

### 规则

- NativeAOT 导出只作为薄 ABI 边界；
- 不用现有 ABI probe 代替生产 ABI；
- 不修改 PresentationCore P/Invoke 声明制造兼容；
- 不删除当前无托管调用方但仍属于原二进制契约的导出。

### 完成条件

- 生产导出名、调用约定、参数、布局、HRESULT、异常封锁和所有权有自动化清单；
- x64、ARM64，以及平台正式支持范围内的 x86 分别验证；
- DLL 名称、资源、attach/exit 和重复调用行为满足差分要求；
- `E2E-01` 可以由真实生产入口通过。

## 11. 阶段 7：PresentationCore E2E 与替换验收

按既有契约渐进执行：

1. `E2E-01`：版本与最小 COM；
2. `E2E-02`：connection/channel；
3. `E2E-03`：resource command；
4. `E2E-04`：软件像素；
5. `E2E-05`：`DrawingVisual → RenderTargetBitmap → CopyPixels`；
6. `E2E-06`：HWND 与硬件路径；
7. `E2E-07`：D3DImage；
8. `E2E-08`：媒体；
9. `E2E-09`：多 Dispatcher、重复创建/关闭、进程退出和生命周期收口。

### 完成条件

- original 与 candidate 在隔离进程中使用相同 PresentationCore 调用路径；
- 不修改 PresentationCore 源码或 P/Invoke 声明；
- 像素、HRESULT、异常、事件、资源和退出行为达到批准的差分条件；
- 所有目标架构、配置和能力环境均有明确结果；
- 只有此阶段完成后，候选库才可称具备替换条件。

## 12. 当前执行位置

当前位于阶段 1。

当前唯一切片由 [`next-session-handoff.md`](next-session-handoff.md) 指定。完成每个切片后：

1. 更新交接中的真实验证结果和唯一下一目标；
2. 更新 [`native-to-managed-file-map.md`](native-to-managed-file-map.md) 的当前事实；
3. 将长期完成事实写入 [`handoffs/progress-completed-work.md`](handoffs/progress-completed-work.md)；
4. 仅在阶段状态、依赖或完成条件变化时更新本文。
