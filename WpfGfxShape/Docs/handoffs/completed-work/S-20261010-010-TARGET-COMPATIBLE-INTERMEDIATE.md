# DrawingImage目标兼容intermediate与消费生命周期

## 原生合同与生产差集

证据仅src/Microsoft.DotNet.Wpf/src/WpfGfx/，每次不超过400行：core/sw/swlib/SwIntermediateRTCreator.cpp 47–113（按目标GetBestBlendingFormat创建）；core/resources/BrushIntermediateRealizer.cpp 143–223（ForBlending、透明Clear、surface clip）；core/sw/swlib/swbitmapcolorsource.cpp 220–281、450–535（目标源格式与预过滤）；core/sw/swlib/swrast.cpp 1218–1254（sRGB要求Pbgra32）、1090–1145；common/shared/real.h 686–725（nearest half-up）。

CreateDrawingIntermediate/DrawingSnapshot.Realize/GeneratedImageMask.RealizeDrawingSource联合传递实际目标格式。float/HDR目标继续创建26格式float面；Pbgra32目标创建16格式兼容面，不再把AddImageMask重定向到float renderer。snapshot映射、viewport独立裁剪、COM AddRef/首错及失败finally逆序释放保持。仅内部签名变更，没有新增导出。

首次16项decoder生命周期失败并非回调/所有权变化，而是旧测试按linear插值再转sRGB推导Pbgra32目标像素。按独立原生字节span公式，source1.25处1/4与3/4权重得到64/191，active为0xff40bf00、replacement为0xffbfbfff，修正辅助预期；保持回调次数、嵌套HRESULT、活动快照、失败重试及释放断言。固定golden未改变，不从字节推导float golden。

## 两次发布证据

命令均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`；项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64；两次成功、退出码0；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。完整发布警告未独立保存，不声明零警告。

1. 起始1102/1，总1103；SHA256 184CC2CF398EC8717D67F0930718EFBCBA7494B9CF7D14BF2738706C3391B83B；TRX 621730f9-46a3-4c1c-9613-19c2f74a6d30；报告wpf-calibration-0d022bf0780c48f898be59ad5dc51605.json。
2. 目标格式接线后首次1086/17；SHA256 5815E495E5CA9C5E89CDF09BA8E7F891579223769916EA7D2F57B83B2E9E8FE7；TRX b4d54206-b843-40fa-9c4b-c990aaec6dab；报告wpf-calibration-c230e2dd3e4348128fadf2b0a58f93a5.json。仅校正测试辅助字节合同后decoder定向通过，联合869/869、1s、退出码0；最终1102通过/1失败/0跳过，总1103、2s、退出码1；最终TRX 640f8f49-6684-474b-9f6d-b315f26d39df；报告wpf-calibration-df2dd5c8d8c744b696a5971b7ecd9d50.json。定向TRX身份未提取。报告及固定com-acceptance.trx在Tests/WpfGfxShape.ComAcceptance/TestResults/。

## 改善与未关闭

直接Pbgra32差分36→9/180；float保持63/450，格式判定差54。固定原版DLL/样本SHA256由最终报告核验不变，BitmapSource与实色对照一致。HDR、1e10、Vector4距离<1e-7保持。

实际目标兼容intermediate、消费者与既有重入/失败生命周期已有合并进展，但完整旋转闭环未关闭。剩余9项直接Pbgra32与Visual mask作用域、float63项不能混为相同问题。下一轮以独立可比合同分别合并mask实际层级、旋转float采样/覆盖及新路径故障释放，不恢复固定float重定向掩盖格式问题。阶段4 Active，不能替换原库。
