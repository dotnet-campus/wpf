# CD3DRegistryDatabase::IsAdapterEnabled adapter 可用性门控

## 原生证据

- 声明位于 `core/hw/d3dregistry.h`，实现位于 `core/hw/d3dregistry.cpp`，非禁止生产调用者位于 `core/hw/d3ddevicemanager.cpp` 的 HAL 设备创建路径。
- `IsAdapterEnabled` 入口先把输出固定清为 `false`，要求 registry 数据库已初始化；adapter ordinal 越界时返回 `E_INVALIDARG` 并保持输出为 `false`。
- 有效 adapter 仅按缓存错误计数是否小于固定阈值 5 决定启用状态；达到或超过阈值均禁用，不读取 D3D 设备、不调用 COM，也不改变计数。
- device manager 只对 HAL 设备执行该查询，并且先检查 adapter ordinal 小于 `IDirect3D9::GetAdapterCount()`；禁用时返回 `WGXERR_NO_HARDWARE_DEVICE`，不继续组合创建设备参数或调用底层设备工厂。software device 不进入该门控。

## 托管差集与修改

- `Direct3D9RegistryDatabase.IsAdapterEnabled` 已在锁内验证 adapter ordinal，并以 `_adapterErrorCounts[adapterOrdinal] < 5` 返回缓存状态；越界以 `ArgumentOutOfRangeException` 拒绝，不修改任何 adapter 状态。
- `Direct3D9Objects.CreateDeviceManager` 已把同一 registry database 的 `IsAdapterEnabled` 委托注入 `Direct3D9DeviceManager`。
- `Direct3D9DeviceManager.GetDeviceAndPresentParameters` 已按原生顺序仅对 HAL 路径先检查 adapter count，再查询 registry enabled 状态；禁用时映射为 `NoHardwareDeviceHResult`，且发生在 capability、display mode、present parameters 与设备创建之前。software 路径只注册软件光栅器，不查询 registry enabled 状态。
- 现有生产实现和测试已形成闭环，本轮无需修改生产代码或测试。

## ABI、HRESULT 与所有权

- 原生 helper 不获取或释放 COM 引用，不进入 device/use context，也不创建资源；托管实现同样只读取进程内缓存。
- 原生越界返回 `E_INVALIDARG` 并保持输出为 `false`；托管内部 API 以精确参数异常表达同一不可用输入，生产调用者在查询前已有 adapter-count 门控。
- registry 禁用只阻止 HAL 设备创建，不污染 software device 路径，不扩展 WOW64 registry view、Reset/ResetEx、普通 scene、render-target 或 present 生命周期。

## 验证

- `Direct3D9RegistryDatabaseTests`：11/11 通过。
- 全量主测试：3862/3862 通过。
- ABI 集成测试：8/8 通过。
- Debug `WpfGfxShape.slnx` 构建：0 警告、0 错误。