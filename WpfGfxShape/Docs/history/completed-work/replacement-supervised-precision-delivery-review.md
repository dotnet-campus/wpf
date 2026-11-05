# CA precision / desktop delivery 最终独立审查

## 1. 裁决与范围

**BW–BZ没有合格生产修复；完整替换未完成。** CA负责独立收口，主代理仅监督。只用实际专用读写搜索工具；无终端、直接间接脚本、代码修改、生产重发或无信息full。先读执行规则、完整方案及BW/BX/BY/BZ报告，再亲核下列原件；获明确授权先补完[BZ初稿](replacement-supervised-curved-primitive-fix.md)并全文分段回读。以下相对路径均在工程根。

本轮只改五文档：BZ报告、本报告、next-session-handoff.md、replacement-supervised-session-summary.md、replacement-issues-and-plan-status.md。历史保留。证据文件内容亲读不等于独立重算全部像素/SHA；不认证整文件历史hash、全仓diff或未验证版本。旧程序集与当前源码严格分开。

## 2. BZ：有限成功不能关闭兄弟差异

亲核范围与实际完整SHA/MVID/远端task详见已补完BZ报告第2–4节。原件根 `artifacts/bz-curved-primitive/`：

- `run-final-{x64,x86}/Results/bz-final-<arch>/supervision.json` 全文1–38、stdout全文1–18/1–16：两格stdout total/pass/fail/skip=2/1/1/0；监督实际Exit2，Reason=exited，非timeout/cancel/kill。失败为 sibling fillsExact。
- 两格 `artifacts/bw-curved-fill/precision/comparison.json:1–19`：rectangle/ellipse均0字节差且nonempty=true、threshold0；对应br/identity.json:1–14绑定实际Native候选、software RTB。
- 两格 `Results/bz-siblings/comparison.json:1–119` 分段核关键结果及尾部：nested ellipse/rounded各18字节差，fillsExact=false；其余六fill为0且非空；ellipse stroke162/128、rounded stroke106/60，不把stroke当已修。没有旧新同程序集相同nested基线，**18差不能裁定为新增回归**。
- sibling official/candidate四份identity全文1–13，测试SHA/MVID与父监督一致。候选SHA x64 `8DD2BF6272D1FE969AC409F920C90F2D9977647A44F3373945D6A65377278CE5`、x86 `DE4F55623582E2396383D38F4BC469990A7A4AAFAB52A343C6B266C1FB7E3D01`；实际requested=actual，.NET10.0.13，官方10,0,1326,50504，非当前撤回后身份。
- 父TRX：整个BZ根搜索child.trx未找到，两run-final搜索.trx仅报告扩展DLL。stdout声明生成位置不是亲读TRX。这里2/1/1只认stdout；监督数值退出独立确认，**父TRX缺口保留**。

## 3. BW：真实反例及已补强oracle

亲读 `artifacts/bw-curved-fill/run-{x64,x86}/br-<arch>/Results/bw-precision/comparison.json:1–31`：BR和BU baseline两种候选均rectangle64/ellipse32，双架构一致、非空、threshold0，反例已从BT源码推导升级为实跑。

同两目录 `Results/bw-stroke/comparison.json:1–134`分段全文：unchanged=true；八项stroke BR/BU changes=0、officialChanges=0，仍对官方arc128/122、cubic108/120、ellipse190/200、rounded104/94。这里是同架构同测试程序集旧新DLL逐字节比较结果，不等于stroke正确，也不覆盖BZ新nested场景。

亲核 `run-x64-oracle/Results/br-integer-oracle/` 五份独立evidence：sample0/2/4结尾85–99、sample1的39–50、sample3的593–607（目录UUID见[BW报告](replacement-supervised-curved-fill-precision.md)第6节），全部differences=[]、threshold0、nonempty=true；同测试SHA05949385…、MVID fa73203b-5210-458b-b515-3a4ebd8290e2，实际官方AD1C9DBB…，software而非GPU。

亲读BW父stdout x64全文18行=2/1/1/0、x86全文16行=7/6/1/0；补强stdout全文12行=5/5/0/0、监督全文38行Exit0且非timeout。双架构BW退出2及其完整角色身份以BW报告第4–5节为引用范围，未在CA逐份重核所有角色监督。BW整个根搜索child.trx未找到，父计数只能称stdout计数，不能冒称亲读父TRX。

## 4. BX：桌面交付仍Red

亲核 `artifacts/bx-desktop-delivery/` 五父TRX的ResultSummary/Counters/RunInfo：run-x64/bx-native 210–220、run-x86/bx-native 199–209、control/run-x64/bx-control 242–252、control/run-x86/bx-control 315–325、control/final-x64/bx-control 326–336（均在TestResults/child.trx）。依次total/pass/fail=2/0/2、2/0/2、2/0/2、4/0/4、4/0/4，均Exit2，TRX timeout0。

亲读final-x64监督全文38行：实际测试SHA6813EB78…、MVID78965af3-d0a3-4328-8e62-91b8074c7035，Exit2、非timeout/kill。亲读其Results/bj-fresh-1.json身份及首红/蓝帧字段1–46：backbuffer红FFFF0000/蓝FF0000FF、Present0；client/screen65280、frontFF00FF00仍绿，accepted=false，duplication没有fresh。GDI observation也失败，pixel0是无结果，不称duplication GDI green。余帧/其他任务窗口及adapter轨迹引用[BX报告](replacement-supervised-desktop-delivery-differential.md)第3–5节，不冒称CA全文重核所有JSON。

BJ反证亲读 `artifacts/bj-capture-fresh-frame/stages-run-x64/TestResults/bj-stages-x64.trx:408–413` 为Completed16/16；red/blue/red及public三阶段成功引用BX第5节，本次未亲核其全部业务帧或数值退出。不抹除历史成功，不推定Basic Render Driver永远不能产帧，不以GDI替代HW、不无证归因环境。

## 5. BY：保留构建修复，非业务验收

亲核 `ExitDiagnostics.cs:21–27` 为ArgumentException(message:null,paramName:nameof(args))，相邻ArgumentOutOfRangeException正确。CA2208根因修复保留，无新资源文案、入口改动。亲读ComAcceptance.csproj全文36行、工程Directory.Build.props/targets全文及Tests/ComAcceptance内WarningLevel|NoWarn搜索未命中：未见本轮新增压制。编译器日志仍有SDK既有/nowarn:1701,1702,8002，不能夸称所有编译参数绝无nowarn；未以此压CA2208。

亲读 `artifacts/by-diagnostic-build/{default-x64,default-x86,diagnostic-x64,diagnostic-x86}/build.log` 汇总分别2403–2415、2398–2409、2398–2409、2399–2410：**四格各92warning/0error**。独有正常发布及旧BD AotDllPath绑定见[BY报告](replacement-supervised-exit-diagnostic-build-fix.md)第3节，非当前生产重发。

亲核run-{x64,x86}/by-control/TestResults/child.trx全文26行及supervision全文38行：各1执行1通过、Completed、Exit0，非timeout/kill；实际程序集task998a892b…/5037888e…，SHA124E4320…/01BE18C3…、MVID68e585b4-bee4-4449-883c-499059521414/e9e7ea67-6805-4d58-8142-481105efe287。这只验证路径控制/退出，不验证加载候选、precision或HW。

default1758无GDI、diagnostic1760含GDI两数据项，是BY报告第4节的专用工具发现输出记录；未落盘完整清单，CA无法独立回读该清单，不冒称亲核1758/1760全项。源码条件编译边界已亲核，不把“未引入诊断”写成skip。CA本次不新构建；BY历史四格成功不认证最后源码全部可编译。未发现所读源码明确悬空引用风险，但当前Tests/ComAcceptance双架构最终编译未独立完成。

## 6. 最终源码与保留清单

- 生产：Code搜索fillPrimitive/ApplyFillLocal/fillFloat/SoftwareFillBezier均无命中。亲核Abi/NativeShapeReader.cs的float输入、本地控制点→共享SoftwareBezierFlattener.Flatten→输出transform.Apply→TryCreateFigures（24–75）；Core/GeneratedImageTransform.cs的double Prepend/Apply（3–35）；Core/SoftwarePathCoverage.cs 266–405 ellipse/rounded double本地构点、parent.Prepend(local)、共享Flatten/Quantize及group递归/rectangle。没有已验收的分阶段float+整数fill生产实现。只确认这些行为范围，不能认证历史整文件hash恢复。
- BW保留BwCurvedFillPrecisionTests精准失败/非空/零阈值，归档批量stroke方法移出默认发现属BW既有收口，CA不删改/skip回归；BrFillIntegerTests自含整数oracle、BM显式实验与既有曲线诊断保留。
- BX保留D3DImageTests.CaptureFreshFrame.cs显式BX_DESKTOP_DELIVERY_DIAGNOSTICS GDI对照；亲核90–122及Trace13–16默认无注册即返回。OutputWindow.cs完整WndProc86–94无Trace同步I/O，既有erase/paint策略保留，不盲撤回为背景重绘。不是生产修复。
- BY仅ExitDiagnostics.cs单行修复；BL/BN监督、阶段/HRESULT与NativeRun现有入口保留。
- BzCurvedPrimitiveTests.cs internal类/方法、无发现属性；Tests名称搜索仅自身，未见显式入口，**未接入实验**。历史运行身份不绑定当前默认发现；不通过删除/skip掩盖回归。源码已亲读首尾，无证据把它列为已接入稳定回归。

## 7. 下一最小任务与剩余合同

**先旧新DLL同程序集nested基线，判明18差是否新增；再做float逐阶段/本地保存/group来源传递及设备28.4整数fill集成。** 保留窄图元、原44/10、兄弟及同程序集stroke非空零阈值对照；不再盲恢复撤回代码。异步surface必须先稳定真实fresh前置，再合法初始化、真正事件唯一所有权与提交完成/关闭偏序，重复新内容、cleanup及双架构正常退出；不能拿无surface控制代替真实合同。

44/10未生产修复；BD旧x64 full1740/1699/41退出2、x86缺父full完成TRX仍属历史，CA不重核/移绑新身份。最后稳定源码Tests/ComAcceptance×x86/x64 Release完整适用四格仍缺。真实物理GPU、device-loss/reset后新色、持续媒体、长期CPU/GPU资源线程callback/异步关闭、通知有效target同步/非零SyncModeStatus/迟到消息、GPU圆角、合法大shader合同及E_HANDLE/首次堆损坏均开。五文档全文分段回读只完成审查交接，不代表完整替换。
