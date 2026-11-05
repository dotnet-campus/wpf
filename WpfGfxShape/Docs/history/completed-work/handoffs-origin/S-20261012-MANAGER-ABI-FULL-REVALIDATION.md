# manager时序与测试ABI修复后的实际全量复验

## 当前发布身份

实际身份测试：`dotnet test WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj --filter FullyQualifiedName~PublishedArtifactTests`，1/1通过，RunID `191fcf17-c86d-4f30-934f-f9b3ae87708e`。
产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`，SHA256 `B68BB90B6B920F53D04689B422C3FB4D411C04E5A2CFAFB01B9FBC15A3D0AC03`。
本次复验没有重新发布，不猜测上一发布命令或序号。上一执行普通4180/4180和COM定向46/46为用户提供事实，不冒称本轮执行。

## 实际COM全量

`dotnet test WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`：1275通过、1失败、0跳过、1276总计。RunID `7d52dc95-d9ef-4702-b064-bffacd46bec1`。
TRX `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`（后续运行会覆盖，RunID必须绑定本报告）。
校准报告 `wpf-calibration-a03f2362703c49ffa69a2b5deb06abb2.json` 实际回读：AotDllSha256与上文一致，FLOAT Mismatches=57/Compared=450，DirectMismatches=0/DirectCompared=180，AotBitmapMismatches=0。唯一失败为 WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements。没有改golden或阈值，没有新增COM回归。

## 生产边界

当前manager时序修复代码及测试ABI修复由本发布身份绑定。媒体命令仍在QI后E_NOTIMPL；自身Core/Av通知合同不等于真实系统sample与Visual闭环。不能把C#对象传给native CMilSlaveVideo注册或CD3DDeviceLevel1参数。媒体、shader、严格FLOAT及D3DImage剩余门禁仍开放。
阶段1–3 Closed、4 Active、5–7 Pending；Ledger RepairRequiredPartial；不能替换原库。
