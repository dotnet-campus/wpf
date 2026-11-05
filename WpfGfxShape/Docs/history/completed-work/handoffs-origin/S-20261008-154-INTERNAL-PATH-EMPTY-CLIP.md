# S-20261008-154：内部路径方法族空裁剪生产消费

## 原生证据与生产修改

读取 core/sw/swlib/swsurfrt.cpp:790-957：DrawPath 与 DrawInfinitePath 共用 DrawPathInternal；UpdateCurrentClip 无交集在 LockInternalSurface、形状及画刷消费前成功退出。读取 core/common/rtutils.cpp:44-132：aliased bounds 按28.4舍入、NaN无交集。BaseSurfRT.cpp:43-202 与 BaseRT.h:79-100 确认父层边界及透明度阈值。

Abi/BitmapRenderTargetExports.Drawing.cs 新增共同 DrawInternalPath 裁剪判定；Internal.cs 的两个真实 Stdcall COM 槽接入该消费者。先读当前层或目标边界，再与原生 CContextState 的 AliasedClip 相交；空层、越界或NaN裁剪直接成功，不读形状/画刷/RenderState，不锁目标。非空路径仍 E_NOTIMPL，null context 为 E_INVALIDARG；未添加导出、伪COM、托管测试入口或新所有权。不得将该方法族的无输出分支当作完整路径绘制。

新增 BitmapRenderTargetTests.LayerPaths.cs，8项真实生产COM用例覆盖两种目标格式×有限/无限路径×空层/上下文越界裁剪，持有目标写锁且传入不得消费的依赖指针；结束子层后父层非空路径仍明确失败，随后确定性清理。旧DLL八项 E_NOTIMPL Red，修改后内部目标41/41通过。

## 本轮实际发布（共两次）

共同完整命令：

`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标为 Code/WpfGfxShape/WpfGfxShape.csproj，配置Release、RID win-x64、TFM net10.0。两次均成功，未关闭分析器。

产物：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll`。

1. 恢复发布：成功，工具日志11行；直接生产DLL全量208/208，0失败/跳过，248ms。TRX ID `aa32d9b0-1157-4c88-b18f-c8748752f781`；SHA256 `21667AFC6014FF3733F870E4D396FAEBF714AB8C14A8EED4BB3BDD11E05036C4`。随后新增八项定向测试取得Red，HRESULT -2147467263（E_NOTIMPL）。恢复与Red TRX均被固定结果文件覆盖，未单独归档。
2. 生产修改后发布：成功，工具日志59行，含既有CA1416平台警告；日志确认Generating native code和最终publish目录。内部目标41/41，0失败/跳过，58ms；最终全量216/216，0失败/跳过，325ms。最终TRX ID `b591b8a5-feaf-4f01-b57a-ccd642f2804f`；测试实际加载DLL SHA256 `7A513FA425A2107EDEC1FAE2C831C5E83E543B67A9795F863872CAEEA5C723AE`。定向TRX被全量覆盖，未取得独立ID。

最终TRX位于 Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256均从相应TRX的实际加载路径输出读取，不根据源码推算。

## 恢复源码与历史证据边界

恢复时已有 NativeLayer 链栈、End/Abort/最终Release清理、Bounds/DrawBitmap/Clear层裁剪、NaN空层及PRGBA128Float裁剪Clear。208项恢复测试覆盖这些既有代码；本轮没有重新实现或宣称它们为新增能力。前会话199项和208项的历史发布完整日志、逐次命令及独立TRX未在本轮取得，不补造旧发布流水。前会话摘要里的中间hash不替代本轮实际加载hash。

## 未交付与下一轮

完整ImageBrush遮罩映射、层消费与Compose生命周期模块仍未交付。非空IShapeData/CBrushRealizer/CPlainPen消费者、几何mask与alpha修正层仍缺；CombinedGeometry、仿射/非退化曲线边缘原生差分、平铺DPI/预滤/接缝联合矩阵和嵌套读取回调失败恢复仍需合并。没有明确外部阻塞，本轮只是模块内部进展，不授权把下一轮拆成空裁剪补测或仅接线任务。

阶段1至3正式关闭，阶段4 Active，后三阶段Pending；机器Ledger保持RepairRequiredPartial，不晋升记录或计算整体完成百分比。当前不能替换原wpfgfx_cor3.dll。没有做覆盖率、完整原生像素差分或PresentationCore E2E。
