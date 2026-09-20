# wpfgfx 原生到 C# 文件映射

> 本文只记录当前状态和已确认事实，不记录逐轮增量、历史测试数字或下一步任务。
> 当前工作入口见 [`next-session-handoff.md`](next-session-handoff.md)，剩余缺口顺序见 [`remaining-gap-closure-plan.md`](remaining-gap-closure-plan.md)，完成历史见 [`handoffs/progress-completed-work.md`](handoffs/progress-completed-work.md)。

## 状态定义

- `Partial`：已有生产实现和测试承载面，但不表示对应原生文件已完整翻译。
- `Not started`：当前生产项目中没有可确认的对应实现。
- 本表没有 `Complete` 项；当前实现尚不能替换原 `wpfgfx_cor3.dll`。

## 当前总体状态

- 当前生产代码集中在 `Code/WpfGfxShape/Core/`，NativeAOT ABI 探针位于 `Code/WpfGfxShape/Abi/NativeAotAbiProbe.cs`。
- 已有实现主要覆盖 D3D9 加载、显示与设备管理、设备状态和资源、HW render target、bitmap/纹理、窄 glyph/text 路径及部分软件渲染。
- 所有已启动的原生职责均为 `Partial`。
- 生产 ABI、UCE/资源协议、完整图元管线和 PresentationCore E2E 尚未完成。

## 当前文件映射

表中的托管文件允许一对多或多对一；仅列主要生产承载面，不展开测试文件和辅助类型。

| 原生文件或职责 | 主要托管文件 | 状态 | 当前事实 |
|---|---|---|---|
| `core/common/d3dloader.cpp`、延迟加载辅助 | `DirectXModule.cs`、`DirectXSystemModule.cs`、`DirectXBindingInfo.cs` | Partial | 已有 D3D9 模块加载、入口解析和模块生命周期实现。 |
| `core/common/display.cpp`、显示快照职责 | `Direct3D9AdapterSnapshot.cs`、`Direct3D9DisplayDeviceSnapshot.cs`、`Direct3D9DisplayDriverSnapshot.cs`、`Direct3D9DisplayModeSnapshot.cs`、`Direct3D9DisplaySettingsSnapshot.cs`、`Direct3D9MonitorSnapshot.cs`、`Direct3D9DisplaySet*.cs` | Partial | 已有 adapter、display mode、显示集合与变更查询承载面。 |
| `core/hw/d3ddevicemanager.cpp` | `Direct3D9DeviceManager.cs`、`Direct3D9Factory.cs`、`Direct3D9HardwareCapabilities.cs`、`Direct3D9Level1DeviceTest.cs`、`Direct3D9MultisampleSupport.cs` | Partial | 已有设备创建、匹配复用、能力探测和 Level1 测试切片。 |
| `core/hw/d3ddevice.cpp`、`d3ddevice.h` | `Direct3D9Device.cs`、`Direct3D9PrimitiveVertexBuffer.cs`、`Direct3D9DefaultState.cs`、`Direct3D9DeviceEntryGuard.cs`、`Direct3D9DeviceDriverWorkarounds.cs`、`Direct3D9GpuMarker.cs`、`Direct3D9Buffers.cs`、`Direct3D9Objects.cs`、`Direct3D9StateBlock.cs` | Partial | 已有大量显式 D3D9 COM 委托、状态设置、资源创建、绘制及错误处理切片；三个 `StartPrimitive` 重载已保持 device-owned buffer identity、每次清空与精确 FVF，`EndPrimitiveFan` 已保持少于 3 顶点静默成功、triangle-fan 参数、原缓冲区保留与既有错误映射；`GetVB_XYZDUV2/GetVB_XYZRHWDUV8` 已分别返回稳定且隔离的 device-owned 泛型 vertex-buffer identity，保留已有内容且无 Draw/FVF/COM/device-entry 副作用；`GetSupportedTextureFormat` 已按 source/destination precision、alpha 与五个缓存 supported-format 字段完成精确选择及 unsupported HRESULT 边界；`DrawBox` 已按原生固定 8 顶点、36 索引、fill/depth/FVF/AlphaSolidBrush 状态、12 个 indexed triangles、首错恢复与 `HandleDIE` 边界完成核对；`GetNumQueuedPresents` 已保持 marker tested/enabled/WDDM 门控、成功 Present 计数驱动的首次 flush、队列超过 2 时的第二阶段强制 flush、仅观察到 marker 消费后报告活动数量以及零输出/零额外引用边界；`InitializeIMediaDeviceConsumer` 已把借用的底层设备恰好一次交给 consumer，由 consumer 自行管理保留引用，设备层不进入作用域且不增减 COM 引用；多个 capability accessor 已确认独立读取对应缓存 caps 字段且无设备副作用，原生设备类未完整翻译。 |
| `core/hw/d3drenderstate.cpp` | `Direct3D9RenderState.cs`、`Direct3D9TextureState.cs`、`Direct3D9TextureStageState.cs`、`Direct3D9SamplerState.cs`、`Direct3D9TransformState.cs`、`Direct3D9ScissorState.cs`、`Direct3D9MaterialState.cs`、各 shader constant state 文件 | Partial | 已有主要缓存状态、失败后重试和文本 capability/shader 初始化状态。 |
| `core/hw/d3dresource.cpp` | `Direct3D9Resource.cs`、`Direct3D9ResourceManager.cs`、`Direct3D9UseContextGuard.cs` | Partial | 已有资源注册、驱逐和嵌套 use-context 生命周期承载面。 |
| `core/hw/d3dsurface.cpp` | `Direct3D9Surface.cs`、`Direct3D9UntrackedSurface.cs` | Partial | 已有 surface 所有权、描述、锁定及释放后保护切片。 |
| `core/common/d3dutils.cpp`、`core/hw/d3dtexture.cpp`、`d3dlockabletexture.cpp`、`d3dvidmemonlytexture.cpp`、`D3DTextureSurface.cpp` | `Direct3D9Texture.cs`、`Direct3D9LockableTexturePair.cs`、`Direct3D9VideoMemoryTextureManager.cs`、`Direct3D9TextureDescription.cs`、`Direct3D9SystemMemoryReferenceTexture.cs`、`Direct3D9SystemMemoryUpdateSurface.cs` | Partial | 已有 minimal texture description、texture level、mipmap、lock/pitch/dirty、staging 与视频内存纹理生命周期切片。 |
| `core/hw/d3dswapchain.cpp` | `Direct3D9SwapChain.cs` | Partial | 已有 swap-chain 创建、Present、Resize 和引用生命周期切片。 |
| `core/hw/d3dswapchainwithswdc.cpp` | 无完整对应实现 | Not started | software-DC present context、兼容 DC/DIB 和 GDI presenter 未翻译。 |
| `core/hw/d3dgeometry.cpp` | `Direct3D9GeometryRenderer.cs`、`Direct3D9PrimitiveVertexBuffer.cs` | Partial | 已有 indexed/non-indexed 批次、顶点打包和设备绘制委托。 |
| `core/hw/hwpipeline.cpp`、`HwShaderPipeline.cpp`、fixed-function shader 职责 | `Direct3D9Pipeline.cs`、`Direct3D9FixedFunctionMeshShader.cs`、`Direct3D9Shaders.cs` | Partial | 已有部分 fixed-function/shader pipeline 状态发送和绘制恢复。 |
| `core/hw/hwpipelinebuilder.cpp` | 无完整对应实现 | Not started | 完整 pipeline builder、waffling 和 expanded vertex 消费链未翻译。 |
| `core/targets/BaseRT.cpp`、`BaseSurfRT.cpp`，`core/hw/hwsurfrt.cpp`、`hwdisplayrt.cpp`、`hwtexturert.cpp` | `Direct3D9SurfaceRenderTarget.cs`、`Direct3D9TextureRenderTarget.cs`、`Direct3D9DelayedBounds.cs`、`Direct3D9Software3DSurface.cs` | Partial | 已有 layer 捕获后透明清理失败的 source-bitmap 保留、上层单次释放与 capture 失败无释放边界，以及 opaque partial-capture 成功返回空矩形时跳过 capture/clear、source bitmap 为空并由 EndLayer 走 no-fixup 但仍恢复状态的边界，以及 display/window/surface/texture 目标的创建、Clear、Begin3D/End3D、Present、Resize、失效门控、窄 layer、软件 3D fallback，以及 texture render-target 的格式检查、固定 surface description、尺寸验证、cache-index 门控、不可驱逐纹理与 level-0 surface 创建/失败清理、派生析构、bitmap-source 引用生命周期、底层 texture 有效性与 no-ref 借用，以及对基础 pixel format、逻辑尺寸、device transform、shader pipeline capability、realization cache index、bounds/Clear/Begin3D/End3D、接口发现、hardware-raster 目标类型和 queued-present 查询的直接委托；三个描述访问器只返回基础目标已保存值，两个设备能力访问器只复用基础目标依据 device capability/cache token 得出的结果，D3D texture format 访问器只返回基础目标构造时保存的 target-surface format，associated display 访问器只返回基础目标构造时保存的 display index；ClearType hint 入口只更新基础目标保存的 force-ClearType 状态并保持 `S_OK`；cacheable bitmap-source 入口恰好一次复用现有 `GetBitmapSource`；`GetBitmapSource` 先清空输出并由窄 `GetBitmap` 取得独立引用，失败时保留首个 HRESULT 并释放临时 texture 引用，成功时直接转移既有引用而不额外 AddRef；窄 `GetBitmap` 自身先清空输出，首次成功后由 render target 保留缓存对象引用，每次成功调用在内容重新验证后仅对同一对象增加一个调用者独立引用，失败后可重试且调用者与 render target 的释放顺序互不提前终止对方生命周期；两入口保持相同缓存对象、内容重新验证和独立引用语义且不创建第二套状态；这些访问器均不读取纹理描述、不从 device adapter 或当前 display set 推导、不进入 device entry/use-context 或创建额外 bitmap-source/cache；六个现有窄绘制入口已在委托基础 surface render-target 前使 texture-backed bitmap-source 内容失效，并由后续窄 `GetBitmapSource` 或 `GetCacheableBitmapSource` 访问恢复有效状态；surface render-target 的 HW intermediate 对象创建重载在既有门控后原样转发 width、height、device、associated display 与 `ForBlending` 到 texture render-target 创建链，失败保持输出为空、首错传播、无 software 回退和部分候选清理，成功后才转移调用者所有权并提交 hardware-used 状态。 |
| `core/hw/hwbitmapcolorsource.cpp`、`hwdevicebitmapcolorsource.cpp`、`hwtexturedcolorsource.cpp`、`bitmapofdevicebitmaps.cpp` | `Direct3D9BitmapColorSource*.cs`、`Direct3D9BitmapReusableRealization*.cs`、`Direct3D9BitmapSystemMemorySurfaceSource.cs`、`Direct3D9BitmapTexturePopulationPreparer.cs`、`Direct3D9DeviceBitmapColorSource*.cs` | Partial | 已有 bitmap color source、realization、staging、device bitmap 更新和 cache 生命周期切片。 |
| `core/hw/HwBitmapCache.cpp` | `Direct3D9BitmapCache*.cs`、`Direct3D9BitmapFormatCacheEntry.cs`、`Direct3D9BitBltColorSourceCache.cs` | Partial | 已有格式缓存、候选复用、检索和生命周期承载面。 |
| `core/hw/HwBitmapBrush.cpp` | `Direct3D9BitmapBrush.cs`、`Direct3D9ImmediateBrushRealizer.cs` | Partial | 已有窄 bitmap brush 和即时 realization 路径。 |
| `core/hw/d3dglyphpainter.cpp`、`d3dglyphrun.cpp`、`d3dglyphbank.cpp` | `Direct3D9SurfaceRenderTarget.cs`、`Direct3D9TextPixelShaderResources.cs`、`Direct3D9Shaders.cs` | Partial | 已有硬件 painter、软件 fallback、ClearType/text shader 与 alpha texture 的窄路径；完整 glyph 私有模型未翻译。 |
| `core/sw/swlib/bilinearspan.cpp` | `Direct3D9SoftwareBilinearSpan.cs`、`Direct3D9SoftwareRasterizer.cs` | Partial | 已有部分 Tile/Flip/Extend/Border fallback、插值与 batch 推进。 |
| `common/scanop/scanpipeline.h`、`core/sw/swlib/swsurfrt.cpp`、`core/sw/scanpipelinerender.h`、`core/sw/swlib/scanpipelinerender.cpp` | `Direct3D9SoftwareIntermediateBuffers.cs`、`Direct3D9SoftwareRenderTargetSurface.cs`、`Direct3D9SoftwareRenderTargetBitmap.cs`、`Direct3D9SoftwareScanPipeline.cs` | Partial | 已有三个 `MilColorF` intermediate buffer 的单块有界分配、索引切片、代际与旧视图失效，并连接 surface 的 SetSurface/失败/重绑/Dispose 所有权；surface 另有 resize uniqueness、DPI device transform、锁定、基础 bitmap/glyph/path 绘制、surface-bounds current clip、CSpanSink 参数转发与 span 输出、`Pbgra32Bpp` 目标 alpha 判定及 glyph ClearType hint 决策，以及非 effect scan pipeline 的 owned operation、初始化代际、失败清理与 PPAA filler slot。完整 scan-op builder/effect/glyph 私有算法仍未翻译。 |
| `core/common/dwritefactory.cpp`、显示文本设置职责 | `DirectWriteFactory.cs`、`DirectWriteDisplaySettings.cs` | Partial | 已有 DirectWrite factory 和显示设置承载面。 |
| registry 与设备开关职责 | `Direct3D9RegistryDatabase.cs` | Partial | 已有当前使用的 registry 查询切片；WOW64 view 未实现。 |
| NativeAOT 导出 ABI | `Abi/NativeAotAbiProbe.cs` | Partial | 当前仅有探针级 ABI 和独立 Host/集成测试，不是完整生产 ABI。 |

## 当前未覆盖边界

以下内容当前没有完整托管映射；其分类、依赖顺序、进入条件和完成条件统一见 [`remaining-gap-closure-plan.md`](remaining-gap-closure-plan.md)：

- effects、UCE 和资源协议；
- 完整 glyph 私有模型与真实 shader bytecode 加载链；
- GDI presenter、software-DC present context、兼容 DC/DIB；
- `Reset/ResetEx`；
- registry WOW64 view；
- surface ScrollBlt 和 software dirty 通知；
- 普通 render-target dummy 解绑；
- 完整 layer stack、几何 mask/effect；
- 普通 scene/dummy back-buffer 生命周期；
- 完整 PresentationCore 到托管替代库的 E2E 调用链。

## 当前验证承载面

- 主测试：`Tests/WpfGfxShape.Tests/`。
- ABI Host：`Tests/WpfGfxShape.AbiIntegration.Host/`。
- ABI 集成测试：`Tests/WpfGfxShape.AbiIntegration/`。
- 最近稳定验证数字记录在 [`handoffs/progress-completed-work.md`](handoffs/progress-completed-work.md)，不在本文重复维护。
