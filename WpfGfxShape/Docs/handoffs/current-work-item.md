# 当前工作切片
> 本文是当前唯一工作切片的详细说明；完成一轮后覆盖更新，不积累历史。
> 执行约束见 [`current-execution-rules.md`](current-execution-rules.md)。

## 当前方向
继续阶段 1。下一轮工作切片为 **Direct3D9Device 释放后保护与device-lost全链路闭环**，统一关闭设备部分初始化失败、已标记不可用、已释放后调用的所有生产调用链，完全对齐原生设备生命周期错误语义。

## 最近完成切片
### S-UNDATED-007-ADAPTER-FAULT-POLICY adapter故障策略模块闭环
已完成CD3DRegistryDatabase、CD3DDeviceLevel1、CD3DDeviceManager全链路的错误计数、禁用逻辑、错误分类对齐，验证通过。详细结论见 `completed-work/S-UNDATED-007-ADAPTER-FAULT-POLICY.md`。

## 唯一下一动作
完成 **device-lost与释放后保护模块闭环**。以原生 `CD3DDeviceLevel1` 所有对外方法的前置检查、设备失效后处理、部分初始化失败清理为中心，同时审计和必要时修复以下相邻链路：
1. 所有`Direct3D9Device`对外公开方法添加前置检查：设备已释放返回`ObjectDisposedException`、已标记不可用返回对应`HRESULT`错误
2. 设备部分初始化失败时按原生顺序清理已分配资源，不得泄漏COM引用
3. device-lost处理时按原生顺序先销毁GPU markers、再通知manager、最后销毁资源，清理完成后禁止所有后续底层D3D调用
4. 已释放/已失效设备的所有方法保持线程安全，无竞态条件
5. 对齐原生`CD3DDeviceLevel1`中`IsProtected`、`CheckDeviceState`等辅助方法的失效判断逻辑

本轮允许在`Direct3D9Device`、`Direct3D9ResourceManager`、`Direct3D9DeviceManager`相关文件中进行成组修改，不得扩展Reset/ResetEx、GDI presenter、UCE等禁止范围功能。

## 执行顺序
1. 枚举`CD3DDeviceLevel1`所有对外公开方法的前置检查逻辑，记录每个方法的失效返回值、检查顺序
2. 对照`Direct3D9Device`所有公开方法，建立原生到托管的检查逻辑差集
3. 一次性修复所有缺失的前置检查、清理逻辑和线程安全保护
4. 补齐设备释放后调用、部分初始化失败、device-lost后调用的单元测试
5. 运行相关device、resource-manager、render-target聚焦测试，再运行全量主测试
6. 创建一个新的 `completed-work/S-UNDATED-008-DEVICE-LIFETIME-PROTECTION.md`，统一记录该模块的原生证据、生产差集、修改、测试和未扩展边界；随后覆盖更新本文件和`current-stable-baseline.md`。

## 本轮完成条件
- 所有`Direct3D9Device`公开方法均有正确的前置检查，释放/失效后调用不会触发底层COM访问或崩溃
- 部分初始化失败场景无资源泄漏，所有已分配资源按顺序释放
- device-lost处理逻辑与原生顺序完全对齐，清理后所有后续操作返回正确错误
- 定向测试、全量主测试保持通过，并按新归档规则生成一个模块级完成文档。
