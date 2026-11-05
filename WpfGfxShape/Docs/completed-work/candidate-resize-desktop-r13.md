# r13：原件回读、独立包纠正；正确 STA 初始化后 nativeControl 仍白 / OverallOpen

## 范围与入口纠正

shader+resize 就是原目标。原调用链为 `StartPresentationThread → WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive → StopPresentationThread`。早期 CreateOutputWindow 失败是遗漏 fixture 初始化，不是入口无关或环境限制证据。本次不改生产、原测试、oracle、expected、容差1、DPI 或 flags；无原版加载、复制/fallback、终端脚本或委派。此包只调用 candidate 桌面方法，绝非 full。

## 先读前任务原件（未用重跑替代）

原件 `artifacts/candidate-resize-desktop-r13/readback/`，task `2899147fc2e347d389593db1574c5d6f`。已亲读父子 exit、identity、stderr、Package/Evidence/result.json 的 baseline/finalModules 与包文件清单。父6048、子9972均 exit1，timeout=false/reaped=true/drained=true/killError=null。

Native SHA256 `2E5DC1BCC9CC7DEF60B0EE05C622954DC9BC7C2C3A57888FEE5767ED5705AFEE`，4853248字节；harness SHA `8FF6CAAB6ECF1A2B8E5CC9BB3503BF5E0CF73564F6DD4ED78A815B8764312785`，MVID `f8b4c469-d0dd-4b2b-b991-a3db25334272`；fixture SHA `19920F58A42632FC12B821CE9698CB07609F7D4AE29063337E1948A815FC7174`。这些是原件回读，不冒充新计算。

真正失败为 `native-search-boundary / Unexpected graphics package file`，requests=[]，尚无 actualNative。旧检查扫描所有包含 wpfgfx 的文件名，把 WpfGfxShape.ComAcceptance.dll、Contracts/wpfgfx.def、wpfgfx_cor3.pdb 等非图形运行库也当作第二 Native。baseline/finalModules未见图形 DLL 加载。因此此原运行没有窗口初始化、nativeControl 像素、resize 或 refs；不得把历史白像素移绑到它。

## 包门禁核查与修正

正式 `ComAcceptance.Package.targets` 未修改：RequireReferenceData 未指定时仍等于 ComAcceptanceSandbox；ValidateComAcceptanceReferenceInputs 在 PrepareForBuild 检查可信 reference。前任务命令行 `ComAcceptanceRequireReferenceData=false` 是全局属性，会传到被引用正式 ComAcceptance 项目，确实绕过其普通 reference 检查，不能认证由该命令形成的正式包。不是“该属性只影响独立入口”；旧产物只保留失败历史。

纠正仅位于 CandidateResizeDesktop / Supervisor：ProjectReference 移除继承的 RequireReferenceData/Sandbox 全局值，引用构建设 ComAcceptanceSandbox=false，禁用无关正式打包而非放宽正式门禁。专属 CandidateFixture.targets 只通过该引用的 CustomAfterMicrosoftCommonTargets 注入运行元数据，将 fixture Sandbox=true、Results=Results、AotDll=Native/wpfgfx_cor3.dll；不设置正式项目打包属性，不伪造 reference。构建机路径与远端运行路径明确分离。Supervisor 使用已有 WPFGFX_CANDIDATE_DLL 绑定已认证包内唯一路径。独立 bootstrap 仅把 wpfgfx.dll / wpfgfx_*.dll 认作图形运行库，仍要求唯一选定 DLL，并保留 SHA/PE/实际模块/同名句柄验证。共享 PureVideo bootstrap 原件未改。

正式默认门禁负控另实际执行一次：普通 ComAcceptance 项目 Release/win-x64/self-contained、ComAcceptanceSandbox=true、不传 RequireReferenceData，在缺少参考数据时失败于 `BlockedMissingTrustedOracle: ComAcceptanceReferenceInputDirectory`。这只是拒绝负控，不是正式业务/full执行。未发布或认证正式 full 包。

## 本次新增运行逐项回读

证据根 `artifacts/candidate-resize-desktop-r13-corrected/`，全部新增运行均已读退出、stdout/stderr及身份关键证据，无待回读运行。

1. `readback`：父4484、子12164均exit1，无timeout且reaped/drained，killError=null。STA启动且finally join；fixture仍请求构建机绝对Native路径，DllNotFoundException，nativeControl未到。
2. `run2-readback`，task `455784d8f49d4d5fa0a0b638cde2f6aa`：父2952、子12872均exit1，同样正常回收。实际Native已等于包内候选且SHA正确；失败是 Results 仍为构建机D盘路径（IOException 设备未就绪），nativeControl未到。此两项是有明确根因的包路径修正，不是 flags/DPI/device/chain 无目标实验。
3. `run3-readback`，task `e0793b8be1de4a82816a399cac7c92b4`：父6016、子7508均exit1，timeout=false/reaped=true/drained=true/killError=null。stdout确认 dedicated STA GetMessage/DispatchMessage 与 `original fixture cleanup joined`。已亲读 stdout/stderr、identity、result actualNative/nativeBasenameHandleMatches/finalModules；唯一图形模块是包内 Native/wpfgfx_cor3.dll，无original。requests=[]不冒充执行了公开WPF图形导入；本方法直接调用候选导出。

最终 Native SHA 仍 `2E5DC1BCC9CC7DEF60B0EE05C622954DC9BC7C2C3A57888FEE5767ED5705AFEE`；harness SHA `16A363797A3DF1A8A89DB563A534651F138F22C90230018365EF28A473D77AC4` / MVID `3276c359-a047-4d96-9d5c-812ba918b39e`；fixture SHA `6072536FF8CE833349B831474B2B94EE6BE6EE462905E43B2927DC14F7692CE9`。身份为本次监督/子进程真实计算后保存，非沿用旧harness身份。

## 最终动态事实与 refs 边界

正确初始化后仍在原 nativeControl 硬门失败：expected `0x00563412`、actual `0x00FFFFFF`；AssertCombinedPixel 原逐通道差<=1，VisualCache.cs:112，后面的 connect 未进入。窗口0x2C0524，ownerPid7508、ownerThread11756、当前调用thread4228；visible=1/iconic=0，target=cover=root=foreground，同一 Default 桌面/session1，cloaked=0，采样屏幕(56,48)。原swapchain32x16/format22/count1/swapEffect1/windowed1/DPI96；nativeControl=d3d9.dll、Ex=false、cooperative=0。这些都是观察，不是本轮改变参数。

未进入候选connection/channel、shader业务、64/48/32x24动态resize与最终业务断言；完成resize=0，不能认证通过。原finally会调用 Release(device/d3d)、DestroyWindow，再fixture Dispose/join；stdout只证实join，原 Release 不保存返回计数，本次没有逐对象refs原件，故COM refs0为Unknown/未认证，不以源码finally或父子exit代替refs0。r10的16条release不移绑。本次未额外削弱测试或加GPU替代consumer，未再无目标重复实验；白像素仍不足以宣称candidate生产缺陷或宿主硬限制。

## 执行账与交接

前任务：publish8（6失败2成功）、sandbox1，原件现已回读。自身：publish6（5成功：candidate2、supervisor3；1预期失败：正式缺oracle门禁负控）、sandbox3；单独build/test0、生产Native发布0、生产源码修改0、正式full业务0。合计r13 publish14（7成功7失败）、sandbox4；不冲销前任务失败或r12/r6历史账。发布存在原有MSTest/nullable分析器警告，不声称零警告。

恢复优先级：先本报告及run3原件；再 [r12有限RTB报告](candidate-resize-isolated-r12.md)：三周期12帧Pbgra32 exact与生命周期有限门通过，不是桌面/nativeControl/COM refs/full；再 [r6视频报告](candidate-purevideo-isolated-r6.md) 和 [r7审查](candidate-purevideo-independent-r7.md)：r6同身份两轮公开视频有限门保留，不迁移为resize或full认证。现状 OverallOpen。下一只在有明确新证据/修复假设时推进真正桌面nativeControl；若其通过才继续原resize全部断言及最终refs取证。禁止再把shader说成非目标，禁止重跑替代本报告已有原件，禁止改默认正式full门禁或加载原版fallback。
