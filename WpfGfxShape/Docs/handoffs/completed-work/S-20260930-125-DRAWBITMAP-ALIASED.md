# S-20260930-125：DrawBitmap非抗锯齿覆盖

NativeRenderState.AntiAliasMode.None传至不可变上下文与扫描管线。矩形仿射覆盖采用28.4边坐标单采样，输出0或64，不以8x8覆盖阈值代替。纹理采样仍独立Linear/Nearest。依据aarasterizer.cpp:1432-1460，None不施加AA半像素补偿/8倍缩放。

真实内部槽8测试红绿源半像素平移，None输出完整红与红绿混合，切回EightByEight输出半覆盖红，准确像素通过；旧DLL失败，新DLL全量成功。旋转/斜切及边界原生差分未独立完成。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未独立保存。

DrawBitmap仍未完整交付：effects、浮点目标、其他插值及原生上下文布局差分等尚缺。无工具阻塞。
