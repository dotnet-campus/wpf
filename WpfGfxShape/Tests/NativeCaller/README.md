# NativeCaller

本目录承载 `WP-00B-NATIVEAOT-ABI-PROBE-X64` 的 Native AOT x64 ABI 验证，只允许调用非生产导出 `WpfGfxShape_NativeAotAbiProbe_v1`。

## 项目与入口

- `NativeAotPublish.proj`：先按 configuration/RID restore，再 publish 生产项目。
- `NativeCaller.vcxproj`：动态加载 caller。
- `NativeCallerRun.proj`：以 publish DLL 的绝对路径运行动态 caller。
- `StaticNativeCaller.vcxproj`：使用 Native AOT 生成的 `wpfgfx_cor3.lib` 静态链接调用。
- `StaticNativeCallerRun.proj`：在对应 native 产物目录运行静态 caller。
- `NativeAotAbiProbe.h`：原生 ABI 原型。

所有入口当前只支持 `win-x64` 的 Debug/Release 验证。

## 动态 caller 验证

动态 caller 在加载 DLL 前直接解析 PE：

- `Machine == IMAGE_FILE_MACHINE_AMD64`；
- optional header 为 PE32+；
- 完整枚举 named exports；
- 精确 probe 导出只出现一次；
- 代表性生产导出 `MilVersionCheck`、`MILCreateFactory` 不存在。

当前 Debug 和 Release 的实际 named exports 均为：

- `DotNetRuntimeDebugHeader`
- `WpfGfxShape_NativeAotAbiProbe_v1`

`DotNetRuntimeDebugHeader` 是 Native AOT 工具链生成的运行时导出，因此当前 DLL并非只有一个总导出；受控 probe 是唯一项目定义的 ABI 导出。

随后 caller 使用绝对路径和受控 `LoadLibraryExW` flags，验证：

- 回读模块路径与请求 DLL 一致；
- 精确导出名可解析，错误大小写、错误版本和 x86 装饰名不可解析；
- checksum、错误 ABI version、空 output、参数错误、受控异常、未知 operation；
- 失败 output 清零；
- 异常路径后再次成功；
- 16 线程、每线程 100 次调用；
- `FreeLibrary` 后重新加载和再次调用。

这里的 reload 结果只证明当前受控 probe 场景，不裁决 Native AOT 完整 unload/reload 语义。

## 静态 import caller 验证

静态 caller 使用对应 configuration 的：

`artifacts/bin/WpfGfxShape/<Configuration>/net10.0/win-x64/native/wpfgfx_cor3.lib`

验证原型可直接链接，并覆盖 checksum 成功和受控异常映射。Debug/Release 均已成功构建和运行。

## 产物位置

- publish DLL：`artifacts/publish/WpfGfxShape/<Configuration>/win-x64/wpfgfx_cor3.dll`
- Native AOT `.lib/.exp`：`artifacts/bin/WpfGfxShape/<Configuration>/net10.0/win-x64/native/`
- caller：`artifacts/native-caller/<Configuration>/x64/`

本目录不包含任何生产 wpfgfx ABI，不得作为可部署替代品使用。
