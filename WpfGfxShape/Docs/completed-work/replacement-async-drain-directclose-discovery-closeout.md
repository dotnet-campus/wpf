# AsyncDrain directClose 6/8：发现收尾与精确执行能力静态审查

## 终态及独立账

发现文档收尾完成，无收尾待办。本阶段亲读已有证据、源码及微软CLI文档，仅写文档；源码/项目/新增测试/build/publish/Native发布/程序运行/沙箱/终端脚本/再委派新增全部0。既有发现账单列：x86发现1、x64发现1、测试体0；不并入任何ownership、postfailure、旧AsyncDrain或其他历史账，不生成新的通过结论。

发现门关闭；精确体仍NotRun。完整替换Open、整体不升、显示冻结，KN17及全部既有独立账不迁移。未来即使通过，也仅支持所选健康关闭、真实释放回调、candidate重入及该释放worker退出；official异分支不提供重入/worker等价，不外推像素、长期趋势、HW/loss/media/full。

## 原件及复核范围

以下路径均相对工作区，证据根 `WpfGfxShape/artifacts/async-drain-directclose-discovery/`：

- x86 task `189899d1153b4b2fafeb97f3c8cb5baa`：`readback/x86/Results/async-drain-directclose-discovery/Diagnostic/log_261027130857726.diag`。
- x64 task `bdb8b2cc0ee449b4a45925b7d3945c65`：`readback/x64/Results/async-drain-directclose-discovery/Diagnostic/log_261027131030346.diag`。
- 工具原返回：`x86-tool-return.txt:1–25`、`x64-tool-return.txt:1–25`，本次分别亲读全文。各列candidate12行、official6行，共18个发现项；部分中文编码损坏，保留原件，不修写为数值exit。

用户交接说明两份diag各1–115已前读；这不是本次重新连续全文读取的声明。本次x86直接可见复核1–28、28–42、64–67、72–75、100–110、112–115，并将102、110单行另读；x64直接可见复核1–28、28、64–67、72–75、100–110、112–115，并将102、110单行另读。请求1–50及100–115有工具字符截断，未把未返回部分冒为本次全文亲读。另分别对两个Diagnostic目录检索Passed/Failed/InProgress/SkippedTestNodeStateProperty，无命中；不把包内DLL字符串命中混入diag结论。

两架构diag第1行MTP2.0.1，第6行分别win-x86/win-x64、.NET10.0.13，第9行各自task下测试DLL，第10行均明确 `--list-tests --filter` 两方法FQN的OR表达式、同步diag，没有测试体执行请求；第28行均MSTest4.0.1。第66/74/102/110行分别是C6/C8/O6/O8，含正确方法、System.Int32参数、显示行值及DiscoveredTestNodeStateProperty。八项逐架构复核见[独立参数清单](replacement-async-drain-directclose-uid-manifest.md)，没有用一架构替另一架构证明。

末会话：x86第115行session `36bfcc2b-53ee-426f-8fc9-3bbba7fe2615` ended with exit code '0'；x64第115行session `494f7ea0-232a-4223-96fe-428dbea1d493`同为'0'。这是MTP会话exit，不是外层工具/OS数值exit。两个txt没有独立数值ExitCode；不得写外层Exit0或监督Exit0。第113行TestRequestExecutionTimeInfo名称不意味着测试体执行；结合list-only、发现状态与无执行状态，只认证发现成功。

## 源码与CLI静态能力

本仓源码相对 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/`：

- `ExitDiagnostics.cs:24–44`：Parse只收root/seconds/filter三个参数；ChildArguments把完整第三参作为一个 `--filter` 值，固定附TRX与diag参数。不能将 `--filter-uid` 嵌入第三参变成参数透传；普通filtered预算上限120秒。
- `ExitDiagnostics.cs:92–118`：监督器确实WaitForExit并记录Process.ExitCode/ExitHex；但目前没有该模式的UID透传。`NativeRun.cs:23–24,38–43`普通正式exe入口可原样传CLI到MTP并返回app.RunAsync值；无需新Host/脚本。RoleAsync虽为params参数方法，但不是通用公开CLI转发入口，不虚构可用分支。
- CLI文档亲读： https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-running-tests 支持正式exe `--filter`； https://learn.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests?pivots=mstest 给出 `<Property><Operator><Value>`，`= != ~ !~`、`& |`、括号；MSTest属性列有FullyQualifiedName、Name、ClassName、Priority、TestCategory、Id。不能把xUnit的DisplayName属性表套给MSTest。TestCase是对象概念，不是由该表认证的filter属性；queueState也不是。
- 同一文档说明MSTest adapter >=3.6的 `Id=<TestCase.Id GUID>` 可选独立展开行。这是合法能力，不能声称所有属性过滤必然只能选方法；但当前diag提供的是MTP TestNodeUid，未提供VSTest TestCase.Id映射。禁止直接假定两种GUID相等。Name合法也不等于本包的Name值已证包含DataRow显示后缀；DataRow.DisplayName是显示配置，不自动认证为独立过滤属性。本测试DataRow没有显式DisplayName。当前证据不足以认证 `Name=方法 (6)` 或 `DisplayName=...` 经现监督器恰选一行。
- 本次尝试读取上游v4.0.1 adapter过滤实现的URL均404，未取得版本源码，不拿最新文档表或搜索RFC冒充4.0.1具体Name/Id映射实现。现有仓源码可认证监督参数处理，CLI可认证合法语法；具体版本行属性精确匹配能力保留未证，不额外执行验证。因已有真实MTP UID，无需为该缺口停工、扩展runner或无限调查。
- https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-cli-options ：`--filter-uid`自1.8可用，2.0.1可直接使用；`--list-tests json`从2.3才有，本包不用。此次发现没有认证UID执行已成功，只得到真实节点身份。
- https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-extensions-test-reports ：TRX依赖扩展，参数 `--report-trx --report-trx-filename`；不用2.3新增文件名占位符或崩溃流式TRX承诺。两clean的`package-files.txt:1–14`均含TrxReport/Abstractions、MTP/MSTest，文件实际存在；NativeRun调用AddSelfRegisteredExtensions。既有同clean candidate `readback/candidate/Results/async-shared-candidate-x86/stdout.txt:1–12`及x64对应文件均报告生成TestResults/child.trx。扩展安装及已有路径可用有静态/历史依据，不需新help运行；这些旧1/1结果不算本回归。

## actual模块身份：现有能力足够有限绑定，不靠file预检冒加载

`UceChannelTests.AsyncDrain.cs:79–95`：official按MediaPlayer程序集目录请求wpfgfx_cor3.dll，candidate按AcceptancePaths.AotDll；NativeLibrary.Load返回module，所有Export从该handle取得。

第118–119行用当前进程Modules.Single(item.BaseAddress == module)取得loaded.FileName；第244–245行最终业务JSON写path、actualPath、读取actualPath文件得到的SHA256、模块文件版本、testSha256/MVID、runtime/architecture、pid及业务事件。这是**按实际加载句柄关联模块路径，再对该路径文件取hash**，不是仅AotFileIdentity。没有调用GetModuleFileName，没有把module/BaseAddress数值写入JSON，也不是内存映像hash/抗文件替换证明；这些边界必须披露，不能借ownership日志更丰富字段填补本合同。

`AcceptancePaths.cs:51–69,74–89`的aot-identity.json只验证候选磁盘PE/架构/hash、pointerSize及参数，official行也不能由它证明实际加载官方。`--diagnostic`是MTP日志，不承诺native模块句柄/hash；WPFGFX_RUN_ROOT不改变AcceptancePaths.Results；WPFGFX_EXIT_STAGES只在现有Stage调用处输出，不能创造缺失模块记录。没有静态证据证明可通过某个现有正式env诊断补出GetModuleFileName/handle输出；本工具也无env字段，故不推荐猜env或改共享代码。当前按handle关联路径的现有业务输出已可做有限actual身份核，不额外要求独立加载记录器。

## 唯一下一最小执行建议（另授权；本阶段不执行）

只先授权 **x86 C6单体canary一次**，使用现有clean及正式exe，直接 `--filter-uid`，不经supervise-exit、不猜Name/Id、不加Host/脚本。精确工具字段/参数与身份期望见[参数清单](replacement-async-drain-directclose-uid-manifest.md)。本轮建议预算：测试体1、沙箱1、build/publish/Native/源码/新增测试0；120秒外层超时，MTP全局60秒；首错/超时/零测/多测/身份不符/证据缺失即停，不重试、不补跑发现。

回收并亲读：原工具txt、完整diag、TRX、Results根的aot-identity.json及async-wic-drain-False-6-False.json。TRX总计/执行/通过必须1/1/1且准确参数行；diag必须是所选UID成功terminal及会话exit0，非仅Discovered或摘要；业务必须system=false/queueState=6/retainExternal=false、completed=true、全部正常calls成功、disposals=1、非caller释放、callback.enter/exit及callbackExited=1、Callback.DestroySibling=0和Deinitialize=0。candidate源码第232–233行对该释放线程退出有断言；结合Passed与最终事件只能认证该worker有限退出。finally兜底忽略HR不能当正常清理；不能仅completed或dispose1替代callback退出。主Destroy返回与callback偏序没有统一强制先后，按事件如实记录。

TRX + 完整业务正常退出/清理 + 同UID成功terminal + MTP会话exit0，可有限认证这个测试合同成功及MTP正常会话完成；业务completed不是独立进程exit。NativeRun返回MTP值是源码预期，不是OS观察。若外层仍不返数值OS exit，则独立OS exit未证、没有supervision.json，结论必须明确保留此缺口；不得冒监督Exit0，也不人为要求无限新runner。完整合同/证据不齐则canary未认证而停止。有限认证不升级成全进程无异常退出强认证。

canary有限认证成立后才建议另授权x86余C8/O6/O8三单体；再独立预算x64四单体，每UID单任务，均复用对应clean并按同规则回读。不是当前八项全跑授权。发现文档与能力审查已完成；未来执行是新的预算工作项，不留作本收尾待办。
