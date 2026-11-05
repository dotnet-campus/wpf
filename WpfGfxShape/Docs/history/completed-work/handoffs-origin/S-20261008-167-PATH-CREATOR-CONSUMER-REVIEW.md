# S-20261008-167：路径创建者与消费者合同复核

## 交付边界

本轮完成一次正常 Native AOT 发布、直接生产 DLL 全量复验及真实调用链复核。没有修改生产代码或测试，没有新增生产能力，也没有关闭完整 ImageBrush 遮罩映射、层消费与 Compose 生命周期模块。不能将本记录视为完成路径翻译；没有确认外部阻塞。

## 发布与验证

- 发布 #1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
- 项目：`WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`；配置 Release；RID win-x64；TFM net10.0。
- 退出码 0，成功。发布后立即读取完整可用日志 1–11 行：还原最新、托管输出及发布目录可见，本次增量发布日志未显示警告；不能据此声称仓库没有诊断。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 实际验收目标：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`，直接加载生产 Native AOT DLL。
- 全量 281/281，失败 0，跳过 0，281ms，退出码 0。
- TRX：`70ed96a6-177d-4b01-be26-3966cabf54e6`；开始时间 `2026-10-08T18:50:00.2629782+08:00`。
- 测试记录的发布 #1 DLL SHA256：`6AA7D9154744CC25E2133CDF441B96684B9469FACEBCCA4E2F0980CFE45A65B7`，与 S-166 相同。
- 完整 TRX 留在 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；本轮没有后续测试覆盖它。测试日志明确提示覆盖了前轮同路径结果。
- 本 Markdown 是已读取日志及证据的摘要，不是完整 TRX 或原始日志副本。没有运行定向测试、覆盖率、原生差分、PresentationCore E2E 或 Ledger 修复/验证。

## 原生证据与真实生产调用链

以下证据均来自 `src/Microsoft.DotNet.Wpf/src/WpfGfx/`，单次请求不超过 400 行；长输出截断处另行读取所需范围。

1. `common/shared/RefCountBase.h:25-80`、`RefCountBase.cpp:18-55`：IMILRefCount 仅声明 AddRef/Release；CMILRefCountBase 不继承 IUnknown，构造引用计数为零且有虚析构，Release 归零后 delete this。不能套 COM 三槽，声明不足以作为完整 MSVC 对象布局验收。
2. `core/resources/BrushRealizer.h:39-115,105-135,136-220`：realizer 带虚析构、EnsureRealization、借用 brush/effects 访问和清理方法；基类还内嵌 LocalMILObject<CMILBrushSolid>。只声明指针前缀不足以分配完整对象。
3. `core/resources/BrushRealizer.cpp:97-190,190-245`：resource、immediate、solid 与 null 创建者 new 后 AddRef，输出转移该引用。CreateNullBrush 创建的是非空 immediate realizer 对象，而不是零指针。
4. `core/resources/brush.cpp:40-64`：资源缓存 realizer，首次 CreateResourceRealizer 获得缓存引用，每次 GetRealizer 再 AddRef 给调用者。
5. `core/resources/BrushRealizer.cpp:522-540`：resource realizer 借用 m_pBrushResourceNoRef，effects 有自己的释放责任；不能擅自增加资源强引用形成缓存循环。`838-877`：immediate SetMILBrush 经 SetRealizedBrush/SetEffect 设置对象，solid 使用内嵌画刷；完整持有语义仍需继续核实这两个 setter，不能据此猜测。
6. `core/uce/drawingcontext.cpp:5493-5550`：非 bounds 模式的空 brush 也返回缓存 null realizer 并 AddRef；资源画刷沿 GetRealizer 获取。`1157-1221`：FillOrStrokeShape 把 context、brushContext、shape、pen 和 fill/stroke realizer 传给 DrawPath，调用者负责 FreeRealizationResources 再 Release。`1233-1291` 的 bounds 路径也使用真实 realizer；不要把该分支清理顺序改写为未经核实的统一逆序合同。
7. `core/sw/swlib/swsurfrt.cpp:796-900,913-956`：有限/无限入口转发全部参数；无限以 null shape/pen/stroke 表示。先 UpdateCurrentClip，空裁剪退出；后锁目标；fill 先于 Widen/stroke，首错 Cleanup，无论失败都解锁并归一化 no-render HRESULT。
8. `core/sw/swlib/swrast.cpp:533-590`：软件中间目标创建者参与 EnsureRealization，软件 cache index 与 associated display 同传；先 GetRealizedBrushNoRef(false)，仍获取 effects HRESULT，然后才判断 realized brush 为 null 并退出。不能因 realizer 指针非空就判定需要扫描，也不能跳过空 brush 的 effects 失败。

## 托管真实差集

- `Abi/BitmapRenderTargetExports.Internal.cs:132-136`：finite/infinite 入口仍丢弃 brushContext、shape、pen、fill/stroke 身份，只给 DrawInternalPath 一个 hasBrush bool。
- `Abi/BitmapRenderTargetExports.Drawing.cs:80-96`：空裁剪和目标锁已接通，但非空画刷返回 E_NOTIMPL，尚无真实 EnsureRealization/brush/effects/shape 消费。
- `Core/Direct3D9SoftwareRenderTargetSurface.cs:505-577`：managed bool 调度保留 fill 在前、Widen/stroke 在后及 finally 解锁，但不是私有对象 ABI 接线。
- `Core/Direct3D9ImmediateBrushRealizer.cs` 是委托驱动的托管类，不是原生创建者输出的私有 MSVC 对象。
- `Core/GeneratedVisualRenderer.cs:178-214`：真实协议 DrawGeometry 有 pen 时仍返回 E_NOTIMPL；无笔画填充走值快照、几何覆盖及 DrawBrush，不经过上述私有 realizer ABI。281 个验收通过不能证明有笔画或真实私有路径闭环。
- `Core/SoftwareImageRenderSession.cs` 已明确自身是 managed lifetime boundary，而不是 IRenderTargetInternal ABI。

## 下一轮合并执行计划

唯一任务仍是完整 ImageBrush 遮罩映射、层消费与 Compose 生命周期模块，不另立“仅布局调查”或补测轮。

1. 从资源缓存、null/immediate 创建者及 drawingcontext 消费者实现私有对象生命周期；同时确认内嵌 solid、setter 所有权、shape/pen/BrushContext 和 MSVC 虚析构布局，禁止伪造对象验收。
2. 保留所有 finite/infinite 参数，接通软件 EnsureRealization、中间目标、borrowed brush/effects；非空 null realizer 必须能无绘制成功，且 effects 失败不能被隐藏。与真实 generated 创建者及生产消费者一起交付，bool 改为指针传递本身不算完成。
3. 同链完成 fill、Widen/stroke、共同扫描、层 Begin/End/Abort/Pop，保持首错、锁清理、调用者 FreeRealizationResources/Release；区分 generated 中间层和原生 alpha 目标层。
4. 合并曲线精确组合 bounds、非轴保持操作数及完整 tile/DPI/stretch/alignment/动画/浮点/预滤；不以矩形或保守 bounds 代替精确区域。
5. 直接 AOT 真实协议联合验证非空 null realizer、非空 brush 绘制、有笔画、失败后恢复、来源回调重入、删除/替换/销毁、调用者引用释放后保活和最终释放；现有 NaN/空区域/回调场景只作回归。
6. 每次发布即读诊断，测试覆盖前保留可取得的证据，关联 SHA/TRX，按实际差集同步映射及 Ledger；满足全部模块完成条件才关闭。

阶段 1–3 已关闭，阶段 4 Active，阶段 5–7 Pending；Ledger 仍 RepairRequiredPartial。本轮不变更映射实现状态，不声称已修复 Ledger，当前不能替换原 wpfgfx_cor3.dll。
