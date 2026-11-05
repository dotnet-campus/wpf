# S-20260930-126：原生插值分派及独立预滤状态

核对swrast.cpp:1000-1088：软件sRGB采样仅Nearest独立，其余合法模式走双线性；DrawBitmap接受Linear/Cubic/Fant/TriLinear/Anisotropic并按此分派，不虚构独立算法。swrast.cpp:1106-1160先DeriveFromBitmapAndContext预滤再GetCS_Resample，修复最近邻强制禁用预滤的错误：上下文显式PrefilterEnabled覆盖Visual质量默认规则。

内部DrawBitmap参数用例扩展5个非Nearest枚举，准确像素/锁失败/子矩形/裁剪/抗锯齿回归；另验证红绿2→1的Nearest+预滤产生平均像素，Nearest无预滤产生绿色。旧DLL非Linear用例失败，新DLL全量144通过、0失败、0跳过，349ms。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取。

仍未完成DrawBitmap：effects和浮点目标尚缺。已读取renderingbuilder.cpp:38-139确认AlphaScale/AlphaMask实际依赖IMILEffectList；当前Abi无该实现，不能忽略效果参数或用测试假对象冒充。原生上下文布局差分仍待执行。无工具阻塞，本次未满足用户完整交付要求。
