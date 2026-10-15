# S-20261014-003：生命周期审查与最终证据收口（非完整模块交付）

## 本轮实际动作与产物身份

本轮直接回读最终publish后的本机TRX、包SHA及运行身份，审查真实HW缓存引用链，构建COM项目，再执行本机定向/full及沙箱短/full。没有终端脚本，没有修改生产C#、增加fake callback计数、skip或放宽断言；没有重复加入程序集DoNotParallelize。未新增HW callback寿命测试，不能据此认定该合同已完成。

生产DLL：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
SHA256：`03C7CD3B6EFAB3EF4696A032B687EDAF69594EC06CC87C46A801EA1BEF6DEEE8`。
最终发布后的TRX日志与本轮实际加载文件的哈希一致；包`short/aot.sha256`、两份旧包Results身份、本轮两份Results身份同SHA。不是依据预期前缀补写。生产publish已由前序执行完成，本轮没有再次publish，也没有补造前序发布日志。

证据根：`WpfGfxShape/Investigations/S-20261014-003-LIFECYCLE/`；所有原有证据保留，新增`closure-*`独立目录。旧`short`/`sandbox-full`目录没有找到原先沙箱TRX，故原先18/7及1355/29仅为输入报告，不虚构其RunID；本轮重新运行取得独立TRX。

| 实际回读运行 | total / pass / fail | RunID | 根下相对TRX |
|---|---|---|---|
| 前序最终publish后本机短 | 25 / 25 / 0 | 433c8596-d739-423f-bdfc-6310aba60b06 | post-publish-local-short/lifecycle-local-short.trx |
| 前序最终publish后本机full | 1384 / 1372 / 12 | bc7b5c0b-4aeb-42f8-945e-ae0ee6ae0c66 | post-publish-local-full/lifecycle-local-full.trx |
| 本轮本机短 | 25 / 25 / 0 | 30c606c1-4426-4beb-925e-90c472dd56ed | closure-local-short/lifecycle-local-short.trx |
| 本轮本机full | 1384 / 1372 / 12 | 36c88022-323b-4221-9ede-cf1685024f36 | closure-local-full/lifecycle-local-full.trx |
| 本轮沙箱短 | 25 / 18 / 7 | 637b565b-f982-41f7-b3dc-8c69b7ab9a09 | closure-sandbox-short/TestResults/closure-short.trx |
| 本轮沙箱full | 1384 / 1355 / 29 | 0ddef638-037c-4417-9099-22337d5affc0 | closure-sandbox-full/TestResults/closure-full.trx |

以上全部executed=total、notExecuted=0。计数回读行分别260、10529、260、10529、305、10608；RunID均在第2行。本机短第37行实际生产SHA及HW像素，第41行起真实旧/新像素；没有以原生Clear代替生产像素。本机full失败12项属于MediaExportsTests，原版`C:/Program Files/dotnet/shared/Microsoft.WindowsDesktop.App/10.0.12/wpfgfx_cor3.dll`加载失败（0x8007007E），不是HW cache像素失败；保留Red。本机full TRX第2175–2183行可重复核对首项。沙箱full为21窗口/呈现相关失败、8真实media通知deadline失败，不把二者混为环境已修复。

COM Release构建退出0、0 warning/0 error；`PresentContract.cs`声明及finally均有staging，先Release(staging)再device/factory，旧编译坏临时标签已失效。测试聚合构建退出1来自full测试失败，MTP测试退出2，不是源码编译失败。本轮本机复跑没有遗漏最后测试入口修改：仅把已有LocalAcceptance工程两个证据目录改为closure目录，随后执行。

普通4211 pass/1 fail/4212、GenProto23/23为本轮输入提供的先前结果，本执行未找到其对应最终TRX重新核验RunID，不能补造身份或绑定到AOT SHA；普通失败为原版10.0.12 runtime前置。GenProto23项通过不关闭C全部required evidence。

## 真实HW候选事务引用审查

审查：`Core/GeneratedBitmapCacheBrushResource.Hardware.cs`、`GeneratedHardwareCacheGeometry.cs`、`GeneratedHardwareVisualRenderer.cs`、`FrozenCacheRequest.cs`、`FrozenVisualTree.cs`、`FrozenBitmapLease.cs`。

- CapturePure保存brush/target/config版本并分别AddRef，纯拓扑冻结先于真实realization。请求销毁先Tree，再Config、Target、Brush；不能用普通托管强引用代替其资源Release合同。
- cache命中额外AddRef，候选初始持有1引用；目标创建/clear/Visit失败由finally释放局部target，不替换已提交cache。
- Geometry.Draw在ProductionDrawBitmap成功之后才Commit；Commit检查brush释放状态、target/config identity及三个版本，先给candidate增加cache拥有引用，再释放旧cache；draw finally释放局部引用。失效先清空成员再释放旧cache，避免从成员重新借用已退役对象。
- FrozenComSlot退休先清空拥有指针，外部Release发生在锁外；pin退役延迟到最后lease释放。该机制是SW/COM冻结拥有证据，不自动证明GPU cache callback。
- HardwareCache引用计数为普通整数，当前审查不能宣称跨线程并发AddRef/Dispose安全；不以程序集串行测试设置替代生产并发合同。未证实生产bug，未贸然改变同步或ABI。

本机真实HW公开AOT fixture是D3DImage/InteropDeviceBitmap→generated通道→凸几何BitmapCacheBrush→生产HWND。已有4个参数化用例覆盖真实像素、caller Release、命中、变更以及连续配置拒绝后恢复重试、后续变更；SW17保持原样。它没有在HW cache候选事务内部触发冻结/关闭/替换callback。

`GeneratedHardwareVisualRenderer.Draw`第102–105行仅接受GeneratedD3DImageResource图像，真实lazy WIC图像无法直接进入该凸几何fixture；shader-effect分支存在RealizeTree，但没有在本轮证明其与GPU cache候选内部callback时序相同。已有7个SW cache decoder动作、late-node冻结及nested-cache测试不能重命名为HW测试。资源不足时按授权不扩draw族、不加fake；HW callback成组公开AOT验收明确未完成。

## 原生呈现语义与观测界限

实际API证据：Windows SDK `10.0.26100.0/shared/d3d9.h:1975–1977,2017`定义`_FACD3D=0x876`、`MAKE_D3DSTATUS=MAKE_HRESULT(0,...)`及`S_PRESENT_OCCLUDED=MAKE_D3DSTATUS(2168)`，对应`0x08760878`，是success status而非失败HRESULT，不能解释成已显示像素。
原生`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/hw/d3ddevice.cpp:3332–3380`只在S_OK置PresentProcessed；OCCLUDED指设备正常但不可见，列举最小化、其他设备全屏及监视器休眠，并在窗口化分支Sleep/PostMessage后转S_OK。不能仅凭部分窗口遮挡猜该status全部根因。

本轮沙箱原生staging=FF123456，显示尺寸frontbuffer=FF000000；非Ex rawPresent=S_OK仍GetPixel黑，Ex部分原生对照rawPresent=08760878；另有CASCADIA窗口覆盖证据。短7失败含4个HW cache测试在原生校准前置失败、1个原生桌面测试、2个原生frontbuffer测试。生产路径未由这4项到达验收点，保持环境不可稳定观测/生产该次未知，不宣称生产算法已确诊错误或沙箱Green。一次短、一次full后停止相同实验。

## 可重复发布/验收入口

下面是入口记录，不是终端脚本；路径相对工作区，执行仅用专用发布/构建/沙箱能力。

- 生产发布：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。生产C#改动后必须重新发布、以新实测SHA重新跑同断言。
- 本机最终定向+full：构建`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/Stage4LifecycleLocalAcceptance.proj`，Release；实际Restore/Build/Test由已有MTP MSBuild入口执行，短filter=`FullyQualifiedName~WhenStage4|FullyQualifiedName~WhenNativeGpu`，结果为上述closure-local目录。重复时先选新的结果目录，禁止覆盖冻结证据。
- 包发布：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true`，初始25文件白名单，Native DLL/SHA/ReferenceData；不上传生产源码、obj或Host。
- 本轮沙箱源包分别为证据根下`short`、`sandbox-full`，已有包25文件清单逐项回读，工具只执行标准`WpfGfxShape.ComAcceptance.exe`。回收目录另存全部任务内容，包含Native、aot.sha256、package-files.txt、Results身份和TestResults TRX。
- 短参数数组：`--filter`、上述完整filter、`--report-trx`、`--report-trx-filename`、`closure-short.trx`；full无filter，文件名`closure-full.trx`。包为net10.0-windows/x64、MSTest4.0.1，沙箱实际.NET10.0.1；本轮短任务ca0be4a5e7084dd9be1dddf4ac0fc617，full任务99c9134d46414ee094eeb3f3e62d1e32，两份Results实际SHA同生产。

## 未关合同与唯一完整模块

A仍开放：真实HW callback冻结/关闭/替换寿命、真实CopyPixels/重绘命中失效计数、首HRESULT及锁失败、旧键恢复与候选后验、per-device、HW+SW全部可达消费者和同产物沙箱可靠生产观测。当前配置拒绝回归不等于真实GPU分配/draw失败注入全部完成。B generation/BadShader/fallback/OOM、C ARM64/layout/X86/history/physical closure及阶段5–7全部保留。凹轮廓/孔洞/stroke、通用lazy WIC HW draw不在当前有限实现完成范围。

唯一下一完整模块仍是“HW+SW Image/Visual/BitmapCache消费者冻结、候选事务与callback拥有寿命闭环”，并合并合法生产观测及最终publish/定向/full/沙箱验收；media独立Red和原版runtime前置保留，不抢成一个新的定位小轮。历史稳定SW SHA `20221306CB14179CA4AA3B0413F187AAEEC9CF2C60337FE6099C6637B97C5AAF`原位保留；本报告不是替换资格或全量Green。
