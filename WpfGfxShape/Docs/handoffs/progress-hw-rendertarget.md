# 生产翻译进展归档：HW RenderTarget（hwsurfrt.cpp / hwdisplayrt.cpp / hwhwndrt.cpp）

> 归档自 `next-session-handoff.md` 第 3 节 HW render-target 主题段；不作为下一轮任务入口。
> 维护规则：历史事实不可覆盖；新进展先写入 `next-session-handoff.md` 当前唯一入口。

## HW render-target 生产翻译进展

本轮依据原生 `CHwDisplayRenderTarget::AdvanceFrame` 证据，闭环 display render-target 的 display-invalid/display-target 门控、device-entry、frame number 原样委托与无返回值窄边界：

- 现有托管入口先执行释放后保护；仅 associated-display 目标且 display-invalid 未失败时进入一次 device entry，并把输入 `uint` frame number 原样委托 `Direct3D9Device.AdvanceFrame`。托管目标构造要求非空 device，因此原生空 device 分支由构造不变量消除；无需修改生产代码。
- rendering disabled 不构成额外门控；有效 display target 仍推进资源管理阶段，退出后 device entry 成对释放。display-invalid、direct surface 和 texture-backed 非 display 目标均不进入 device entry，也不推进设备帧。
- 强化回归覆盖 `uint.MaxValue` 原样委托、相同帧只推进一次、禁用渲染仍推进、display-invalid 后零作用域、新帧资源阶段、阶段失败短路与作用域退出、两类非 display 目标跳过及释放后保护。
- 未扩展普通 frame/resource 回收内部协议、完整 Present、frame pacing、DWM、effects、UCE/资源协议、ScrollBlt、software dirty、GDI/software-DC、dummy back buffer、`ReleaseUseOfRenderTarget`、普通 scene 生命周期或 `Reset/ResetEx`。
- 验证：AdvanceFrame 聚焦测试 8/8、surface render-target 聚焦测试 533/533、全量主测试 2849/2849、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CD3DDeviceLevel1::GetRasterStatus` 的能力前置、swap-chain index 原样委托、Windows `Stdcall` COM slot 19、输出与 HRESULT 边界。

上一轮依据原生 `CHwDisplayRenderTarget::WaitForVBlank` 证据，闭环 display render-target 的 display-invalid、device-entry、固定 swap-chain index 0 委托与 HRESULT 窄边界：

- 现有托管入口先执行释放后保护；display-invalid 已失败时固定返回 `WGXERR_NO_HARDWARE_DEVICE`，不调用设备等待且不进入 device entry。托管目标构造要求非空 device，因此原生空 device 分支由构造不变量消除；无需修改生产代码。
- display 有效时现有实现进入一次 device entry，并固定以 swap-chain index 0 恰好一次委托 `Direct3D9Device.WaitForVBlank`；设备返回的成功 HRESULT 原样传播，退出后作用域成对释放。
- rendering disabled 不构成额外门控；新增禁用渲染时仍在 device entry 内以 index 0 委托、成功 HRESULT 原样传播和退出作用域回归，并强化 display-invalid 后未残留 device entry 的断言。
- 未扩展完整 Present、frame pacing、DWM、effects、UCE/资源协议、ScrollBlt、software dirty、GDI/software-DC、dummy back buffer、`ReleaseUseOfRenderTarget`、普通 scene 生命周期或 `Reset/ResetEx`。
- 验证：surface render-target 聚焦测试 513/513、全量主测试 2848/2848、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::AdvanceFrame` 的 display-invalid/device 门控、device-entry、frame number 原样委托与无返回值边界。

上一轮依据原生 `CHwDisplayRenderTarget::DrawVideo` 证据，闭环 display render-target 的 rendering-enabled 门控、一次基础委托、HRESULT 与内容有效性窄边界：

- rendering disabled 时现有托管入口在释放后保护之后静默返回 `S_OK`，不访问绘制操作或 video pipeline 依赖，不进入 device entry/use-context，也不标记内容有效；无需修改生产代码。
- rendering enabled 时现有实现恰好一次在既有 device/use-context 作用域内执行基础 video 绘制，原样传播 HRESULT；仅成功后提交内容有效，失败保持原状态。
- 新增 enabled 成功/失败的一次 scoped 回调与 HRESULT/内容有效性回归，以及 disabled 空操作参数门控和零设备作用域回归。
- 未扩展完整 video pipeline、effects、UCE/资源协议、普通 Present、ScrollBlt、software dirty、GDI/software-DC、dummy back buffer、`ReleaseUseOfRenderTarget`、普通 scene 生命周期或 `Reset/ResetEx`。
- 验证：surface render-target 聚焦测试 512/512、全量主测试 2847/2847、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::WaitForVBlank` 的 display-invalid/device 门控、device-entry、swap-chain index 0 委托与 HRESULT 边界。

上一轮依据原生 `CHwDisplayRenderTarget::DrawGlyphs` 证据，闭环 display render-target 的 rendering-enabled 门控、一次基础委托、HRESULT 与内容有效性窄边界：

- rendering disabled 时现有托管入口在释放后保护之后静默返回 `S_OK`，不访问绘制操作或 glyph/text pipeline 依赖，不进入 device entry/use-context，也不标记内容有效；无需修改生产代码。
- rendering enabled 时现有实现恰好一次在既有 device/use-context 作用域内执行基础 glyph 绘制，原样传播 HRESULT；仅成功后提交内容有效，失败保持原状态。
- 新增 enabled 成功/失败的一次 scoped 回调与 HRESULT/内容有效性回归，以及 disabled 空操作参数门控和零设备作用域回归。
- 未扩展完整 glyph/text pipeline、effects、UCE/资源协议、普通 Present、ScrollBlt、software dirty、GDI/software-DC、dummy back buffer、`ReleaseUseOfRenderTarget`、普通 scene 生命周期或 `Reset/ResetEx`。
- 验证：surface render-target 聚焦测试 509/509、全量主测试 2844/2844、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::DrawVideo` 的 rendering-enabled 门控、一次基础委托、HRESULT 与内容有效性边界。

上一轮依据原生 `CHwDisplayRenderTarget::DrawInfinitePath` 证据，闭环 display render-target 的 rendering-enabled 门控、一次基础委托、HRESULT 与内容有效性窄边界：

- rendering disabled 时现有托管入口在释放后保护之后静默返回 `S_OK`，不访问绘制操作或 infinite-path/geometry pipeline 依赖，不进入 device entry/use-context，也不标记内容有效；无需修改生产代码。
- rendering enabled 时现有实现恰好一次在既有 device/use-context 作用域内执行基础 infinite-path 绘制，原样传播 HRESULT；仅成功后提交内容有效，失败保持原状态。
- 新增 enabled 成功/失败的一次 scoped 回调与 HRESULT/内容有效性回归，以及 disabled 空操作参数门控和零设备作用域回归。
- 未扩展完整 infinite-path/geometry pipeline、effects、UCE/资源协议、普通 Present、ScrollBlt、software dirty、GDI/software-DC、dummy back buffer、`ReleaseUseOfRenderTarget`、普通 scene 生命周期或 `Reset/ResetEx`。
- 验证：surface render-target 聚焦测试 506/506、全量主测试 2841/2841、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::DrawGlyphs` 的 rendering-enabled 门控、一次基础委托、HRESULT 与内容有效性边界。

上一轮依据原生 `CHwDisplayRenderTarget::DrawPath` 证据，闭环 display render-target 的 rendering-enabled 门控、一次基础委托、HRESULT 与内容有效性窄边界：

- rendering disabled 时现有托管入口在释放后保护之后静默返回 `S_OK`，不访问绘制操作或 path/geometry pipeline 依赖，不进入 device entry/use-context，也不标记内容有效；无需修改生产代码。
- rendering enabled 时现有实现恰好一次在既有 device/use-context 作用域内执行基础 path 绘制，原样传播 HRESULT；仅成功后提交内容有效，失败保持原状态。
- 新增 enabled 成功/失败的一次 scoped 回调与 HRESULT/内容有效性回归，以及 disabled 空操作参数门控和零设备作用域回归。
- 未扩展完整 path/geometry pipeline、effects、UCE/资源协议、普通 Present、ScrollBlt、software dirty、GDI/software-DC、dummy back buffer、`ReleaseUseOfRenderTarget`、普通 scene 生命周期或 `Reset/ResetEx`。
- 验证：surface render-target 聚焦测试 503/503、全量主测试 2838/2838、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::DrawInfinitePath` 的 rendering-enabled 门控、一次基础委托、HRESULT 与内容有效性边界。

上一轮依据原生 `CHwDisplayRenderTarget::DrawMesh3D` 证据，闭环 display render-target 的 rendering-enabled 门控、一次基础委托、HRESULT 与内容有效性窄边界：

- rendering disabled 时现有托管入口在释放后保护之后静默返回 `S_OK`，不访问绘制操作或 mesh/shader pipeline 依赖，不进入 device entry/use-context，也不标记内容有效；无需修改生产代码。
- rendering enabled 时现有实现恰好一次在既有 device/use-context 作用域内执行基础 mesh 绘制，原样传播 HRESULT；仅成功后提交内容有效，失败保持原状态。
- 新增 enabled 成功/失败的一次 scoped 回调与 HRESULT/内容有效性回归，以及 disabled 空操作参数门控和零设备作用域回归。
- 未扩展完整 mesh/shader pipeline、shader bytecode、effects、UCE/资源协议、普通 Present、ScrollBlt、software dirty、GDI/software-DC、dummy back buffer、`ReleaseUseOfRenderTarget`、普通 scene 生命周期或 `Reset/ResetEx`。
- 验证：surface render-target 聚焦测试 500/500、全量主测试 2835/2835、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::DrawPath` 的 rendering-enabled 门控、一次基础委托、HRESULT 与内容有效性边界。

上一轮依据原生 `CHwDisplayRenderTarget::DrawBitmap` 证据，闭环 display render-target 的 rendering-enabled 门控、一次基础委托、HRESULT 与内容有效性窄边界：

- rendering disabled 时现有托管入口在释放后保护之后静默返回 `S_OK`，不访问绘制操作或 bitmap pipeline 依赖，不进入 device entry/use-context，也不标记内容有效；无需修改生产代码。
- rendering enabled 时现有实现恰好一次在既有 device/use-context 作用域内执行基础 bitmap 绘制，原样传播 HRESULT；仅成功后提交内容有效，失败保持原状态。
- 新增 enabled 成功/失败的一次 scoped 回调与 HRESULT/内容有效性回归，以及 disabled 空操作参数门控和零设备作用域回归。
- 未扩展完整 bitmap pipeline、effects、UCE/资源协议、普通 Present、ScrollBlt、software dirty、GDI/software-DC、dummy back buffer、`ReleaseUseOfRenderTarget`、普通 scene 生命周期或 `Reset/ResetEx`。
- 验证：surface render-target 聚焦测试 497/497、全量主测试 2832/2832、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::DrawMesh3D` 的 rendering-enabled 门控、一次基础委托、HRESULT 与内容有效性边界。

上一轮依据原生 `CHwDisplayRenderTarget::ClearInvalidatedRects` 与 base surface render-target dirty-region 清理证据，闭环 display render-target 的基础委托与纯 dirty-state 清理窄边界：

- 原生 display wrapper 恰好一次直接委托基础 `ClearInvalidatedRects`；现有托管实现已在释放后保护后清空普通 dirty rectangles 与 empty-invalidation 全量呈现标记并返回 `S_OK`，无需修改生产代码。
- 新增组合 dirty/empty 清理、重复清理幂等及 rendering-disabled 无副作用回归；清理不进入 device entry/use-context、不调用 swap-chain Present，也不改变 rendering-enabled、display-invalid HRESULT、marker、内容有效性或资源引用，后续 Present 不消费旧区域。
- 未扩展普通 Present、`ShouldPresent` 区域算法、ScrollBlt、software dirty、GDI/software-DC、dummy back buffer、`ReleaseUseOfRenderTarget`、普通 scene 生命周期或 `Reset/ResetEx`。
- 验证：surface render-target 聚焦测试 494/494、全量主测试 2829/2829、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::DrawBitmap` 的 rendering-enabled 门控、一次基础委托、HRESULT 与内容有效性边界。

上一轮依据原生 `CHwHWNDRenderTarget::Present` 证据，闭环 HWND render-target 的基础委托与 HRESULT 窄边界：

- 原生包装层仅以 `IFC` 恰好一次调用 `CHwDisplayRenderTarget::Present(pRect)`，随后 `RRETURN(hr)`；现有托管矩形 Present 入口已承载基础行为，无需修改生产代码。
- 新增成功与失败参数化回归，确认底层 Present 恰好调用一次并原样返回 HRESULT；未增加第二套门控、状态提交、清理或错误映射。
- 未扩展普通 Present 内部协议、`UpdateLayeredWindowEx`/GDI presenter、ScrollBlt、software dirty、dummy back buffer、`ReleaseUseOfRenderTarget`、普通 scene 生命周期或 `Reset/ResetEx`。
- 验证：surface render-target 聚焦测试 492/492、全量主测试 2827/2827、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::ClearInvalidatedRects` 的基础委托与纯 dirty-state 清理边界。

上一轮依据原生 `CHwHWNDRenderTarget::UpdatePresentProperties` 与 `CMILDeviceContext::SetLayerProperties` 证据，闭环 HWND render-target 的属性保存与无副作用窄边界：

- HWND 路径固定传入空 display；现有实现保持 opaque、color-key、constant-alpha、per-pixel-alpha 与 destination-alpha 的原生门控和后调用覆盖语义，无需修改生产代码。
- 新增各 transparency flag 独立门控、后调用覆盖及 enabled/disabled 无副作用回归；入口不进入 device entry/use-context、不调用 Present，也不改变 rendering-enabled、display-invalid HRESULT、dirty region、marker、内容有效性或资源引用。
- 未扩展 `UpdateLayeredWindowEx`/GDI presenter、普通 Present、ScrollBlt、software dirty、dummy back buffer、`ReleaseUseOfRenderTarget`、普通 scene 生命周期或 `Reset/ResetEx`。
- 验证：surface render-target 聚焦测试 490/490、全量主测试 2825/2825、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwHWNDRenderTarget::Present` 的一次基础委托与 HRESULT 边界。

再上一轮依据原生 `CHwHWNDRenderTarget::SetPosition` 与 `CMILDeviceContext::SetPosition` 证据，闭环 HWND render-target 的位置保存与无副作用窄边界：

- 原生仅把输入 `POINT` 原样赋给保存的窗口原点；现有 `Direct3D9SurfaceRenderTarget.SetPosition` 已保持 X/Y 直接覆盖、后调用优先和无坐标换算，无需修改生产代码。
- 强化回归覆盖负坐标与后调用覆盖，并确认 enabled/disabled 均保存相同位置、不进入 device entry/use-context且不调用 Present；既有释放后保护继续拒绝重复 Dispose 后更新。
- 未扩展 `UpdateLayeredWindowEx`/GDI presenter、`UpdatePresentProperties`、普通 Present、ScrollBlt、software dirty、dummy back buffer、`ReleaseUseOfRenderTarget`、普通 scene 生命周期或 `Reset/ResetEx`。
- 验证：surface render-target 聚焦测试 485/485、全量主测试 2820/2820、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwHWNDRenderTarget::UpdatePresentProperties` 的 layer-property 保存与无副作用窄边界。

上一轮依据原生 `CHwDisplayRenderTarget::Begin3D/End3D` 证据，闭环 display render-target 的启用门控与基础委托窄边界：

- rendering disabled 时现有 `Direct3D9SurfaceRenderTarget.Begin3D/End3D` 在释放后保护之后固定静默返回 `S_OK`，不进入 device entry/use-context、不调用基础 3D 入口，也不改变 in-3D、bounds、multisample、depth-stencil 或资源引用；已处于 3D 后禁用渲染时，`End3D` 保留既有配对状态。
- rendering enabled 时现有实现恰好一次执行基础 surface 入口并原样保持其 HRESULT、Begin3D 状态提交与失败恢复、End3D 配对与恢复语义。
- 生产代码无需修改；现有回归已覆盖禁用 Begin3D 完全无状态变化及禁用 End3D 不解除既有配对，未扩展普通 scene 生命周期、dummy back buffer、`ReleaseUseOfRenderTarget` 或普通 surface/texture/software 3D 协议。
- 验证：surface render-target 聚焦测试 484/484、全量主测试 2819/2819、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwHWNDRenderTarget::SetPosition` 的位置保存与无副作用窄边界。

上一轮依据原生 `CHwDisplayRenderTarget::Clear` 证据，闭环 display render-target 的启用门控、HRESULT 与内容有效性窄边界：

- rendering disabled 时现有 `Direct3D9SurfaceRenderTarget.Clear` 在释放后保护之后静默返回 `S_OK`，不进入 device entry、不执行基础 clear 链且保持内容无效。
- rendering enabled 时现有实现恰好一次执行基础 surface clear 链并原样传播首个 HRESULT；只有成功退出才标记内容有效，任一设备步骤失败均保持原内容状态并退出 device entry。
- 生产代码无需修改；强化 disabled、成功和逐步骤失败回归，未扩展普通 clear 协议、dirty-region、software dirty、ScrollBlt、GDI/software-DC、dummy back buffer、普通 scene 生命周期、`ReleaseUseOfRenderTarget` 或 `Reset/ResetEx`。
- 验证：surface render-target 聚焦测试 484/484、全量主测试 2819/2819、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::Begin3D/End3D` 的 rendering-enabled 门控、基础委托与 HRESULT 窄边界。

上一轮依据原生 `CHwDisplayRenderTarget::HrFindInterface` 证据，闭环 display render-target 固定拒绝 QI 的窄边界：

- 原生对任意 IID 固定返回 `E_NOINTERFACE`；现有 `Direct3D9SurfaceRenderTarget.FindInterface` 已先执行释放后保护、清空输出并固定拒绝，无需修改生产代码。
- 新增参数化回归覆盖 `Guid.Empty`、`IUnknown` 和任意 IID，均保持输出为零；新增无副作用回归确认查询不进入 device entry/use-context、不调用 Present、不改变 rendering-enabled、display-invalid HRESULT、内容有效性、dirty region 或资源引用。
- 释放后调用继续抛出 `ObjectDisposedException`；未扩展 COM 接口表、生产 ABI 或普通 surface/texture/software render-target 接口协议。
- 验证：surface render-target 聚焦测试 484/484、全量主测试 2819/2819、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::Clear` 的 rendering-enabled 门控、基础委托、HRESULT 和内容有效性窄边界。

上一轮依据原生 `CHwDisplayRenderTarget::IsValid` 证据，闭环 rendering-enabled 与 swap-chain validity 的双门控窄边界：

- 原生仅在 rendering enabled 且 swap-chain valid 时返回 true；现有 `Direct3D9SurfaceRenderTarget.IsValid` 已保持相同短路组合，无需修改生产代码。
- 新增回归覆盖双条件均为 true、仅 rendering disabled、仅 swap-chain invalid，以及重复 Dispose 后安全返回 false。
- 查询不进入 device entry/use-context、不调用 Present，也不改变 display-invalid HRESULT、内容有效性、marker 或资源引用；未扩展普通 surface/texture/software render-target 的有效性协议。
- 验证：surface render-target 聚焦测试 481/481、全量主测试 2816/2816、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::HrFindInterface` 固定拒绝 display render-target QI 的窄边界。

上一轮依据原生 `CHwDisplayRenderTarget::PresentInternal` 证据，闭环 copy/flip 参数选择与 occluded 成功返回边界：

- `Direct3D9PresentRequest` 增加 destination HWND 承载，并由原生 swap-chain `Present` 调用原样转发。
- `D3DSWAPEFFECT_COPY` 原样传递相同 source/destination rectangle、dirty region、HWND 与 flags；非 copy 清空 source/destination/dirty region，只保留 HWND 与 flags。
- `S_PRESENT_OCCLUDED` 原样返回，不归一化为 `S_OK`，同时不记录成功 GPU marker、不触发设备失败处理或渲染禁用。
- 新增 Copy/Flip 参数转发回归并更新 occluded 预期；未扩展 dirty-region 协议、software dirty、ScrollBlt、普通 scene 生命周期或 dummy back buffer。
- 验证：surface render-target 聚焦测试 478/478、全量主测试 2813/2813、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::IsValid` 对 rendering-enabled 与 swap-chain validity 的双门控窄边界。

上一轮依据原生 `CHwDisplayRenderTarget::Present` 与 `InvalidateRect` 证据，闭环 display render-target 的禁用门控、失败持久化和 dirty-region 保留边界：

- 矩形 `Present` 在 device-entry 内先检查渲染启用状态；禁用时返回已保存的 display-invalid HRESULT，且不调用 swap-chain。所有退出路径均清理 invalidated rectangles。
- `WGXERR_DISPLAYSTATEINVALID` 会被保存供后续 `Present` 一致返回；其他 Present 失败只禁用渲染，不持久化原 HRESULT，后续调用返回初始成功值。现有生产实现符合该契约，无需修改。
- `InvalidateRect` 仅在渲染启用时委托基础 dirty-region 保留；禁用时静默成功且不保留矩形，释放后调用仍优先抛出 `ObjectDisposedException`。现有生产实现和既有回归已覆盖，无需修改。
- 新增三个矩形 Present 回归，覆盖零尺寸禁用门控、display-invalid 持久传播和非 LDDM `E_INVALIDARG` 非持久化；未扩展 dirty-region 协议、software dirty、ScrollBlt、普通 scene 生命周期或 dummy back buffer。
- 验证：surface render-target 聚焦测试 476/476、全量主测试 2811/2811、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `CHwDisplayRenderTarget::PresentInternal` 在 copy 与非 copy swap effect 下的 source/destination/dirty-region 参数选择及 `S_PRESENT_OCCLUDED` 保留边界。

上一轮依据原生 `CHwSurfaceRenderTarget::BeginLayerInternal` 与 `CBaseSurfaceRenderTarget::EndLayer` 证据，闭环 opaque partial-capture 返回空矩形集时的 no-fixup 边界：

- partial-capture 只在 `!HasAlpha()` 时尝试；判定成功且返回零矩形时 `fCopyEntireLayer == false`、`cCopyRects == 0`，因此跳过 destination texture capture 与透明 clear，source bitmap 保持为空且 BeginLayer 成功。
- 外层 EndLayer 发现 source bitmap 为空时不调用 `EndLayerInternal`，也不释放 bitmap；仍恢复 previous bounds、按 alpha 条件恢复 ClearType 状态并完成弹层。partial 判定失败继续按完整 layer capture 处理。
- 现有 `Direct3D9SurfaceRenderTarget.BeginLayerInternal` 与 `EndLayer` 已符合该契约，无需修改生产代码；回归覆盖空矩形跨 Begin/End、partial 判定失败回退完整捕获，以及重复 Dispose 后释放后保护优先于回调参数检查。
- 未扩展完整 layer stack、opaque geometry fixup、alpha/geometry mask、effects/UCE、生产 ABI 或 COM 接口表。
- 验证：surface render-target 聚焦测试 468/468、全量主测试 2798/2798、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `EndLayerInternal` 的 use-context/device-entry、target/clip/2D 状态准备首错短路，以及 alpha 目标最终 `SourceUnder` 合成的现有窄边界。

上一轮依据原生 `CHwSurfaceRenderTarget::BeginLayerInternal`、`CBaseSurfaceRenderTarget::BeginLayer` 失败弹栈和 `CHwRenderTargetLayerData` 析构证据，闭环捕获成功后透明清理失败的 source-bitmap 所有权：

- `GetHwDestinationTexture` 成功后 source bitmap 已由 layer 数据拥有；随后 alpha 目标 `ColorFill` 失败时保留该引用并直接传播首个 HRESULT，不在 surface 委托层清空或释放。
- 上层 BeginLayer 失败路径弹出 layer，由 layer 数据析构恰好释放一次 source bitmap；capture 自身失败时托管输出保持清空，透明清理不执行，上层没有 bitmap 可释放。
- 现有 `Direct3D9SurfaceRenderTarget.BeginLayerInternal` 与 `EndLayer` 已符合该契约，无需修改生产代码；新增跨入口回归覆盖 clear 失败后的单次释放、capture 失败无释放及 device-entry/use-context 确定性退出。
- 未扩展完整 layer stack、alpha/geometry mask、effects/UCE、生产 ABI 或 COM 接口表。
- 验证：surface render-target 聚焦测试 464/464、全量主测试 2794/2794、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 opaque layer 的 partial-capture 判定成功但返回零个矩形时，跳过 capture、source bitmap 保持为空且 EndLayer 无 fixup/释放的窄边界。

上一轮依据原生 `CHwSurfaceRenderTarget::CreateRenderTargetBitmap` hardware 分支调用 `CHwTextureRenderTarget::Create` 的证据，闭环真实 HW intermediate 创建委托与输出所有权：

- `Direct3D9SurfaceRenderTarget.CreateRenderTargetBitmap` 的对象重载继续按原生顺序执行尺寸、wrap mode、software-only、software device 与 realization cache index 门控，并在 hardware 分支把 width、height、device、associated display 与 `ForBlending` 原样交给 `Direct3D9TextureRenderTarget.TryCreate`。
- texture render-target 创建失败保持输出为空并传播首个 HRESULT，不回退 software、不提交 hardware-used 状态；委托返回部分候选时由 surface render target 确定性释放。成功后才向调用者转移一个对象所有权并提交 hardware-used 状态。
- 原 `nint` 窄重载保持不变；未扩展 meta render target、active displays、software intermediate 对象模型、完整 `IntermediateRTUsage`、生产 ABI 或 COM 接口表。
- 验证：surface render-target 聚焦测试 462/462、全量主测试 2792/2792、ABI 8/8、Debug 四项目构建 0 警告、0 错误。
- 下一切片唯一目标：核对 `BeginLayerInternal` 捕获成功后透明清理失败时 source bitmap 的保留与上层恰好一次清理边界。

上一轮依据原生 `CHwSurfaceRenderTarget::DrawVideo` 的虚 `IsValid()` 边界，闭环直接 surface/texture 目标失效后的 video 绘制语义：

- `Direct3D9SurfaceRenderTarget.DrawVideo` 的真实 video 管线在 device/use-context 内对已绑定具体目标统一应用有效性判断：display 目标检查 swap chain，直接 surface/texture 目标检查 render-target surface。
- 失效直接 surface 静默成功，并在 surface renderer `BeginRender`、bitmap `AddRef`、prefilter 修改与 bitmap 绘制前退出；作用域仍确定性配对退出，display 内容有效性语义保持既有行为。
- 无具体底层资源的纯依赖注入路径保持既有测试边界；未扩展生产 ABI或引入 video 私有模型、effects/UCE/资源协议或 `Reset/ResetEx`。
- 验证：新增直接 surface 失效回归 1/1、目标 render-target 测试 415/415、全量主测试 2440/2440、ABI 8/8、Debug 构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描 `hwsurfrt.cpp` 相邻绘制入口，选择已有托管生产管线可表达且不依赖未翻译私有模型的真实有效性、作用域或 HRESULT 边界。

上一轮依据原生 `CHwSurfaceRenderTarget::DrawGlyphs` 的虚 `IsValid()` 边界，闭环直接 surface/texture 目标失效后的 glyph 绘制语义：

- `Direct3D9SurfaceRenderTarget.DrawGlyphsCore` 的真实 glyph 管线在 device/use-context 内对已绑定具体目标统一应用有效性判断：display 目标检查 swap chain，直接 surface/texture 目标检查 render-target surface。
- 失效直接 surface 静默成功，并在文本 capability、brush realization、state、hardware painter 与 software fallback 前退出；作用域仍确定性配对退出。
- 无具体底层资源的纯依赖注入路径保持既有测试边界；未扩展生产 ABI或引入 glyph 私有模型、effects/UCE/资源协议或 `Reset/ResetEx`。
- 验证：新增直接 surface 失效回归 1/1、目标 render-target 测试 414/414、全量主测试 2439/2439、ABI 8/8、Debug 构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描 `hwsurfrt.cpp` 相邻绘制入口，选择已有托管生产管线可表达且不依赖未翻译私有模型的真实有效性、作用域或 HRESULT 边界。

上一轮依据原生 `CHwSurfaceRenderTarget::DrawPathInternal` 的虚 `IsValid()` 边界，闭环直接 surface/texture 目标失效后的 path 绘制语义：

- `Direct3D9SurfaceRenderTarget.DrawPathInternal` 的真实 path 管线在 device/use-context 内对已绑定具体目标统一应用有效性判断：display 目标检查 swap chain，直接 surface/texture 目标检查 render-target surface。
- 失效直接 surface 静默成功，并在 infinite shape 创建、fill/stroke brush realization、shape bounds、widen、state 与 fill-path 依赖前退出；作用域仍确定性配对退出。
- 无具体底层资源的纯依赖注入路径保持既有测试边界；未扩展生产 ABI或引入私有几何 mask/effects/UCE 模型、资源协议或 `Reset/ResetEx`。
- 验证：新增直接 surface 失效回归 1/1、目标 render-target 测试 413/413、全量主测试 2438/2438、ABI 8/8、Debug 构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描 `hwsurfrt.cpp` 相邻绘制入口，选择已有托管生产管线可表达且不依赖未翻译私有模型的真实有效性、作用域或 HRESULT 边界。

上一轮依据原生 `CHwSurfaceRenderTarget::DrawBitmap` 的虚 `IsValid()` 边界，闭环直接 surface/texture 目标失效后的 bitmap 绘制语义：

- `Direct3D9SurfaceRenderTarget.DrawBitmap` 的真实 bitmap 管线在 device/use-context 内对已绑定具体目标统一应用有效性判断：display 目标检查 swap chain，直接 surface/texture 目标检查 render-target surface。
- 失效直接 surface 静默成功，并在 scratch brush、bitmap bounds、shape、state 与 fill-path 依赖前退出；作用域仍确定性配对退出。
- 无具体底层资源的纯依赖注入路径保持既有测试边界；未扩展生产 ABI或引入私有 bitmap/effects/UCE 模型、资源协议或 `Reset/ResetEx`。
- 验证：新增直接 surface 失效回归 1/1、目标 render-target 测试 427/427、全量主测试 2437/2437、ABI 8/8、Debug 构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描 `hwsurfrt.cpp` 相邻绘制入口，选择已有托管生产管线可表达且不依赖未翻译私有模型的真实有效性、作用域或 HRESULT 边界。

上一轮依据原生 `CHwSurfaceRenderTarget::Begin3DInternal` 的虚 `IsValid()` 边界，闭环直接 surface/texture 目标失效后的 3D 状态语义：

- `Direct3D9SurfaceRenderTarget.Begin3D` 对已绑定的具体底层资源统一应用目标类型有效性判断：display 目标检查 swap chain，直接 surface/texture 目标检查 render-target surface。
- 失效直接 surface 静默成功并清空当前 3D bounds，仍进入可由 `End3D` 配对退出的空 3D context；`End3D` 恢复原 bounds，期间不会进入 render-target/depth setup 或其他 D3D 路径。
- 无具体底层资源的纯状态注入路径保持既有测试边界；未扩展生产 ABI或引入 layer stack、几何 mask/effects/UCE/资源协议/`Reset/ResetEx`。
- 验证：新增直接 surface 失效回归 1/1、目标 render-target 测试 426/426、全量主测试 2436/2436、ABI 8/8、Debug 构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描 `BaseSurfRT.cpp`/`hwsurfrt.cpp` 邻近路径，选择不依赖未翻译私有模型且可独立验证的真实 HW renderer 边界。

上一轮依据原生 `CHwSurfaceRenderTarget::Clear` 的虚 `IsValid()` 边界，闭环不同 HW render-target 类型的有效性判断：

- `Direct3D9SurfaceRenderTarget.IsValid` 现在区分 display swap-chain 目标与直接 surface/texture 目标：前者要求 swap chain 有效，后者要求 render-target surface 有效，避免有效的无 swap-chain 目标被误判并跳过 clear。
- 失效 display 目标在 device scope 内静默成功并跳过 render target、viewport、scissor 与 clear；有效目标继续按当前 `_bounds` 限定 clip，并保持 render target → clip → clear 顺序、颜色预乘、HRESULT和释放后保护。
- 未扩展生产 ABI或引入 layer stack、几何 mask/effects/UCE/资源协议/`Reset/ResetEx`。
- 验证：新增失效目标回归 1/1、目标 render-target 测试 410/410、全量主测试 2435/2435、ABI 8/8、Debug 构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描 `BaseSurfRT.cpp`/`hwsurfrt.cpp` 邻近路径，选择不依赖未翻译私有模型且可独立验证的真实 HW renderer 边界。

上一轮依据原生 `CBaseSurfaceRenderTarget::EndLayer`，闭环 layer 结束时的外层所有权与状态恢复边界：

- `Direct3D9LayerEndState` 增加内部 `PreviousBounds` 与 `SavedClearTypeHint`；`Direct3D9SurfaceRenderTarget.EndLayer` 仅在存在 saved source bitmap 时调用 `EndLayerInternal`，无 fixup 时直接成功。
- 成功、失败和无 fixup 均归一化 no-render HRESULT、恢复上一层 bounds、恢复 alpha target ClearType hint，并确定性释放 source bitmap；`EndLayerInternal` 继续只承担表面状态恢复与 `SourceUnder` 合成。
- 未扩展生产 ABI，也未引入 layer stack、几何 mask/effects/UCE/资源协议或 `Reset/ResetEx`。
- 验证：目标 render-target 测试 408/408、全量主测试 2433/2433、ABI 8/8、Debug 构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描 `BaseSurfRT.cpp`/`hwsurfrt.cpp` 邻近路径，选择不依赖未翻译 layer 私有模型且可独立验证的真实 HW renderer 边界。

上一轮依据原生 `CHwSurfaceRenderTarget::EndLayerInternal`，修正重新绑定 render target 后的当前 clip 恢复：

- `Direct3D9LayerEndState` 新增内部 `CurrentClip`，`EndLayerInternal` 按 render target → 当前 clip → 2D state → alpha target `SourceUnder` 合成执行，不再将 layer bounds 误作当前 clip。
- 保持首个 HRESULT、无 alpha target 跳过最终 under、无效目标静默成功及 device/use-context 确定性退出；未扩展生产 ABI或引入几何 mask/effects/UCE/资源协议/`Reset`。
- 验证：相关测试 417/417、全量主测试 2431/2431、ABI 8/8、Debug 构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描 `EndLayerInternal` 邻近路径，选择不依赖几何 mask/effect 私有模型且可独立验证的真实状态或调用边界。

本轮依据原生 `CD3DRenderState::InitPixelShaders`，闭环可独立验证的 text pixel shader 初始化状态：

- 按 pixel shader 2.0/1.1 与 L8/非 L8 alpha texture format 选择原生资源组 112–115、108–111、104–107 或 100–103，并按 CTSB → GSSB → CTTB → GSTB 顺序创建。
- `Direct3D9Device` 拥有四个 shader，并以只读 `IsTextPixelShaderInitialized` 表示完整成功；任一创建失败传播首个 HRESULT，释放失败返回值及此前已创建资源，设备释放时确定性清理全部 shader。
- 尚未接入真实 shader resource bytecode 加载和设备管理器初始化，因此 `CanDrawText` 继续保持关闭；未扩展生产 ABI或虚构 bytecode、glyph 私有模型/UCE/effects/`Reset`。
- 验证：定向数据/回归 6/6、全量主测试 2423/2423、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：闭环 `CreatePixelShaderFromResource` 的真实资源加载边界与设备管理器接线；四个真实 shader 全部成功前不得开启 `CanDrawText`。

上一轮依据原生 `CD3DRenderState::Init`，闭环 text pixel shader 初始化前的基础 capability 门控：

- `Direct3D9Device` 新增只读 `IsTextPixelShaderInitializationEligible`，按 pixel shader 1.1、至少 4 个 texture blend stages、blend factor capability、硬件文本禁用开关及 alpha texture 初始化结果共同决定。
- `Direct3D9DeviceManager` 在 texture-format support 探测后执行门控；不满足 capability、显式禁用或 alpha texture 失败均合法关闭硬件文本路径，不终止设备创建。
- 真实 text pixel shader resource 尚未创建，因此 `CanDrawText` 仍不自动开启；保持 HRESULT、创建顺序和释放后保护，未扩展生产 ABI或引入 shader bytecode、glyph 私有模型/UCE/effects/`Reset`。
- 验证：新增定向数据/回归 7/7，全量主测试 2417/2417、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：追踪并闭环一个可独立验证的 `InitPixelShaders` 状态；真实 shader 创建完整成功前不得自动开启 `CanDrawText`。

上一轮依据原生 `CD3DRenderState::Init` / `InitAlphaTextures`，闭环 glyph alpha texture 格式初始化状态：

- `Direct3D9Device` 新增设备拥有的 `GlyphAlphaTextureFormat`，设备管理器在 texture-format support 探测后按 A8 → L8 → P8 的原生优先级初始化。
- P8 路径复用既有 `SetLinearPalette`，只有调色板 entries 与 current palette 均成功后才提交 P8；无支持格式或调色板失败保持 `Undefined`，且不使设备创建失败。
- 尚未闭环 text pixel shader resource 创建，因此该状态不自动开启 `CanDrawText`；保持 HRESULT、设备错误映射、释放后保护和创建顺序，未扩展生产 ABI或引入 glyph 私有模型/UCE/effects/`Reset`。
- 验证：新增定向数据/回归 6/6，全量主测试 2410/2410、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：继续闭环基础文本 capability 门控或可独立验证的 text pixel shader 初始化状态；真实 shader 创建完成前不得自动开启 `CanDrawText`。

上一轮依据原生 `CHwSurfaceRenderTarget::DrawGlyphs` 直接查询 `m_pD3DDevice->CanDrawText()`，闭环 glyph 硬件 eligibility 的设备所有权：

- `Direct3D9Device` 新增只读 `CanDrawText` 初始化状态；`Direct3D9SurfaceRenderTarget` 新增标准 glyph 入口并直接查询设备，不再要求该入口的调用方传入设备 capability。
- capability 为 false 时跳过硬件 brush realization，保持 `EnsureState` 后以 `WGXERR_DEVICECANNOTRENDERTEXT` 进入软件 fallback；为 true 时保持 realization → state → hardware painter 顺序。能力值暂由尚未翻译的 alpha texture/pixel shader 初始化边界注入，不虚构 capability 推导。
- 保持 ClearType、brush eligibility、fallback reason、HRESULT/no-render 归一化、device/use-context 顺序和释放后保护；未扩展生产 ABI，也未引入 glyph painter/glyph run 私有模型、UCE、资源协议、effects 或 `Reset/ResetEx`。
- 验证：新增定向数据 2/2，全量主测试 2404/2404、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描原生 `core/hw/hwsurfrt.cpp` 的 `DrawGlyphs` 邻近路径，优先闭环设备文本能力初始化所依赖且可独立验证的 alpha texture/pixel shader 状态，或选择其他不依赖 glyph 私有模型、无需扩展生产 ABI 的真实 HW renderer 切片；不虚构 effects、UCE、资源协议或 `Reset/ResetEx`。

上一轮依据原生 `CHwSurfaceRenderTarget::SoftwareDrawGlyphs` 自身的 `ENTER_USE_CONTEXT_FOR_SCOPE`，闭环 glyph 软件回退的嵌套 use-context 所有权：

- `Direct3D9SurfaceRenderTarget.SoftwareDrawGlyphs` 在外层 `DrawGlyphs` 已进入 use-context 后再进入一层嵌套 use-context，覆盖软件 brush realization、fallback 获取与实际 glyph 绘制；退出后资源 use-depth 恢复为零。
- 保持 fallback reason、ClearType capability、空 brush 短路、HRESULT 首错、no-render 归一化、device scope 与释放后保护不变；未扩展生产 ABI，也未引入 glyph painter/glyph run 私有模型、UCE、资源协议、effects 或 `Reset/ResetEx`。
- 验证：新增定向回归 1/1，全量主测试 2402/2402、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描原生 `core/hw/hwsurfrt.cpp` 的 `DrawGlyphs` 邻近路径，优先选择不依赖 glyph painter/glyph run 私有模型、无需扩展生产 ABI且可独立验证的真实 HW renderer 切片；不虚构 effects、UCE、资源协议或 `Reset/ResetEx`。

上一轮依据原生 `CHwSurfaceRenderTarget::DrawGlyphs` 的 `m_forceClearType || !HasAlpha()` 判定，闭环 glyph 绘制入口的 ClearType target capability 所有权：

- `Direct3D9SurfaceRenderTarget` 新增只读 `_forceClearType` 构造状态，并新增按 `canDrawText` 调用的 glyph 入口；该入口不再要求调用方虚构 target capability，而是由 render target 自身按原生规则计算 ClearType 支持。
- 无 alpha target 支持 ClearType；PBGRA/PRGBA alpha target 默认不支持；显式 `forceClearType` 可覆盖 alpha 限制。计算结果统一传给既有硬件 painter 与软件 fallback 核心，保持 eligibility、brush realization、`EnsureState`、fallback reason、no-render HRESULT 归一化和设备/use-context 顺序不变。
- 未扩展生产 ABI，也未引入 glyph painter/glyph run 内部模型、UCE、资源协议、effects 或 `Reset/ResetEx`。
- 验证：新增定向数据 3/3，全量主测试 2401/2401。因用户要求快速结束，本轮未重新运行 ABI 集成测试与完整解决方案构建；最近已知基线仍为 ABI 8/8、Debug 构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描原生 `core/hw/hwsurfrt.cpp` 的 `DrawGlyphs` 邻近路径，优先选择不依赖 glyph painter/glyph run 私有模型、无需扩展生产 ABI且可独立验证的真实 HW renderer 切片；不虚构 effects、UCE、资源协议或 `Reset/ResetEx`。

上一轮依据原生 `CHwSurfaceRenderTarget::HwShaderFillPath` 的 AA/aliased 几何生成、empty-fill 成功归一化、shader vertex-buffer 绘制与 cleanup 顺序，闭环独立的硬件 shader path 绘制切片：

- `Direct3D9SurfaceRenderTarget.HwShaderFillPath` 按 anti-alias mode 选择硬件 rasterizer 或 fill tessellator 对应的几何生成委托，并将原始 shape、可选 ShapeToDevice 与 rendering bounds 传入几何阶段。
- 几何为空时将 `WGXHR_EMPTYFILL` 归一化为成功并跳过绘制；创建成功却返回空 geometry 时返回内部错误；其他创建或 shader 绘制失败保持首个 HRESULT。
- shader 绘制获得 shader、geometry、rendering bounds 和当前 Z-buffer enable，任何绘制结果下都确定性释放已创建 geometry；未扩展生产 ABI，也未引入 effects、UCE、资源协议或 `Reset/ResetEx`。
- 验证：新增定向测试 6/6，全量主测试 2398/2398、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描原生 `core/hw/hwsurfrt.cpp` 中 `HwShaderFillPath` 之后的 glyph 或邻近真实绘制路径，选择一个无需扩展生产 ABI、可独立验证且直接推进完整 HW 渲染器的切片；不虚构 effects、UCE、资源协议或 `Reset/ResetEx`。

上一轮依据原生 `CHwSurfaceRenderTarget::Ensure3DRenderTarget` 仅管理 `m_pD3DIntermediateMultisampleTargetSurface`、构造注入专用 3D surface 属于外部固定依赖的所有权边界，闭环动态 multisample intermediate 与专用 3D surface 的职责分离：

- `Direct3D9SurfaceRenderTarget.Ensure3DRenderTarget` 现在只复用、检查、替换和释放内部 `_intermediateMultisampleSurface`；不再把 `_renderTargetSurfaceFor3D` 当作动态 intermediate，也不查询或释放该外部 surface。
- 新增专用 3D surface 存在时仍创建动态 multisample intermediate 的回归，并修正既有 End3D copy-back 失败测试，使其显式提供动态 render-target 创建结果，不再依赖已移除的错误所有权行为。
- 保持 HRESULT 首错传播、OOM multisample 降级、成员 intermediate 复用与最终释放、外部 surface identity 和释放后保护；未扩展生产 ABI，也未引入 effects、UCE、资源协议或 `Reset/ResetEx`。
- 验证：全量主测试 2392/2392、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描原生 `core/hw/hwsurfrt.cpp` 与现有 `Direct3D9SurfaceRenderTarget`，选择一个无需扩展生产 ABI、可独立验证且直接推进完整 HW 渲染器的真实绘制或资源状态切片；不虚构 effects、UCE、资源协议或 `Reset/ResetEx`。

上一轮依据原生 `CHwSurfaceRenderTarget::DrawBitmap` 在 source rect 解析后先执行 `bitmapShape.Set(rcSource)`、随后才调用 `EnsureState` 的顺序，闭环 bitmap shape 准备阶段：

- `Direct3D9SurfaceRenderTarget.DrawBitmap` 的完整 FillPath 路径现在在解析显式 source rect 或 bitmap bounds 后立即创建并验证 bitmap shape，再执行 render-target/clip/device state 确保；通用简化重载继续保持既有行为。
- shape 创建失败或成功却返回空 shape 时立即传播首个 HRESULT，并跳过 `EnsureState`、clip/fill、scratch bitmap brush 临时设置与 brush realizer 初始化；scratch brush 获取仍保持在 source rect/shape 之前，匹配原生调用顺序。
- 更新 bitmap 与 video 复用路径的完整顺序断言，并新增 shape 创建失败短路回归；未扩展生产 ABI，也未引入 effects、UCE、资源协议或 `Reset/ResetEx`。
- 验证：定向测试 6/6、全量主测试 2391/2391、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描原生 `core/hw/hwsurfrt.cpp` 与现有 `Direct3D9SurfaceRenderTarget`，选择一个无需扩展生产 ABI、可独立验证且直接推进完整 HW 渲染器的真实绘制或资源状态切片；不虚构 effects、UCE、资源协议或 `Reset/ResetEx`。

上一轮依据原生 `CHwSurfaceRenderTarget::Setup3DRenderTargetAndDepthState`、`EnsureDepthState` 与 `Ensure3DState`，闭环当前 `Begin3D` 上下文的动态 depth-state：

- `Direct3D9SurfaceRenderTarget` 在 render-target 与 clip 成功后记录当前 `Begin3D` 的 `useZBuffer`，并在 `Ensure3DState` 处于活动 3D 上下文时绑定本轮创建或复用的 depth-stencil surface，仅在当前活动深度开启时设置 context Z function。
- 非活动 3D 边界继续保留构造时 `_depthStencilSurface`/`_isDepthBufferEnabled` 行为；`End3D` 无论 copy-back 成功或失败都清除活动深度标记，后续状态确保不再误用上一轮 `Begin3D` 的动态 depth surface。
- 保持原生 render-target → clip → depth ensure/bind → depth clear 顺序、HRESULT 首错短路、OOM multisample 降级、surface identity、失败逆序清理和释放后保护；未扩展生产 ABI，也未引入 effects、UCE、资源协议或 `Reset/ResetEx`。
- 新增活动 3D 深度面 identity 绑定和 `End3D` 后不复用活动深度面的回归；全量主测试 2390/2390、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描原生 `core/hw/hwsurfrt.cpp` 与现有 `Direct3D9SurfaceRenderTarget`，选择一个无需扩展生产 ABI、可独立验证且直接推进完整 HW 渲染器的真实绘制或资源状态切片；不虚构 effects、UCE、资源协议或 `Reset/ResetEx`。

上一轮依据原生 `CHwSurfaceRenderTarget::Ensure3DState` 对 `m_pD3DTargetSurfaceFor3DNoRef->Desc().MultiSampleType` 的判断，闭环 3D 状态设置对当前 3D render-target surface 实际 multisample 描述的依赖：

- 不再仅依据构造时缓存的 3D multisample 值决定该状态；当缓存值与当前 3D surface 实际描述不一致时，以原生代码使用的 surface 描述为准。无实际 surface 的既有测试边界仍使用缓存值，不扩展生产 ABI。
- 保持原生调用顺序、HRESULT 首错短路与释放后保护；新增缓存 multisample/实际非 multisample 及实际 multisample/缓存非 multisample 两种错配回归。
- 验证：全量主测试 2388/2388、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描原生 `core/hw/hwsurfrt.cpp` 与现有 `Direct3D9SurfaceRenderTarget`，选择一个无需扩展生产 ABI、可独立验证且直接推进完整 HW 渲染器的真实绘制或资源状态切片；不虚构 effects、UCE、资源协议或 `Reset/ResetEx`。

上一轮依据原生 `CHwSurfaceRenderTarget::Ensure2DState` 对 `m_pD3DTargetSurface->Desc().MultiSampleType` 的判断，闭环 2D 状态设置对当前 render-target surface 实际 multisample 描述的依赖：

- 不再仅依据构造时缓存的 multisample 值决定该状态；当缓存值与 surface 实际描述不一致时，以原生代码使用的 surface 描述为准。无实际 surface 的既有测试边界仍使用缓存值，不扩展生产 ABI。
- 保持原生调用顺序、HRESULT 首错短路和释放后保护；新增缓存 multisample/实际非 multisample 及实际 multisample/缓存非 multisample 两种错配回归。
- 验证：全量主测试 2386/2386、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描原生 `core/hw/hwsurfrt.cpp` 与现有 `Direct3D9SurfaceRenderTarget`，选择一个无需扩展生产 ABI、可独立验证且直接推进完整 HW 渲染器的真实绘制或资源状态切片；不虚构 effects、UCE、资源协议或 `Reset/ResetEx`。

上一轮依据原生 `CHwSurfaceRenderTarget::EnsureClip` 对 `CBaseRenderTarget::UpdateCurrentClip` 的调用，补齐 render-target bounds 与 aliased clip 的严格交集语义：

- `EnsureClip` 在 device use-context 内计算 render-target bounds 与 aliased clip 的交集；空交集返回 `WGXHR_CLIPPEDTOEMPTY` 且不调用 device，成功时仅下发交集矩形并传播设备 HRESULT。
- 新增空 bounds/非空 clip 的无设备调用回归，旧有效 2D 状态顺序测试补齐 16×16 与 5×5 的目标尺寸覆盖。
- 保持原生调用顺序、HRESULT 首错短路与释放后保护；未扩展生产 ABI，也未引入 effects、UCE、资源协议或 `Reset/ResetEx`。
- 验证：定向回归通过，全量主测试 2384/2384、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描原生 `core/hw/hwsurfrt.cpp` 与现有 `Direct3D9SurfaceRenderTarget`，选择一个无需扩展生产 ABI、可独立验证且直接推进完整 HW 渲染器的真实绘制或资源状态切片；不虚构 effects、UCE、资源协议或 `Reset/ResetEx`。

上一轮依据原生 `CHwSurfaceRenderTarget::EndLayerInternal` 的目标恢复顺序，闭环不依赖未翻译 layer 内部模型的最终合成前状态恢复：

- `Direct3D9SurfaceRenderTarget.EndLayerInternal` 新增仅接收最终 saved-layer composite 操作的生产入口，复用现有 `SetAsRenderTarget`、`_device.SetClipRect` 与 `Ensure2DState`；保留全注入重载作为状态顺序和失败传播测试边界。
- 保持原生有效性门控与调用顺序：进入 use-context/device scope，无效 render target 成功短路；有效目标依次恢复当前 2D render-target、设置 layer bounds clip、确保 2D device state，最后仅在目标含 alpha 时以 `SourceUnder` 合成 saved bitmap。任一状态步骤失败均传播首个 HRESULT 并跳过后续步骤。
- 扩展 `Direct3D9SurfaceRenderTargetBindingTests`，通过真实 `Resize` 建立有效 swap-chain/back-buffer，覆盖生产路径选择 2D target、完整 target/viewport/layer-clip/2D/composite 顺序，以及绑定失败短路；测试不绕过 `IsValid` 门控，也不扩展生产 ABI、layer/mask/brush/effect 模型或 UCE 资源协议。
- 验证：EndLayer 定向测试通过，全量主测试 2383/2383、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- `EndLayerInternal` 剩余主体仍依赖未翻译的 layer stack、mask/brush/effect context 与真实 bitmap compositing；下一切片继续扫描原生 `core/hw/hwsurfrt.cpp`，只选择现有生产契约可承载且可独立验证的 HW 绘制或资源状态切片。

本轮继续依据原生 `CHwSurfaceRenderTarget::EnsureState` 对 `SetAsRenderTarget` 的调用，闭环状态确保与 render target 实际 2D/3D 上下文的一致性：

- `Direct3D9SurfaceRenderTarget.EnsureState` 统一复用无参数 `SetAsRenderTarget`，由 render target 当前 `_in3D` 状态选择普通 2D surface 或 active/intermediate 3D surface，不再由可能错配的 `contextState.In3D` 决定绑定 surface。
- 保持原生调用顺序：绑定 render target、设置 clip、重置每图元资源使用，再按 context state 确保 2D/3D device state；绑定失败仍传播首个 HRESULT 并跳过后续调用。
- 扩展 `Direct3D9SurfaceRenderTargetBindingTests`，覆盖 context state 与 render-target 上下文错配时仍选择实际 2D/3D surface；旧 3D 顺序测试先通过 `Begin3D` 建立真实 render-target 3D 状态。
- 验证：定向绑定与 3D 顺序测试通过，全量主测试 2381/2381、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描原生 `core/hw/hwsurfrt.cpp` 与现有 `Direct3D9SurfaceRenderTarget`，选择一个无需扩展生产 ABI、可独立验证且直接推进完整 HW 渲染器的真实绘制或资源状态切片；不虚构 effects、UCE、资源协议或 `Reset/ResetEx`。

上一轮依据原生 `CHwSurfaceRenderTarget::Clear` 对 `SetAsRenderTarget` 的调用，闭环 Clear 与当前 2D/3D render-target 绑定规则：

- `Direct3D9SurfaceRenderTarget.Clear` 不再直接绑定固定 2D surface，统一复用 `SetAsRenderTarget`；普通上下文选择 2D target，3D 上下文选择 active/intermediate 3D target。
- 保持原生调用顺序：绑定 render target、设置 clip、执行无矩形 `D3DCLEAR_TARGET`；绑定失败立即传播首个 HRESULT 并跳过 clip/clear，继续保持 device entry、空颜色/空交集短路、颜色转换与释放后保护。
- 扩展 `Direct3D9SurfaceRenderTargetBindingTests`，覆盖 3D Clear 绑定 active 3D surface 后再清理，以及绑定失败时跳过 Clear。
- 验证：定向测试 6/6、全量主测试 2379/2379、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 下一切片唯一目标：继续扫描原生 `core/hw/hwsurfrt.cpp` 与现有 `Direct3D9SurfaceRenderTarget`，选择一个无需扩展生产 ABI、可独立验证且直接推进完整 HW 渲染器的真实绘制或资源状态切片；不虚构 effects、UCE、资源协议或 `Reset/ResetEx`。

上一轮依据原生 `CHwSurfaceRenderTarget::PopulateDestinationTexture`，闭环 render-target surface 到目标纹理 level 0 的真实资源复制切片：

- `Direct3D9SurfaceRenderTarget.PopulateDestinationTexture` 在 device use-context 内调用目标 `Direct3D9Texture.TryGetSurfaceLevel(0)`，成功后通过既有 `_device.StretchRect` 将指定源矩形复制到指定目标矩形。
- 严格保持原生 level 0、`D3DTEXF_NONE`、GetSurfaceLevel 失败短路、StretchRect HRESULT 首错传播、确定性 surface 引用释放和 render target 释放后保护；未扩展生产 ABI，也未引入 effects、UCE、资源协议或 `Reset/ResetEx`。
- 新增 `Direct3D9SurfaceRenderTargetDestinationTextureTests`，覆盖 level 0、源/目标矩形、无过滤、use-context、GetSurfaceLevel 失败跳过复制、StretchRect 失败传播及 disposed 保护。
- 验证：定向测试 4/4、全量主测试 2373/2373、ABI 集成测试 8/8，Debug 解决方案构建 0 警告、0 错误。
- 原生 `CHwHWNDRenderTarget::ScrollBlt` 当前直接返回 `E_NOTIMPL`，现有实现已一致，无需修改。
- 下一切片唯一目标：继续扫描原生 `core/hw/hwsurfrt.cpp` 与现有 `Direct3D9SurfaceRenderTarget`，选择一个无需扩展生产 ABI、可独立验证且直接推进完整 HW 渲染器的真实绘制或资源状态切片；不虚构 effects、UCE、资源协议或 `Reset/ResetEx`。

上一轮完成的 shader builder path effect context 消费继续生效：

- `Direct3D9SetupPathShaderPipeline` 与 path 专用 `InitializeForRendering` 重载将同一 `Direct3D9PathBrushContext` 及其 world-to-device、rendering/sampling bounds、fallback 状态原样交给 builder Setup。
- 保持 Setup、2D vertex builder mappings、outside bounds、geometry generator 与 shader 获取顺序；既有通用句柄入口继续保留。

上一轮完成的 `FillPathWithBrush` brush context 建立继续生效：

- 新增不可变引用类型 `Direct3D9PathBrushContext`，近似对应原生栈上且禁止复制的 `CHwBrushContext`；保存 `WorldToDevice`、device rendering bounds、由同一区域建立的 sampling bounds，以及 `fCanFallback = true`。
- `FillPathWithBrush` 在 bounds 交集成功后只创建一个 context 实例，并将同一实例依次传给硬件画刷派生和 geometry generator 创建后的实际加速绘制。
- 保持 HRESULT 首错传播、`WGXHR_EMPTYFILL` 成功归一化、硬件画刷优先于几何生成器的逆序清理及释放后保护。

上一轮完成的 `FillPathWithBrush` 几何资源生命周期继续生效：

- `Direct3D9SurfaceRenderTarget.FillPathWithBrush` 的 antialiased `CHwRasterizer` 与 aliased `CFillTessellator` 对应句柄均纳入确定性清理。
- 严格保持原生 `Cleanup` 顺序：先释放派生硬件画刷，再销毁 tessellator/rasterizer；成功绘制、绘制失败以及创建回调返回失败但已交付非零句柄时均执行清理。
- 保持空图元契约：`WGXHR_EMPTYFILL` 归一化为成功且不绘制；未创建几何生成器时不调用几何释放。

## 相关文档

- 当前唯一入口：[`../next-session-handoff.md`](../next-session-handoff.md)
- 原生到 C# 文件映射：[`../native-to-managed-file-map.md`](../native-to-managed-file-map.md)
