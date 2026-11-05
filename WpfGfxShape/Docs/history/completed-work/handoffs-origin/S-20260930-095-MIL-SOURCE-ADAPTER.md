# S-20260930-095：MIL源到WIC源生产适配

新增Abi/MilBitmapSourceAdapter.cs：8槽IWICBitmapSource包装，IUnknown/WIC源身份、自身引用计数、持有MIL源引用、元数据/调色板/CopyPixels转发。格式枚举转GUID按include/wincodec_private.h:143-149低字节规则；方法和所有权依据common/scanop/bitmapwrappers.cpp:99-205。拒绝没有已知位深的格式。

整数预滤波、格式转换和仿射消费三处统一通过包装，而非要求MIL源QI到WIC。因此现有转换/预滤波/仿射生产回归会实际经过新适配器。没有新增测试导出或虚构可写位图接口。仅实现源接口，不宣称可写位图反向包装、全部格式或缓存合同完成。

## 验证与发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

项目Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，成功。
产物：artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
全量131通过、0失败、0跳过，247ms。
TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
SHA256：`B2F56E42B170D1BD667A616D1E8A690796F84018CDBF4DFCF5FD0F587F7EEC3C`。
本次复用已有生产行为回归，没有新增仅IWGX外部对象的独立用例，也没有新行为Red。不能声称已独立验收任意外部IWGX-only实现。

## 状态

大模块仍未完成；无外部工具阻塞。内部目标/上下文、完整路径/曲线/组填充、原生ABI及完整格式矩阵仍未闭合。S-094已提供跨目标活动回调/首错/释放证据，不应继续泛称这部分完全没有证据。完整完成条件不降低，阶段和替换资格不变。
