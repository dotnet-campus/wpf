# DEC-0002：Silk.NET 作为首选低层来源，按 API 家族裁决采用形态

> 状态：Accepted  
> 本地基线：`D:/lindexi/Code/OriginWpf/Silk.NET` @ `ed4b3c299a44e8b6936f6b1c61c0b119e82237cd`

## 背景

用户计划用 Silk.NET 调用 DirectX，但本地快照并非覆盖 wpfgfx 全部平台面，整体引用还可能改变 loader、依赖和 ABI。

## 已确认事实

- 本地项目提供 D3D9/D3D9Ex、DXGI、DXVA、D3DCompiler。
- 未发现 DirectWrite、WIC、Direct2D、Media Foundation、EVR、DirectShow、WMP 项目。
- D3D9 与 Core `IUnknown` vtable 生成代码使用大量 `delegate* unmanaged[Cdecl]`；原 Win32 COM 是 x86 stdcall 高风险边界。
- bindings 自动引用 `Silk.NET.Core` 和 SilkTouch；Core 还带旧包依赖。
- 默认 `GetApi()` 走 `LoadLibrary/GetProcAddress/FreeLibrary`，不能替代原 wpfgfx loader 的锁、搜索路径、引用和失败语义。
- Silk.NET 使用 MIT 许可证；冻结源码时必须保留许可证和来源。

## 决策

Silk.NET 是 DirectX 低层类型、常量和函数声明的首选来源，但**不强制整体 ProjectReference，也不允许直接用高层 API 重写原算法**。

`WP-00G` 对每个 API 家族从以下三种方式中形成唯一 Decision：

1. 最小本地 ProjectReference；
2. 冻结经许可、经审计的必要生成源码；
3. 对缺失或错误绑定局部手写最低层互操作。

可以按 API 家族混合，但同一 COM 接口在生产代码中只能有一份权威定义。无论采用哪种方式，原 loader、COM 生命周期、调用约定、HRESULT、结构布局和释放顺序都必须由逐文件实现保留。

## 被拒绝替代方案

- 整体引用 Silk.NET 并用 `GetApi()` 替代全部 loader；
- 因 Silk.NET 有 DirectX 项目而迁到 D3D11/12；
- 把 DXVA 覆盖视为媒体栈覆盖；
- 无测试直接修正生成 vtable；
- 因绑定缺失而引入高层 WPF/媒体封装。

## 影响

- `WP-00A` 不引用 Silk.NET。
- `WP-00G` 前 HW D3D9 互操作保持 Blocked。
- DWrite、WIC、D2D、媒体和普通 Win32/COM 各自进入平台互操作 Ledger。
- 冻结源码必须记录原路径、commit、许可证和本地修改差分。

## 重新评估触发条件

- 切换 Silk.NET commit；
- 目标 .NET/Native AOT 或 x86 支持变化；
- 找到此前未发现的 bindings；
- 代表 API 的调用约定、布局或 AOT spike 结果变化。