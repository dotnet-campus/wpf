# S-20261009-237：分数tile覆盖贡献合成

## 实际生产修复

建立半透明红色DrawingImage、内部绝对viewbox子区域和(2.5,2.5)分数四tile窗口。普通像素为0x80800000，交界像素在旧SourceOver子tile合成下变为1768488960，精确像素Red。初始夹具未选择内部viewbox导致透明区域影响，先修正为(0.25,0.25,0.5,0.5)，相同半透明预期仍Red，排除source padding。

Direct3D9SoftwareImageRenderer.AddVisibleTileMask不再将互不重叠的tile分区当多层SourceOver图像；读取每个目标尺寸子bitmap的premultiplied覆盖贡献并累加。32bpp逐分量饱和，float逐分量累加，保留HDR值；WIC读锁、首错返回和临时资源finally释放保留。同一brush不同tile的区域贡献应共同覆盖一个像素，而不是相互衰减。最终普通及四tile交界均精确0x80800000。

当前仍先量化每个32bpp贡献再累加，1LSB舍入、任意旋转分区及float合同尚未独立验收，不称完整原生过滤合成直译。

## 原生读取与范围

本轮读取ViewportAlignedIntermediateRealizer.cpp 610–708，确认一维tile/flip分派、基础tileceil尺寸、base→surface比例及偏移初始化。未读取完整后续CalculateSurfaceSizeAndMapping1D和source clip过滤消费者，不能宣称完成全部像素对齐/padding合同。

## 验证

起始716/716。新增1项精确半透明分数接缝行为Red；生产修改后定向144/144通过，全量717/717通过，0失败/跳过。整数tile、1e10fill/stroke/mask及既有回调生命周期共同回归通过。没有增加测试专用ABI。

## 发布记录

三次命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，目标WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始成功，11行日志；全量716/716，起始独立hash/TRX未保留。
2. 覆盖累加修改后失败，57行截断日志未显示编译错误；随后Release build诊断CS1686：已取地址局部pixels不能被lambda捕获。该次没有有效新发布产物/hash/TRX。
3. 改为捕获独立指针和stride值后成功，60行日志，仍有CA1416。定向及全量Green；SHA256 `58E1108B7FA981802CB92E6118ABE4686D731584192994B6BB1147459B84BBE1`，TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，Run ID `0e72d408-dee7-4d1f-bc68-e44d7f8111d4`。TestContext确认实际加载该产物，定向TRX被全量覆盖。

## 未关闭与下一整块

本轮关闭已证明的半透明四tile SourceOver衰减错误，不仅增加测试。完整采样空间像素对齐、过滤padding、任意旋转分区与未量化统一合成仍未关闭；可见跨度超过viewport仍走旧完整tile。下一轮必须联合实现共同精度的采样/过滤/分区生产路径，不能仅继续单色或单参数测试。

阶段4Active、Ledger RepairRequiredPartial；完整intermediate原模块、MSVC ABI、层栈/UCE与PresentationCore E2E仍未关闭，不能替换原库。
