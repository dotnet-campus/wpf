# Direct clear：有效帧合同与 Present 调度有界审查

## 结论、范围与证据等级

本阶段生产/测试源码及配置修改0、build0、publish0、沙箱0、测试0、终端/脚本/再委派0；仅报告及恢复交接文档。已读 current-execution-rules、完整替换方案1–120、当前事项/基线最新节、clear-control/resource-graph-audit、章程实现合同主要节及仓库贡献规则；未找到 AGENTS.md。SDK10.0.107、net10.0-windows/Nullable enable 仅核未改。

**无法证明本次空 root 提交实际形成了一个交给所观察 DC 的有效帧；但也未发现足以认定首次清屏命令无效的确定协议错误。** 不能把“null root 合法”升级成“本次必见绿”。本次 green consumer flush 成功可以发生在根本没有 Render/Present 的合成轮，也可以发生在 target Present 成功返回、却没有任何有效显示子 target 的轮次。

新增关键区别：
1. 实际 SetRoot=0，即 native `m_pRoot == NULL`，**不是空 visual**。旧资源图报告§6“空visual”措辞不对应本次实包源代码；本报告更正概念，不改历史结果。
2. setup commit/flush 本身已会安排合成，可能已完成首次 Clear/Present。后续 invalidate 只是 invalid-region/no-screen 状态更新，**不设置 needsFullRender/needsPresent**；null-root 第二轮不会因“有 invalidate”必然再次 Clear。因此不能把第二次 flush 等同于“刚执行了一次绿 Present”。这不是确定遗漏：若首轮已正常显示，第二轮不重画仍应保留绿色。
3. 自定义窗口 WM_PAINT 只 ValidateRect；官方真实 HwndTarget 会 DoPaint→DUCE invalidate，并处理 display availability/power/enable。**恢复反馈环确实没有等价实现**；只有本次实际遇到 recreate/screen-access/重新呈现条件时，它才成为根因。现有原件未记录这些条件，不能据此直接补协议或归环境。

证据等级：R=本次回收原件；S=当前测试源码；A=仓库官方参考源码，不冒称已与 servicing 二进制逐行绑定。所有官方路径下文相对 `src/Microsoft.DotNet.Wpf/src/`，测试路径相对 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/`。没有反编译、调试器私有字段读取、ETW新采集或内部断点命中证据。

## 本次原件与身份

R目录 `WpfGfxShape/artifacts/direct-consumer-clear-control/readback/official/Results/direct-clear-official/`。亲读 direct-consumer.jsonl身份、阶段及清理字段；长行输出截断的末尾异常全文不冒全文重审；supervision.json25–40亦核读。

- task ced1177b9f8e41849623fedc404ce201，实际 .NET10.0.13/X86/pointer4；Requested==Actual任务根官方 DLL。
- official SHA256 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`，10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。
- test SHA256 `CB821F7F92434D636BA8E98AFA44F6C11E9E15F1BDD907E5DC9E6797D2460FA7`，MVID d947710e-6534-4473-8ce1-285381190852；PresentationCore SHA D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D。
- HWND22873106/target4/channel83790936/owner2436；flags12、DPI context-2/scale1、client32×16；root0/RGBA(0,1,0,1)。唯一 DC1973532=001E1D1C，非65280=0000FF00。
- setup flush 03:00:00.3941311，invalidate commit .4218355，green flush .5354562，DwmFlush .7098281，pixel记录 .9757164 UTC。仅阶段记录时间，不冒 GetPixel 精确调用起止。
- HR均0；红绑定/红 ColorFill/dirty/PresentPacket/event/DC未到达。四refs0；callback Available1/Version7一次；owner销毁1，Errors=[]。监督 exited/Exit2、无timeout/kill/drain错误。
- candidate `20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D`仅文件认证，运行0；不将官方失败归候选。

## 逐阶段合同、代码位置、本次证据与未知

|阶段|S发送与A官方依据|本次R与未知|
|---|---|---|
|建立target/加入manager|DirectConsumer.cs139–190资源46+0x31；uce/composition.cpp1799–1833 HwndTarget_Create调用ProcessCreate后AddRenderTarget。hwndtarget.cpp398–418保存HWND/DPI/flags。构造25–55 enabled=true/cookie0/full=true|创建及setup flush0；没有target manager数组/实际对象指针观测，但未发现漏发关联命令|
|window settings/enable|OutputWindow.cs73–83发72字节0x33：GetWindowRect、opaque/notLayered/alpha1/enabled1/cookie0。hwndtarget.cpp511–650：cookie匹配则enable，保存screen rect及属性。HwndTarget.cs2107–2270真实WPF用client-in-screen坐标，禁用走OOB，启用走main|本窗WS_POPUP/无边框、DPI1、client断言32×16，GetWindowRect在此形态无已知nonclient偏差；但settings矩形四坐标没有原值日志。不得泛化该helper适用于带边框/DPI混合窗口。初始不需要人为disable→enable握手|
|SetRoot/clear|DirectConsumer.cs175–190 tree hRoot=0；uce/rendertarget.cpp138–166只在非0时取visual；新target初始root为空。hwndtarget.cpp658–678复制green且full=true|不是visual3作为空内容root；visual虽存在但未绑定data，也未设为root。green0、root0；实际native字段未读|
|flags/managed初始化|hwndtarget.cpp398–418已应用create flags；689–710 SetFlags仅覆盖允许子集。HwndTarget.cs726–805创建双channel/transform/clear/root/settings；612–632的SetRenderingMode用于渲染偏好|flags12不是Null，也非HardwareOnly；DPI1且null root不需world transform。未发现必须补发SetFlags/首轮禁用cookie/空visual的依据；真实backend unknown|
|提交批次→partition|uce/crossthreadcomposition.cpp114–127 SubmitBatch→partitionmanager.cpp356–388 enqueue/NeedsBatchProcessing/唤醒worker；crossthreadcomposition.cpp233–257 OnBeginComposition抽取批次，358–383反转为原序，随后ProcessBatches|不是必须managed timer才处理commit。实际批次合并到几轮、worker时间点未记录；不能精确数轮|
|Compose与display gate|composition.cpp598–685先UpdateDisplayState、再OnBeginComposition处理命令；DISPLAYSTATEINVALID且compat不允许时跳Render；775–827 Compose把成功码归S_OK；841–859 Tick后manager.Render|命令可处理、flush可成功而Render被跳过。未采displayCount/compat/deviceState，不能断言此分支已命中|
|target manager Render|rendertargetmanager.cpp323–397遍历targets并OR fPresentThisTarget；hwndtarget.cpp82–146 Ensure target、enabled/no-screen/zombie gate之后，null-root且full时Clear/InvalidateInternal/needsPresent=true|Clear不是DrawImage；不依赖D3DImage PresentPacket。HR0无target gate实测；也没有Clear执行凭据|
|desktop target/bounds|hwndtarget.cpp846–959创建desktop RT、SetPosition(screen rect)、UpdatePresentProperties；meta/desktophwndrt.cpp442–515、590–740按monitor相交启用/resize子RT，surface bounds变成本地0..width/height|有HWND/可见/非最小化不等于内部子RT fEnable或present rect非空；未读subtarget数、intersection、resize结果。desktoprt.cpp552–560 Clear委派meta；608–665 Present仅在fEnable且present rect相交时才调实际子RT Present。即使外层Present执行亦非像素实证|
|invalidate与本地坐标|DirectConsumer.cs191–194发0x37/LTRB(0,0,32,16)。hwndtarget.cpp738–750清noScreenAccess、记录非空本地区域；969–989 InvalidateInternal只置invalid flags。215–239发invalid regions并返回needsPresent|client32×16/中心16,8在范围内；该invalidate不强制full，也不独自设置needsPresent。setup首轮若已清屏/Present，第二轮可无Clear/Present；若首轮失败则依赖保留下来的full及恢复条件|
|Render→schedule Present|partitionthread.cpp132–167 Compose若needsPresent就SchedulePresentAndCompleteProcessing，否则直接FlushChannels；partitionmanager.cpp463–474置PartitionNeedsPresent|没有独立“漏启动composition timer”证据；batch即可驱动。needsPresent实际未知|
|HWND Present|composition.cpp972–1033若deviceState Occluded则跳manager；否则manager Present，无listener时frameTime默认0。rendertargetmanager.cpp592–650逐HWND Present；hwndtarget.cpp263–299另检查target/enabled/noScreen/zombie，复位needsPresent/full|官方present-notify/VBlank订阅是通知/节拍机制，不是无订阅就拒绝呈现。actual target Present调用及下层子RT执行未知；HandleWindowErrors1035–1133可释放重建/吞部分错误，occluded亦非失败|
|SyncFlush覆盖哪轮|clientchannel.cpp812–841→connection.cpp307–346追加SyncFlush命令、CloseBatch/Commit并等待。composition.cpp1342–1359只登记待完成channel；partitionthread.cpp151–204有present则其后flush，否则无present亦flush|setup flush覆盖setup之前命令及其对应处理轮；green flush覆盖invalidate之前命令及sync marker对应处理轮，可能与先前batch同轮或后轮，不带target/颜色/frame ID；不能说green flush强迫再次Clear。0较不支持未处理的fatal zombie，但不排除成功跳过/吞错|
|DwmFlush/DC|DirectConsumer.cs196–205，DwmFlush后一次GetDC/GetPixel16,8|DWM没有本target帧ID关联。只证DC非绿，不证GPU从未绿或正在读另一内部表面；不新增捕获设施|
|真实WPF恢复环|HwndTarget.cs1038–1051 WM_PAINT→DoPaint；1354–1394 BeginPaint取dirty→InvalidateRect；1534–1551向OOB发DUCE invalidate。hwndtarget.cpp1066–1101/1160–1216发生recreate/access问题后依靠InvalidateWindow/WM_PAINT回到ProcessInvalidate清noScreenAccess|OutputWindow.cs87–93只ValidateRect，未转发display availability/power消息。确定行为差异，但本次没有相应消息日志或错误分支证据；一次初始invalidate能清已有noScreenAccess，不能覆盖后续新设的状态|

### 严格隔离两种 Present

`resources/d3dimage.cpp249–285 ProcessPresent` 消费16字节image PresentPacket，更新interop frontbuffer/NotifyOnChanged，cleanup即便失败仍SetEvent/CloseHandle。它不是 HWND target Present，也不保证内容已绘制。此次这条路径根本未到达。HWND Present由Compose/manager/target调度产生，无须image PresentPacket；两者的event、HR与完成语义不得混用。

## 已确定差异及最小处置

- **无确定的首帧协议错误可直接修正**。不改生产、flags、cookie、root类型、surface格式、阈值或等待。
- 确定的报告概念修正：null root而非空visual；第二次invalidate/flush不是第二次Clear保证。已在本报告与恢复入口澄清。
- 确定的host能力差异是缺少真实HwndTarget的WM_PAINT/显示恢复反馈；若将来有内部/消息证据证明触发，最小修正应针对该状态反馈，不是延长等待或盲加SetFlags。现在不实现第二channel/消息补丁，避免把未证根因写进测试。

## 最多两个分支假设及已有信号边界

1. **没有完成有效HWND内容呈现**：display gate、noScreen/recreate、外层成功但子target未启用/矩形为空或没有needsPresent。现有可复用信号：IfPartitionRenderStateTests.cs40–81的注册/64字节队列读取；composition.cpp2225–2273状态Normal/NoDevice/Occluded；PresentNotificationContractYTests.cs既有type10 Presented/NoPresent解码及WM_USER(123,456) instrumentation。NoPresent能区分“本轮没要求present”；NoDevice/Occluded可缩小状态，但Normal与Presented仍不保证指定target或像素。WM_USER在hwndtarget.cpp275–286仅在调用下层Present返回后发，也不能证明子RT实际呈现。开启instrumentation/注册本身改变状态，不冒称纯被动读取。
2. **target链已走过，但window DC未反映该内容**：同窗口信号可到达target Present后位置，而DC仍错。现有window坐标/DC辅助观测可复用，不需WGC/新捕获。只有实际子RT内容/Present证据才足以与假设1完全区分；目前均无。partition正常或通知到达不能单独证实假设2。

本次direct未注册上述通知，也未记录WM_USER/availability消息，当前工具不能追溯进程私有字段或补证内部命中；不会把另外partition的IF/Y历史输出嫁接到本次target。把现有observer接入direct虽可增加状态分类，但还要新增构建运行，且仍不足证明像素合同；不选继续手工协议审计循环。

## 唯一推荐下一门：已存在public WPF target official矩形控制（需另授权）

**仅运行一次现有 `WpfGfxShape.ComAcceptance.D3DImageTests.WhenOriginalPublicHwndTargetDrawsThenWindowContainsHardwarePixels`，不再运行direct清屏/红图，也不接候选。** 已用离线DLL公开API确认该方法就在本次clear-control clean包，不需新增源码或捕获设施。既有 `D3DImageTests.cs298–325` 创建public HwndSource/DrawingVisual/DrawRectangle，经真实HwndTarget初始化、MediaContext/Dispatcher调度与窗口消息反馈，用同种32×16/40,40/topmost popup/中心window DC；这是对管理链与手工链的架构判别，不是等价同一target实验，不把其成功解释为原target有效。

选择它而非第四次direct：独立绿已否定了“只需排查interop”的前提；完整managed target控制能直接验证当前沙箱/官方WPF/DC是否至少存在一条可工作的真实调用方路径，复用既有完整握手而不是再发一组合法但不可见的手工命令。它不含public D3DImage/SetBackBuffer，不能认证interop。ContextIdle只是调度进展，不是帧内容凭据，内容仍由DC断言。

冻结最小预算：源码/配置修改0、build0、publish0、official沙箱≤1（现有监督120秒/外180秒，仅精确FQN）、发现0、candidate0、Native/native-only/旧线程变体/WGC/x64/full/resize0。用现存clear-control clean，记录所用test SHA/MVID、runtimeconfig两框架10.0.13、包来源，回读精确TRX、stdout/stages/监督。不将本次尚未运行写入累计账。若无法建立相应实际身份，停止而非重发包/加预算。

身份限制必须预注册：现有public方法只打印由Visual.Assembly推导的installed wpfgfx文件路径/SHA，不像direct那样枚举实际模块，也不证明该文件就是actual加载模块。现有监督提供test SHA/MVID；原输出不足的模块身份如无现成证据，不补跑、不冒称exact5EBD绑定，只记public installed-WPF辅助控制/身份覆盖不足。该方法沿AssertCombinedPixel，实际既有阈值每通道≤1，stdout输出raw expected/actual；本门额外解释只有raw严格00563412才算严格内容控制成功，不修改原阈值或golden，也不将仅容差Passed冒零阈值通过。

终止/转向标准：
- 身份/监督/清理异常、ContextIdle超时、像素失败、raw非严格值或raw缺失：立即停止，候选0；冻结当前consumer验收阻塞，要求可验证实际显示/目标状态的既有证据或可运行真实WPF条件，不继续补手工探针/等待变体/捕获设施，不笼统归环境。
- 唯一official控制严格内容成功且其覆盖身份明确：停止本门；说明真实managed矩形路径可用、手工target仍未闭。下一工作方向应是另授权复用该public HwndSource作为真正D3DImage消费者的最小实现，而不是重跑direct；那是未来实现门，当前不新增、不自动运行、不判candidate通过。
- 无论结果均不追加本轮任务；官方控制不过候选不运行规则保留。public成功也不自动授权candidate或full。

## 预算与未闭事项保留

本阶段执行全部0。direct累计build4（1败3成）/publish3/official3真实体3失败/candidate0。WGC累计build4（1败3成）/publish4（1败3成）/沙箱3（发现1真实2）/体2失败，已退场；撤后default build1成功单列。通用包退场残项保留。KS build1/publish1/沙箱4/请求3/发现1/体1/A1/B1/native2另账。旧成功SHA不认证当前20B3，完整替换Open。
