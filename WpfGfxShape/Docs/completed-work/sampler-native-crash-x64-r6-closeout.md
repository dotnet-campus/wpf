# sampler-native-crash-x64-r6：离线执行失败收尾，目标未完成

状态：ExecutionFailed / CallerUnknown / RootCauseOpen。完成本轮记录收尾，不是可信caller定位完成，不是确切外部API能力障碍，不是fixture修复或测试通过。

## 范围、执行与原件
实现前[范围预算快照及执行终账](../../artifacts/sampler-native-crash-x64-r6/execution-receipt.md)已写。授权normal COM build≤2/publish≤2/sandbox≤3（offline1、确证修复后official1、必要candidate1）。实际normal build1（绝对COM目标，SamplerNativeCrashR2/win-x64/net10.0-windows，exit0，148warnings/0errors，4.52s）、publish1（exit0，包含隐式编译）、offline sandbox1（外300秒）；Native/full/official/candidate/重采崩溃/下载PDB/脚本/再委派0。剩余build1/publish1未消耗，未挪业务沙箱角色再次offline。

新独立r6/clean和publish由项目Content/Copy合法binarycopy原dump/context/PDB、AD1C官方image；Native候选aot.sha256亲读632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671，生产DLL未重发。诊断入口默认关闭，只有SamplerNativeCrashR2定义SAMPLER_OFFLINE_SYMBOLS，正式fixture/SDK/TFM/rootWait未改。

sandbox task fe2e18361b604289b703df51f3fe4e83，PID10708，test MVID8468436b-38c1-40a1-9612-8f3071fe641b，.NET10.0.13 X64。工具原return仅“沙箱执行完成”及保存路径，父OS exit Unknown，不等于exit0。无新TRX/业务监督/stages，离线角色不运行原测试。

原件根：`artifacts/sampler-native-crash-x64-r6/readback/Results/sampler-offline-symbols/`；unwind.txt37行、symbols.txt106行、official-source-status.txt2行全部亲读。符号身份、全部寄存器、dump hash/5077ranges、清理结果读完；SymCleanup=True。API403仍存在，不代表SDK能力障碍；全部历史403/404、失败/违规/日志截断保留。build日志工具截去前95925字符，不虚称完整原始日志。

## 实际API结果及失败原因
文档StackWalk64允许提供ReadMemoryRoutine后使用唯一稳定process/thread token，符号API同token。本实现token0x632F0066，非liveprocess；独立AMD64 CONTEXT1232、flags0x10001F、16字节对齐实值0，STACKFRAME64 size264，原PC7FFC17C6B65F/SP78A963EF00/FP0。native回调异常捕获并记入callbackFailure，不跨边界抛；本次无callback异常。

AD1C SHA256及PDB732FB582…/GUID8756f755-10f4-48be-9886-22c9a80c75b8 age1再次认证；模块Size1978368，SymType3/Machine8664。SymFunctionTableAccess64非零、LastError0，B650–B6FA/UnwindRva18924C与image实际.pdata记录一致；SymGetModuleBase64返回原base。

关键失败：ReadMemory仅dump内范围。DbgHelp请求VA7FFC17C60000的64字节image header以及VA7FFC17DE924C的4字节UNWIND_INFO，原dump均未捕获；因此Blocked=True。StackWalk64 step0虽True/LastError0且返回地址B72F，但缺unwind bytes，明确不认证该返回地址，更无UnwoundFrame。不将symbols.txt的rawstack候选混成真实调用链。

**代理实现偏差**：只验证了image .pdata，却未让DbgHelp读到匹配image .xdata。匹配DLL其实本地已有，故这是可修的image-backed读取缺口，不是“缺image”、不是DbgHelp API不可用，也不是外部确切能力障碍。未满足用户本轮推进到可信caller或确切API障碍的目标。offline1已用完，不擅自复用official/candidate角色。另需一次offline角色授权才能修正后验证；当前不请求重采或下载，也不选新静态门。

## 根因及fixture决策
已证最窄事实仍是GetScaleDimensions+F的F30F1001读取原RCX0（空矩阵this）。未取得可信直接caller，不能判定是否SetupEffectTransform传null worldtransform或其他路径。r5本地官方参考关于MatrixStack空引用/SetupEffectTransform必需矩阵的假说仍未确证，未新增源码认证；不能凭raw B72F/FFC53改fixture。

before/after：正式fixture完全相同，expected/skip/fallback/阈值均不变，无业务修正或回撤；仅诊断增加partial unwind文件、offline token/回调，诊断配置切独立r6输出。要撤诊断只移除Unwind partial调用/文件并恢复r5配置路径；无需生产回撤。未确证fixture错误所以不消耗修复后build/publish/official/candidate预算。

## 基线及恢复
原full x64 1823/1803/20fail+x86 1823/1820/3fail，23红不减；memoryGreen=null，显示/HW/target/loss/音频/自然资源/全部worker退出/final full与完整替换Open，rootWait原诊断未新验证。

恢复需明确区分dump-only数据与已匹配image-backed元数据：在已认证image范围内按PE节RVA及raw边界提供只读header/.pdata/.xdata，不以当前进程内存代原进程；读缺口不伪成功、所有来源记录。修正后需要额外offline1授权（不包含在本轮剩余业务角色），优先原context到直接caller，取得可信链后才决定fixture；不是已实施或已验证。本轮文档收尾完成，但用户定位/修复验证目标未完成。
