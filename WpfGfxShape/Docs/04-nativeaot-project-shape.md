# 当前 .NET 10 Native AOT 项目形态

> 本文记录当前项目结构和稳定项目边界，不维护当前工作切片、完成比例或测试流水。
> 当前恢复入口见 [`next-session-handoff.md`](next-session-handoff.md)，最近构建和测试结果见 [`handoffs/current-stable-baseline.md`](handoffs/current-stable-baseline.md)。

## 1. 当前解决方案结构

`WpfGfxShape.slnx` 当前包含四个项目：

```text
WpfGfxShape/
  Directory.Build.props
  Directory.Build.targets
  WpfGfxShape.slnx
  Code/
    WpfGfxShape/
      WpfGfxShape.csproj
      Abi/
      Core/
  Tests/
    WpfGfxShape.Tests/
      WpfGfxShape.Tests.csproj
    WpfGfxShape.AbiIntegration.Host/
      WpfGfxShape.AbiIntegration.Host.csproj
    WpfGfxShape.AbiIntegration/
      WpfGfxShape.AbiIntegration.csproj
  artifacts/
```

职责分别为：

- `Code/WpfGfxShape/WpfGfxShape.csproj`：唯一生产项目，目标为 .NET 10 Native AOT shared library；
- `Tests/WpfGfxShape.Tests`：MSTest 主测试项目；
- `Tests/WpfGfxShape.AbiIntegration.Host`：发布后 ABI 独立托管进程宿主；
- `Tests/WpfGfxShape.AbiIntegration`：ABI 集成测试编排项目。

不创建或恢复 `Tests/NativeCaller`，不要求 C++ 动态/静态 caller 或 `.lib` 消费者验证。

## 2. 稳定项目边界

- 原 WPF/wpfgfx 源码和项目保持只读；
- 新项目不引用、不复制、不递归构建原 WPF 产品项目；
- 生产逻辑只位于一个 C# 项目中，但不得因此扁平化原生职责；
- 生产代码继续保持 loader、device/resource、HW、software、ABI 等可追溯边界；
- 测试和 ABI Host 是独立承载，不计入生产项目数量；
- 输出、中间文件和测试结果位于 `WpfGfxShape/artifacts`，不写入原 WPF artifacts；
- 局部 `Directory.Build.props` 和 `Directory.Build.targets` 隔离仓库根 WPF Arcade/Testing 构建注入。

## 3. 生产项目要求

生产项目保持以下能力和约束：

- 目标框架为 `net10.0`；
- 使用 `Microsoft.NET.Sdk`；
- 支持 unsafe 低层互操作；
- 以 Native AOT shared library 生成候选 `wpfgfx_cor3.dll`；
- Native AOT 导出仅作为薄 ABI 边界；
- 托管异常不得越过导出边界；
- 不通过 warning suppression、假导出或批量 `E_NOTIMPL` 隐藏未迁移能力；
- 不修改 TFM、SDK 或语言版本来绕过实现问题。

具体项目属性以当前 `.csproj`、局部 `Directory.Build.*` 和实际 publish 结果为事实源，不在本文复制易漂移的完整属性表。

## 4. DirectX 和 Win32 依赖

- DirectX 绑定固定为 Silk.NET 2.23.0；
- Silk.NET ABI 或类型存疑时读取对应版本源码；
- Windows API 优先使用 Microsoft.Windows.CsWin32 0.3.298；
- COM vtable 调用保持显式 Windows `Stdcall`；
- 未覆盖或语义不透明的 API 使用最小低层互操作，不引入高层替代架构。

版本事实以 `handoffs/current-stable-baseline.md` 和项目包引用为准。

## 5. ABI 测试承载

当前 ABI Host/集成测试用于验证非生产 probe 和 Native AOT 加载机制，包括：

- 独立进程加载；
- 绝对路径 DLL 解析；
- 真实 P/Invoke；
- 缺失 DLL、缺失导出、无效 PE 和错误架构；
- 模块路径和文件身份；
- HRESULT、out 参数、异常封锁和失败后恢复。

该承载不能替代完整生产 ABI。生产导出、COM/factory、资源、版本和 PresentationCore E2E 仍按 [`remaining-gap-closure-plan.md`](remaining-gap-closure-plan.md) 推进。

## 6. 架构边界

- x64 是当前主要构建和测试环境；
- ARM64 必须在真实 ARM64 Windows 环境验证；
- x86 Native AOT shared-library 支持必须有目标 SDK 和运行证据，不能由普通 RID 或 x64 结果推断；
- 任一架构通过不能替代其它架构；
- 平台能力不足时保持明确阻塞并请求范围裁决，不自行删除兼容目标。

## 7. 当前完成与未完成

已经形成稳定承载：

- 独立解决方案和四项目结构；
- 局部构建隔离；
- 生产项目、主测试和 ABI Host/集成测试；
- Native AOT probe 机制；
- 大量 D3D9/HW 和部分 software 生产实现。

仍未完成：

- 完整生产 ABI 和导出；
- 完整 COM/factory；
- generated protocol；
- resources/UCE；
- 完整内容管线；
- PresentationCore E2E 与最终替换验收。

当前不能替换原 `wpfgfx_cor3.dll`。

## 8. 维护规则

- 项目结构发生实质变化时更新本文；
- 包版本变化更新稳定基线和项目文件，不在多个规划文档复制版本状态；
- 最近测试数字只写入 `handoffs/current-stable-baseline.md`；
- 当前下一动作只写入 `handoffs/current-work-item.md`；
- 每个完成切片在 `handoffs/completed-work/` 创建独立历史文件。
