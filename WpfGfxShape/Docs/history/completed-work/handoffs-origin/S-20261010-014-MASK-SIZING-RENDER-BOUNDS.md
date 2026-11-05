# Visual mask sizing/render bounds分离（未关闭）

原生证据仅src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/drawingcontext.cpp 2860–2918、2934–3067、3070–3180、3200–3248、5170–5204，每次小于400行。CreateAndFillLayer使用inner bounds作brush sizing，实际rcWorldSpaceBounds为无限；PushLayer再以变换后的内容bounds与clip交集限制layer。render options在退出Visual最后才Pop，不应猜测mask默认忽略Visual scaling。

生产GeneratedImageMask.TryCapture增加内部infiniteRenderBounds参数并传递至局部tile递归；CaptureMaskLayer区分sizing与实际render bounds。GeneratedVisualRenderer对mask layer按内容device bounds floor/ceil与Visual clip取交集，几何fill/stroke仍使用有限render bounds。没有新增导出、修改COM所有权或golden。完整layer offset/非矩形clip合同仍未关闭。

三次发布命令均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`，项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64，均成功、退出码0；共同产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。完整警告未独立保存。

1. 起始1102/1；SHA256 63F8A80B5C9EE5E56A74022513696BEBB017062EB26CA43016A3C04B8F6FFA78；报告wpf-calibration-70b1717950e54755a6d09ed16fa61d2a.json；TRX未提取。
2. sizing/render分离1102/1；SHA256 2598469F42C24106FE5C955B854DDA716DA8ADB4827CB2EEF087FA2C05D0F2FE；报告wpf-calibration-2dfeebc4f5db4c9abf0f469c60831870.json；TRX未提取。该发布联合869/869、2s、退出码0。
3. layer整数clip接线最终1102通过/1失败/0跳过，总1103、2s、退出码1；SHA256 4764A0F2E43A22027B870907DCC9F74CB4613351D3072D369937B16E94AE4B1F；TRX 62bfbd47-8dc3-4186-bee8-7e12ad7c5c4d；报告wpf-calibration-ad8d40d2485245a3b7e1e82e429f1d22.json。发布3未另做定向，不混用发布2的定向身份。

报告及固定com-acceptance.trx位于Tests/WpfGfxShape.ComAcceptance/TestResults/。严格差分仍float63/450、直接Pbgra32 9/180，格式判定差54；固定原版DLL/样本身份不变，HDR/1e10/Vector4距离<1e-7保持。

该生产边界修正没有减少差分，未满足完整旋转闭环交付。下一轮必须合并layer局部坐标/offset、mask采样和float旋转合同及真实失败生命周期，不再把render options作用域猜测当成修复。阶段4 Active，不能替换原库。
