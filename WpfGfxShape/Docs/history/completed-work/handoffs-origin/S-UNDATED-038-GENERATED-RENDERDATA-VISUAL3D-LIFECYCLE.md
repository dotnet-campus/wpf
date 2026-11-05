# Generated RenderData 与 Visual3D 生命周期

## 完成切片

完整建立 generated render-data 核心指令流与 Visual3D/Viewport3DVisual 状态、2D/3D parent identity、依赖通知、事务式树修改及确定性释放闭环。

## 原生证据

- `include/Generated/wgx_renderdata_commands.h`、PresentationCore `Generated/RenderData.cs` 与 `RenderDataDrawingContext.cs`：冻结代表性 `Draw*`、`Push*`、`Pop` 数据体布局、8 字节 record header、QWORD 对齐和发送尺寸。
- `core/resources/renderdata.cpp` 与 `renderdata_generated.cpp`：确认 `MILCMD_RENDERDATA` 的 `cbData` 边界、instruction stream 扫描、精确 dependency family、push/pop 栈平衡及失败后销毁语义。
- `include/Generated/wgx_commands.h`、`exports.cs` 与 `generated_process_message.inl`：冻结 Viewport3DVisual/Visual3D command ID、Pack=1 size/offset、发送者和 dispatch。
- `core/resources/visual3d.cpp`、`Visual3D.h`：确认 Model3D/Transform3D dependency、3D parent/children identity、索引规则、通知和析构释放。
- `core/resources/viewport3dvisual.cpp`、`Viewport3DVisual.h`：确认 camera、viewport、单一 3D child、2D/3D parent link 和替换释放顺序。

## 托管实现

- 新增 `GeneratedRenderDataVisual3DResources.cs`，包含 render-data command、record writer、可判别 instruction 模型和 Visual3D/Viewport3DVisual 强类型状态。
- render-data 支持代表性 DrawLine、DrawRectangle、DrawGeometry、DrawImage、DrawGlyphRun、DrawDrawing、DrawVideo、PushClip、PushOpacityMask、PushOpacity、PushTransform、PushGuidelineSet、PushGuidelineY1/Y2 与 Pop。
- instruction parser 验证 record header、QWORD 对齐、完整 payload 消费、固定数据体尺寸、精确 resource family、非空 handle 和 push/pop 栈平衡。
- render-data 完整验证后一次性提交 instruction 与 dependency 图；dependency changed notification 传播到 RenderData。
- Visual3D 使用强类型 content、transform、parent 和 children；insert/remove/clear 维护索引、单一 parent、自环/祖先环和跨 parent 规则。
- Viewport3DVisual 使用强类型 camera、viewport 和单一 3D child；2D viewport 与 3D child 共享同一 parent identity 约束。
- factory、router、handle table 和 production context 已接通 RenderData variable update 与八类 3D visual state command。

## HRESULT、所有权与失败语义

- record 尺寸、对齐、payload、unknown instruction、dependency family、null required handle、栈下溢/未闭合和树规则错误返回 malformed packet HRESULT。
- dependency、property 和 tree 更新均先完成验证再提交；失败保持旧 instruction、状态、引用和 change count。
- dependency changed notification 通过 listener 图传播；Visual3D child 与 Viewport3D child 以引用保持 identity。
- RenderData、Visual3D、Viewport3DVisual 删除最后 handle 时确定性解除 dependency、children 和跨维度 parent link。
- packet batch 保持首错停止。

## 验证

- generated render-data/Visual3D 定向测试：10/10。
- 全量主测试：4089/4089。
- ABI 集成测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未扩展 animated/rounded/ellipse/effect render-data 全族、3D generated resource update family、effects、UCE connection/composition、生产 ABI 或 PresentationCore E2E。阶段 4 仍需完成 3D generated resource family 与剩余 render-data 指令后再评估关闭条件。