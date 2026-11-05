# S-20261008-151：内部位图裁剪与回调引用保护

## 交付边界

本轮推进完整ImageBrush遮罩/Compose模块中的内部位图消费者，未完成或关闭整个模块。没有外部工具阻塞，不授权将后续工作继续拆为补测小轮。

## 原生依据与生产修改

- 原生证据：src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/swlib/swsurfrt.cpp:635-687。DrawBitmap先UpdateCurrentClip，无面积直接进入Cleanup；否则锁目标、调用软件光栅器，最后解锁。
- 上下文依据：core/api/api_rendercontext.h:20-106的CContextState字段顺序。继续只读取Release布局前缀，不构造完整原生对象。
- BitmapRenderTargetExports.Drawing.cs：裁剪与目标尺寸交集无面积时，在效果捕获、源GetSize和目标锁之前返回成功；目标尺寸查询首错直接传播。
- 目标及源AddRef提前到效果列表Capture之前，引用由finally逆序释放，避免效果列表外部回调释放调用者引用后继续访问未保活对象。未改变Stdcall槽、指针ABI、接口身份或托管软件消费者链。
- 新增BitmapRenderTargetTests.InternalClip.cs六项生产COM用例：PBGRA32/PRGBA128Float，空裁剪、右侧越界、左侧越界，目标持有写锁时无面积绘制仍成功。

## 实际发布流水

两次发布均使用完整命令：

`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标项目WpfGfxShape.csproj，配置Release，RID win-x64。产物均为`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`（相对工作区根目录）。

1. 恢复发布：成功，工具退出码0，日志11行。紧随全量185/185，0失败/跳过，229ms，TRX ID bd9022ab-c10a-4747-b4f1-e69d81026711。此发布SHA256未在覆盖TRX前读取，不以旧归档哈希冒充本次证据。
2. 修改后发布：成功，退出码0。可见诊断包含CA1416；工具保留日志尾部20000字符，前部输出截断，不宣称无警告。随后内部目标定向16/16，0失败/跳过，45ms，TRX ID 5c62041c-7248-44a0-9956-6fd6ded085a2；最终全量191/191，0失败/跳过，232ms，TRX ID 06702c47-dc6c-444a-a787-2310f4ceeefc。最终TRX实际加载DLL SHA256为5A787A91E90806B7B6B6017E4C8B8FB1AD1284CA3E0DD40A258637522612808D。

## Red与验证证据边界

新增六项用例在恢复发布DLL上全部失败，实际HRESULT为0x88982F0D（目标锁冲突），定向0通过/6失败/0跳过，27ms；修改后发布进入Green。首次测试编译有MSTest分析警告，不在本轮扩展处理。

最终TRX路径`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。初始、Red和定向TRX被后续运行覆盖，未独立复制；上述ID及结果取自当时读取或工具日志，不能作为保留了全部原始文件的声明。提前保活变更尚未新增专门效果列表释放调用者引用的回调用例，全量Green不能替代该项证据。

## 保留缺口与下一轮

唯一下一动作继续为完整ImageBrush遮罩映射、层消费及Compose生命周期闭环：优先真实内部DrawPath/BeginLayer/EndLayer/AbortLayers及IShapeData/画刷指针消费者，并联合CombinedGeometry/非退化仿射曲线差分、Visual/作用域动画映射、tile/DPI/仿射/8位浮点/预滤接缝矩阵、读取回调与嵌套层及失败恢复验收，统一发布、定向/全量、映射和Ledger差集收口。本轮空裁剪修复不重复领取。

未运行原生二进制差分、覆盖率或PresentationCore E2E；未修改原WPF源码、方案或新增Host。阶段4仍Active，机器Ledger仍RepairRequiredPartial，当前不能替换原wpfgfx_cor3.dll，不报告整体完成百分比。
