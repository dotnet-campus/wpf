# S-20260929-084：图像轴对齐变换栈（完整模块未完成）

GeneratedImageTransform解析无动画Translate/Scale/轴对齐Matrix/TransformGroup，按局部到父级顺序组合；GeneratedVisualRenderer将Visual变换和offset应用到图像矩形，render-data PushTransform和PushOpacity共用带类型状态栈，Pop恢复对应变换或结束透明层，不再把全部Pop当透明层。循环/深度保护和非有限数检查保留。

证据：src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/drawingcontext.cpp:2384-2404,4863-4892，Visual先压offset后transform，内容PushTransform进入变换栈。原生资源解析已有生产模型复用，不修改generated协议文件。

边界：正向轴对齐缩放/平移，转换结果仍需满足既有整数覆盖率边界；旋转、斜切、负缩放、动画仍E_NOTIMPL。TransformGroup组合/中心缩放已有源码但未取得本轮独立像素验收，不代表全部通用变换已完成。

## 行为测试

原3个Composition场景增加Matrix(2,1,-1,0)放大/偏移，准确红绿插值FFBF4000/FF40BF00；解除Visual变换后PushTransform/Pop，后续未变换图元覆盖得到原红绿，验证栈恢复。增加不支持非对角矩阵失败目标保持不变，恢复矩阵后后续绘制正常。新增断言先在旧DLL定向失败，发布后全量121通过/0失败/0跳过；最终补失败恢复后仍121/121（233ms）。不增加用例数。

## 发布流水

本轮实际发布1次：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标生产csproj，Release/win-x64，工具成功，59行日志；产物`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。未独立保存完整发布诊断；Red使用前轮产物。发布后全量通过，之后仅增加测试再次全量通过。

最终TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，SHA256 `99B0FBF4AE8A5A6AC3DF63621C6632792F1AD6EF7BB2E2B2379B367D085C1ED1`。Debug/net10.0测试加载Release原生DLL，中间TRX被覆盖。

完整模块仍未交付，无外部环境阻塞。通用仿射覆盖率/裁剪、内部目标/上下文、剩余源包装和活动绘制回调重入仍须联合实现；本轮仅变换栈子集，不关闭阶段4，不宣称wpfgfx替换资格。下一轮不应仅补变换用例，应继续原完整生产模块的核心消费契约和联合验收。
