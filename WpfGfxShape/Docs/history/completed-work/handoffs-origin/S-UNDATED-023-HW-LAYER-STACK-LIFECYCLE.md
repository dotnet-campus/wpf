# 完成切片：HW render-target layer stack 与嵌套恢复生命周期

## 原生证据

- `core/targets/BaseSurfRT.cpp`：`BeginLayer` 先建立 layer state，保存 previous bounds/ClearType，按需复制 mask 与调用 target-specific begin；失败时弹栈并恢复。`EndLayer` 严格使用栈顶，结束后无论合成成功与否都恢复 bounds/ClearType 并 pop。`EndAndIgnoreAllLayers` 逆序销毁全部活动层。
- `core/targets/RTLayer.h`、`RTLayer.inl`：每帧拥有 geometric mask、alpha mask brush 和 target-specific source bitmap，栈严格 LIFO。
- `core/hw/hwsurfrt.cpp`：HW begin 支持完整或 partial capture；alpha target 在 capture 后透明清理。HW end 先恢复 target/clip/2D state，再消费 geometric mask、constant alpha、alpha-mask 窄边界并在 alpha target 最后执行 source-under。
- 原生 HW begin 当前对 alpha-mask brush 返回 `E_NOTIMPL`，但 end 路径已有未来消费占位。托管实现只建立有证据的 retain/compose/release 窄契约，不扩展 effects/UCE 或 brush 私有模型。

## 托管修改

- 在 `Direct3D9SurfaceRenderTarget` 增加 `Direct3D9LayerState`、`Direct3D9LayerCompositeState`、`Direct3D9LayerOperations` 和内部 `Direct3D9LayerFrame`。
- 新增拥有资源的 `_layerStack`。每帧保存 layer bounds、previous bounds、current clip、constant alpha、AA mode、geometric mask、alpha-mask brush、captured source bitmap、saved ClearType 与操作契约。
- 高层 `BeginLayer` 完成 bounds intersection、空层/no-fixup 判定、mask retain、partial/full capture、alpha target transparent clear、成功后 push，以及任一步失败时 source/mask 逆序释放与 bounds/ClearType 回滚。
- 高层 `EndLayer` 严格消费栈顶：先恢复父 target/clip/2D state，再调用强类型 composite；无论合成成功、失败或 no-render，均 pop、恢复 previous bounds/ClearType 并逆序释放 frame 资源，保持首个 HRESULT。
- `Present` 在存在活动层时返回 `WGXERR_INVALIDCALL`；`Resize`、显式 `EndAndIgnoreAllLayers` 和 `Dispose` 按 LIFO 丢弃活动层并释放所有拥有资源。
- 保留既有低层 `BeginLayerInternal`/`EndLayerInternal` 入口，继续覆盖原生 HW capture、transparent clear、state setup 和 source-under 细节；未虚构 effect list、UCE 或完整几何实现。

## 所有权与失败语义

- 只有 retain 成功后的 geometric mask/alpha-mask 和 capture 成功后的 source bitmap 进入 frame 所有权。
- Begin 只有全部步骤成功后才 push；失败时栈深不变，资源按 source bitmap、alpha mask、geometric mask 的逆序释放。
- End 先记录栈顶，合成失败不阻止 parent bounds/ClearType 恢复及资源释放；第二次 End 返回 `WGXERR_INVALIDCALL`。
- Dispose/Resize abort 不执行 composite，只按严格 LIFO 释放活动 frame；重复 Dispose 保持幂等。

## 验证

- layer 定向测试：30/30 通过，覆盖单层/两层嵌套、空层、partial capture、constant alpha、alpha mask retain/consume、Begin retain 失败、End composite failure、恢复顺序、重复 End、Present 拒绝、Resize abort、Dispose 活动层和释放后保护。
- 全量主测试：3966/3966 通过。
- ABI 测试：8/8 通过。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

- 未实现 effects/UCE、完整 alpha-mask brush realization、完整 geometric mask tessellation 或新的生产 ABI。
- 本切片关闭的是 HW render-target layer stack、嵌套恢复、capture/intermediate ownership、alpha/mask 合成契约、失败回滚和释放生命周期；阶段 2 下一项仍是几何 mask 的真实生产消费闭环。
