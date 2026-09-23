# CD3DDeviceLevel1::DbgCanShrinkRectLinear capability 差集审计

## 原生证据

- 声明与内联实现位于 `core/hw/d3ddevice.h` 的 `DBG_STEP_RENDERING` 条件块。
- `DbgCanShrinkRectLinear()` 只读取初始化缓存的 `m_caps.StretchRectFilterCaps`，判断 `D3DPTFILTERCAPS_MINFLINEAR` 位，不调用底层 D3D9，不进入 device/use context，不改变 HRESULT、COM 引用或设备状态。
- 唯一生产调用者位于 `CHwDisplayRenderTarget::DbgStepRenderingPresent`。只有 stepped-rendering 的增量调试呈现确实需要缩小源区域时，该位才在 `StretchRect` 的 `D3DTEXF_LINEAR` 与 `D3DTEXF_NONE` 之间选择；它不参与零售普通 present 生命周期。

## 托管差集

- `Direct3D9Device.CanGenerateMipmapsWithStretchRect` 已对同一缓存字段和同一 `D3D9.PtfiltercapsMinflinear` 位执行纯只读判断，并保持释放后保护。
- 现有 `Direct3D9DeviceCapabilityAccessorTests` 已覆盖该位存在、不存在、无关 capability 干扰、重复读取和不进入 device entry 的行为。
- 该原生成员仅属于 debug stepped-rendering 诊断路径；现有共享 capability 访问器已覆盖其可观察判断语义，因此无需新增生产 API、debug present 实现或测试。

## ABI、HRESULT 与所有权结论

- 本切片没有 COM 调用，不涉及 Windows `Stdcall` slot、AddRef/Release、输出所有权或失败清理。
- 本切片不产生或映射 HRESULT，也不改变当前 render target、scene、depth-stencil 或 present 状态。

## 验证

- `Direct3D9DeviceCapabilityAccessorTests`：200/200。
- 全量主测试：3859/3859。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未扩展 debug stepped-rendering present、普通 scene/render-target/present 生命周期、GDI/software-DC、effects/UCE、完整 glyph/shader、生产 ABI 或 `Reset/ResetEx`。