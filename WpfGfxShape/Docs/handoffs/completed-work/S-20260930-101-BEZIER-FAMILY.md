# S-20260930-101：二次及连续Bezier消费

SoftwarePathCoverage统一Bezier/QuadraticBezier/PolyBezier/PolyQuadraticBezier，每组控制点使用当前起点；二次通过标准2/3升阶转换到三次，复用HFD细分；消除每个输出点临时数组，统一量化。Arc仍明确不支持。

新增真实弯曲曲线(0,0)→控制(1,2)→终点(2,0)，以及等价三次、PolyQuadratic单段、PolyBezier单段准确像素对照。最初预期错误地假定左右各40/64；按四段折线与子扫描交点上取整手工复算为42/64、46/64，对应0xffa70058、0xff00b748。四种表示均通过。多段连续/变换/极端控制点矩阵和原生差分尚未完成，不据此宣称完全兼容。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功；产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
最终全量回归成功，TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
SHA256 `F247C6D01E2B655DCDD41EE5D2798B20D866E6E6C1D8CB446E77984F6678D70B`。
旧DLL新二次用例失败；发布后先有预期差异失败，核算并扩充等价表示后通过。中间日志未独立保存。

完整模块仍未完成，弧段/圆角/椭圆/完整组合及通用内部目标/上下文缺口保留，无工具阻塞。
