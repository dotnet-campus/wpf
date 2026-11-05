# S-20261009-236：多tile可见source捕获与合成

## 生产推进

GeneratedImageMask扩展S-235的单tile路径：可见窗口每轴跨度不大于基础viewport且跨边界时，按实际相交的两个/四个tile建立独立capture。每个tile保留整数索引、负索引奇偶flip及brush transform的组合顺序，并以非重复路径按目标clip实现source。较小tile大量重复仍走既有完整tile路径；没有固定纹理尺寸阈值。

新增VisibleTiles owner及Direct3D9SoftwareImageRenderer的合成消费者，形成捕获→延迟source实现→目标尺寸bitmap合成→fill/mask消费者闭环。所有capture在COM回调前完成；捕获首错时逆序Dispose已经取得的tile，合成首错不继续下一个tile；临时bitmap/锁/COM source在finally释放。最终owner逆序释放子tile。不新增ABI或测试导出。

多tileparent用deviceTransform→tile→inverseBrush组合，递归恢复原brush变换后得到正确原始顺序；不是把tile直接插入parent后冒称非均匀/旋转正确。

## 验收

起始713/713。新增3项四tile可见窗口：viewport(2,2)、基础tile1e10、非对称source，Tile/FlipX及opacity mask。起始定向140通过/3失败（present E_NOTIMPL）；实现后142通过/1像素失败。剩余FlipX夹具错误地期待Y翻转，修正为左上透明/右下红；该次像素失败属于预期错误，不算新增生产修复。最终全量716/716，0失败/跳过（2s）。既有1e10fill/stroke及二维tile生命周期回归通过。

## 发布记录

三次完整命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始成功，11行日志；全量713/713。起始独立hash/TRX未保留。
2. 多tile捕获与合成后成功，60行日志；定向142通过/1失败，独立hash/TRX未保留。
3. brush组合次序修正后成功，59行日志；仍有CA1416警告。最终716/716；SHA256 `17CD3EFD0D4D794C90667AF16A7C2AACED1D9A56AFC190CB0096EA80DED254FF`，TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，Run ID `e1162111-8369-415b-88ec-82ebb4b3f2c9`，TestContext确认实际加载产物。

## 边界与下一完整任务

本轮复用S-235的rendered/base tile合同开展托管可见source差集，没有新读取原生完整CalculateSurfaceSizeAndMapping1D，不称完整原生方法直译。可见窗口任一轴跨度超过viewport仍走旧重复路径；分数边界的过滤padding、精确旋转parallelogram与像素对齐没有关闭。多个tile子bitmap采用SourceOver合成，分数边界coverage重叠/透明缝的原生语义必须下一轮确认并实修，不能仅凭整数采样通过关闭模块。

下一轮联合完成原生采样空间像素对齐、子tile过滤边界与透明padding合成合同；多tile旋转/非均匀及首错回调验收作为同一生产模块内部步骤，不单独留为补测轮次。阶段4Active、Ledger RepairRequiredPartial，不能替换原库。
