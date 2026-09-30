# S-20260930-118：内部目标视图与基类行为

第1项部分完成：新增独立InternalView主表及creator基类表，拥有者为公开位图目标，共享引用计数与IUnknown身份。Bounds/Clear/GetNumQueuedPresents转发真实目标方法，GetType返回原生SWRasterRenderTarget=0x400，设备矩阵为BaseRT默认单位矩阵。主表不复用公开位图表，QI返回稳定内部视图。

真实DLL两个InternalTargetContract测试由Red转Green：查询内部接口、统一身份、释放公开目标/工厂后Clear仍写入保活位图；扩充GetType及矩阵对角线断言，全量通过。

## 未完成范围

DrawPath/DrawBitmap/BeginLayer等内部专属方法仍E_NOTIMPL；EndLayer无活动层返回无效调用，Abort无层可清理。Creator方法未实现。不能将本视图宣称完整IRenderTargetInternal或大模块完成，GenericTarget尚未经内部DrawPath接线。后续必须实现上下文/形状/画刷合同。仅当前x64构建验收，未证明跨架构C++多继承兼容。

## 发布流水

发布1、2命令相同：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

两次Release/win-x64成功。发布1后内部测试及全量通过；发布2修正GetType从0x401到原生0x400并增加类型/矩阵验收，最终全量成功。
产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，覆盖中间结果。SHA256未提取。

原生依据InternalRT.h、intermediatertcreator.h、wgx_render.h、BaseRT.cpp:60/215、swsurfrt.h:151。完整模块不关闭。
