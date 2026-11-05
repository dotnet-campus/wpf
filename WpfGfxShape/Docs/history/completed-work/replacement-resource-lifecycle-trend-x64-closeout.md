# x64 同一资源生命周期趋势：SourceBlocked / Execution NotRun 收尾

## 裁决

**SourceBlocked / Execution NotRun / 文档收尾Completed；完整替换Open。** 本轮未获得x64趋势硬门结果，不能沿用x86通过或历史x64单轮通过冒本轮16轮通过。显示根因Unknown，软指标Unknown（未运行），没有内存Green或无泄漏结论。

先核来源发现当前授权权威文件不存在：
`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\postfailure-source-rebuild-x64-r1\native\wpfgfx_cor3.dll`。
相对目录、绝对native目录、精确DLL检查均返回不存在；按名称递归查找权威目录无匹配。没有以旧clean替换、没有重建Native（预算0）、没有启动注定缺来源的正式发布。未消耗任何执行预算。阻塞不是build失败或沙箱故障，不存在需检查的沙箱task，也没有重试执行。

当前要求SHA为 `54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD`，本次未能认证实际文件。预期official为 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，未加载认证。旧40D0不得冒当前。历史来源报告1–40亲读，描述54BC62历史来源链；历史记录不能替代本轮文件存在/hash验证，原来源artifact记录亦缺失。

## 预定义范围与独立预算

快照在 `WpfGfxShape/artifacts/resource-lifecycle-trend-x64-r1/predefined-scope-budget.md`，执行前落盘；限定仅复用现有诊断配置，不改任何源码/项目/测试/资源/SDK/包规则，不复制门、不增加场景。

|独立项|授权上限|本轮实际|
|---|---:|---:|
|normalCOMbuild：ResourceLifecycleTrendDiagnostic（Release优化语义）/win-x64/net10.0-windows|1|0|
|成功正式COMpublish，同配置/self-contained NETCore及WindowsDesktop10.0.13|1|0|
|正常publish隐式编译，单独披露|随正式publish1|0|
|Native发布|0|0|
|Windows沙箱，外900秒|1|0|
|父正式协调body，840秒|1|0|
|official角色 / candidate角色，各360秒|各1|各0|
|每角色同PID fresh生命周期，轮20秒|各16（4预热12测量）|各0，16轮全NotRun|
|每角色固定样本|65|0，NotRun而非零资源|
|发现/重试/x86/full/显示/ETW/额外测试/终端脚本/再委派|0|0|
|源码/项目/测试/资源/SDK/包规则修改|0|0|

本轮真实运行时长、exit、warnings/errors均不适用：无构建/发布/运行发生，不能填exit0或warnings0/errors0。工具检查参数与原可见停止证据见同artifact `source-check-visible-returns.md`。没有publish/clean/sandbox-results、父TRX、supervision、comparison、role summary/identity/samples/descriptive、32轮business/events/callback本轮原件，所有相关核验NotRun；未捏造testSHA/MVID/PID/TRXID/协调UID或运行时身份。

## 已亲读与未读边界

已读最新next-session-handoff头部、嵌套x86-r2入口1–13，并跟读误放根Docs的完整x86-r2报告1–50；保留该完整报告不复制或改写。当前执行规则1–27、global.json1–35、历史x64来源报告1–40，以及五份状态文档开头已读。

启动修正、合同review、项目和循环partial的长返回存在4000字符截断，不冒全文：准确已返回边界记在artifact。静态关键切片已读：ResourceTrendCoordinator207–265显示固定opt-in、840父CTS、official先成功且16轮/65样本才candidate；ExitDiagnostics22–53显示仅诊断符号和精确FQN开放840，其他filtered120/full1500。csproj头显示诊断符号/Optimize=true，并默认绑定x86 Native，故x64正式publish必须显式覆盖AotDllPath和AotRuntimeIdentifier，不能误用默认值。普通build无需强求工具传自定义属性，但来源阻塞下本轮未执行build。专用role/回收其余源码未完成全文复核，亦未编译认证入口或opt-in，本次不冒完整启动链认证。

此前阅读工具没有每次全部逐字即时保存；本轮只将决定停止的准确参数/原可见短返回及已知截断边界持久化，不声称完整raw工具流水。没有构建/测试日志被覆盖，也无日志缺失下补造warnings。

## 固定合同保留（不是本轮通过）

唯一准许下一次运行仍应从新独立`resource-lifecycle-trend-x64-r1/clean`启动`WpfGfxShape.ComAcceptance.exe`，参数为`[--supervise-exit,Results/resource-lifecycle-trend-x64-r1,840,FullyQualifiedName=WpfGfxShape.ComAcceptance.UceChannelTests.WhenRepeatedAsyncSharedWicLifecyclesCompleteThenEachRoundReleasesOwnershipAsync]`，外900，完整任务回收同artifact/sandbox-results。正式发布必须明确上述Native源/win-x64、self-contained10.0.13；本次未发未跑。

每角色一个PID16fresh实例/65样本，无强制GC，不按数据改预热/阈值/窗口/斜率算法、不丢峰/加轮。硬门须逐轮真实WIC callerRelease、A关闭前后保活、B新visual更新、duplicate最终deleted、callback恰1且exit、真实channel destroy/disconnect/deinitialize、已知对象账0、primary=null和cleanupErrors=[]。KillFailed/Unreaped/强杀/超时资源Unknown，不可正常Pass。callback退出不冒所有worker退出。

PrivateBytes/WorkingSet/handles/threads/managed/naturalGC全部仅DescriptiveOnly；本轮首尾、范围、窗口、斜率及单位数值Unknown，没有本轮数据不能引用x86数字代填，不猜增长来自缓存。即使将来有限门通过，也不证明长期无界稳定/HW/loss/实际音频/媒体全闭/full或全部GPU/worker归零。

## 下一主要工作建议与停止

首先恢复或提供**原已批准54BC62权威Native文件及来源记录**到原路径，并通过实际SHA/来源认证；不通过Native新发布0的约束变相重建、不选其他旧包。然后由监督方决定是否继续尚未消耗的既定一次性x64同门预算；先完成必要启动源码与包身份核验再执行，不能把本报告当运行授权扩容。此为缺输入恢复，不建议无限新微门或无依据重跑原显示用例。后续主要收敛工作应基于现有剩余矩阵明确完整替换的未证合同与可观测验收边界，而不是把有限趋势升级全局Green。

五份入口已同步本轮SourceBlocked/NotRun并保留x86报告、所有历史账和旧正文；本轮结束后停止，不追加执行。
