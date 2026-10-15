# S-20261008-150：遮罩模块续接与曲线方法族回归

## 交付状态

本次完成上轮未收口源码的恢复验证、曲线协议方法族验收和文档同步；不是完整ImageBrush遮罩映射、层消费与Compose生命周期模块交付。没有外部工具阻塞，不授权把原完整模块缩为独立补测任务。阶段4仍Active，仍不能替换原wpfgfx_cor3.dll。

## 源码事实和原生证据

恢复时生产源码已包含GeneratedGeometryBounds及GeneratedImageContentBounds.TryClip接线。GeneratedGeometryBounds以端点和三次曲线导数极值累计边界，遍历填充路径、组、矩形/圆角矩形及椭圆；二次曲线转为等价三次曲线，Arc复用SoftwareArcConverter。不以光栅采样边缘代替bounds，实际覆盖仍由既有光栅链计算。CombinedGeometry仍返回不支持。原生核对：src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/utils.cpp:769-825、903-1048的CBounds::UpdateWithBezier、SolveSpecialQuadratic和GetDerivativeZeros。数学方法核对不等于完整浮点精度或边缘差分验收。

恢复全量182通过，包含上轮留下的线段及非直线三次曲线相对遮罩用例；先前未运行的曲线用例在本轮实际通过。本轮未修改生产实现，没有新的Red→Green生产修复声明。

本轮修改CompositionEffectsTests.ImageMask.cs：真实PathGeometry协议增加QuadraticBezier、PolyBezier、PolyQuadraticBezier三项。顶部曲线端点y=0，二次控制点y=-0.5或三次控制点y=-1/3，曲线实际极值y=-0.25，区别于控制点包围盒。复用既有真实connection/channel/resource/render-data、相对ImageBrush、嵌套PushClip/Pop、源锁失败丢弃层与释放后恢复验收。全部直接加载生产AOT DLL；没有新增导出、Host、托管生产程序集引用或.slnx修改。

恢复源码还包含上轮六项生产IStream/系统WIC读取回调验收，覆盖Present重入、通道/连接销毁、资源删除、源替换、帧快照及最终流释放一次；本次全量通过包含这些既有测试，不宣称本轮重新实现或完成全部联合生命周期矩阵。

## 本轮全部发布流水

两次调用完整命令均为：

`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标Code/WpfGfxShape/WpfGfxShape.csproj，配置Release，RID win-x64。产物均为d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll。

1. 开始前发布成功，工具返回11行日志；在测试前未另读取/保存完整发布输出。初始全量182/182、0失败/跳过、220ms；TRX ID 432d6e57-da8b-4258-80e2-82728c79495c。实际加载DLL SHA256 F0A15A0916C2620D6D745A085DC2EED43EEA3C0C3FEC74D0075BC0037E44EE10。
2. 新测试通过后再次发布成功，日志11行已读取，退出码0，包含生产构建及发布目录输出，无编译警告行；这是增量发布，不代表整个项目无警告。最终全量185/185、0失败/跳过、247ms；TRX ID 5866cba4-6cc0-4b2f-b278-cafaf8fc157b。实际加载DLL SHA256同上，对应此次发布。

本轮没有失败发布。初始三次测试目标使用目录路径，被工具以“目标文件不存在”拒绝，未执行测试；改用WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj后正常执行。不得将这些路径错误记为发布失败或工具不可用。

## 测试证据

定向筛选FullyQualifiedName~WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping：22/22通过、0失败/跳过、52ms，TRX ID 9a8ee5b9-e230-450c-b76c-cda14833cf65，使用发布1。测试编译有既有MSTEST0044/MSTEST0037警告，不在本任务扩修。

最终测试目标WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，Debug/net10.0测试进程加载Release/win-x64生产AOT DLL。TRX位于Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx；此路径会覆盖，目前仅保留最终全量TRX，初始/定向ID与结果在本记录保存，未独立复制其TRX文件。未运行原生二进制差分、代码覆盖率或PresentationCore E2E，不宣称其通过。

## 当前差集与下一轮

继续同一完整模块：CombinedGeometry及非退化/仿射曲线边缘原生差分；平铺基础tile尺寸、DPI、非整数/仿射、stretch/alignment、8位/浮点、预滤与接缝联合矩阵；真实读取重入和嵌套层失败恢复联合矩阵；原生内部目标CContextState/IShapeData/画刷消费者合同，不能用QI成功或托管上下文替代；统一发布、全量、映射/Ledger及独立归档。此处是未交付边界，不是外部阻塞。

Ledger现有机器证据有连接JSON对象损坏且subject链接未修复，本轮不以不可靠记录晋升文件/阶段状态；实际差集另存Ledger/views/image-mask-module-current.md，机器Ledger仍RepairRequiredPartial。
