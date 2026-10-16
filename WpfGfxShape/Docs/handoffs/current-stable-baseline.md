# 唯一当前基线：C5当前源码已测、风险与正式门分离

> 当前合同覆盖：[DEC-0005](../decisions/DEC-0005-x86-x64-acceptance-scope.md)正式仅x86/x64全部矩阵；ARM64编译/布局/运行及工具链/实机为NotApplicable-UserScopeDecision，非Passed，不计阻塞。下文ARM64 MissingExecution/MSB8020及原NA=0统计保留历史原义。10.0.12为历史路径绑定，未证用户指定，精确patch口径待澄清，见[版本审计](../windowsdesktop-version-provenance.md)，不再作为永久必需用户包。Ledger原值未改；C5/两required及parent仍因x86/x64完整条件未满Blocked，stage4Closed=false，总门Red。本次未执行任何构建发布测试。

## 最新身份及边界，覆盖以下旧轮

最终audit程序集SHA `F2590E8CFABD36E4DCE7844CC9467379D2460F430A984D98B4A8AF4D285382FD`，runUtc `2026-10-16T05:25:13.4747388+00:00`，X64；Release build 0 warning/0 error、自包含audit发布与沙箱运行成功。生成报告完整重新解析并逐行比较17,377记录通过；summary完整回读，输入身份216项。无源码未测尾改。

正式9入口不变；333风险=13适用build声明已证+92 build未知+227 include未知+1缺项目，全部333源行定位，NA=0；319未知不冒称外部或Excluded。原readiness/doneRule不要求保守333穷尽，撤销旧额外门但保留原表及正式Blocked。每个required必要证据/充分性/gap只见[唯一详档](../completed-work/GenProto-C5-final-adjudication.md)，机器表不建立第二份。

C5/两个required/父级Blocked、stage4Closed=false。下一仅请求bilinearspan原项目/依赖或权威生成合同及既有外部条件，不继续扫描；无已证必需生产修改。下列生产SHA、历史COM/strict及46/46身份保持，均未重跑；DLL驻留。

## 以下旧轮数字与穷尽任务仅作历史

> 当前覆盖本页旧“无机器总裁决”描述：[唯一裁决](../completed-work/GenProto-C5-final-adjudication.md)。正式6+3=9；17,377是审计记录非正式边分母。105 build条件、227精确include适用性未证、1 bilinearspan原项目缺失，逐行见JSON Ordinal17045–17377；332不是已证必需生产修改，也不是全部外部。本轮独立audit最后build退出0/0警告/错误并沙箱运行完成，summary已完整回读；不是新增testfull，既有46/46与生产/COM/strict身份不变。最小精确适用边全集未证，自主全审计未完成；C5/两个required/父级Blocked、stage4Closed=false。

[C5 activeincludes最终清单](../completed-work/GenProto-C5-activeincludes.md)、[此前evaluation详档](../completed-work/GenProto-C5-selected-build-evaluation.md)、[原条件逐assertion裁决](../completed-work/GenProto-required-final-reconciliation.md)、[当前Ledger](../../Ledger/mediaown-required-finite-acceptance.json)。正式分母固定6 physical + 3 evidence；C5整体Blocked，不扩边分母。

- 生产保持 `3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D`。历史COM总1616/通过1583/失败33/跳过0，strict Missing2/player Extra0，非本轮新跑。无生产修改/publish/COMfull，DLL驻留，unload=N/A。
- **此前源码回归（不是本次CL运行，本次未重跑）**：源码build成功；专项 `12_22_34` 原TRX 2/2，RunID `58846cbb-e699-420c-9824-099094aaf746`；full `12_22_49` 46/46/0 fail/skip，RunID `5cc712a5-0337-4877-ae59-e1a3b47623aa`，TRX SHA `D05E3204B35C2F55BED7BC65153D89E482FCA4BEFA7F8C7449E18FB396BAEB2E`。原路径均为 `Tests/WpfGfxShape.GenProto.Tests/TestResults/lindexi_DESKTOP-51A5UGG_2026-10-16_<时间>_net10.0.trx`。
- 最终归档专项 `12_23_19` 1/1，RunID `99939695-c22a-44df-b064-1c3f9baed822`；目录 `Investigations/GenProtoEvidence/wirecompiler-delivery-ebf1bb9eb58647d2a68f5b243ce5d768/`，report SHA `6770D6733298D2A40B723A379EBD83B85A6AE80C0E951F296C111549CE4BD954`。完整机器读取C5 115221 leaves/summary11、wire12078/75并核对来源/assembly/full快照。其targeted字段为旧wire-only，不冒称最新专项。
- 当前test assembly SHA `C11E39974E4EAC04FF714D8CB3AFC99D36F28AA432A4540C86514C099F5A2E86`，MVID `ac2ca61c-15fa-4633-baf8-b05015fda449`。SelectedEvaluationTests source SHA `A30D6F90229BBF44F43C0D1E6EB0B80B1D80874C0E628DB83AAF86B1B1E75173`；ArchiveTests source SHA `EB19E3FBBB9E0B45B2754929EDA595BE936B2D2812AE7484A0A65CCE1B4CCB13`。
- C5 report SHA `129609D5E8953FEB424DEF7CDE3E07A0E15BA5F07AFE7EF05AF5B09DFEEA2E71`：18 native+2 generator；12 Win32/x64选定v143/14.44.35207/SDK26100路径有效，6 ARM64缺toolchain已分类。888 direct candidates/150 distinct不证明active closure；原721包络及333（332 unknown+1 RealMissing）对账实际写入并读回，不是新required。
- 首轮full `12_17_52` 45pass/1fail因归档误选delivery目录而缺summary；按测量/归档真实分类修正并完整重验，原失败TRX保留，未skip或放宽。原333未写report已补；最终源码已测，之后仅Docs/Ledger。
- 既有Ownership6/6、再生378输出/275 byte-exact/378所列范围等价保持有限裁决；原Resources.rsp无ProtocolFingerprint，历史binding未证。consumer/factory/x86/enum/padding结构有限Satisfied不降级。两transport packet/独立discard未证但不自动新增原required门，API功能不批准完整等价。

- 原项目真实selected CL已完成：resources/uce/wpfgfx Debug/Release×Win32/x64共12配置16必要TU，v143/14.44.35207/SDK26100，CL均exit0；原物在工作区根 `artifacts/log/GenProto-C5`。原入口最终exit1是wpfgfx四配置bilinearspan引用GetTargetPath的MSB3202，不是header失败，不是完整生产build。本次只读现有原物并同步文档，没有新CL、测试或机器总裁决报告。
- 本次16份.i实际检索及固定6 physical+3 evidence有界映射已写独立清单：core/render全部到达；resources/uce的factory/resources/data与factory正文绑定既有P/R/F签名、生成及断言。不是全部333原边逐条裁决，不以888候选或全WPF图扩门。

仍可自主执行的正式条件：C5所选project/import/属性/include适用边→既有record/assertion的剩余逐项审计，未到达不Excluded，未知不一律external。真正资料边界是确缺bilinearspan原项目/依赖或权威生成合同，以及D4历史binding；前者阻断原C5完整物理引用记录，后者不能用当前再生替代。补件统一见外部页。不再重复采集同一16 TU或全量重跑，不声称自主余项零。

历史authority、ARM64、full10.0.12、官方player协议及DWM/HW可见control前置仍缺。C5未足、两个required及父级Blocked，stage4Closed=false，用户全目标持续、总门Red。
