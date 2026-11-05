# S-20261009-222：联合路径 realizer primitive 生命周期

## 原生证据

- `core/resources/BrushRealizer.h:40-110`：resource/immediate/null 创建者以及 Ensure、Restore、Free、借用 Brush/Effects 虚方法。
- `core/sw/swlib/swsurfrt.cpp:796-900`：DrawPathInternal 的裁剪、锁、填充先于描边、填充首错阻断加宽/描边和最终解锁；913-951 的 DrawPath/DrawInfinitePath 委托。
- `core/uce/drawingcontext.cpp:1232-1290`：联合路径调用者在完成后按 stroke→fill 调用 FreeRealizationResources/Release。该段为 bounds 调用者，不将其冒称所有绘制上下文的完整实现。

## 生产差集

- 纳入此前未收口的联合 fill/stroke 帧命令、目标外剔除及软件 resource realizer 虚方法实现；开始前发布并在新 DLL 上确认 431/431。
- `SoftwareBrushRealization` 以 BeginPrimitive/EndPrimitive 取代单画刷 Draw：primitive 开始时建立 owner 重入保护，resource realizer 由真实创建者取得，结束时 Free 槽→owner Release→延迟 Dispose。未完成创建时回滚 guard。
- `SoftwareImageRenderSession.DrawPath` 在消费前取得 fill 与 stroke 两个 realizer；取得 stroke 失败也清理已取得的 fill。整个路径退出时以嵌套 finally 按 stroke→fill 清理；释放回调结束后才撤销路径重入 guard。
- 原实现填充消费后立即释放 resource realization，再开始描边；现将实现保活扩大至整个联合 primitive，尚未消费的描边 owner 也在填充回调期间受保护。
- `BitmapRenderTargetExports.Drawing` 的 private 和 generated 路径均经过 realizer 指针入口和共同填充首错/描边控制方法；private 保留原几何读取及 NativeStrokeReader 加宽，generated 使用已捕获 coverage 和 drawing context；删除会话的单画刷 DrawRealization 旁路。
- null/solid 使用已有 immediate 创建者，ImageBrush 使用软件 resource 创建者→Ensure→借用 bitmap immediate→消费者→Free→Release。没有新增公开 ABI 或测试导出。

## 验收

唯一测试项目：`Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`，直接加载发布的生产 AOT DLL。

- 开始前全量：431/431，359ms，0 失败/跳过。
- 增加 4 项 solid fill + ImageBrush stroke 的真实 generated 命令验收，包含联合图像遮罩、解码回调重入 Present，以及释放 target/factory、关闭 channel/connection；断言最终像素、嵌套 HRESULT 和流最终销毁一次。
- 修改后定向 `FullyQualifiedName~WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels`：69/69，70ms。
- 最终全量：435/435，355ms，0 失败/跳过。既有 private path、bitmap/null/solid、锁失败重试、联合填充首错及 owner 重入用例均通过。
- 本轮新增测试在生产修改后添加，属于行为回归验收，没有取得实现前 Red，不报告 TDD Red→Green。
- 最终 TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，run id `e0324dc6-d49c-448f-b0b2-6c1bad43e795`。
- 最终测试加载 SHA256：`F494CD0582309BA9FEA14AB2CA90E1CFF9B5060307C96220D885615E1F53ADD3`，对应发布 2。

## 实际发布记录

两次均使用完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。

| 序号 | 项目/配置/RID | 结果及证据 | 测试对应 |
|---|---|---|---|
| 1 | WpfGfxShape.csproj / Release / win-x64 | 成功，发布日志 61 行 | 开始前 431/431；未单独保存该次 SHA256/TRX，后续测试覆盖同名 TRX |
| 2 | WpfGfxShape.csproj / Release / win-x64 | 成功，发布日志 60 行 | 定向 69/69、最终 435/435；SHA256/TRX 如上 |

产物均为 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`（相对工作区）。发布没有运行脚本或自定义执行器。构建与测试仍存在既有 CA1416、MSTEST0044/MSTEST0037 警告，不报告无警告构建。

## 边界及下一生产模块

本轮关闭的是纯 C# 软件联合 primitive 的创建/借用/消费/逆序释放生命周期，不是完整 CBrushResourceRealizer/MSVC 布局/QI/meta。generated 仍未调用 private DrawPath 第 10 槽；共同 realizer 方法入口不得冒称真实槽接线。

下一模块合并 generated 几何/描边适配、真实第 10 槽调用、原生 context 与 creator 参数接线、fill/stroke 首错和 borrowed owner 重入保护以及发布验收。不得仅增加槽表、自调用或测试专用字段 writer；遵循 coverage/layer/效果保真及原生实际可达性边界。完整层栈、非圆 dash/marker、meta 及 MSVC 对象布局仍未完成。

阶段 1-3 Closed、阶段 4 Active、阶段 5-7 Pending；Ledger 仍为 RepairRequiredPartial，分母未确定，不报告整体完成百分比。当前 DLL 不能替换原库。
