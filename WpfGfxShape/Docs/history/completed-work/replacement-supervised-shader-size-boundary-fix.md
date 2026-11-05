# KB ShaderBounds 真实内容边界

## KC 最终裁决：有限保留，不撤回
KB 的完整 bytecode 长度检查保留：保护开启（disabled=0）时 `bytes.Length > 1048576` 返回 E_FAIL；非零关闭这一大小限制。恰上限不拒绝，包含 version 与 END 的完整字节数，不是已翻译指令数。KC 未发现需要再改生产的语义漏洞，未重发 Native、未重跑已通过 COM 组、未重跑旧红、未跑 full。文档初稿落后于真实新绿，不据此撤回已验证修复。

验收仅覆盖本报告实际同输入软件 shader 边界及 BOOL 转换。不是整个 shader 兼容关闭，不认证原生白色的具体错误传播或 fallback；Available/HWloss/mediaresources/final 四格仍开放，完整替换未完成。

## 修改范围与源码审查
- KB 生产修改位置：`Code/WpfGfxShape/Core/SoftwarePixelShaderProgram.cs` Compile 的完整字节上限判断及解释注释。KC 不再改生产。
- 原比较与新增回归：`Tests/WpfGfxShape.ComAcceptance/ShaderBoundsComparisonTests.cs`；场景证据：`Pcore/ShaderBounds.cs`。原像素预期、零阈值、正式测试隔离/入口/打包未改。
- KC 发现 `Tests/WpfGfxShape.Tests/BoundsCheckProtectionTests.cs` 仍把 disabled=0、1048580 字节编译返回值预期为0，和新合同矛盾；只将这一 DataRow 改为 E_FAIL（0x80004005）。未改官方颜色预期，未改其它 DataRow/公共设施。旧方法名仍保留以避免无关 ID 变动。
- 真实调用链：导出 `WpfGfx_SetDisableBoundsCheckProtection(int)` → Volatile 存储 `disabled != 0`；Compile 读取 `!BoundsCheckProtection.Disabled`。GeneratedPixelShaderResource 的 Bytecode/PixelShaderGeneration → FrozenShaderInvocation.GetSoftwareProgram → GeneratedSoftwareShaderEffect.Capture → PixelShaderGeneration.GetSoftwareProgram → Compile。字节长度检查在 MemoryMarshal.Cast 和 token 解析前，不把 NOP 数或 Instruction[] 长度当资源大小。
- 空字节仍先替换原 Identity；普通短非法字节、版本/END尾随/截断操作数仍走原 Rejected=0x80070057。新增拒绝只针对开启保护的超大输入，因此超大且非法输入可先得到 E_FAIL，不能声称所有非法输入 HRESULT 均未变。关闭大小限制不等于取消所有原语法检查。
- 失败仍走既有 GetSoftwareProgram 非内存错误通知、Compile(default) identity 路径，未硬填白色；输入本来是白色矩形，成功常量 shader 输出红色。Generation.Release、冻结资源 Dispose、逆序硬件对象释放未改。不能用此源码推断官方返回了同一个 HRESULT。
- main 的 `WpfGfxSwitches.cpp` 默认false及 `!!fDisable`、`pstrans.cpp:291-318` ByteCodeSize 检查和 DWORD转换仅作辅助；并未把 main 绑定为 10.0.13 servicing 完整源码。主要行为依据是下述真实官方10.0.13同输入二进制结果；不另设必须找到旧 servicing source 的无限验收门。

## KB 已运行结果与监督（KC 亲读，不重跑）
证据根：`artifacts/kb-shader-boundary/readback/`。

| 架构 | task | 父 TRX | 监督 |
|---|---|---|---|
| x64 | f72497ed502b48da919068332cfc09ad | x64/Results/kb-shader-x64/TestResults/child.trx | 同目录上级 supervision.json |
| x86 | c6b516b0234f4ab1a3825eada7fec4c2 | x86/Results/kb-shader-x86/TestResults/child.trx | 同目录上级 supervision.json |

两个 TRX 全文各32行已亲读：各 total/executed/passed=2，failed/skip/timeout/aborted=0，Completed。原 ID `105b96c7-ae45-8225-b7ac-be58d2f59b51`、新增 `158e085b-4ca4-89c8-8e32-3eada8049688` 均 Passed。监督各40行全文及 stdout 全文已读，Exit0/exited、120秒内，TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut 均false，输出收集成功。不是仅角色 result.json 自报 Green。

x64 父 testSHA `1E3DFB7EF96C0BD4206354AC1C520089BADD0996816F8EF4FCA8C664B9E684E6`，MVID `97028319-f9ee-4c91-b3d1-491d91f62b54`；x86 testSHA `E61B7949055B7BD88BC2B0A4C10302A2BBE54087B71BF07C11C87112F51862E9`，MVID `368ab861-c9a1-4505-b1c9-db9d51d1507e`。运行 .NET10.0.13、Windows10.0.26200。KC 后述 managed 单测不是这两套发布产物，不移绑身份。

## 原始内容、输入与 actual 身份
下列路径均相对 readback，各下有 `Results/{original,candidate}/result.json`：
- x64 `Results/shaderbounds/1a3c9b0bcdeb4b61a32176b94e0f0ffb`：原方法时段；KC 逐段亲读整个业务消费者区280-1180、flags区1181-1341、actual及结束1342-1355，含31组各16像素、长度/wordCount/SHA/首尾token/invalidEvents；身份区235-279亦读。
- x86 `Results/shaderbounds/4079b7322e3f48b0b90f9000e2677b76`：同上业务区、flags区及 actual 亲读。
- x64 `Results/shaderbounds/fece254db10d436ab838996db727ad28`、x86 `Results/shaderbounds/baa61ea7ae3342929c7d6a918ad58879`：新增方法时段。KC 亲读两角色 actual/结束尾节及 candidate flags区；其31组逐像素比较通过由父TRX及同一方法调用证明，KC没有把这另四份消费者全文冒称逐行亲读。

actualNativeModules 来自 Process.Modules 枚举及文件SHA，不只 requestedNative。VerifyLoaded 要求恰一个、路径等于 selected；original resolver 在 WPF 之前选择 task根官方DLL，candidate选择 Native子目录。两时段 actual 相同：

| 架构 | candidate actual SHA256 / 字节 | official actual SHA256 / 字节 |
|---|---|---|
| x64 | E331526A9594E90BD5CB71414E45A6B055916205F7061FEEE731BB39A010324C / 4525056 | AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1 / 1956648 |
| x86 | 7E0936D24BBB7415DD2A8701AE260E8F5E5D8140E61E8510C3F5B6CD6DBA82DD / 4000256 | 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834 / 1807144 |

官方版本 `10.0.13-servicing.26504.104 @Commit:4099f83977f4d29604a9aeb971fb0a7bc1d5f699`，候选 `1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a`。同进程实际只加载所选官方或候选，虽JSON同时记 frozenCandidate 并不代表 original 执行候选。

shader 输入为同一场景函数创建 byte[] 后 SetStreamSource；NOP padding、最后mov oC0,c0和END，首token 4294902272（0xffff0200），尾65535（0xffff）。两角色原始记录的同 size SHA 相同，是输入相同的记录证据，不是独立重构全部 shaderbytes 的第二份 oracle。未新增保存字节文件或另造 expected shader。

| 完整字节数 | DWORD数 | 输入SHA256 |
|---|---|---|
| 1048572 | 262143 | 8EC0F66FD9191A7845C7484C13F7BD641BA02BA43334B1AB6D094AA6A8DCC843 |
| 1048576 | 262144 | BB3348DEC3DF211CAE9920F37F0EA73678C5B2C0634BBE84E7BC5A6BE42790EA |
| 1048580 | 262145 | A5FD967BD7EE77946FD363F760943E112D557298311BA2E28EF598180B9057EA |

两架构实际 flags序列 `0,1,-1,2,0`（最后 finally 复位0）：0开启保护，上限及下限两边16像素均4294901760/FFFF0000红；上限+4两边16像素均4294967295/FFFFFFFF白。1、-1、2关闭限制，三大小均红；回0再现上限+4白而上限红。确实测了 BOOL 的其它非零 -1、2；不是穷举所有32位非零值。默认普通20字节常量shader亦16红，故至少已有普通大小功能回归可核。

每轮非法版本8字节、END后尾随24字节、截断操作数16字节均16白；invalidEvents实际记录均0、managedHResult记录0，nativeHResult明写 `not exposed by RenderTargetBitmap.Render`。这不证明底层 native HRESULT，也不证明所有非法shader兼容。

新回归是直接 await 原方法，复用原场景/比较器，不能称完全独立新oracle。独立性在 original 用真实官方模块且不经候选算法生成预期；相同场景生成器与比较流程仍共享。父比较真实SHA及全部pixels/invalidEvents，保留原 shaderBounds 全文比较，不更改 golden/阈值。

## 清理证据及其限度
两角色记录 completed/exitCode0、cleanup文本；源码可核 using stream/DrawingContext、visual.Effect=null、finally取消 InvalidPixelShaderEncountered订阅、finally set(0)、Dispatcher.InvokeShutdown及GC/WaitForPendingFinalizers。父监督证明正常退出，无强杀/输出排空超时。

本 shaderbounds 原件没有专属 WeakReference 存活数、COM最终refs或逐对象 release 事件。cleanup 是流程说明，不是弱引用收敛测量；invalidEvents=0也不是释放事件。不能将其它lifecycle场景弱引用移作此场景证据，更不能声称已认证此族全部资源泄漏或回调时序。生产释放代码未改变是源码审结论，不等于新的完整资源实测。

## 旧红核对：仅回读，不重跑
`artifacts/jv-postfix-full/inventory/Results/jj-full-inventory/{x64,x86}-failures.json` 中上述原ID的Message已核：两边字符串长度2939、index320首次差异，Expected4294967295、Actual4294901760。x64原始 `readback/full-x64/Results/shaderbounds/23da43c872d64de587069ad5c12eea69/Results/{original,candidate}/result.json:270-330` 已核 disabled0 size1048576两边红、1048580官方白/旧候选红；旧candidate1782797…，不绑定新E331526…。x86 Message原件已读，未冒称再次逐项核完其整份原始JSON。旧full各1809、失败44/19是历史运行，不能减去本轮绿推算新full。

## KC 实际验证与最终源码
KC仅纠正上述陈旧单测一行，生产/COM测试不再尾改。使用现有专用测试入口对 BoundsCheckProtectionTests 精确filter：4Passed/0Failed/0Skip、Exit0、28ms；工具默认生成 Debug/net10.0，此项仅是定向单测，不冒称正式x86/x64 Release验收。此前已过COM组没有重跑。该新增执行超出“仅必要normalbuild”的收口偏好，如实登记，不隐匿为纯文档操作。

随后唯一默认 normal Release solution build（WpfGfxShape.slnx）成功，2182 warnings/0 errors，2.64秒，未关闭分析器或改配置；这不是发布/运行/full。未新Host/工程、未终端/直接间接脚本、未窗口前置/新业务/重跑旧红。KC文件清单：本报告、next-session-handoff、session-summary、issues，以及上述单测一行。KB原生产与shader专属证据保留；COM/fill/通知/JN/JD/KA/JZ及其它测试公共入口/隔离/打包不改。

## 下一唯一相关门与交付
当前大小边界修复有限验收关闭，保留源码与既有发布SHA，不因无完整资源计数或文档历史未重读悬挂真实新绿。下一唯一相关任务为同最终Native双架构既有 shader 族的必要回归（普通编译/合法采样/通知恢复/清理），先核现有证据和精确选择，不重复本次边界组、不靠窗口前置；通过后才由上层安排集成full。旧 official sampler x86 AV不能混作候选回归成功，也不能从本次边界结论推其已消失。

剩余显示/WIC原11业务前置与stream合同、HW真实恢复、Available/mediaresources/final仍未闭。旧full44/19不相减，不能宣布完整替换。

## KC 写后回读登记
本报告完整终态及三份共享最新KC受改节写后回读；历史仅核本报告列出的旧红、规则/完整方案及共享开头相关节，未重复通读千行历史。以下初稿保存为历史，不再代表当前状态。

## 历史：KB 初稿（已由KC终态替代）
先核执行规则、完整方案、JV/JY、KA/JZ与原失败证据，再核生产边界、开关及独立官方合同。尚无KB构建、发布、运行或生产修改。
历史已知：两架构full原ID105b96c7-ae45-8225-b7ac-be58d2f59b51失败；disabled0 size1048576两边红，size1048580官方白/候选红。白色原因尚未裁决，不称fallback。旧full44/19保留，不推算新full。
范围仅shader必要生产/专属测试/报告及三共享交接；保留COM/fill/通知/JN/JD/KA/JZ修复。Available、HWloss、mediaresources、final四格开放，完整替换未完成。
