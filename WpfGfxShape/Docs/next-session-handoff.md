# 下一轮交接：建立发布后托管 P/Invoke ABI 集成测试

> 本文件是下一轮唯一权威入口  
> 已完成工作包：`WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`  
> 已作废证据模型：`WP-00B` 以 C++ NativeCaller 作为 ABI 主证据的方案  
> 下一唯一工作包：`WP-00B-MANAGED-PINVOKE-ABI-INTEGRATION`

## 1. 已接受的测试架构决策

[`decisions/DEC-0003-managed-pinvoke-abi-integration-tests.md`](decisions/DEC-0003-managed-pinvoke-abi-integration-tests.md) 已接受：项目真实消费者是 WPF 托管 P/Invoke，因此 Native AOT ABI 的主证据必须来自发布后托管 P/Invoke 集成测试，而不是独立 C++ caller。

现有 MSTest 直接调用：

```text
NativeAotAbiProbe.Invoke(...)
```

只验证内部实现，绕过：

```text
AOT publish → DLL 加载 → 导出解析 → UnmanagedCallersOnly → P/Invoke
```

因此这些测试只能作为快速单元测试，不能作为最终 ABI 验证。

## 2. 目标测试链

```text
MSTest 编排
  → 发布指定 Configuration/RID 的 Native AOT DLL
  → 启动独立托管测试进程
  → 注册 NativeLibrary.SetDllImportResolver
  → 使用发布 DLL 绝对路径解析固定 P/Invoke 库名
  → 通过真实 DllImport/LibraryImport 调用导出
  → 验证 ABI 行为、实际模块路径和 SHA-256
```

不得依赖 `[DllImport("wpfgfx_cor3.dll")]` 的默认 DLL 搜索路径。resolver 应采用等价形态：

```csharp
NativeLibrary.SetDllImportResolver(
    typeof(NativeAotAbiProbeImports).Assembly,
    (_, _, _) => NativeLibrary.Load(publishedDllAbsolutePath));
```

实际实现必须处理 resolver 只能为程序集设置一次、测试并行和独立进程隔离问题。

## 3. 当前代码状态

保留：

- `Code/WpfGfxShape/WpfGfxShape.csproj`：.NET 10 Native AOT shared library；
- `Code/WpfGfxShape/Abi/NativeAotAbiProbe.cs`：唯一非生产导出 `WpfGfxShape_NativeAotAbiProbe_v1`；
- `Tests/WpfGfxShape.Tests`：内部纯逻辑 MSTest；
- 局部构建隔离和 `artifacts` 输出根。

删除：

- `Tests/NativeCaller` 全目录及其中 C++、`.vcxproj`、头文件和 MSBuild wrapper。

不再要求：

- Native AOT `.lib` 能否被 C/C++ 链接；
- 纯原生消费者是否可调用；
- C++ 动态或静态 caller；
- 以 C++ caller 验证 x86 stdcall/ESP、名称修饰、loader 或 crash。

过去 C++ caller 的运行结果只保留为历史调查信息，不再计入当前完成门禁。

## 4. 下一工作包实施内容

1. 在 `Tests` 下建立独立托管 ABI 集成测试宿主/子进程承载，建议边界为 `Tests/WpfGfxShape.AbiIntegration`；具体项目数保持最小，但不得在 MSTest 进程内执行可能污染进程状态的场景。
2. 建立可重复的 Native AOT publish 编排：
   - 每次只发布一个 Configuration/RID；
   - 从项目级执行 RID restore/publish；
   - 输出固定到 `artifacts/publish/WpfGfxShape/<Configuration>/<RID>/`。
3. 为 probe 声明真实 `[DllImport]` 或 `[LibraryImport]`，库名保持固定，EntryPoint 精确为 `WpfGfxShape_NativeAotAbiProbe_v1`。
4. 使用 `NativeLibrary.SetDllImportResolver` 将固定库名解析到当前 publish DLL 的绝对路径。
5. 加载后校验：
   - 期望 DLL 绝对路径；
   - 实际加载模块路径；
   - 发布前后/加载文件 SHA-256；
   - PE machine 和导出表可由托管二进制检查器补充验证。
6. 通过真实 P/Invoke 覆盖：
   - checksum 成功；
   - ABI version 错误；
   - null output；
   - 受控参数错误；
   - 受控内部异常及 HRESULT 映射；
   - 未知 operation；
   - 失败前 output 清零；
   - 异常后再次调用成功；
   - 并发首次调用与重复调用；
   - 缺失 DLL、缺失导出、错误架构和加载失败。
7. crash、fail-fast、错误架构、加载失败及 unload/reload 等可能污染状态的 case 必须一 case 一独立托管子进程；父 MSTest 只判断退出码、超时和结果文件。
8. 更新 `06-testing-strategy.md`、`09-test-evidence-and-e2e-contract.md` 和本交接中的执行证据；不得把内部 `Invoke` 测试记为 ABI 通过。

## 5. 当前完成状态

- [x] Native AOT 生产项目和唯一 probe 存在；
- [x] 内部纯逻辑 MSTest 存在；
- [x] Debug/Release x64 Native AOT publish 曾成功；
- [x] 已确认真实 DLL named exports 包含工具链导出 `DotNetRuntimeDebugHeader` 和项目 probe；
- [x] 已接受 DEC-0003，C++ NativeCaller 不再是目标架构；
- [ ] `Tests/NativeCaller` 已删除；当前工作区仍存在该目录，必须先删除；
- [ ] 发布后托管 P/Invoke ABI 集成测试项目存在；
- [ ] resolver 使用绝对 publish DLL 路径；
- [ ] 实际加载模块路径和 SHA-256 已验证；
- [ ] Debug/Release x64 真实 P/Invoke 全协议测试通过；
- [ ] 并发和失败场景在隔离托管子进程通过；
- [ ] 当前 `WP-00B-MANAGED-PINVOKE-ABI-INTEGRATION` 证据写回文档。

## 6. 范围限制

- 不添加任何生产 wpfgfx 导出；
- 不实现 `MilVersionCheck`、`MILCreateFactory` 或其它真实 API；
- 不翻译原 C/C++ 文件；
- 不引用 Silk.NET；
- 不进入 ARM64/x86 裁决，必须先完成新的 x64 托管 P/Invoke 主证据；
- 不恢复 `Tests/NativeCaller`；
- 不以默认 DLL 搜索路径或复制到测试输出目录的偶然命中代替 resolver 和路径校验。

## 7. 后续顺序

完成 `WP-00B-MANAGED-PINVOKE-ABI-INTEGRATION` 后，才可重新领取 `WP-00C-ARCHITECTURE-AND-X86-DECISION`，并将同一托管 P/Invoke 集成测试模型扩展到 ARM64/x86 支持调查。
