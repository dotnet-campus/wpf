# KP20B3 direct consumer：范围与预定义验收快照

## 执行前冻结
仅正式 ComAcceptance 新增独有默认关闭 partial、最小资源键、显式 ConsumerDirectDiagnostic 配置及本文/恢复文档。沿 net10.0-windows；不改生产、Tests、公共 helper、原业务测试、NativeRun、ExitDiagnostics、global.json，不重开 WGC，不新增 Native 探针，不委派/终端/脚本。

两方法 official/currentNative 同一序列：真实 D3D9 surface→InteropDeviceBitmap→image/drawing/visual/HWND target，HD async connect、packed92/flags12/合法 DPI context、separate PresentPacket/duplicate event/commit/5秒等待/flush。红FFFF0000→蓝FF0000FF→绿FF00FF00三次独立提交；每色 DwmFlush 后仅一次 GetPixel(window DC,16,8)，严格 COLORREF 255/16711680/65280，首次错误立即失败，不轮询颜色、不调预期/容差。无 producer 读回、partition/snapshot义务或软件 fallback。backend 未知；window DC 不认证 screen/duplication/scanout/HW。

保留产品初始化、consumer创建、资源删除/flush/channel/connection/partition清理、bitmap detach/四 refs0、owner线程窗口销毁和回调记录；复用既有持续泵 owner 窗口接口，不在错误线程销毁。模块身份必须实际加载路径/SHA、test SHA/MVID/runtime及真实TRX UID，不以初始化文件摘要充实际加载。

预算：正常 COM build≤1，成功后正式 self-contained10.0.13/win-x86 COM publish≤1，显式绑定 KP clean20B3 Native，独立输出与clean；Native0。任一 build/publish 失败即停。最多两沙箱任务，先official精确 FQN 等值 filtered 监督120秒/外180秒，TRX恰一真实体；official三色与cleanup均过才同包另一独立进程 currentNative一次。无额外发现/list、x64/full/shader/resize或Native重发。入口静态无法确认则停止，不上传publish根。

中心候选：20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D。官方根模块10.0.13；实际模块与包身份待核。全部WGC/KS历史账本和通用打包退场残项保留，见 retirement-and-coverage 报告，不由本门重置。

## 实施与验证
**唯一正常 COM build 失败，按授权立即停止，未进行失败后源码修复或重试。**

实际代码修改：
- `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.DirectConsumer.cs`：新建整文件 `CONSUMER_DIRECT_DIAGNOSTIC` guard；两公开测试方法共享 RunDirectConsumer。无 HB fresh-native 前置/observePartition；每色一次 window DC 严格断言；记录实际加载身份、HR/event、refs/callback及 owner 销毁线程。
- 同目录 `WpfGfxShape.ComAcceptance.csproj`：新增显式 `ConsumerDirectDiagnostic` 配置（符号/Optimize/portable），仅该配置的 FrameworkReference metadata 固定10.0.13；不传全局 RuntimeFrameworkVersion，TFM不变。不修改 Package.targets。
- 同目录 `AcceptanceResources.resx`：新增单一 DirectConsumerContract 键。

正常构建参数：目标正式 ComAcceptance.csproj；configuration=ConsumerDirectDiagnostic，runtimeIdentifier=win-x86，targetFramework=net10.0-windows。配置通过正常 build 接口可选；未设置 ComAcceptanceSandbox/self-contained，非 publish。专用日志64行已亲读：Exit1，134 warnings/1 error，3.51秒；stdout106243字符，前86243未显示，不声称全部警告已审。

唯一错误为 `D3DImageTests.DirectConsumer.cs(196,47) CA2219`：finally 内 `(windowThread ?? throw new InvalidOperationException()).Invoke(...)` 的显式 throw 表达式被分析器拒绝。该实现错误复现了 HD 报告曾记录的同类 analyzer 边界，属于本次执行实现失误，不是产品或环境失败；没有禁分析器/抑制错误，也没有在失败后改源追绿。当前新增诊断配置**不能成功构建**。默认关闭仅由 guard/config 静态成立，本轮未另构建默认配置，不声称其新验证通过。

静态边界：已读 HB 全文及 HD合同关键表、WindowThread全文、OutputWindow创建/完成/设置路径、DestroyWindow包装器、NativeRun/ExitDiagnostics入口。packed92/PresentPacket16、flags12、DPI语义、async/separate/event/flush沿HD；owner持续泵与同owner直接销毁意图保留。但共有 owner ready/Invoke/Join 本身无局部超时，外层监督原拟限制120秒；清理仍为顺序断言，前一清理失败可能阻断后续，不能宣称失败清理已完整认证。duplicate send失败所有权边界沿HD未独立认证。上述均未运行，需下一次实现审查，不得将静态自洽记为通过。

源码准确 FQN：`WpfGfxShape.ComAcceptance.D3DImageTests.WhenDirectOfficialConsumerReceivesThreeColorsThenWindowDcMatches`；currentNative方法同前缀 `WhenDirectCurrentNativeConsumerReceivesThreeColorsThenWindowDcMatches`。现有监督Options产生 `--filter` 参数，原拟表达式为 `FullyQualifiedName=<准确FQN>`；不执行额外list发现。仅源码入口确认，不冒称编译后发现/UID验证。

|本次实际计数|数量|
|---|---:|
|独立正常 COM build|1失败|
|COM publish / Native publish|0 / 0|
|沙箱 / 单独发现 / official请求 / candidate请求|0 / 0 / 0 / 0|
|真实测试体 / 真实颜色 / consumer创建|0 / 0 / 0|

亲读 KP clean 根 aot.sha256 为20B3完整摘要，独立 direct-consumer-kp20b3输出根在实施前不存在；没有新clean/publish/manifest/依赖验证。没有新的TRX、监督、stages、JSON业务实跑原件、test实际SHA/MVID/runtime/UID或official/candidate实际加载身份；一律未取得，不借历史或文件初始化摘要补齐。build日志只证明本次构建失败。

## 覆盖与主线结论
|能力|本轮裁决|
|---|---|
|当前20B3直接consumer红蓝绿/事件/清理|未执行，仍Open；非official颜色失败，非candidate缺陷|
|KN/KO两架构各17条有限resize/lifetime|历史保留，不升级为像素覆盖|
|HD/HE/HF旧SHA合法consumer|仅历史协议先例，不绑定20B3|
|原KP resize像素ID、Available/HWbackend/real loss/media/最终四格|全部沿既有Open，不升级|

WGC实验仍build4（1败3成）/publish4（1败3成）/沙箱3（发现1真实2）/真实2失败；WGC撤后default build1成功另列。本次direct build1失败另列，不能混入WGC。KS仍build1/publish1/沙箱4/请求3/发现1/体1/A1/B1/native2。通用ReferenceCopyLocalPaths/FullPath去重/PackagePath/Copy及manifest引入前快照缺口原样保留，未动打包规则或历史包。

**唯一下一门：另行授权修正本独有partial的CA2219构建阻塞并审查失败清理/等待边界，再重新给定有界build→publish→official-first预算。** 本轮预算结束；不自行继续，不恢复WGC、不要求native-only先恢复。真正consumer直接验证目标尚未达成，完整替换未完成。
