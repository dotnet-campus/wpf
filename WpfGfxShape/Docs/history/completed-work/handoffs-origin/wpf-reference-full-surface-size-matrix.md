# 原版校准：全表面与尺寸矩阵

本轮仅扩展用户要求的原版校准，不修改生产算法，不发布Native AOT。使用上一轮裁剪先于限幅产物，SHA256 3A5A375270B9DD5AEFE7F8F85550CDC41ADA3C843E3496E38D72DF880106C54C；对应发布见wpf-reference-crop-before-cap-calibration.md。

## 实施

- CompositionTests.BooleanContours.cs增加完整surface观察，按bitmap lock的stride逐行复制浮点像素并核验数据长度，保留默认严格断言与释放。
- CompositionTests.ReferenceCalibration.cs由两个点扩展为4×4全图，报告差异坐标、原版字节、完整AOT浮点及其Pbgra32投影，严格比较每个字节。显式断言270组、每组16像素/64字节，防止空数据误通过。
- ReferenceBehaviorTests.cs扩展sourceMode22–24：(-1,-1,1e6,1e6)、(-1,-1,4096,4)、(-1,-1,4,4096)，保留25–27。每类5种tile、3种scaling、3种consumer共270组，输入与COM一致。
- COM csproj固定链接新原版样本，不选择目录最新文件、不以AOT结果重录预期。

## 验证

原版项目2/2通过。270组×16像素=4320像素，17280个Pbgra32字节全部一致，0个不一致fixture。
原版文件：Tests/WpfGfxShape.WpfReference/TestResults/original-wpf-af79e5e1044c452baa52bc5042db24e7.json。
文件SHA256：49C0E47479F94B1387F1DBBAA1052E4CF417F90B490A25225FE33D92FDC7594F。
原版DLL SHA256：501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A。

COM全量891/891通过、0失败0跳过。TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
Run ID：2ae70be2-4b15-4daf-8fc8-28ddb626cdeb。
独立报告：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-6355b911b2784df2b34264008a517f1b.json，Compared270、Mismatches0，Scope完整4×4。

## 边界与下一动作

没有发现本矩阵需进一步修改的参数预期，既有HDR和1e-7断言保持。Pbgra32颜色转换/量化不能证明原生float精确读回；本次全图仅4×4fixture。旋转/剪切、负viewbox、透明padding、曲线/AA及其他导出契约尚未逐项原版校准。

下一闭环优先将旋转/负viewbox与跨tile过滤的源几何、变换、mask边界对齐原版，在共享完整surface对比中定位并修复差异，再联合生命周期、AOT发布验收。intermediate方法族和全面替换资格不因本矩阵通过而关闭。
