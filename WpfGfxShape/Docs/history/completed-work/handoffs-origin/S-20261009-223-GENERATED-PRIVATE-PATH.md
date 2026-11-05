# S-20261009-223：generated 软件路径实际调用 private DrawPath 第10槽

## 原生证据

- `core/sw/swlib/swsurfrt.cpp:789-960`：裁剪后锁定目标，fill→pen/stroke，首错跳转清理，最终解锁。
- `core/sw/swlib/swrast.cpp:533-593`：Ensure 接收 context/creator，借用 Brush/Effects，消费后由上层释放。
- `core/geometry/ShapeData.h:1-125`：IFigureData 原生虚方法合同；不能把 managed coverage 冒充该对象。
- 原生源码未修改，保持 net10.0 与现有 SDK/依赖版本。

## 生产差集与边界

- `SoftwareImageRenderSession.DrawPath` 不再直接调用共同 managed 消费者。通过真实 target QI 取得 IRenderTargetInternal，实际调用 Stdcall 第10槽；QI 引用保护 target，finally 平衡释放。
- 在真实 target 的 InternalView 中登记仅调用期有效的生产 SoftwarePathPrimitive：已变换 fill coverage、已加宽 stroke coverage、各自 context 和现有 renderer。GCHandle 在进入槽前建立、退出槽后解除；失败路径同样解除。
- private DrawInternalPath 在活动软件 primitive 内复用会话 renderer/写锁及 opacity 层；不重复锁定目标，不重建丢失层状态的 renderer。外部 IShapeData/CPlainPen 路径保持原读取与 Widen 实现。
- 槽内使用 GetInternalClip，fill→stroke 首错入口及共同 Ensure/Brush/Effects 消费；传入实际软件 context prefix 与 target creator，不再给 generated resource Ensure 传三个零参数。
- 保留 fill/stroke BeginPrimitive owner 保护以及 stroke→fill EndPrimitive 的 Free/Release/延迟Dispose。活动槽拒绝不同 context 或重入消费，返回 WGXERR_INVALIDCALL；拒绝不改变活动状态。
- 合并软件 render-state 创建方法，避免单画刷与联合路径重复配置。

这关闭了 generated→真实第10槽的软件生产路由，不是完整原生几何/画刷对象布局闭环：槽参数 shape/pen 为0，设备空间几何从明确的软件 primitive 适配取得；context 只分配本库消费的 prefix，不能交给外国 CContextState；GCHandle 软件适配不是 MSVC IShapeData/CPlainPen。不得宣称原工作项中完整生产ABI目标已经全部完成。没有 fake COM、测试导出或字段writer；不扩展 meta。外部阻塞未确认。

## 验收

- 开始前先发布，再全量435/435（366ms）。
- 修改后发布，既有重入定向69/69（71ms）。这是既有行为回归，不是实现前功能Red。
- 新增4项真实generated解码期间直接重入private第10槽，覆盖联合ImageBrush/solid与图像遮罩、像素及流最终销毁一次。
- 首次新增定向69通过/4失败：测试把private WGXERR_INVALIDCALL误写为Present的E_UNEXPECTED，实际像素正确；核对Direct3D9Factory的0x88980085后修正预期。这不是生产功能Red或TDD修复。
- 最终发布后定向73/73（73ms），全量439/439（362ms），0失败/跳过。覆盖原有fill锁失败→stroke不解码→解锁重试、目标外不解码、回调删除/替换/关闭channel/connection及释放target/factory。
- 最终TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，run id `e8102715-3a6f-4dbc-b45a-5ac715d31bf5`。
- 最终SHA256：`0FA49B5FF35F540C6A4EF6B9348416B6DDFFC0A2E4074137CF7135A3D908583D`。

## 实际发布记录

三次均调用发布工具，完整命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`；目标项目 `Code/WpfGfxShape/WpfGfxShape.csproj`、Release、win-x64、Native AOT共享DLL。

|序号|结果与诊断|对应验收/产物证据|
|---|---|---|
|1|成功，日志11行|开始前435/435；未单独保留SHA256/TRX，后续同名TRX覆盖|
|2|成功，日志59行|接线后69/69；新增测试69通过/4预期错误失败。中间TRX run id `560177d9-7498-4812-b867-d5bd665d5d14` 对应69项；中间SHA256未单独保留|
|3|成功，日志60行|软件render-state复用后最终73/73及439/439，对应上面的最终SHA256/TRX|

产物路径三次均为 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。已有CA1416、MSTEST0044/MSTEST0037警告未作为本轮旁支修改。日志工具成功结果和测试记录可核实，未运行脚本或自定义进程。

## 下一整块任务

合并完成生产generated几何/pen原生适配→第10槽真实参数→resource/immediate Ensure消费→primitive逆序释放，移除软件primitive侧挂几何路径。需要同时处理矩形/路径/曲线/组合几何、设备变换与加宽、coverage保真、mask/layer/effects、重入与首错，不能仅完成槽表或几项补测就结束。完整MSVC Brush/QI/meta、非圆dash/marker与完整层栈继续明确保留；阶段4Active，阶段1-3Closed、5-7Pending，不能替换原库，不报告整体完成百分比。
