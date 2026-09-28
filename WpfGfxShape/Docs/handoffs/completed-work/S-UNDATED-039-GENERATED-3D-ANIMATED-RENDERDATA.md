# Generated 3D 与动画 render-data：状态、解析和生命周期回归

## 恢复核对

本归档合并记录此前已落地但未归档的 3D 实现与本轮修复。恢复时已有 3D command/factory/router、动态 group/mesh 和动画 render-data；不能将这些全部算作本轮新增。上一轮已补齐固定 3D 强类型状态、SpotLight/Scale/Rotate 字段和 Model3DGroup.Transform，主测试为 4110/4110。

## 原生证据

- `include/processed/wgx_core_types.h`：rotation/camera/model/light/material/Transform3D command 字段，以及后续 effects/shader 资源布局。
- `core/resources/marshal_generated.cpp`：SpotLight ProcessUpdate 的 transform、color、position animation family 解析。
- `core/resources/renderdata_generated.cpp`：rounded rectangle/ellipse animation 的 brush、pen、geometry animation family。
- `core/uce/resslave.h` 的 `AddHandleToArrayAndReplace`：零 handle 不解析、不注册；非零 handle 注册 notifier；重复资源允许多次登记。
- `include/Generated/wgx_renderdata_commands.h`：PushOpacityAnimate 的 opacity、animation 与 QWORD padding。

## 实现与修复

- 固定 3D resource 以具体 command 布局保存强类型 payload 状态，不再只保存 byte[]；其中 Type/Handle 头部未作为资源状态保存，不应将 Data 当作可直接重发的命令包。
- Model3DGroup 保留 Transform identity；group/mesh 验证完整长度和元素对齐，失败保留旧数据。
- render-data 修复零 handle 被统一拒绝的问题，保留 nullable ResourceSlots 的位置语义；Resources 仍只列实际依赖，保持现有调用兼容。
- 保存独立复制的 record Data，避免丢失坐标、半径和 opacity，避免输入 packet 后续修改影响已提交状态。Data 中的 handle 是输入快照；后续消费者应使用 ResourceSlots 中已解析且保留的资源身份，不能重新查表。
- 补齐 PushOpacityAnimate 的 16 字节数据体、24 字节 record writer、DoubleResource dependency、栈计数和解析。
- 保持先验证全部记录，再提交依赖/状态和发出通知。重复 dependency 在最终删除时逐次释放。

## 验证

- 3D 与 render-data/Visual3D 定向测试：41/41。
- 全量主测试：4120/4120。
- ABI 测试：8/8。
- Debug 解决方案增量构建：0 警告、0 错误。
- 定向测试重新编译期间存在既有 MSTEST0044 警告；增量构建无警告不代表全量重新编译无警告。
- 新回归覆盖 null slots、payload snapshot、animation family 错误回滚、重复引用释放、opacity golden bytes/通知/栈平衡、3D 跨表 duplicate 后最后 handle 删除、mesh 错位长度及批次首错停止。

## 边界与阶段评估

阶段 4 保持 Active。本轮验证的是 protocol 解析、状态和引用生命周期，不是 3D/动画 render-data 到渲染目标的完整执行或 PresentationCore E2E；没有宣称所有 3D 字段均具备独立 producer golden-byte 差分。原生读取范围仍限于 WpfGfx，未扩展到 PresentationCore 源码。

工厂仍对未实现资源返回通用 GeneratedProtocolResource，不能据 create 成功认定 update 已实现。effects/shader、DrawingImage 等更新及其消费者契约仍需继续关闭。生产 ABI、UCE composition 和替换资格不变；不能替换 wpfgfx_cor3.dll。
