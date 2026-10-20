# CK fill integer reconstruction

## CM接管时事实补正

CK已新增 `Code/WpfGfxShape/Core/SoftwareFillBezier.cs`（183行）及默认发现的 `CkFillIntegerTests.cs`（120行）；ComAcceptance以Compile Link直接编译该实现。生产目录存在代码不等于接入：未挂NativeShapeReader，无真实fill消费入口。CF有限行为保留，未修改stroke/shared flattener。

初版x64独立官方cubic→重建整数折线及舍入合计10/10为接管前记录，最后修改后尚未复跑，x86尚未跑；不认证为最后源码通过。最后源码双架构正式包已发布于 `artifacts/ck-fill-integer/tests-{x64,x86}/com-acceptance-package`，CM将复用，不重发生产扰动身份。当前默认测试为7个cubic样本及3个half-integer舍入样本。

## 范围与限制

仅专用工具与Windows沙箱正式C# exe，无终端或直接间接脚本、新Host。实现接收28.4坐标；测试在真实官方WPF中分别栅格化cubic及该实现输出的折线，独立检查非空及零差。该控制不能证明候选DLL已消费算法、父设备矩阵及普通Path本地float阶段正确，也不是完整HFD/clip/边界证明。

## CM最终复跑补完

复用最后正式包，x64 task `1b773132806d4a749087456fba3c2080`、x86 task `44924e3a4a2d415eb23ca3320966d340`；两架构均10/10/10/0（total/executed/passed/failed），BL监督Exit0/exited，无timeout/cancel/kill/drain异常。两父TRX各80行分段全文亲读，14份oracle结果/身份末节直接精确路径亲读，均非空/threshold0/differences0；三舍入样本包括负半格通过。初版未复跑事实已由此次最终包运行补齐，不再只借旧10/10。

测试SHA/MVID：x64 `950CF353047F5C710DB71B96187DC579583CF001953D0280C5B285397B60C50D` / `10df52bf-a1eb-4059-85cb-e373dab5b9f1`；x86 `8F089C4C3070CA9451FE7C909885A8E2AAF6F6BDC7E1ECDCFEB3FDC5CDEF1D10` / `4bdf53ae-139a-4d20-af9f-b0c6b8f3474a`。实际.NET10.0.13，官方10,0,1326,50504；官方模块而非候选渲染，算法由测试程序集直接编译执行。完整官方SHA、包输入候选SHA（非加载证明）、绝对TRX及亲核限制见[CM](replacement-supervised-integer-ownership-review.md)。源码包逐字节溯源、全部trace及原始像素独立重算未另认证。

完整回收 `artifacts/cm-integer-ownership/{x64,x86}/`；父TRX分别 `Results/cm-ck-x64/TestResults/child.trx`、`Results/cm-ck-x86/TestResults/child.trx`。本轮无代码修复、无生产入口挂接，CF不变。下一步父设备矩阵/普通本地float分阶段与真实fill限定接入；原44/10、nested/stroke和完整替换保持开放。不能因BU历史快照缺失停工，也不能把有限算法控制通过称生产替换通过。
