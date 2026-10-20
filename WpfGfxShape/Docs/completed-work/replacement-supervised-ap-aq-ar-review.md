# AU AP/AQ/AR独立证据审查与交接

## 范围与结论

AU只读规则、方案、现成源码/证据并写指定五份文档；主代理只监督。无代码/项目修改，无构建发布运行，无终端/shell/直接间接脚本。已先读current-execution-rules、完整替换方案及resource-brush-fix初稿、curve-causality、managed-baseline报告。**本包文档收尾完成，完整替换未完成；最后源码四格full未完成。** AS/AT报告及源码未修改，不预判并行结果。

## AP：已亲核与仅交接

详细逐文件路径、行号、八份独立JSON文件名及实际SHA见[AP最终报告](replacement-supervised-resource-brush-fix.md)。统一原包根 `artifacts/ap-resource-brush-20260721/`。

|项目|AU亲核|裁决|
|---|---|---|
|修复前复现|pre-x86-repro/Results/aot-identity.json：原AM SHA C422…、X86/4、producer filter；原崩溃栈未取得|AP复现GetState/Ensure崩溃为交接事实，不冒称AU亲核栈；无TRX不算成功|
|成员合同|NativeResourceBrushRealization槽9 core/10 brush/11 effects及回调Thiscall；BorrowBrush借immediate槽10；SoftwareBrushRealization.EndPrimitive调用槽8并finally释放owner|当前实现亲读；不是AU改代码，也不独立证明长期释放收敛|
|producer|双架构ap-producer-<arch>.trx:731-734各119/119，Completed|原过滤范围有界通过，不代旧6+3复验|
|realizer|双架构ap-family-<arch>.trx:1148-1151各78/78，Completed|有界通过|
|宽族|双架构ap-bitmap-family-<arch>.trx:2931-2939各251/247/4，Failed退出2|四处消息395/414/433/452缺glyph-arial-original.json；不是全绿，不证明glyph像素等价|
|新Managed|双架构managed-readback-<arch>/TestResults/ap-resource-unit-<arch>.trx:35-38各3/3|定向回归，不是full；实际加载托管程序集SHA未取|
|独立像素|bitmap-family-{x86,x64}/Results/independent-bitmap-realizer-*.json八份全文|官方/候选未平移两红像素、平移首透明次红，16次S_OK及三类回调各16，ContextMatches=true；两个输入有独立内容，不外推全组合|

实际producer Native SHA x86 `AB8EEB23982411FB5F74CB0F65ECB7D1E0804A542B8066FB3309ED4DE89B81BF`、x64 `6F8307E80245F2E84026608613816F5E6962730520833108A8788CA5507AD33F`，与独立像素JSON候选一致。family identity虽同SHA，Arguments为source-clip子角色，不能当父78完整参数。八份业务JSON实际.NET10.0.13、官方10,0,1326,50504，布局x86 48/164、x64 80/208。未重算SHA/二进制像素，不冒用Native SHA作托管identity或绑定最后源码。

递归枚举首次未列TestResults，不能据此猜通过或断言不存在；后经直接目录/逐路径读取八份顶层TRX全部找到。原崩溃异常输出未亲核仍标交接。旧6 RotatedDrawing和3 original-wpf缺reference未在新119过滤中复验，继续开放；新增四glyph缺参考不能与旧三项混为同一批。

## AR：AM快照，不是最后源码

AU亲读 `artifacts/ar-managed-baseline-20260721/readback-x64/TestResults/ar-managed-x64.trx:25503-25506`：Completed，total/executed/passed=4249，failed/error/timeout/aborted/notExecuted及其他非成功计数全0。

AU亲读 `readback-x86-diagnostic/TestResults/log_261019125013119.diag:37810-37830`，末开始事件为 `WhenBaseDeviceQueryForExtendedInterfaceFailsWithPointerThenTemporaryReferenceIsReleased`，末时间2026-10-19T12:50:15.6500185+00:00，之后文件结束；实际源码位置显示Direct3D9DeviceInterfaceQueryFailureTests.cs。该目录专用搜索无完成TRX。末事件只是线索，不能证明该用例导致崩溃、卡死或timeout，也不生成x86执行/通过总数。

AR报告的四worker、实际runtime/包与输出路径失败尝试属于报告交接，本包未逐份复核。选用AM现成自包含快照；x64全量不代表AP/AQ后最终源码。两架构实际加载托管生产/测试程序集SHA未取得，不能造值、用NativeAOT SHA或ProductVersion补齐，更不能用当前入口源码推断旧包已产出identity。参考[AR报告](replacement-supervised-managed-baseline.md)。

## AQ及并行边界

只读[曲线因果报告](replacement-supervised-curve-causality.md)：AQ预量化、整数步长精确求值与整数HFD在限定历史实验摘要记零差；原44/10差、正数小曲线无变换与测试侧flatten干预边界必须保留。无生产修复，不证明官方内部HFD，也不证明负数半格/变换顺序/大曲线/stroke或GPU圆角。AU没有重新计算AQ原始像素/轨迹，零差为报告交接，不能写生产闭环。源报告提及AT的内容只作其当时交接；AS/AT仍按用户要求视为并行，不推断最终成功、失败或完成，不改其报告。

## 测试线与问题线并行交接

1. 测试线：集成稳定后用最后源码、正常依赖图构建生产及两个正式项目，运行Tests/ComAcceptance × x86/x64 Release四格完整适用full。每格明确任务内绝对TestResults路径，回收parent TRX、actual identity、原始内容/时序/资源/恢复及异常输出；进度和child TRX不代parent完成。不拼AM快照full/AP局部Green，不累计通过率。SHA仅证据归属，不另设无谓推进前置门。
2. 问题线：旧6 RotatedDrawing、3 original-wpf缺参考与新4 glyph缺参考分别保留；曲线生产差异、通知有效target同步、非零SyncModeStatus及迟到消息未闭。真实屏幕HW/duplication帧、真实loss→Reset→新色HW消费、持续媒体、长期CPU/GPU/线程/callback及异步关闭继续是关键缺口，不由软件RTB、回调次数或全量数字关闭。
3. 下一独立参考任务仅纯C#：真实官方同版同架构生成并记录actual identity/原始输出，仅ignored artifacts；不写tracked ReferenceData，不改golden、阈值、skip，不把缺文件当通过。生成后仍须相应用例正式复验；AU不执行该任务、不无限扩大scope。

## 文档交付

更新AP最终报告、Docs/next-session-handoff.md、Docs/replacement-issues-and-plan-status.md、监督session-summary，并创建本文；均落盘后专用回读。历史AO及更早内容保留，以AU首节覆盖过时的AP尚未开始/自产仍全部未修状态，未删除历史失败。未写AS/AT报告或源码。
