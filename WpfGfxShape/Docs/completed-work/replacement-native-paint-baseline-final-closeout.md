# NativePaintBaseline 最后既定 A/B：A错B错，方向结束

## 终态与授权账

本次亲自执行，无再委派、终端或脚本；仅实际发布和Windows沙箱工具。源码/项目修改0、独立normal build0、Native publish0、独立测试发现0；COM publish1/1成功、Windows沙箱1/1完成、测试体1/1失败，无重试。读取文档不代运行，诊断失败不冒产品Passed，完整替换仍Open、根因未定，不断言缺权限或要求外部维护者。

|阶段|normal build|COM publish|沙箱|测试体|
|---|---:|---:|---:|---:|
|r1历史序列化失败|1|1|1|1（A/B未知）|
|r2历史CS1686编译失败|1失败|0|0|0|
|r3历史修正编译成功|1成功（139 warnings/0 errors）|0|0|0|
|本次final|0|1成功|1|1失败|
|上述累计|3|2|2|2|

87d673efe07345f9b0133d2c3287e0f9独立build1/publish1/沙箱1/体1及其原件不并账。r1/r2/r3原报告replacement-native-paint-baseline-r1-r2-closeout.md全部保留，不覆盖历史失败。没有额外official/x64/full/原shader/resize运行。框架在这一次filtered execution内部记录Found1823，是正常执行内部发现，不是另一次测试发现调用。

## 发布与核包

实际参数与完整直接可见返回立即持久化在 artifacts/native-paint-baseline-x86-final/01-publish-tool-return.md；核包02-preflight.md；沙箱实际参数与完整直接可见返回03-sandbox-tool-return.md。唯一dotnet publish当前项目，Configuration=NativePaintBaselineDiagnostic，win-x86/net10.0-windows，self-contained=true，ComAcceptanceSandbox=true，AotRuntimeIdentifier=win-x86，显式AotDllPath=postfailure-source-rebuild-x86-r1/native/wpfgfx_cor3.dll，新final/publish与final/clean，未 --no-build。固定r1 obj/bin是既有配置，不改项目。日志35–39显示当前诊断partial144/145/160/196/367行编译警告，58为当前DLL、59为新final publish；不是直接拿旧不可运行clean。

publish exit0；最后发布日志60行均分段亲读，完整可见原返回保留工具会话。stdout原55328字符只显示尾20000，前35328缺失，不能称完整原stdout或补造warning总数；没有独立stderr段不证明原始stderr为空。专属工具不支持直接保存最后日志文件，本地即时记录保留准确返回/边界，不冒已持久化不可见输出。

读PackageSelfContainedComAcceptance42–71：从本次PublishDir核依赖后复制clean，计算published Native SHA。clean aot.sha256=15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620。runtimeconfig完整21行NETCore/Desktop均10.0.13；deps前22行win-x86、两个runtimepack10.0.13；根exe/dll/deps/runtimeconfig存在。读取新clean DLL公开API确认精确入口；当前partial370–424有合成完整报告自检，当前编译警告和本次运行selfcheck共同确认新代码生效。文件认证不冒产品加载。

## 唯一沙箱、身份与退出

clean根WpfGfxShape.ComAcceptance.exe，参数 --supervise-exit Results/native-paint-baseline-final-x86 120 FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenPaintScopedNativePresentIsComparedThenOriginalClientPixelIsRequired；外180，outputRelativePath留空整任务回收。原件根 artifacts/native-paint-baseline-x86-final/readback；任务8803fd5b02f84af882ec19bc8c30fa41。监督StartedUtc2026-10-28T05:28:49.1712719Z、EndedUtc05:28:51.6772322Z，SupervisorPid10536、child4972。OS exit2/0x00000002，TimedOut=false、Cancelled=false、KillAttempted=false、HasExited=true，输出回收完成且无drain timeout，SupervisorError=null。直接工具只返回完成和回收路径，不冒直接返回exit。

监督/最终JSON/selfcheck一致：testSHA947B0DBC42B3A578DCA1D8774E8789C8EC4DCFE3410FA9F129BFA459512C1977，MVID f21a1c7b-1dc5-49cd-942d-d653eaf50e2e，.NET10.0.13/X86/pointer4/QPC频率10000000。aot-identity和candidateFile同15A545完整SHA；productExportsInvoked=false。因此actualNative产品加载=NotObserved（只初始化候选文件身份），不是候选产品运行认证。

TRX48行完整亲读：total1/executed1/failed1/passed0/error0/timeout0；UID12f862af-8b14-876d-baef-f49ad240c880、execution bafb2825-8e6c-48fb-8e1b-d497f7eb15c4，失败为最终 a.Target && b.Target && a.BaselineKnown && b.BaselineKnown 断言，不是序列化崩溃。diag session489dd395-caf5-4415-b528-84749bd794ad最终exit2。stdout中文显示编码损坏但身份/断言/1失败清楚，stderr读为空。按名称搜索曾漏报child.trx，随后直接列TestResults找到并读全，不能登记TRX丢失。

## 四分类与独立有效性

业务最终classification=AWrongBWrong；a/b Pixel均15987699（00F3F3F3），expected00563412、每通道阈值1，Target=false。A颜色错后继续B，实际各Clear1/Present1、各HRESULT0，总Clear2/Present2，没有等绿、重发或新增采样。

- A错B对：本次不命中；即使命中也只支持有限事务敏感性，不改变原shader前置。
- A错B错：本次命中，否定既定WM_PAINT内呈现形式足以恢复此观察的假设，停止paint方向。
- A对B对：不命中；目标命中仍因无预提交基线不能单凭此认证恢复。
- A对B错：不命中，不提出paint修正。

独立于颜色的API/参数/cleanup：两角色primary=null、callbackFailure=null、cleanup=[]，chain/query cleanup errors=[]，Overflow/Reentered=false；所有已记录HRESULT0。请求format0，actual两侧before/after均22（UNKNOWN被解析，不是擅改格式）；32×16/count1/DISCARD1/windowed1/adapter0/HAL1/behavior38，Focus/Window均各自HWND，owner5116/pid4972、DPI96/context6010、client与采样(16,8)/screen(56,48)前后不漂移。单DC无法证明预提交非目标色，BaselineKnown=false，scanoutOrFrameIdentity=false；这些是证据能力边界，不拿来掩盖已见两错。

## 实际事件、时序与清理

A HWND34012E：createdQpc39864011044694；初始WM_PAINT QPC11055940（同前缀398640，以下保留原件完整值为准）Update1/Validate1，Clear39864011250189、Present39864011251211、Returned39864011256525；Entered=false，提交不在paint回调。之后Query.GetData21轮最终0（完成等待不是按色循环）、两次DwmFlush0；销毁事件WM_WINDOWPOSCHANGED71、WM_DESTROY2、WM_NCDESTROY130。总4个受记录事件，仅1次初始paint，不等于4次呈现。

B HWND35012E：created39864019078131；初始paint39864019078977，真正armed paint39864019167271，二者Update1/Validate1；Clear39864019167493、Present39864019167826、Returned39864019179572，Entered=true/Update1/Validate1，真实非空更新区内单次提交后ValidateRect。Query.GetData2轮最终0、两次DwmFlush0；后续71/2/130，共5个记录事件、2次paint但仅一次提交。两侧事件owner5116、depth1，无重入/溢出；不是把paint事件数当Present数。

GetDC→GetPixel(16,8)→ReleaseDC代码紧邻，没有中间消息泵/额外采样/写盘；每侧release=1。但没有三API独立QPC，不能宣称GetDC到GetPixel实测零延迟。A dcBefore39864016258572/dcAfter39864018763836，合段250.5264ms；B39864023811378→39864026380732，256.9354ms。该段包含API本身耗时，不能定位到某一个API或隐去约250ms；完成后先after invariant再采样，不冒Present后立即无间隔采样。采样次数各1。

真实Release：每侧before/after各一次chain.Release均refs0；每侧query/device/factory各一次Release均refs0，无重试。采样ReleaseDC后device→factory→DestroyWindow；A DestroyWindow返回1/QPC39864018874778，B返回1/39864026695154，均owner5116，WM_DESTROY/NCDESTROY真实记录，强引用保留到销毁。仅认证这些已记录资源，不外推产品资源或所有进程线程。

## 亲读覆盖与终止建议

最终JSON1–662全读；A partial1–308、B1–320各分三段全读；serialization-selfcheck1–109全读（Synthetic.NotExecuted不是额外测试体，裸指针已DTO，完整报告序列化落盘成功）；supervision1–40、identity1、stages1、stdout1–27、空stderr、TRX1–48全读。diag1–89全读，初次1–40截断在29，随后25–38/39–52/53–66/67–80/81–89补全。没有recording-error文件记录，最终primary=null并进入classification stage，记录错误与业务失败分开。全部原件在readback，不改写。

当前方向结束：不建议owner/override/格式/DPI/capture变体，不再另静态接力，也不追加运行。主要未证能力是候选产品实际加载后的原内容消费、原shader、真实resize/device-loss恢复及最终双架构适用矩阵；本次只是普通D3D9诊断，未覆盖原产品代码/原测试，不能以诊断设施恢复冒产品通过。

唯一收敛主线建议：将此诊断方向关闭，后续工作直接回归原产品原用例的真实HW内容与resize验收，只有取得可验证的产品修复依据后才另行授权原矩阵验证；当前不已有修复、不再用观察变体换名续跑，也不转推外部维护者。全部最终结果归档后停止。
