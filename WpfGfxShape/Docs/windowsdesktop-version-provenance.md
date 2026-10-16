# WindowsDesktop 版本来源与自主修复边界

## 现行裁决

[DEC-0006](decisions/DEC-0006-installed-official-runtime-contract.md)已Accepted：采用已安装、来源可核验的官方完整.NET10 WindowsDesktop runtime；原版/候选同版同架构隔离对照并各记身份。**10.0.12不是用户指定目标，不再待澄清，不要求用户另交该patch。**
历史采集身份与当前运行时定位必须分离。官方完整包括同版同架构PresentationCore、WindowsBase、原版wpfgfx及托管/原生依赖，不是单拷一个DLL，也不是要求购买运行时。

## 来源链与核验层次

| 来源 | 结论 |
|---|---|
| `Tests/WpfGfxShape.Tests/SystemVideoSampleTests.cs:27–32` | 本轮直接读源码：查找固定 `original-wpf-e07cbceaf453471dacb4cb12c99bdbc2.json`，直接 `NativeLibrary.Load` 其 `NativeDll`；运行时定位错误地依赖历史绝对路径 |
| `Tests/WpfGfxShape.WpfReference/ReferenceBehaviorTests.cs:397–417,424–440` | 本轮直接读源码：采集实际runtime/架构/PresentationCore/native版本与SHA；从实际DrawingVisual程序集目录找原版native，不强制10.0.12 |
| 上述历史JSON及[采集档](handoffs/completed-work/wpf-reference-behavior-capture.md) | 既有审计记录：2026-10-10采集.NET10.0.12/X64；Native SHA `501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A`，位于历史 `C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\10.0.12\`；本轮不重新hash原物 |
| [历史加载核查](investigations/INCOMPLETE-HW-CACHE-CLOSEOUT/evidence-readback.md) | 原0x8007007E为DLL **or one of its dependencies** 加载失败；不改成已证只有主DLL缺失 |
| [PresentationCore历史对照](completed-work/PresentationCoreE2E-host-evidence.md) | 10.0.1原版Green/候选Red按原身份有效；后续“不替代目标10.0.12”是派生口径，现由DEC-0006覆盖，不抹除失败 |
| `global.json` / TFM | SDK选择、`net10.0-windows`不等于强制runtime patch10.0.12 |

本次再次只读核对上述测试加载语句及采集器身份/程序集目录定位代码；未加载DLL、检查当前安装是否完整或重新hash历史原物。路径工程问题有源码依据，原0x8007007E的具体缺失依赖仍未诊断，不能称已修复。

## 下一工程工作（本轮尚未实施）

1. 修改 **`Tests/WpfGfxShape.Tests/SystemVideoSampleTests.cs:27–32`**，不再把历史JSON `NativeDll` 当当前执行路径。复用既有官方runtime发现/显式路径与身份验证；参考 `Tests/WpfGfxShape.WpfReference/ReferenceBehaviorTests.cs:424–440` 的实际程序集目录定位，但不得不顾宿主/架构直接照搬。
2. 验证官方完整依赖、实际进程/native架构、路径、版本、SHA，拒绝候选自身和混版加载；记录原版/候选各自PresentationCore/WindowsBase/native身份。x86/x64分别实测，不由x64推断x86。
3. 历史golden/JSON只读保留；新同版对照另存版本化证据和差异，不静默换golden、放宽断言或skip，不伪称重现旧10.0.12。
4. 专项成功后普通完整回归。旧4224pass/1fail/4225不改写通过；此失败现在归**可自主修复测试定位/身份工程问题**，不是外部缺物。修复仍可能暴露后续真实产品失败，按实际结果处理。

原 `target10.0.12Acceptance=false`、`FixedGolden1012Satisfied=false`、版本/SHA与全部失败保持原义；采用新合同不把它们设true。x86/x64所有正式ABI/寿命/strict/full/E2E门仍须满足。
本轮只更新Docs/范围解释，无代码、安装下载、构建、发布或测试。
