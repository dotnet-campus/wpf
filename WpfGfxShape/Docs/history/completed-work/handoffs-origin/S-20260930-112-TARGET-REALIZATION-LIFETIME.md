# S-20260930-112：目标内画刷实现生命周期

## 执行反思

多轮小分支发布后即停止，持续推迟内部目标ABI，是未完成模块的执行原因；工具正常。回归Green、接口命名或报告未完成都不能替代完成门禁。

## 核心合同复核

core/uce/printtarget.cpp:47-105：GenericTarget创建DrawingContext、BeginFrame、Render、EndFrame，失败释放DrawingContext；138起ProcessCreate持有IMILRenderTarget。drawingcontext.cpp:157-201：BeginFrame明确QI IID_IRenderTargetInternal，ChangeRenderTarget，设置RenderState、当前时间；EndFrame清空栈和EndAndIgnoreAllLayers。

当前BitmapRenderTargetExports仅有公开11槽位图目标表，SoftwareImageRenderSession QI位图并取缓冲，不满足上述内部目标ABI。不能以内部C#类、改名或同名QI成功代替此合同；该缺口仍是内部待实现，不是外部阻塞。

## 生产修改

Direct3D9SoftwareImageRenderer.DrawTransformed先进入既有SoftwareRenderTargetSurface.DrawPath，实际源尺寸/格式获取、转换、预滤波和仿射CopyPixels在fillPath回调中执行；回调仅在目标裁剪非空且缓冲锁定验证成功后发生。两种采样分支在该区间内执行扫描，不再重新进入DrawPath。finally清除待执行源实现及当前裁剪，阻止同实例嵌套执行。

移除仿射入口重复的空裁剪预判，交由软件目标统一判断。没有改变整体会话持有COM位图锁的范围，没有宣称完成原生上下文。

## 发布1与回归

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64 build和publish成功。产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
真实DLL全量133通过、0失败、0跳过，271ms，包括七项活动读取回调及空裁剪源锁恢复。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
SHA256 `C1EDBFF28451ECE2B15CF48115D66338892409565A966CB488597D40AA189C06`。
本次复用现有行为回归，没有宣称新行为Red。

## 未通过门禁

1. 生产GenericTarget→BeginFrame→内部目标ABI/上下文仍缺；当前C#生命周期重构不关闭此项。
2. GeometryGroup含CombinedGeometry等完整组合仍缺；曲线边缘原生差分及数值精度未闭合。
3. RenderOptions仅位图缩放、SourceOver/Copy子集；完整合同未验收。
4. 模块完整交付、阶段关闭和替换资格均未达到；当前实现保持明确的不支持边界。
