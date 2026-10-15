# S-20261008-185：双缓冲空内容消费者修复

## 生产差集与原生合同

依据imagebrush.cpp:112–152及doublebufferedbitmapres.h:1–67，GeneratedDoubleBufferedBitmapResource增加内部HasContent（底层对象非零）；GeneratedImageMask.TryCapture在mapping及Acquire之前对无内容双缓冲资源返回成功空快照。与BitmapSource空内容共用实际填充/遮罩消费者，不将Acquire失败误作空内容，不放宽未知资源，不新增ABI或测试导出，不改变拥有/释放顺序。

现有生产DLL验收clearSource恢复链同时使用真实95/96类型通道资源，覆盖绝对、相对及192DPI作用域几何透明和源恢复；原有锁失败/重试继续回归。Red见S-184。

## 本轮发布#2

命令 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`；目标项目同命令，Release/win-x64，退出码0。产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。测试前立即读取可用日志1–12及52–59：可见CA1416、Generating native code、native lib/exp创建及publish成功。工具保留59行日志，原输出409681字符，前389681字符未显示，仅保留末尾20000字符；本文仅保留实际取得摘要，不声称保留完整诊断。SHA/TRX待下述验收更新。

## 保留边界

这是原完整ImageBrush模块内部的内容判定修复，不构成大模块关闭；私有realizer ABI、非零有画刷Pen、一般曲线布尔精确bounds等仍未完成。未修改原WPF、slnx或Host，未运行覆盖率、原生差分、PresentationCore E2E及Ledger修复验证。
