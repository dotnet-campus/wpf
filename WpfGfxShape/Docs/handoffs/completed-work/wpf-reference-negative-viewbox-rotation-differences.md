# 原版校准：负viewbox/旋转新增差异

扩展原版和AOT全图矩阵至450组，新增sourceMode17/18各无变换/旋转45组。源为1×1不透明线性红GeometryDrawing，viewport(0.25,1,2,2)，Absolute viewbox：17为(-0.5,0,2,2)，18为(0,0,2,2)，Stretch.None；Visual旋转矩阵(0.8,0.6,-0.6,0.8,3,0)。stroke宽度2、mask为红consumer的Visual.OpacityMask，与COM fixture参数对应。

原版2/2通过，固定样本Tests/WpfGfxShape.WpfReference/TestResults/original-wpf-969d90798ec749f1ba25b7e89974e14b.json，SHA256 F7262BD9018B48C57BEEAA934E5BD82091202F35DA022423BEC628FDC0C842DA。原版DLL SHA256 501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A。

全图450组：334一致、116不同。分组：17无旋转3/45不同，17旋转45/45；18无旋转23/45，18旋转45/45。原22–27的270组仍全过。

报告Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-dc0bade79a564b709f4478cf7ef1e13b.json，包含分组、差异坐标、原始float、全部Pbgra32值。全量890通过1失败0跳过，总891，2秒。TRX Run ID 0a0144d8-c145-4f0c-84e6-5ee12487852a，路径Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。本次未发布，产物继续使用上一轮3A5A375270B9DD5AEFE7F8F85550CDC41ADA3C843E3496E38D72DF880106C54C。

没有修改生产算法或回填预期，没有skip/放宽；这不是新的稳定基线。旋转存在原版非零边缘而AOT全强度/零的差异，但尚未确认根因。读取确认VisualTransform传入Capture与GeneratedNativeStroke；不能仅根据现象归因于变换遗漏，须继续核对扫描覆盖、intermediate采样、nearest路径及输入层级。

下一闭环：分离源内容、目标几何覆盖和变换影响，以原版相同参数的实色/刷子fixture定位，再联合修正生产映射/过滤/AA、fill/stroke/mask和生命周期，发布全量验收。不要把116组差异直接换成AOT预期，也不要从8位字节推定原生HDR精确值。
