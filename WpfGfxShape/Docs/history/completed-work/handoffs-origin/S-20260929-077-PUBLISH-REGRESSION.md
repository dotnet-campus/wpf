# S-20260929-077：按用户要求重新发布并执行行为回归

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标：Code/WpfGfxShape/WpfGfxShape.csproj；配置Release；RID win-x64。退出码0，发布成功。11行日志显示还原已是最新，产物输出至`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。这是增量发布，没有强制全量重编译；本轮没有生产代码修改。

## 发布后的真实DLL行为测试

执行WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj全量测试。退出码0，116通过、0失败、0跳过，日志测试耗时229ms。测试程序集Debug/net10.0，直接加载上述Release/win-x64生产Native AOT DLL，不引用生产托管程序集。

TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，记录执行时间2026-09-29 16:28（+08:00），SHA256 `90812A9279200F1A32B7E77550E838ADC8A4B1A4CD64502999A51259E7F545F9`。哈希与上次一致，不代表没有执行发布；本次发布日志和随后新测试记录已核对。

本轮仅按要求发布和验证，未实现新生产能力，不升级阶段。下一轮生产任务继续遵守current-work-item.md，完整Compose和替换资格仍未达成。
