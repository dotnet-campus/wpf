# S-20260930-117：内部目标ABI依赖及基类验收Red

核对BaseMatrix.h无额外字段约束、api_renderstate.h字段顺序、AliasedClip.h布局、CIntermediateRTCreator第二基类及InternalRT.h方法族。ShapeData.h明确IShapeData是CShapeBase typedef；CShapeBase含虚析构及轮廓遍历，IFigureData另有虚方法表，不能将托管形状地址代入。CMILLightData及矩阵调试字段仍需完整布局验证，未编造偏移。

新增InternalTargetContract测试，要求真实内部视图与公开视图具有相同IUnknown身份；释放原目标/工厂后，内部视图基类Clear仍写入真实保活位图。只调用已确定IMILRenderTarget前缀槽，不猜测私有DrawPath参数。当前在QI阶段E_NOINTERFACE失败，尚未到达身份/像素断言。

全量134通过、2失败、0跳过，230ms；TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。本轮未改生产、未发布。工作区不是Green。

仍未满足用户完整交付要求。没有工具阻塞；新增Red本身不是实现进度，生产内部视图/上下文/画刷/形状仍须联合完成。不通过虚假QI、跳过或改预期掩盖。
