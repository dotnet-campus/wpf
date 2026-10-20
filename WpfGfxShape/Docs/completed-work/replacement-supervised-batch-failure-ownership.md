# CL：batch发送失败所有权专项

## 1. 裁决与范围

**当前仓库官方源码的同线程失败路径已闭环到use-after-free（UAF）；再次delete是有条件的后续路径，不是已证明发生的二次free。尚不能绑定BN实际官方DLL，更不能认定为BN的0xC0000374根因。候选未发现同构的已释放batch指针保留机制，没有生产修复。**

先创建本报告后调查。仅新增本报告和状态JSON；未改生产、NativeRun、ExitDiagnostics、原VisualCache/historical、捕获、CF/CK、项目打包或共享交接。没有新增诊断测试，没有终端、直接/间接脚本、构建、发布或沙箱执行，不重复desktop/fresh/full，不拿官方冒充候选。

主产物根为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\cl-batch-ownership\`。初始状态文件误写到外层工作区的 `artifacts/cl-batch-ownership/audit-status.json`，最终同步为指向主根的状态；该额外文件不含运行证据。

## 2. 已读规则与证据等级

分段读完current-execution-rules27行、完整方案120行、current-work-item31行、CJ122行、BN86行、BL65行；初次长输出截断处补读。枚举解决方案及正式ComAcceptance文件，读取csproj、global.json、Directory.Build.props/targets。net10.0-windows、Nullable启用、SDK10.0.107/latestFeature、MSTest4.0.1，均未修改。名称查找未发现AGENTS。

C#符号索引对SubmitBatch无结果，CommitChannel只返回无关tracing/Ink符号；未作定义证据。以下官方路径以 `src/Microsoft.DotNet.Wpf/src/WpfGfx/` 为根，候选以 `WpfGfxShape/Code/WpfGfxShape/` 为根，行号均为本轮亲读当前快照。

BN/BL运行数字仅引用报告，**本轮没有亲读其原始supervision/module/stages/TRX**。名称查找bn-x86-sampler无结果，工程artifacts列出bx–ck等目录但没有BN根。递归按wpfgfx_cor3.dll查找只返回 `.dotnet/shared/Microsoft.WindowsDesktop.App/10.0.13/wpfgfx_cor3.dll`，未发现历史BD/CF候选DLL。不声称亲核历史actual identities，不重新发布并发生产来冒充旧候选。

## 3. 官方版本与编译条件核查

BN报告记载官方ProductVersion `10.0.13-servicing.26504.104`、FileVersion `10,0,1326,50504`、commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。历史官方SHA：x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`；x64 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`。两架构首次Commit为0x80070006，随后Destroy未记录返回并以0xC0000374退出。以上不是CL新测身份或结果。

当前未取得仓库修订或该commit归档源码，无法证明工作树等于上述servicing源码；本地同名DLL的目录版本也不能证明SHA/架构/源码一致。未认证安装DLL的编译宏或指令，不把仓库源码缺陷直接冠为该官方版本bug。

源级：uce.vcxproj:90无行内条件包含samethreadcomposition.cpp；apifunc.cpp:186–198由同步bool选择SameThread；connectioncontext.cpp:614–634据marshal type创建same/cross composition。Commit:558–575、SendBatchToChannel:356–384、ProcessCommandBatch:495–509的关键所有权语句没有DEBUG/PRERELEASE包围。

composition.cpp:26–27将两个FUZZING开关设0；417–468的错误转S_OK分支还要求对应开关、PRERELEASE且!DBG。generated_process_message.inl:235–240仅PRESENT尺寸检查受DEBUG控制，资源验证与ProcessPresent调用245–258不受该条件控制。不能用私有fuzzing分支解释默认快照，也不能据此认证安装二进制宏。

## 4. 精确所有权状态机

|状态/转换|实码依据|内存与错误传播|
|---|---|---|
|S0录制→closed|clientchannel.cpp:351–358、519–539|Close把有效B存入closed[i]，m_pCommands=NULL。|
|S1提交|clientchannel.cpp:562–569|先读B.GetFreeIndex并flush handles，再Submit；清槽在IFC之后。|
|S2路由|connection.cpp:386–396；connectioncontext.cpp:370–383|查server失败直接delete B；成功则调用server.Submit，随后无条件pBatch=NULL，再检查hr。|
|S3同线程消费|serverchannel.cpp:99–102；samethreadcomposition.cpp:104–136|设置channel引用；MIL_THR保存ProcessPartitionCommand的hr；FlushChannels(false)为void，不覆盖hr。|
|S4失败并释放|composition.cpp:166–176、395、475–509|处理错误仍SetChannelPtr(NULL)、delete B，然后返回失败hr。不是未接收。|
|S5失败返回|instrumentationapi.h:507–515、707；clientchannel.cpp:568–574|IFC失败goto Cleanup，跳过569/571；closed[i]保留已释放B数值，count未清。|
|S6 Destroy再访问|clientchannel.cpp:773–788、812–824；connection.cpp:330–333|Destroy→SyncFlush→Send sync命令→CloseBatch→Commit。若到达第二次Commit，564再次读closed[i]->GetFreeIndex：源级UAF。|

这不是仅依据“失败也转交”注释：实际delete、局部清NULL、错误跳转和后继再次解引用已逐项核对。CMilCommandBatch析构虽为空（cmdbatch.cpp:49–51），但继承CMilDataStreamWriter（cmdbatch.h:29），基类析构memwriter.cpp:64–66调用FreeResources，89–99释放数据块；cmdbatch.h:41还有自定义堆分配声明。不能以空析构否认释放。

ProcessPresent:279–285无条件SetEvent/CloseHandle；instrumentationapi.h:109–128显示失败会写hr。同步Event=0若实际到达此处有返回E_HANDLE的具体来源。生成路由:251–258先验证资源，不能断言所有失败包都进入Present cleanup。

## 5. 再次free：已证与未证边界

第二次Commit不是立即delete：先读已释放B，可能在GetFreeIndex、handle flush、GetChannel、SetChannelPtr或commandType读取处失效，也可能地址已被新分配复用。若旧地址仍可访问并再次到达SendBatchToChannel失败cleanup，或再次被路由到composition删除，就存在第二次delete同地址的控制流。**这是条件性重复释放路径，不是运行中必然二次free，也不是首次堆破坏位置证据。**

多batch另有不同分支：较早batch成功后槽置NULL，后续batch失败阻止Reset；下一Commit从0开始会先解引用NULL槽。不能混称为失败batch二次free。

排除“Destroy析构自动逐项delete closed”：clientchannel.cpp:107–127只显式delete m_pCommands；closed为DynArray<CMilCommandBatch*>（clientchannel.h:224–225）；dynarray.h:30–40按sizeof(T)存储，dynarrayimpl.cpp:112–118只释放数组底层m_pData。本次锁定的是Destroy的SyncFlush再次Commit，不是数组元素析构。

异步不可照套：crossthreadcomposition.cpp:127正常入队返回提交hr，worker命令错误不等于原Commit同步失败。zombie拒收167删除B，151的PostMessage失败可使Submit失败，亦需考虑悬空槽；但不是任意zombie必失败。未证明BN执行异步分支。

## 6. 候选实现独立核查

UceChannelExports.cs:405–419同步调用State.Commit，异步交scheduler，不使用官方CMilCommandBatch/DynArray。SameThreadChannel.cs:426–513先TryDequeue再处理每个batch，失败写Partition.Failure，仍遍历，并在finally对未转交packet ReleasePacket；返回失败时已出队batch不留在_closed。

PartitionScheduler.cs:87–99锁内EnsureCapacity→DetachBatches→Enqueue。SameThreadChannel.cs:404–409把_closed换新队列后返回旧队列；worker:145–176处理Work，destroy在finally调用State.Destroy。UceChannelExports.cs:181–214销毁同步State或提交destroy Work，不重用官方失败槽。

SameThreadChannel.cs:582–629防重复Destroy并释放剩余_open/_closed；ReleasePacket:561–572在释放interop/software或完成event前清包字段。GeneratedD3DImageResource.cs:130–147在Present finally完成event，正常转交packet不由ReleasePacket再次补放。

以上排除当前候选源码的**同构原生closed-batch悬空重放机制**，不证明全部OOM/reentry/异常路径安全，更不证明CF/BD旧DLL行为。空事件返回码差异不是本轮修候选的充分依据；生产不变。

## 7. 原夹具独立核查

当前VisualCache.cs:84 connect(1)；114–116给bitmap AddRef后发ImagePacket；123附近构造默认零event Present；252–256 Close后Commit记录结果再断言。421–431 finally先destroy、disconnect，再释放调用方bitmap。故首次Commit失败即可进入Destroy→SyncFlush再次Commit，不需假设夹具主动重试或提前Release调用方bitmap。

BN历史时间线与该源级UAF相容，但仍缺历史精确源码/编译配置、第一次B free地址/栈、Destroy内部第二次访问或free地址/栈。不能从最后阶段反推这些均已发生，也不能排除更早ABI或资源写坏。

## 8. 为什么不强跑

完整缓冲区、零原生指针的未知/错误命令可以造成解析失败，却不自动构成安全且有判别力的释放实验。同线程错误若按当前源码先free，正常Destroy会主动再入已证明的UAF；跳过Destroy又不能证明完整失败释放。异步解析错误通常不回传原Commit，事件信号不能观察B的分配/free次数。不能为更像堆损坏而使用坏ABI、违规句柄或无效指针。

尚未建立符合本轮限制、无需主动再入悬空访问且能区分first-free/second-free的公开调用实验。不是断言永远不可验证。本轮采用源码闭环分支；未以不复现证明不存在，不再重复desktop/full。无surface事件控制即使通过也不证明内容。

没有CL新测试SHA/MVID、actual module identity、退出、阶段、TRX；不借BL控制TRX冒充CL通过。未发布，所以没有新正常依赖图包、双架构矩阵或绝对沙箱参数可亲核；历史原始产物缺失已明确披露。

## 9. 结论分类与下一最小动作

确定事实：当前官方同线程失败先delete，Commit跳过清槽，Destroy的SyncFlush可再次读该槽；原夹具结构与此相容。候选当前源码先出队/换队列，不应无证套修。默认行为未变。

排除的假设：失败一律未接收；空析构等于没free；closed数组析构就是第二delete；异步worker错误必同步回传Commit；双架构历史同退出足以证明x86 ABI唯一根因。

未定链：历史官方精确实现、真实first-free与second-use/free、E_HANDLE实际命令、更早破坏、候选双架构失败释放及资源趋势。BN堆损坏根因与完整替换仍未完成。

下一最小动作：先回收BN最终x86/x64整任务原始产物及真实官方/候选DLL，取得commit4099f839对应源码与可核实编译信息，逐一比对上述UCE函数/宏。如已有dump或分配释放记录，先核对B地址first-free与Destroy访问，避免再运行。只有建立不主动触发悬空访问的公开诊断方案，才在正式exe新增独有CL显式编译隔离测试，复用未改BL监督，合法无指针命令错误和成功控制分项裁决。两模块×两架构分别记录实际身份，TRX参数须为任务绝对路径，整任务回收并亲读。没有这些证据，不宣称官方bug、候选修复或堆损坏因果已证。
