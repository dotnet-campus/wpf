# S-20260930-135：ImageBrush Stretch与Alignment

依据TileBrushUtils.cpp:268-359映射公式，捕获Stretch/Alignment，实现None、Fill、Uniform、UniformToFill。viewport裁剪反变换到源坐标，与viewbox相交后作为一次覆盖形状，避免两次几何覆盖相乘产生边缘错误。TileMode仍限None，相对viewport/RelativeTransform未实现。

真实ImageMask参数用例增加不同纵横比的UniformToFill右对齐、Uniform、None，保留源锁失败父目标不变和调用方释放保活重试。旧DLL新增3项失败；发布后右对齐用例初始预期误将alpha写成255，按透明目标上0.75遮罩推导改为0xbfbf0000，未改生产算法。最终全量通过。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间结果未单独存档。

方案C未关闭；所有Alignment组合、viewport裁剪边缘差分、tile、相对单位/变换和活动回调仍需完成。
