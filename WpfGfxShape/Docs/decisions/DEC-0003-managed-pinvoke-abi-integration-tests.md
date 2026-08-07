# DEC-0003：发布后 ABI 验证以托管 P/Invoke 集成测试为主

- 状态：Accepted
- 日期：2026-06-23
- 适用范围：Native AOT shared library 的 ABI probe、后续生产导出及测试承载

## 背景

项目的真实消费者是 WPF 托管代码中的 P/Invoke，而不是独立 C/C++ 应用或通过 import library 静态链接的原生消费者。

现有 MSTest 直接调用 `NativeAotAbiProbe.Invoke(...)`，只验证内部实现，绕过了真实发布和调用链：

```text
AOT publish → DLL 加载 → 导出解析 → UnmanagedCallersOnly → P/Invoke
```

因此，内部方法单元测试不能作为最终 ABI 证据。此前建立的 `Tests/NativeCaller` 动态/静态 C++ caller 也偏离了真实消费者模型，并把项目没有实际需求的 `.lib` 链接、纯原生调用和 x86 原生栈探针引入主门禁。当前设计的真正缺口不是缺少 C++ caller，而是缺少以实际托管调用方为主体的发布后 ABI 集成测试。

## 决策

1. Native AOT ABI 的主证据改为发布后托管 ABI 集成测试。
2. 测试必须先发布指定 Configuration/RID 的生产项目，再在独立测试进程中加载发布目录中的实际 DLL。
3. 测试通过 `[DllImport]`、`[LibraryImport]` 或 unmanaged function pointer 进入真实 `[UnmanagedCallersOnly]` 导出；优先使用与 WPF 实际调用方一致的 P/Invoke 形态。
4. 不依赖 `[DllImport("wpfgfx_cor3.dll")]` 的默认搜索路径。测试注册 `NativeLibrary.SetDllImportResolver`，由 resolver 使用发布 DLL 的绝对路径加载目标模块。
5. 测试必须记录并校验预期 DLL 的绝对路径、实际加载模块路径和 SHA-256，防止误加载同名文件。
6. HRESULT、参数宽度、out 参数、异常封锁、并发、缺失 DLL、缺失导出、加载失败和进程崩溃等均由托管集成测试及其隔离子进程覆盖。
7. 原有内部普通方法测试保留为快速 T1 单元测试，但不能替代发布后 ABI 集成测试。
8. `Tests/NativeCaller` 必须删除，不再要求：
   - Native AOT `.lib` 的 C/C++ 静态链接；项目没有此需求；
   - 纯原生消费者调用；项目没有此需求；
   - 独立 C++ 动态或静态 caller；
   - 以原生 caller 验证 x86 stdcall/ESP、名称修饰、loader、crash 或低层 ABI。
9. 上述 ABI、加载和进程失败场景由托管自动化测试承担；可能污染测试进程的场景必须放入独立托管子进程。
10. PE machine、导出表、资源和版本等静态二进制事实仍可由托管专用检查器验证，但不需要 C++ caller。

## 主测试链

```text
MSTest 编排
  → 发布 Native AOT DLL
  → 启动隔离的托管测试进程
  → 注册 DllImport resolver 并绝对路径加载 DLL
  → 通过真实 DllImport/LibraryImport 调用导出
  → 验证 ABI 行为、路径、哈希、并发与失败结果
```

## 结果

- `Tests/WpfGfxShape.Tests` 继续承载纯逻辑单元测试。
- 新增独立的托管 ABI 集成测试宿主/子进程承载，名称和项目形态由下一实现工作包按现有 solution 约定确定。
- 过去由 C++ caller 得到的结果仅保留为历史调查证据，不再是当前完成门禁，也不能替代托管 P/Invoke 集成测试。
- 后续文档若与本决策冲突，以本决策为准，并应及时改写冲突内容。
