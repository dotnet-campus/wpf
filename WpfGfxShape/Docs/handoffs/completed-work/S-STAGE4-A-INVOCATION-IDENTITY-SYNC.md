# Stage4 A invocation 已交接身份同步（非整块完成）

## 证据来源

本次读取 `Docs/investigations/stage4-block-a-invocation-progress.md` 和 `stage4-final-finite-closure-audit.md`，同步此前被旧失败记录遮蔽的最新交接身份。以下发布与测试结果来自用户交接及 invocation-progress 的已记录回读，不是本次重新执行或独立回读产物。

- Release/win-x64 Native AOT 发布成功；此前完整发布命令、发布序号未在该记录保留，本次不补造。
- 产物：artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
- SHA256：E798360D7B3C0E5C0254016C917D4F6803692B786D346A0FC0EAD78FD339BE32。
- 普通 full：4199 通过、0 失败、0 跳过；COM full：1346 通过、0 失败、0 跳过。
- COM full RunID：3322e91f-f596-412a-94a8-62cc2b23b96f。
- report：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-37d0765da7b84724ae03961e4c583128.json；此前记录同 SHA，Compared=450。
- 本次名称搜索未找到该 report 或 Tests 下的 TRX，不能声称本次同产物回读。发现数未有独立原始结果，不另造数字。

## 本次实际验证

专用测试执行 `WpfGfxShape/Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj`，filter `FullyQualifiedName~FrozenShaderInvocationTests`：退出码0，发现/通过2，失败0、跳过0。实际回读完整16行日志；这是既有ordinary tests复验，不是新增COM消费者验收。本次没有生产代码修改、没有新测试、没有发布或full；未取得新SHA/RunID/report。

五个恢复入口已实际写入并回读搜索确认最新SHA，Ledger证据文件也实际回读；未改机器Ledger记录状态或将required evidence晋升完成。

## 生产范围与差集

FrozenShaderInvocation 捕获 effect command、常量数组、samplerInfo/handles/列表、shader bytecode/RenderMode/CompileSoftwareShader，并拥有内部依赖引用；新增两项 ordinary tests，不是公开 COM callback 证据。

代码回读确认：SoftwareBrushRealization.Capture 的 CacheBrush/VisualBrush 路径仍进入 source realization；GeneratedImageMask.TryCapture 在 AcquireBitmapSource 后仍读取 brush.Source/ViewboxUnits/Stretch/Alignment；GeneratedVisualRenderer.CaptureContents 在 CaptureInstructions 后遍历 live visual.Children。不得只复制 child 数组或多 AddRef 就声称整树 pure owning capture。

A 的剩余完整交付：phaseCapturePure/RealizeFrozen 覆盖所有消费树、brush 参数与 source/originformat、安全源 lease、visual root/clip/children/content instructions/内部依赖；既有 cache key/version pending transaction 后验不变；真实公开 decoder/IStream/WIC callback 改后续 sampler/child/opacity/close handle 的当前帧旧值和下一帧新值；公开 CopyPixels 的 cache hit、不重复重绘、invalidate/retry 计数；最终发布、普通 full、COM full 和同产物独立回读归档。

B generation/device shader cache 及 nonOOM fallback/BadShader/OOM；C GENPROTO required evidence 仍待有限验收。阶段4 Active，A 未 Closed；该文件只同步身份，不是整块交付或稳定资格晋升。
