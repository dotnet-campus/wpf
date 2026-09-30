# S-20260929-081：已有位图目标生命周期（完整模块未完成）

## 生产实现

接通factory槽6及原生导出MILFactoryCreateSWRenderTargetForBitmap。当前要求源同时提供IWICBitmap/IWGXBitmap，支持PBGRA32/PRGBA128Float目标；保留原位图而非复制，查询引用失败清理、分配失败逆序释放、目标最终COM Release。

BitmapRenderTargetExports.Bounds/Clear/Release不再解读SoftwareBitmap内部布局：尺寸与GUID格式经COM读取，Clear校验stride/size后写入，目标释放使用COM Release。已有源码的透明Clear及颜色Clear行为保留。

证据：src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/api_factory.cpp:344-390，exports.cpp:94-102。原生通过CWICWrapperBitmap包装普通WIC位图；本轮只实现双接口位图子集，不代表外部WIC-only包装已完成。InternalRT.h及intermediatertcreator.h:73-107确认多重继承及中间目标职责，本轮未虚构这些ABI。

## 验收

新增已有位图建目标用例先通过factory槽6调用得到失败Red；生产实现后通过：释放原目标，向新目标Clear红色，释放新目标后原位图仍可读取正确像素。新增导出factory/bitmap/output空参数3项行为测试。

Composition三个场景改为从已有输出位图创建新目标后执行整个Visual/DrawImage链，覆盖共享目标、锁失败重试、层、转换、源替换和释放保活。最终120通过/0失败/0跳过（235ms）。创建用例Red的完整诊断未独立归档，不冒称精确错误文本已提取。

## 发布流水

发布1完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

生产项目Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；工具成功，60行日志，未单独归档完整诊断。产物`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。发布后初次全量成功；仅扩展测试后最终全量120/120。本轮只发布1次。

TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，记录SHA256 `C6F53B8B3742067FF32DAA65C79F58894A35E16E529A5CD8F8AD8907970523C3`。测试程序集Debug/net10.0直接加载Release AOT DLL。Red使用前轮产物，TRX已被后续覆盖。

## 未完成

仍未满足用户要求的完整模块。本轮交付工厂和目标生命周期子集，不等同通用IRenderTargetInternal/CContextState、WIC-only包装、变换/缩小预滤波/复杂裁剪或活动绘制重入完成。没有外部环境阻塞，不降低原条件，阶段4保持Active，不能替换wpfgfx。下一轮必须合并这些生产依赖，不能继续用独立导出或回归测试作为整体交付。
