# S-20261008-161：实色几何填充消费者与遮罩失败恢复

## 生产差集和原生证据

读取 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/solidcolorbrush.cpp:20-72`，确认 GetBrushRealizationInternal 获取当前动画颜色和 opacity，仅将 opacity 乘到颜色 alpha，再设置缓存实色实现。

`Core/GeneratedVisualRenderer.cs` 的无笔画 DrawGeometry 从仅接受 ImageBrush 扩展到 SolidColorBrush。捕获当前颜色/透明度的值快照，验证有限数，保留局部 bounds、设备覆盖和裁剪。实色快照不持有资源 handle 或虚构 C++ COM 对象。

`Core/SoftwareImageRenderSession.cs`、`Core/Direct3D9SoftwareImageRenderer.ImageMask.cs` 接通实色值；`Core/Direct3D9SoftwareImageRenderer.SolidMask.cs` 将常量实色物化为 1×1 位图，Extend 复用现有几何覆盖、合成和层消费者。8 位目标复用已有 scRGB→sRGB 预乘转换；浮点目标写预乘浮点值，不经 8 位中转。临时写锁在消费前释放，finally 逆序释放写锁和位图；HRESULT 首错沿用既有路径。浮点分支本轮没有新增专门实色验收，不能据此声明其边缘差分完成。

## 测试诊断与结果

开始发布后全量 268 通过/3 失败/0 跳过，275ms：上轮未收口的三项实色用例在 commit 返回 malformed packet。修正测试 SolidColorBrush 命令 0x7D→0x7E，以及新资源创建前将 fillBrush 归零（否则对既有 ImageBrush handle 加引用），之后 commit 成功，三项在 Present 返回 E_NOTIMPL；定向 60 通过/3 失败，69ms，生产缺口得到确认。

生产修复发布后定向 62 通过/1 失败，71ms：非作用域用例默认遮罩插值与不透明像素预期不一致。将实色用例明确设置最近邻，与既有作用域测试一致，不修改生产默认采样；最终定向 63/63、66ms。

最终全量 271/271，0 失败/跳过，261ms，退出码 0。覆盖普通/作用域几何实色填充、半透明颜色与遮罩组合、相对 bounds、嵌套 opacity/Pop、遮罩源写锁首错、失败层丢弃、来源引用释放后重试、空 viewport 和下一帧恢复。最终模型修正过，不将全部最终像素断言宣称为同断言 Red→Green；正确协议包下的 E_NOTIMPL 生产失败已明确取得。

TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，TestRun `62567d7b-cb88-4da7-a03d-6bf362cb6ddc`。
实际加载 DLL SHA256：`0DE97F7F7F075E2A1B3B54560E6C6B705F3ABC0CF1C1696FC9BB49EAC2FD3267`，来自最终 TRX 的 ABI 测试输出，对应下述发布 #2。

## 逐次发布记录

两次完整命令均为：

`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

项目 `Code/WpfGfxShape/WpfGfxShape.csproj`，Release，win-x64，Native AOT 共享库；产物均为 `artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

1. 恢复发布：工具报告成功、11 行日志；随即全量测试取得 268 通过/3 失败。原始发布日志未在下一次测试前读取或独立保存；没有取得该次独立 SHA/TRX 标识，不借用历史 SHA。
2. 生产修复后发布：退出码 0，59 行可读日志；有 CA1416 平台警告。工具显示原输出 409681 字符，受 20000 字符限制截断；已核实 Native code generation 和 publish 产物路径。该产物取得上述定向/全量结果和 SHA/TRX；之后未再发布。

测试编译还出现既有 MSTEST0037/MSTEST0044。恢复、Red 和中间定向 TRX 被固定结果文件覆盖，未独立保存完整原始文件；最终文件也使用固定路径，后续运行可能覆盖。本文保存核实过的结果摘要、最终标识和 SHA，不冒充完整日志/TRX 独立备份。

## 未完成边界及下一轮

本轮为既定模块内部生产缺口修复，不是完整模块关闭，没有确认外部阻塞。原生 IShapeData/IFigureData、CBrushRealizer/CMILBrush、CPlainPen/CContextState 非空 fill/stroke ABI 消费者、一般组合 bounds、效果回调保活与完整 Compose 生命周期仍待完成。未修改原 WPF 源码、方案、导出或依赖；未执行 Ledger 修复/验证、覆盖率、原生差分或 PresentationCore E2E。阶段 4 Active，Ledger RepairRequiredPartial，不能替换原 DLL。

下一轮合并任务以 `../current-work-item.md` 的唯一下一动作执行；实色/图像填充及锁失败恢复作为回归，不另立补测轮。
