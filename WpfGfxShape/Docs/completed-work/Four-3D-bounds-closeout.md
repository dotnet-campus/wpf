# MIL3DCalcProjected2DBounds：有限 Green

最终身份、原始报告和运行命令见[四项集成档](Four-exports-integration-closeout.md)，候选SHA627F2732…，不是历史currentstage SHA。

本次最后测试修改后重新发布测试包并实际调用：24组ProjectedBounds的HResult与4个float位全部24/24精确一致，ProjectedBounds.Null1/1一致；覆盖clip、跨near/far、零/负extent、w=0/负、NaN/Infinity、透视、旋转/平移及scale。原版为独立10.0.1 SHA94C06484…。

Closed仅限这些公开调用位级合同；完整矩阵/消费者、跨架构与PresentationCore E2E不外推。strict Missing6/Extra0已测，不代表全ABI Green。本次无生产修改，验证后不再改源码。
