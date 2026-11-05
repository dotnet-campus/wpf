# Candidate purevideo r2：监督合同修复与只读二进制审计，真实视频未跑

## 结论与边界

SupervisorAssertionsImplemented / NegativeControlsPassed / SupervisorFailureExitVerified / BinaryAuditPartial / CandidateVideoNotRun / OverallOpen。

生产修改0。CandidatePureVideo源码、原PureVideoPublic、普通COMAcceptance/Package.targets、oracle/expected/flags/DPI/像素阈值修改0。未加载/复制原版gfx、未运行original/compare/full、未使用终端脚本。保留显式PreBusinessIsolationUnproven与不可达ExecuteWpf；未以负控成功替代视频完成。

## 独立审查旧原件

先读r1报告、最新native-video报告和current-execution-rules；本次用户授权覆盖历史耗尽预算，不冒称缺授权。亲读task d7de687d187b45f399e0951e14f76854的process-results和三个原始result：PID3560/12768/1220各真实exit1，timeout=false/reaped=true/drained=true；分别digest-required/FileNotFoundException、sha-validation/InvalidDataException、pe-architecture-validation/BadImageFormatException。读取baseline/finalModules及空requests：所见模块无candidate或original wpfgfx。快照不是瞬时加载历史证明，不据此宣称全程拦截。旧harness SHA39331A61CEB335A654A6E8B84B8DE1EB625B6285B42ACDC79A61E68687D6E32A、MVID465bdf1d-1362-4426-b41f-7d8447fcc00e。

旧监督源码末尾无条件return0确为缺陷：只采集没有验合同。旧报告的runtime字符串不能当导入/加载证明。

## 修复与复验

仅改Tests/CandidateVideoSupervisor：Program.cs精确校验各负控failureStage、最终stage、异常类型、报告exit及真实process exit1；baseline/finalModules非空且禁止任何wpfgfx；requests必须空；拒绝candidate认证/场景证据越过负控边界；子harness SHA/MVID必须等于包内重新计算身份。校验包内唯一gfx是Native/wpfgfx_cor3.dll且匹配aot.sha256。所有合同失败返回总exit1，成功才0。timeout/reap/drain/kill错误均纳入失败，执行30秒、kill后reap10秒、drain10秒；未捕获的启动/I/O异常也使进程非零。未复现timeout/kill分支，不声称该分支动态通过；异常中断时完整收据仍可能缺失。

新增SupervisorProbe.cs通过独立父进程记录监督真实exit。verify-failure-exit仅在监督层将missing-digest期望阶段故意设错，不改变child入口、candidate或原consumer。阶段断言实际报告wrong-failure-stage/wrong-final-stage并总exit1，不是手工写一个失败收据。该探针只验证阶段失败到总体退出的接线，未逐个故障注入模块/超时/drain分支。

最终原件：artifacts/candidate-purevideo-r2/verified-readback/Evidence，task bfca691322324c98bd207018b7d12cd9。

|正常负控|PID|真实exit|阶段|timeout/reap/drain|断言|
|---|---:|---:|---|---|---|
|missing-digest|1608|1|digest-required|false/true/true|Passed|
|wrong-sha|5248|1|sha-validation|false/true/true|Passed|
|wrong-architecture|6408|1|pe-architecture-validation|false/true/true|Passed|

normal-process-results.json记录以上；normal监督PID11352/exit0；错误阶段探针监督PID13036/exit1；两个监督均无timeout、已reap/drain，详见supervisor-exit-probe.json。探针missing PID1372/exit1，在监督层被正确判Failed。由于两个子监督顺序使用同一工作目录，未加前缀的child JSON/stdout/stderr为后运行探针的原件；正常轮process结果及监督stdout另存normal-*，不要把两轮child PID混用。

package-identity.json重新核包Native SHA FBD35CDD609DE2D67EB494B41A07FDB9240DC955B3F54D681D48F2423C8A4959，harness SHA/MVID与旧值相同（harness源码未变）；唯一gfx路径位于该task Package/Native。不宣称重新冻结505项生产源码或新Native生产发布。

## consumer与生产边界

csproj直接Compile链接原Tests/WpfGfxShape.ComAcceptance/Pcore/PureVideoPublic.cs，未复制改写断言。亲核Opened/HasVideo/无音轨/CanPause/时长，0.5与2.5 seek、target+0.3时钟、后续Rendering与Render派发、Pause后RTB、target+1上限、alpha255、红蓝dominance、非重复帧、Ended及Close/解绑/退订/窗口释放仍在原文件。MediaHelpers保持32x16、96DPI、Pbgra32，15秒等待，40帧10fps无音轨asset0。直接链接证明断言未删，不证明视频运行通过；ExecuteWpf仍不可达，业务0。

## 固定10.0.13包只读审计：新增事实与明确未闭合处

新增ReadOnlyAudit.cs，PEReader/MetadataReader只读，不Assembly.Load、不执行被审DLL。pe-initialization-audit.json对包内每个DLL重新SHA，解析PE普通导入和延迟导入的DLL名称与原始descriptor，提取.cctor、Loader/LoadLibrary/Initialize相关方法及首三个方法的token/RVA/原始IL和相关MemberRef。没有解析导入函数thunk、没有完整IL反汇编或符号执行，不把原始IL输出当调用链覆盖证明。

已亲核固定PresentationCore SHA0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31：普通/延迟本机导入均空；<Module>..cctor token0x06000001/RVA4263656的IL为call0x06000002;ret。ModuleInitializer.Initialize token0x06000002/RVA4263664中依次可见call0x06000003、call0x0600024C（DWriteLoader.LoadDWrite）、call0x0A000105，以及后部call0x0A000108（MemberRef解析为MS.Internal.NativeWPFDLLLoader.LoadDwrite）。这说明不能忽略Assembly.Load初始化路径。

DirectWriteForwarder SHA AEC60CD1A89ECE8F998DD7029BA9E4FD51211A20DB1A6F940B16E5AED39CECD7的普通/延迟本机导入均空；其中NativeWPFDLLLoader.LoadDwrite token100664044/RVA262736原始IL166A80E20300042A，只写零后返回；其.cctor为ret。旧源码注释不是原版加载实证，该固定方法也没有call LoadLibrary。

仍未完成且阻止真实启动的具体范围（不声称已发现加载原版）：

1. PresentationCore.ModuleInitializer.IsProcessDpiAware token0x06000003及其调用的0x06000004、Initialize内0x0A000105/0x0A000108以外的调用和退出委托0x06005514，尚未完整解码闭包；不是修改DPI的授权。
2. PresentationCore.MS.Internal.Text.TextInterface.DWriteLoader.LoadDWrite token100663884/RVA4290532，以及LoadDWriteLibraryAndGetProcAddress token100663888/RVA4290664：已提取IL，但ldstr实际值、0x06000269包装及MS.Win32.LoadLibraryHelper.SecureLoadLibraryEx到UnsafeNativeMethods.LoadLibraryEx调用/flags/搜索目录尚未逐项解释认证。
3. DirectWriteForwarder.<Module>的CModuleInitialize.{ctor}、CreateCModuleInitialize、<CrtImplementationDetails>.LanguageSupport.InitializePerAppDomain/InitializeNative/InitializeVtables等初始化闭包仅提取IL，尚未递归证明。LoadDwrite短方法不能代表整个模块。
4. candidate Native/wpfgfx_cor3.dll及包内hostfxr/hostpolicy/coreclr/clrjit等已输出普通/延迟DLL descriptor；尚未逐个解析函数thunk/显式LoadLibrary/GetProcAddress调用点，也未绑定递归System32/API-set目标文件身份与初始化路径。包内导入表输出不等于递归OS审计闭合。

因此没有实施未经证明的进程级边界，也未删除拒绝。下一最小动作是解码上述PresentationCore/DWrite初始化闭包及实际字符串/目标，优先决定其是否有gfx请求；不是无界扩展到所有系统安全研究。只有形成足够的candidate替换隔离证明后，才接入ExecuteWpf并加成功模块路径/SHA及清理收据、运行原consumer；本轮未达到该条件。

## 准确执行账

专用publish4：CandidatePureVideo1成功；CandidateVideoSupervisor3成功（初版、父监督探针版、真实阶段错误探针版）。专用sandbox4：初次三负控task7ed397f57be64fed8ffd353d79b8d389；初版人为失败标记探针（task未从工具响应取得，不补造）；父监督初验task1f9f71fba7bf4cf4be725ecee8104e73；最终真实阶段错误探针taskbfca691322324c98bd207018b7d12cd9。累计child负控18次（3+3+6+6），其中含监督失败出口探针，不称18个独立业务测试。最终只采用verified-readback作为当前接线验收。

单独build/MSBuild/dotnet test/生产Native发布/真实WPF/视频/original/compare/full各0。publish包含其必要编译，不另算独立build。未新增单测框架；验收辅助通过沙盒进程探针验证，故障分支覆盖限制见上。
