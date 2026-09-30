# S-20260930-124：DrawBitmap自定义预滤阈值

NativeRenderState.PrefilterThreshold传入不可变绘制上下文，整数/仿射共用PrefilterSize。取消只接受sqrt(2)的限制，按BaseMatrix.cpp:912-1055、1070-1130处理非正阈值不预滤、倒数阈值分桶及阈值>=1的精确尺寸分支；默认Visual路径仍用sqrt(2)。非有限调用方阈值明确E_INVALIDARG。

真实内部槽8验证红绿源2→1缩小，自定义阈值2和1的准确像素，NaN失败及恢复；旧DLL失败，新DLL全量通过。该尺寸只覆盖最低桶，尚不证明大图桶边界全部正确，不报告完整DrawBitmap完成。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功；产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未独立保存。

剩余非抗锯齿图元、effects、浮点目标等及原生上下文差分保持开放。
