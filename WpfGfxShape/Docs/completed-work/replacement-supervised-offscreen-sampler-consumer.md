# IW 离屏 sampler consumer

## IX 终态：x86与IW x64同软件离屏合同通过

有效official1/1、candidate1/1，仅两task，部署纠正0；无full/桌面fresh/COM变体/x64重跑。无生产、测试或项目源码修改，无新增工程、脚本或委派。发布成功后只上传正式com-acceptance-package，readback另目录；未上传PublishDir/父目录/历史。两任务filtered120秒、外180秒；父监督不是MTP，无父TRX，以下计数取各精确子role TRX。工具没有另给独立外层exit，不捏造。

|role|task|监督/子PID|子Exit|精确role TRX|
|---|---|---|---|---|
|official|7f2c794d25304c39a1767d29af9eea83|10140/3684|0|1total/1executed/1passed/0failed|
|candidate|03289f6c059a4252907b4c8ea0ec74e9|4840/1192|0|1total/1executed/1passed/0failed|

两份supervision39行、业务evidence96行、TRX35行、stdout12行、stderr空、stages7行全文亲读。Reason exited、HasExited/OutputCollected true；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut false，SupervisorError null。official完整内容/更新/cleanup亲读通过后才启动candidate，无零测试、无像素失败部署重试。

两边原304bytes ps_2_0 shader/s0隐式+s1 VisualBrush、SoftwareOnly与SW target、同一second WriteableBitmap更新、256consumer CopyPixels全数组RGB零阈值断言均未改：初态Actual FF00FF80/Expected RGB0000FF80，mutation Actual FFFF0080/Expected RGB00FF0080，两态Nonempty true。effect解绑、VisualBrush解绑、target清空；8个托管弱引用全false，DispatcherShutdown/FinalizersDrained true，NativeComRefsMeasured false、ModuleRetained true。原始initial.bgra/mutation.bgra/shader.bin均已回收并列目录核存在，**未另独立解码二进制**，内容结论依真实CopyPixels完整数组断言及业务JSON，不冒全原件审计、COMrefs0或长期资源收敛。

actualNative由真实子进程模块枚举各仅一份且路径断言成功：
- official：C:\CodingAgentSandbox\Tasks\7f2c794d25304c39a1767d29af9eea83\wpfgfx_cor3.dll；SHA256 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。独立官方runtime模块，不是候选或旧reference。
- candidate：C:\CodingAgentSandbox\Tasks\03289f6c059a4252907b4c8ea0ec74e9\Native\wpfgfx_cor3.dll；SHA256 63B91E6CD6B5C1E8D0095E1673933DD5BFC539744F7EC36DC06C8DCB074E3136；1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a。精确复用IP同源当前Native，没有重发或虚移绑旧包。
- 两边test SHA256 7F5397021FE193443369918003450DD13E07217BD82427C829B2818B62E12561；MVID7b4ea56f-4adc-4ffc-afc4-b02d5ee9e7e2；X86/.NET10.0.13/IW_OFFSCREEN_SAMPLER。
- 两边PresentationCore位于各task根；SHA256 D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D；MVID2934bccd-58e4-4afe-8677-ec985888f34e；10.0.13-servicing.26504.104+4099f83977f4d29604a9aeb971fb0a7bc1d5f699，不混x64或10.0.1。

精确绝对TRX分别C:\CodingAgentSandbox\Tasks\7f2c794d25304c39a1767d29af9eea83\Results\ix-x86-official\TestResults\child.trx及C:\CodingAgentSandbox\Tasks\03289f6c059a4252907b4c8ea0ec74e9\Results\ix-x86-candidate\TestResults\child.trx。本地原件在artifacts/ix-offscreen-sampler/readback/<role>/Results/ix-x86-<role>/，其中iw-sampler/evidence.json为全部业务记录。

正常分析器default Release/win-x86 build成功Exit0，117warnings/0errors/2.37秒，普通增量非clean/full，日志63行只亲读56–63不冒全警告审计。默认DLL公开元数据复核无两个IW方法，专属包元数据预检有两方法；默认隔离保持。IL元数据仅公开类型/方法签名核验及运行MVID，未独立反汇编方法体；不冒全IL审计。

结论：x86与IW x64相同明确SoftwareOnly离屏sampler内容/更新及有限托管清理合同通过；不是原HW HardwareOnly路径通过，也未用其SkipRender/fallback换绿。下一唯一门：**离屏默认回归门**——另轮审查将已双架构验证的合同纳入默认回归，维持独立role、原断言与清理，不在本轮偷偷去符号。原窗口sampler/0xA AddRef与event副本所有权、Available/HWbackend/loss/mediaresources/full四格、x64Event0等仍全部开放，不用软件成功宣布旧路径已修，完整替换未完成。

## IX 阶段记录（历史，已完成）

无源码修改，原样IW_OFFSCREEN_SAMPLER发布正式Release/win-x86 selfcontained10.0.13，独有artifacts/ix-offscreen-sampler/obj、bin、publish、com-acceptance-package，正常分析器，无NoWarn/关闭规则；显式PublishDir，正式targets生成clean包。Native复用IP native-x86/publish，包aot.sha256核为63B91E6CD6B5C1E8D0095E1673933DD5BFC539744F7EC36DC06C8DCB074E3136；未重发生产。离线DLL公开元数据已核两个IW实际方法存在，源码核实际filter与监督参数，未改expected/cleanup/编译符号。

official task7f2c794d25304c39a1767d29af9eea83已亲读supervision39行、evidence96行及精确child.trx35行：10140/3684，Exit0，1executed/1passed；真实256consumer像素FF00FF80→FFFF0080、非空，8弱引用false及finally完成。actual官方5EBD18E0…，X86/.NET10.0.13。official1/1，candidate0/1，部署纠正0；官方通过后才开始唯一candidate。完整身份与终态随后补同报告；不认native COMrefs0，旧窗口0xA/event/HW门保持开放。


## IX 末次收口补记

本报告全文1–91行已分段写后回读，handoff/session-summary/issues各最新受改1–7行已回读。历史核证仅限本报告所载IW/IV范围与此前定位的IP身份及IW源码，不作全部长历史审计。以下全部为IW历史记录，旧“下一唯一任务x86”已由顶部IX终态完成并替代；当前下一唯一门以顶部IX的离屏默认回归门为准。没有新增执行，原始bgra未独立解码与native COMrefs未测的限制保持，完整替换未完成。

## 初始进行中记录（IW历史）

本轮审查官方 RenderTargetBitmap / COM bitmap target 的真实 sampler 消费合同；不再运行 native 显示前置或 full。保留 IV/IU 历史与原窗口断言；仅允许新增 IW_OFFSCREEN_SAMPLER 显式测试及必要内部 helper。先确认原 HardwareOnly shader 对软件离屏是否适用，再选择同等 sampler 功能的正式支持路径；不以 SW 冒充 HW、D3DImage 或窗口通过。

初始任务计数 official0/1、candidate0/1，未发布/执行（历史，现已执行见下）。生产不改不重发。

## 适用性审计与执行阶段

官方 drawingcontext.cpp DetermineEffectCompositionMode 5415–5425：SW target 的 HardwareOnly 为 SkipRender；SoftwareOnly/Auto 有软件支持时为 RenderCompatible。原 mode2/CompileSoftwareShader0 不能直接在软件 RTB 完整执行。本轮显式选择 SoftwareOnly 的同等 sampler 功能合同，不是原 HW 场景完全覆盖。保留原 TwoInputShaderTokens 304字节 ps_2_0、s0/s1、采样/乘加语义及原RGB断言。已有 CompositionEffectsTests.SoftwareShader 的 WhenSoftwareGenericSamplerCombinesTwoInputsThenOffscreenPixelsAreReal 和 WpfReference SoftwareShaderReferenceTests 说明软件 sampler/RTB 的既有测试范围；未改为常量或geometry shader。

官方 RenderTargetBitmap.cs 181–185 Render→BitmapVisualManager.Render；234–256 创建正式 MIL bitmap target，并以 GetBitmap 得 WIC source；本轮消费者为 RenderTargetBitmap.Render/CopyPixels。producer 是两个16×16 WriteableBitmap，经 DrawingVisual 的 DrawImage 分别进入 implicit s0 和 VisualBrush s1；消费 target 是独立 RenderTargetBitmap。初态蓝+绿，直接 WritePixels 修改同一第二资源为红，再清空同一target重新Render，保持同一visual/effect/VisualBrush，不读源像素或D3D生产者backbuffer来充消费结果。

新增 D3DImageTests.IwOffscreenSampler.cs，全文件 IW_OFFSCREEN_SAMPLER。复用现有正式 exe 和 filtered监督角色，官方先执行并亲读Passed后才执行candidate；未改 NativeRun/ExitDiagnostics。正常分析器 Release/win-x64 selfcontained10.0.13 独有 artifacts/iw-offscreen-sampler/obj、bin、publish、com-acceptance-package；显式PublishDir，正式targets清理/重建包，只上传包，不上传PublishDir或历史/回收父目录。离线公开API核两个IW方法真实存在；runtimeconfig双framework10.0.13、aot.sha256已核。复用IV包的IP当前生产Native0C16A66C…，不重发生产、不用旧无通知修复包。

## 唯一任务及亲读结果

有效任务 official1/1、candidate1/1，总2；部署纠正0，full0，native显示前置0，COM变体0，x86本轮0。

|角色|task|监督/子PID|子Exit|TRX|
|---|---|---|---|---|
|official|45f86c80473542a89dc3c09a0ca75560|4732/9148|0|1 executed/1 passed/0 failed|
|candidate|199a0198e4ca4325b4d13af2d6a0cacb|6232/3576|0|1 executed/1 passed/0 failed|

两份supervision39行、业务evidence96行及TRX35行已亲读；官方evidence先读至95后补末行。均filtered120秒、外180，Reason exited，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut全false，OutputCollected true，SupervisorError null；不是零测试或仅event完成。

两边全部256消费像素初始 FF00FF80、mutation FFFF0080；RGB分别0000FF80/00FF0080零阈值精确断言，两态非空。alpha保持FF，旧窗口RGB expected不变；未新增声称旧窗口alpha验收。输出存 initial.bgra/mutation.bgra，evidence中的distinct Actual来自consumer CopyPixels完整数组，非候选算法造预期。两进程分别验证相同固定原expected，因此本场景两边RGB精确一致；没有另跑独立二进制全文件比较器，不冒称任意图像全量差分。

成功清理：解绑input.Effect、VisualBrush.Visual，清空target，8个托管弱引用均false；dispatcher shutdown/finalizers drained均true。模块驻留；NativeComRefsMeasured=false，没有直接COM Release返回值或GPU趋势观测，**不认native refs0或所有权资源门完全通过**。没有event副本/0xA转交，本合同不能修复或排除旧IS AV、zombie丢弃、interop AddRef失败责任。

## 实际身份

两任务 testSHA D3EB5EDC759BFE28401573209AB1A51885E60ABBF7D3DBA5DD56DCCB3BFE864E，MVID81df88ac-104b-4cca-ab05-b29233e3baa9，X64/.NET10.0.13，显式符号IW_OFFSCREEN_SAMPLER。

actual official = official task根/wpfgfx_cor3.dll，SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，10.0.13-servicing.26504.104 commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。

actual candidate = candidate task/Native/wpfgfx_cor3.dll，SHA0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A，1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a。模块列表各仅一份wpfgfx且路径精确断言；不是只认包hash。

两边 PresentationCore SHA0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31，MVID6cb5dedb-eea3-4832-9a08-c0225e9ff82f，同10.0.13/4099f839。

原件相对WpfGfxShape：artifacts/iw-offscreen-sampler/readback/official/Results/iw-x64-official 与 readback/candidate/Results/iw-x64-candidate。精确TRX远端为各 C:\CodingAgentSandbox\Tasks\<task>\Results\iw-x64-<role>\TestResults\child.trx，已直接读对应本地回收路径。

## 覆盖差异及尚开门

不需要0xA原surface，不验证D3DImage、nativeAvailability、HWbackend、窗口捕获/显示、D3D9纹理资源/设备代际、Present异步event、原interop所有权；WriteableBitmap软件资源更新不能证明这些门通过。共享VisualCache/Historical原断言完整保留，未skip旧测试；现有class initialize仍启动公共窗口owner线程，但测试不创建/读取显示consumer，不依赖原native红帧门。没有新增Host/第三工程/脚本/机器码，没有改生产COMpartitionfill/DRDS或打包/版本/默认并行。

## IV证据补核

本轮直接读精确 artifacts/iv-isolated-sampler/readback/Results/iv-x64/TestResults/child.trx 实际存在；此前缺TRX的历史判断保留并由此补记更正，不新跑IV。该文件绑定task5f9bc6bef1314af3904b182d12e53839，WhenIvOfficialIsolatedSamplerUpdatesThenContentAndResourcesConverge Failed；断言expected5649426(00563412) actual15790320(00F0F0F0)，sampler仍未执行。TRX最终counter已直接读：total1/executed1/passed0/failed1，timeout0，RunInfo Exit2。未新跑IV。

## 最后收口补记

default正常分析器Release/win-x64 build Exit0，117 warnings/0 errors，1.41秒；日志64行仅亲读58–64，不冒全文警告审计，普通增量非clean/full。新增文件受全文件符号隔离，默认不编译新增方法，不改默认行为。两份stdout12行、stderr空、stages7行均已亲读，完整identity/业务/TRX已读；未另用工具解码bgra二进制，原始像素文件留存，本文内容结论依据真实CopyPixels数组断言及业务像素记录。无新增运行。报告全文分段及handoff/session-summary/issues最新受改节均写后回读；本末次补记另回读。原COMfill/partition通知已验收状态保持，无生产改动。

## 下一唯一可执行任务

下一轮单独预算在x86复用本IW显式软件sampler合同，仍official先、candidate后，记录实际同架构身份与消费内容/托管释放；不重复x64、不恢复显示前置。原窗口sampler/HW整合和interop/native所有权仍独立开放。Available/HWbackend/loss/mediaresources/full四格仍开，完整替换未完成。

