# D3DImage dirty 完整像素回归复验（Partial）

## 本次实际修改

仅修改 Tests/WpfGfxShape.ComAcceptance/D3DImageTests.cs；未修改生产程序集。公开 COM 消费者回读完整12像素，验证五个不相邻dirty矩形保持间隙、包含矩形重复不触发union、第六个不同矩形形成union且不越过最后列。真实锁定surface导致Present复制失败时检查前缓冲完整像素不变及完成event；解锁后同一partition重试验证完整union像素。没有fake设备，没有内部调用冒充公开验收；Detach不计device-loss。

## 实际验证身份

定向 D3DImageTests + PublishedArtifactTests 已执行通过。先前宽筛选23/23含其他Published名称，不能计为D3DImage新增23实例。本次没有新增测试实例，只增强既有两种格式的公开像素合同。

最终全量：1238总计、1237通过、1失败、0跳过。RunID 9afbdd26-7ff5-447d-922d-2eb92213b973。TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。

测试直接读取生产 DLL SHA256：405831E98056BD193D3B071C6F74B5F606E9CFC86E0F73B7EE715CA47D152F76。路径 artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。本次未执行发布，不虚构发布序号或新产物。该身份与旧交接SHA不同，不沿用旧SHA。校准报告 wpf-calibration-d6de3679161c4a088bdcb59d75c3422e.json。唯一失败 WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements；未改golden/阈值。

## 原生与生产映射及调查

core/hw/InteropDeviceBitmap.cpp UpdateFrontBuffer → Abi/InteropDeviceBitmapExports.cs PresentLocked；AddDirtyRect → AddDirty；core/resources/d3dimage.cpp → Core/GeneratedD3DImageResource.cs；公开通道消费和event → Core/SameThreadChannel.cs；通知 → Core/Direct3D9DeviceManager.cs。

已读取真实原生符号 TryCreateDependentDeviceColorSource：SharedSurface同LUID使用CreateSharedColorSource并UpdateValidBounds；BitBlt使用CreateBitBltColorSource；Software不创建dependent。不存在本次查证的GetD3DColorSource/CreateDependentColorSource字面匹配，不能猜新ABI。

原生CMilD3DImageDuce::GetBounds直接读取interop，而非可用软件源。生产interop目前只有IUnknown vtable和快照消费者，没有据此实现独立bounds/HW dependent消费者。本次不声称实现。

manager GetOrCreateDevice持_syncRoot调用CreateDevice，后者CreateInitializedDevice在初始化完成前通知valid；NotifyAdapterStatus自身释放嵌套锁不等于释放外层锁。完整无锁通知改造、公开manager触发验收与并发注销仍需实现，未将调查记作关闭。

## 状态

D3DImage Partial；阶段4 Active。没有交付完整生产族，不能Closed或替换原库。HW dependent、独立bounds、真实availability/manager并发生命周期、media/glyph/shader/source17float合同仍开放，本次没有跳过并宣称完成。
