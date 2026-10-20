# BQ：curve / capture / exit 独立审查与交接

## 1. 裁决及审查边界

**完整替换未完成；BI–BP 最终没有已验收的生产修复。** BM 生产预量化试验已撤回；BO 未验收异步行为已撤回。撤回不代表历史同步 Event=0 合同正确，更不代表堆损坏已修。BP 无 surface 最小控制与最终路径控制成功，不等于真实内容恢复或 full。

主代理只监督；BQ 仅专用文件工具读证据、核对源码、更新本报告及三份共享文档。没有终端、直接/间接脚本、构建、发布、新测试运行、代码修改或无信息 full。根为用户指定 WpfGfxShape；下文路径均相对该根。已读执行规则、完整方案1–120及BI/BJ/BK/BL/BM/BN/BO/BP报告；报告长段存在截断处以关键节补读，不宣称每个历史报告逐字无遗漏。本文亲核范围见各节，报告转述与原始亲核分开。

## 2. 曲线：有差分离器成立，生产修复不成立

BI 业务根：`artifacts/bi-curve-fill/run-x64/Results/curve-an/fa671b8394a6408f9ffad6a953d21c8a/`（x86 为 run-x86/…/d140094b12e34552b85922fb56e30c05）。BQ亲读x64 comparison全文、separator-summary.jsonl:65–70、133–134：原frame1/3/5各44字节、frame9为10，threshold=0、exact=false、hardwareVerified=false；B/E官方互差30，geometry对B30/对E0，drawing对B0/对E30；D/F互差8，nested对D0/对F8。visual选B由BI报告及其原始summary71–72补核。不能把旧负样本“匹配B”外推排除E。

BQ另亲核BI双架构bi-parent/parent.trx结果及RunInfo（x64:30–41、x86:28–39），均1执行/0通过/1失败、退出2；x86 comparison全文及summary65–71、133–134同样支持44/10与30/8。BQ不把角色渲染成功当父成功。正确阶段为geometry本地float保存后消费、drawing/visual父变换消费、nested本地float保存后outer；本次F矩阵先合成被有限分离器排除，不证明所有SIMD/FMA顺序。

BM根：`artifacts/bm-fill-controlpoint/run-experiment-{x64,x86}/`。BQ亲核两份bm-parent/parent.trx:20–26各1/1，x64十帧comparison全文全0；x86十帧全0由BM报告及comparison补核。另亲核x86业务根separator-summary:65–68、133–134及x64:67、133，双架构真实geometry仍差30、nested仍差8，x86 drawing为0。有限十帧绿不能取消自己的分离器失败，也不能把原44/10写成生产已修。

最终`Code/WpfGfxShape/Abi/NativeShapeReader.cs`回读覆盖Capture及文件尾：仍以本地float控制点交共享double Flatten，transform参与误差，细分输出再Apply父变换，最后TryCreateFigures；无BM新增TryRasterPoint或设备控制点预量化分支。BM仅两处唯一片段撤回；没有修改前整文件逐字节快照，**只能认证所读行为恢复，不能认证历史整文件hash相等或恢复后二进制回归**。

真正fill链见BI续派114–133：GeneratedVisualRenderer→GeneratedNativeStroke→NativeGeneratedPath→BitmapRenderTargetExports→NativeShapeReader；只改SoftwarePathCoverage备用入口不足。普通cubic/arc/rounded在Segment→type=2丢来源，不能按点值猜来源或盲改共享stroke。需要fill专属来源/阶段信息及正确float设备变换→28.4控制点→整数细分，不是预量化后逆映射给double细分器。

## 3. BJ fresh-frame：推翻绝对不能产帧，不证明owner根因

BQ亲核`artifacts/bj-capture-fresh-frame/stages-run-x64/TestResults/bj-stages-x64.trx:405–413` Completed 16/16；stages-gate-run-x86对应bj-final-gate-x86.trx:400–406 Completed 17/17，异常计数全0。x64同任务bj-public-stages.jsonl全文三条为native gate通过→official通过→candidate通过。x86 public TestResults递归仅部署空目录，无父完成TRX；不能拼gate成public通过，也不能编造其退出码/超时/崩溃机制。

亲读x64 bj-fresh-3.json全文60行：red/blue/red的expected、backbuffer、duplicated、frontPixel逐帧一致，accepted=true，Present及frontResult均0，client/screen独立颜色一致，提交/完成递增。TRX/backend记录补核为Microsoft Basic Render Driver。故“Basic Render Driver无法产帧”的绝对判断被推翻；BJ非owner对照也通过，owner是BG唯一根因仍未证。该系统D3D9+duplication控制不是物理GPU或候选内部backend验收。BJ完成TRX未提供数值Process.ExitCode，不冒填0。

## 4. BL/BN：新复现有退出状态，首次堆破坏仍未知

BQ全文亲读BL final-sampler/bl-sampler/supervision.json：task ca99c8d97e4e4cc4ae4e8fb9fdbe2709，子4124，ExitCode=-1073740940/0xC0000374，Reason=exited，timeout/cancel/kill=false，双流完成、SupervisorError=null。不是超时kill；没有完成TRX。只证明这次官方sampler新复现，不回填BD历史full。

BN两份final-official-{x86,x64}/bn-sampler/supervision.json全文及stages:29–35亲读：任务bf7a4fbfe70d43e0a1ea21989000e53a/2ac1a430d4734550ba435809179ca49c，子2288/4104，两架构首次Commit均0x80070006(E_HANDLE)，随后cleanup destroy channel before，无返回，最终0xC0000374；均非timeout/cancel/kill、双流完成、监督无异常。最后可见DestroyChannel不能定位首次写坏内存的位置；无dump/分配释放栈，不宣称官方bug或唯一ABI根因。

最终VisualCache亲核connect(1)、初始与mutation两个PresentPacket均未填Event、present(connection)、原Commit断言及finally destroy→disconnect→COM/device→window顺序，BN/BL同步Stage和原HRESULT仍保留。BN审查的官方ProcessPresent与PresentationCore合同明确要求异步Present及复制事件转交；**同步Event=0偏差仍留在最终文件，未修**。BO探索曾有0xC0000005失败，BP撤回不是修复证明。

## 5. BP：最小控制、前置失败、精确撤回

BQ亲核`artifacts/bp-safe-closeout/probe/readback/bp-contract/`监督身份/退出尾及TRX结果：task60397d1f53d44696a3f35bec57d866be，4/4，退出0。无真实surface的同步update-only/异步事件重复控制，仅证明有限初始化、事件及cleanup序列，不是surface内容恢复。

同根content/readback/bp-content：taska59f9da1f7bc4c06a87cac4d1a488a92，监督退出2、非超时kill；stages全文24行在AssertCombinedPixel before后进入finally直至cleanup complete，尚未connect。TRX错误及1失败补核：expected00563412、actual00CBAE87。这是原生fresh前置不稳定，不是异步surface内容验证成功。

最终CaptureHistoricalControls全文39行保留5候选DataRow及官方geometry/sampler默认历史测试，移除BO四数据合同及asynchronousPresent调用。VisualCache参数和关键提交/cleanup段已读；全ComAcceptance目录搜索asynchronousPresent/SendHistoricalPresent/HistoricalCloseHandle/BO新合同名均0。不是整文件历史hash认证。

BmFillControlPointTests现为internal、无TestClass/TestMethod；NativeRun Main先分派--supervise-exit，再显式--bm-fill-control-points，await原实验，只有成功才return0，默认MTP返回路径保留。Render仍为4种×fill/stroke×identity/nested共16场景，比较保留nonempty、threshold0、exact断言，无skip。历史BM默认MSTest失败记录不删除；BP改运行边界不是给失败改判。

BQ亲读BP显式x86 comparison及两角色identity全文：16场景均非空但exact=false；arc fill22/4、stroke128/122，cubic fill10/12、stroke108/120，ellipse fill8/40、stroke190/200，rounded fill24/32、stroke104/94。报告记录AssertFailedException且无独立数值退出；显式角色不走MTP、无TRX，不补造退出2。

最终final-{x86,x64}/readback/bp-control两份监督JSON38行及child.trx26行全文亲读，各路径控制1/1、退出0，非timeout/cancel/kill、双流完成。双架构发布成功是BP报告记录，本轮未运行/重建且未独立取得完整发布日志；不能认证零警告。控制不是sampler/full。

## 6. 实际身份与版本（不得跨快照移绑）

所有下列原始身份来自task实际路径，官方根/wpfgfx_cor3.dll、候选Native/wpfgfx_cor3.dll；运行时.NET10.0.13。官方版本10,0,1326,50504，ProductVersion10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；候选1.0.0.0。不外推其他servicing版本。SHA是JSON所记录值，本轮未重算DLL哈希。

|模块快照|x86 actual SHA256|x64 actual SHA256|
|---|---|---|
|官方|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|BI归档候选|0FCA4287C3E44CB7A7B0801D297C6D4E85F4AD84402D8B9580D0725E8449741B|C8C35CE4872FA659B02F8731F34F1F0492775D30879DEC17E54779F06228BE01|
|BM已撤回试验候选|F26110CB17B6F44EEE5AADE1BF4FA3883A96D91B59F710DC749F8758EFA74AAA|1C71D3B0BF08CCCBF0D06776DB0B34ACAE2AD28883877CDEE2BE566B24559492|
|BD旧候选，BJ/BL/BN/BP复用|E0D3F5EFEDFACFD5CC3491DD43B194E19028E8B541A462123C8F73C73D6E25B5|1C381539E7C82DDD75908C0563F0AC062E583166BDB23AEE066BDD21E838752F|

|测试阶段|actual测试SHA256 / MVID|身份来源与亲核界限|
|---|---|---|
|BJ x64|7FFDF4F38E97BB2E67ECBFE3219D2D8BA19E7A3B9AA6F2ACDCE653A3498220C7 / a185491a-3e71-4a5d-88a6-bb93c71db7b9|BQ亲读bj-fresh-3全文；x86 EB9F506B298B5BA11DC477937BCB62E7B33B7CEE75907F0FF2B1094932D46098 / 1dc8a8ca-6e55-42ae-bdaf-6f1aec58cae0由BJ报告，另补原始身份段|
|BL x86最终|9C9C460AE376528299952B88DC123F1E246702A8CC8E22C06651CB01E2C9640C / 6f73bb3d-3311-4bd4-8859-9155414c07db|BQ全文监督JSON|
|BN x86最终|B7E4902BFA47A792B108FBA0E7D87E3E6B3D5EB2E963A72A80FD015699597D1D / 82da2e62-6250-4b71-8a2c-75e0ff7ca445|BQ全文监督JSON|
|BN x64最终|F781C165AB103EBD7F2218FC81616D262EDBB03CA0B63D9462BDA63E6DA0F98F / 6e7abd51-aeb0-4887-a3ac-189db9e1c9d0|BQ全文监督JSON|
|BP无surface控制|F4C0453E0D63054D7CBF7E335B59FBACC3E31F754E94FA7AEE1E9C23D18AF44E / c9c7dd9b-58e1-4fb3-808e-93180f3f537a|BQ亲读监督20–38及TRX身份/结果段|
|BP真实前置失败|E7F5DC64D1D61022EB308421F9201EE37859AB3DD768BA9DF7D042CC0326FD5B / 7b1edb1a-51bb-4d4e-8abb-3e5448f634a1|BQ亲读监督20–38及stages全文|
|BP最终x86控制/显式BM|83B35C8205BF24B541FE663FA7BCBED3DB75ED3655FEB6B1089A35E93DB89DDD / 51a184a4-864c-4499-8282-c5e2c4e04db8|task4af8284f2a214d7cb0fdd85c9adb1319控制；task2ba9fb83b2624ea4ba1b72b8ad0188d7实验；BQ监督及角色identity全文|
|BP最终x64控制|3618466036359A53BC1030F6F3BB949B7BBEA8304E519A185EB41466B5441FBC / bb4fb7f5-a654-452a-9b3b-defec0e1a303|task3a8c11f6b4eb4de49c52db2a71b6e23b；BQ监督全文|

BI测试actual SHA/MVID见BI身份表：x86 6BBBCA3386028D144E7B0111E0629461AB45F3A92E7E076405B22F4525901927 / c7ea250c-7116-4f4f-ab7b-a1ce413971c5；x64 49A7B4CED67FBC6A70D0DE22AFB91702ECEF734B8825BE9826DB94EB631FEAFA / 21edce72-842f-4663-9e96-a93aa281b56b。本轮未全文复核两份bi-experiment-identity，不冒称全亲核。BM十帧identity不含测试SHA/MVID，不补造；BM siblings x64旧失败测试8F28D012F49B9CC30EA34FA05628A7CE560751A86D43117575AF17211395FFA8 / f5bf814e-ae13-4c6a-b4ac-87147c733dd5由BM报告，不混同BP最终测试。

## 7. 保留文件、矛盾与证据限制

最终保留实验/测试：CurveDeviceQuantizationExperiment.cs（BI E/F判别）、既有CurveCausalityExperiment.cs及RoundedRectangleContractTests.cs、BJ D3DImageTests.CaptureFreshFrame.cs、BmFillControlPointTests.cs显式实验。保留诊断：ExitDiagnostics.cs、NativeRun.cs监督/实验分派、CaptureHistoricalControls.cs及VisualCache.cs的BL/BN Stage/HRESULT。BO异步分支和四数据合同已移除；BM NativeShapeReader补丁已撤回。此为针对本续轮文件的核对，不是全仓dirty清单或无其他历史生产改动认证。

BQ原始亲核为指定TRX结果/关键stdout、上述监督JSON、业务comparison/identity及源码关键段；不是160行summary全审、全部图像逐像素重算、全部模块重算hash或所有ABI/生命周期闭合。最初误读BI summary到original子目录不存在，已改读业务根真实文件，不算缺失文件已审。报告记录的退出2/发布成功等若未见独立数值字段，明确维持报告级证据。

补核BP最终两份Results/aot-identity.json全文，与表中BD旧候选SHA及X86/4、X64/8一致；BJ x86 bj-fresh-3:1–10实际测试SHA/MVID、x64模块JSON全文与表中版本相符，x86 public阶段全文仅两条。历史BH“待补float替代分支/身份”和旧“无法产帧”的推断已被BI/BJ推进，不再作为当前结论；BM原默认发现问题已由BP改显式入口，但其失败原样保留。BD x64 full1740/1699/41退出2、x86无父full完成TRX仍是旧快照基线，BK分组2/2、186/186或BL/BN新退出不能拼成BD full，也不能回填BD根因。

## 8. 下一最小工作及完成门

1. 曲线：在真实fill链来源丢失之前保留必要阶段/来源信息，实现分阶段float转换/本地保存/父设备变换与28.4控制点整数细分。保留B/E30、D/F8、原44/10、负半格及arc/rounded/ellipse/stroke兄弟控制；不以逆映射double近似替代整数合同，不盲改共享stroke。
2. 捕获/Present：先稳定同任务原生red/blue/red fresh前置；再在真正surface路径实现正确初始化、异步channel、DuplicateHandle事件所有权及在途cleanup，官方/候选双架构重复内容更新和正常退出定向验收。无surface控制不能替代；必要时用真实崩溃栈/分配释放证据定位首次堆损坏，不跳过DestroyChannel或放宽断言求绿。
3. 不再无信息full。限定修复有证据、源码稳定后，最后产物绑定Tests/ComAcceptance×x86/x64 Release完整适用四格full；目前仍缺，BC历史托管绿、BD旧full、新控制不得拼最终矩阵。

真实物理GPU、loss→reset后新色、持续媒体、长期CPU/GPU/线程/资源/callback及异步关闭、通知有效target同步/非零SyncModeStatus/迟到消息、GPU圆角、合法大shader内容差等合同全部仍开放。四文档完成只完成交接，不是完整替换。
