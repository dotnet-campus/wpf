# FH：成功/失败精确差异审计与崩溃证据

## FI 最终回读补记
既有FH报告/handoff/session-summary/issues四文档正文与历史已全文小段回读至末，超长截断末行及余段均缩小补齐；补记前分别25/638/664/681行。本补记随后再次回读，历史未删、无重复报告。FI默认编译隔离与有限交接完成，旧进行中由此终态覆盖；并非运行安全验收或生产修复。默认normal analyzer Release build1次Exit0/98警告0错误，显式诊断build0；实际执行0、取证0、新沙箱请求0。原请求/连接错误独立原件仍未取得，上传阶段未知及调试清理静态风险保留，不造hash/实际候选身份。连通后先正常退出控制1，再合法DR默认候选Create崩溃取证1，目标异常地址/实际模块偏移；Available/HW恢复/full/x64Event0等全部未闭。

## FI 安全隔离终态（最新优先）
FI 仅修改 Tests/WpfGfxShape.ComAcceptance/FhCrashEvidence.cs 和 NativeRun.cs：整份诊断源码从首行到末行，以及 --fh-normal-exit/--fh-crash-evidence 两分派共同受 FH_CRASH_EVIDENCE 条件保护。默认未定义该符号，诊断类型、系统导入和专属入口不进入编译结果，不成为默认发现/触发路径。源码原实现保留为 snapshot；未修改其他分派、MTP、既有监督 filtered/full、生产 COM、DR/DS 断言、VisualCache/historical/捕获、项目打包或 CF/CX/CY。不新增脚本、Host 或工程。

### 编译与执行边界
FI 默认正常分析器 Release build 恰好 1 次：目标为 ComAcceptance.csproj，Exit0、98 警告、0 错误、2.74 秒；不是 clean/AOT 或双架构完整构建。未添加 NoWarn、未关闭规则。可读日志64行已至末；stdout 原77103字符仅保留末20000，前57103字符不可见，不声称全审警告。显式诊断 build 0 次：本次仅预处理隔离，不重编/发布既有未验收诊断包。FI 沙箱请求0、实际执行0、崩溃取证0、full0，编译不等执行或安全认证。

### FH 请求及原件的独立检查
亲读 artifacts/fh-crash-evidence/Build.props 全17行及 source-sha256.txt 全4行。旧 props 定义 DR_INTEROP_ABI、独有输出目录及 GetFileHash/WriteLinesToFile；没有 FH_CRASH_EVIDENCE，本次未改它，也未重新执行其身份目标。旧source记录不是本次改后源码hash。fe-production-experiment.cs.txt 可直接读取；GetFileHash阶段是否能访问同一路径与读取能力不同，不能据前者不可见判原件丢失，不能声称本次hash完成。
在 FH 目录、artifacts 及工作区检索 request/error/50250/172.20.115.32，没有取得独立保存的控制请求JSON、原始连接错误或对应工具记录；.coding-agent目录为空。此项独立核证受限，不能把用户提供的事实冒称原件亲核。按本次明确交接：FH DebugAPI 已编译未运行，控制提交连接172.20.115.32:50250超时且无taskID。可证上限是提交/连接尝试，不能断言远端上传完成、任务创建、程序启动或测试失败；上传的具体阶段及服务端接收情况未知。无taskID不补造远端路径。未新请求沙箱或恢复网络。
当前诊断候选没有实际进程SHA，不能绑定旧默认shared publish、旧aot.sha256或source hash作为运行身份；本次未计算候选hash。

### 调试循环静态审查（不等安全验收）
FhCrashEvidence 全文已读（含先前截断的第80行补读）。只允许Windows x86；x86 DEBUG_EVENT union从12偏移、异常地址24/firstChance92、96字节缓冲及模块偏移计算与该布局一致，不能外推x64。首次first-chance breakpoint用DBG_CONTINUE，其余异常用DBG_EXCEPTION_NOT_HANDLED；EXIT_PROCESS事件继续后记录完成。120秒TerminateProcess，130秒抛超时；模块文件句柄在finally释放，create事件进程/线程及新线程句柄与PROCESS_INFORMATION句柄均有关闭代码。
明显未验收风险保留：首个任意breakpoint被当loader断点，未独立核来源；ContinueDebugEvent失败只记录并尝试kill，未检查kill结果，也未重新抛错；最终kill及CloseHandle返回值未核，未等待确认退出；模块记录异常时create事件进程/线程句柄关闭不在其自身finally，存在异常路径泄漏可能；CreateProcess成功后环境恢复若抛异常，后面的清理finally尚未进入；记录/IO异常会中断调试循环。不能仅凭这些代码声称异常捕获、退出、超时回收或句柄安全通过。此次不扩大诊断逻辑变更，靠显式默认隔离保证未验收角色不进入默认路径；恢复运行前须解决/核证上述异常路径。

### 源码事实与根因猜测分层
FE snapshot GetUpdateMethod为direct slot7及QueryDevice/ReleaseDeviceEx helper，默认对应GetDeviceCaps/QueryInterface/Release generated；FE与EV/EX成功控制的trace/helper/内联和条件编译布局并非同一配置，历史成功不认证FE组合。已亲读FE snapshot250–310及默认250–305；其他EV/EX完整归档配置本轮未重新取得，不能声称完整同包差异审计已闭。后置slot6/QI0/Release2曾成功的历史不证明所有前序/第二slot9/LUID/Ex释放/BitmapState分配/Register/cleanup都已一致或执行。这些配置差异是事实/待核项，不是已证根因；无本次故障地址/栈，不猜扩大COM替换。

### 唯一下一步与开放门
沙箱连通后，先审清静态风险并显式启用同一诊断条件，以正常退出控制1次核调试循环/退出/句柄及证据完整性；控制通过才进行合法DR默认候选Create崩溃取证1次。目标是异常地址、实际模块与偏移及必要身份，不预设COM根因、不扩大替换，不跑full。实际候选SHA必须从该次真实加载绑定。Available/native通知与consumer、HW内容/resize/device-loss恢复、media/resources及线程callback、full最终四格与独立x64Event0/E_HANDLE全部未闭。安全隔离完成不等生产修复或完整替换；四文档历史全文回读进行中，最终补记后才记闭环。

## FH 进行中
已接管 FG 终态；仅直接使用专用工具，不委派、不使用终端或脚本。先核规则、完整方案、EV/EX成功与FE失败原件和源码快照，再判断最小缺陷或既有监督角色纯C#原生取证的安全可行性。尚无新构建、运行或生产修改，不把历史记录作为本轮实测。最多正常控制1次、合法DR崩溃1次；未启动。保留所有开放门及历史。
