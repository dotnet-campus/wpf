# nearest固定点取整候选：未改善撤回

按真实残余分组选择source18/affine0/Tile/NearestNeighbor，Linear两种模式已一致而nearest两种consumer不同。原生core/sw/swlib/bilinearspan.cpp693–746：Transform integer device点后Round到16.16，再加FIX16_HALF右移选nearest；Initialize的AdjustForIPC见419–436相关索引，不能把单项Round当完整span直译。

实验两种输出格式Affine/Float将直接floor(samplePoint)改为先量化(point-.5)*65536再加半像素取整。发布后普通bitmap仍64不同、DrawingImage86，旧896通过无新增失败。无改善，已撤回而非保留猜测。

两次完整发布命令均：dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64
目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；固定产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
发布1退出码0、60行日志；SHA2561F2B6BE2B56DF42400BDA136C65513CD99BCEF8731C0E7746D5A16953439708B，报告dfc40d1f10f54f8e9208a6a3eff584a0对应发布1。
发布2撤回退出码0、60行日志；SHA256252DAC7D124A51CBE02996D51E4A71E25B290FD15A39C4E3D9099C4D5AA350DF，报告Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-c74d08ca384c4829ba28fb49de5d707d.json对应发布2。完整发布诊断未独立保存，不声明无警告。
两次全量均896通过1失败0跳过总897，2秒；最后TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。没有本轮保留生产修复，不是新稳定基线。

下一步必须针对所选fixture独立验证base tile内容、实际scaling mode和纹理映射，不重复局部取整猜测。缺少原版中间面实际读回时用显式2×2 padding位图与同参数绘制对照，先证明差异在哪个阶段；严格原版golden不变。
