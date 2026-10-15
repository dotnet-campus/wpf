# S-20261008-145：平铺遮罩与层位图共同消费链

## 原生证据

- `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/drawingcontext.cpp:3890-4040`：DrawLayer以单位世界变换、最近邻、1:1映射将物化位图作为临时bitmap brush，经几何形状/DrawPath消费；无几何遮罩时不对层位图边缘再次抗锯齿。
- `core/resources/ViewportAlignedIntermediateRealizer.cpp:480-735`（分段读取）：viewport基础tile物化、按各维Tile/Flip决定重复模式，按理想tile跨度向上取整表面尺寸并建立映射。
- `core/sw/swlib/bilinearspan.cpp:10-55`：翻转维度由两个相邻subtile构成周期，FlipXY有四个subtile；负采样坐标先归一化。
- `core/common/typeconvert.inl:38-80`：TileMode到BitmapWrapMode的映射。

## 生产修改

- GeneratedImageMask快照保存TileMode，接通Tile/FlipX/FlipY/FlipXY；Extend保持明确不支持。
- ImageMask先物化viewport基础tile，再复用共同绘制/扫描管线重复或翻转，创建真实AlphaMask资源；保留源映射、伸缩/对齐、格式转换和DPI计算。未以逐块重复source-over绘制代替wrap采样。
- 软件绘制上下文保存wrap及几何遮罩状态，8位与浮点扫描复用已有Select64BitFallbackTexels。浮点插值保留完整小数精度；Extend维持边缘夹取。
- EndOpacityLayer将内容缓冲复制到真实SoftwareBitmap，释放写锁后经DrawTransformed→DrawPath→scan pipeline消费效果与几何遮罩，删除直接8位混合及CompositeFloatLayer旁路。无几何遮罩时关闭层位图边缘抗锯齿；最近邻且禁用再次预滤波。
- 所有新建bitmap/lock/MIL源引用在finally逆序释放，先实现遮罩与效果再进入父目标扫描，源锁失败保留首错。退化平铺变换不省略遮罩，回落到透明遮罩实现。
- 这只是既有托管软件绘制链接线，不等于原生CContextState/形状/画刷指针ABI或完整IRenderTargetInternal消费者合同。

## 测试与验证

- 基线先发布后全量164/164通过。
- 层位图实现Release构建成功，发布2后原全量164/164通过；新增8位及浮点嵌套opacity用例，通过真实render-data命令验证父层消费和逐层舍入。
- 四类平铺新增断言在发布2取得Red：源写锁时应返回0x88982F0D，旧DLL实际返回E_NOTIMPL；不是接口存在性验收。源码平铺实现当时已修改但尚未发布，不宣称是源码实现前Red。
- 发布3后效果/遮罩定向通过；再增加2×2反向alpha图案，验证负viewport坐标下Tile/FlipX/FlipY/FlipXY二维周期和翻转，以及源写锁失败、解锁重试和调用方源引用释放后准确像素。
- 发布4后全量174/174通过，0失败、0跳过，233ms（2026-10-08 13:05:36）。新增共10项参数化展开：嵌套层2项、平铺8项。
- 最终DLL SHA256：`A8D2B8215D74942E7378DA6EE13F0FA257CD7D6F46B4DD0ECB0E7757FBE5DF23`。
- 最终TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，TestRun ID `8d6b5822-09ff-4b34-a889-07fac7efa611`，对应发布4。基线/中间Red/中间Green TRX被标准配置覆盖，未单独归档。
- 构建/发布有既有分析器警告，不宣称零警告。未修改TFM、SDK、原生WPF源码或slnx。

## 四次Native AOT发布流水

各次均针对生产项目，配置Release、RID win-x64；四次均由发布工具返回成功，无失败发布。

### 发布1：恢复基线

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64 -o d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64`

成功，11行发布日志；随后全量164/164通过。产物：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll`。本次SHA256未提取，对应TRX已覆盖，不以发布4哈希代表本次。

### 发布2：层位图消费链

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64 -o d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64`

成功，59行发布日志，产物路径同发布1；原全量164/164通过，之后嵌套层通过、平铺取得4项Red。本次SHA256未提取，对应TRX已覆盖。

### 发布3：平铺方法族

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64 -o d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64`

成功，59行发布日志，产物路径同发布1；效果与遮罩定向通过，追加二维图案后ImageMask定向通过。本次SHA256未提取，对应TRX已覆盖。

### 发布4：退化变换及层边缘状态

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64 -o d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64`

成功，59行发布日志，产物路径同发布1；最终全量174/174通过，SHA256/TRX见上。日志工具只保留最后一次日志，发布2至4的成功状态和行数已在调用返回中核实，完整发布文本未独立保存。

## 当前模块差集与续接

完整ImageBrush/Compose模块仍未交付，无外部工具阻塞。复杂几何内容bounds、平铺的完整8位/浮点/DPI/非整数变换差分、遮罩真实IStream读取期间资源更新/删除/连接关闭及最终释放一次、原生内部目标消费者合同仍缺。层位图旁路已删除，但不能据此关闭整个层/内部ABI任务。

当前唯一动作仍为current-work-item.md中的完整模块，剩余工作联合验收，不把单个平铺矩阵或单项回调补测拆为默认独立下一轮。阶段4仍Active，不能替换原wpfgfx_cor3.dll。

## 恢复说明

前次响应错误声称工具不可继续读取/修改；本次实际读取日志、TRX并更新文档均成功，该判断没有依据，现已纠正。此记录补齐上述同一轮发布及验证，不增加发布次数，不冒称完成新的生产功能。
