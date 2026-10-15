# S-20261009-242：生产intermediate有界尺寸与映射

## 实际交付与未完成项

本轮实际接通原生二维intermediate尺寸限制至DrawingSnapshot和tile物化消费者，避免完整tile按接近int.MaxValue的面积分配。没有完成原计划的整个ViewportAligned/DeviceAligned方法族；没有确认外部阻塞，不以本轮边界作为模块完成条件。完整设备对齐、tile shift/flip裁剪及任意仿射差分必须继续合并推进，不规划仅参数补测轮。

## 原生证据

只读取src/Microsoft.DotNet.Wpf/src/WpfGfx/，每次≤400行：
- core/resources/ViewportAlignedIntermediateRealizer.cpp:380–450、677–835、835–997、1000–1085，读取实际尺寸、整数边界、负tile shift、flip与二维尺寸上限调整。
- core/resources/DeviceAlignedIntermediateRealizer.cpp:271–338，设备对齐floor/ceil与最大尺寸调整。
- core/resources/TileBrushUtils.cpp:555–610，非重复二维且无缓存选择DeviceAligned；重复选择ViewportAligned。
- core/resources/BrushIntermediateRealizer.h:29文本搜索确认MAX_TILEBRUSH_INTERMEDIATE_SIZE=2048。

## 生产改动

GeneratedImageMask.AdjustSurfaceSize复用原生二维viewport限制：两维均超过2046时2048×2048，细长维保留两个texel为2046；按原生X后Y顺序调整。DrawingSnapshot在整数裁剪后限制实际surface尺寸，基于最终尺寸重算content→surface及surface→device，保持内容范围。非重复尺寸上限2048，但仍未切换完整DeviceAligned方向，不能把限制称为完整DeviceAligned直译。

Direct3D9SoftwareImageRenderer.ImageMask使用相同限制，保留完整同尺寸DrawingImage AddRef复用，避免capture与消费者网格尺寸不一致及二次栅格化。原有超大1e10局部分区路径保留；未扩展缓存、3D pow2、Extend、ABI或layer stack。

候选snapshot未提交时Dispose，bitmap/lock/session首错和逆序释放不变；全量重新验证既有真实过滤回调、关闭/重入、首错和重试，但本轮没有为新限幅fixture增加专门decoder回调生命周期，不能宣称该联合缺口已关闭。

## 独立oracle与验收

新增sourceMode22–24：百万×百万、4096×4及4×4096 viewport，HDR常量与fill/stroke/mask、模式1/2/3。新增sourceMode25：4096平方viewport，经限幅映射到2048网格，半tile内容边界位于texel1023/1024；设备点对应权重0.75/0.25，nearest为1/0，HDR为(0.625,0,0,0.5)，mask为(0.5,0,0,0.5)。保持Vector4距离<1e-7，不回填实测值。

定向先248/248，增强oracle后251/251；最终全量853/853，0失败0跳过，14s。新增12项，原841项全部通过。

## 逐次发布

两次完整命令均为：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
目标WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，配置Release，RID win-x64。
产物均为WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始发布成功，工具日志11行；在测试覆盖日志前未提取全部发布诊断，不声称零警告。首次测试使用目录目标被工具拒绝（目标文件不存在），改为唯一COM csproj后841/841、0失败0跳过、1s。SHA256 DDA62707422E7DA76E3CE3F0195F7A72201844DB2830358DEF1080314901731C；Run ID db1f39e2-df6b-4c7b-b490-2f8f957121e8，已在覆盖前读取。
2. 修改后发布成功，退出码0，日志60行，含CA1416平台警告，生成生产Native AOT DLL。之后只修改测试，没有再次修改生产源码。定向及最终全量对应此发布。SHA256 7B94AE055658F304EA02E693CD1F23834A6D76E61F90BD0610593AA08CB1503C；最终Run ID 989008c6-2272-4876-8dba-b922e34ec900。

TRX固定路径WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx会覆盖；最终hash及Run ID已直接读取。未取得两次定向Run ID，不虚构。

阶段1–3 Closed、阶段4 Active、阶段5–7 Pending；Ledger RepairRequiredPartial。完整intermediate、曲线成本/拓扑、MSVC ABI、层栈/UCE及PresentationCore E2E未关闭，不能替换原库。
