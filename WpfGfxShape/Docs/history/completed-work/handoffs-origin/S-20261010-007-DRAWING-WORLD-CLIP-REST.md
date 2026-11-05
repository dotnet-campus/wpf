# DrawingImage world clip/rest映射推进（未关闭）

## 实际生产差集

DrawingSnapshot独立接收worldTransform，求viewportToWorld及brush scale/rest，正常网格clip先转换world空间与render bounds求交，再经inverse brush rest及world scale进入网格。surface返回映射按inverse world scale、brush rest、worldToDevice组合。GeneratedImageMask局部tile递归独立保留原worldTransform，不将tileParent作为world尺度来源。超大有界路径保留。

原生证据：src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/ViewportAlignedIntermediateRealizer.cpp 397–462，world clip求交及inverse non-scale转换；每次读取少于400行。当前double矩阵及render bounds反算仍非完整原生FLOAT合同，不标记整族关闭。

尝试DeviceAligned直接消费：移除CanRealizeDirectly的DeviceAligned限制导致9项新增回归，float差分66→69、直接Pbgra32 42→45，已撤回。不能仅移除限制绕过现有全目标路径；须联合DeviceAligned源clip及目标格式span合同实现。未放宽断言、修改golden或更改COM所有权/首错/逆序释放。

## 四次发布证据

每次完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`。
目标WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64；四次成功、退出码0。共同产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll；完整发布警告未独立保存，不声明零警告。

|发布|全量结果|SHA256|TRX Run ID|校准报告|
|---|---|---|---|---|
|1 起始|1080/1|C23D9C6A3F1E766571C3E6F830D896044ABBEF76EA9325A66274F13F5DC5AA2F|6873d9e2-9b49-46a1-a4bd-54a5eb682368|wpf-calibration-fc670347d11c46b88ab7e8c90adb9418.json|
|2 world clip/rest|1080/1|047CBA2481CCEDD0A16B9CA028F0A3C3A43CB946F5A5792DB16D2CBCFD17A8DE|未提取，固定TRX已覆盖|wpf-calibration-82a234e52bcc49aeb6286a234c6fafdc.json|
|3 DeviceAligned直接消费候选|1071/10|AF0C6EEB3C5ACA1ECC99E75A44043F53488B83A6A583F2AC8616E87F6E2A851B|329a219c-cc48-468e-93ee-a053f606c153|wpf-calibration-1ca2b9c53a6b4a8ca00d55c967a3272c.json|
|4 撤回候选并保留递归world身份|1080/1|9FEFFC30AFEB02F821EA3CDEA584B406CBBD466805612B4C470CDE309C988608|32ff0520-9558-4117-a702-fa2ec9ea5158|wpf-calibration-57ded207e58947a882757fde56229aff.json|

发布4联合DrawingViewbox/CombinedGeometry/Decoder/HdrGradient/CroppedDrawingTile 707/707通过、1s、退出码0；定向TRX身份未提取。全量总1081、0跳过、2s、退出码1。报告及固定com-acceptance.trx位于Tests/WpfGfxShape.ComAcceptance/TestResults/。

最终唯一失败仍严格校准：float66/450、直接Pbgra32 42/180、格式判定差24。固定原版DLL与样本身份由最终报告核验不变；保持HDR、1e10、Vector4距离<1e-7。

## 未关闭与后续合并

world clip/rest生产接线有进展，但严格差分没有减少，完整旋转闭环未完成。下一轮合并DeviceAligned源clip/目标格式span、ViewportAligned最终span及消费者失败生命周期，避免重复仅修改CanRealizeDirectly或FLOAT最终组合。无外部证据阻塞，不缩为补测或重复发布；阶段4 Active，不能替换原库。
