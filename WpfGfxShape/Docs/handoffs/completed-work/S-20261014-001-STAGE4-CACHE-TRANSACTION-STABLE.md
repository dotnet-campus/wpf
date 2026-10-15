# Stage4 SW 缓存事务稳定收口与未交付 HW 撤回

## 交付与原生边界

Stage4 Active；A/B/C 未关闭，不能替换原库。本轮保留前轮生产修复，不重新实现或回退其它生产工作。

生产 `Code/WpfGfxShape/Core/GeneratedBrushResources.cs:289–359` 的 RealizeFrozen 已实际读回：命中 AddRef 旧 bitmap；失配使用独立 candidate；实现失败直接返回首 HRESULT，不销毁旧提交；成功且 brush/root/config 身份版本仍有效才 AddRef candidate、交换 bitmap/key、Release previous。finally 释放 snapshot/candidate/target 并清除 capturing。根因是失败候选不能提前破坏已提交缓存，修复不是忽略 HRESULT。

原生读回 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/BitmapCacheBrush.cpp:111–183`：PreCompute → UpdateCaches → GetBitmapSource → bitmap brush/采样变换，IFC 首错与 Cleanup Release。该合同不能推导出当前 generated HW DrawGeometry 已实现。

`GeneratedHardwareVisualRenderer.cs:75–132` 实际只接受 DrawImage/DrawImageAnimate 的 GeneratedD3DImageResource；其它绘制返回 E_NOTIMPL。因此把 SW 缓存 DrawGeometry fixture 直接接 HWND 是未交付的 HW 方法族扩展，不是缓存事务回归。本轮不扩普通 scene、不加 SW fallback。

## 原始 Red 与撤回裁决（独立差集）

前轮缓存原始证据仍保留：`artifacts/bin/WpfGfxShape.ComAcceptance/Release/net10.0-windows/TestResults/stage4-cache-red.trx`，RunID `8dfccd55-ca2f-487f-b2a0-0ebc28e6baf3`，0/1；旧键恢复 present 实际返回 -2147287010（0x8003001E），已读回错误与栈。后续 `stage4-cache-green.trx` 7/7，RunID `931f3f85-fd89-48b1-9d2d-062005ea61dc`；历史 `stage4-matrix.trx` 16/17 不覆盖当前重新验收结果。

HW 原始尝试：用户交接的沙箱 27 executed /17 passed /10 failed，首 Present E_NOTIMPL；4 个 scalingMode 事务案例和 action=0/1/2/3/4/8 六个拥有寿命案例。本轮未取得该原始 TRX/RunID，不能编造独立读回。原回收目录 `artifacts/sandbox-readback/stage4-hw-sw/`、`stage4-hw-sw-retry/` 保留不覆盖，已枚举后者，仅有包与 Results 身份，无原始 TRX。以上作为原始失败交接事实归档，不称 10 项已修复。

精准撤回 `CompositionTests.ActiveRender.cs` 的 hardwareConsumer 参数、HW HWND target 分支、窗口读像素/关闭辅助逻辑；恢复直接 SW Read(output)。`CompositionTests.Stage4Hardware.cs` 清为空文件（文件工具删除替代），不留 HW 死代码，不跳过测试、不改断言。`CompositionTests.Stage4LazyWicDiagnostic.cs` 17 项完整保留。

## 本轮真实验收与身份

SHA256 `20221306CB14179CA4AA3B0413F187AAEEC9CF2C60337FE6099C6637B97C5AAF`。

- 本机通过 `Tests/WpfGfxShape.ComAcceptance/Stage4StableAcceptance.proj` 调用原项目标准 MSBuild Restore/Build/Test；未改 runner。专用构建成功，0 错误，24 个既有测试分析警告。
- `artifacts/bin/WpfGfxShape.ComAcceptance/Release/net10.0-windows/TestResults/stage4-stable-directed.trx`：RunID `bbfc7e7a-3e45-4a3e-aa40-5dcab5090c0c`，17/17。
- 同目录 `stage4-stable-full.trx`：RunID `f75ac66c-4a46-4a4a-92b4-68b98a02ec14`，total/executed/passed=1376，failed/notExecuted/aborted=0。计数、RunID 和 TRX 生产 SHA 日志已实际读回；相邻业务身份 `Tests/WpfGfxShape.ComAcceptance/TestResults/aot-identity.json` 同 SHA，Sandbox=false。
- 前轮 `stage4-final-full.trx` RunID `edfad6fb-1d06-40eb-9cdb-0322fd54771a`、1376/1376 与同 SHA 日志亦已定位读回，不以转述代替当前全量。
- 沙箱仅上传 `artifacts/com-acceptance-package`，参数 `--filter FullyQualifiedName~WhenStage4 --report-trx --report-trx-filename stage4-stable-sandbox.trx`；17/17、零跳过，任务 `41c4e6fa37834867975f3038d700fe81`。回收 `artifacts/sandbox-readback/stage4-stable-closeout/TestResults/stage4-stable-sandbox.trx`，RunID `87dca74f-b65d-4e07-af07-22b734bb86fa`；完整 Counters 已读回。`Results/aot-identity.json` 同 SHA、Sandbox=true；包 aot.sha256 同值。不是沙箱全量重新通过。
- 普通 4212/4212、GenProto 23/23 为既有交接值。本轮搜索其 TestResults 未取得对应 TRX/RunID，未重跑、未宣称独立验收或同 SHA 证明。

## 发布记录

本轮发布 #1：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true`，标准 COM 验收项目/Release，未显式 RID，成功退出0；exe 运行架构 win-x64。输出 `artifacts/bin/WpfGfxShape.ComAcceptance/Release/net10.0-windows/publish/`，PackageComAcceptance 重建白名单包。使用既有 production Release/win-x64 AOT DLL，未新发布生产 DLL；生产身份不变且包 GetFileHash/实际测试身份一致。初始包 Native/ReferenceData 两目录，无 obj/PDB/历史源码。

## 成组公开合同与未关闭项

17 项为 scaling=0/1/2/3 四项失败保留旧提交/旧键恢复/同输入重试；action=0/1/2/3/4/8/9 七项 cache callback 重入、替换、删除、关闭拥有寿命；later child/brush/geometry/source 四项冻结旧帧/新帧；nested cache request 一项；真实系统 WIC mutation/nested Present 一项。不是 17 次 CopyPixels；armed IStream.Read 回调计数不是总 decoder read/CopyPixels 计数。

精确 CopyPixels 命中/失效计数未关闭；callback 内锁失败 HRESULT 的原生证据未取得，未关闭。HW cache DrawGeometry/Image/Visual/Cache 可达消费者方法族未交付，必须先界定原生授权再完整实现、成功/失败/拥有寿命/真实 HW 像素一起验收。原沙箱 22 项失败不处理、不豁免；B generation/per-device/BadShader/OOM、C required evidence 继续开放。下一轮只推进上述 HW 缓存消费者完整模块，不转向基础设施或单独计数小切片。
