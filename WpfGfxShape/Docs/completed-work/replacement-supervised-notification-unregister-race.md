# HW 合法注销与在途通知受控交错

## HX终态：有限注销合同成立，类型5内容合同缺陷已证，未修复

HX仅独立读取既有HW证据及当前/官方源码，补同报告和三共享文档；无新构建、发布、沙箱、测试、上传、脚本或生产修改。不是把文档收口当完整替换验收。默认HW_NOTIFICATION_UNREGISTER显式隔离保持，HA COM/Stdcall与fill修复未动。四角色Passed不代表payload exact。

## 1. 原件、任务及读取登记

证据根：`artifacts/hw-unregister-notification/readback/{x64,x86}`（相对WpfGfxShape根）。
- x64 task `6d6698ebd4f24ee0b28d661ceca9d5ff`；父PID1928/监督9640；official9460/candidate10584。
- x86 task `82ae7e3fda164265ade7a96a986851de`；父PID9440/监督9380；official8732/candidate7876。
- 父TRX：`Results/hw-<arch>/TestResults/child.trx`，两份各26行全文亲读；各total/executed/passed=1、failed/timeout/aborted/notExecuted=0。绝对远端路径为`C:\CodingAgentSandbox\Tasks\<task>\Results\hw-<arch>\TestResults\child.trx`。
- 四份`Results/hw-roles/{official,candidate}/role.trx`各26行全文：各1执行1通过0失败；四份exit.json各16行全文，exited=true/Exit0，PID与业务相合。
- 两份supervision.json各39行、stdout各32行、stderr空全部读取：Reason=exited、子Exit0，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，OutputCollected/HasExited=true，SupervisorError=null。不虚构外层工具数值Exit。
- 四份business.json各386行，按1–65/66–190/191–315/316–386全文读取，包含实际模块/test/WPF身份、38项原子事件、两条完整64字节hex和最终清理结果；两份Results/aot-identity.json全文读取。不是仅搜Counters或读JSON头尾。
- 最初名称搜索没有列出父child.trx，后按监督/stdout给出的确切路径成功读取；该早期搜索不作为原件缺失结论。
- 执行规则27行、完整方案120行已分段读全；HW初稿7行、HwNotificationUnregisterTests205行（截断处定点补读）、HU/HS报告80行及最新HV报告57行已读。共享长历史仅定点读取并保留，不冒全文历史审计。

## 2. 实际身份（均为原件记录值，HX没有重算hash）

实际.NET10.0.13，X64指针8/X86指针4。业务loaded.path按Process.Modules BaseAddress匹配NativeLibrary句柄，不是预期包路径替代实际加载。

|项目|x64|x86|
|---|---|---|
|test SHA|A9986EBE93A509960BB6FD98B5A9810C02616C036B3D09D9CB006B823F01AD48|94F76770141FB34BC92EF9027900FA33ABF56EB05BB66EEDEEF9A86FBF5A848D|
|test MVID|da02e1b8-8c01-45ab-a87a-52a2e9198b0e|49d62faa-a7a5-4e8f-82d7-4171d8761d0f|
|official SHA|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|
|candidate SHA|8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0|75D136F71223DB9EA0830BFBF9CDFC0EAAB748B5067397B682E42323B698EC30|
|PresentationCore SHA|0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31|D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D|

各模块均在各task根；候选为Native/wpfgfx_cor3.dll，官方为根wpfgfx_cor3.dll，WPF为根PresentationCore.dll。官方版本`10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`；WPF10.0.1326.50504，候选/test1.0.0.0。两runtimeconfig均NETCore与WindowsDesktop10.0.13。业务未记录PresentationCore MVID，不补造。

## 3. 类型5结构、长度及真实差异

官方仓库源码路径以下相对`src/Microsoft.DotNet.Wpf/src/WpfGfx/`：
- `codegen/mcg/xml/Resource.xml:2008–2015,2094–2113`定义packed设备状态结构及MIL_MESSAGE union；`include/Generated/wgx_misc.h:1424–1430,1572–1591`生成结果与之相合；processed/wgx_core_types.h也有对应结构。生成文件未修改。
- `include/Generated/wgx_misc.h:928–933`：Normal=0、NoDevice=1、Occluded=2。`include/wincodec_private.h:36–43`的枚举宏带FORCE_DWORD=0xffffffff；Windows此ABI枚举32位，非nint/BOOL或64位指针字段。
- MIL_MESSAGE pack1：type uint32位于0，dwReserved DWORD位于4；union起点8；deviceStateOld uint32位于8，deviceStateNew uint32位于12。设备状态数据本身8字节，有效消息字段前缀16字节。
- 整体MIL_MESSAGE为64字节：最大tier成员CommonMinimumCaps4+DisplayUniqueness4+Caps40+Assessment8=56，加头8；已读生成Caps/Assessment十项/两项32位字段。size_t所在SysMemUsage成员x86为8/x64为12，均不支配union最大长度。两架构不是由自然指针对齐推出不同偏移。
- `core/uce/clientchannel.cpp:856–970`复制MIL_MESSAGE并以min(cbSize,sizeof(MIL_MESSAGE))写回；测试peek缓冲64字节预清零，retrieved是BOOL而不是长度。现有记录没有另一个“实际长度返回值”，64是传入和保存长度；结构源码支撑合同长度，不把hex自身当长度测量。

两架构、两个stage（retained/new-backchannel-without-window）完全相同的差异：

|偏移/语义|official四样本|candidate四样本|
|---|---|---|
|0/type|05 00 00 00|05 00 00 00|
|4/reserved|00 00 00 00|00 00 00 00|
|8/deviceStateOld|01 00 00 00（NoDevice）|00 00 00 00（Normal）|
|12/deviceStateNew|01 00 00 00（NoDevice）|00 00 00 00（Normal）|
|16–63/该union分支未用尾部|全0|全0|

原始完整hex保留在上述四份JSON的samples数组（每份两条）。差异是8和12两个实际状态字段，不是offset4 reserved、union尾部、padding、HWND唤醒消息wParam/lParam或别的消息类型。1/1不是计数、BOOL成功、HW Available或SyncModeStatus；表示当前NoDevice状态快照old==new。0/0也不是“无payload可忽略”，它向消费者编码Normal→Normal。

## 4. 状态与序列化根因，兄弟发送点

`core/uce/composition.cpp:44–46`构造初始化m_deviceState=NoDevice；`Partition_RegisterForNotifications:1243–1289`首次加入监听集合后，以m_deviceState同时填old/new，再经NotifyHelper广播给所有已登记channel。重复enable不重发；disable移除并Release。这里是partition状态，不从系统当前显示可用性随意写常数。

同文件`ProcessRenderingStatus:2225–2270`保存old、根据NORMAL→Normal、DEVICE_LOST/RELEASED→NoDevice、OCCLUDED→Occluded更新m_deviceState，仅状态变化时填old/new再NotifyHelper；loss/release同时通知VisualCache失效，occluded调度组合。上游包括rendertargetmanager HandleRenderErrors失败、HandlePresentErrors状态及设备释放路径，composition自身loss路径。NotifyHelper:2280–2289→serverchannel:180–186→transport→clientchannel复制完整消息。该状态变更发送点与注册快照必须共用真实状态语义，不能只补注册常数1/1。

候选当前`Core/SameThreadChannel.cs:112–115`确切为新增集合成员时`channel.PostMessage(0x05)`；`232–251`的uint重载分配64字节零数组，只写type，byte[]重载仅入队、PostMessageW及SetEvent，不补状态。`455–471`将命令3的8字节合法包路由到此方法，已与HW Submit(type3,enable)核对。因而缺失old/new写入已由静态闭环确定，且与实际HA候选原始0/0一致；不是只猜测“可能缺失”。

当前SameThreadPartition没有官方对应m_deviceState及ProcessRenderingStatus联动；Compose只处理HRESULT/Failure、Targets.Render及present listeners。限定搜索发现此注册点是候选显式PostMessage(0x05)发送点，未证明所有动态/间接生成发送全仓闭包。官方同因兄弟是状态变更通知和多监听者广播：候选只向新增channel投递，当前代码亦不等价于官方NotifyHelper对全部监听者广播；这是静态合同缺口，尚无多channel运行反例。不得把D3D9单设备CheckDeviceState结构直接等同partition状态。

证据层级：安装servicing二进制10.0.13的实际差异已直接亲读；上述为当前工作区官方源码和生成模型解释，未检验工作区修订与commit4099f839逐行完全一致，不能声称已反编译或完全审过该servicing源码。结构字段、初始化及状态机解释与双架构二进制样本吻合；本轮没有修复生产、没有验证硬件状态转换。

## 5. 有限注销偏序与边界

夹具用真实STA HwndSource，ParentWindow=-3即message-only HWND；Notification=0x8147/33095。不是可见HW render target，也不是HU析构callback。每角色锁内递增order记录线程/QPC，唤醒hook通过entered/release/exited显式控制。

四轨迹共同必要顺序：window-ready1→register-enter5/return6→first.submit7/send8→dispatch-enter（official10/candidate9）→unregister-enter11/return12→callbackexit13→retained.drain-complete16(count1)→partition disable17–21(count0)→再次enable22–27（无window但backchannel count1）→最终disable28–32(count0)→owner-barrier33→destroy34/disconnect35/deinitialize36→window-dispose37/owner-joined38。所有协议hr0、callbacks1、callbackReleased/ownerJoined=true、windowError=null。

official first.flush9先于dispatch10；candidate dispatch9先于first.flush10。二者都合法，不要求无关全序相同。四个实际HWND分别28247154/28312690/37094996/37160532，dispatch窗口匹配各自注册值，wParam/lParam均0。

官方clientchannel.cpp:707–715只在queue锁内替换窗口/消息；PostMessageToChannel仅empty→nonempty时PostMessage（不是SendMessage）。注销窗口不清backchannel，不撤回已经post到Windows队列的消息，也不等待正执行的hook。候选相同锁内替换及empty→nonempty PostMessageW满足本次有限语义。样本直接证明unregister-return可早于已进入callback的退出，旧payload仍能peek，注销窗口后重新登记partition仍可产生无window的backchannel消息。

未覆盖：注销前已post但尚未dispatch、注销并发后任意迟到、真实HW target、非零SyncModeStatus、device-loss、长期循环/worker/资源收敛。owner-barrier和一次最终callbacks1不是任意未来无迟到证明；测试循环上限16不是生产资源cap，也不是收敛证明。没有caller COM资源并已完成channel/connection/partition清理，不等于测得全部引用归零。

## 6. clean包与回收可得范围

`Tests/WpfGfxShape.ComAcceptance/ComAcceptance.Package.targets:33–54`已读：clean目录重建，白名单为正式exe/dll/deps/runtimeconfig、指定测试依赖、RuntimePackAsset、Contracts两文件、Native候选及显式ComAcceptanceReferenceInput；不是PublishDir递归全拷。该静态定义不等同本轮实际集合验收。

可得源包为`artifacts/hw-unregister-notification/{x64,x86}/com-acceptance-package`，回收单独在`readback/{x64,x86}`；Publish目录另存且含额外平台/coverage目录，不能拿PublishDir当clean输入。回收含Results预期运行输出，不是上传输入清单。

亲读回收清单头尾：x64共419行/x86共418行，正式exe/测试依赖/Contracts/根官方DLL/Native候选/aot.sha256存在；aot.sha256与actual加载记录值一致。两runtimeconfig全文已读。未逐行遍历中间清单、未做双向集合差分、未核实际RuntimePackAsset求值或全部源包与回收逐项身份、未核输入ReferenceData实际集合、未重算hash/未取得上传API历史记录。因此不能宣称clean白名单完整验收、完整输入输出集合相等或所有文件未变；本轮不为收口再上传/回收/运行。目录分离已核，集合及hash完整性仍未完成。

## 7. 下一唯一生产修复门（本轮未执行）

优先修复可证的CompositionDeviceStateChange payload/state合同，而不是重复HV相同native前置。最小修复以partition权威状态为源：初始NoDevice、真实渲染/释放状态映射、注册快照old==new、实际状态变化old/new及所有通知监听者共用序列化；不要在PostMessage(uint)全局补1/1，不用HasDisplays/fallback假装Normal，不掺HW consumer恢复修复。

在现有正式测试项目增加**默认启用、独立于HW注销guard**的内容回归：独立C#声明/解析MIL_MESSAGE，验证总长度合同、offset8/12的32位状态而不复用候选ABI；新建同样未渲染partition，官方/候选独立角色提交合法命令3，分别采集首次注册、重复enable、disable再enable；比较真实官方快照及相同前置的候选字段，并以官方构造NoDevice及已控制未渲染前置作独立语义锚，不能只有old==new/枚举有效（旧0/0也满足）。环境或前置不成立应暴露失败，不改期待值/放宽门。

旧HA候选在该固定前置应因Normal/Normal≠NoDevice/NoDevice失败，新修后应通过；这是从现有原件推导的回归设计，尚未新增或实跑，不能报red/green已执行。额外在同一内容任务内验证多channel新增监听时的快照广播及序列化器可区分old/new的纯C#负控，剔除padding/保留位的无意义比较；负控不冒真实HW转换覆盖。后续必要发布/两架构最小内容回归绑定新actual SHA，不扩大full/显示门，不改HW诊断隔离及HA/fill。

HV最新native expected00563412/actual001D1C1D在connect之前失败，共享迁移已撤，E_HANDLE/consumer本次未覆盖；显示线保留独立阻塞，暂停重复同一native门，不因协议payload修复声称显示消费成功。HU有限关闭结论保持，HS立即释放反例保留。

Available、HWbackend、loss、mediaresources、final四格与x64Event0继续开放；完整替换未完成。

## 历史登记（保留，不再作当前状态）

HW初稿曾记“进行中、尚未发布执行、生产0”，它描述执行前快照，现由上述既有两task原件更正；不是抹掉历史或把HX静态审计冒成新运行。初稿意图为先核公开注册/注销/投递消费合同，不把HU析构通知当composition通知、不预设严格墙钟边界，继续保留。

最后补记：HX只修改本HW报告和handoff/session-summary/issues最新HX节；两父/四roleTRX、监督/roleexit/四份完整业务JSON及身份/原始bytes均已亲读；报告全文与三共享受改节在写入后定点回读核验。无新运行、hash重算或生产修复，历史HV/HU/HS/并行内容不覆盖。
