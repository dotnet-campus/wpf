# S-20260929-092：矩形几何动画消费

GeneratedRectangleGeometryResource分离Transform与RadiusX/RadiusY/Rect动画属性引用，保留已有Dependencies监听。GeneratedImageClip两条路径共享CurrentValue解析，避免将Double/Rect动画资源当作变换。非零当前圆角仍明确不支持，不能声称完整圆角动画。

原生依据：core/resources/rectanglegeometry.cpp:58-96，GetRectangleCurrentValue分别接收矩形/半径动画，再读取几何变换。原生源码只读。

真实Composition用例先于实现：绑定RectResource覆盖静态整数矩形产生半像素裁剪；只更新RectResource转为整数左像素裁剪；解绑后恢复静态半像素裁剪并继续既有层/变换回归。旧DLL定向失败，新DLL全量124通过、0失败、0跳过，244ms。扩展既有用例；中间Red日志未独立存档。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

项目Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，成功。
产物：`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。
对应SHA256：`346B9AF85319CD5A6E3366130DEC4831CC8EE695DE91499B96A7FA0B0951D7BA`。

## 未完成

完整大模块仍未交付，没有工具阻塞。内部目标/上下文、完整路径/曲线/组填充、圆角/椭圆几何及动画、IWGX-only反向包装、活动Render真实回调重入仍缺。矩形动画不等于全部几何动画。阶段4Active，不能替换原DLL，不提高Ledger完成状态。
