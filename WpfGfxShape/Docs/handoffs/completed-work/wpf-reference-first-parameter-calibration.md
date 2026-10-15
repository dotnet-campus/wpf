# 第一批原版参数校准与差异

按用户要求开始使用原版实验结果核对现有参数/输出，而非继续增加自推导 oracle。

## 输入校正

读取生产 MilTileBrushCommand 与 COM fixture 后，修正 WPF 基准 ViewboxUnits 为 Absolute；mask 从 DrawingContext 的白色 PushOpacityMask 改为 Visual.OpacityMask 的红色 consumer。原来样本 cae82710033d408886b8f8833c9a2219 输入不等价，不得继续用于校准。

原版重新测试 2/2，生成 Tests/WpfGfxShape.WpfReference/TestResults/original-wpf-7a705f2b844f46e2946e01200fbeb614.json，SHA256 A5C1B1D8ED17893D29B0F0CE98A9711B82C334BD58FD95CA74B7A987F108CB67。原版 DLL SHA256 501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A。

## 差异测试

唯一 COM 项目添加 CompositionTests.ReferenceCalibration.cs，复用生产 AOT 调用、bitmap lock 与清理；观察出口不改变原有测试默认断言。135 组输入每组读取 (1,1)/(2,2)，浮点线性预乘值先取消预乘、转换 sRGB 再预乘及量化，不从原版字节反推 float golden。修正投影满强度端点的浮点舍入后：111 组一致，24 组不同。

差异全部集中 sourceMode25，Tile/FlipX/FlipY/FlipXY × LowQuality/HighQuality × fill/stroke/mask。原版 BGRA 为 [0,0,128,128]/[0,0,0,0]；AOT 投影为 [0,0,96,96]/[0,0,32,32]。旧 0.75/0.25 权重不是原版实测一致 oracle；现阶段不能用8位字节直接替换 HDR 精确值。

全量测试：890 通过、1 失败、0 跳过，共891；失败为新校准测试，保留红灯而非 skip/放宽。TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。

独立报告：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-e8e915be23634b52a9b84d33e42eb4f4.json。对比 AOT SHA256 046103C7C13EA5DA49CCD0D9A43044C82C419F0A0EBA9E31D21AF3FC816B9524。

本轮未发布 Native AOT、未修改生产源码或既有 float/HDR 预期，使用已有产物。当前不是全量通过的新稳定基线。111组仅两个投影采样点一致，不表示全图或浮点一致。

## 下一闭环

围绕这24组差异核对原版可见局部tile裁剪与限幅时序，修正生产中间面映射/过滤相位；扩展原版全图配对与可比格式/浮点读回证据，联合fill/stroke/mask生命周期及发布验收。禁止修改预期来迎合AOT实测值或用量化结果推定 <1e-7 的 HDR 值。
