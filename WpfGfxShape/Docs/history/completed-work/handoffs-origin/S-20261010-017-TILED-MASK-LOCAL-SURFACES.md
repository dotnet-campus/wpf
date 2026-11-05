# 重复tile及VisibleTiles mask局部surface推进

AddTiledImageMask按LayerBounds创建重复采样局部面，tileToDevice经toLocal进入局部坐标，effect matrix恢复原点；directConsumer仍直接复用原tile映射。AddVisibleTileMask累加面同样局部化，GeneratedImageMask.LayerBounds递归传递给VisibleTiles各子分区，子面与累加面使用相同边界。保持float贡献相加，不恢复提前字节量化；新增子锁pixels/stride/size校验，失败首错返回并finally释放。普通未指定LayerBounds调用保持目标范围。

新增4项Tile/Flip与嵌套深度1/2/5的真实COM锁失败/重试回归。活动layer局部存储、父子交集和offset恢复沿用上一轮。完整旋转差分未关闭，未宣称全部新分配故障注入完成。原生依据沿用drawingcontext.cpp的layer/alphaMask offset证据，不修改原WPF、COM导出或golden。

## 三次发布

完整命令均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`；项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64；三次成功、退出码0；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。完整警告未独立保存。

1. 起始1105/1，总1106；SHA256 E7D69870E73974BDFF619461D69DDB6E9FCEB98B28E3DE55B7B9D7A8E1CED3E4；报告wpf-calibration-6a2cfec2ff8f404a8bac839a780f1a08.json；TRX未提取。
2. 局部tile/partition面1105/1，总1106；SHA256 DDB640D7BFAD2D9F0C31FF2F7E4F87AA436B60A9022ABF992C02DF2011FE9061；报告wpf-calibration-632f47c493c149a4b8ecdd06813712dc.json；TRX未提取。
3. 子buffer验证与新增测试后联合876/876、1s、退出码0；最终1109通过/1失败/0跳过，总1110、2s、退出码1；SHA256 A4DFC48BC611CED6504ECE3524DF3030FD131664A4C856A162CA47CF56F6AFA5；TRX a8824a9f-7d93-4181-aaec-bd5c603300d3；报告wpf-calibration-28365c5af9d0475d970258252d9119f3.json。报告及固定com-acceptance.trx在Tests/WpfGfxShape.ComAcceptance/TestResults/，定向TRX未提取。

float63/450、直接Pbgra32 9/180、格式判定差54未减少；固定原版DLL/样本身份不变；HDR、1e10、Vector4距离<1e-7保持。重复mask全目标分配分支已有生产移除，但完整局部layer/旋转方法族和新增路径失败生命周期仍未正式关闭；下一轮应聚焦真实剩余mask采样合同/float旋转差分及端到端故障释放，不再以局部表面接线或补测为默认交付。阶段4 Active，不能替换原库。
