# async-shared-ownership-x86-r1 最终收尾：有限合同通过

## 裁决与范围

既有official原件完整回读通过后，亲自执行剩余candidate一次，双方准确单体各TRX total/executed/passed=1/1/1、监督ExitCode0、实际模块身份及真实清理通过。本门完成并停止执行；整体基线不升级，完整替换Open。仅证明x86健康异步同connection/partition跨channel Bitmap95真实WIC所有权与独立Visual39有限更新/关闭合同。

本接续源码/项目修改0，build0/COM publish0/Native0，新增Windows沙箱candidate1；未委派、未用终端脚本、未发现、未重复official。全门累计normal COM build1（Exit0，133warnings/0errors）、COM publish1（Exit0，显式15A545 Native）、official1/candidate1共2沙箱，Native发布0。原partial保留，未改expected/截止等待/阈值，未补跑。

## 可复核原件与工具账

相对本报告所在仓库子根WpfGfxShape，证据根为 `artifacts/async-shared-ownership-x86-r1/`：
- `pre-execution.md`：引用账、拥有链、固定输入、停止条件及预算。
- `01-build-return.md`、`01-build-readback.md`、`02-publish-return.md`、`03-preflight.md`：既有参数、直接返回与压缩日志回读边界。build stdout104998仅末20000、publish stdout52788仅末20000；不是完整stdout认证。既有专用工具直接返回无数值Exit，数值来自当时日志回读记录，不伪造全文。
- `04-official-readback.md`：既有official逐段亲读、精确task/TRX/UID及放行理由。既有official工具直接原返回未在本接续上下文或已定位文本找到，保留这一窄缺口；不因此重复official，不由完成文字造工具Exit。
- `05-candidate-tool-return.md`：本次调用全部真实参数及完整直接原返回，立即归档。其“沙箱执行完成”无数值Exit；下表Exit均来自监督原件。
- `readback/{official,candidate}/Results/aot-identity.json`：初始化身份，只作初始化证据。
- `readback/official/Results/async-shared-official-x86/` 与 `readback/candidate/Results/async-shared-candidate-x86/`：各 `supervision.json`、`TestResults/child.trx`、`stdout.txt`、`stderr.txt`、`AsyncSharedOwnership/<业务UID>/business.json`。TestResults目录枚举漏列，但两份child.trx均直接读取成功，不判不存在。两个stdout各12行中文编码乱码原样保留，stderr均空。

两角色均亲读监督1-40、TRX1-26、identity1、stdout1-12、空stderr和business完整1-576（分1-140/141-250/251-360/361-470/471-576）。未冒diagnostic全文审阅。

|标识|official（既有回读）|candidate（本次唯一执行）|
|---|---|---|
|task|ec25ea907a6f42a589251d0bf2692c81|62e3b148e59d41f691a7e0e21333f329|
|业务UID|47ce6825e9c0433c8426c52b8c16f431|25655978921b43788dd24653cda1250d|
|TRX runId|7fc0eae7-464d-4f83-ba35-90f80e3b295c|0ebbfa1d-29a9-4cdd-a466-a4bed7162663|
|TRX executionId|9f30412b-8b10-4ce8-bfd7-09873599a514|9dbffb44-03fe-4c02-8851-5f6434728e27|
|监督child PID|10160|11496|
|actual moduleHandle=BaseAddress|1854210048|1851981824|
|actual角色路径（各task内）|根wpfgfx_cor3.dll|Native/wpfgfx_cor3.dll|
|结果|TRX1/1/1，监督Exit0|TRX1/1/1，监督Exit0|

准确FQN分别为 `WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalAsyncSharedBitmapOutlivesSourceThenFinalCloseReleasesOwnership` 与 `WpfGfxShape.ComAcceptance.UceChannelTests.WhenAsyncSharedBitmapOutlivesSourceThenFinalCloseReleasesOwnership`。TRX定义/实际过滤均对应，非仅方法存在或发现。

两角色同testSHA `D8A5F57FE4A61CC28459A89DCB50DC75E0ADE80E1D0AEDC35D7E18AC5069A7BB`、MVID `e207b681-4c15-453e-b0a4-a1690fa3db78`、.NET10.0.13/X86/pointer4。official实际SHA `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`，version `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`；candidate实际SHA `15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`，version1.0.0.0。各business RequestedEqualsActual=true且句柄等于实际模块BaseAddress；不是用初始化15A545代官方actual。

candidate从既有同clean启动新独立进程，不是publish根；参数为--supervise-exit、Results/async-shared-candidate-x86、120、上述FullyQualifiedName等值，外180秒。两监督Reason=exited，TimedOut/Cancelled/KillAttempted=false，HasExited/OutputCollected=true，输出drain无超时，SupervisorError=null。

## 逐层合同与真实所有权

已有partial静态核对：connect(0)，同handles[0]创建A(source0)、B(sourceA)；两资源独立duplicate，Visual不引用Bitmap。执行前引用账限定真实BMP/system WIC/NoCache/wrapper，不把NoCache扩为所有WIC任意时刻保证。

|实际事件|official|candidate|
|---|---|---|
|创建A/B，共享partition|174332000/174331104|2/3|
|Bitmap95/Visual39|各resource1/2|各resource1/2|
|Transfer及两Duplicate，A close/commit/flush（order10-15）|全部HR0，先于目标使用|全部HR0，先于目标使用|
|caller wrapper/frame/stream Release（17/19/21）|真实返回各1|真实返回各1|
|A关闭前/后dispose，A.Destroy|0/0，HR0|0/0，HR0|
|B关闭A后的新Visual alpha0.25，close/commit/flush|全部HR0、dispose0|全部HR0、dispose0|
|B Visual/Bitmap最后Release|各HR0/deleted1|各HR0/deleted1|
|B release close/commit/flush|全部HR0|全部HR0|
|真实callback enter/exit（33/34）|线程11912，恰1次，dispose1|线程11292，恰1次，dispose1|
|B.Destroy/Disconnect/Deinitialize|全部HR0|全部HR0|

两callback均在释放批次commit之后、B.Release.Flush返回之前完成，早于B.Destroy；不强行要求Destroy后才触发。最终原有5秒截止观察finished=true，未新增等待。A关闭的资源卸载不能替代B显式Release/deleted实测；caller返回1也不冒全部COM引用归零。callback是实际stream最终释放回调，非模拟计数、Available或device-loss通知。源关闭后新alpha仅验证Visual更新，不证明Bitmap消费或像素。

双方completed/cleaned=true；created与destroyed完整对应、released=[2,1]，disconnected/deinitialized=true；outcome=(0,0,1,1)，callbackEntries1，callbackErrors/cleanupErrors空，primary=null；WIC scope正常Dispose。结果副本提前dispose/缺失dispose两负控均拒绝，只是validator自检，不是生产故障注入。

## 覆盖边界、历史账和唯一下一建议

本次仅增加15A545 x86健康异步共享资源有限所有权/有限关闭合同；x64该合同仍未验证，不转绑54BC62。既有两架构postfailure/isolation有限通过及来源限定闭合保持。旧20B3失败、40D0隔离、旧测试SHA及KN17/direct/public/WGC/KS/Ledger和其他历史账独立保留，不移绑、不冲销。

Register真实失败/异常回滚/OOM/QI或构造中途异常仍未证；composition callback重入等价、长期资源趋势、跨connection共享、consumer像素、HW、resize、real-loss、media/full及完整替换均未由本门证明。显示冻结，不扩错误/共享矩阵，不造新探针。

唯一下一建议：另授权评估并执行x64同一有限合同官方/候选对照；先核现有x64测试入口/clean和实际54BC62来源，若缺测试包另定COM build/publish各≤1、生产不动Native0，official≤1全部合同身份清理过才candidate≤1，120/180、首错停。此处仅建议，当前不执行，不重跑x86或自动发布。完整覆盖状态见最终矩阵顶部及当前恢复入口。
