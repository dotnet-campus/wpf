# PresentationCore primitive / miter：生产根因与真实像素有限验收（06）

## 结论与身份

2026-10-15 实际执行标准 Staging → 原版 → 候选 → 独立 compare，32×32 主场景由历史12pixel差异变为0，threshold=0、byte-exact Green。此有限 primitive/miter 主场景里程碑 Closed；不是完整 E2E-01–09、全架构或最终替换验收。

生产 DLL：`2F83C04E3DEBA5A106BCE5215AD8400238C31FBC988F166B44BE94366D59AE09`，4447232 bytes。本轮没有修改生产源码，没有生产 publish，没有 floor quantize、RGBA 或阈值尾改。标准 Staging 构建成功；仅为标准 COM MTP 包执行 `dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true`，成功，未重新发布 Native AOT 生产项目。包 aot.sha256、三次 COM Results/aot-identity.json 及真实 Pcore requested/frozen/actual 模块一致。

## 根因与原生对应

四 typed primitive Rectangle/RoundedRectangle/Ellipse/Line 及 animated 资源已经由真实 DrawGeometry 消费；初始化、RenderOpen、RTB Render/CopyPixels 不再是停点。根因修复位于 `Code/WpfGfxShape/Core/SoftwareStrokeWidening.cs` 的 join/下一段起点连接：普通 miter 保留 miter 点为下一段外侧起点，不再额外延伸至 offset 起点；闭合接缝继续桥接。

原生 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/strokefigure.cpp:1794–1837` 的 `CSimplePen::MiterTo` 明确区分 fExtended：闭合接缝 PolylineWedge 两点且 current=ptNextStart；普通角仅一个 miter 点且 current=ptMiter。生产对应 starts 更新和 extendToOffset=false（约203–232行）。几何上共线的额外浮点点，经定点 floor 量化后不保证共线，改变外侧斜率/覆盖；修复连接语义而不是改量化或颜色。

先前独立控制 `Investigations/PresentationCore-render/Results/controls-candidate-04` 保留 fill=0、Bevel/Round stroke=0，而 Miter stroke/both=12 的定位；`controls-miter-05/Results/candidate/controls.json` 已逐节实读，同一生产SHA的18项全部 count=0（fill/stroke/both × Miter/Bevel/Round × 两种figure形式）。这些是既有独立控制证据，不冒充本轮重跑。

## 本轮真实主场景原物（均相对 WpfGfxShape）

证据根：`Investigations/PresentationCore-render/Results/`。

| 原物 | 实际核验 |
|---|---|
| `miter-main-original-06/Results/original/result.json` | .NET10.0.1 X64；status Green、exitCode0、4096 bytes；原版实际模块94C06484… |
| `miter-main-candidate-06/Results/candidate/result.json` | status Green、exitCode0、4096 bytes；requested/frozen/actual均2F83…；cleanup保留模块至进程退出 |
| 上述两目录 `pixels.pbgra32` | 两者 SHA `7F55E138C9605464AE90811BD28B5477871BB3E1AA71F5FD3A1C8B052E0C308B` |
| `miter-main-compare-06/Results/diff/comparison.json` | 独立进程 Green、Equal、byte-exact、threshold0、nativeModulesLoaded=[] |

真实未改 PresentationCore SHA `605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654`；原版 native SHA `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`。两角色实际 runtime10.0.1、target10.0.12Acceptance=false；本机来源 SDK10.0.301/runtime10.0.13不冒充沙箱运行版本。

`MiterEvidence.proj` 仅用内置 Copy 取本轮新原版 result/pixels 和新候选 pixels 到标准执行包，再实际运行 compare；不使用候选自比较或历史原版冒充本轮 oracle。候选先运行时使用相同原版模块及像素身份的既有Baseline，最终独立比较已更换为本轮新原版。

## 成组测试与回归（不拼分母）

| 本轮证据目录 / TRX | pass / fail / total | 身份及边界 |
|---|---|---|
| `miter-primitive-06/TestResults/miter-primitive-06.trx` | 692 / 0 / 692 | 标准COM MTP，2F83；既有realizer、image-mask/pen、reentrant active-render、geometry utility及boolean contour成组用例 |
| `miter-com-related-06/TestResults/miter-related-06.trx` | 1057 / 2 / 1059 | 标准COM MTP，2F83；当前扩大filter，不继承旧524分母 |
| `miter-com-full-06/TestResults/miter-full-06.trx` | 1415 / 32 / 1447 | 标准COM MTP，2F83；失败保留、不降skip或阈值 |
| `miter-managed-regression-06/*16_53_30_net10.0.trx` | 4224 / 1 / 4225 | 本轮普通Tests源码回归；不是直接AOT DLL验收 |
| `miter-managed-regression-06/*16_53_35_net10.0.trx` | 28 / 0 / 28 | 本轮GenProto回归；不直接加载生产DLL，不虚称同SHA ABI测试 |

以上均0 skipped/notExecuted。普通Tests唯一失败 `WhenRealSystemMediaHasNoFrameThenSoftwareSampleLeaseCanBeClosed`：硬编码的10.0.12原版runtime native缺失，DllNotFoundException；没有改测试绕过。先调用专用测试验证后，使用 `MiterRegression.proj` 的标准 VSTest logger 再真实运行并保存TRX，MSBuild汇总因这一个失败返回失败；GenProto确实继续运行且28通过。

成组filter详见各 Results/aot-identity.json Arguments，覆盖既有路径/笔/端帽/连接/miter限制、曲线、ellipse/roundrect、裁剪及重入失败/资源释放参数族；不扩展无关生产。Full还覆盖typed primitive的现有测试。692不是新增用例数，也不是所有primitive输入的穷尽证明。

related两项为硬件cache geometry像素失败；full32项保留硬件present/cache/shader相关失败，真实native GPU control亦有失败，不一概推断为生产或环境单一根因。本轮不盲扫、不修无关HW。普通Tests及GenProto不加载本候选，关联同一集成时点而非伪造DLL identity。

## 生命周期与正式门

Pcore JSON记录 DrawingContext释放、Dispatcher shutdown、finalizers drained，DLL retained until process exit；成组active-render覆盖失败/重入及调用者释放。有限已测合同 Closed 不代表完整resources/UCE所有失败、取消、线程、callback及OOM均已关闭。

七阶段及A/B/C、E2E-01–09条件不变。四外部输入 authority、ARM64工具链/真实环境、完整10.0.12 runtime、player协议/ownership/oracle仍Open；10.0.1建设版不是10.0.12 target。遵循DEC-0004 DLL驻留、不卸载。正式总门Red，无整体完成百分比或替换资格。

历史12pixel、B9CC与rail回撤只见 [rollback档](PresentationCore-primitive-rollback-02-execution.md)；本档替代其当前停点，不篡改历史JSON/TRX。
