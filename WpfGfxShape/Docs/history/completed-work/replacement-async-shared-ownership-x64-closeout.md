# async-shared-ownership-x64-r1 最终收尾：双架构有限门完成

## 裁决与预算

x64 official/candidate准确单体各TRX total/executed/passed=1/1/1、监督ExitCode0，实际模块身份、完整业务事件与真实cleanup通过。不是仅据头部cleaned=true认证。结合既有[x86收尾](replacement-async-shared-ownership-x86-closeout.md)，两架构健康异步共享所有权这一有限门可以关闭；完整替换仍Open，整体基线不升级，显示冻结。

本次接续仅回读原件和写Docs：源码/项目/测试代码修改0，build/test/publish/Native/沙箱新增全部0，无终端脚本、再委派或补跑。x64本门累计normal COM build1（Exit0、133warnings/0errors）、COM publish1（Exit0）、official1/candidate1共2沙箱且各1通过、Native发布0；预算结束。x86 ownership、source-rebuild、partition/direct/public/WGC/KS及其余历史账各自保留，不冲销、不相减。

## 准确原件根、结构及回读边界

以下路径相对仓库子根 `WpfGfxShape/`，证据根 `artifacts/async-shared-ownership-x64-r1/`：

- `pre-execution.md` 1–10全文：限定输入、角色、真实引用链、预算与停止条件。
- `01-build-return.md` 1–4、`01-build-log.md` 1–5；`02-publish-return.md` 1–4、`02-publish-log.md` 1–3；`03-preflight.md` 1–5全文。build/publish数字Exit来自即时日志摘要，不来自无数值的直接成功返回。build原stdout104998字符仅末20000可见，publish52788仅末20000可见；持久摘要不是逐字日志，不补造前84998/32788字符。publish没有warning/error总数，不造总数。
- `04-official-tool-return.md` 1–9、`05-official-readback.md` 1–8、`06-candidate-tool-return.md` 1–9全文：准确参数、完整直接返回和official放行记录。两沙箱工具只返回完成及保存目录，没有数值exit；本文Exit0均来自监督。
- `readback/{official,candidate}/Results/aot-identity.json` 各1行全文，仅初始化候选身份；不能代替official实际加载身份。
- official实际子根 `readback/official/Results/async-shared-official-x64/`；candidate实际子根 `readback/candidate/Results/async-shared-candidate-x64/`。各自 `supervision.json` 1–40、`TestResults/child.trx` 1–26、`stdout.txt` 1–12全文、`stderr.txt` 空，均本次亲读。目录枚举漏列TestResults，两份child.trx直接精确读取成功，不记不存在。stdout中文乱码保留，不修写原件。
- official `AsyncSharedOwnership/b0da8366627d4d90893ddbb84135dcb4/business.json`：此前05记录已完整亲读1–576；本次再次完整覆盖1–42/43–140/141–230/231–320/321–400/401–490/491–576。
- candidate `AsyncSharedOwnership/b86f9be3b6b34b78b2fde35ae03624af/business.json`：此前已读1–400；本次亲读401–566可见部分（请求401–600在4000字符边界截断），随即精确540–576补齐尾部，再复核1–100/101–200/201–300/301–400。合计完整1–576，无尾部遗漏；没有将截断响应冒充全文。

未声称Diagnostic全文、全部manifest/deps、全仓库冻结或原始构建stdout全文审阅；本次不重算SHA、不新验包。限定来源沿pre-execution/03及既有[x64来源重建报告](replacement-postfailure-source-rebuild-x64-closeout.md)，COM正式打包显式引用该轮Native路径。本门没有Native重发。

## 独立业务、测试与模块身份

|标识|official|candidate|
|---|---|---|
|task|6959aa230c4e41a48b46a92b4a125d15|52f521167ff4414183a5b5198008eb41|
|业务UID|b0da8366627d4d90893ddbb84135dcb4|b86f9be3b6b34b78b2fde35ae03624af|
|TRX runId|f461900c-c359-4651-9c98-29e1e4cd465c|86788801-bc68-4e5b-82b3-f60c42ffa67f|
|TRX testId|14df25f0-b713-8701-93c7-30611ff822ce|1342b468-c217-8f38-ab71-b60fd6bc94de|
|TRX executionId|d88b229e-e02e-4f29-918a-ca729f0954a1|4a1d2b64-8751-409a-a604-2f65f5dc9e42|
|child PID|12016|8268|
|actual moduleHandle=actualBaseAddress|140720669196288|140720477831168|
|各task内requested/actual路径|根 `wpfgfx_cor3.dll`|`Native/wpfgfx_cor3.dll`|

两business `requestedEqualsActual=true`；上表真实句柄等于实际模块基址，不以初始化路径充actual。official实际SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，version `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`；candidate实际SHA `54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD`，version1.0.0.0。

双方同testSHA `4A8356201F062B01161744C6B3BBF7D9AB7DB5909F6F12A9389806FFB6B6ED43`、MVID `d41adfe8-8904-4c9b-887d-8d1f5979ba46`、`.NET 10.0.13`、`X64`、pointerSize8。业务UID与TRX testId/executionId不同，不混记。

精确FQN分别为 `WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalAsyncSharedBitmapOutlivesSourceThenFinalCloseReleasesOwnership` 与 `WpfGfxShape.ComAcceptance.UceChannelTests.WhenAsyncSharedBitmapOutlivesSourceThenFinalCloseReleasesOwnership`。监督实际等值过滤、TRX定义/结果一致，非发现或整组运行。双方TRX其余失败计数0，监督Reason=exited、ExitCode0、HasExited/OutputCollected=true；TimedOut/Cancelled/KillAttempted及两类drain超时false，SupervisorError=null。内120/外180，均从同clean启动，各独立Results，没有重试。

## 真实所有权、偏序和清理裁决

manager0、connect0，同健康connection/partition内A(source0)/B(sourceA)。official A/B为1778255551136/1778255549120，candidate为2/3。两边Bitmap95/resource1与Visual39/resource2独立duplicate至B1/2，Visual不引用Bitmap。

|事件|两角色实际结果|
|---|---|
|Transfer、两Duplicate、A close/commit/flush，order10–15|全HR0，源先提交，先于目标使用|
|caller wrapper/frame/stream真实Release，17/19/21|各references=1，disposals仍0；不冒所有COM引用归零|
|A关闭前/后22/24，A.Destroy23|dispose0/0，真实Destroy HR0|
|A关闭后B新Visual alpha0.25、close/commit/flush，25–28|全HR0，dispose0|
|B Visual/Bitmap最终Release，29/30|各HR0/deleted1，不以A卸载代替B显式释放|
|B release close/commit，31/32|全HR0，dispose0|
|stream Callback.Enter/Exit，33/34|恰1进入并退出，dispose最终1；official线程11400，candidate11912|
|B.Release.Flush35、B.Destroy36、Disconnect37、Deinitialize38|全部HR0，dispose1|
|既有5秒观察39/40及WIC scope Dispose41|finished=true，正常完成，无延长等待|

两边callback均在release commit事件32之后、release flush返回事件35之前完成，早于B.Destroy；不写成必须Destroy后触发。这是实际stream最终释放callback，不是模拟计数、Available或device-loss。

完整事件与尾部相互印证：created=destroyed两真实channel、released=[2,1]、disconnected/deinitialized=true、completed/cleaned=true；outcome=(BeforeSourceClose0,AfterSourceClose0,FinalDisposals1,CallbackExited1)，callbackEntries1、callbackErrors=[]、primary=null、cleanupErrors=[]。两结果副本提前dispose/缺失dispose负控均拒绝，只是validator自检，不是生产故障注入。

## 覆盖、历史与唯一下一建议

本次新增54BC62 x64这一有限所有权/关闭合同，与既有15A545 x86对应门一起完成；两架构postfailure/isolation原有限通过和限定来源链保留。旧20B3红、40D0隔离、旧测试身份、KN17/direct/public/WGC/KS/Ledger及所有历史不迁移、不冲销。只增加有限合同，不升级整体稳定基线。

没有证明Bitmap真实像素消费、长期资源趋势、Register异常/OOM或构造中途回滚、composition callback重入官方等价、跨connection共享、HW/resize/real-loss/media/full。独立Visual更新不是Bitmap consumer闭包。五项完整维度仍待验，完整替换Open，显示冻结。

已完整阅读实施方案1–149及当前矩阵，按方案§8.2/§6.4收敛：下一唯一建议是**零执行静态确定现候选受影响的既存AsyncDrain directClose queueState6（open）/8（committed）最小回归包**，不继续发明微小ABI/共享/异常门。本次仅选方向，不创建入口、不执行，也不是恢复旧全组。沿[有界审查](replacement-register-rollback-publicabi-bounded-review.md)已列现有方法 `WhenOriginalAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates` / `WhenAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates`，静态核对真实参数可精确选6/8、现有clean/test与当前两架构actual绑定、候选特有重入断言边界；不得用整方法FQN误跑6/12行。精确选择不可用就记录阻塞及所需另授权的两个薄入口，复用现有RunAsyncWicDrain，不扩全组或等待。执行预算须外层另授，当前新增仍0。

这只是完整计划现有受影响回归的下一步，不替代最终适用矩阵。主要后续能力缺口仍为真实WPF非空新帧/producer-consumer、真实HW与可确认device-loss恢复、持续媒体和资源循环趋势；需具备对应可观测Windows图形会话/adapter与消费链、合法可确认loss触发及媒体源并另行解除相应显示执行冻结。缺条件记Blocked/未覆盖，不以软件fallback、正常销毁或更多ABI微门代替。
