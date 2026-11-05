# S-20261008-162：基线复验与私有路径合同核对（未交付生产模块）

## 本轮结果

本轮完成生产 Native AOT 发布、现有直接 DLL 验收和原生创建者/消费者合同核对；没有修改生产代码或测试，没有新增生产能力，不宣称生产 Red→Green 或完整模块关闭。本文件归档验证与核对事实，不代表完成一个生产翻译模块。阶段 4 仍 Active，Ledger 仍 RepairRequiredPartial，不能替换原 wpfgfx_cor3.dll。

## 发布与验收证据

### 发布 1

- 完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
- 项目：`Code/WpfGfxShape/WpfGfxShape.csproj`；配置 Release；RID win-x64；TFM net10.0。
- 发布工具返回执行成功、完整日志 11 行。随后测试覆盖了最近日志，本轮未独立保存发布原始日志；不能据此声称没有警告，也不补写未读取的诊断。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`（相对于工作区根目录）。
- 测试 TRX 的 DLL 记录：SHA256 `0DE97F7F7F075E2A1B3B54560E6C6B705F3ABC0CF1C1696FC9BB49EAC2FD3267`，与 S-161 相同；相同哈希不表示本轮新增能力。

### 发布后测试

1. 首次指定目录 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance`，测试工具返回目标文件不存在；此调用没有运行测试，不计作测试失败。
2. 改用实际项目 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`，退出码 0：271 通过、0 失败、0 跳过，总计 271，265ms。
3. TRX 路径：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；TestRun ID `5db80508-db20-4b7d-bcaa-a205a6836416`，运行时间 2026-10-08 18:05:14 +08:00。读取 TRX 前 14 行取得本次 ID、时间、产物路径和 SHA256。
4. 测试日志提示覆盖既有 com-acceptance.trx。本轮未独立复制完整 TRX；本文保存摘要和身份，不等同于保存完整测试原始证据。后续测试会再次覆盖该文件。

## 核实的原生合同

所有原生证据均来自 `src/Microsoft.DotNet.Wpf/src/WpfGfx/`，单次读取不超过 400 行。

- `common/shared/RefCountBase.h:18-63`：IMILRefCount/CMILRefCountBase 明确不继承 IUnknown；基类有 AddRef、Release 和虚析构，引用计数初值为零。不得复用 IUnknown 三槽布局或通用 COM Release 来释放 CBrushRealizer。
- `core/geometry/ShapeData.h:25-105`：IShapeData 是 CShapeBase 的 typedef；IFigureData 是带虚析构的 C++ 类，包含属性、矩形优化和分段遍历，不能作为 generated 资源 handle 或 IUnknown 消费。
- `core/geometry/ShapeBase.h:40-95`：形状具有虚析构、GetFigureCount/GetFigure/GetFillMode，以及受保护的缓存边界虚方法。声明顺序不是经过验证的 MSVC ABI 槽号；本轮没有验证私有对象实际布局。
- `core/resources/brush.cpp:40-63`：CMilBrushDuce::GetRealizer 懒创建并缓存 resource realizer，每次返回额外 AddRef。
- `core/resources/BrushRealizer.cpp:97-166`：resource/immediate 创建者在 new 后 AddRef，再转移引用；immediate 创建还调用 SetMILBrush。`BrushRealizer.h:40-100` 声明 EnsureRealization、借用的 realized brush/effects 访问等方法。
- `core/sw/swlib/swsurfrt.cpp:796-960`：有限和无限路径共同进入 DrawPathInternal；先空裁剪判定，再锁目标；先 fill，后 pen+stroke 的 WidenToShape 和 fill；失败进入统一清理、解锁。无限路径传空 shape、pen、stroke，但仍保留 fill realizer。
- `core/sw/swlib/swrast.cpp:533-596`：先 EnsureRealization（软件 cache、display、context、intermediate creator），再获取借用 brush/effects；空 realized brush 不绘制，否则交给共同 FillPath。实现器指针存在不等于存在可绘制内容。

## 当前托管实际差集与风险边界

- `Abi/BitmapRenderTargetExports.Internal.cs:132-136` 将有限/无限路径参数缩为 bool；`Drawing.cs:80-97` 仅执行裁剪、锁和无画刷成功/有画刷 E_NOTIMPL，不消费 shape、pen、brushContext、stroke/fill realizer。
- `Core/Direct3D9ImmediateBrushRealizer.cs` 是依赖注入委托的托管对象，不是生产 C++ 私有对象布局适配。`Direct3D9SurfaceRenderTarget.cs:977-992` 的便利调用以空操作委托创建它，不能据此宣称原生创建者/所有权已接通。
- generated 几何覆盖可以递归组合采样，但 `GeneratedGeometryBounds` 的精确组合区域仍限制矩形及轴保持/轴交换操作数；不能用一般包围框冒充精确布尔区域，也不能把 generated 覆盖接线当作原生私有路径 ABI 已完成。
- 本轮没有外部阻塞证据。尚未验证 MSVC 虚析构/继承布局、CMILBrush、CPlainPen、完整 context/creator 私有合同及真实生产对象创建闭环，属于必须继续落实的实现前置，不是权限限制或已交付边界。

## 下一轮合并目标

继续完整 ImageBrush 遮罩映射、层消费与 Compose 生命周期模块，不另立“仅定位/仅 ABI/仅补测”轮。先从真实创建者与调用者落实上述私有布局、调用约定、借用/持有引用和失败释放，再保留全部路径参数接通有限/无限 fill/stroke 共同扫描；同轮合并层 Begin/End/Abort、mask/clip/opacity/Pop、一般组合与映射、Compose 重入和释放生命周期，采用直接生产 AOT DLL 的成功/失败像素验收，并统一发布与全量回归。完成条件及执行步骤只维护在 current-work-item.md。

本轮未运行覆盖率、原生像素差分、PresentationCore E2E、Ledger 修复或验证；未修改原 WPF 源码、方案、Host 或新增测试专用导出。
