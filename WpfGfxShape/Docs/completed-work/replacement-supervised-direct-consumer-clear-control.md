# Official same-target independent clear / image control

## 验证前冻结快照
本门已授权，仅 DirectConsumer 默认关闭 partial、一个准确资源键与文档。沿 ConsumerDirectDiagnostic；不改原三色或固定时点语义，不改公共 helper、生产、Tests、NativeRun、ExitDiagnostics、Package.targets、SDK/TFM/global.json。不新增捕获设施、不native前门、无终端/脚本/再委派；执行仅 Windows 沙箱。

精确 FQN=`WpfGfxShape.ComAcceptance.D3DImageTests.WhenDirectOfficialTargetClearsGreenThenImageRedMatches`。共享初始化新增默认false clearControl 参数：同 HWND/target/channel/flags12/DPI，visual不绑定原renderdata、target root=0；SetClearColor24字节0x36/RGBA float(0,1,0,1)，commit/flush/invalidate/commit/flush/DwmFlush后一次DC严格65280。先存原值，绿错即断言清理，不绑定/不发红；绿通过且无DC清理错误才绑定原renderdata/root，复用一次红 ColorFill/dirty/独批Present/event/flush/DwmFlush/一次DC严格255。最多两次取色、红提交一次；无颜色循环、延长、重发、GDI预涂或producer读回。

官方合同已亲读：include/Generated/wgx_misc.cs197–203为顺序float r/g/b/a；wgx_commands.cs358–378为root/clear24/invalidate24；command_types clear=0x36。uce/rendertarget.cpp138–166接受hRoot0；hwndtarget.cpp658–678 SetClearColor复制颜色并强制full render，alpha<1才另需destination alpha，本门alpha1；Render132–146空root在full render时Clear/Invalidate/NeedsPresent，263–297 Present路径合法。仍受实际target/enabled/no-screen/zombie等分支约束，不保证HR0就绿；此处是仓库参考源码，不冒servicing二进制逐行绑定。

分类预注册：身份/HR/event/cleanup错误单列并停止；绿错=target共同输出或观测未闭，不唯一归环境；绿过红错=缩小interop/image或后续target差异，不唯一根因；绿红过=仅加清屏路径有限内容，不证明原缺陷解释或candidate通过。清屏可能预热HW device并改变interop frontbuffer条件；actual backend未知。主异常EDI保留、清理逐项尽力、finally不throw、owner仅一次销毁。

预算：正常COM build1，成功才publish1，均失败即停不修后重试/不抑制分析器；self-contained win-x86 NETCore与WindowsDesktop10.0.13，显式KP clean20B3，独立artifacts/direct-consumer-clear-control输出/clean。核包根roles/manifest/runtime/精确方法后official沙箱1，--supervise-exit Results/direct-clear-official 120 + 精确FQN等值，外180秒。发现/candidate/Native/native-only/线程/WGC/x64/full/resize0。最终结果后停止。

此前direct build3(1败2成)/publish2/official2真实体2失败/candidate0；本次待填。WGC build4(1败3成)/publish4(1败3成)/沙箱3(发现1真实2)/体2失败、撤后default build1成功及通用打包退场残项保留；KS build1/publish1/沙箱4/请求3/发现1/体1/A1/B1/native2另账不变。完整替换Open。

## 实际结果
### 构建发布及包
正常COM build1成功：ConsumerDirectDiagnostic/win-x86/net10.0-windows，135warnings/0errors/3.66秒，分析器正常；仅核日志末13行，不冒全warnings审阅。publish1成功，显式KP artifacts/kp-resize-pixels/com-acceptance-package/Native/wpfgfx_cor3.dll，独立新direct-consumer-clear-control/publish与com-acceptance-package。发布末9行亲读，之后源码无改/重试，Native发布0。
clean runtimeconfig全文21行，两框架10.0.13；aot.sha256确为20B3完整值；manifest621行核首15及611–621，搜索确认根official413/Native619、Contracts/入口/MTP齐备；未逐条重hash全包。离线公开API确认新方法；NativeRun与ExitDiagnostics亲读确认监督三参数/--filter等值，无斜杠filter。仅上传clean不是publish根，无发现执行。

### 唯一运行与原件
任务ced1177b9f8e41849623fedc404ce201；回收根artifacts/direct-consumer-clear-control/readback/official/Results/direct-clear-official/。监督40行、TRX53行、业务JSON45行、stages46行全部分段亲读；stdout32行和空stderr亲读。stdout中文替代字符，以UTF-8 TRX/JSON裁决。
监督7712/子8536，2026-10-27 02:59:59.350178→03:00:01.2026636 UTC；exited/Exit2，无timeout/cancel/kill/drain错误，OutputCollected=true/SupervisorError=null。精确新FQN，TRX total/executed/failed=1/1/1、passed/notExecuted=0/0；UID/testId=1a26941d-5215-8460-837d-cda13c4b5bc1，executionId=fdfb5f4c-7662-4dfe-90b4-0a646ef6d2bd，runId=66809636-8d4d-418f-b998-d9a580dd50d6，duration1.0013968秒。非空发现，不将监督完成当Passed。

### 实际身份
- test SHA256=CB821F7F92434D636BA8E98AFA44F6C11E9E15F1BDD907E5DC9E6797D2460FA7，MVID=d947710e-6534-4473-8ce1-285381190852，监督/业务一致；actual.NET10.0.13/X86/PointerSize4。
- Requested==Actual为任务根C:\CodingAgentSandbox\Tasks\ced1177b9f8e41849623fedc404ce201\wpfgfx_cor3.dll；actual official SHA256=5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834，10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。
- actual PresentationCore SHA256=D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D，同servicing版本。
- candidate SHA256=20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D仅包文件及初始化认证，非actual加载证据，candidate运行0。

### 内容、HR和资源
同HWND22873106/target4/channel83790936，flags12、DPI context-2/scale1、32×16、owner2436；桌面前置target=cover=foreground=0x15D0412/Default，不代内部target状态证明。root0/clearRGBA(0,1,0,1)记录完整；clear/root/setup close/commit/flush/invalidate/close/commit/green consumer flush/DwmFlush全部HR0。一次green DC实际1973532(0x001E1D1C) !=65280，Accepted=false；原值持久保存后主断言Failed，立即清理。红visual/root绑定、ColorFill/dirty/Present/event/DC均not reached，不写成HR0或通过；实际ColorReads1/红提交0，无重发或延长。合法clear/present路径不证明本次内部Present已执行，backend unknown。
资源4→1均Deleted1/HR0；delete close/commit/flush/channel destroy/disconnect/partition deinitialize全部0；bitmap/surface/device/factory四refs0。callback仅Available1/Version7一次，不证明loss。owner/actualOwner/current均2436，Destroy Result1且一次；cleanup Primary保留greenPixel断言、Errors=[]，无finally throw。无红event transfer，event仍dispose，DC清理无报错。

### 裁决、覆盖与唯一下一门
独立绿也未反映到DC：共同target输出或观测未闭；不能将原红失败唯一归interop/image，不能唯一归环境，更不判candidate失败。红侧未覆盖，本次未检验清屏预热后的interop；清屏可能预热HW device并改变frontbuffer条件的混杂保留。有限consumer/完整替换仍Open，不存在产品通过或原缺陷已解释。已停止验证与设施扩展。
本次build1成功/publish1成功/official1真实体1失败；累计direct build4(1败3成)/publish3/official3真实体3失败/candidate0。发现/Native/native-only/线程/WGC/x64/full/resize/原三色/固定时点均0。WGC及撤后default build、通用打包退场残项、KS独立账保持前述数值。
唯一下一门仅建议另授权有界只读核查同target空root清屏的调度/Render-Present到window DC关联，基于现源码与本次原件，分清合法提交和实际状态/观察未知；代码/build/publish/沙箱/新采样0。不改生产、不换目标、不延长、不补跑，不恢复native/线程/WGC/full。
