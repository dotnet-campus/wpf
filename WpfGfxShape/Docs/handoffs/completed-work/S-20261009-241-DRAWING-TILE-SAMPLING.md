# S-20261009-241：旋转 DrawingImage 透明 padding 与共享 tile 采样闭环

## 范围与生产修改

本轮从当前工作区实际恢复，不以旧757项文档代替源码。起始全量813/817，4项nearest旋转透明padding失败。GeneratedImageMask原先将nearest DrawingImage分解为独立局部tile，小viewbox/Stretch.None产生的透明padding因此变成旋转几何边缘，并被再次覆盖削减。

改为所有可分配DrawingImage模式共享完整viewport tile网格；普通Bitmap及超过int.MaxValue的超大tile继续局部优化。复用DrawingSnapshot的原网格整数裁剪、PixelToDevice及完整同尺寸tile AddRef路径，不新建ABI。既有二次栅格化DrawingImage禁用预过滤，filtered Border、nearest Extend及分区Extend策略保留。未修改精确预期、未回填实测值、未放宽Vector4距离<1e-7、HDR/1e10断言。

本轮增加24项sourceMode21组合，与已有12项组成模式1/2/3 × FlipX/FlipY/FlipXY/Tile × fill/stroke/opacity-mask完整36项。内容为一个红色不透明texel及透明padding；viewport(0.25,2,8,8)、viewbox(0,0,1,1)、Stretch.None、左上对齐，旋转(0.8,0.6,-0.6,0.8)，平移X=3。

## 原生证据与oracle

读取src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/ViewportAlignedIntermediateRealizer.cpp:677-755、1090-1175。原生先算完整基础tile整数尺寸与映射，再按原网格像素边界考虑裁剪；CreateSurfaceAndContext合成content→viewport→scaled sample→surface变换，不把透明padding当独立旋转内容边缘。

设备点(1.5,1.5)/(2.5,2.5)反算为(-0.3,2.1)/(1.1,2.3)。减viewport原点后为(-0.55,0.1)/(0.85,0.3)。双线性texel中心坐标为(-1.05,-0.4)/(0.35,-0.2)：首点X翻转权重.95，非X翻转为0，Y翻转行权重1、否则.6；第二点X权重.65，Y翻转行权重1、否则.8。nearest选择首点仅X翻转为texel零、第二点均为texel零，alpha为1。预期在修改生产前已存在并失败。

## 所有权与生命周期

不改变Stdcall、HRESULT首错或COM身份。完整同尺寸tile AddRef后由finally释放；非复用路径锁先释放，再使用tile。repeated锁、bitmap、MIL source按逆序释放。DrawingSnapshot失败不提交候选，释放draw快照；mask幂等Dispose和释放后保护保留。联合CompositionTests包含真实嵌套位图模式1/2/3、回调替换/关闭/重入、首错阻断/失败重试、目标外零解码及公共释放保护；不是本轮新注入内部对象的验收。

## 本轮发布记录

两次均使用完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。

共同目标：WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj；Release；win-x64；net10.0；生产Native AOT shared library。
共同产物：WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始发布成功，日志11行。对应全量817项，813通过、4失败、0跳过、1s；失败实际alpha分别.546875/.609375、0/.78125、.890625/.78125、0/.609375，预期为nearest texel0的0或1。测试有MSTEST0044/MSTEST0037警告。未在TRX覆盖前提取起始发布完整SHA256及Run ID，故不能提供该发布的历史hash关联；不使用旧hash前缀代替。
2. 修复后发布成功，日志60行，包含CA1416平台警告，完成Generating native code与发布输出。对应初始几何定向215/215，Run ID d158668a-bcce-49fe-8e39-7f86d8fb313e。添加测试不改变生产DLL；随后联合定向478/478及最终全量841/841通过，0失败、0跳过、1s。完整SHA256：DDA62707422E7DA76E3CE3F0195F7A72201844DB2830358DEF1080314901731C。

最终TRX：WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx；Run ID：9dc47d2e-4a9c-452a-8d45-c4fe628e9e3f。hash由WhenPublishedArtifactIsValidatedThenItsSha256IsRecorded记录，关联发布2。固定TRX路径后续可覆盖。

上轮未归档发布的完整hash/日志不可恢复，本文件不虚构上轮发布历史。当前源码的共享filtered邻域、Border/分区Extend及完整tile复用在本轮发布2统一重新验收。

## 完成边界与后续

关闭当前可分配DrawingImage旋转透明padding重复覆盖缺口，36项消费者组合及现有真实过滤生命周期通过。不是完整ViewportAligned/DeviceAligned原生方法族直译，也不是所有任意仿射的原生差分证明。下一轮合并完整尺寸/映射/裁剪/flip方法族及DeviceAligned非重复映射，接入生产消费者和失败生命周期，不再按单一参数拆轮。阶段1–3Closed、4Active、5–7Pending；Ledger RepairRequiredPartial，不能替换原库。
