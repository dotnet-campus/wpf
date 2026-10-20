# AO 独立审查：Bitmap / 曲线及最终监督交接

## 结论与权限边界

**四份独占文档收尾，完整替换未完成；最后集成源码两正式项目×x86/x64 Release完整基线尚未具备。** 主代理仅监督。AO仅使用专用读取、搜索与文档写入，未改代码/项目，未构建、发布、测试或运行，未使用终端、shell或任何直接间接脚本。已读 `Docs/handoffs/current-execution-rules.md` 与 `Docs/wpfgfx-complete-replacement-validation-plan.md`，不恢复历史治理门。

本报告补齐Bitmap及圆角既有事实的独立审查章节；`replacement-supervised-bitmap-layout-fix.md`（AJ/AM）与 `replacement-supervised-rounded-rectangle.md`（AK/AN）已有实际完成章节，AO只读引用，未争用或覆盖。AM夹具修复报告完成但生产自产未闭；AN有界实验报告完成但无生产修复，不能仍笼统写“两人正在修、结果未知”，也不能写成全部通过。

路径约定：下文 `artifacts/` 均相对于项目根 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`。行号为文件1起始行号。SHA只用于结果归属，未重算，不是修复/集成/全量准备的前置门。

## 1. AJ 亲核：独立Bitmap成功不等于自产闭环

证据根 `artifacts/aj-bitmap-layout-20260721/`：

|证据|AO直接回读事实|界限|
|---|---|---|
|`post-readback-x86/TestResults/post-x86.trx:2709-2715`|191执行/160通过/31失败，error/timeout/aborted/notExecuted均0，退出2|不是4项独立测试全包，也不是全仓基线|
|`post-readback-x64/TestResults/post-x64.trx:2335-2338`|191执行/191通过，失败及未执行0|只属该族|
|x86同TRX DrawRealizedPath消息（17、30等行）|失败断言实际-2147467263，测试名为 `WhenPrivateBitmapBrushIsRealizedThenMappingClipAndOpacityReachPixels`|未独立逐项重计31条，全部同族的分类另由AM报告提供|
|`producer-readback-x64/TestResults/producer-x64.trx:1084-1090`|128执行/103通过/25失败，退出2，计数未执行0|自产路径并非全部缺reference|
|同producer TRX:306-307|DrawingImageVisualMask(17,0)抛DirectoryNotFoundException，缺 `ReferenceData/original-wpf.json`|另两条缺reference和16+6 present分类见AM报告，不冒称AO逐项穷举|
|`producer-readback-x86/Results/aot-identity.json:1`|实际X86/pointer4，任务1f1a195cff7b4ba1b53a34db178278b4，过滤含image-mask/visual-mask/rotated三族，SHA609893…|启动身份不是完成/崩溃栈证据；AJ GetState/Ensure崩溃来自交接，AO未找到并亲读AJ原始崩溃输出，不能写成亲核|

### 八份业务JSON与实际身份（全文各1-45行亲读）

均在上述根 `post-readback-<arch>/Results/`；文件名前缀均为 `independent-bitmap-realizer-`：

|架构|官方非平移 / 平移|候选非平移 / 平移|
|---|---|---|
|x86|`False-17e7f76c420f4c3e8ca096c666b8a5e0.json` / `False-c0314204703b45339ab85f547fbbc977.json`|`True-784634e83dec4cfd8b9c4a3935e39d40.json` / `True-a2ab5545e44b405ebfc7b65bd1e8c27b.json`|
|x64|`False-ff1c83619e9348f7a2da8899d671b971.json` / `False-55dade0653d44e3e8b3d1d58931b0680.json`|`True-c77d7c3ed0d74867b86c84656f0c181e.json` / `True-e8675543b13542a0844ffd3ca40f1fb6.json`|

八份均16次HRESULT=0，Ensured/Brushes/Effects各16，ContextMatches=true；双方非平移BGRA `[0,0,255,255,0,0,255,255]`，平移 `[0,0,0,0,0,0,255,255]`。matrix/texture相对CMILBrush次级基址为x86 48/164、x64 80/208。实际.NET10.0.13、pointer4/8及模块完整路径均在业务JSON；官方版本10,0,1326,50504，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。

- AJ候选x86 SHA `609893C352E789BD672217DE62F6DD4C984470D6E6FADC730462F0833E4A4673`；x64 `6514FF2F76B36237BE1EA46C968C39DB8B9ECAEEB03E5A454CB49C9C4C518BD9`。
- 官方x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`；x64 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`。

“修后独立各4/4”与八份业务一致，独立4项计数沿用AJ/AM报告；AO未亲读单独4项顶层TRX，不把业务JSON当独立TRX计数。不是仅候选自产预期：官方独立角色真实非空及平移差分存在。

## 2. 源码审查：producer / consumer同步，仍需防候选自洽

直接读取：

- `Code/WpfGfxShape/Abi/NativeBitmapBrushLayout.cs:6-44`：Sequential，包含ResourceCache、Matrix4x4、Parallelogram及两个int BOOL，无固定x64 Explicit偏移。
- `NativeImmediateSolidRealization.cs:21-80`：自产BitmapBrush以前述Native结构为前缀，私有ExactMapping/clip等在尾部；CreateBitmap写Native.Mapping/Clip/HasSourceClip/Source/Wrap/Opacity。
- `BitmapRenderTargetExports.Drawing.cs:309-348`：FillInternalBitmapBrush读同一Native类型的Space/Mapping/Source/Wrap/Opacity/Border/Clip；优先TryGetExactMapping/ExactSourceClip。

因此**这条自产bitmap前缀与consumer自然布局已经源码同步**，不是consumer修了而producer仍固定旧偏移。独立官方行为支持48/164与80/208，不能仅凭共用结构/私有精度尾部证明所有自产路径兼容。TryGetExact*只对自家对象可用，自产对自产可能掩盖本机布局/回调风险，仍须保留独立官方consumer/producer及真实WPF组合验证。

当前兄弟夹具 `Tests/WpfGfxShape.ComAcceptance/BitmapRenderTargetTests.BitmapRealizer.cs:100-134`，仅调用候选消费手造brush，现已按架构写matrix48/80、points120/160、useClip156/200、texture164/208。它自身不是官方对照；旧31失败不能仅凭候选端改后Green认定产品兼容。独立 `IndependentBitmapRealizerAbiTests.cs:22-60` 有测试侧Sequential声明，不引用生产Native类型；八份官方实际像素为其额外行为锚点。但独立两种输入不能覆盖31种clip/opacity/mapping组合，也不能替代自产resource realizer。

### AM最终报告已到达（来源明确，不覆盖原文）

来源：[AJ/AM Bitmap报告](replacement-supervised-bitmap-layout-fix.md)“已闭环根因”“最新源码”“自产崩溃”及“最终状态”。AM仅修兄弟夹具旧x64偏移，报告双架构191/191、独立各4/4；AO直接回读 `artifacts/am-bitmap-closure-20260721-a/readback-x86/TestResults/am-family-x86.trx:2335-2338` 为191/191/0。x64新191/191、Managed x64 4249/4249、producer x64新119/103/16来自AM报告与 `execution-observations.txt:3-15`，AO未逐份亲读这些最终TRX/identity，不升级为全部亲核。

AO亲读同observations:5-11，是AM保存的执行器输出摘录：新x86自产GetState+0x3/Ensure+0x32 NullReferenceException，无完成TRX、无数值退出码；不是AO实跑或原始完整dump。Managed x86仅进度3566、无完成TRX（同文件:13），不算全通过。AM提示NativeResourceBrushRealization默认unmanaged与相邻Thiscall差异为线索，未证根因。

保留原始失败分类（来源AM报告，非AO穷举）：x64原25=锁源image-mask16项present HRESULT差异+RotatedDrawing六项present HRESULT失败+VisualMask三项缺reference；后者须通过正式官方生成流程补齐，不改tracked golden。新119项仍16失败不证明其余九项已修，它们未纳入该新过滤。x86崩溃、x64生产失败、31组合缺独立官方广覆盖，均不能被191 Green抹掉。

## 3. AK亲核：软件曲线差异与新GPU圆角未验证分开

根 `artifacts/rounded-ak/readback/com-x64-final/Results/rounded-ah/b54c329f437544139e66f76de228896f/`：

- `comparison.json:1`亲读：threshold0，frames6，exact=false，hardwareVerified=false；帧0/2/4零差，帧1/3/5各44字节差，首offset696、(14,4)、channel0、官方36/候选40。
- `candidate/identity.json:1`、`original/identity.json:1`亲读：两者.NET10.0.1；官方来自系统WindowsDesktop10.0.1，版本10,0,125,57005，SHA `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`；候选SHA `C8C35CE4872FA659B02F8731F34F1F0492775D30879DEC17E54779F06228BE01`。backend明确 `RenderTargetBitmap software contract control only; NOT window GPU evidence`。
- 不能将AJ/AN的10.0.13官方版本写给AK这包。AO只枚举了原始pbgra32文件，未解码/复算像素，亲核的是保存的comparison和identity；B/A两通道细节及形状分组沿用AK/AN报告。
- AK x86 runtime未启动为任务交接事实；AO查询其 `readback/com-x86-final/Results` 不存在，且未搜到“You must install”原始文本。只有包文件不能证明执行，亦不足独立证明失败原因；AK x86无可亲核完成结果，不能写通过。

**软件RTB曲线覆盖差异是已观测失败；新GPU圆角缺backend内容证据是未覆盖，二者不是同一结论。** managed coverage通过不代GPU，也不代官方像素零差。

### AN最终报告已到达（未修复）

来源：[AK/AN圆角报告](replacement-supervised-rounded-rectangle.md)“AN有界实验与最终结果”。报告已完成，无生产算法修复；官方控制点先28.4量化再整数HFD与候选double flatten后量化的顺序差异是调查方向，未闭完整因果。不得写成已证实“预取整即可修复”。报告称双架构managed各4/4，正式self-contained.NET10.0.13；原合同帧1/3/5仍44差，新增边界cubic帧9十字节差（官方171/候选179）。以上实验细节/身份来自AN报告，AO未逐份回读AN业务JSON或原始像素。

AO另外亲读 `artifacts/curve-an/readback/com-x86-final/TestResults/curve-an-x86.trx:45-52` 及x64对应`:49-56`：各2执行/0通过/2失败，退出2，未执行0。这纠正AK x86无运行证据之后的最新状态：**AN已有双架构失败结果，但不追认AK x86曾启动成功**；AN新包官方10.0.13不能与AK10.0.1身份混用。GPU继续未覆盖。

## 4. AG / AI有限完成与保留门

沿用AL已经回读并写入[AG](replacement-supervised-present-sequences.md)、[AI](replacement-supervised-x86-bitmap-abi.md)的证据；AO本次未重新读其全部TRX/业务，不称新亲核：AG COM双架构各9/9、Managed各6/6及64字节FrameTime响应，只限当前CrossThread HWND异步场景；一般VSync、有效target同步、非零SyncModeStatus、在途销毁与迟到消息仍开。AI独立夹具有效、D归档x86透明缺陷已发现，不是当前源码验收；AJ新独立输出与D分属不同身份。

真实HW屏幕/duplication Capture的WAIT_TIMEOUT/resource0、真实loss→Reset→恢复后新色HW消费、持续媒体、长期CPU/GPU/线程/callback资源、广泛随机及异步关闭沿用既有交接，AO未新核原包。都仍为关键缺口，不因软件RTB/有限同步Release或局部Green降门。

## 5. 下一阶段：停止无限局部，集成稳定后立即完整基线

1. 先集成并稳定自产x86崩溃、x64 image-mask/RotatedDrawing、缺reference的正式官方生成，以及共享曲线软件差异；保留全部Red及未覆盖，31项夹具修复不代表自产闭环。GPU圆角单独验证真实backend和新内容。
2. 集成稳定后，**以最终源码、正常项目依赖图**构建生产及两个正式测试项目；不靠残留bin、禁依赖构建或旧候选拷贝宣称最终集成。正式矩阵为 `WpfGfxShape.Tests`、`WpfGfxShape.ComAcceptance` × x86/x64 Release四格完整适用基线，不能无限追加局部绿延后全量。
3. 每格实际测试输出指定本次任务目录内**绝对TestResults路径**，业务Results亦明确绝对回收根；完整回收顶层TRX、identity、参数、原始像素/事件/资源/恢复记录及异常输出。确认计数闭合，child TRX不顶替parent；无完成TRX记未完成，不拿3566进度作分母。此为下一轮要求，本轮未执行。
4. 对原始失败逐项分类：产品功能/ABI/时序/资源/恢复缺陷、夹具缺陷、缺reference/依赖、实际环境触发不足或未覆盖、崩溃/超时/未完成。每项保留原路径、输入、消息、归属和下一动作；不把25项一概说缺文件，不把所有HW失败归环境，不以错误码相同替代恢复内容。
5. 真实HW、真实恢复、持续媒体及受控长期资源与异步关闭继续独立补证；最终全量也不自动补齐这些未进入目标路径的门。所有SHA仅绑定各自产物/结果，旧full不绑定新源码，不设无谓hash前置阻塞。

## 6. 四文档交付与验证

本报告及 `Docs/next-session-handoff.md`、`Docs/replacement-issues-and-plan-status.md`、`Docs/completed-work/replacement-supervised-session-summary.md` 是AO独占写入范围。三入口均新增AO优先章节，保留历史；AJ/AM与AK/AN报告只读引用。四文件写后均以专用读取回读；无构建/测试作为本次文档验证。新实跑为零，完整替换仍未完成。
