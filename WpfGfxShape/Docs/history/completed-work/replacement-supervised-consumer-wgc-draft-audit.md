# Consumer WGC 草稿独立审核：拒绝交付，验证门阻塞

## 1. 当前裁决

本轮接续用户指定的四文件草稿，完成独立静态审核，但**没有完成实施与验证，不交付草稿、不称功能通过**。未执行 build、publish、MTP 发现、测试体、consumer 或沙箱。没有委派、终端、脚本、生产修改或 Native 发布。

停止原因不是像素失败或 WGC 不可用：

1. 当前 `run_build` / `run_msbuild` 接口仅提供 targetPath、configuration、runtimeIdentifier、targetFramework，不能显式设置 DefineConstants、独立中间/输出目录、RuntimeFrameworkVersion 等属性。两个新 partial 默认由 `CONSUMER_WGC_DIAGNOSTIC` 关闭；普通 x86 Release build 不编译新代码，不能冒其已验证。没有把属性拼进路径、配置或 TFM，没有临时默认打开符号，没有改入口或添加构建驱动。`RunDotNetPublish` 接受完整发布参数，但不能代替用户要求的先行正常 build；本轮没有调用它，也没有测试或猜测其参数传递实现。
2. 草稿存在下述确定资源所有权问题，尚未修正。按代码必须在 build 前完整完成且不能未验证运行的约束，没有消耗唯一预算来试错。

需要的最小能力是：受支持的正常构建接口可显式传 MSBuild 属性，包含诊断符号与隔离目录；或用户明确批准另一种保持默认关闭、可由现有构建接口选择的诊断构建配置。当前没有自行创造配置、工程、脚本、Host 或入口。获得该能力后仍须先完成代码修复、资源规范及 ABI/依赖闭包核验，不能直接启动草稿。

## 2. 实际范围与读取依据

四文件沿用用户给出的临时范围：

- `Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`
- `Tests/WpfGfxShape.ComAcceptance/ComAcceptance.Package.targets`
- `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.ConsumerWgcDiagnostic.cs`
- `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.ConsumerWgcCapture.cs`

本轮对以上四文件**修改增量均为 0**；没有回滚或重写前轮草稿。项目仍是 `net10.0-windows10.0.18362.0` / SupportedOSPlatformVersion `10.0.18362.0`，两个 partial 仍默认关闭。targets 仍保留草稿增加的 ResolvedFileToPublish 两名称收集规则。前轮增量来自用户交接，不是本轮 git 差分认证；没有历史原文便不捏造精确 +/- 行数。

本轮仅新增本报告，并在唯一恢复入口和 current-work-item 顶部追加阻塞状态。生产、Tests 项目、global.json、原业务方法、公共捕获、NativeRun、ExitDiagnostics、资源文件及二进制均无本轮修改。

已完整分段读取适用 current-execution-rules、current-stable-baseline、完整替换方案、consumer-observability-audit（1–141）、consumer-observability-feasibility（1–132）、current-work-item（1–57）及四个草稿文件；返回截断部分作了小段补读。恢复入口读取当前顶部最新 KS 审计节，不声称读完其五百余行历史。AGENTS / instructions 名称检索没有适用文件。另只读本地 Directory.Build.props/targets、根及 ComAcceptance global.json、AcceptanceResources.resx、HD 协议表及相关现有私有 helper。没有委派图片或代码审查。

## 3. 投影 API 与版本理由/限制

使用公开 API 查询实际读取 `Microsoft.Windows.SDK.NET.Ref/10.0.18362.57`：

- GraphicsCaptureItem：FromAbi(nint)、Size、Closed；**不实现 IDisposable**。
- Direct3D11CaptureFramePool：CreateFreeThreaded(IDirect3DDevice, DirectXPixelFormat, int, SizeInt32)、CreateCaptureSession、TryGetNextFrame（空队列可返回 null）、Dispose。
- Direct3D11CaptureFrame：ContentSize、Surface、SystemRelativeTime 为 **TimeSpan 而非 nullable**；实现 IDisposable。
- IDirect3DDevice、IDirect3DSurface：均实现 IDisposable。
- WinRT.MarshalInterface<T>：FromAbi、CreateMarshaler 返回 IObjectReference。
- WinRT.IWinRTObject：NativeObject、AdditionalTypeData、GetObjectReferenceForType。
- WinRT.IObjectReference：ThisPtr、Dispose、As/AsValue 等。

这说明上述草稿调用的主要投影成员确实存在，不能凭印象把 TimeSpan 改成 nullable 或把 surface.Dispose 当编译错误。它**不是编译成功、引用所有权实现审计或完整 SDK ABI 认证**。首次查询泛型反引号名称未命中，随后按工具公开类型名称 MarshalInterface 查询取得泛型签名，不把未命中当 API 缺失。

18362 的选择与已有 feasibility 所列 CreateForWindow 最低 Windows 10 1903 一致；CreateFreeThreaded 更早可用，因此没有理由仅凭草稿升级 19041。查询精确 .57 只认证该包的公开 API；csproj 没有显式固定投影包版本，未取得 restore assets，故**未认证当前 SDK 最终解析版本就是 .57**，也未认证运行时 WinRT.Runtime 精确版本/传递闭包。TFM/SupportedOSPlatformVersion 不等于访问权限、策略或沙箱可捕获保证。

GraphicsCaptureItem.FromAbi 后直接 Dispose NativeObject 的方式仅涉及一个公开引用包装；不能由接口签名断言其所有缓存接口引用均已确定性释放。需要按实际 CsWinRT 实现审计完整引用所有权及事件注销，不以 GC/进程退出冒清理完成。Surface.Dispose 是 IClosable 语义，不是单纯 Release；它与 marshaler、frame 归还和 native texture 的次序需要实现级确认，不能因为可编译就声称正确。

## 4. 独立静态审核结果

### 4.1 确定缺陷：超时与所有权

- WgcThread 构造启动线程后仅等待 ready 五秒；若超时/初始化异常，调用方拿不到实例，构造失败路径没有停止队列并收束线程。
- Invoke 先入队，后检查剩余时间；即使 deadline 已到也可安排操作。超时只停止调用方等待，操作仍继续。
- `consumer = owner.Invoke(CreateWindow...)` 及 capture 构造同类赋值仅在返回后建立外部持有者。超时后操作若成功，新 HWND/WgcCapture 无调用方持有者，既没有迟到结果回收，也没有 owner 内注册统一清理。
- capture dispose Invoke 或 Join 超时后，外层 finally 仍继续 detach、销毁 channel、Release producer 和销毁 HWND；不能证明 capture 的在途工作已经停止，存在跨线程清理竞争。线程是 background 不是已收束证据。
- 异常记录 Record 自身可能抛出；Cleanup 和 WgcCapture.Close 的 catch 再调用 Record 没有保护，可能中断余下清理。构造 catch 中 Dispose 失败也会覆盖原始初始化异常。

必须改为 owner 线程内登记所有资源、迟到创建可回收、停止新工作与排空/终止状态明确、失败保留原异常，并在未收束时禁止声称 clean 或继续无条件跨所有权销毁。最终不可中止系统调用仍依现有监督兜底，不能谎称所有 API 可取消。

### 4.2 初始化失败与 GDI

- 标尺绘制同时取得 DC 与 brush，然后 WgcCheck 两者非零才进入 try；brush 失败时已取得的 DC 不释放，DC 失败而 brush 成功时 brush 不释放。
- GDI finally 先检查 DeleteObject，再 ReleaseDC；前者失败会跳过后者。应逐项尽力释放，不能把断言放在会截断剩余释放的位置。
- D3D/interop/连接返回值写入 stackalloc outputs，外部所有权变量在 WgcHr 后才赋值；需明确每个失败 HRESULT 的 out 参数所有权合同，不能假设失败输出可无条件忽略或释放。资源创建、AddRef/send 失败转移同样要逐项核实。
- HD 报告明确保留失败 send 的 duplicate 所有权未独立认证；草稿直接失败 CloseHandle 不能作为已认证释放协议交付。成功 separate send 后不再测试侧 close 的方向与 HD 一致。

### 4.3 D3D11/WinRT 薄桥

草稿使用 BGRA_SUPPORT=0x20、硬件 device、IDXGIDevice 桥、独立 wrapper/QI 引用、surface native access、staging CopyResource、同一纹理 DO_NOT_WAIT Map，Map 后 finally Unmap；纹理读完仍在 frame checkout 内。未见 producer staging 冒 consumer。

但本轮没有逐 IID/槽位对照真实 Windows SDK 头文件、没有认证 FromAbi 引用实现、没有编译或执行。不能把参照旧 DesktopDuplication 相同槽号当完整独立 ABI 证明。description 要求 format87/sample1/单 mip/array1、固定 crop 0,0/32,16；仍需记录/核 sample quality、实际容量和每帧几何连续性。CPU map 忙循环受本次截止约束，系统同步调用不可中止的限制保留。

### 4.4 观测合同：已有方向与未闭部分

已有：两个 target 预建 session/drain；固定红→蓝；QPC/100ns 使用 Int128 比较；门槛取 flush 返回后；锁定首个时间合格帧后才检查颜色；RGB 全 crop 精确比较且 alpha 单列；GDI 标尺独立不重叠；WGC 不自动真值、非同一物理扫描帧、观察者干扰均有记录。没有调用 RunHbConsumer/CheckFreshDesktopDelivery，保留原 AssemblyInitialize 和原旧测试。

未闭：

- WgcInspect 检查完整 client/window/DWM rect，却只保存 X/Y 等摘要，没有把全部原始 rect 写入证据；连续 HWND 生命周期、样本时刻尺寸/crop 和基线还需审计。
- 锁定帧后先读 DC，后保存 BGRA；若 DC/遮挡检查失败，首个合格帧的已读字节不会落盘。应先持久化捕获证据，再执行可能失败的后续观察。
- 首个错误 WGC 帧会停止，不会追下一正确帧，这是正确方向；若 A 失败则 B not entered，不得把“固定两次”报告为已完成两次。
- 内存负控只覆盖错色和时间谓词，没有覆盖 crop/format/身份或完整选择路径。未执行负控，不能称其已通过。
- 新异常/超时文本硬编码，与现有资源规范不符；需要明确资源修改属于批准范围后使用现有 AcceptanceResources，不新增第三方依赖或借无关资源键掩盖原因。

## 5. 条件编译、入口与 clean 包静态角色

源码精确入口：

`WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialConsumerTwoMarkersAreObservedByGdiAndWindowCapture`

方法为现有 [TestClass] partial 内的 [TestMethod]，两个文件均整文件包在 `#if CONSUMER_WGC_DIAGNOSTIC`。csproj 没有定义该符号。默认构建没有该方法；没有真实 MTP 发现，**UID unknown**。未来发现 filter 必须为 `FullyQualifiedName=` 加上述完整 FQN，等值而非路径/wildcard；只允许 list-tests，不执行测试体/consumer，不能沿用 feasibility 的旧实跑建议。

现有 StartupObject=NativeRun、MTP 正式入口及 AssemblyInitialize 保留；本轮无修改。静态名称和属性不是发现证据。

clean 包仅指现有 AfterPublish 的 ComAcceptancePackageDirectory，不是 PublishDir。根 wpfgfx_cor3.dll 是官方 WindowsDesktop runtime 资产；Native/wpfgfx_cor3.dll 是候选身份依赖，不能互换。本轮没有生成新 clean 包，也没有把历史 KS 包冒新 WGC 包。

草稿仅在 self-contained 路径按 ResolvedFileToPublish 的 Filename 收集 Microsoft.Windows.SDK.NET/WinRT.Runtime。存在以下未闭点：

- 不是经过 restore/deps/runtime 认证的闭包；没有失败即报错的预期依赖检查，名称筛选为空也可继续。
- AcceptancePublishedFile 的显式、RuntimePackAsset、投影来源汇合没有去重。未实际 publish，不能断言已发生重复，但不能交付为“已证无重复”。须按实际 PackagePath 检查重叠，并保留必要资源子路径。
- framework-dependent PackageComAcceptance 分支没有新增投影依赖；本轮目标是 self-contained，不能因此声称该分支也闭合。
- **clean 新增依赖实际清单：not produced / not observed**，不能写成已包含两个 DLL。无新 package-files、deps、runtimeconfig、SHA/PE/MVID 或总字节认证。

## 6. 真实预算与结果账

|项目|本轮授权上限|本轮实际|结论|
|---|---:|---:|---|
|正常 COM x86 Release build|1|0|未调用；无编译结果、warning/error 数量 N/A|
|COM x86 self-contained 10.0.13 正式 publish|1|0|未调用；无发布结果、warning/error 数量 N/A|
|Native publish|0|0|生产未改未发|
|Windows clean 包沙箱，仅 MTP list-tests + FQN 等值过滤|1|0|未发现；task/唯一节点/UID 均无|
|WGC 测试体 / official consumer / 标尺更新|0|0|未经授权执行|
|native-only Present / candidate / x64 / full / 重跑|0|0|均未执行|

前轮草稿阶段用户交接的 build/publish/沙箱均 0，本轮不改变。历史 KS 累计保持：沙箱4、精确运行请求3、发现1、实际 KS 体1、A/B各1、native提交2；历史初始 build1/COM publish1 保留，绝不挪作 WGC 预算。

## 7. 恢复条件与交接

状态：**Blocked — 草稿审核不通过，未交付可执行实现**。四个草稿文件留在原处供后续修正，不是批准使用的产物；不得直接编译默认分支后宣称 WGC 验证，也不得上传旧包/PublishDir 发现。

恢复前须明确提供受支持的诊断 build 属性传递能力（或批准可选配置）、必要现有资源键修改范围；之后在四文件批准范围内关闭所有权/线程/证据缺陷，确认 SDK/CsWinRT 实现和实际依赖闭包，再使用唯一 build。任一 build/publish 失败即停止，不修后重构建、不关闭分析器。成功后才能唯一 clean 包 list-tests，回读真实唯一发现节点/UID/filter。测试体、consumer 和内容/生命周期真实验证仍待另行授权，不把发现成功视为功能成功。

完整替换仍未完成；本报告既不裁决历史 KS 根因，也不宣布 WGC 能力或产品通过。
