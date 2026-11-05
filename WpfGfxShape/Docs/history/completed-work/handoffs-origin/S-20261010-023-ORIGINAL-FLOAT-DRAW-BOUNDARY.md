# 原版float DrawBitmap可达路径与限幅证据（未关闭）

现有WpfReference/FloatReadbackTests新增原版Release/x64 IRenderTargetInternal.DrawBitmap探针：factory创建26格式source/target，写锁输入HDR梯度，QI内部目标，调用DrawBitmap，WIC确认float格式并逐Vector4读回。宽6/10返回成功但红1.25/2.5变1、负绿变0，蓝呈sRGB量化梯度；不能作为scRGB HDR渲染golden。最终明确命名WhenOriginalFloatTargetDrawsBitmapThenSrgbClampingIsRecorded，不用AOT生成预期、不放宽生产HDR断言。原版探针3/3通过（存储1项、绘制2项），先前HDR假设失败已记录观察，不宣称HDR兼容。

原生证据仅src/Microsoft.DotNet.Wpf/src/WpfGfx/：core/common/InternalRT.h 30–115；core/api/api_rendercontext.h 20–119；core/api/api_renderstate.h 25–94；core/sw/swlib/swsurfrt.cpp 55–102。SetSurface源码按scRGB选float，与安装二进制探针行为不一致，版本/内部绑定/颜色源及私有ABI仍须核对。每次少于400行。

本轮仅一次生产发布：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`；项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64，成功退出码0；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll；未改生产代码，SHA256沿用CCD2C01F350C9E8BA1F3D3DFA2F0E945BFE9BD9C3D5E39AFCE40602162197D1D，最终报告头部本轮未读取不冒称新提取。完整警告未保存。

起始1111/1，总1112，起始报告完整名称未提取；最终1111通过/1失败/0跳过，总1112、2s、退出码1；TRX 2e0b35b4-a945-437c-b37a-14b8383a50a0；报告wpf-calibration-a7ed26e276b54c8aa4ddd6fd44a2e8aa.json，位于Tests/WpfGfxShape.ComAcceptance/TestResults/。严格float投影57/450、直接0/180基线不变。原版独立报告代码写入ReferenceResultsPath但本轮未独立取得产物，不能使用其作为冻结golden。

本轮没有生产方法族进展，未满足大块目标。记录真实阻塞：可达float存储不等于当前安装原版绘制scRGB；尚不能建立独立HDR DrawingImage参考，禁止以限幅观察替代HDR正确性。下一轮合并核对原版实际内部目标绑定/格式与同版本ABI、可达scRGB绘制及旋转DrawingImage/失败验收；严格失败保留。阶段4 Active，不能替换原库。
