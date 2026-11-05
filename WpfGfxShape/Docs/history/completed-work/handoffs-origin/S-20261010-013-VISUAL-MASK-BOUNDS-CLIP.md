# Visual mask消费者bounds及自身clip接线（未关闭）

GeneratedVisualRenderer.Capture对所有ImageBrush Visual alpha mask取得local content bounds，不再仅在相对viewport或RelativeTransform时取得。先计算Visual有效矩形clip，mask捕获与content共享该clip；保持layer栈顺序及非矩形clip原路径，不宣称非矩形mask完整等价。该修改补齐绝对viewport intermediate消费者bounds，未修改COM所有权或golden。

原生证据仅src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/drawingcontext.cpp 2830–2865：CreateAndFillLayer使用Visual inner bounds，opacity mask位于Visual内部、共享transform/clip。每次少于400行。

三次发布均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`；项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64，成功、退出码0。共同产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。完整警告未独立保存。

1. 起始1102/1；SHA256 F18C12E09EAFF2C1D346624CA90EDFFECFCA8C20B7BB5D2AED0F66E4A2E17C80；报告wpf-calibration-33e282ad7c644718955ce5b1aff036a6.json；TRX身份未提取。
2. 绝对mask bounds接线1102/1；SHA256 D92877E12313F2191C39731309A1EB564298A56F3ADC7C983EF3EE8E8C714D1F；报告wpf-calibration-f5c0629001074e55ae265daca6a081a8.json；TRX身份未提取。
3. 自身clip接线后联合定向成功（本轮未读取计数，不混用前轮869身份）；最终1102通过/1失败/0跳过，总1103、9s、退出码1；SHA256 63F8A80B5C9EE5E56A74022513696BEBB017062EB26CA43016A3C04B8F6FFA78；TRX 07e35683-afad-4f3c-b8ea-e4fdc6b3f102；报告wpf-calibration-1dd52ef1342e46c293beab2a6ada1bc9.json。报告及固定com-acceptance.trx位于Tests/WpfGfxShape.ComAcceptance/TestResults/。

严格差分仍float63/450、直接Pbgra32 9/180、格式判定差54，没有减少。固定原版DLL/样本身份不变；保持HDR、1e10、Vector4距离<1e-7。本轮有实际边界接线但未完成要求的大块旋转闭环；下一轮合并Visual mask层级render options及最终采样、float旋转覆盖和新路径失败生命周期，不能把新增局部修正算整族关闭。阶段4 Active，不能替换原库。
