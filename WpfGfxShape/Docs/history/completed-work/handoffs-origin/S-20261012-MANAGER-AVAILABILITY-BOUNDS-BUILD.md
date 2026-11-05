# manager availability / bounds 编译恢复与真实公开验收

## 交付边界

阶段4 Active，D3DImage Partial；本次未完成全部生产验收，不宣称Closed或可替换原库。最终生产及测试代码均已编译，未保留未测试代码修改。

## 修复与真实证据

- 当前CS1503准确定位到 GeneratedHardwareVisualRenderer.cs：Direct3D9PointAndSizeRect 的四字段为 int，Direct3D9BitmapDrawState.SourceRect 接受该整数矩形。删除误加的 float 转换，直接传入已构造的 sourceBounds；没有盲cast，没有更改共享类型、WorldToDevice或设备clip合同。
- 公开HW target当前确实持有manager；Target构造调用 Objects.CreateHardwareTargetDeviceManager / Objects.CreateDevice(... manager: Manager)。本次未恢复旧直接路径。
- 增强现有真实公开HW target用例：真实D3D9/Ex surface、公开资源命令、真实窗口GetPixel；目标销毁由channelDestroy/disconnect触发manager false通知，version=8的生产availability callback内调用公开Detach和COM AddRef/Release。四组窗口位置/格式/shared组合均通过，每组断言收到一次false通知。
- callback使用每个测试实例的局部delegate闭包，GC.KeepAlive保活；未使用internal fake、测试专用导出、自定义程序或脚本。
- 原有两帧、consumer矩形变化、dirty颜色变化、DPI192真实输入替换/shared窗口像素均继续通过。替换发生在callback之前，不冒称callback内替换。

## 锁与引用审查

Direct3D9DeviceManager.ManagerLock.Dispose在最外层深度归零时捕获并clear pending通知，finally退出manager锁，之后Dispatch。BitmapState.NotifyAdapterStatus在Gate内CAS获取非零COM引用，退出Gate后执行callback，finally释放保护引用。Dispose先注销订阅，再于Gate内标记disposed并清空身份字段后释放资源。上述为代码审查事实；当前测试只证实公开目标关闭通知及callback内detach/平衡引用，不证明穷举并发交错。

## 本次发布 P1

- 完整命令：dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64
- 配置 Release，RID win-x64，生产 Native AOT Shared DLL；发布成功，退出码0。此前同项目Release/win-x64 build成功。
- 产物：WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll
- 实际加载SHA256：8223CF0A171C8E1BC75EB6366798DE8C093EAE6D74278BC231600986BD06D6F9
- 发布后仅增强测试，未再次修改生产代码。

## 验收身份

- 定向：FullyQualifiedName~D3DImage|FullyQualifiedName~PublishedArtifact，13通过/0失败/0跳过；RunID 31763cf8-f49a-42b3-abf5-ed9e6307188e。
- 全量：1241通过/1既有tile严格校准失败/0跳过/1242；RunID 7e3d8f2d-e5e6-4411-9dd4-63666d039833。
- TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx（运行器固定名称，全量覆盖定向；定向RunID记录于本归档）。
- 校准报告：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-c432e8b4257b4679a0ec410c0c9d27ea.json，AotDllSha256与P1一致，Compared=450、Mismatches=57、DirectMismatches=0、AotBitmapMismatches=0。
- 唯一失败：WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements，ReferenceCalibration.cs:210。golden/阈值不变，未通过跳过或弱化断言隐藏。

## 尚未验收

系统callback内替换/关闭及并发注销、真实device-loss/recover/fallback完整矩阵、独立source bounds公开输出及全部原始source/world/device合同差分仍未完成。目标析构false通知不冒称真实GPU device-loss；callback内detach不冒称fallback恢复。异常clear顺序及并发清理完整证明仍开放。后续必须接续同一闭环，不重复领取已编译修复和shared窗口回归。

## 映射

core/resources/d3dimage.cpp → Core/GeneratedD3DImageResource.cs；core/hw/InteropDeviceBitmap.cpp → Abi/InteropDeviceBitmapExports.cs；core/hw/d3ddevicemanager.cpp → Core/Direct3D9DeviceManager.cs / Core/Direct3D9Objects.cs；公开HW消费者 → Abi/HardwareRenderTargetExports.cs / Core/GeneratedHardwareVisualRenderer.cs；实际验收 → Tests/WpfGfxShape.ComAcceptance/D3DImageTests.cs。
