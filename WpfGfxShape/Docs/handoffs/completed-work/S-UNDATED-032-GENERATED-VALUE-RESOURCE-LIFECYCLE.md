# Generated 基础值资源生产生命周期

## 完成切片

完整实现九类基础值资源 `DoubleResource`、`ColorResource`、`PointResource`、`RectResource`、`SizeResource`、`MatrixResource`、`Point3DResource`、`Vector3DResource`、`QuaternionResource` 的 generated resource data、factory、update packet、router、`ProcessUpdate`、duplicate/update/delete 生命周期。

## 原生证据

- `include/Generated/wgx_commands.h` 与 `wgx_commands.cs`：九类命令均为 Pack=1 的 `Type + Handle + Value`，值从偏移 8 开始。
- `include/Generated/wgx_command_types.*` 与 processed core types：命令 ID 为 `0x0E` 至 `0x16`，资源类型 ID 为 `49` 至 `57`。
- `core/uce/generated_process_message.inl`：每类命令按精确资源类型查找 handle，然后调用对应资源的 `ProcessUpdate`。
- `core/uce/generated_resource_factory.cpp`：九类资源分别创建 `CMilSlaveDouble`、`CMilSlaveColor`、`CMilSlavePoint`、`CMilSlaveRect`、`CMilSlaveSize`、`CMilSlaveMatrix`、`CMilSlavePoint3D`、`CMilSlaveVector3D`、`CMilSlaveQuaternion`。
- `core/resources/valueres.h`：模板化 `ProcessUpdate` 在校验完成后复制强类型值并发送 changed notification。
- PresentationCore `*IndependentAnimationStorage.cs`：生产发送者写入相同 command ID、handle 和值布局。

## 托管实现

- 新增九类显式命令结构和值类型镜像，冻结 command size、handle/value offset 和固定宽度字段。
- 扩展现有 `GeneratedProtocolPacketWriter` 与 `GeneratedProtocolRouter`，未建立第二套协议路径。
- router 对每类 packet 先执行 exact-size 校验，再将无分配的 value span 交给生产 handler。
- 新增强类型 `GeneratedValueResource<T>`，保存值与 change count；失败 update 不提交新值。
- 新增 `GeneratedResourceFactory`，将九类 resource type 映射到精确强类型资源，并保留 glyph-run 与其余当前资源的通用创建边界。
- 扩展现有 handle table 完成 create、精确类型 update、duplicate identity、delete 与引用计数归零。

## HRESULT、所有权与失败语义

- zero handle、invalid type、handle collision、unknown handle、resource type mismatch、truncated/oversized packet 均返回 malformed packet HRESULT。
- current/target channel 不存在继续返回 handle lookup failed HRESULT。
- batch 在首个失败处停止，后续 update 不执行。
- duplicate 后两个 handle 共享同一资源 identity；任一 handle 的成功 update 对另一 handle 可见。
- 每个 handle 持有一个引用，最后一个 handle 删除后引用计数确定性归零。
- update 在完成所有尺寸、handle 和精确类型校验前不改变旧值或 change count。

## 验证

- generated protocol/value resource 定向测试：18/18。
- 全量主测试：4025/4025。
- ABI 集成测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未扩展 visual/target、resource dependency、render-data、effects、UCE connection/composition、生产 ABI 或 PresentationCore E2E；这些能力继续作为后续独立生产切片推进。
