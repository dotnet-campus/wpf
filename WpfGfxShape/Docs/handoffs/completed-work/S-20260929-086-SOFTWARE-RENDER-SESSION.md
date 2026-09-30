# S-20260929-086：软件目标消费会话重构（完整模块未交付）

## 实际变更

新增 Core/SoftwareImageRenderSession.cs，集中拥有目标QI引用、MIL/WIC位图、写锁和软件图像消费者。GeneratedVisualRenderer保留绘制列表捕获，执行时通过会话提交图像及透明层，不再直接操作目标位图COM槽和锁指针。

初始化失败由finally逆序清理；Dispose先摘除字段，再丢弃未结束层、释放锁、位图及目标接口。Draw/BeginLayer/EndLayer在会话释放后返回无效调用。本次属于内部生命周期重构，没有新增COM导出或虚构QI接口，也没有新增功能测试或声称取得新行为Red→Green。

## 原生证据与边界

核对 core/common/InternalRT.h:44 起：IRenderTargetInternal继承IMILRenderTarget与CIntermediateRTCreator；DrawBitmap使用CContextState。core/api/api_rendercontext.h:20-125包含显示集/DPI、矩阵、裁剪、RenderState等状态。

会话是托管内部所有权边界，不是IRenderTargetInternal实现，不是CContextState布局翻译。仍通过目标位图锁消费，仅保持已有整数轴对齐DrawImage能力。没有证据支持将这次重构视为通用目标接通。

## 验证及发布流水

- Release/win-x64 dotnet build成功。
- 发布1，完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`。
- 结果：成功；产物 `artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 发布后执行 `dotnet test WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`（从工作区根目录），退出码0，全量121通过、0失败、0跳过，236ms。
- TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；本次覆盖旧结果，未独立复制TRX。
- 对应发布1 SHA256（从TRX读取）：`0D021D7397B0A32E30406C6D7619B610654E5A54064C272ABBAE64CF32E506C9`。

## 未完成与续接

本轮没有完成要求的大模块，也没有外部阻塞；仅完成其内部所有权重构。原生内部目标/上下文、通用仿射覆盖率、复杂裁剪、剩余源包装、活动Render真实回调重入均未完成。阶段4仍Active，不具备替换资格。原生映射和Ledger未提高完成状态。

继续原工作项的完整通用消费者与Compose闭环，不将本会话类型作为模块完成证据，不以既有121项回归代替新增合同验收。
