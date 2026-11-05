# post-failure attach x86 有限门

## 终态：official通过；20B3候选Create失败，已停止

本轮亲自新增一个正式partial及一个资源项，无生产/共享Api/项目配置/入口/依赖/SDK/Package.targets改动，无再委派。正常Release默认编入；源码在build/publish后未再修改。build1成功（133warnings/0errors，5秒），publish1成功，Native0；沙箱2，official1通过后candidate1失败；发现/重试/x64/full/显示/KN17/终端脚本0。不抑分析器。构建日志输出截断，不声称逐条复核全部警告。

### 包、身份与亲读范围

新独立目录 `WpfGfxShape/artifacts/postfailure-attach/{publish,clean,readback}`，publish明确绑定 `artifacts/kp-resize-pixels/com-acceptance-package/Native/wpfgfx_cor3.dll`，AotRuntimeIdentifier=win-x86；没有publish根目录。clean根aot.sha256由既有GetFileHash生成，亲读为 **20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D**。runtimeconfig includedFrameworks NETCore/WindowsDesktop均10.0.13。既有白名单/缺件检查成功，manifest共621行，亲读首45及550–621（含Contracts/Native/根摘要）；没有另加离线发现或全清单独立逐文件hash审计。官方根wpfgfx_cor3与候选Native/wpfgfx_cor3分离，正式NativeRun --supervise-exit及其等值filter参数已亲读，实际调用见监督原件。

两角色共同test SHA **1C56E8CB2F0679FFF67EB217708EFD1DAF969200201E4973802D6927963759BD**、MVID **cc7cfbfe-6b94-4400-814c-a299f138970d**、.NET10.0.13/X86/pointer4；监督与business一致。业务loadedWpf=[]，未观测到PresentationNative实际加载，不能把包内WPF版本冒成已加载版本；官方实际wpfgfx版本见下。

- official task **7e9e0e363f4547fbbd2e30cd73090d3e**，PID10664；requested=actual=`C:\CodingAgentSandbox\Tasks\7e9e0e363f4547fbbd2e30cd73090d3e\wpfgfx_cor3.dll`，moduleHandle=actualBaseAddress=1854210048；SHA **5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834**，version `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。
- candidate task **c9f330bdff8a479297b58a460383cf7c**，PID5872；requested=actual=`C:\CodingAgentSandbox\Tasks\c9f330bdff8a479297b58a460383cf7c\Native\wpfgfx_cor3.dll`，moduleHandle=actualBaseAddress=1832386560；SHA上述20B3，version1.0.0.0。
- 两份requestedEqualsActual=true；按真实模块handle匹配BaseAddress后计算actual文件hash，不用初始化候选hash替代official actual。

完整原件已先回收至 `artifacts/postfailure-attach/readback/{official,candidate}`，不覆盖。逐段亲读official business全267行、candidate全221行；各监督40行、stdout（12/22行）、空stderr、aot-identity全文；official TRX26行及candidate TRX错误正文/Counters均亲读。stdout中文解码乱码不作裁决依据。Diagnostic保留，未声称全读。

|角色|TRX TestRun id|TRX testId|executionId|业务自生成UID（不是TRX ID）|
|---|---|---|---|---|
|official|13a7bcde-6f2c-4177-a4f5-f94d09141828|1492e97c-4a0e-8e0a-b34c-d1512cc86f05|070fcfa7-03f6-40ba-9b5a-3f03f5c64648|4a99f2de81fa4216bf9fdf446f257d22|
|candidate|9ff6d78c-d6c6-4021-ab35-77f44cefb147|1c192168-0326-8ff6-be42-a78cea1f8ece|e38ced8d-7953-49f6-a58c-0f467fd74905|77900378d8e646d69c8c221994d24cf0|

TRX准确FQN均 `WpfGfxShape.ComAcceptance.UceChannelTests.` 加以下方法，非零测试：
- official `WhenOriginalFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure`，total/executed/passed/failed=1/1/1/0。
- candidate `WhenFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure`，1/1/0/1，原始断言expected0/actual-2003303418（late）。

各角色 `Results/postfailure-{official,candidate}/TestResults/child.trx`；同目录supervision.json/stdout.txt/stderr.txt，业务在 `PostFailureAttach/{业务UID}/business.json`；候选文件认证为角色根 `Results/aot-identity.json`。两监督精确等值filter/filtered120，外工具180；Reason=exited，Exit分别0/2，HasExited/OutputCollected=true，TimedOut/Cancelled/KillAttempted/KillReturned/两drain超时=false，SupervisorError=null。

### 原始业务与清理裁决

相同输入：Initialize0、connect async0；A source0；visual39 id1，完整alpha0x20/16字节/0.75更新flush0；bitmap95 id2 flush0；一次CompleteNullSource0x0c/12字节，Send/CloseBatch/Commit0。两者Order16 A.SyncFlush原始 **-2003303418=0x88980406**，之后才Order17 Create L。

official connection81750992/A81763072/L81762944；Order17 Create(source81763072) HR0/output81762944；18 L.flush88980406；19 Destroy A0；20 L.flush88980406；21 Destroy L0；22 disconnect0；23 deinitialize0。created/destroyed=[81763072,81762944]，逐个一次，stillKnown=[]，completed/cleaned=true，unknownRegistration=false，primary=null/cleanupErrors=[]。5份结果副本负控全部拒绝：失败create/零输出、L首次flush0、source关闭后L flush0、缺DestroyL、提前Create；不改真实产品调用，不是额外TRX测试。

candidate connection1/A2；Order17 Create(source2) **HR-2003303418/output0**，立即保留主断言，不执行L.flush或猜L值；Order18真实A2 Destroy0、19 disconnect1返回0、20 deinitialize0。created/destroyed=[2]，stillKnown=[]，cleanupErrors=[]；但 **unknownRegistration=true、completed=false、cleaned=false**。这些已知对象的正常返回不证明未返回的内部channel正常关闭；正常清理不能闭合。进程自然退出仅是隔离终结边界，不是正常资源回收；未Destroy0、未探测隐藏identity。candidate outcome=null，negativeRejected=[]不是候选负控通过，负控已由official完整结果验证。

### 已证差异与唯一下一门（本轮不修）

实测已证20B3在活source、同连接、失败已确认的合法Create上把普通partition错误返回给管理调用且output0；不是仅静态风险。未取得L，故候选L失败继承及source关闭后存活 **未覆盖**，不得推断也通过/也失败。未知内部注册是否残留不能由公开原件确定，不能报告已证特定hidden handle或精确泄漏数量。

只读复核候选 `Core/PartitionScheduler.cs:149–183` 与 `Abi/UceChannelExports.cs:138–180`：Register执行后completion仍被既有Failure映射；Channels.Add先于等待completion，result<0不写output。该静态路径与实测吻合，但不冒充内存/内部表直接观测。

**唯一下一门：另授权最小生产修正与同一x86合同复验。** 限于管理Register完成结果与既有业务partition失败分离，保持真实注册异常正确失败并核失败创建的回滚/所有权闭合；不得仅强行返回S_OK、写猜测句柄、清除partition Failure或放行普通batch。真实成功注册应返回可持有L，后续L自身SyncFlush仍报告既有88980406；真实失败不得留下未返回注册对象。先审清Register/失败回滚所有权，再授权新Native发布及新身份测试预算；本轮生产0改/Native0，不自动发布、不重试、不先转x64。复用本正式测试，不扩大错误矩阵。

双架构isolation既有通过（20B3/40D0对5EBD/AD1C）完整保留。direct累计build4(1败3成)/publish3/official3失败/candidate0；public独立1失败；WGC退场及通用打包残项、KS、KN/KO历史17/17、Ledger均独立不冲销。本轮只增加上述post-failure门的build1/publish1/两沙箱账。显示冻结，真实HW/loss/media/full及完整替换仍Open。

## 验证前快照（历史冻结，非当前执行计数）

本轮获授权亲自新增正式 UceChannelTests.PostFailureAttach.cs 及必要资源/文档；不改生产、共享Api、入口、配置、依赖、SDK、Package.targets。沿 Release 默认编译，official/candidate 同一单参数合同。当前 build/publish/Native/沙箱均0，尚无新运行结论。

冻结：async connection0，A(source0)合法 visual39 alpha0.75更新/flush0，bitmap95/flush0；一次完整CompleteNullSource，A flush88980406后才Create L(source=A)。Create应0/非零且不同A；L flush88980406，Destroy A0后L再flush88980406；Destroy L/disconnect/deinitialize逐项0。无S/B/新健康partition/其他错误/Peek6/HW/loss。

预算：normal COM Release win-x86/net10.0-windows build≤1；成功才publish≤1 self-contained NETCore/WindowsDesktop10.0.13，显式绑定KP clean Native20B3，全新独立publish/clean；Native0。核clean摘要/manifest/runtime/入口后沙箱≤2，精确official FQN先1；合同、actual身份、清理全过才candidate1；filtered120/外180。发现/重试/x64/full/显示/KN17/终端脚本/再委派0。失败即停，不修后重试，不抑分析器。

创建失败output0标记内部注册unknown，正常清理不能闭合；只清理所有真实已知对象，不猜hidden、不Destroy0。进程边界不等于正常回收。主异常保留，逐项清理不截断，清理中不throw。结果副本validator不注入产品。

官方源码已亲读apifunc.cpp228–260、partitionmanager.cpp345–395、crossthreadcomposition.cpp130–172：活source选择partition，zombie仅拒绝普通batch，管理Open/Close继续；新channel的自身flush通知保存错误。静态依据不替代official实测。既有双架构isolation通过和direct/public/WGC/KS/KN/Ledger独立账全部保留，显示冻结，完整替换Open。
