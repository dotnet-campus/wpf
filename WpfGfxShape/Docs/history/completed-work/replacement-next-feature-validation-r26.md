# r26：桌面独立静态审计与既有几何资源门 / 有限托管通过 / OverallOpen

## 当前结论
A 未确定可最小修复的白像素验收根因；不改 fixture，不重跑同一白像素/WGC 场景。B 已实际执行现有 GeneratedGeometryResourceTests 全类原测试集，13通过、0失败、0跳过，专用工具返回 exit0，30ms。**仅托管纯代码有限覆盖，不是 NativeAOT、公开 WPF 渲染或整体替换通过。** 生产修改0，测试源码修改0。新增本文和测试日志抄存，更新恢复入口。

已先读 next-session-handoff、r25 当前收尾、r22/r13/r19必要结论，再读实际源码。r25不再派生外部流实验；负 Flush 通用有界恢复仍 Open，不擅做媒体进程隔离。full两份可信JSON未提供，不绕过默认门禁。

## A：原桌面 nativeControl 根因代码审计
直接读 D3DImageTests.VisualCache.cs（初始化/前置/cleanup/断言）、SwapChainObservation.cs、OutputWindow.cs、WindowThread.cs、GpuReadback.cs、PresentDiagnostics.cs、D3DImageTests.cs（字段/窗口销毁）、HhNativePrecondition.cs原提交段，以及 CandidateResizeDesktop/CandidateFixture.targets、ObservationDiagnostic.cs。未改生成文件。没有再推演已排除的vtable/device-vs-swapchain/GDI/WGC路线。

- 实际原初始化：Width32/Height16/Count1/SwapEffect1/Windowed1；Format未初始化即0（UNKNOWN），Flags0，不是lockable backbuffer。r13/r19记录运行时Format22（X8R8G8B8），本轮未重新动态认证；不能把另一个GpuReadback控制的Format21/SwapEffect3移绑原场景。X8的X不是需要合成的alpha，不存在本源码把clear alpha0误当透明的依据。
- PresentNativeControl先一次全目标Clear（count0/rectNULL/flags1/colorFF123456），断言成功，再一次原Present（源/目标矩形NULL、原window override），断言成功。没有后续Clear、额外Present或设置另一个render target的原路径代码。之后CompleteNativePresent执行EVENT query等待、消息协作、两次DwmFlush，不触发第二次绘制；它们也不承诺GetPixel一定反映DWM呈现。
- Microsoft D3DSWAPEFFECT文档明确DISCARD Present后backbuffer内容不可假设；GetBackBuffer(0)表示下一次Present的buffer，不是已呈现帧。故Present后GPU读回不能证明刚才提交的surface颜色。本原失败路径DiagnoseNativePresent/AssertCombinedPixel只读screen/window DC，**没有GPU backbuffer回读断言**，不能用“交换后读错帧”解释这个白像素门。Hh原控制已明确不采DISCARD后backbuffer；GpuReadback的验证在Present前且用COPY，非本门。没有发现本任务需修的误帧验收断言。
- HWND创建在专用STA owner上同步Invoke，类无背景刷，WS_POPUP|WS_VISIBLE及topmost；建立可观察窗口在CreateDevice/Clear之前。窗口初始32x16；当前失败发生在connect前，后面的真实resize/第二窗口业务尚未执行。DestroyWindow通过owner Invoke，位于释放device后的finally；原前置内没有销毁/重建/Reset/resize。未发现“尺寸尚未提交后立即采样”的确定代码链。
- OutputWindowProcedure实际截获WM_ERASEBKGND返回1、WM_PAINT调用ValidateRect(window,NULL)返回0，其他才交DefWindowProcW；不是DefWindowProc默认背景刷反复覆盖。ValidateRect文档语义仅移除更新区域，不绘制、不提交D3D、不保证DWM收帧。不能把未BeginPaint当成白填充根因，也不能把ValidateRect当成功呈现合同。没有确证需改此消息分支。
- windowThread为class静态；ClassInitialize创建、ClassCleanup Dispose/join并置null，D3DImageTests有DoNotParallelize。独立诊断r16Window/r18PresentReturn为静态，R16Begin在原Present前绑定window，r18PresentReturn在实际Present返回时写入，闭包传入本次window/device；日志用锁。它们不是通用并发安全设计，但没有原单用例隔离执行中交叉窗口覆盖的确定证据。不能由静态字段本身认定竞态并改生产。

官方正文已直接取得并阅读：
- https://learn.microsoft.com/en-us/windows/win32/direct3d9/d3dswapeffect
- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-validaterect

结论仅为本次定点源码审计未发现可证修复，不是已证明桌面系统正确。系统D3D9前置先失败，候选connect/render未到，不能靠改gfx解释。A桌面动态执行0，停止同白像素试探。

## B：结构盘点与功能选择
先调用结构工具：默认返回仓库Microsoft.Dotnet.Wpf.sln（不是本地小解决方案），随后亲读WpfGfxShape.slnx并取得两个目标测试项目文件清单，查看Tests各独立harness目录、项目与入口。SDK global.json请求10.0.107/latestFeature；生产和单测net10.0、Nullable开启，沿用现有LangVersion，未更改任何构建配置。

静态安全入口分层：
- WpfGfxShape.Tests：GeneratedGeometryResourceTests直接调用handle table/router/frozen geometry及固定常量断言；GeneratedBrushResourceTests有固定布局/packet常量；GeneratedDrawingImageResourceTests有独立golden bytes及通知测试。这些是纯代码层，不冒充公开System.Windows或Native。
- 公共WPF资源/几何/画刷/图像：Pcore/Program.cs存在candidate角色和固定背景白/矩形红断言，以及images分支。但candidate完成时仍标PendingIndependentComparator，不能把images运行无异常等同所有功能通过；不调用original或compare，也不选它绕过比较门。
- 文本：GlyphRunTests有candidate-only候选导出/真实DirectWrite字体入口及创建/截断/销毁断言，但不是已隔离公开WPF入口；没有本轮认证的Native包，不能借旧绿直接运行。
- GeometryUtilityTests明确调用系统reference和candidate比较，排除。CandidatePureVideo隔离入口仅视频，不扩展已通过媒体范围。

选择几何资源协议图，是跨多种几何、依赖事务/通知/生命周期的现有明确断言门，比再测视频或只运行布局单项更有价值。原文件已分段亲读各测试主体与末尾辅助（工具部分返回存在超长截断，不冒称317行逐字无遗漏）；保留13项原断言，无新测试/框架/oracle。覆盖冻结快照抗后续突变、循环拒绝与active set恢复、布局/包字段、强类型创建、六类几何更新、错误依赖事务不变、组子引用替换、合法path、非法count/backpointer/trailing拒绝、通知传播、跨channel复制后最终依赖释放、失败停止后续packet。不是光盘点或只运行一条新造断言。

## 当前Native输入与证据限制
直接列举 `WpfGfxShape/artifacts/candidate-media-source-bound-r22` 返回目录不存在；不能亲读r22 freeze/Source/source.sha256/native包/父子exit/模块SHA原件。r22文档列的SHA不作为本轮身份认证。没有挪用r13/r19/r22旧绿。选择允许的纯托管有限覆盖分支，**没有当前Native执行，因此没有为本任务重新冻结/发布Native，也没有冒称认证r22输入。** Native功能门仍待恢复实际输入或另行新冻结发布后执行。

## 唯一动态执行账与原件回读
- 专用test调用1：目标 `WpfGfxShape/Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj`，filter `FullyQualifiedName~WpfGfxShape.Tests.GeneratedGeometryResourceTests`。内含默认Debug构建，不是独立build调用。
- 单独build0、publish0、Windows sandbox0；桌面/WGC0、外部流0、原版gfx加载/复制/fallback0。没有终端/脚本，没有重试直到绿。
- 工具返回完整16行日志，随后read_last_log_lines 1–30实际取得全部1–16行，已亲读。抄存见 [测试日志](replacement-next-feature-validation-r26-test-log.txt)，保留来源性质，不冒称独立TRX。
- 真实工具报告退出码0；输出显示生产托管assembly和测试assembly来自当前工作树Debug/net10.0，再对该测试dll执行13项，全部通过。不是NativeAOT共享库：虽然生产项目配置PublishAot=true，dotnet test的普通build并不执行native publish。
- 工具**未提供**父子PID、分别的OS exit、模块SHA/MVID、单项断言结果文件、独立stderr分节或cleanup收据。本轮不补造、也不宣称达到有界父子监督/模块身份/cleanup动态认证。原文件的跨channel释放测试明确断言dependency.ReferenceCount=0、geometry.ReferenceCount=0、IsReleased=true，并在本次13项汇总内通过；这不证明生产Native COMrefs0或所有测试对象全回收。
- 额外目录检查 `artifacts/test-results` 不存在，没有可补读TRX。没有已运行未回读任务；此次可用动态原件就是已全读的工具日志。30ms是测试汇总持续时间，不是父子总耗时。

## 保持Open
桌面nativeControl白像素、shader+resize原门、full可信双JSON、当前Native认证及公开WPF资源/画刷/文本/图像NativeAOT功能门仍Open。负Flush通用有界恢复、晚drain/永不回调等不获新结论。不改defaultasync/oracle/expected/threshold/DPI/flags，不以producer替桌面consumer，不新做外部流/WGC或媒体进程隔离。下一步应优先恢复真实Native输入并选择上述非媒体candidate-only既有断言；不得把本次13项托管绿扩写成替换验收通过。
