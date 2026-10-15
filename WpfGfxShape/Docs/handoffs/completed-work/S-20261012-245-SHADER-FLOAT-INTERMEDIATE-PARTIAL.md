# S-20261012-245：shader float intermediate（部分实现，阶段4未Closed）

## 生产修改

- Direct3D9TextureRenderTarget.TryCreate新增可选pixelFormat参数，既有调用保持Pbgra32。PRGBA128Float使用A32B32G32R32f，传递到CheckRenderTargetFormat、GetMinimalTextureDescription、TryCreateRenderTargetTexture与surface身份；不支持格式返回UnsupportedOperation，不尝试8位替代。
- Direct3D9SurfaceRenderTarget双向格式映射补齐PRGBA128Float/A32B32G32R32f。
- GeneratedHardwareShaderEffect依目标格式选择中间格式；未知目标拒绝，不将未知高精度格式默认降为8位。
- 新增2个普通测试：float surface格式身份与不支持中间格式失败输出。普通测试不是HW float像素验收，不代表HDR闭环。

## 实际发布与验收

发布序号1，命令 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。专用发布成功；项目net10.0，Release/win-x64。产物位于WpfGfxShape/artifacts/bin/WpfGfxShape/Release/net10.0/win-x64/publish/wpfgfx_cor3.dll（本轮未单独回读发布目录，实际生产DLL身份由COM报告取得）。

生产SHA256：36B5AF4787ACF6E9B01D0C1A92545B7596F4736F92AE26C8A1D2A02C6CEA839E。

- Release build成功。
- 修改后普通full：4184通过/0失败/4184（新增2例）。
- 发布后COMfull：1294通过/1既有失败/1295，无新增失败；RunID c06fdbc3-d855-4b58-b792-84a5c9eb080e。
- TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，已回读RunID。
- 独立报告：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-0837db3878d14f00ba3530632b073bec.json，已回读同SHA；FLOAT57/450、直接Pbgra32 0/180，AotBitmapMismatches=0。
- 本轮没有精准新增真实shader COM实例，不声称精准发现数量或新HDR成功。

## 明确未完成

padding/Ddx仍E_NOTIMPL；独立consumer bounds/inputscope/clipping、显式sampler/register/constant版本目标合同、系统D3DCompile自有shader公开颜色变换、malformed/type首错与失败逆序清理、替换/关闭/失败重试生命周期尚未完成。不得称为shader有限表收口或完整模块交付。source17float根因尚未推进。mappingledger/stableworkitemremaininggap未全部同步，不能声称统一闭环。

唯一继续动作仍为完整shader有限表生产消费者与真实COM验收，之后source17float严格同格式合同；不得重复领取本轮格式身份测试作为模块进度。
