# S-20261008-182：空BitmapSource修复发布

发布#2：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，退出码0成功。产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

测试前立即读取可用日志1-10及53-60行：日志工具共60行；原输出409681字符，前389681字符未显示，仅保留末尾20000字符。可见CA1416警告、Generating native code、创建native lib/exp及publish成功。不能声称完整诊断均已保留；本文保存取得的摘要，SHA/TRX待本发布验收取得，见后续S-183/184。

生产差集：GeneratedImageMask.TryCapture在映射前识别GeneratedBitmapSourceResource.Bitmap为0，返回成功的空快照。与无Source资源使用相同空遮罩/空填充消费者；不获取源引用、不改变Stdcall或私有ABI，不吞异常，不放宽未知ImageSource实现。原生imagebrush.cpp DoesContainContent及bitmapres.h HasContent为依据。
