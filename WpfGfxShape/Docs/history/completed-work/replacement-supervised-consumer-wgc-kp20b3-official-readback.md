# KP/20B3 唯一真实 official-only WGC/GDI 诊断回读

## 裁决与边界

**Failed / Blocked，已停；不是产品通过，也未证明 candidate 缺陷。** 2026-10-26 唯一沙箱任务 `55fb7bcd5e3142e6aaac0d19db6bb9be`，精确 official 测试请求1、真实体1，TRX 1执行/1失败/0通过，子进程 Exit2。首个独立红标尺合格 WGC 帧内容不匹配，随后两项 HWND 销毁检查失败；没有重试追绿。主代理只监督和回读证据，不探索/修改源码，不委派，无终端/脚本、build/publish、补件或包修改。用户本次授权覆盖旧“待授权”阶段限制，不扩大后续权限。

调用严格使用发布准备报告所列 clean sourceDirectory、根 WpfGfxShape.ComAcceptance.exe，参数依次 `--supervise-exit`、`Results/consumer-wgc-kp20b3-x86`、`120`、`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialConsumerTwoMarkersAreObservedByGdiAndWindowCapture`；外超时180秒，完整回收，无新增发现。

## 原始证据与亲读范围

完整回收根（仓库相对）`WpfGfxShape/artifacts/consumer-wgc-kp20b3-bound/official-readback/`，下称 R。任务远端根 `C:\CodingAgentSandbox\Tasks\55fb7bcd5e3142e6aaac0d19db6bb9be`，下称 T。

- `R/Results/aot-identity.json` 全1行。
- `R/Results/consumer-wgc-kp20b3-x86/supervision.json` 全40行、stdout全31行、stderr空。
- 同目录 `TestResults/child.trx`：结果、异常、定义/执行节点及尾部计数均亲读；首次长输出截断，后续补读尾部。异常为观察失败及两项清理失败组成的 AggregateException。
- 同目录 `consumer-wgc/evidence.jsonl` 全26阶段行分段读取；首段长输出截断，后续11–26逐段补齐。帧数据嵌在 texture/frame persisted/observation 中，没有另一个成功帧 JSON。
- 同目录 `consumer-wgc/ruler-0.bgra` 为保留的失败原件，直接读取返回 NUL 字节。此文本读取不是独立二进制长度/hash/全像素统计认证；中心及 alpha 结论来自实际帧 JSON，不冒称已用另一解码器独立认证。
- 同目录 `Diagnostic/log_261026141826116.diag` 定向读 runtime/OS、节点UID、异常/源码位置等；并非全文逐行重审。没有读取源码，诊断日志中源码路径仅为测试节点属性。
- R 根 runtimeconfig 全21行、deps 的官方本机资产/投影段亲读；既有发布链来源见 kp20b3-publish-readiness，历史 revision-validation/规则/当前事项/基线/章程先读。没有构建或重新认证全部包文件。

## 实际初始化与测试身份

`Results/aot-identity.json` 实际指向 `T/Native/wpfgfx_cor3.dll`，实读 SHA256 **20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D**，与授权期望一致；sandbox=true、ProcessArchitecture=X86、PointerSize=4。初始化后 identity/geometry/capture/observation 阶段及测试方法内部栈证明**真实体进入**，不是只凭 TRX executed=1 推断。

候选 PE 结论边界：既有发布准备报告说明 AssemblyInitialize/ValidateIdentity 会用 PEReader 验无 CLR/I386，历史 KP/KS 为相同 SHA 的 I386/无 CorHeader；本次初始化成功越过既有身份门，没有 PE 校验异常。但本次 identity JSON **未显式序列化 Machine/CorHeader**，监督代理未重新读源码或独立展开 PE，因此不将 X86 进程字段冒充新增独立候选 PE 原始字段认证；本次 PE 门成功为既有检查链的间接证据，若要求独立 PE 原始证据仍缺。

实际 test SHA256 **DFB88CAC41B073F87171E4259382F3E82853F37EEA97C66B67436F9BDD15C9AB**；MVID **d940a3e4-b046-4c15-b918-8f95269d3d93**；runtime **.NET 10.0.13**；X86/pointer4。监督与体内 identity 一致。MTP diag49 实测节点 UID **1695dafe-7676-88fa-8285-193a9e6ab9e3**，TRX testId相同，executionId **746fa56a-f0ae-4817-ab87-f8b0bbfd71eb**；不是借用 KS UID。OS 日志 Windows NT10.0.26200.0，64位系统，不等于测试为x64。

监督 14:18:25.97375Z–14:18:27.4922247Z，reason=exited、TimedOut=false、Cancelled=false、KillAttempted=false、HasExited=true、ExitCode=2、OutputCollected=true，输出排空未超时、SupervisorError=null。不是崩溃/超时/零命中，工具“执行完成”也不代表测试成功。

## 官方加载身份及可信来源边界

体内 identity 记录 Requested 与 Actual 均为 **T 根 wpfgfx_cor3.dll**，不是 Native 子目录；两字符串本次实际一致。实际官方 SHA256 **5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834**；ProductVersion **10.0.13-servicing.26504.104 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699**。既有代码没有 Requested==Actual 断言，本次只是回读相等，不能说新增了断言。

来源链：既有发布准备报告中的正式清单/RuntimePackAsset clean复制链指向 WindowsDesktop10.0.13 官方资产；本次回收 runtimeconfig 的 NETCore/WindowsDesktop includedFrameworks 均10.0.13，deps 官方根 wpfgfx fileVersion10.0.1326.50504，与实际产品版本属同补丁构建系列。实际加载路径在同任务 clean 根，实际 runtime/test身份相符。这是**正式发布链+实际模块路径/hash/版本**关联，不是只看根DLL存在。但没有独立受信 runtime pack 原文件 hash 比对、官方 PE 原始字段或实际 PresentationCore 模块路径/hash记录；deps/版本声明不补这些缺项，不宣称官方字节来源及配套模块已独立完全认证。

CandidateLoadedByDiagnostic=false 是既有常量，不能证明全进程/全时段未加载 candidate；本次没有加载事件负证据。方法为 official-only，NativeOnlyPresentCount=0、ProducerReadback=false；不据此虚构全进程模块排除证明。

## 分层观察：不能拼接未发生的 consumer 内容

1. **系统支持/访问/投影/COM**：没有访问拒绝、系统不支持、投影缺依赖或 COM 初始化异常原件；实际两个 HWND 的 CreateForWindow capture bind 成功，FeatureLevel45056、32×16、2缓冲，随后取得真实纹理帧。这证明本次越过捕获绑定并可得帧，不等价于穷尽所有访问/系统API状态。不能把内容失败改写为“WGC不能初始化”。memory negative controls 的全部true仅内存判定器自测，RealCaptureEvidence=false，不算实际帧。
2. **窗口与几何**：consumer/ruler 均32×16、DPI96、Session1，分别位于(40,40)/(140,40)，crop(0,0,32,16)。绑定及drain完成，FrameArrived/Closed均未订阅，Owner记录 synchronous STA；不是代码调查结论，不重开已失败线程方向。
3. **独立 GDI 标尺第一红标记**：brush/DC准备完成，submission index0 Color4294901760(0xFFFF0000)，StartQpc38453781481681，FlushQpc38453782021278，Deadline38453881481681。
4. **首合格 WGC 帧**：只锁定首个 eligible，不寻找后续正确帧。Time100ns38453782031250 > ThresholdQpc38453782021278（Frequency10000000，差9972，即约0.9972ms）；这满足记录的时间门，不证明画面已反映提交。纹理32×16、format87/BGRA8、SampleCount1/Quality0、crop32×16、RowPitch128。帧落盘 ruler-0.bgra；中心B/G/R=0/0/0，AlphaMinimum=AlphaMaximum=0，WgcMatch=false。
5. **同一提交稳定阶段的 DC**：Window=255、Screen=255，ExpectedDc=255（COLORREF红）；WindowQpc38453782279674、ScreenQpc38453782282203，Covering等于ruler HWND42599538，WindowMatch/ScreenMatch均true。它们晚于帧时间，不是同一扫描帧，SamePhysicalScanFrame=false。仅有这组提交后DC结果，没有本轮可引用的提交前颜色/前后颜色转变证据；不得补写。WGC不是自动真值，DC也不自动认证目标Present帧。
6. **停止点**：classification=first-eligible-content-mismatch、NoRetry=true。第二标尺蓝标记未发生；official consumer 的真正创建/资源/channel提交及两标记内容**not observed**（仅窗口/capture存在不能当consumer资源创建）。consumer实际WGC帧、其GDI颜色、新鲜性/内容关联、两观察闭环均not observed，不可用标尺阶段替代。
7. **清理独立失败**：stopped完成；两个capture close各Errors0、两个dispose完成；consumer destroy及ruler destroy各WgcCheck InvalidOperationException，HWND owner join完成；cleanup complete Errors2/Clean=false。没有断言具体Win32错误码、没有证明泄漏归零，不能把join或进程退出替代资源清理成功。

最窄原因：**独立红标尺已提交且后续GDI两DC均红，但按当前时间/几何门锁定的首WGC帧为零色/零alpha，内容闭环失败；并伴随两个HWND销毁检查失败。** 不能从本次确定是捕获内容时序、窗口内容路径、读回实现或其他具体根因；不笼统归环境，不归因candidate，不把WGC当真值，也不由标尺失败推断official consumer像素失败。

## 预算、SDK与下一唯一门

本次增量：沙箱1/1、监督精确请求1/1、真实体1/至多1；新增发现0、独立build0、COM publish0、Native发布0、native-only0、candidate运行0。既定consumer两标记实际0/2，独立标尺实际1/2，失败即停未补齐。

WGC累计：build **3（1败2成）**、COM publish **3（1败2成）**、沙箱 **2（此前发现1+本次真实1）**、单独发现 **1**、真实体 **此前0→1（失败）**、本链Native发布0。保留revision-validation中的编译/restore/发布历史失败及C427输入漂移记录，不清零。KS另列不变：build1、COM publish1、沙箱4、精确请求3、发现1、真正体1、A1/B1/native提交2。

SDK建议：**临时保留但冻结**现有默认关闭的ConsumerWgcDiagnostic SDK/TFM/框架版本元数据隔离变更及现有包，供本次失败追溯，不提升为正式业务依赖或产品能力。不因内容失败擅自回撤已解决投影解析冲突的配置；若用户决定终止诊断路线，另授权只回撤诊断专属配置/文件并保留原证据，本次不实施回撤。

下一唯一门：**仅请求另授权，对现有失败原件所限定的“独立标尺首帧内容关联与HWND销毁检查”作有界静态根因审查，先定位具体判定/所有权证据缺口再决定最小修订和新预算。** 本轮不继续代码探索、不修码、不执行，不自动重开线程方向、不扩原shader/resize/x64/full，也不追加native-only或candidate运行。官方受信资产独立hash/PE及PresentationCore配套仍是披露的身份缺口，不能在后续比较中忽略。整体替换未完成。
