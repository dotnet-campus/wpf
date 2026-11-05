# 双缓冲palette、全格式消费与复制失败/丢包联合生命周期验收

## 实际生产范围

已有 `BitmapFormatConverter.Create` 接收真实palette，`DoubleBufferedBitmapExports.Create` 传递palette；本次对当前源码重新发布，确认该差集进入实际AOT。28格式直接用系统WIC转换作为字节合同，经过真实生产命令、Visual/render-data DrawImage消费前缓冲，并验收替换、目标读锁首错、重试及销毁后独立后缓冲引用。

新增第29例直接持有真实后缓冲写锁，触发 `CopyForward` 内部CopyPixels失败，而不是目标渲染失败。验收精确WINCODEC_ERR_ALREADYLOCKED、重复完成事件通知、重复句柄关闭(ERROR_INVALID_HANDLE)、partition首错持续传播。随后向失败partition提交另一个真实双缓冲及CopyForward：未消费更新引用被回收、事件完成并关闭；销毁channel/connection后保留的真实IWICBitmap可重新写入读回。没有fake COM、dummy、测试专用导出。

原生证据：`core/resources/doublebufferedbitmapres.cpp:185–323`；托管对应 `GeneratedDoubleBufferedBitmapResource.ProcessCommand`、`DoubleBufferedBitmapExports.CopyForward`、`SameThreadChannel.CommitBatches/ReleasePacket`。不回填原生在锁前递减的dirty数量；不在partition失败后自动恢复或放宽首错。

## Red与修正

新增测试首轮28通过/1失败：commit精确锁冲突、事件通知及句柄关闭已通过，释放后缓冲锁之后Present仍返回锁冲突，而非0。读取SameThreadChannel与UceChannelExports确认partition持久首错合同。修正了错误的测试恢复假设，改为验收首错持续与失败partition丢包清理；未修改生产首错语义或放宽错误码。

## 发布与验证身份

发布1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`。目标 `Code/WpfGfxShape/WpfGfxShape.csproj`、Release、win-x64；退出0，日志确认Generating native code与发布目录。产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`（工作区相对）。AOT SHA256 `A88BC8331CEAB4E276C8F87AE02CCF69E938687DCD8F2EF8C5D40C6D5AD6307D`，由同次全量校准报告实际读取。

定向：`FullyQualifiedName~WhenAuthorizedDoubleBufferedFormatIsCopiedAndReplacedThenVisualConsumesRealFrontBuffer`，29/29，RunID `ddfcc78d-142f-4e92-bc31-559bbb45e7d5`，覆盖前直接读取TRX第2行。测试构建成功，有已有MSTest分析器警告。

全量：1199通过、1失败、0跳过、1200总计，退出1；RunID `d9790bb9-8f27-42e0-9cc4-8488d1d2f592`。固定TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。唯一失败 `CompositionTests.WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements`。报告 `wpf-calibration-dc37bbf41a724fd69418bbd189997d7b.json`：float投影57/450、Pbgra32 0/180；原版SHA `501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A`，参考SHA `98D740A895372A31DAF4EB234F3942E31A8C99A15D278F121DA84C36B585CE58`。与恢复基线相比无新增失败，不声明全绿、覆盖率或UI验收。

## 状态边界

本次实际关闭上述palette/全格式前缓冲消费/复制失败/失败partition丢包联合验收范围，不晋升整个原生文件族Complete。完整通用消费者、缓存容器及其他复杂Visual/glyph/media/shader/D3DImage、严格旋转float仍开放。阶段4 Active，机器Ledger RepairRequiredPartial，不可替换原库。映射更新及ledger补充记录见对应文档；下一完整生产族为GlyphRunDrawing/DrawGlyphRun真实资源与字体来源消费者。
