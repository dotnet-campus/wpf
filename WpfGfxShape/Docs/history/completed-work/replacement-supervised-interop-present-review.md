# DY：Interop / Present 独立证据审查与交接收口

## 1. 裁决与本轮范围

**证据审查收口，不是修复收口；没有新的生产修复，完整替换未完成。** 执行审查子智能体直接核查文件，主代理监督。本轮仅创建本报告并更新三份共享文档；构建、发布、运行、实验、full 均为 0，源码、测试、打包均未改。无终端、直接或间接脚本。

已读 current-execution-rules、完整替换方案全部120行、DR/DS/DT/DU/DV/DW/DX报告，长响应尾段补读。AGENTS名称搜索无结果。下文区分本轮原件亲核、历史报告转述和只读源码确认，不将报告作者的“全文亲读”继承为DY亲读。

## 2. 原始证据范围与可复核索引

根目录为 `WpfGfxShape/artifacts/`（相对工作区根，项目内为 `artifacts/`）。

- DR：`dr-interop-abi/{official-x86,official-x64,candidate-x64,candidate-x86}/Results/dr-<role>-<arch>/`。四份 supervision.json 全39行亲读；成功三格 child.trx 的 Completed/Counters 原件亲核，官方x86 TRX全文亲读。四格 dr-<role>.jsonl 的 actual identity 亲核；候选x86全6行、成功三格尾段30–39亲读，官方x86首14行亲读。不冒称成功三格业务全文均已亲读。
- DS：`ds-async-present/run-<role>-<arch>/Results/`。四份 supervision 全39行及四份 child.trx 的 Completed/Counters 原件亲核；四份 `ds-async-present/<role>.json` identity 1–21亲读。四份原始业务文件均检索到140/210/280行 Signalled=true、378行 cleanup complete；候选x86监督 stages.jsonl 全38行亲读。不冒称四份387行业务全文亲读。
- DW：`dw-aot-qi/run-{direct,generated}-x86/Results/`。两监督全39行、两份业务identity及内部 dt-interop-create/stages.jsonl 全17/8行亲读。final-default身份和结果本轮仅来自DW报告，未移作亲核原件。
- DX：`dx-com-callchain/run-{stage,direct,final}-x86/Results/dx-<variant>-x86/`。三监督全39行、三份业务全6行亲读；stage/direct内部 `Results/dt-interop-create/stages.jsonl` 各19行全文亲读。
- DT两次及DU三次的原始产物本轮未重新认证，仅阅读其报告。DV没有执行。旧DP full本轮仅继承DQ/交接历史，不冒称重新认证DP原件。

本轮所用关键原件仍可读；未取得/未亲读的原件不得以报告替代“亲核”。未来若原件丢失，应降为报告记录，不能拿本报告冒作运行原件或重新计算的二进制hash。

## 3. DR：三成功格与候选x86最小失败

|格|task|退出/完成|
|---|---|---|
|官方x86|ce4d6c062a77460d8d52f896bcebe2af|Exit0；Completed，1执行1通过|
|官方x64|03bd7901f627494783c25dbd981685d6|Exit0；Completed，1执行1通过|
|候选x64|5b50a24f69e144eeb697e56b2a55aa54|Exit0；Completed，1执行1通过|
|候选x86|c9c5265acc6548d881ff9c2cfabfae3c|Exit -1073741819 / C0000005；首次Create.before无after，不填完成通过率|

四监督均 filtered/30秒，Reason=exited；timeout/cancel/kill/排空超时均false，OutputCollected=true、SupervisorError=null，结果目录为对应task下绝对路径，不是工具超时杀进程。

候选x86原件记录CreateDevice/CreateRenderTarget/ColorFill均0，合法surface首次Create前停止。DR独立Marshal声明、无channel/Present/batch的测试设计见DR报告和原测试；该最小失败排除了“必须经过旧Event0/channel/桌面consumer才会崩溃”的解释，不证明与DP相同异常地址或共同根因。

成功三格末段有bitmap Release=0、surface/device/factory各0、callback根释放及completed。**这些refs=0只属于各自成功格，不可移绑候选x86、DW或DX崩溃格**，进程退出也不是COM正常收敛。

成功三格通知Count=0、NativeInvocationCovered=false、ConsumerCovered=false。Available实际调用/注销在途通知仍未覆盖；不能由候选x86Create前注册记录断言崩溃前绝无callback。

## 4. DR/DS实际身份与适用范围

原始identity中Requested=Actual：官方在各task根 `wpfgfx_cor3.dll`，候选在 `Native/wpfgfx_cor3.dll`；不是只看包路径。DR与DS复用相同历史候选，不是当前源码新产物。

|实际模块SHA256|x86|x64|
|---|---|---|
|官方|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|候选|7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA|F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7|

DR实际testSHA：x86 `44219DB431C505C7974F3ED714F44F4D48C4B23F650CA069454726871454403E`，x64 `726DB0F28C178CF645043E9A1C941FA897EEE79E4C5B1CA507CB189DDE4A6369`。
DS实际testSHA：x86 `8ECD1D35DCA103AC7B8FAAD40C2D92D1460A516A4C0CE18B32F3D9D18E250848`，x64 `FD592849E564842DFA1BF07AE3D39333B3CDBCEADF3D7CD7D8FA7D68A9C27787`。

runtime均.NET10.0.13；官方Product10.0.13-servicing.26504.104/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。DS另记录真实PresentationCore文件SHA：x86 D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D；x64 0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31。DR的程序集Version10.0.0.0不能替代servicing文件认证；不外推其它版本。

## 5. DS：合法异步序列不是消费证明

|格|task|退出/完成|
|---|---|---|
|官方x64|7e3093c7f6f54db4b8432c065087feb0|Exit0；1/1 Passed，Completed|
|候选x64|d1ceff4407d745cbbbec4398b98c74da|Exit0；1/1 Passed，Completed|
|官方x86|2682aec7533d4acbb9b2ba91ab49caa6|Exit0；1/1 Passed，Completed|
|候选x86|f6233b9409e24c8aad5d597cd853cfdc|Exit0；1/1 Passed，Completed|

四格filtered/30秒，均非timeout/cancel/kill，双流完成。初始化→异步connect/channel→资源→每次独立DuplicateHandle、separate Present、Commit、事件完成、SyncFlush→delete/flush→Destroy/Disconnect/Deinit正常序列成立。每格3次，共12次Signalled=true；原事件仍可用，cleanup complete。

四格identity明确Surface=false、ContentConsumptionProven=false、NativeCloseCountDirectlyObserved=false。事件完成不能证明像素消费，不能证明native duplicate直接计数恰好关闭一次，也不能证明失败态全部所有权、长期资源趋势或历史Event0/C0000374首错。DS提供共享夹具设计依据，不是已经合并并验收的真实surface修复。

## 6. DW/DX：返回边界与不同SHA

|亲核格|task|actual nativeSHA256|
|---|---|---|
|DW direct|e6acc1e87194407ab3094554a8f19c9b|294085410FA94A0B0F1C91E55F30B156C0ECC0B57C01FECD58B65F33AB5BF8F5|
|DW generated|69f0001db7c04656b274c523d93b72de|96B0CC245703E20F19D1CEC695CF32FD8B6ED8324F896CD8DCFBB01AB09F77D4|
|DX stage|3bce7c15b964423ea3eead0f7b37d5d3|77F605F3141DC90DEBA88518F633BFB8413A6DAE343EC1E4D7F85625B522CA0F|
|DX direct|220834eccd80445187fd091a91c864cd|239E749354DD0AD76E16BEE30E9F09AAA4A680CC0FC63133EFB9A5619F48904F|
|DX final默认|27caeb1c537d4e9e8de257e5c82fe366|0931D4911F8262B89B60731C22E115486E9AD24B5B877F3AB11F633DA7D3A616|

以上五监督均x86/.NET10.0.13，Exit C0000005，非超时/取消/kill，未完成Create，不以工具成功回收当Exit0；不移绑DR/DS旧SHA通过。DX final actual testSHA为05AF172BD947606FF3B254717EF62742191531DADC1180AB2B59A7CA3F637EEF，MVID 5c9cefa8-1933-43d7-a15f-6a4faa5f1fc5。

DW generated停CheckDevice.generated.QI.before；direct取得QI.after E_NOINTERFACE/null并通过TestCooperativeLevel、GetCreationParameters，到GetUpdateMethod.before。只证入口替换改变可达阶段，未完成Create，不足交付默认修复。

DX stage和direct均有GetDeviceCaps.after=0以及第二次GetUpdateMethod.QI.after=E_NOINTERFACE/null；两者都无GetUpdateMethod.after。**不能说第二次QI没返回。** after证明执行抵达日志点，不认证该调用或所有此前已返回调用的真实x86 ABI，也不排除此前栈/内存已经损坏。没有异常地址/栈，不推断首次写坏位置。

DU的IL calli Cdecl元数据仅精确绑定线索，JIT对照通过不能认证NativeAOT行为；不能直接定整个Silk库bug。DV误解执行角色而没有执行，属于未执行历史记录，不是平台能力阻塞；其要求新委派环境的下一动作已由DW实际执行覆盖。

## 7. 最终源码只读确认与撤回边界

亲读 `Code/WpfGfxShape/Abi/InteropDeviceBitmapExports.cs` 的TraceCreate、CheckDevice、GetUpdateMethod、GetAdapterLuid相关段；当前CheckDevice和GetUpdateMethod都调用生成device->QueryInterface，未跳过Ex能力检测。名称搜索仅见DT_INTEROP_CREATE_TRACE和DX_INTEROP_COM_DIAGNOSTICS的Conditional标注，未见DW/direct生产行为分支；默认配置未定义诊断符号，条件trace默认不调用。它只写诊断，显式开启时有I/O，并非运行零扰动认证。

撤回结论限定为已核的试验行为分支及相关调用语义；不是历史整文件hash一致性保证，不把版本Product提交当工作区diff。CF/CX/CY fill修复按既有交付保留，本轮未触碰，也不声称重新认证其整文件或重跑矩阵。无推测性默认修复，不吞HRESULT、不扩大分配、不绕GetUpdateMethod或Ex检测过关。

## 8. 下一最小根因切片与开放门

下一轮先逐slot独立认证当前类所有此前生成COM调用的真实x86 ABI：this、slot、约定、参数/返回宽度、输出缓冲大小及对齐、拥有引用与释放路径。重点包括GetDesc(12)、GetDevice(3)、device QI(0)、TestCooperativeLevel(3)、GetCreationParameters(9)、GetDeviceCaps(7)，并审查同类后续GetDirect3D/factory QI/GetAdapterLUID及Release兄弟路径。sizeof32/16/304只是当前日志，不是独立SDK/实际写入布局认证。

以有判别力的直接Stdcall替换分组控制识别“已返回但可能破坏栈/内存”的前序调用，独立缓冲边界/对齐核验与分组控制配合，而非仅追最后before。保持合法surface、原Create断言、必要Ex检测及完整cleanup；先最小无Present切片，授权后用新actualSHA和正式监督绑定，不能拿旧成功补新身份。无故障地址/栈时保持首坏写位置未知。

x86最小无Present Create崩溃与x64旧Event0/E_HANDLE堆损坏分两线，不盲full；旧DP无父完成TRX，不拼角色计数。Available实际通知仍0/未覆盖；真实GPU消费、loss/resize恢复、media、resource/线程callback/通知及最终Tests/ComAcceptance×x86/x64 Release四格全部开放。文档完成不代表修复完成。
