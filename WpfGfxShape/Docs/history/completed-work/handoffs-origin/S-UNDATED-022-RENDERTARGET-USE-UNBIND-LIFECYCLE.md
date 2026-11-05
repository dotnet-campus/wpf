# Render-target 使用身份与失败解绑生命周期闭环

## 原生证据

- `CD3DDeviceLevel1::SetRenderTarget` 仅在底层 slot 0 绑定成功后提交 `m_pCurrentRenderTargetNoRef`；该 identity 为非拥有借用，不执行 AddRef。
- viewport、clipping matrix 或 BeginScene 后续失败通过 `ReleaseUseOfRenderTarget` 清空 identity，按需忽略 EndScene 失败，借用设备持有的 dummy back-buffer 解绑，再释放当前 depth-stencil 使用；cleanup 不覆盖原始 HRESULT。
- `CD3DSurface::ReleaseD3DResources` 只在 surface usage 含 render-target 时通知 device；仅 identity 等于 current target 才解绑，非当前 target 直接释放自身 COM 引用。
- `HandlePresentFailure` 明确不能调用 `ReleaseUseOfRenderTarget`，因为 present 路径已经结束或处理 scene；它直接清空 current identity、借用 dummy 解绑并释放 depth-stencil，不再次 EndScene。
- device 析构先清空 current identity，再销毁资源，使 surface release 通知成为无操作；dummy 是 device owning 引用，所有解绑路径只借用。

## 托管实现

- `Direct3D9Device` 抽取统一 `UnbindCurrentRenderTarget(bool endScene)`，集中执行 current identity 清空、可选 scene cleanup、dummy slot 0 绑定和 depth-stencil 使用释放。
- `ReleaseUseOfRenderTarget` 仅在通知 surface identity 等于 current identity 时调用统一解绑，并传入 `endScene: true`。
- `HandlePresentFailure` 改为调用同一解绑实现并传入 `endScene: false`，保持 present 失败不重复 EndScene 的原生约束。
- 统一解绑首先清空 identity，使重复 cleanup、surface 随后释放和递归资源销毁均幂等；dummy 和 depth-stencil cleanup HRESULT 均忽略。
- device Dispose 调整为先清空 current render-target identity，再释放 dummy/Ex 并销毁资源，明确对齐原生析构顺序。

## 所有权与错误语义

- current render-target identity 始终非拥有；surface wrapper 自身拥有 COM identity，并在释放通知完成后才执行 COM Release。
- dummy back-buffer 由 device 持有唯一 owning 引用；unbind 仅借用，不 AddRef/Release。
- 普通 surface release cleanup 可尝试 EndScene；失败保留 `_inScene` 状态但仍继续 dummy/depth-stencil cleanup。
- present failure cleanup 不 EndScene；重复 present failure 不重复 dummy 绑定或 depth-stencil 释放。
- viewport/matrix/BeginScene 的原始失败继续保持首错，cleanup 失败不进行 HandleDIE 映射。

## 验证

- 强化 present failure 测试：失败解绑后 surface Dispose 不产生二次 SetRenderTarget、EndScene 或 depth-stencil cleanup。
- 新增重复 present failure 测试：dummy 与 depth-stencil 各只清理一次。
- render-target-state 定向测试通过。
- 全量主测试：3957/3957。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未新增普通 render-target dummy 策略、Reset/ResetEx、公开 ABI、COM interface 或完整 scene 生命周期；仅合并已有原生证据明确的 current identity 与失败解绑状态机。