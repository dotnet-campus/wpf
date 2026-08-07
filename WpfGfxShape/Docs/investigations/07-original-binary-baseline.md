# WP-00H 原 wpfgfx 二进制基线

> 状态：`PartialBaselineFrozen`  
> 工作包：`WP-00H-ORIGINAL-BINARY-BASELINE`  
> 范围：只记录工作区中已存在原工件的可验证事实；不迁移生产代码，不把字符串扫描当作 PE 枚举  
> 采集限制：当前受限环境只能列举文件、读取文本和搜索可提取字符串，不能读取原始二进制字节、文件系统元数据或启动二进制检查工具

## 1. 已定位原工件

当前工作区只定位到一个名为 `wpfgfx_cor3.dll` 的原工件：

`D:\lindexi\Code\dotnetcampus\WpfLab\WpfRuntime\origin\wpf\.dotnet\shared\Microsoft.WindowsDesktop.App\9.0.5\wpfgfx_cor3.dll`

其伴随清单为：

`D:\lindexi\Code\dotnetcampus\WpfLab\WpfRuntime\origin\wpf\.dotnet\shared\Microsoft.WindowsDesktop.App\9.0.5\Microsoft.WindowsDesktop.App.deps.json`

该清单直接给出：

- runtime target：`.NETCoreApp,Version=v9.0/win-x64`；
- package identity：`Microsoft.WindowsDesktop.App.Runtime.win-x64/9.0.5`；
- native asset：`wpfgfx_cor3.dll`；
- file version：`9.0.525.21602`。

因此本工件可冻结为 **shipping-style .NET 9.0.5 WindowsDesktop win-x64 Release 原二进制基线候选**。`Release` 还得到 DLL 内可提取 CodeView 路径字符串的支持：

`D:\a\_work\1\s\artifacts\bin\wpfgfx\x64\Release\wpfgfx_cor3.pdb`

该路径只证明 DLL 记录了对应 PDB 身份路径线索；本地工作区没有找到 `wpfgfx_cor3.pdb`，不能据此声称符号已冻结。

## 2. 当前工件矩阵

| 配置 | RID/架构 | 工件 | 状态 |
|---|---|---|---|
| Release | win-x64 | .NET 9.0.5 shared framework `wpfgfx_cor3.dll` | `LocatedAndSourceAttributed` |
| Debug | win-x64 | 未找到 | `ExternalBaselineGap-LowPriority` |
| Debug/Release | win-x86 | 未找到 `wpfgfx_cor3.dll`；现有 `runtime.win-x86.microsoft.dotnet.wpf.dnceng/9.0.0-rtm.25168.3` 只有 `PresentationNative_cor3.dll/.pdb` 与 `bilinearspan.lib` | `ExternalBaselineGap-LowPriority` |
| Debug/Release | win-arm64 | 未找到 | `ExternalBaselineGap-LowPriority` |

工作区没有找到对应 `wpfgfx_cor3.pdb`、wpfgfx import `.lib`、`.exp` 或独立 manifest。缺失项不阻塞对现有 Release x64 工件的冻结，也不得由其它组件的 PDB/LIB 替代。

## 3. 已冻结与未检查字段

| 字段 | Release win-x64 当前值 |
|---|---|
| role | `Original` |
| artifactId | `ORIGINAL-WPFGFX-NET9.0.5-RELEASE-WIN-X64` |
| absolute path | 已记录于第 1 节 |
| source/package identity | `Microsoft.WindowsDesktop.App.Runtime.win-x64/9.0.5` |
| configuration | `Release`，由 CodeView 路径线索支持 |
| RID / PE machine | RID 为 `win-x64`；PE Machine 尚未直接读取 |
| file version | `9.0.525.21602`，来自 `.deps.json` |
| size / filesystem timestamp | `NotInspected` |
| SHA-256 | `NotInspected` |
| product version / OriginalFilename / Debug flag | `NotInspected` |
| imports / delay imports / dependencies | `NotInspected`；项目文件只能作为预期来源，不能代替二进制事实 |
| exports / ordinals / RVA / kind / forwarders | `NotInspected` |
| resources / version resource / hashes | `NotInspected` |
| PDB identity | CodeView 路径字符串已定位；GUID/age/signature 与 PDB 文件均 `NotInspected` |
| loader 回读与关键运行行为 | `NotTested` |

这些 `NotInspected` 字段需要原始字节读取或 PE/符号检查能力。当前环境无法完成，已降为外部证据采集项；不得请求用户代做，也不得用 `.def`、项目属性或字符串命中填成 `Verified`。

## 4. 静态构建与资源预期

原 `wpfgfx.vcxproj` 静态记录：

- 目标配置包含 Debug/Release × Win32/x64/arm64；
- 目标类型为 DynamicLibrary，目标名为 `wpfgfx$(WpfVersionSuffix)`；
- module definition 输入为预处理后的 `wpfgfx.i`；
- linker 静态依赖列出 `kernel32`、`winmm`、`user32`、`gdi32`、`ole32`、`oleaut32`、`uuid`、`advapi32`、`rpcrt4`、`windowscodecs`、`evr`、`strmbase`、`psapi`、`ntdll`；
- delay-load 静态预期包含 `winmm.dll` 与 `WindowsCodecs.dll`；
- `milcore.rc` 直接纳入 `NativeVersion.rc` 与 `wpf-etw.rc`；
- `hw.rc` 的 shader 资源通过静态库资源链进入最终 DLL 的设计已在既有规划中记录。

以上仅是**静态预期**。真实 Release x64 DLL 的 import、delay import、resource type/name/language、版本资源和 shader 资源仍为 `NotInspected`。

## 5. 106/107/99/8/7 与数据导出状态

静态合同保持不变：

- `.def` 名称 106；
- 托管唯一 native EntryPoint 107；
- 交集 99；
- 托管独有 8；
- `.def` 独有 7；
- Debug 源码另有 `.def` 外数据导出候选 `g_fNoMeterChecks`。

对当前 Release x64 DLL 的可提取字符串搜索只命中 `MILLoadResource`，未在该 DLL 的搜索结果中命中其余 8/7 差异名或 `g_fNoMeterChecks`。PresentationCore.dll 中可见部分托管声明字符串。

此结果只能作为定位线索：

- 字符串存在不证明名称位于 PE export table；
- 字符串缺失不证明导出不存在；
- 当前工件是 Release，不能用于裁决 Debug-only `g_fNoMeterChecks`；
- 106/107/99/8/7 仍必须由真实 PE export 枚举复核。

因此所有 entry 的 `originalBinaryPresence` 仍保持 `NotInspected`，不能升级为 `VerifiedCode`、`VerifiedData` 或 `AbsentVerified`。

## 6. 本轮裁决

1. `WP-00H` 已从“完全没有真实工件”推进到“Release win-x64 原工件已定位、来源/版本/配置可追溯，二进制细节待采集”。
2. 当前 Release x64 工件允许作为后续原实现差分和 loader 基线的候选输入，但在 SHA-256 与模块回读完成前不能成为不可变 golden。
3. Debug x64、x86、ARM64、PDB、LIB/EXP 及全部 PE 深度字段记录为外部基线缺口并降低优先级，不阻塞 `WP-00I` 的静态 Ledger 工作。
4. `WP-00H` 状态为 `PartialBaselineFrozen`，不满足总计划原定义的“各架构/配置有真实基线”，不得标记 Done。
5. 下一轮继续推进 `WP-00I-MACHINE-READABLE-LEDGER`；未来一旦环境具备二进制字节检查能力，再增量补齐本记录而不是重新调查静态来源。
