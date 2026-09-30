# S-20260930-130：SolidColorBrush真实AlphaMask生产链

实现方案B核心：SolidColorBrush分离opacity/color动画槽并捕获当前alpha；Visual AlphaMask合并Visual Alpha到同层，PushOpacityMask/Pop捕获独立层。层结束生成1x1同目标格式真实SoftwareBitmap，常量Extend等价纯色遮罩，解锁后AddWithResources AlphaMask，再Add AlphaScale。列表Capture取得源引用，Prepare在独立单位矩阵/Nearest/无预滤上下文实现遮罩，后续Apply并做几何覆盖。列表、临时位图及快照确定性释放。不新增导出/fake COM。

独立CompositionEffectsTests新增8位Mask alpha=2/255、Opacity=0.75、白源：准确0x02020202，反序将为0x01010101；浮点mask=0.5、opacity=0.5保留HDR/负分量。两项在旧DLL失败、新DLL通过。追加PushOpacityMask后Pop和后续图元恢复，8位/浮点通过。动画槽已实现但更新/失败/释放计数尚无独立验收，不关闭B全部门禁。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64 build/publish成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
最终155通过、0失败、0跳过，286ms。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx覆盖中间结果。
SHA256 `050EF59422F18140A45A22EF1B60050710E04F0A66F86C0832D4E86936443E97`。

方案文档已同步。ImageBrush遮罩、B剩余矩阵、完整DrawLayer/内部DrawBitmap效果参数链及原生ABI差分仍待完成，不报告整个DrawBitmap/大模块完成。
