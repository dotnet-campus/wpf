# S-UNDATED-072：Visual/render-data双缓冲图像消费者窄路径

## 本轮实现（验收进行中）

新增GeneratedVisualRenderer：从真实目标COM取得位图/写锁，遍历Visual子树和render-data DrawImage，取得双缓冲所选图像源，执行整数平移、1:1 PBGRA32/BGR32 source-over绘制。不清除已有目标内容。SameThreadPresent导出遍历当前连接的不同共享partition，传播partition首错并渲染目标快照；目标快照保活并最终释放。

原生依据：printtarget.cpp:47-115（根Visual、保留原目标内容）、drawingcontext.cpp:1302-1362（DrawImage获取源并绘制）、doublebufferedbitmapres.cpp:148-175（选择前缓冲/转换后的后缓冲）。

这不是完整原生Compose/DrawingContext翻译：没有IRenderTargetInternal通用实现、硬件/显示恢复、复杂状态/缩放/滤波/透明层。当前不支持状态返回E_NOTIMPL，不以导出存在声称全部完成。现有Commit同步执行批次，Present消费执行后的资源。

## 测试

新增真实DLL两帧测试：通过生产命令建立target/root Visual/render-data/image引用；第一帧复制全部，第二帧后缓冲两像素均变但只标脏第一像素，最终目标第二像素须保留旧前缓冲值。先在旧DLL缺Present导出前置失败，不声称像素行为Red。

## AOT发布流水

### 发布1
- 完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`
- 配置/RID：Release/win-x64；目标项目见命令。
- 结果：成功，工具报告13行日志；发布前Release/win-x64 build成功。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 本轮验收补记：发布后两帧定向成功，随后测试参数化增加目标写锁失败/重试及释放目标和通道后位图保活，全量112项112通过/0失败/0跳过。本轮共发布1次，参数化仅修改测试。
- 对应DLL SHA256：`AEF3F994E7E21EEC5B52B4460A16DB77B4BB091069E48E8B0BDDE78EC2FA4F88`。
- TRX：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。

## 差集与下一轮完整任务

首次取得Visual/render-data/CopyForward/Present/真实目标像素的窄路径Green。不是完整Compose，也不关闭阶段4或原大模块。当前renderer是专用软件DrawImage执行器，尚未复用通用IRenderTargetInternal/rasterizer，缩放/滤波/复杂裁剪/alpha层返回E_NOTIMPL。只验收不透明PBGRA32两帧像素，不能将实现的BGR32、source-over或子树遍历声明为均已像素验收。

下一轮将此执行器合并进通用软件绘制/目标内部接口，完成转换/裁剪/透明层和源替换、共享分区/根通道时序、重入渲染资源快照及失败生命周期；补充半透明混合/偏移/空图像/不支持状态/多目标验收。特别需要核对connectioncontext.cpp:448-525的根通道筛选与显示状态重试，当前仅去重partition，不等同原生所有连接状态。源替换、失败事件、格式转换端到端矩阵仍为未完成。
