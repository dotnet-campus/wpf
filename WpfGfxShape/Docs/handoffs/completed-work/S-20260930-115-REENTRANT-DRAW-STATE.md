# S-20260930-115：活动绘制状态快照验收

扩展独立CompositionTests.ActiveRender参数场景7：两个放大图元，第一个源读取时经真实生产IStream+系统WIC回调提交VisualSetRenderOptions(NearestNeighbor)。当前帧两个图元仍使用捕获的双线性状态，准确输出混合红绿；下一帧使用最近邻，准确输出纯红绿。最终流释放一次断言保留。

本轮只增测试，不改生产、不发布，使用S-114已发布DLL。定向8项及全量134项通过，0失败、0跳过，最终全量266ms。TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。

这是捕获上下文重构的真实重入覆盖，不等于原生内部目标ABI或完整模块完成；门禁保持开放，无工具阻塞。
