# S-20261008-188：恢复验收与真实路径合同复核

## 交付范围

本轮完成先发布、直接生产 DLL 全量恢复验收、创建者与消费者合同复核，以及当前工作项和稳定基线同步。没有生产或测试代码修改，没有新增绘制能力，没有关闭完整 ImageBrush 遮罩映射、层消费与 Compose 生命周期模块。不存在已确认的外部阻塞；不能把本次调查当作生产闭环完成。

## 发布 #1

- 完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`
- 项目：`WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`；配置 Release；RID win-x64；TFM net10.0，未改变 SDK/TFM/语言版本。
- 退出码 0。立即读取全部 11 行日志，未见警告或错误；此次日志没有显示重新执行 native 编译，不能声称完成了全量重新编译。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 测试记录 SHA256：`2A1D0FA5AAF99090C8B957924B9488EE13277149FBEBFD18BEB3AAEF30B5436B`，与 S-187 相同。

完整可用发布日志另存 `S-20261008-188-publish-01.log`。

## 验收

发布后的首次测试目标误传目录，工具返回目标文件不存在，没有执行测试。随后改用真实 csproj：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`，退出码 0。

- 全量 290/290，通过 290、失败 0、跳过 0，304ms。
- TRX TestRun ID：`040c7bb6-ce96-4a5e-9a0e-7196af76c13b`。
- 开始：`2026-10-08T19:41:56.7033623+08:00`；结束：`2026-10-08T19:41:57.3475560+08:00`。
- 文件：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，本轮没有再次运行测试覆盖它。
- 测试日志明确报告覆盖了前轮固定名称的 TRX。前轮完整 TRX 未在本轮覆盖前另存；既有 S-183～187 摘要不能冒称原始 TRX 副本。本轮保存完整可用 16 行测试日志及上述 TRX 身份/SHA 摘要，未另存完整 TRX 副本。
- 没有运行定向新测试、覆盖率、原生差分、PresentationCore E2E 或 Ledger 修复验证。

## 原生证据与当前实际差集

所有原生证据仅读取 `src/Microsoft.DotNet.Wpf/src/WpfGfx/`，每次请求不超过 400 行。工具对部分长片段有字符截断；以下结论仅取实际返回内容，不以请求范围冒充完整已读范围。

- `common/shared/RefCountBase.h:24–63`：IMILRefCount 不继承 IUnknown；只有 AddRef/Release 的接口声明，CMILRefCountBase 声明虚析构，构造引用初值为 0。不能套 IUnknown 三槽，也没有通过本轮阅读验证 MSVC 最终布局。
- `core/api/api_base.h:52–94,110–138`：LocalMILObject 只允许局部/内嵌生命周期，retail AddRef/Release 返回 1 且不删除对象；不能把内嵌 solid 当作独立堆对象释放。
- `core/resources/BrushRealizer.h:41–100`：EnsureRealization 必须保留 adapter、destination、BrushContext、ContextState 和 intermediate creator；GetRealizedEffectsNoRef 是 HRESULT 方法。
- `core/resources/BrushRealizer.cpp:97–124,175–229,266–291`：resource/immediate/null 创建者 new 后 AddRef；null realizer 是非空对象但初始 realized brush 为 null；内嵌 solid 初始化透明。
- `core/resources/BrushRealizer.cpp:308–341,522–540,838–873`：SetRealizedBrush 使用 ReplaceInterface；resource realizer 借用 brush resource（NoRef），析构释放 effects；immediate setters 接通 realized brush/effects。
- `core/resources/brush.cpp:25–63`：brush resource 持有缓存 realizer，GetRealizer 返回时再 AddRef，resource 析构释放缓存。
- `core/uce/drawingcontext.cpp:92–104`：真实 drawing context 初始化缓存 null realizer。
- `core/sw/swlib/swsurfrt.cpp:796–897`：共同 clip/lock，fill 首先执行；随后 WidenToShape 使用 pen、flattening tolerance、WorldToDevice、surface bounds，再经相同 FillPathUsingBrushRealizer 消费；首错跳转清理并解锁。

当前托管差集：

1. `Abi/BitmapRenderTargetExports.Internal.cs:132,136` 将 private finite/infinite path 参数折叠为 bool，`Drawing.cs:80–96` 非空 brush 仍 E_NOTIMPL；没有调用真实 realizer。
2. `Core/Direct3D9SoftwareRenderTargetSurface.cs:504–566` 保留 fill→widen→stroke 次序，但仍为 bool/delegate 辅助合同，不是私有 ABI 实现。
3. `Core/Direct3D9ImmediateBrushRealizer.cs` 是委托驱动 managed helper，其 EnsureRealization 只有 cache/destination 参数；不能把它当作已实现原生 CBrushRealizer。
4. `Core/GeneratedVisualRenderer.cs:180–208` 和 `GeneratedImageContentBounds.cs:68–70` 均拒绝 HasNonEmptyStroke Pen；目前没有可复用的完整生产 Widen 实现。不得以矩形膨胀或包围框替代一般笔画。
5. generated 帧快照、逆序释放与未完成层不合成的现有实现仅作回归能力，不能证明 native alpha layer/private shape ABI 已完成。

## 收口与下一轮

修复 current-stable-baseline.md 滞留 S-179 的权威摘要，统一到本轮复验；current-work-item.md 保留同一个大模块及合并完成条件，明确本轮仅复验/复核，不新增能力。不修改 .slnx、不恢复 Host、不修改原 WPF、不新增测试专用导出。

下一轮应先发布、全量复验并在覆盖前保留证据，随后以真实生产创建者驱动 private shape/pen/context/realizer 与共同软件扫描、Widen、层状态机及 Compose 生命周期的联合实现和直接 AOT TDD；不能另立纯调查或空资源补测轮。阶段 1–3 关闭、4 Active、5–7 Pending，Ledger RepairRequiredPartial，不能替换原 wpfgfx_cor3.dll。
