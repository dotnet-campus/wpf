# Typed/sharedref generation 与原生 owning 独立有限报告

## 交付、身份与结论

新增标准 COM fixture `Tests/WpfGfxShape.ComAcceptance/UceChannelTests.GenerationOwnership.cs`（167 行，低于 400 行），直接调用公开 DLL 导出；没有 Host、fake COM 计数、内部生产测试入口、skip 或 golden 改动。真实系统 WIC decoder/frame + 真实 callback stream 用于寿命观测。候选与系统原版各两种关闭顺序共四 case 全通过。已有机制正确，无有效生产 Red，不宣称生产修复或新生产进展。

生产 SHA256 `57CE56EA65057F8F609AD56E318B459A4248790C1231B5909DDD4DEA6E0A6F74` 未变，未重新发布生产。有限 generation/owning 条款 Closed，不意味着完整取消/异常注入或整个方法族、阶段5已经完成。151/154 已Closed，不重复领取。

## 原生与实际 managed 路径核对

原生每次读取不超过400行，未修改原 WPF：

- `core/uce/htmaster.cpp:67–150`：CreateOrAddRef 新建 master entry 为1；已有 handle 只增加 master refCount。`:265–333`：最后 Release 才排队 DeleteResource；排队失败保留 handle，避免 slave 碰撞。
- `core/uce/htslave.cpp:313–355`：DuplicateHandle 复制同一 slave resource 指针并 AddRef，不复制 bitmap generation。`:375–394`：DeleteHandle Release 该 entry 的 resource。
- `core/uce/composition.cpp:1377`：Channel_DeleteResource 按 exact type 校验后删除。该逻辑和 master handle 引用不是外部 COM stream 引用计数。
- `core/uce/apifunc.cpp:718–754`：SendCommandBitmapSource 额外 AddRef 用于 transport；入队失败在 Cleanup Release，成功把此引用交给 slave。
- `core/resources/bitmapres.cpp:60–88,157–160`：ProcessSource ReplaceInterface 更新拥有引用，随后 Release transport；析构释放当前 bitmap。
- 实际 managed caller `include/exports.cs:583–645` 的 DUCE.Channel.DuplicateHandle 要求提交源 channel；ReleaseOnChannel 返回 master 是否已离开 channel。`:927` 的 SendCommandBitmapSource 调公开导出；本fixture遵循同一源提交顺序。
- 候选 `Abi/UceChannelExports.cs:339–366`：AddRef/queued/finally 与 transport 失败释放对应；`Core/SameThreadChannel.cs:239–302`：Duplicate 排入源channel、已有master加引用、最后Release排Delete；`Core/GeneratedProtocol.cs:681–710`：slave Delete/共享同一资源；`Core/GeneratedBitmapSourceResource.cs:91–147`：先获得新拥有引用，再交换 FrozenComSlot/Retire旧slot，finally释放临时及transport，最终释放当前slot。

这是一份所有权转移审计，不把实现内计数当测试oracle。COM Release 的实际返回值保存在 JSON，仅作诊断；COM不保证一般对象的返回数适于强等值断言，因此不自造精确原始refcount硬门。精确的是上述谁拥有、何时转移/回退，以及外部stream处置的观测序列。

## 完整独立合法 oracle

每个case新建connection/source/target、三份不同BMP真实decoder A/B/C。CreateOrAddRef两次返回同一source handle；A交接后Duplicate到target并提交源/目标。通过alias把同一个真实resource的generation换为B；释放A wrapper/frame仍保留调用方stream A。此时三个stream均未处置；最后外部stream A释放后只有A恰好处置一次，证明旧generation已从共享resource退出，不是一直等channel关闭才一起释放。

B调用方wrapper/frame/stream全部释放后仍存活。合法完整BitmapInvalidate packet开启BeginCommand，期间尝试交接C，原版/候选均返回 WGXERR_UCE_MISSINGENDCOMMAND `0x88980411`；EndCommand、Close、Commit正常完成，未发送畸形batch。释放C调用方引用后观察 `[A,B,C]=[1,0,1]`：失败transport无残留，既有B拥有引用未丢。

master第一次Release报告deleted=0且B存活。第二次Release报告deleted=1：

1. source先删除/关闭：target alias继续持有B，target最后关闭才处置B。
2. target先关闭：source master仍持有通过alias更新的B；最后DeleteResource提交就处置B，随后source关闭无重复释放。

最终均 `[1,1,1]`。这是同一共享resource的generation替换、旧外部stream独立拥有、master计数与slave alias独立拥有及失败入队逆向释放的组合证据，不只是两个无关resource分别销毁。系统原版走完全相同合法步骤和断言；不继续系统畸形batch的历史挂死实验。

## 最终编译、运行与原始证据

源码最后修改之后执行专用发布成功：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true`。仅发布fixture，小包 `WpfGfxShape/artifacts/com-acceptance-package`；现有项目分析器警告保留，无生产改动。

最终原物 `.coding-agent/sandbox-results/22ce68bdd0604abe90b5715915e78ff7/Results`：

- `generation-owning-final.trx`，RunID `32c3c510-e61b-43a5-924d-cb237428af2d`，15 total / 15 pass / 0 fail / 0 skip：新增owning四case + 路径身份11case。不是原154的重跑或158全related。
- `generation-owning-{False,True}-{False,True}.json` 四份完整调用轨迹、HRESULT、handle、Release诊断数及处置序列，均completed=true；两份候选SHA为上述生产SHA，系统原版另记其实际SHA。
- `aot-identity.json` 完整回读，Sandbox=true，加载Native/wpfgfx_cor3.dll，绑定上述SHA；实际运行.NET 10.0.1 win-x64，不冒充10.0.12。
- 只回收最终Results。首次把带目录的TRX文件名当参数未取得Results，不计成功证据；随后独立4/4诊断任务 `d9b1630190ab49339ad73a23eb87cd38` 完整回收，仅历史，不替代最终15分母。

扩展既有只读证据审计项目输入并专用publish/run；`.coding-agent/sandbox-results/9d626bea79c14cf6ac807ad105507488/Results/audit.json`完整解析5份TRX、138份JSON（包含本轮全部四JSON和identity）；`summary.json`已完整回读，AddedFailures/RemovedFailures均空。审计不参与生产调用。最终源码后无未测尾改。

## 同SHA全量复用与正式Open

仅新增4个fixture case，不改生产及GenProto/普通测试代码，合理复用前轮原始全量：`939dfe5ca6014ac2866fe1da8461a7ce/Results/typed-final-full.trx`，RunID `3f83cf82-5467-4731-8e2b-22d234ca4e33`，1594 total / 1561 pass / 33 fail / 0 skip，只跑一次。该full不含新增4case；新fixture静态分母应为1598，但未运行1598 full，不写为新full或推算通过数。related154/154、GenProto34/34与普通4224/1/4225均沿用前轮真实证据，不声称本轮重跑。普通仍缺指定10.0.12 DLL或依赖8007007E。

Closed限于以上合法共享generation替换、两种最后拥有者顺序、master非末/末Release、公开Begin/End guard入队失败rollback。不证明任意取消、COM QI/分配异常、ReplaceInterface中途异常、transport取消/worker异常注入的独立完整合同；这些具体Open单列，不以原版未知畸形恢复合同强求候选。既有SameThread/async callback关闭其他channel及worker退出证据沿用154/full，不重新领取。

四外部authority binding、ARM64、完整10.0.12、player protocol真实输入仍缺需用户；DLL进程驻留不卸载。HW可观测会话正常原版成功前置仍未成立，33失败不假Green、不全部归环境。全required/physical、同SHA strict、七阶段及E2E总门仍Red；本自主有限模块已交付，父级继续具体未证合同，不以总plan文档待办替代本交付。
