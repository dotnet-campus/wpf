# S-UNDATED-025：HW effect list 与 effect 消费链

## 完成切片

完整关闭阶段 2 的强类型 effect list、AlphaScale/AlphaMask pipeline 消费与 layer alpha-mask 生产接线。生产 accelerated fill 不再只能以裸 `nint effects` 表达 effect；旧入口保留为兼容薄边界。

## 原生证据

- `common/effects/effectlist.h/.cpp`：`CEffectList::ParamBlock` 分别记录 CLSID、参数大小/偏移和资源数量/偏移；AddWithResources 先复制参数与资源数组，成功后对每个资源 AddRef，Clear/析构逐个 Release；GetResources 返回带 AddRef 的资源副本。
- `core/hw/hwpipelinebuilder.cpp`：按 effect 插入顺序枚举；仅接受 AlphaScale 与 AlphaMask，未知 effect 返回 `WGXERR_UNSUPPORTED_OPERATION`；AlphaScale 验证参数/资源数量和 0..1 范围；AlphaMask 验证单资源，取得 transform 与 bitmap，基于 effect context 派生 bitmap color source，再进入 `Mul_AlphaMask`，临时资源在 Cleanup 释放。
- `core/uce/drawingcontext.cpp`：layer effect 顺序为 AlphaMask 后 AlphaScale；AlphaMask resource 由 effect list 持有；opacity 在 mask 后追加。

## 托管实现

- 新增 `Direct3D9EffectList`、`Direct3D9EffectEntry`、`Direct3D9EffectResource`、`Direct3D9AlphaMaskParameters` 与 `Direct3D9EffectProcessor`。
- effect list 保持插入顺序，复制参数值，并通过 retain/release 委托拥有 resource；Clear/Dispose 逆序释放，部分创建失败释放已 retained resource。
- shader 与 fixed-function processor 共用同一 effect 模型：AlphaScale 分别进入 constant-alpha operation，AlphaMask 派生 bitmap pipeline color source并进入 multiply-alpha operation；派生失败保持首错，builder 拒绝时立即释放 color source。
- `Direct3D9ShaderPipelineItemBuilder` 新增 AlphaMask texture operation；shader/fixed-function 都把 effect color source 所有权转移给 pipeline item。
- `Direct3D9PathHardwareBrush` 与 `ProductionAcceleratedFillPath` 新增强类型 effect overload；shader 返回 NotImplemented 时，同一 typed effect list 进入 fixed-function fallback。旧 `nint` overload 保留。
- layer alpha-mask 从 retained frame resource 建立 AlphaMask effect，再按原生顺序追加 AlphaScale；无 geometric mask 时使用 bounds geometry；alpha target 完成 effect fill 后以无 effect 的 source bitmap 执行 SourceUnder。geometric mask、constant alpha 与 alpha-mask 可共用 typed fill 边界。

## HRESULT 与所有权

- 非有限或越界 AlphaScale、entry 形态不匹配及未知 effect 返回 unsupported，不降级为成功。
- effect、resource、derived mask color source、brush、generator 与 pipeline 均确定性释放；真实失败停止后续 effect 与 source-under，EndLayer 外层仍负责 frame pop 和 retained layer resource 释放。

## 验证

- 新增 5 个定向测试，覆盖 effect 顺序与参数、resource retain/release、shader/fixed-function AlphaScale/AlphaMask、派生失败短路、layer AlphaMask→AlphaScale→SourceUnder。
- 主测试：3976/3976 通过。
- ABI 测试：8/8 通过。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

- 未实现任意自定义 effect、shader effect、UCE effect resource 协议或生产 COM ABI。
- 未扩展 shader bytecode、glyph 私有模型或 PresentationCore E2E。
