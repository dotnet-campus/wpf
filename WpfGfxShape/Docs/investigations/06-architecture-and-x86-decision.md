# WP-00C 架构矩阵与 x86 裁决

> 状态：静态裁决完成；真实 ARM64 运行与目标 SDK x86 Native AOT 发布证据延期  
> 日期：2026-02-26  
> 前置：`WP-00B-MANAGED-PINVOKE-ABI-INTEGRATION` 的 x64 托管 P/Invoke 主证据

## 1. 本轮结论

| 架构 | 当前状态 | 已有证据 | 尚缺证据 | 调度结论 |
|---|---|---|---|---|
| x64 | `Verified` | Debug/Release Native AOT publish、真实 `LibraryImport`、绝对路径 resolver、模块路径、SHA-256、协议/异常/并发/加载失败 | 无 WP-00C 新缺口 | 保持支持基线 |
| ARM64 | `ReadyForRealHostValidation` | 生产项目按 RID 发布；集成测试现按进程架构自动选择 `win-arm64`，并复用完整托管 P/Invoke 矩阵 | 真实 ARM64 Windows 上的 Debug/Release publish 与 8 项测试结果 | 外部执行证据延期，不由 x64 或伪造 PE Machine 代替 |
| x86 | `BlockedByPlatformEvidence` | 原 WPF 支持 Win32；托管调用约定要求与真实 WPF P/Invoke 一致；集成测试已具备 `win-x86` 进程架构映射 | 目标 .NET 10 SDK 对 Windows x86 Native AOT shared library 的实际 restore/publish/PE/运行支持 | 不宣称支持，不删除 Win32，不修改调用方；延期等待可验证平台能力 |

## 2. 测试编排变更

`Tests/WpfGfxShape.AbiIntegration/NativeAotAbiIntegrationTests.cs` 不再硬编码 `win-x64`：

- x64 测试进程选择 `win-x64`；
- ARM64 测试进程选择 `win-arm64`；
- x86 测试进程选择 `win-x86`；
- ARM32 和未知架构被明确拒绝；
- 错误架构加载测试按当前进程架构生成不同 PE Machine，不再假定宿主一定为 x64；
- 测试结果目录按 RID 隔离。

该变更只扩展测试编排，不添加生产导出、不实现真实 wpfgfx API，也不改变固定库名、绝对路径 resolver 或独立子进程模型。

## 3. 证据边界

当前工作区可以证明测试代码已经具备跨架构承载能力，但不能证明：

- ARM64 DLL 已在真实 ARM64 Windows 加载和调用；
- `win-x86` Native AOT shared library 受目标 SDK 支持；
- x86 stdcall、未装饰导出名或连续调用栈稳定性已经通过。

禁止把修改 PE Machine 的错误架构失败测试登记为 ARM64 成功证据，也禁止从 WPF 的普通 `win-x86` RID、原 C++ Win32 配置或 `global.json` 中存在 x86 runtime 工具条目推导 Native AOT x86 支持。

## 4. 延期项与后续触发条件

以下事项需要当前沙箱之外的真实平台/SDK执行能力，已记录并降低调度优先级：

1. 在真实 ARM64 Windows 以 ARM64 `dotnet test` 进程运行 `WpfGfxShape.AbiIntegration`，保存 Debug/Release publish、PE Machine、模块路径、SHA-256 和测试结果。
2. 在目标 .NET 10 SDK 提供明确 x86 Native AOT 工具链时，以真实 x86 测试进程运行同一矩阵；若 restore 或 publish 明确不受支持，保存完整诊断并维持 `BlockedByPlatformEvidence`。

这些延期项继续约束“完整替换支持 x86/ARM64”的承诺，但不再阻塞与架构无关的静态基线、Ledger 和其它可独立调查工作。

## 5. 验证说明

本轮环境禁止启动构建、发布和测试进程，因此未产生新的运行结果。代码变更已通过文件级静态复读，运行证据仍以此前 x64 成功结果为准。
