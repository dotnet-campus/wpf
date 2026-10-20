# AE/AF：独立证据回读与检测有效性收尾

## AF 最终独立验收（优先于下文历史状态）

**本包文档收尾完成；完整替换未完成。** AF先读执行规则、完整替换方案、AD present-layout-fix、AE报告及交接，只用专用读取/搜索/文档写入；主代理只监督。无代码/项目修改，无构建、发布、运行、终端/shell或任何直接间接脚本。本节是现成证据复核，不是新实跑。最后源码两个正式项目×x86/x64 Release四格全量未跑，不能拼旧full或累加定向Green。

### AD：最新集成布局修复已完成有界验收

证据根 `artifacts/replacement-present-ad-2026/readback/`。亲读顶层 `com-x64/TestResults/ad-com-x64.trx:74`、`com-x86/TestResults/ad-com-x86.trx:74`，各8执行/8通过/0失败；`managed-x64/TestResults/ad-managed-x64.trx:60`及x86对应，各7/7/0。四份error/timeout/aborted/notExecuted均0。不是全量；子进程结果不重复计数。

亲读两 `com-<arch>/Results/aot-identity.json`、通知双方actual identity.json及两Managed `Results/preflight-identity.json`：.NET10.0.13，X64/8、X86/4；实际候选Native SHA如下，明确不是V归档。AD报告声明保留AA图像生产实现，AF本包不重验源码diff。

|现场Native SHA256|x64|x86|
|---|---|---|
|AD候选|2BD7C2B9207ACF1E75182A8EC5C8B13B7B1B84E525045272733B158E8F069D5C|255E699B643E0A6F5250F80C28AE7086038E5FDA6D9959F3EEF5AE1F0C40DA1F|
|官方|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|

官方版本 `10,0,1326,50504`、commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。AD/AE最终Managed现场产品均 `0464F3455C9E5B684E91CFEF08902DFFB897ABB0A569FD8AE947E6E05BC1CA1B`；测试DLL x64 `E9BFEF5CBCA3EFF80FE7B559B63B226603EB786CE396F66072333DBF7E76B148`、x86 `C6721F1D97D7C31DED2488589ADA53E9AF7CE081B10A15D40E2E41C1569A6310`。不混同托管与Native身份，不称AF自行重算文件hash。

通知业务根：x64 `com-x64/Results/present-contract-y/770b19e2fd014620b25c1c225d7da3e3/`；x86 `com-x86/Results/present-contract-y/5159efbe2dac444292820edda5cd271a/`。AF亲读两comparison.json全文及四角色 `samples.json:1`、identity.json全文：双方compose均两条64字节0x0A，完整hex一致，前24字节 `0A00000000000000020000003C0000007B00000000000000`，其余40字节全0；Reserved@4=0、result@8=2、refresh@12=60、time@16=123。before-close/compose-again/replacement-channel均空，threshold0。是无损hex亲读，不冒充.bin独立解码或全部事件亲核。

AD生产修复范围来自已读AD报告：Presented偏移8/12/16，SyncModeStatus HRESULT@8。后者成功值0的Managed覆盖不能区分旧偏移；非零HRESULT官方差分仍未覆盖。重复FrameTime=123仅有界验证布局，不证明实际值算法（固定refresh60、最小非零FrameTime）、不同FrameTime/QPC/VSync/DWM、有效target同步分支、注销与在途销毁必要偏序。替代通道空不证明已销毁通道无迟到消息。

亲读最新AA集成的四窗口comparison（均一行frames12/threshold0/exact=true）：图像x64 `com-x64/Results/replacement-state-window/4491832266f2427c9eebdaa1b0006919/comparison.json`、x86 `com-x86/Results/replacement-state-window/39c93f3eabda4435a3e7f022047c5c38/comparison.json`；矩形x64 `com-x64/Results/replacement-rectangle-window/597da205e8c1466a8ad37f8c029a5dff/comparison.json`、x86 `com-x86/Results/replacement-rectangle-window/7dd199a0df3d4bf88ef60ee8106bea10/comparison.json`。这是RTB consumer有界回归；AF未逐帧重算BGRA、未亲核全部窗口事件/窗口角色加载identity，不外推屏幕HW或旧INVALIDCALL精确首因。

### AE：最终包实际回读，不引用早期Managed路径

根 `artifacts/replacement-evidence-ae-2026/final-readback/`。AF亲读 `x64/TestResults/ae-final-x64.trx:143`、`x86/TestResults/ae-final-x86.trx:141`：各12执行/11通过/1失败，退出2；`managed-x64/TestResults/ae-managed-x64.trx:66`及x86对应：各8/8/0。四份error/timeout/aborted/notExecuted均0。Managed最终目录及文件已实际列出并读取，身份为各 `managed-<arch>/Results/preflight-identity.json`，不是早期readback。

双COM TRX第8–24行亲核唯一失败 `WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover`：firstCreate=0、secondCreate=0x88760868、firstCooperative=0，未产生首设备真实loss，保留失败而非skip。不能声称已Reset/恢复后新色HW读回，也不据此推定非交互根因。

资源业务根x64 `x64/Results/replacement-resource-z/dc15a5421bd54956b2670e5a0a66d0c3/`、x86 `x86/Results/replacement-resource-z/34ec618b22314f19a923453d72266950/`。两identity/comparison/detection-controls全文亲读：4预热12测量、8×8、16轮/48帧每角色、Failure=null、threshold0、旧两处置观测负控true；两个独立模块同进程，明确offscreen software，非HW/恢复。实际候选仍为下文V SHA，不绑定AD新候选。

两detection-controls各Count34、ObservationOnly=true：四类非0 Release精确 `remaining-reference`，14事件各遗漏/重复共28项 `event-count`，callback-after-release及retained-read-before-close两项 `event-order`。按AE报告这些观测副本共用正式Detect；AF亲核落盘拒绝原因而未重新执行或重新审查测试源码。**34项不是34个MSTest项，也不是34个原生生产缺陷注入**，不能把托管指针清零当实际refs，不能声称证明任意异步关闭/全局引用/长期资源收敛。AF未重读全部cycles/events/resources或重算全部像素，沿用下文AE/AC审查的有限边界。

### 可独立派发的下一包

1. 通知FrameTime/同步销毁官方对照：不同及重复FrameTime、有效target同步、非零SyncModeStatus、注销/在途销毁与迟到通知必要偏序；不改阈值。
2. 非退化圆角合法实现及独立内容回归，保留零轴退化成功边界。
3. x86 Bitmap独立C# ABI/实际consumer夹具；AA图像成功不代替整个Bitmap合同。
4. 真实恢复条件包：先可重复lost→not-reset→同线程Reset→新色HW消费及释放；屏幕捕获WAIT_TIMEOUT/resource0另保留开放，需实际duplication帧。
5. 持续媒体与受控长期资源包：排除保留观测/序列化/GC/同进程顺序污染，检验CPU/GPU/线程/callback收敛及异步关闭；广泛随机亦未闭。
6. 最后源码全量基线：冻结代码后按两正式项目×双架构Release完整适用矩阵运行，绑定各现场SHA并回读TRX/业务/生命周期，保留真实失败。此前所有AD/AE均有界，整体不关门。

---

# AE：AA/AB 独立回读与 Z 检测有效性补齐（历史执行报告）

## 最终裁决与修改边界

本包有界检测完善完成；完整替换、最后源码四格 full 未完成。仅修改 `Tests/WpfGfxShape.ComAcceptance/ReplacementResourceCycleTests.cs` 与本报告及指定三份交接文档。未修改生产、通知、图像、捕获、ABI、入口、项目、AD 文件或 AA/AB 报告；未重发 NativeAOT，未清理他人。仅专用读取/写入/发布与沙箱直接执行既有正式 C# 测试 exe，无终端/shell/直接或间接脚本。亲读执行规则、实施方案、AA/AB/Z 报告与 AC 资源审查。

AE执行时AD尚未独立验收属于历史状态；AF已亲核AD最终包，布局修复及有界回归完成，详见本文AF节。AE COM资源证据仍只归属V归档，不覆盖AD最新候选。

## AA：亲读而非仅报告交接

根 `artifacts/replacement-x-image-aa-2026/readback/`：

- `unit-{x86,x64}-final/TestResults/aa-unit-<arch>-final.trx:30` 均 total/executed/passed=2，failed/notExecuted/timeout/aborted=0。
- `com-{x86,x64}-final/TestResults/aa-window-<arch>-final.trx:50` 均 total/executed/passed=4，failed/notExecuted/timeout/aborted=0。
- 两单元 `Results/preflight-identity.json:1–19` 亲读 .NET10.0.13、X86/4、X64/8；测试 SHA x86 `A895641FB4FED337008C348733EEA3F1CB99354AFED496EF230AF0EDEAD42F8E`、x64 `7ADEBF3562722B1923FA51233DB983872860F0434E9A91ED2B8B0792BCE70BBB`；实际托管产品均 `E1CB75CCF3D578ED2E4A693C967E7CA341E6AE77DEEAA44A2DA5DD624A36322A`。
- 图像业务根 x86 `com-x86-final/Results/replacement-state-window/b7c2dabac358431aa09123268791c631/`，x64 `com-x64-final/Results/replacement-state-window/2e6f6eccbb28400081e2a497406c5d01/`。两 `comparison.json:1` 均 frames12/threshold0/exact=true；两 candidate/loaded-before-show.json 亲读实际 Native 模块 x86 `1DF90D18D299F900D442EE71DE706FC489017B6265198290D4097B39F5782A71`、x64 `59D6020A8FCD1B1C7F621008EBB024475A5EF8A40D23536AFA440E9D0D9B24B9`。
- x86 candidate/events.json:8–90 亲读首周期红蓝24×16、resize40×24、绿黄更新及 close-drained，callbacks=callbacksAtUnsubscribe=9；x64 :205–245 亲读末周期绿黄40×24及相同关闭计数。已列出 x86 candidate 十二份原始 pbgra32 文件，未自行重算全部 BGRA 或文件 hash；精确比较结论引用归档测试 comparison，不称独立逐字节复验。官方加载 identity、其余事件全量和独立 GPU 回归1/1仍为 AA 报告交接，不冒充 AE 亲核。

有限 RTB 图像门成功，不证明屏幕 HW、旧 INVALIDCALL 首个 D3D 操作、长期 GPU/资源或真实恢复。托管与 NativeAOT SHA 不混同，不拼旧 full。

## AB：亲读实际非空通知与独立布局依据

根 `artifacts/replacement-present-ab-2026/readback/`：`final-{x64,x86}/TestResults/present-ab-<arch>.trx:36–40` 均 1执行/0通过/1失败、notExecuted/timeout/aborted0、退出2。

业务根 x64 `final-x64/Results/present-contract-y/2ec6158048434b25a7d02be9bbb7cefd/`，x86 `final-x86/Results/present-contract-y/2fa5958605824c68be2dc8631ba74b0a/`。四角色 identity.json 全部亲读实际 Loaded、runtime10.0.13、架构/指针、SHA；候选为 V 归档，见下表。官方版本 `10,0,1326,50504 @Commit:4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。

四角色 samples.json:1 的完整64字节 hex 已亲读，每个 compose 两条：官方前24字节 `0A00000000000000020000003C0000007B00000000000000`，候选 `0A000000020000003C0000007B0000000000000000000000`。两架构一致；compose-again/replacement-channel 都为空。此处读取的是保留原始包的无损 hex，不是断言摘要；compose-*.bin 已确认归档存在，但未额外逐字节解码 bin，因此不把它描述为 bin 独立复算。

亲读官方 `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/exports.cs:322–343`：Presented 相对结果@0/刷新@4/时间@8，Message Type@0/Reserved@4/Presented@8，得出绝对结果@8/刷新@12/时间@16。与实际官方包相符，候选提前4字节，是独立于候选类型的布局证明。亲读两官方 events.json:1：注册→close/commit/flush→WM0x400(123,456)，compose仪表 x64=2/x86=1，随后真实 peek 两消息、空 flush、注销/在途销毁/最终释放。候选仪表及发送0x06包细节、绝对QPC排除规则、Managed5/5来自 AB 报告，不升级为本次全量亲核。替代通道空不证明已销毁通道无迟到消息；FrameTime变化、QPC必要偏序仍未覆盖。

## 先审查可观测合同，再改检查器

原测试真实 factory/target/GetBitmap/CopyPixels 与四对象 vtable Release，消费 bitmap 持有独立引用；Release 返回值保存在 Cycle 中。`target=0` 等托管变量只防止 finally 再释放，**不是实际 refs 证据**。实际返回0仅限所持接口，不代表所有内部缓存/进程/GPU资源归零。

EventProxy 的 Calls/Disposals/Sum 是真实同步 native→managed 回调产生的计数，events 中 frame-consumed-callbacks 是 Raise 返回后的观测节点，descriptor-disposals 是 Release 返回后的采样节点，不是回调入口时间戳；能检验本有界序列与观测一致性，不能证明任意异步迟到 callback 不存在。

正式共享 Detect 现同时读取 Cycle[] 与真实 Event[]：16轮标识、224事件、每轮节点和值唯一性、创建→消费/更新→producer释放→保留读→consumer/factory释放，及 callback→proxy释放→处置采样必要边；不比较角色墙钟或强加无关事件全序。保留原逐像素独立红蓝 oracle、官方零阈值、Release/回调计数门。

新增同一测试内34项观测变异：四种 Release 分别改1；测量首轮14事件逐项遗漏和重复（28项）；移动真实 callback 节点到 proxy释放后、移动保留读到 producer释放前（2项）。全部基于实际候选运行观测副本，调用正式同一 Detect，无原生 double-free/释放后调用。旧 Disposals0/2 两项仍保留。34不是新增34个测试用例，也不是实际注入34个生产缺陷。

首轮回读曾发现四个 Release 变异被事件一致性先拒绝，不能证明目标分支；已调整检查顺序并添加精确 `remaining-reference` 断言，随后重新发布/运行。早期 readback 留存但不计最终。最终34项中四项 remaining-reference、28项 event-count、2项 event-order；旧处置负控仅断言拒绝，不声称各自命中某分支。

## 最终构建与实跑

独有 ignored 输出根 `artifacts/replacement-evidence-ae-2026/`；COM final-publish/final-package 均独有，AotDllPath 明确 V production/{x64,x86}/wpfgfx_cor3.dll；没有重发生产。两个正式项目 Release win-x64/win-x86 自包含发布成功，Managed 构建其生产依赖，不等于 NativeAOT 重发。一次 Managed 初试传全局 StartupObject 导致依赖库 CS2017，去掉该参数后沿用既有条件入口成功，无项目/入口修改。

|最终定向|执行/通过/失败|原始证据|
|---|---|---|
|COM x64|12/11/1，退出2|final-readback/x64/TestResults/ae-final-x64.trx:143|
|COM x86|12/11/1，退出2|final-readback/x86/TestResults/ae-final-x86.trx:141|
|Managed x64|8/8/0|final-readback/managed-x64/TestResults/ae-managed-x64.trx:66|
|Managed x86|8/8/0|final-readback/managed-x86/TestResults/ae-managed-x86.trx:66|

COM两份顶层Counters已亲读，error/timeout/aborted/notExecuted均0。Managed为身份+ImageUploadCallContractTests+WindowPresentNotificationTests定向，不是官方通知布局验收/full。最终COM唯一失败仍首设备cooperative=S_OK，secondCreate=0x88760868，不构成真实loss/恢复；未改 skip 或测试来追绿。

最终资源业务根 x64 `final-readback/x64/Results/replacement-resource-z/dc15a5421bd54956b2670e5a0a66d0c3/`，x86 `final-readback/x86/Results/replacement-resource-z/34ec618b22314f19a923453d72266950/`。两 identity/comparison/detection-controls 已完整亲读；comparison Failure=null/threshold0/16轮/48帧每角色/旧两负控true，detection-controls Count34/ObservationOnly=true且无null。每角色4预热12测量、8×8，真实模块同进程先官方后候选，不宣称角色进程隔离。

|架构|V候选 SHA256（AB/AE实际加载）|官方 SHA256（AB/AE实际加载）|
|---|---|---|
|x64|7B491B79976CB1E1C8C24792E067CFD8948F0D47F6C174933C6B99F230D331E3|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|x86|508C123706484CF830C2448BEE34F3624AF3A74E02B9E8920A766AA296638A6E|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|

SHA 是现场记录归属，不绑定 AA/AD 最新源码或旧 full。源码最后修改后 COM 双架构最终重新发布实跑；Managed最后确认见本包补充回读。无覆盖率声明。

## 未覆盖与下一步

AD最终原始hex/identity/TRX及窗口comparison已由AF独立回读，布局修复有界验收完成；通知算法、同步与销毁的完整合同仍需补证，不拿本次V归档结果裁决修复后未覆盖行为。实际共享 AddRef/QI 图、全局引用、长期内存/GPU趋势归因、异步在途关闭/回调入口事件、真实loss→not-reset→同线程Reset→新色HW消费、屏幕HW/duplication持续帧、广泛随机与最后源码四格full仍开放。保留帧/事件和序列化、GC及同进程顺序污染未消除，不能以有限 Release0 声称长期资源收敛。主代理只监督，本包检测工作完成不等于整体完成。
