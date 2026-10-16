# WindowsDesktop 精确版本来源审计（只读）

## 裁决边界

未找到用户明确指定必须兼容 **10.0.12** 的来源。已读证据能确认：它首先是历史本机原版采集身份，随后冻结 JSON 的绝对路径被普通测试直接重放，后续 Docs 将其称为“目标版本”。不能把这些派生表述反向当成用户强需求。仓库文件不等于完整用户会话历史，故最终精确 patch 口径标为 **待澄清**，不自行选定新版本，也不将缺 10.0.12 永久列为必需用户输入。

## 证据链（路径均相对 WpfGfxShape，根 global.json 除外）

| 文件/行段 | 已核实内容 |
|---|---|
| `Tests/WpfGfxShape.Tests/SystemVideoSampleTests.cs:19–36` | `WhenRealSystemMediaHasNoFrameThenSoftwareSampleLeaseCanBeClosed` 在27–30行查找固定资源 `Tests/WpfGfxShape.WpfReference/TestResults/original-wpf-e07cbceaf453471dacb4cb12c99bdbc2.json`，31行解析，32行 `NativeLibrary.Load` 直接使用 `NativeDll`；不是从当前 runtime 动态发现路径 |
| 上述 `original-wpf-e07…json:3–11` | 采集于 `2026-10-10T06:19:28.4371734+00:00`，Runtime `.NET 10.0.12`、X64；PresentationCore 和 NativeDll 均指向 `C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\10.0.12\`；wpfgfx 文件版本 `10,0,1226,42308 @Commit: 95017c711e6afc1085133d440e42b4bd78155701`，SHA `501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A`；此为校准原物身份，不是用户版本声明 |
| `Tests/WpfGfxShape.WpfReference/ReferenceBehaviorTests.cs:397–417,424–440` | Runtime/架构/PresentationCore路径/NativeDll/版本/SHA从实际进程采集；426–427行以实际 `DrawingVisual` 程序集目录拼接 wpfgfx 路径，没有写死10.0.12。确认“当时安装环境采集→资源固定→重放绝对路径” |
| `Docs/handoffs/completed-work/wpf-reference-behavior-capture.md:3,9–15` | 用户诉求记载为新建空WPF项目、DLL Import 原版并提供独立预期证据；11–14行记录当次10.0.12/X64及SHA，并未记载用户指定patch |
| `Docs/investigations/INCOMPLETE-HW-CACHE-CLOSEOUT/evidence-readback.md:35–41` | 历史加载异常为10.0.12下wpfgfx **or one of its dependencies**，0x8007007E；另有指定目录不存在的核查。异常本身不区分主DLL与依赖缺失；不改历史失败 |
| `Docs/completed-work/PresentationCoreE2E-host-evidence.md:11–18,22,40` | 已有真实10.0.1原版 Pcore Green 与候选Red，双方身份独立；后续文档称“不替代目标10.0.12”，这是派生验收口径，不证明用户固定版本 |
| 工作区根 `global.json:2–5,12–20`；`Tests/WpfGfxShape.ComAcceptance/global.json:2–5` | SDK10.0.107，latestFeature；SDK选择不同于 WindowsDesktop runtime patch 选择，不能推出必须10.0.12 |
| `Tests/WpfGfxShape.WpfReference/WpfGfxShape.WpfReference.csproj:3–8` | `net10.0-windows`、UseWPF、x64；TFM不是强制runtime patch10.0.12。x86仍须另有正式架构证据 |

## “合法完整 WindowsDesktop”究竟要什么

指来源可追溯、获合法使用的 **Microsoft.WindowsDesktop.App 运行时**，含同版/同架构 PresentationCore、WindowsBase、原版 wpfgfx_cor3.dll 及配套托管/原生依赖，并满足宿主所需 .NET 运行依赖。完整是为避免单拷一个 DLL 导致依赖缺失、错架构或混版加载；不是要求用户购买 runtime。官方可用发行包/已安装完整官方 runtime 应优先复用，来源/版本/架构/实际加载路径与 SHA 需记录。

真实10.0.1原版 Pcore Green 是有效的该版本有限证据，不是“用户必须买10.0.12”的依据，也不自动等于最终 x86/x64 全门通过。旧 `target10.0.12Acceptance=false` 保持原义，不改成 true；10.0.1不能冒充10.0.12。

## 后续可实施路线（本阶段未执行、不改测试/golden）

1. 先明确正式兼容口径：是特定patch，还是选定并固定的官方.NET10 WindowsDesktop版本；现有文件不足以认定用户要求10.0.12。
2. 若口径允许，复用已安装、来源可核验的同版本完整官方 runtime；原版与候选在同版同架构 PresentationCore/WindowsBase承载下隔离执行，只替换候选wpfgfx角色。x86和x64分别核对真实进程/模块身份、版本、路径、SHA及所有正式配置，不预设两架构都已安装。
3. 历史10.0.12校准资源只读保留。未来如需解除绝对路径绑定，应另行有界修改测试的运行时发现/配置与来源验证，将“历史golden身份”和“运行时定位”分离；不得静默替换原golden、放宽断言或skip。新版本独立采集须保留版本隔离和差异说明，不伪称重现旧10.0.12。
4. 原版 Green、候选 Red 与历史普通失败全部保留；最终同身份差分、ABI/寿命/strict/full和x86/x64全部矩阵未满仍Open/Blocked。未证明当前失败全是环境问题，不把版本澄清变成无限外部等待。

本次只读源码/资源与文档合同更新；无终端脚本、构建、发布、测试、安装下载或生产/测试/配置/Ledger原物修改。
