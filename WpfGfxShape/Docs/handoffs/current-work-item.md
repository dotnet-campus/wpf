# 当前工作切片

> 本文是当前唯一工作切片的详细说明；完成一轮后覆盖更新，不积累历史。
> “唯一下一动作”必须是较大的、可独立验收的生产任务，默认完整关闭一个原生方法、紧密耦合的方法族、完整生命周期或端到端生产闭环；不得为实现方便拆成连续小任务。仅在存在明确且已记录的技术阻塞时允许缩小。
> 执行约束见 [`current-execution-rules.md`](current-execution-rules.md)。

## 当前方向

阶段 1 至阶段 3 已正式关闭。阶段 4 当前 Active；transport/channel 核心命令协议已闭环，下一步建立 generated resource data、resource factory 与 `ProcessUpdate` 的首个生产资源闭环。

## 最近完成切片

### Generated Protocol Transport/Channel 生产闭环

已冻结 `MILCMD`、`MIL_RESOURCE_TYPE`、32 位 handle、SDK fingerprint 和五类 transport/channel 命令布局，并实现共享生产 packet writer、exact-size reader/router、强类型 handler、最小 handle/resource table 与首错传播。新增定向测试 10/10、全量主测试 4017/4017、ABI 8/8，Debug 解决方案构建 0 警告、0 错误。详细结论见 `completed-work/S-UNDATED-031-GENERATED-PROTOCOL-TRANSPORT-CHANNEL.md`。

## 唯一下一动作

以一个大块建立 generated resource data/factory/`ProcessUpdate` 的首个端到端生产闭环。围绕原生 `data_generated.h`、`resources_generated.h`、`generated_resource_factory.*`、`marshal_generated.cpp` 与对应 PresentationCore resource update 发送者，选择紧密耦合的基础值资源族 `DoubleResource`、`ColorResource`、`PointResource`、`RectResource`、`SizeResource`、`MatrixResource`、`Point3DResource`、`Vector3DResource`、`QuaternionResource`，完整实现 resource factory 创建、handle table 注册、generated update packet 编码、router 分派、强类型 resource data 更新、malformed packet、引用与释放生命周期。不得把该闭环拆成仅声明 data struct、仅 factory switch、仅 update parser 或仅测试 golden bytes 的连续小任务。

## 下一轮完成条件

- 定位九类基础值资源在模型、签入 native/managed generated 输出、PresentationCore update 发送者、`generated_process_message.inl`、`generated_resource_factory.*`、`marshal_generated.cpp` 和具体资源实现中的完整调用链。
- 冻结九类 update command ID、command size、handle/value offset、值类型 layout、resource type ID、factory target 和 `ProcessUpdate` 签名；明确 native 与 PresentationCore 副本的差异或一致性。
- 扩展现有生产 router 与 writer，而不是建立第二套协议路径；每类 update 必须先 exact-size 校验，再验证 handle 与 exact resource type，最后提交强类型值。
- 建立共享 generated resource 基类/值资源承载面，但不得以大一统弱类型字典替代九类强类型数据和 factory 映射。
- factory 创建必须覆盖 invalid resource type、glyph-run 特例边界、handle collision、partial creation 和失败回滚；成功后 handle table 拥有资源，临时创建引用按原生顺序释放。
- update 失败不得改变旧值；unknown handle、resource type mismatch、truncated/oversized packet 返回原生对应 HRESULT，并保持首错停止。
- 删除与 duplicate 必须继续复用当前 handle table，验证 duplicate 后共享 resource identity、任一 handle update 的可观察一致性，以及最后引用删除后的确定性释放。
- 定向测试至少覆盖九类 ID/layout/golden bytes、factory dispatch、create→update→duplicate→update→delete 生命周期、invalid type、collision、unknown handle、type mismatch、malformed packet、update transactional behavior、首错和释放计数。
- 全量主测试、ABI 测试和 Debug 解决方案构建必须通过；完成后新建独立归档并把下一动作推进到 generated visual/target 或 resource dependency update 族。
