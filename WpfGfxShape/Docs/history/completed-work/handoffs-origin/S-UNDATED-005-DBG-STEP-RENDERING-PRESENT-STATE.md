# CD3DDeviceLevel1 debug stepped-rendering present 状态差集审计

## 原生证据

- `CD3DDeviceLevel1` 构造时仅在 `DBG_STEP_RENDERING` 下把 `m_fDbgInStepRenderingPresent` 初始化为 `false`。
- `DbgBeginStepRenderingPresent()` 断言当前不在 stepped present 后置 `true`；`DbgEndStepRenderingPresent()` 断言当前在 stepped present 后置 `false`；`DbgInStepRenderingPresent()` 只返回该缓存状态。
- `CHwDisplayRenderTarget` 的 stepped-rendering 调试入口在取得临时 surface 和执行增量调试 Present 前调用 begin，并在统一 cleanup 尾部调用 end。
- `CD3DDeviceLevel1::MarkUnusable` 在该状态为 true 时延迟 manager 通知、GPU marker 重置和资源批量销毁，避免调试呈现中的 primitive 仍借用无独立引用的缓存资源。
- `CD3DResourceManager::EndFrame` 在该状态为 true 时直接返回，避免同一 use context 内的多次调试 Present 推进正常帧资源列表。

## 托管差集

- 托管项目未定义或启用 `DBG_STEP_RENDERING`，也没有 stepped-rendering 调试 Present 入口；普通 Present、`MarkUnusable` 和 `EndFrame` 均不得虚构该临时调试状态。
- 现有 `Direct3D9Device.MarkUnusable` 已覆盖正式生产路径的 entry-protected 与跨线程延迟清理，`Direct3D9ResourceManager.EndFrame` 已覆盖正常帧结束时的 use-context 不变量和列表迁移。
- 因此该组三个成员属于条件编译调试协议，没有独立生产实现或新增测试缺口；无需修改生产代码或测试。

## ABI、HRESULT 与所有权结论

- 三个状态成员不调用 COM，不涉及 Windows `Stdcall`、HRESULT 映射、AddRef/Release 或输出所有权。
- 调试状态只暂缓已有资源清理和帧推进，不取得资源所有权，也不改变普通 present、scene 或 render-target 协议。

## 验证

- `Direct3D9ResourceManagerTests`：199/199。
- `Direct3D9DeviceManagerTests`：113/113。
- 全量主测试：3859/3859。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未实现 debug stepped-rendering Present；未扩展普通 scene/render-target/present 生命周期、GDI/software-DC、effects/UCE、完整 glyph/shader、生产 ABI 或 `Reset/ResetEx`。