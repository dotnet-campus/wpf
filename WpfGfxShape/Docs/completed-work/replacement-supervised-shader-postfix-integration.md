# KD：ShaderBounds 修复后集成回归

## KE 最终核证：KD两full已完成，失败分类交付；完整替换未完成

KE仅读写文件，无命令、脚本、build、publish、沙箱、新full或业务执行；生产及测试业务代码0改，未撤任何修复。以下KD阶段记录保留为历史，不能解释为尚未运行。

### 正式结果与身份

|架构|task|执行|Passed|Failed|skip|TRX/监督|
|---|---|---:|---:|---:|---:|---|
|x64|0fa9829009a640a3b6075cb559b55051|1814|1774|40|0|Failed / Exit2|
|x86|087548bed8dc4a7db312504c02debe41|1814|1810|4|0|Failed / Exit2|

原件根 `artifacts/kd-shader-integration/`；父TRX `readback/full-{arch}/Results/kd-full-{arch}/TestResults/child.trx`，同级supervision.json全文亲读。均Mode=full、实际Arguments无filter、内1500秒、Reason=exited、HasExited=true、TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut/OutputDrainCancellationTimedOut=false、OutputCollected=true，无SupervisorError。外1800是KD提交边界，不冒称KE重验远端。x64父TRX14308起及x86父TRX14072起计数实核，timeout/aborted/notExecuted等均0。

父testSHA x64 `0CE3CC0D7378B1CF241998DF06B919BA6649EBB1CE59C6202BD2B5C11FFB9B73`，MVID `50e5b23d-0236-4bd7-aa57-d0b8dfba48fc`；x86 `8AF9EEB3CAC94CEC7BDC7E1EDD80BB3E9A21033E5131C0808369B8E242BC667B`，MVID `db3219c8-692f-47c0-80c6-7502b0801755`。与族监督身份一致。actual Native x64 `E331526A9594E90BD5CB71414E45A6B055916205F7061FEEE731BB39A010324C` / x86 `7E0936D24BBB7415DD2A8701AE260E8F5E5D8140E61E8510C3F5B6CD6DBA82DD`。顶层Results/aot-identity.json由Curve候选角色覆盖，含精确filter；它证明该角色模块，不是父full被filter的证据，父监督/TRX独立证明full。

官方actual x64根wpfgfx_cor3.dll SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`；来自同任务实际输出，.NET10.0.13，文件10,0,1326,50504、servicing.26504.104、commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。ReferenceData/original-wpf.json头记录相同架构/官方路径SHA/版本，CapturedAt分别06:05:49/06:14:10，属于本次full生成；SoftwareOnly Pbgra32量化参考不是FLOAT/HDR oracle。未逐字节重新散列所有模块或审完所有角色，身份结论限这些原件。

族filter确为 `FullyQualifiedName~ShaderBoundsComparisonTests|FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~OffscreenSamplerTests`。两父TRX1140行均187执行187Passed/0失败/0skip、Completed，两监督Exit0无超时kill/drain。task x64 7cdf2061c3124e11986151055d700514 / x86 534bf00a699a4fe6a040a8066df5c3af。使用TRX实际结果项而非方法数/发现日志相加；KD记录边界2+offscreen3+effects182=187。方法/DataRow及角色filter会重叠，不可再把方法计数或子角色结果重复累加为族分母。

### 最终机器分类及对账

正式文件 `classified/Results/kd-inventory/final-classification.tsv` 与 `.json`。44行按架构+TestId唯一；EvidenceReference是零基JSON Pointer，按JSON的base解析到不可改的 `inventory/Results/jj-full-inventory/{arch}-failures.json`，每项原Name/ParentTestName/Arguments/Message/Stack/Output/RawResult均由此无损关联保留，非只存摘要；初稿名称推断分类已被正式DirectCategory取代。不是宣称根因关闭。

x64：23原生首帧像素前置 +3新提交桌面帧未观测 +2隔离正常Exit2且子TRX首帧失败 +2公共target像素差异 +9原生Present非零状态断言 +1真实loss未触发 =40。x86：1隔离AV +1候选resize后像素差异 +1官方resize前shader内容差异 +1loss未触发 =4。合计44记录、40唯一ID。交集4：`1c6be0d5-e7e9-8416-85f5-e4e394a6710c`、`1eded079-5019-809c-bf7c-07ef4caf0e29`、`14e862b3-9b71-843a-9ff5-67f779272c33`、`16ff21fd-ec04-8a70-814e-ca43404cfbd3`；仅x6436为TSV x64行排除这4；仅x860。相同ID不等于同阶段/根因。不相减旧full估修复贡献。

### 共同4项逐项裁决

1. official sampler `1c6be0d5…`：x86监督PID5688实记-1073741819/C0000005，父HistoricalProcessIsolation.RunAsync失败；historical-role.json和Diagnostic/log_261026061502112.diag确认方法已开始，未见完成子TRX、stderr为空，不能从官方名称推faulting module或产品归属。x64对应子TRX实际Failed/Exit2，001E1D1C对00563412原生首帧失败，非AV。不能混账。
2. candidate resize `1eded079…`：x86真实native首帧00563412通过，CreatePixelShader=0、resize前两次FF80通过，HardwareResize official=False client64x24后FF00而应FF80；这是resize后内容/状态问题，值得产品/夹具定位，不能归为未显示首帧。x64同ID首帧001E1D1C即失败，未证明进入resize。
3. official resize `14e862b3…`：x86实际官方根路径SHA已记录，native首帧通过、CreatePixelShader=0，第一shader帧00000000对FF80失败；失败前没有HardwareResize记录，故本次未证resize到达，不应称“官方resize后坏”。x64仍首帧失败。优先核官方夹具合同，不能据此断候选缺陷或全环境。
4. loss `16ff21fd…`：双方firstCreate0、secondCreate88760868、firstCooperative0，断言要求旧device88760868未满足。直接类别为触发未成立/恢复未覆盖，不是恢复产品已经失败或通过。

x64额外9项Stack均在PresentNativeControl，result141953144=0x08760878并非负HRESULT；精确等0断言拒绝非零成功状态，尚未证明目标shader消费者。初稿把15d634ec…/1f833676…/13c24575…按名称归“产品内容”不成立，已更正。其余6条为公共target shared参数行。3个fresh项CheckFreshDesktopDelivery及bj-fresh-1/3.json显示backbuffer红蓝红正确、Present0、桌面accepted=false/未取得当前提交帧；2个public像素项实际1973532/16774377对5649426，归观测差异，不把所有HW视为环境。所有根因继续开放。

### 重点修复在本次full中的结果

|精确项|x64|x86|
|---|---|---|
|ShaderBounds原105b96c7-ae45-8225-b7ac-be58d2f59b51|Passed|Passed|
|新158e085b-4ca4-89c8-8e32-3eada8049688|Passed|Passed|
|KA window170ff4ac-8c18-8c83-8b35-77294231a78e|Passed|Passed|
|KA window1e7cbafa-f206-8321-a8c4-2c890e8cc69e|Passed|Passed|
|KA回归13baa1a6-edce-89fd-892d-6ad5de576148|Passed|Passed|
|JN Rounded17bea8a5-bb70-8ccd-b35b-5694c41d27d2|Passed|Passed|
|JN Curve13a2f345-685a-81b1-bcce-02ed89041f18|Passed|Passed|
|AsyncWic mode7 127af4e2-6c5f-8c5f-ad31-ffdc04a68f63 / mode8 19a3b855-228b-8fad-a67a-cfae47a754f2|均Passed|均Passed|
|Original AsyncWic mode7 16a3e8d8-7a95-82e8-8dfa-7e78ffdcfefb / mode8 173013d6-ba45-853f-8195-7f9c30f57a50|均Passed|均Passed|

WIC原11逐项（参数不得忽略）：1de8aecd…(4278255360,65280)、197f02db…(4278190335,16711680)、11194225…(0)、16d0ec26…(1)、1872055b…(2)、168b8cd4…(3)、1fa69fb3…(4)、17eef5c8…(4278255360,65280)、12f170ce…(4278190335,16711680)、1a7331dd…(False)、1ac32cda…(True)：本次x86每项Passed，x64每项Failed于原生首帧；完整GUID见正式TSV，x86父TRX1547/1562/1577/1589/1601/1613/1627/1639/1658/1677/1700已逐条亲读。不是拿JZ定向旧绿代full，也不意味着WIC所有资源/长期合同关闭。

1814相对旧1809差5：当前已证新增回归来源为JZ布局/合法调用3、KA角色回归1、KB边界回归1；本次未做旧TRX完整集合diff，精确集合差仍未闭，不继续无限找档。现有包清单含正式exe/dll、runtime/deps、MSTest、系统runtime、官方根DLL、Native候选、Contracts、ReferenceData；回收Results含本次生成参考与角色证据，不等于原输入。已读x86清单首尾420行中的范围，不声称全清单/总字节审计；输入总字节未知，不为此发布大包或重采集。

### 下一前三个最小根因任务（本轮不运行）

1. `D3DImageTests.RunVisualCacheConsumer`/`WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive`，ID1eded079…：先静态追resize后target、shader/sampler及generation重建，保留FF80零阈值；下一授权门仅x86这个精确方法一次有界验证，需记录resize前后内容和绑定，不重full，不随机显示参数。
2. `WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive`/`RunVisualCacheConsumer`，ID14e862b3…：先核official sampler输入、资源创建/提交与首个shader帧顺序。下一授权门仅同官方方法/同架构一次隔离，先证明resize前FF80，再谈resize；与候选共享夹具问题应先修夹具不改oracle。
3. `WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain`/`HistoricalProcessIsolation.RunAsync`，ID1c6be0d5…：先静态核x86 ABI/参数和角色路径，以现有Diagnostic执行起点为界；下一授权门仅该方法一次隔离，保留异常退出及可得fault证据，不把x64 Exit2当AV复现，不增Host。真实loss任务另待能证旧device确已lost的条件，不能合成替代。

历史实际覆盖登记：已读执行规则全文、完整替换方案1-120（初次长输出截断部分35-91已补读），KD原报告全文、handoff/KC等最新受影响节；44失败Message/Stack逐项及分类全ID/参数、重点4业务原输出/子TRX/diagnostic、父监督全文/父TRX计数、focus重点结果、官方及参考头。未声称完整阅读每个巨大RawResult/全部历史千行/所有产物字节；长行截断部分以原件引用完整保留。以下历史不反复重读。所有既有修复保留。

KE分类与文档交付收口不等于完整替换：COM两full仍Failed，旧IOmanaged4256两格为旧身份，不能最终四格全交。Available、HW backend、真实device-loss、media resources、长期资源及final四格仍开放。只对本次Windows x86/x64 Release与10.0.13所读证据作有限判断，不外推未来版本。

最后补记：本报告全文1-74及三共享最新KE受改节均已写后回读；final-classification.tsv表头+44行及JSON全文/修正后引用base已回读，40+4及各分类和交差集手工对账。未执行脚本或新测试进行自动重算；不冒称每个原始长行已完整展开，原文完整保留可按指针核验。

## KD历史状态：族通过，full阶段
阶段历史：Release小构建成功0warnings/0errors/0.78秒；双架构正常分析器self-contained10.0.13发布成功。独有artifacts/kd-shader-integration/{arch}/{obj,bin,publish,com-acceptance-package}，显式PublishDir。复用KB aot及同架构ReferenceData，包SHA核E331526A9594E90BD5CB71414E45A6B055916205F7061FEEE731BB39A010324C / 7E0936D24BBB7415DD2A8701AE260E8F5E5D8140E61E8510C3F5B6CD6DBA82DD。
实际filter：FullyQualifiedName~ShaderBoundsComparisonTests|FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~OffscreenSamplerTests。发现187，边界2+Offscreen3+Effects182（含常量/操作码/两输入/float/HDR/拒绝通知/mask/layer兄弟）。每架构一次120秒组，父TRX末尾直接核各187Passed/0Failed/0skip；task x64 7cdf2061c3124e11986151055d700514 / x86 534bf00a699a4fe6a040a8066df5c3af。监督全文核Exit0/exited，无timeout/cancel/kill/drain。回收readback/shader-{arch}/Results/kd-shader-{arch}/TestResults/child.trx。即将同包每架构一次无filter full内1500/外1800。

以下为开始阶段历史，不代表当前未运行：
已开始读取当前 KC/KB、KA、JZ、JV 交接与既有正式 COM 隔离设施。尚未构建、发布或运行 KD 测试，不沿用历史计数作为本轮结果。

执行边界：生产、测试业务、入口及项目打包代码零修改；只使用专用文件、构建、发布、测试/API 工具及正式纯 C# exe。保留 KB 完整 bytecode 1048576 上限及非零 disabled 关闭保护、JR/JN/KA/JZ/JD 与原 COM fill 通知修复。先核真实 shader 族及入口、正常依赖图、输入隔离和身份，再双架构各一次有界族；可信条件下同最终包各一次无 filter full。任何新缺陷保留原件，不修改标准求绿。

本轮结果、失败分类、交差集、输入清洁边界及版本待实际执行后登记；不宣称完整替换。Available/HW backend/device-loss/media resources/长期资源/最终四格保持开放。
