# S-20260930-121：内部DrawBitmap的2D子集

新增BitmapRenderTargetExports.Drawing.cs读取Release CContextState至WorldToDevice的前缀、CRenderState，复用软件消费者绘制。前缀不是完整CContextState分配类型。矩阵依据BaseMatrix及DirectXLayer matrix_base_t继承D3DMATRIX；字段顺序依据api_rendercontext.h/api_renderstate.h。仅x64测试，未取得C++编译器布局差分。

当前支持无effects、SourceRectValid未设置、8x8抗锯齿、Linear/Nearest、默认预滤阈值、SourceOver/Copy、2D有限矩阵和整数aliased clip。其他状态明确失败。引用持有到会话释放，异常映射HRESULT。分离DrawInfinitePath未实现入口，不再共用InternalBitmap函数。

真实内部槽8测试：独立按x64偏移构造上下文缓冲，平移源位图、目标锁失败后解锁重试、准确输出左空右红。旧DLL失败，新DLL通过；最终全量140通过、0失败、0跳过，267ms。此为真实绘制行为验收，但并非原生完整对象构造/布局差分。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功；产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未独立保存。

DrawBitmap只关闭上述子集，不能从剩余清单删除完整方法。DrawPath/InfinitePath、层、显示、3D/文字/效果/视频及GenericTarget内部ABI接线仍缺；完整模块未交付。无工具阻塞。
