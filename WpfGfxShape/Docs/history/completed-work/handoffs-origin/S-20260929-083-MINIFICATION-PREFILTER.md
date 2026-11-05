# S-20260929-083：整数边界缩小预滤波接线

## 实现与原生证据

Direct3D9SoftwareImageRenderer新增默认sqrt(2)阈值分桶尺寸，调用BitmapFormatConverter.CreateScaler创建WIC Fant scaler，再回到既有格式转换/双线性重建/source-over链。prefiltered状态避免重复分桶，scaler/QI接口finally逆序释放，失败HRESULT传播。非整数边界、非正尺寸仍明确失败，未扩展通用变换。

证据均在src/Microsoft.DotNet.Wpf/src/WpfGfx：core/common/BaseMatrix.cpp:918-1054,1090-1195的ComputePrefilteredSize/阈值分桶与矩阵补偿；core/sw/swlib/swbitmapcolorsource.cpp:162-205的尺寸选择，490-590的Fant scaler后格式转换；core/api/api_renderstate.h默认PrefilterThreshold=sqrt(2)。当前仅轴对齐正向整数目标矩形，不等同完整任意仿射预滤波。

## 验收

原三个Composition场景将2→1缩小从不支持断言改为准确像素行为，旧DLL定向失败；实现后新AOT全量通过。预乘红绿源缩小叠蓝结果FF40407F，未覆盖目标像素仍为蓝色。普通BGRA32源通过scaler/格式转换得到相同结果；源锁冲突0x88982F0D、目标不变、解锁并释放调用方源引用后恢复，切回1:1正常。最终121通过/0失败/0跳过，234ms。测试数未增加。多行二维、非整数桶边界及任意变换未取得本轮完整像素矩阵验收。

## 发布流水

本轮发布1次。完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；工具成功，59行日志；产物`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。未独立保存完整发布诊断。发布后全量成功，之后仅扩展BGRA32缩小失败恢复测试再次全量121/121，没有再改生产。

最终TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`记录SHA256 `1F155094C5D6B1477698C6BF903BDA47EF74D5290C5D557A2C9B96845BAAE756`；测试程序集Debug/net10.0加载Release AOT DLL。修改前Red使用上轮产物，未在本轮修改前发布；中间TRX已覆盖。

## 完整模块差集及下一轮

完整模块仍未交付，无外部环境阻塞。此为缩小子集，不关闭阶段4。下一轮必须联合通用目标IRenderTargetInternal/CContextState、变换/覆盖率裁剪、剩余源包装及活动绘制回调生命周期，合并已有缩放/层/系统WIC路径进行验收；不把单纯补预滤波测试当作独立下一轮。当前不能替换wpfgfx。
