# DE 独立上传边界审查

## DF 授权最终补完（最新优先）

本轮仅证据审阅和五文档更新，主代理监督；没有终端/脚本、代码修改、新构建发布运行或 full 提交。以下“持续审查/暂不授权”保留为历史，由本节最终裁决更新。

### 根因与证据优先级

上传过大为用户纠正的首要根因。DB错误sourceDirectory使用回收目录，混入Results/TRX/诊断/历史；递归PublishDir额外纳入alpine/macOS/Ubuntu/arm64/CodeCoverage等资产。100秒HTTP错误不足以证明长执行通道障碍；旧任务当前0字节不是原上传0字节。原上传总字节未知，不提供缩减百分比。

直接路径读取原件高于目录枚举，监督/实际身份高于stdout推断。DD双架构 final-readback-*/Results/dd-final-*/TestResults/child.trx 最终Counters已亲读：均total/executed/passed=36、failed=0；两supervision.json全文均filtered/120秒/Exit0，无timeout/cancel/kill/排空超时。DD x64 Results/aot-identity.json 也已直接亲读（实际BW候选F34DEAB0…）。因此此前根据目录枚举写“缺TestResults/未亲核x86 TRX”错误，现明确撤销，不假装文件缺失。

### 最新 clean 输入与 smoke 原件

所有下列相对路径以WpfGfxShape根为基准。

|架构|clean输入目录|总字节|smoke任务|
|---|---|---:|---|
|x64|artifacts/de-upload-review/clean-x64-02/com-acceptance-package|157451706|21a05e928a394e2bb7ad4b4bc654539c|
|x86|artifacts/de-upload-review/clean-x86-02/com-acceptance-package|147783111|cce3c41fe7a4417b84f9406b8ae9e70f|

清单原件为 artifacts/de-upload-review/smoke-{x64,x86}-02/Results/dd-upload-boundary.json，头部TotalBytes/身份已亲核；并非声称本轮逐项重算清单。最终父TRX直接路径为各 Results/de-smoke-{x64,x86}/TestResults/child.trx，均36/36、失败0；两监督全文filtered/120秒/Exit0，无timeout/cancel/kill/排空超时，.NET10.0.13及匹配架构。

测试x64 SHA673EAA4260DCFD2233E391F00668264DB6EF391211457F0BA9C631AA49139A93、MVID21821cb2-22f7-4c5e-8376-ef1812e9e4bb；x86 SHA198989C80F41518CCD6B41559A8ACA67A7391BB47A4942C779D326762E9D6AE8、MVID1e3c5e42-4f06-47bc-86bc-20fc903ab14d。各Results/aot-identity.json全文直接亲读，actual候选x64 F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7；x86 7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA，保留生产修复不变。DD旧x86的147782599字节不可混写为DE最终包体积。

### 闭包与后续裁决

体积主要为.NET/WPF10.0.13自包含依赖；已安装runtime探测不足，不擅改runtime或改为framework-dependent。当前显式RuntimePackAsset/MTP/Contracts/Native/ReferenceData选择避免递归PublishDir；插件/coverage生成注册与依赖不能靠随意删DLL隐藏问题，现有smoke通过也不是所有full插件路径已覆盖。

ReferenceData两静态文件glyph-arial-original.json、original-wpf.json仅代码追踪支持选择合理；source-clip-intersection.json和original-publicabi.json同次官方角色生成。没有full运行证明完整依赖闭包；36/36仅入口/打包边界/BW smoke，不是完整替换或最终四格验收。

允许下一阶段在明确逐文件清单、总大小、架构及actual身份审查后安排全新clean包full；本轮不提交。禁止使用任何readback/smoke回收根、DB回收根或PublishDir作sourceDirectory，默认包禁止Results/历史/无关资产。未发现必须继续拒绝clean full的已证技术阻断；闭包由full实跑最终裁决，不能把需full验证闭包当永久不跑理由。后续回收监督、父TRX/失败组及actual test/native/official/runtime，再决定修复；保留CX/CY等生产修复及未闭合同，不移绑旧BD/未知DB full。

## 授权和执行边界

用户最新纠正具有权威性：先修大上传包，不以异步启动/轮询绕时限。主代理仅监督。仅允许打包 targets、专属包测试、DD/DE 报告；结束后才更新三份指定交接文档。生产、NativeRun、ExitDiagnostics、默认业务断言、项目版本不动。禁止终端及直接间接脚本；只使用实际专用工具和 Windows 沙箱中的正式纯 C# exe。不提交 full，不新增 Host/项目，不清理他人目录。DE 新产物唯一根 artifacts/de-upload-review/。

## 持续审查记录

已读规则、完整方案尾节、DD 初稿、当前打包 targets/专属测试、正式项目结构及 DB 报告。DD 初稿仅9行，未包含已有最终 smoke 证据；现开始补录，后续结论以亲读原件为准。

已亲读 DD x64 最终监督全文：task216843acbe554e38badc6ccbd7c5aba1，filtered，120秒，Exit0，无 timeout/cancel/kill/排空超时。测试SHA 7D23BE155F6078938506908DF003D492199F4787A43FC539417AD9950036FA0F，MVID c2229b26-1d90-491d-bdbe-6e11710c4fdb，.NET10.0.13/X64。x86 stdout 记36/36、失败0、跳过0，但本地回收缺 TestResults，尚不能声称亲读最终 TRX。

DD 输入审计记录 x86 task174197efec39493d9e070e63a1151c4d，总量147782599字节；x64用户交付157451706字节待完整清单复核。这是清洁输入包记录，不是旧DB上传总量。旧任务当前快照不代表当时上传内容。

当前 targets 自包含路径从 RuntimePackAsset + 显式 MTP/Contracts/Native/ReferenceData 取文件，不递归 PublishDir。PublishDir 仍可见 alpine/macOS/Ubuntu/arm64/CodeCoverage 等资产，因此不能直接上传 PublishDir。当前另有 DisableAcceptanceCoverageRegistration，必须审查其生成注册和运行依赖，不能靠删除已注册插件依赖缩包。

ReferenceData 两文件是否完整尚未裁定。已找到 source-clip-intersection.json 消费链，其由 CreateSourceClipReference 在同次隔离官方角色中生成，不是静态输入；PublicAbi 的 original-publicabi.json 同次生成。仍须核其余全项目引用和默认 full 路径；不据两个文件名直接宣称 full 闭包完整。

## 当前裁决

审查进行中，暂不授权下一次 full。smoke 不等 full 依赖闭包运行验证。此前把100秒HTTP错误直接归为长执行通道问题的归因不充分，应先审查实际上传边界。原DB包总字节数未知，不报告缩减百分比。
