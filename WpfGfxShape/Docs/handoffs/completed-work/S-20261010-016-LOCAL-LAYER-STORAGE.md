# 局部layer存储、surface寻址与非重复mask局部表面

生产活动layer记录Bounds并按父子交集分配局部数组，Lock返回局部stride，绘制clip取活动bounds交集。Direct3D9SoftwareRenderTargetSurface新增可选内部lockedBounds来源，保持设备span坐标、仅写入offset扣除原点，锁定大小校验按局部尺寸；默认调用保持原合同。EndOpacityLayer直接复制局部行并回合成恢复offset，嵌套pop恢复父bounds，Abort仍逆序丢弃不回调。

非重复AddImageMask按LayerBounds创建局部mask表面，采样与viewport几何变换转入局部坐标，effect资源矩阵恢复原点。重复tile/VisibleTiles仍全目标实现，不声明全部mask分支局部化。effect空间与color空间分离保持。

新增真实COM nested层深度1/2/5的锁失败、丢弃和重试像素验收3项，复用既有公共fixture。不宣称覆盖全部新路径故障注入或关闭完整旋转严格差分。原生依据沿用drawingcontext.cpp PushLayer/alphaMaskLayer offset已记录证据，无修改原WPF、ABI导出、golden或精度要求。

## 四次发布

命令均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`；项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64；共同产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始成功退出码0，全量1102/1；SHA256 199818477A9B903BABD95D9DAC3C26DA4E8C9A37186A6E560DBF0FD6B4DC6F14；报告wpf-calibration-21cdc1420b3e492ba5905c6c1587e00b.json；TRX未提取。
2. 局部layer成功退出码0，全量1102/1；SHA256 FBAE55FD8E4EBE0B60AC18C510C7E6D82DA72AEE61719C43136FFCAE56CBEE86；TRX c17c48ab-3d06-4102-8bf7-404d16318ea4；报告wpf-calibration-3fdf1a6909a74e12aace8257fba849d5.json。
3. mask局部表面发布失败，CS0128：ImageMask.cs局部left与既有sourceBox变量冲突；没有新成功产物，不用旧DLL身份冒充。退出码未单独读取。
4. 修正maskLeft/Top命名，发布成功退出码0；初次1102/1，报告wpf-calibration-1c8f068dc28540ae8b2063be9a6eb681.json。新增测试后nested定向成功，联合872/872、1s、退出码0；最终1105通过/1失败/0跳过，总1106、2s、退出码1；SHA256 E7D69870E73974BDFF619461D69DDB6E9FCEB98B28E3DE55B7B9D7A8E1CED3E4；TRX 1f103f83-069a-45a1-bc60-bc711858624a；报告wpf-calibration-130327c01a7b4dafa4b71c58a0ac2dac.json。

报告及固定com-acceptance.trx位于Tests/WpfGfxShape.ComAcceptance/TestResults/；定向TRX未提取，完整发布警告未独立保存。固定原版DLL/样本身份保持，float63/450、直接Pbgra32 9/180、格式判定差54均未减少。HDR、1e10、Vector4距离<1e-7未放宽。

局部layer绘制存储及非重复mask表面已有生产进展，完整方法族未关闭。下一轮合并重复tile/VisibleTiles mask局部surface、嵌套offset回退与失败首错清理、旋转最终采样差分；不可只复制像素或以补测作交付。阶段4 Active，不能替换原库。
