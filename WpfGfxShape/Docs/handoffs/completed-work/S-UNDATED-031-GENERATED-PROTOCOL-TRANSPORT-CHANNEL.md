# Generated Protocol Transport/Channel 生产闭环

## 完成切片

冻结当前消费的 command/resource 编号与 SDK fingerprint，并完整建立 transport/channel 核心命令族的生产 packet writer、exact-size reader/router、强类型 handler 和最小 handle/resource table 闭环。

## 原生与托管证据

- `include/Generated/wgx_command_types.h/.cs`、`include/processed/wgx_core_types.*`、`Common/Graphics/wgx_core_types.cs`：`MILCMD` 为 32 位，transport/channel 核心命令固定为 `0x01`、`0x02`、`0x07`、`0x08`、`0x09`，当前 retail 最后命令为 `0x8D`。
- `include/Generated/wgx_resource_types.h/.cs`：`MIL_RESOURCE_TYPE` 为 32 位，`TYPE_LAST = 98`，最后实际资源类型 `TYPE_D3DIMAGE = 97`。
- `include/processed/wgx_core_types.h`：`HMIL_OBJECT`、`HMIL_RESOURCE`、`HMIL_CHANNEL` 均为 `UINT32`。
- `include/Generated/wgx_commands.h`：目标命令布局依次为 4、8、12、12、16 字节，字段均按 4 字节边界连续排列。
- `core/uce/generated_process_message.inl`：命令先按 ID dispatch，并对目标命令执行 exact-size 验证后调用 special handler。
- `core/uce/composition.cpp`、`htmaster.cpp`、`htslave.cpp`、`connectioncontext.cpp`：确认 create/delete/duplicate、sync flush、destroy-resources 的生产发送者、消费者、资源 identity、引用计数与失败边界。
- `include/wgx_sdk_version.*`、`Common/Graphics/wgx_sdk_version.cs`：MIL SDK fingerprint 为 `0x200184C0`，DWM SDK fingerprint 为 `0x0BDDCB2B`，重复副本一致。

## 托管实现

- 新增完整冻结的 `MilCommand` 与 `MilResourceType` 32 位枚举，以及 SDK/协议边界常量。
- 新增五个 `[StructLayout(LayoutKind.Explicit, Pack = 1)]` 强类型命令结构，固定 size 与 field offset。
- 新增共享生产 `GeneratedProtocolPacketWriter` 与 `GeneratedProtocolRouter`；writer 输出原生 little-endian golden bytes，router 执行 ID 读取、exact-size 校验、强类型解析、未知命令映射与首错停止。
- 新增 `GeneratedProtocolHandleTable`、channel registry 与 production context，覆盖 create/delete/duplicate 的资源 identity 和引用计数。
- 新增 UCE unknown packet、malformed packet 与 handle lookup failed HRESULT 常量。

## 错误、布局与所有权

- truncated/oversized packet 返回 `WGXERR_UCE_MALFORMEDPACKET`，未知命令返回 `WGXERR_UCE_UNKNOWNPACKET`。
- channel lookup 失败返回 `WGXERR_UCE_HANDLELOOKUPFAILED`；无效 handle、碰撞、类型不匹配和重复目标 handle 返回 malformed packet。
- 多包处理在第一个失败 HRESULT 处停止，不调用后续 handler。
- duplicate 保持同一 resource identity 并增加引用；删除源 handle 后目标 handle 仍拥有资源。
- 固定 `UINT32` handle 未替换为 `nint`，未运行生成器覆盖签入输出。

## 验证

- 新增定向测试 10/10：fingerprint/ID、SizeOf/OffsetOf、golden bytes、五类路由、truncated/oversized/unknown、handler 首错、多包停止、create/delete/duplicate、identity/refcount、碰撞、类型不匹配和 channel lookup。
- 全量主测试：4017/4017。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

- 未实现完整 connection/channel/composition 生命周期。
- 未实现 generated resource data、resource factory、`ProcessUpdate`、marshal 或 render-data parser。
- 未运行 MilCodeGen，也未把重新生成结果当作权威基线。
- 未扩展生产 ABI 或 PresentationCore E2E。
