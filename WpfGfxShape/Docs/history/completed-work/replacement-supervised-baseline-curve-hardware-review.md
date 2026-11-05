# BH 独立监督审查：baseline / curve / hardware

## 裁决与边界

**完整替换未完成。本轮无生产修复，仅审查和文档收口，不构建、发布或运行测试，更未重跑 full。** 主代理仅监督；BH 只使用专用读取、搜索、目录和文档写入工具，无终端、直接或间接脚本。工作根为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`，下文路径均相对此根。

先读当前执行规则、完整替换方案全文，再分段读 BD baseline、BF baseline-review、BE curve-device-quantization、BG hardware-present-investigation 四报告；截断末段另补读。未改四报告，保留作者所有权。本包仅新增本审查、更新入口/汇总/问题状态的最新优先节，历史正文保留。代码仅只读 WindowQ、曲线实验/比较器和原生呈现完成切片；不是全面代码审计，也未重新计算所有二进制像素/hash。

BD–BG 轮次无生产修复；源码变更范围为曲线实验挂接/新增实验文件及 WindowQ 夹具。本轮没有取得完整 git dirty 清单，不把报告所述范围冒充独立版本控制差异清单。归档曲线候选、BD 候选、新测试程序集是不同快照；ProductVersion 提交字符串不证明 dirty 源码一致，不能拼成最终矩阵。

## 独立亲核的数字与原始路径

|对象|亲核原始证据与裁决|
|---|---|
|BD x64 full|`artifacts/bd-comacceptance-baseline/full-x64/TestResults/bd-full-x64.trx:13613–13621`：1740 执行、1699 通过、41 失败，Failed、退出 2，timeout/aborted/notExecuted 均 0。没有新 full40 失败或全绿结果。|
|BD x86 full|亲列 `full-x86/TestResults`，仅部署目录及继承的 `bd-smoke-x86.trx`，无父 `bd-full-x86.trx`。亲读 `diagnose-x86/Diagnostic/log_261020010009863.diag:35–51`，发现1740、开始官方 sampler 后结束；最终计数、退出码及中断原因未知，不由末用例推断访问冲突/timeout/候选崩溃。|
|BF WindowQ|`artifacts/bf-comacceptance-review/windowq-{x86,x64}/TestResults/bf-windowq-{x86,x64}.trx:83–86` 均 Completed、11执行/11通过/0失败，其余0。仅定向，不从 BD 41 中减一。父数值退出码未在这些片段给出，不造0。|
|BE separator-v4|`artifacts/be-curve-device-quantization/separator-v4/run-{x86,x64}/be-parent/parent.trx` 两份全文亲读，分别39/41行：各1执行/0通过/1失败、退出2，失败为 parent exact。TestRun x86=94ee48e7-6655-43b7-b6d0-9636a5a8ecd4；x64=565b703c-2a72-4be7-8dfe-8d4ef66d2985。|
|BE 原差|两份业务 `comparison.json` 全文亲读：frame1/3/5各44字节，frame9为10，其余0；threshold0、exact=false、hardwareVerified=false。首差696/B36→40及2432/B171→179。子角色生成完成不是父验收通过。|
|BG 三次定向|`artifacts/bg-hardware-present/probe-x64/TestResults/bg-hardware-x64.trx:307–315` 为11/0/11；probe-x86的bg-hardware-x86.trx:203–211为6/1/5；resize-x64的bg-resize-x64.trx:109–117为3/1/2。三者退出2、timeout/aborted/notExecuted均0；通过项不证明硬件内容成功。|

BF 仅 WindowQ 夹具根因可确认：亲读其源码，按帧0/1=24×16红/蓝，2/3=40×24绿/黄验证完整 BGRA，包括 alpha，官方 exact 保留；4正例+5负控，不再固定24×16且只接受红蓝。亲读 x64 `windowq-x64/audit-t/q-raw-frames.json:1–50`，1536/3840字节、绿黄exact及hash一致；不是本轮重新读取全部帧。该文件在任务根 audit-t，不在 Results/audit-t（最初该路径读取失败，目录搜索纠正）；报告相对路径本身无冲突。RTB软件消费不关闭硬件合同。

## BE 续轮独立核对与未闭分支

最新 BE 报告已出现“续轮实做 separator-v4”优先节，非仍停留旧稿。BH 已从原始证据亲核，而非只接受口头“已更新”；旧节“下一最小实验”不能覆盖续轮状态。

v4 x64 task=`3cea3b588a674696980033f9a7ccda0b`，业务根 `Results/curve-an/606269f7f75049eebbaab712cfe210ce`；x86 task=`940ff07be0b142648f81f97a4cd2ad58`，业务根 `Results/curve-an/982c21e1286a4054ad2c345a61dec949`。以下均在各 `separator-v4/run-{arch}` 下。

两架构 `separator-summary.jsonl:1–8,65–72` 均亲读：ab-negative-0 的官方 A/B fill 差18，真实 geometry/drawing/visual 对A差18、对B差0；候选真实差64、line差12。cd-positive-0 nested fill 官方C/D差8，真实对C差8、对D差0；候选真实差38、line差28。stroke不能据此选择同一fill算法：nested stroke对C218、对D224。不是96行全审，也不是全部原始图像人工逐像素重算。

亲读实验 RenderSeparators:85–180 及 Compare:213–246：真实 geometry.Transform、drawing.PushTransform、visual.Transform、nested geometry local+parent outer 确有挂接；显式模型与真实官方缓冲比较，不只是整数命名。A为double local.Transform后栅格量化，B为原点经float栅格矩阵，C为double合成矩阵后float，D为local float结果再outer。Compare 的 official 字段是各模型对真实官方曲线的差；line另对官方line，不能误读为line与cubic相等。

有限样本支持B优于A、D优于C，但仍未排除 E=先本地float变换并保存MilPoint2F、再identity栅格，以及两个float矩阵先合成等替代分支。C/D负桶未找到，不能宣称负半格全覆盖。BH未逐段亲读控制点IEEE大JSON；报告中-23.500004/-23.5位型细节不冒充BH独立核验。普通cubic/rounded上游尚未全审，InitBufferWithRoundedRectanglePoints、CRectangle.Set算术及候选全部调用点未闭；完整HFD、GPU圆角仍开。没有理由现在实施生产修复或向共享stroke加量化。

## 实际身份与快照隔离

BE v4 两架构 original/candidate 的四份 identity.json 及两份 aot-identity.json 均全文亲读，Requested=实际path；官方位于上述task根/wpfgfx_cor3.dll，候选在Native。运行时.NET10.0.13，X86/4、X64/8；官方版本10,0,1326,50504，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。均明确RTB软件控制、hardwareVerified=false。aot Arguments为candidate子角色，不当父filter证据。

|实际模块 SHA256|x86|x64|
|---|---|---|
|官方（BE/BG亲核）|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|BE归档候选（v4角色亲核）|0FCA4287C3E44CB7A7B0801D297C6D4E85F4AD84402D8B9580D0725E8449741B|C8C35CE4872FA659B02F8731F34F1F0492775D30879DEC17E54779F06228BE01|
|BD候选（BG角色亲核）|E0D3F5EFEDFACFD5CC3491DD43B194E19028E8B541A462123C8F73C73D6E25B5|1C381539E7C82DDD75908C0563F0AC062E583166BDB23AEE066BDD21E838752F|
|BD/BG测试（BG角色亲核）|84190DE3614F5983BAB69CE9CE0F9FB031EC5AB1964D55482461D442E31FC31F|D64A04DE7F85C63176B16C18AAFC4B470FB2D4E8BD573B58BB939A42237B6FE4|
|BF新测试（startup亲核）|198801A86173E60291EEBAB2B9B10CF6F1A8D312CFA1875586172A05D9392FD4|E49F32A612B2E58B3E264C0996A6D7748EAC9FDC1C4FBC9F50877CDC13B41121|

BF双架构candidate/startup-identity.json全文亲读，位于BF报告列出的rectangle业务根；其requested候选与BD相同，但startup不是实际loaded的替代，BH未全文亲核BF各report loaded/resolved段。BG四份 hw-module JSON（resize-x64两个、probe-x86两个）全文亲读Requested=Actual、上述SHA及测试SHA。BD x64 `full-x64/Results/az-reference-consumption.json:1–31` 亲读候选/官方/测试、MVID ce5bf492-6283-4571-8ec8-563c6f1564c6、PresentationCore SHA 0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31；其余BD表格身份不全冒称重新亲核。

**BE v4 最新测试程序集 SHA/MVID仍未取得可绑定记录。** 已搜索两v4任务Results及v4身份文件；只有native身份，无测试SHA/MVID。扩大搜索只见DLL符号名，不是身份值；未把二进制文本命中当哈希。旧compact/identity轮099C…/0CC6…不移绑v4，BF新SHA也不移绑BE。该缺口是证据不完整，不能仅靠同名DLL、版本或发布成功补齐；后续应取得对应v4实际程序集身份，仍无法绑定就明确未核。

## BG 原始呈现链与反证

亲读 probe-x64父TRX:171–190、probe-x86:100–125及源码 PresentContract:25–65、OutputWindow:49–70：staging从FF123456更新FFFF0000，Present断言成功，经EVENT query Issue/GetData及两次DwmFlush后记录completed/backbuffer仍红；随即duplication 887A0027 WAIT_TIMEOUT、resource0。backend为Microsoft Basic Render Driver、vendor1414/device008C、LUID00000000:00004E71；不是物理GPU已验。初始mouse-only帧lastPresent0不算当前提交。固定诊断expected=FF123456不代表本次红色断言预期；源码使用color变量。

本轮亲核上述原始切片，不声称四条swap/架构日志全篇亲读；BG关于全部首红帧失败、蓝及后红未达的结论沿用其报告。未取得稳定red/blue/red fresh桌面内容、业务resize或loss恢复。共同前置阻塞未定位具体驱动/夹具，更不能证明BD37项全属环境。反证亲读BD父TRX:2215–2221：官方PublicHwnd Passed且00563412实际匹配；BG同轮共同失败不抹除这个历史不对称，也不能跨任务借绿。

## 下一最小执行序列及未完成合同

1. 先补BE v4测试SHA/MVID绑定，排除明确的本地float保存/identity栅格及float矩阵合成替代分支；逐段审普通cubic、rounded上游生成算术，分离line差与细分差。证据充分才做fill限定最小生产修复，不动共享stroke；保留原44/10、半格、真实变换栈及兄弟回归零阈值证据。
2. 硬件先在稳定可呈现交互桌面同task/adapter建立原生red/blue/red fresh内容门，DISCARD/COPY逐提交验证lastPresent与新色，再同条件官方/候选对照。只有稳定官方成功候选失败才定位backbuffer→Present→桌面消费；resize实际新尺寸新色，loss需第一设备真实lost→Reset→新色硬件消费。
3. x86 full中断只做有新信息的有界阶段/异常诊断；先排除呈现前置，不能无限full重跑或按最后测试猜根因。合法大shader差异仍未闭，不能随WindowQ通过消失。
4. 最后稳定集成源码正常依赖图完成 `Tests/ComAcceptance × x86/x64 Release` 完整适用四格full，回读parent TRX、实际模块/程序集身份及内容/时序/资源/恢复；当前仍缺，BC历史托管绿与本轮归档/定向不得拼接。

Capture WAIT_TIMEOUT/resource0、真实HW/物理GPU、loss-reset新色、持续媒体、长期CPU/GPU/线程/callback/异步关闭、GPU圆角、通知有效target同步/非零SyncModeStatus/迟到消息等未闭合同全部保留。原报告数字与本次亲核切片未见统计矛盾；发现的是BE已更新与旧“下一实验”叙述的时序差、最新程序集身份缺失，以及不得混同的BD历史官方成功/BG后续共同失败。原始证据优先。本包文档完成不等于实现或完整替换完成。
