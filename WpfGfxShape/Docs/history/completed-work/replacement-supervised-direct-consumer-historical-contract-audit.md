# Direct consumer 对 HD/HE/HF：合同与原件静态调查

## 1. 实质裁决

本阶段仅阅读、调查和文档交接；源码/配置修改、build、publish、沙箱、测试、终端、脚本、再委派均0。完整替换未完成。

**不能认定此次直接 consumer 与 HD/HE/HF 是同一验收合同。已确定的最大实现差异是窗口内容观察的时间合同，不是 target 类型或 packed92。** 当前 `D3DImageTests.DirectConsumer.cs:174–185` 在 event→consumer flush→一次 DwmFlush 后只读一次 DC；当前保留的 HB 实现 `D3DImageTests.HbSurfaceConsumer.cs:190–207` 在相同 event/flush 后反复 DwmFlush/GetPixel，每次不符 Sleep(20)，最多5秒，最后仅记录最终 pixel。旧报告三色 accepted 因而不能证明首次观察已匹配，也不能证明与 direct 的首错停止具有相同时序要求。此差异并非 r2 擅自改动：direct 原预定义报告 `replacement-supervised-direct-consumer-kp20b3.md:6` 明确要求一次采样、不轮询。应纠正的是“复用HD合同”的过宽表述，而非偷偷恢复追色循环。

**未找到可确定造成此次首红失败的命令布局/资源类型/事件所有权/API错用。** event 到达不等于窗口已显示指定颜色；但源码的 SyncFlush 确实等待命令处理/合成，不能反过来武断声称它“仅入队”。一次 DwmFlush 也未建立指定提交到 window DC 的内容身份凭据。本报告不把观察时间差异直接升级为唯一根因，不归因 candidate、笼统环境，也不重开 native/线程/WGC 探针。

## 2. 证据身份、可取回范围与限制

相对路径根为 `WpfGfxShape/`；下文测试文件均在 `Tests/WpfGfxShape.ComAcceptance/`。

- **R（本轮亲读原件）**：`artifacts/direct-consumer-kp20b3-r2/readback/official/Results/direct-official-r2/`，`direct-consumer.jsonl:1–51` 全读；`supervision.json:1–40` 全读；`TestResults/child.trx:1–52` 全读。目录枚举没有列出 TestResults，但直接读取成功，不能据枚举断言缺TRX。
- R task=`c7993216b896464389746e0275b2229b`；test SHA=`3EB8DEB93C0BECE79BE24C2882C6E8256D0D9FC638FB401FE6CB74A697F269DA`，MVID=`57b2cf99-7ae6-4161-9b76-819de491536b`，UID=`1c362925-9d12-8b9c-a7bd-4ddbf15cb32b`。TRX8/19/45：真实1执行1失败，首红255/1973532，蓝绿未跑；监督34 Exit2、29–39无timeout/kill/drain错误。
- **H（历史报告引用，非本轮重核原件）**：`Docs/completed-work/replacement-supervised-hwnd-consumer-contract.md`，HD40–85、HE93–113、HF121–170。HD task=`9e32d79eddaf4449a0c8b936f3831604`，HE=`333b412deb784bc3bdea920f2a519ba7`，HF official=`e0036f3dbf3347bb8891145835f4c9df`、candidate=`71daf6c78a9c4cdabc67748c1c9d8e98`。
- 实际查询 `artifacts/hd-hwnd-contract`、`he-candidate-consumer`、`hf-x86-consumer` 均目录不存在；HF已知 JSON 路径直接读取也失败。无法重新读取历史JSON/TRX、包DLL/PDB、历史编译源码。本轮不能复算历史包API、原始命令字节、重试次数和逐次像素。旧报告“包字节保留可得”在本工作区当前不成立，须保留此证据可用性缺口，不删除历史结论。
- **S（当前源码确证，不冒充历史）**：当前 HB 全文1–247已读，它已具有 observePartition/outputRoot 等后来扩展；历史报告85/168声称整体 guard，而现源码仅10–19将两 TestMethod guarded，私有方法仍编译。此即不能将现源码视为历史编译源码的实际例子。
- **A（仓库官方合同参考）**：`src/Microsoft.DotNet.Wpf/src/...`，并非与实际 servicing 二进制逐字绑定的源码。已读当前执行规则全文、完整替换方案1–120、基线/事项最新节、恢复入口最新节和r2报告；无新增适用AGENTS/instructions文件，读了 .github/CONTRIBUTING.md 及 Documentation/contributing.md。global.json SDK10.0.107，项目net10.0-windows/Nullable enable，均未改。

## 3. 差异矩阵

“确证”指当前原件/当前源码；历史一律显式标H。没有抓取到历史字节就不宣称历史字节相等。

|核对项|此次 direct（R/S）|HD/HE/HF（H；当前HB仅S参考）|裁决/可信度|
|---|---|---|---|
|资源type及创建顺序|Direct106–117：surface→interop→initialize(0)→connect(0)→channel(connection,null)→image97/drawing43/visual39/target46；R2–11 HR0|H47/49、HF137相同；HB104–123同顺序|当前确证；历史字段相同为H。A Common/Graphics/wgx_core_types.cs830/834/837/888核定39/43/46/97，未误用generic47|
|image绑定|Direct118–120，Pack1 ImagePacket24，0:0x0A/4:image/8:bitmap UInt64/16:software0，发送前AddRef|HB124–126相同；H47说明native偷取AddRef|当前字段相同；历史实际指针/字节unknown。A include/Generated/wgx_commands.cs46–60证布局|
|draw/shader|Direct121–124：60字节DrawPacket，头12+record48；record size48/command0x47，X/Y0，W32/H16，image/padding0；0x22 visual→data|HB127–130相同；H47同图|本门没有shader、Effect或sampler绑定，不能拿旧shader门当同场景。Pack1定义见D3DImageTests.cs702–707；命令0x47见A wgx_command_types.cs99|
|target92完整字段|Direct125–143先Clear92；0:uint0x31/4:handle/8:UInt64 HWND/16:section0/24:master0/32:width32/36:height16/40–55:RGBA(0,0,0,1)/56:flags12/60:bitmap0/64:stride0/68:format0/72:context-2/76、84:double1|HB131–159相同构造；H42–44/71/HF137同摘要|当前构造确证，R15证92/12/-2/1；完整原始92字节未落盘，两时代不可做byte-exact认证。A wgx_commands.cs305–323偏移匹配|
|窗口设置72|共享OutputWindow.cs73–83：0x33/target/8:screen RECT/24:layer0/28:transparency0/32:alpha1/36:child0/40:RTL0/44:enabled1/48:colorKey0/64:cookie0/68:gdiBlt0|HB160共享调用；H45记同合同|当前确证；A wgx_commands.cs331–346匹配。无边框窗口windowRect=client屏幕rect；当前R未序列化完整rect|
|root/初始invalidate|Direct145–150：0x35/target/visual12字节；setup close/commit/flush；0x37/target/RECT(0,0,32,16)24字节再close/commit|HB161–169相同；H46/HF137同|当前确证，R17–23实际HR0；历史字节unknown|
|async/channel/批边界|Direct60–71/110–112：connect byte0、channel第二参0；setup独立flush，invalidate单批，再Present separate=1→commit，无另加close|HB62–73/112–113/163–189同；H48–49|当前无确定差异。A PresentationCore/InterOp/D3DImage.cs653–719证独批理由，不能改成普通send；单async不是WPF双channel完全复刻|
|Present16/时间/flags|Direct170：0x0B/image/UInt64 event，大小16；无时间戳、无Present flags字段；发生在ColorFill/GPU完成/dirty后|HB187同；H48|不要混淆flags12（target flags）、send参数1（独批）与PresentPacket字段；本packet不存在时间/flags。若要求扫描呈现时间需另合同，本轮不添加|
|事件duplicate/Wait|Direct159–177：manual-reset原event Reset；SafeWaitHandle临时AddRef、DuplicateHandle SAME_ACCESS2/非继承；原句柄等待5秒，duplicate交native不caller close；R27–30 duplicate1820/signalled true|HB177–193相同；另HB209–211每色后有Reset/Set/WaitOne(0)自检，direct无|核心当前相同；自检差异不构成内容栅栏。A resources/d3dimage.cpp248–285：即使Present失败也SetEvent/CloseHandle。失败send所有权unknown，不能因HR失败补CloseHandle；本次成功，未测逐柄exact-close|
|producer format/填充/同步|Direct104–108：D3D9 SDK32、HAL1、behavior0x26，32×16、swapEffect1；CreateRenderTarget format21 A8R8G8B8、MS0/quality0/lockable1、无shared handle输出；Interop96dpi/version7/fallback0。156–158 ColorFill→CompleteNativePresent→dirty|HB103–108/173–176同；HF137 producer21|没有显式DXGI共享句柄/keyed mutex合同；interop内部共享/复制，不能因未AcquireSync判错。共享OutputWindow.cs49–70用D3DQUERYTYPE_EVENT8/Issue1/GetData FLUSH1等待GPU，5秒截止；本轮未读producer surface，R25 HR0不是byte-exact内容证据|
|HWND/style/几何/可见|Direct97嵌套owner Invoke，CreateOutputWindow本身也Invoke且同线程会inline；共享OutputWindow28–45：exstyle8 TOPMOST、style0x90000000 VISIBLE/POPUP、40,40/32×16，无brush；GpuReadback68–90 Show/SetWindowPos/Update/覆盖检查；TRX10 target=cover，screen56,48|H50/71/104/HF137同位置/尺寸，HB98调用同helper|当前明确；历史style/完整helper编译源码unknown，不假定未变。嵌套Invoke不会必然死锁：WindowThread71同owner直接执行|
|DPI context|Direct129–139用AreDpiAwarenessContextsEqual映射合法-1..-4，GetDpiForWindow/96；R15为-2/1|H44/71/HF137同|当前与历史记录值相同；原始opaque handle未记不影响协议enum。无Invalid0确定错误|
|线程/owner泵|R各业务NativeThread11016，窗口owner5992；R50销毁owner/actual/current=5992。WindowThread40–83独立STA持续GetMessage，业务Wait不阻owner|H50/104等记相同专用owner模型|当前确证；历史具体共享实现unknown。ready.Wait/Invoke.Wait/Join无本地截止，外监督120/180；本次没有卡住，不以此另开线程门|
|采样坐标/时间|Direct178–185一次DwmFlush+DC(16,8)；R30 event 02:00:16.6734085、31 flush16.780234、32 DwmFlush16.9199396、33像素日志17.1782298|HB195–205为5秒循环，每20ms重读；H仅保存最终accepted摘要，H73/HF148–150红蓝绿匹配|**确定观察策略差异（S）；历史采样次数/首值/首值时间unknown。R记录时间在日志点，不冒实际GetPixel瞬时时刻。** R33首红非255不能推出5秒仍不符|
|新鲜性|R只有frame0且不符；后两帧0覆盖|H73/HF155后蓝绿变化支持consumer更新；native DXGI QPC只属于另外HWND前置|不能把历史native fresh QPC当consumer栅栏，不能把当前event/HR当新内容。consumer原始target surface/backend均unknown|
|前置及状态|Direct无fresh-native步骤；只创建consumer窗口|H52/67/HF155先独立native红蓝红再销毁/consumer；HB84–97保留前置|确定场景不同；前置带来运行状态/耗时差异仅推断，不要求恢复native、不把它设产品门|
|实际official/架构|R1 official5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834，X86/.NET10.0.13；PresentationCore D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D|HF报告131–134同official/WPF SHA与架构；HD/HE为X64 official AD1C9DBB…/WPF0E605CCC…|与HF报告同official身份，不是同测试DLL：HF C61C02D7…/e37ffa74…；与HD架构不同不强称同。历史资产本轮不可重hash|
|candidate身份|R TRX15仅初始化文件20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D；未运行candidate|HE actual8F74588B…/HF75D136F7…见H98/133|旧候选SHA成功不能认证新20B3。direct46–58 candidate实际加载时还硬校验20B3，不可拿旧candidate直接套入冒当前门|
|guard/包API|Direct1–17/261整体CONSUMER_DIRECT_DIAGNOSTIC，csproj20–29显式配置及该配置10.0.13元数据；离线读取当前clean包D3DImageTests公开API确有两Direct方法|H7/160发布HB_SURFACE_CONSUMER，当前HB10–19只guard TestMethod；历史包缺失无法核API|当前guard/API确证，历史仅H，不声称可运行旧包；生产/default配置无本次新验证|
|cleanup|R34–51资源4→1 Deleted1/HR0、四refs0、销毁1/Errors空|H75/106/153–155成功|当前清理较HB finally逐断言改成逐项尽力/保留主异常，属独有实现差异，不解释清理之前首红|

## 4. 确定误用与最小修正表（本阶段不实施）

|项目|确定性|最小处置|
|---|---|---|
|“与HD完全同合同/历史通过可支持首次采样”|证据使用错误已确定：当前HB195–207具有颜色条件循环，旧原件/编译源码又缺失|修正文档认定；保持r2历史失败原样。不得倒填历史首次即成功|
|把event=true或flush0作为指定窗口红内容栅栏|合同推论不成立；native ProcessPresent Cleanup可在失败时signal。没有证据认定此次真正Present失败|不要放宽像素，不把HR/event判绿；用固定、预先约定的观察时点区分“首次未对应”和“持续未对应”|
|target type/布局/资源图/绑定/独批/dup参数错误|未发现确定误用；原始命令流未记录，历史编译源码unknown|不改协议、flags、format、资源type和顺序；不做无证据生产修正|
|共享无限等待|静态存在，非本次失败根因证据；已经外层监督|如实保留，不为本轮扩改共享helper或重开线程诊断|

## 5. 尚未排除的假设（最多2个）

1. **观察过早/延后才可见**：相同合法consumer提交已被处理，但首次window DC仍非本次颜色；历史最终匹配可能依赖其既有5秒观察窗口。当前只有一次失败观察，不足证实或排除。不是宣称DwmFlush必然异步失效，也不是宣布产品缺陷。
2. **窗口端持续未收到该内容**：producer→interop→target合成/窗口更新链中尚未观测的状态使红在固定期限内仍不匹配。producer真实内容、consumer backend/target内容、历史前置造成的状态差异都未独立确定；不将这些打包命名为“环境”，亦不归因候选。与假设1的区分必须由既有window DC的固定时点实证完成。

## 6. 唯一下一门建议：同official、同提交、固定两时点时间对照

没有确定协议错误可先修，故选择**基于当前已可用合法official consumer设施的最小固定时间对照**，而不是运行不可得的旧包。历史HF包当前目录不存在，直接复用旧包不可行；即使未来取回，带fresh-native前门也不满足本次继续主线要求。当前RunHbConsumer(observePartition:true)虽可绕native，但附带partition义务，亦非本门最小设施。

建议另授权仅在现有 `D3DImageTests.DirectConsumer.cs` 同一official路径增加一个默认关闭、精确过滤的固定时间对照入口，沿用现有资源/提交/cleanup和DC API，不改公共helper、不新建Host/捕获设施。**只提交一次红，不重发、不重dirty、不按颜色决定等待次数**：保留event/flush/DwmFlush后首个DC原值；从首次实际GetPixel时以Stopwatch固定截止+5秒，无条件到时再DwmFlush并第二次GetPixel(16,8)。只记录两次实际采样QPC及预定截止、沿现有JSON写原值；无20ms追色/无颜色条件循环。两个点之间不增加consumer提交，不运行native/WGC/线程探针。

这是缩减的时间对照诊断，不伪装三色验收、不覆盖r2失败：
- 首次错、固定末次严格255：排除“该次红始终不达窗口”，支持时间合同差异，仍不证明旧历史首次是否错；后续应再由新授权决定正式三色验收的合法完成条件。
- 两次均错：固定5秒内延后可见假设未获支持，立即停止，不延长、不重跑、不按颜色追绿；保留具体失败以继续消费链主线调查。
- 两次都红：仅本固定控制通过；不凭新首次通过抹去r2失败，也不推断唯一因果。
- 首次红末次错、等待/event/HR/cleanup异常：记录并停止，不换参数追加。

**范围和预算（建议，非本阶段执行授权）**：仅一独有partial（必要时一个独有resx键）及文档；保留现ConsumerDirectDiagnostic/config/TFM/runtime10.0.13、官方根及候选20B3原包绑定，不新增打包规则，不改原两方法断言。正常COM build≤1、COM publish≤1、self-contained win-x86独立clean输出；显式AotDllPath仍取KP clean20B3，Native publish0；official精确FQN沙箱任务≤1，监督120秒/外180秒。candidate0、发现0、x64/full/resize0。构建或发布失败即停；实际official SHA/WPF SHA/架构与本R不同、TRX非恰一真实体、入口guard/API或目标身份不符均停止；无失败后修码追加预算。两次观测不依颜色增减。没有同包当前候选独立实际运行/身份/三色与cleanup证据，旧SHA成功或该official控制均不能认证20B3。

## 7. 账本与覆盖不变

- direct历史：独立build2（1败1成）/COM publish1/沙箱official1失败/candidate0；真实体1失败、蓝绿未执行。此阶段新增全部运行预算0。
- WGC历史：build4（1败3成）/publish4（1败3成）/沙箱3（发现1真实2）/真实2失败；退场后default build1成功另列。WGC已退场，不重开。
- 退场残项：通用ReferenceCopyLocalPaths、FullPath去重、PackagePath/Copy与manifest引入前可信快照缺失，仍保留，不称全部打包差异恢复。
- KS另账：build1/publish1/沙箱4/请求3/发现1/体1/A1/B1/native2，未混入direct/WGC。
- KN/KO双架构各17条有限resize/lifetime历史保留；本轮不产生内容成功。原ID resize像素、Available/真实backend、真实loss/recovery、media/resources及最后源码对应最终四格仍Open。旧full1814/40与另4不相减，完整替换未完成。
