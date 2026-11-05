# D3DImage 真实 Ex/shared HWND consumer 验收记录

## 实际修改

仅修改 `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.cs`，无生产源码修改，无原WPF源码修改。公开HW target测试从2个实例扩展到4个实例：保留D3D9 software选择两处窗口位置，新增Direct3DCreate9Ex / IDirect3D9Ex::CreateDeviceEx真实设备，A8R8G8B8(21)及X8R8G8B8(22)。Ex surface非lockable，严格断言共享与StretchRect能力，不以软件设备代替。

每个实例同一target先输出FF123456，两帧窗口像素00563412；随后同consumer修改destination width为16并提交真实dirty，surface填色FFABCDEF，窗口(8,8)严格断言00EFCDAB，(24,8)严格断言0，确保旧consumer内容没有残留。再以DPI192的第二真实InteropDeviceBitmap替换D3DImage资源输入，公开协议转移引用，dirty/present后仍严格断言00EFCDAB。该DPI变化只证明输入替换，不证明独立bounds consumer。

## 原生选择证据

原版 `core/hw/InteropDeviceBitmap.cpp:178-243`：Ex + Caps2 CANSHARERESOURCE + DevCaps2 CAN_STRETCHRECT_FROM_TEXTURES选SharedSurface；BitBlt仅pre-Vista且GetDC/ReleaseDC成功。保持此选择，不为Windows10测试强制BitBlt。

真实TRX记录Windows NT10.0.26200.0，Ex=True，格式21/22，Caps2=E0020000，DevCaps2=00000051，两项能力严格断言通过。生产选择入口 `Abi/InteropDeviceBitmapExports.cs:207-226`，HW消费者 `Core/GeneratedHardwareVisualRenderer.cs:78` → GeneratedD3DImageResource.TryCreateDependentPipelineColorSource → TryCreateDependentDeviceColorSource。共享创建使用实际CreateTexture shared handle，非修改生产选择器。测试没有internal造transition，没有把detach计作availability/device-loss。

## 发布与测试

发布序号1，完整命令 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。成功；Release/win-x64 Native AOT共享DLL `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。无生产修改，因此SHA保持 `A4D41108E97D25DC642CE978C678E07499BE6428D51602980066129AB5F3BFE3`，由PublishedArtifactTests真实计算，不宣称新生产修复。

发布后定向筛选 `FullyQualifiedName~D3DImageTests|FullyQualifiedName~PublishedArtifactTests`：13/13，RunID `d2a24142-7f08-4076-9c81-13693ffe7420`。

发布后完整测试：1242总计，1241通过/1失败/0跳过，RunID `62778da8-166a-462e-a4d1-c0b422e98228`。唯一失败仍为WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements，不修改golden或阈值。报告 `wpf-calibration-4381adb5d4f7472f97161ab67656351e.json`，TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。SHA由该全量再次计算。

较早筛选 `FullyQualifiedName~D3DImageTests|FullyQualifiedName~Sha` 意外匹配命名空间WpfGfxShape，实际跑全量1242/1241/1；不计作定向证据。最终定向使用准确PublishedArtifactTests筛选。

## 状态与审查发现

这不是用户授权的整个D3DImage族完成交付，不晋升Closed，也不宣布media/provider、shader或source17float已实施。本轮没有完成真实availability/manager重入和loss验收；不以测试增长抵充生命周期闭环。审查发现公开HardwareRenderTargetExports.Target直接Objects.CreateDevice，并未通过Direct3D9DeviceManager创建，这是继续接通真实manager通知时必须按原生合同处理的生产入口，不能用内部NotifyInteropAdapterStatus补测试。Independent bounds及brush/transform clip亦未被本轮DrawImage destination裁剪断言替代。

阶段1–3 Closed，4 Active，5–7 Pending；原机器ledger状态不修改为Closed，不能替换原库。该记录只保存实际已执行证据，不宣称整个阶段4完成。
