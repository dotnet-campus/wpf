# Public cache contract 实证闭环

状态：已由 [生产packet修正正式收尾](replacement-bitmapcache-public-packet-fix-closeout.md) 关闭公开命令差异（ClosedLimited）。双架构新candidate actual B07C9073/6BDF1EAE，官方5EBD/AD1C，四角色四帧各512/512、业务JSON exit0/cleanup完整；无独立OS exit，不冒独立diff通过，comparisonPendingIndependentComparator保留。原147B malformed/cleanup失败及下文旧预算均历史，不撤销。新默认full独立2次均已执行：x64 1823/1783/40，x86 1823/1786/37，child2，见replacement-bitmapcache-fix-default-full-closeout.md；COM旧32夹具待修，完整替换仍Open。

最新已证（execution.md/correction-execution.md）：诊断 Transform.Value 空引用已改 null-safe，官方默认 null 不是 layout 产品缺陷。修正 official task472863886f344f16be92faf80d98f626，actual5EBD，四帧各512/512，Green/程序exit0/cleanup完整。candidate task81f254a861ec42dba517ac99e1e775f5，actual147B，VisualBrush通过，BitmapCacheBrush提交0x8D遭 UCEERR_MALFORMEDPACKET，shutdown异常，cleanup未完；不得以fixture绿替代。旧执行账预算独立保留。

本轮恢复入口：核对 PresentationCore10.0.13 managed发送定义、官方native exact struct、当前手写decoder及cache siblings，确证后最小修正并完整单元/公开运行验证。预算 normalbuild≤3、定向tests≤2、Nativepublish≤2、COMpublish≤3、sandbox≤5；当前本轮均0。正式新收尾目标 replacement-bitmapcache-public-packet-fix-closeout.md。旧defaultfull1823/1821/2与诊断40红保留，HWconsumerloss/audio/naturalresources/finalfull仍Open。

执行账：`../../artifacts/public-cache-contract/execution.md`。已执行 publish=1（exitCode=0，implicit build=1）、sandbox=1（outer 300 秒，raw 回包无数值 exit，输出截断）。生产与 Native 未改。公开 RTB 不是原 HAL HWND；不据 BGRA 文件名判绿，不将 producer bitmap 作为 consumer 证据。

证据：`../../artifacts/public-cache-contract/original-readback/Results/Results/original/result.json`，任务 e879331e8e6d4d7aa39c7fc214af6395。最终结论待全文、身份、像素、cleanup 及合同核对后追加。原 full 两红及全部历史保持。
