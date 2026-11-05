# EI：x86 COM 最小根因复现进度

## EJ最终安全撤回补记（最新优先，覆盖历史待办）

[EJ独立报告](replacement-supervised-creation-parameter-isolation-review.md)：完整711行实验源码先读后保存artifacts/ei-interop-repro/snapshot/InteropDeviceBitmapExports.cs（文本快照，换行归一化），生产Interop经19处精确替换撤尽EI行为/EiRelease，Code检索零匹配；默认generated、DT/DX默认关闭trace、EF DR char*与CF/CX/CY保留，无新生产行为修复。EJ默认正常分析器Release方案build一次Exit0、1482警告/0错误、30.93秒，无压制；普通增量不是发布/运行。日志artifacts/ei-interop-repro/ej-closeout-build.log，原stdout1213693字符仅尾20000可读。

EJ亲核control监督39/业务39/TRX26及creation监督39/业务6/stderr2/trace22和两actual identity。control taskb7691bc80334453a9e6226027f000bb2三轮refs0/Exit0，TRX1/1；单恢复task4236dabcf8664380b9f4937032a0b4c8 GetCreationParameters generated Hr0及GetUpdateMethod.after后Create+0x346 NRE/C0000409。完整SHA/MVID/证据路径见EJ。最小关联仅单调用或其编译影响；测试SHA/MVID不同、代码布局不能排除，不定唯一默认根因或全库bug。EG目录已找回，EH缺失推断撤销；EG原件本轮仅EI引用，非EJ亲核。

EJ新实验/发布/full均0，EI有效实验仍2；以下再跑控制建议失效。下一仅核GetCreationParameters实际IL/calli对SDK Stdcall slot9，16字节输出结构/字段/合法地址，再单点修复并正常分析器双架构DR/生命周期/BW/CK。Available0不是通知验收、x64 Event0独立，full/GPU/loss/media/resources/真实callback未闭，替换未完成。

## 阶段 EI-1：入口与证据恢复（进行中）

时间依据：本次工具调用顺序；工具未提供当前墙钟时间，故不伪造日期或时间戳。已读取最新handoff开头、执行规则全文、EH/EG全文、EF全文；完整方案正在补读。当前安全源码沿用EH撤回后的默认generated COM行为，DT/DX只读trace默认关闭，EF DR char*及CF/CX/CY保留；尚未修改任何源码或运行实验。

实际目录核验：工作区根为d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape；项目根为其下WpfGfxShape。精确目录Code/WpfGfxShape/artifacts/eg-remaining-com不存在，根.coding-agent为空。仍须精确核查实际artifacts层级及两个指定task，不能以名称搜索无命中代替目录核验。

待核：EG all-direct taskebf9a136ffe74b94b87ff1bf58c73df6、early task3eae50f5bc544823b46150e570ce9b6c的监督/stderr/业务39行/trace/TRX/actualSHA、MVID和runtime。尚无本轮新task、有效实验0；C0000409性质未知，不认定ABI失败。

## 阶段 EI-2：原件已恢复（证据裁决进行中）

在实际项目级`WpfGfxShape/artifacts/eg-remaining-com`取得control/early/late发布包和两任务回收，补正EH与本报告初始“未见”；名称工具忽略ignored产物，不能作为不存在证据。完整方案120行已补读。已全文核两份监督39行、control业务39行、early业务6行、early stderr2行及trace20行、control原child.trx26行。

历史实际UTC：control 2026-10-22T03:10:12.9480423至03:10:14.7667932，taskebf9a136ffe74b94b87ff1bf58c73df6，父6748/子3400，Exit0；early 03:11:39.0374323至03:11:44.2964955，task3eae50f5bc544823b46150e570ce9b6c，父5424/子10124，Exit C0000409。两者X86/指针4/.NET10.0.13、filtered30秒、无timeout/cancel/kill、OutputCollected=true、SupervisorError=null。

关键补正：early stderr明确`Unhandled exception. System.NullReferenceException`位于候选InteropDeviceBitmapExports.Create +0x3b2，不是监督设施异常，不可称只有未知native退出码，也不据此断定栈ABI。early四generated均返回成功，两deviceQI direct均E_NOINTERFACE/null；GetUpdateMethod.after亦存在，之后无GetDirect3D.before/首轮Create.after。control三轮Create/QI/AddRef/GC dirty/Detach/最终bitmap refs0，surface/device/factory refs0，TRX total1/passed1/failed0；Available0、实际callback/HW消费未覆盖。

control实际nativeSHA 282C1C451B32D914AB7A25032BEDA1CE782C398F9B079BFBF872EC16BD4E3EF0，testSHA 3A4D75858E83398E6FAB7CDBBC7CFC7DBF29984BE5C2D74DD6D563BFF1692F16，MVID6bce33a9-72eb-4090-b1ee-433ba0547df5；early nativeSHA71E1791704197B05B6C73E4EDA0ADE1C064176FF3952585C4C685CEDF59ABB6B，testSHA2BDA9EE2C0C305E7E5219382A2A8B5C60386B58E97F7F12E5178F290FF6CA925，MVID6f1fd03d-c75d-4655-867c-6662db505e0a。Requested=Actual均任务根Native/wpfgfx_cor3.dll。官方来源不适用于这两候选实验，不冒充官方新对照。

control原TRX绝对路径`C:\CodingAgentSandbox\Tasks\ebf9a136ffe74b94b87ff1bf58c73df6\Results\eg-control-x86\TestResults\child.trx`，这是监督子测试TRX而非父监督TRX；父没有MTP运行不能虚构parentTRX。early无完成TRX，仅部署目录，启动1/完成0，不编造TRX计数。新实验仍0。

## 阶段 EI-3：新all-direct控制已运行并立即回读

实际UTC 2026-10-22T03:44:31.3615329至03:44:32.4197659；taskb7691bc80334453a9e6226027f000bb2，父2980/子5372，Exit0，无timeout/cancel/kill、双流已收、stderr空。新task1/有效实验1；监督39行、业务39行、stdout12行已全文核。三轮12×3 Create Hr0/IUnknown同identity/AddRef2平衡1/GC dirty0/Detach/bitmap refs0及surface/device/factory refs0；callback0/consumer未覆盖。actual nativeSHA A9D1201BF9901FBFA557649B88CD99E27E3083D7EAF8906784A58AE4151B85C8，testSHA C2A12EBC97815B542C0F0D2C03AB0F134B9196D2ED49266B934D819C4E2FAF1B，MVID afc74971-3bc6-44fa-a262-1d5dc803a0bd，X86/4/.NET10.0.13，Actual=Requested任务Native模块。stdout1/1，原TRX待紧接读取。

仅Interop添加19块显式EI_INTEROP_GROUP诊断，全direct保留原stack/Ex/HRESULT/null/refs、sizeof 32/16/304；默认#else原语义不变。SDK10.0.26100.0真实d3d9.h429–450/1622–1647、d3d9types.h1495–1502/1734–1746独立确认早期slots12/3/3/9及x86结构32/16，与运行trace一致；caps304尚未逐字段SDK独立核完，不冒称全部布局闭合。NativeAOT及DR正常分析器self-contained10.0.13各发布1次成功，无规则压制；独有obj/bin/publish/package根artifacts/ei-interop-repro/control。Native日志原607333字符仅尾20000，非完整保存；工具日志不是完整原始输出认证。

下一仅Create.GetCreationParameters恢复generated，另三早期和两deviceQI保持direct；这是早期四组内最后输出结构调用的单切片，不是预判唯一根因。不运行第三次/full，不留默认生产修复。

## 阶段 EI-4：单调用恢复失败，停止新实验并安全撤回

实际UTC2026-10-22T03:46:02.0198959至03:46:06.97284；task4236dabcf8664380b9f4937032a0b4c8，父10296/子3032，Exit-1073740791/C0000409，无timeout/cancel/kill、OutputCollected=true、SupervisorError=null。监督39/业务6/stderr2/stdout2/trace22行已立即全文回读。stderr是候选Create+0x346未处理NullReferenceException，非监督异常。唯一恢复Create.GetCreationParameters generated仍返回Hr0；GetUpdateMethod.after亦返回，随后首次Create未完成。其余早期三项与两deviceQI direct。

actual nativeSHA0E48BA986291577C2AA0728A6094DB8A7F2A9AFB7BE4B179D6F148D2F031EEA5；testSHADBA22222AFEA84D930C69EA1A20186E8BD9AA844160F774C2E314BB8B8193123；MVIDd2b49352-d54e-4b28-a33d-7659a5b883bb。X86/4/.NET10.0.13，Actual=Requested任务Native模块。共新task2/有效实验2（上限已到），全direct1通过/单恢复1启动0完成，NativeAOT2及DR2发布成功。没有x64/官方新运行/full，不增加第三实验。

新control原TRX26行已全文读，total1/passed1/failed0；绝对路径`C:\CodingAgentSandbox\Tasks\b7691bc80334453a9e6226027f000bb2\Results\ei-control-x86\TestResults\child.trx`，不是父TRX。creation仅stdout启动无完成总结，不编造0失败TRX。

因果强度：在本轮同源诊断框架中，仅恢复Create.GetCreationParameters generated足以由成功变为同类失败；因此可将四调用组收窄到此单调用/其编译代码影响，不证明它是全部默认故障的唯一原因，不证明首次写坏地址或整个Silk库问题。两包测试SHA/MVID因独立输出编译不同，但DR源未改；不是二进制完全相同的测试程序集。下一轮优先认证该方法真实generated签名/IL、x86调用约定与结构输出值/边界，再选择最小修复并经双架构DR/生命周期/BW/CK门；本轮不保留默认修复。

后续仅在恢复证据或新有效对照基础上分组。若旧原件缺失，以当前源码显式EI_INTEROP_GROUP建立all-direct+原stack控制，再最多一次有据generated组恢复；不增加Host、脚本、原生探针，不改DR/DS或监督入口，不跳Ex/GetUpdateMethod，不关闭分析器。结束撤尽新增行为并正常构建，最终同步四文档。
