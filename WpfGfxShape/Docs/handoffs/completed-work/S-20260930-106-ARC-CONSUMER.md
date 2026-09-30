# S-20260930-106：Arc消费接线

SoftwareArcConverter按原生utils.cpp:370-501、522-696实现端点转圆心、半径绝对值/放大、旋转、大小弧及扫掠、1至4段Bezier、退化线/点。SoftwarePathCoverage读取修正后的64字节Arc，复用已有HFD与有向边。非有限及数值不可表示条件明确失败。

真实Composition新增半径不足的半圆，半径从1放大至10，正反Sweep在目标内产生全覆盖与无覆盖准确像素；旧DLL定向失败、新DLL全量通过。未取得旋转/大弧/退化及边缘完整矩阵和原生差分，不宣称完整Arc兼容。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
SHA256 `A8DC22C9D7620B15E51BD6B2F896ECF1E93162F5341AB351B93AF5E66778A88F`。
中间Red未独立保存。

原完整模块仍未完成。通用内部目标/上下文及完整组合、边缘差分等门禁保持开放；没有工具阻塞。
