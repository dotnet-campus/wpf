# 问题线 S：捕获内容帧超时及状态机负控

## 结论与纪律

已探索、编码、完成最后源码双架构 Release 发布和定向 Windows 沙箱实跑。**捕获仍未闭合，不是全量验收。** 两架构各18执行、14通过、4失败：两项原Present control、两项新交替帧control均内容等待失败，TRX timeout=0。

执行前已创建本报告。已读交接执行规则、完整方案85–120、问题状态、capture diagnosis、cleanup preflight、independent review，并回读L原始失败。仅专用读写/发布与沙箱执行编译exe，无终端/shell/脚本；未改生产、窗口、ABI、启动、项目、golden、skip或零阈值，未重发生产或清理共享目录。使用正式ComAcceptance项目，未新增项目；Managed项目本轮未改未跑，不冒称两个项目全量。

## 更改文件

- `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.DesktopDuplication.cs`：提取真实循环共用 `WaitForDesktopContent`，保留非零且不早于提交点的筛选和原5秒截止。逐次记录Acquire HRESULT/QPC/resource/提交点；成功帧记录lastPresent/lastMouse/accumulated/coalesced/protectedMasked。拒绝帧resource在finally释放，外层沿用L的CleanupDesktopCapture。
- 同文件新增共用循环负控：WAIT_TIMEOUT→pointer-only→旧帧→WAIT_TIMEOUT→新帧（两次拒绝释放，最终帧持有）；连续WAIT_TIMEOUT到截止；ACCESS_LOST立即失败且不释放未持有帧；无关新鲜蓝帧交独立红色断言拒绝，不消费后续正确红帧，禁止按颜色重试追Green。这些是共用控制流可控回归，不是原生COM故障注入或真实外部更新实证。
- 同文件增加官方模块身份记录入口，复用LogHardwareModuleIdentity；模块进程驻留。
- `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.PresentContract.cs`：原两swapEffect control仍检查原颜色、front buffer与screen pixel；新增红→蓝→红→原色逐次真实Clear/Present/duplication独立颜色检查及提交/消费QPC。不以timestamp充当窗口内容身份，不按颜色重试。实际首红帧便失败，后续交替未覆盖。
- 本报告。

## 实际超时链和根因边界

独有证据根 `artifacts/replacement-capture-s/`，最终为 `final-readback/{x64,x86}/`；初次调查为 `readback/{x64,x86}/`，不拼接计数。

最终x64 `TestResults/capture-s-final-x64.trx:46–70`：窗口target/cover/foreground一致，inputDesktop=Default，screen(56,48)。adapter为Microsoft Basic Render Driver、vendor1414/device008C、LUID00000000:00004E71；DISPLAY1 bounds(0,0,1920,1080)、attached1、rotation1，output/window monitor均10001。

- 初始Acquire=0x887A0027 WAIT_TIMEOUT，不是本轮取得LastPresentTime0帧。
- submissionStart32141017180575；提交32141017188670；完成32141022379233。提交前后真实staging/backbuffer均FF123456。
- 后续十次Acquire全部WAIT_TIMEOUT、resource全部0，QPC32141027396281至32141072880905，截止断言失败。
- swapEffect3及两个交替用例同为初始超时+后续十次超时；交替首红帧真实backbuffer=FFFF0000。x86轨迹相同。

已定位本轮直接失败在 **Acquire未交付任何资源**，早于时间戳过滤、QueryInterface、CopyResource、Map/RowPitch消费，不能描述为谓词误拒本次帧、读回零像素或候选绘制失败。只读检查CompleteNativePresent确认GPU query及两次DwmFlush成功仍不能保证duplication交付。更底层为何该会话不交付更新，证据不足，未做猜测性修复。

本轮没有实际acquired resource，所以没有可回读的新帧metadata、纹理或PPM；已检查最终Results仅身份文件。不伪造图像，也不使用旧PPM冒充本轮原始内容。实际颜色证据是TRX内backbuffer读取日志，无duplication消费成功证据。

## 最后源码构建与实跑

两架构最终专用发布均成功：Release、win-x64/win-x86、自包含、RuntimeFrameworkVersion10.0.9、ComAcceptanceSandbox=true、对应AotRuntimeIdentifier、显式AotDllPath选L归档各架构Native/wpfgfx_cor3.dll，DebugType=None、DebugSymbols=false、ProduceReferenceAssembly=false。输出 `final-publish/<arch>/`、`final-package/<arch>/`。未修改TFM/SDK/C#版本或重发生产。

正式 `WpfGfxShape.ComAcceptance.exe` 筛选WhenDesktop、原Present、交替帧及PublishedArtifactTests，开启TRX，回收整个任务目录。工具未另行暴露数值退出码；TRX RunInfo记失败码2。

|最终格|TRX|执行|通过|失败|error/timeout/aborted/notExecuted|
|---|---|---:|---:|---:|---|
|x64|`final-readback/x64/TestResults/capture-s-final-x64.trx:302`|18|14|4|均0|
|x86|`final-readback/x86/TestResults/capture-s-final-x86.trx:297`|18|14|4|均0|

已直接回读两份最终Counters/RunInfo、官方身份正文、aot-identity.json和提交/Acquire日志。14通过包括四谓词、四清理、四新增循环/无关内容负控、官方身份及候选身份，不是14个HW内容场景。初次调查两架构各16执行12通过4失败，不是最终结果。

## 实际身份

候选来自最终 `Results/aot-identity.json`，x64/pointer8、x86/pointer4；系选择L既有归档，不是最新生产声明：

- x64 SHA `2BFC4AC24A83EBAF45A4B375CDC5FE1F9C90BD9370D73BD590C259091C5BF58F`
- x86 SHA `747F307104A9E51E9689307BD85CAABC60EB60294F7F307E21CF687F77E7A142`

官方来自最终TRX22–37及Results/hw-module-*.json，Requested=Actual为包根官方DLL，非Native候选：

- x64 SHA `098D9CEFACA7B1E36460EB28F2725D7AFDB8E93855C29F8DD3627A1FB692AE8E`
- x86 SHA `0CC79CC937B17827344B8652556A4CFD774CFF64B7FD2709EF5284C480894047`
- FileVersion `10,0,926,27113`；ProductVersion `10.0.9-servicing.26270.113`；commit `901ca941248413c79832d2fdbd709da0c4386353`；.NET10.0.9。
- 最终测试程序集SHA：x64 `FC56AA0841486B9FE31D3496C968FBD3CAF493B6AD78CAD840C8077B8B5F70E0`，x86 `848D178A050CA654C76F9ABED35F30BF1FA73933C6802C8694BB156490C14B63`。

系统D3D9 control不经过候选绘制；官方身份通过不等于官方geometry/sampler内容通过。本轮未实跑官方对官方完整scene，不冒称官方同场景闭合。Basic Render Driver不是物理HW或候选内部backend证明。

## 未闭与后续具体条件

1. 需要可观察交互桌面同一会话、同DISPLAY1上，Desktop Duplication在真实Present后实际返回成功资源，不能只验证Create或DwmFlush成功。当前每架构四个control均初始及后续WAIT_TIMEOUT；应核实该会话/output实际更新交付条件。现有证据不足以归因具体驱动或会话内部原因。
2. 条件满足后原样运行两Present及两交替control，取得metadata/PPM/颜色消费和front/screen独立断言，不换捕获方式、不放宽阈值、不追Green。
3. 真正外部无关桌面更新、真实ACCESS_LOST恢复、多屏/rotation、资源长期收敛、官方geometry/sampler及物理HW仍未覆盖。可控负控只证明共用控制流，不代替真实场景。
4. 最后生产依赖及双架构全量仍归集成；本包不关闭I03，不宣称完整替换。
