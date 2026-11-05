# DD 上传输入边界审计与最小打包修复

## DF 最终事实补正（最新优先）

本轮只读证据并更新五文档；主代理监督，无代码修改、终端、脚本、新构建发布运行或 full 提交。以下进度为历史，由本节覆盖。

首要根因按用户纠正为上传过大：DB 错误 sourceDirectory 指向含 Results/TRX/诊断/历史产物的回收目录；递归 PublishDir 又纳入跨平台、异架构及 coverage 额外资产。不能继续称已证明长执行通道障碍；100秒HTTP异常不是被测退出。原上传字节未知，不能由当前快照为0推导原上传0，不造缩减百分比。

直接路径亲读优先于目录枚举：final-readback-x64/Results/dd-final-x64/TestResults/child.trx 与 final-readback-x86/Results/dd-final-x86/TestResults/child.trx 的最终 Counters 均36/36、失败0；两监督全文均 filtered/120秒/Exit0，无timeout/cancel/kill/排空超时。对应task216843acbe554e38badc6ccbd7c5aba1及task174197efec39493d9e070e63a1151c4d。此前“缺TestResults/未核x86 TRX”仅为枚举漏列造成的错误结论，不再适用。x64 Results/aot-identity.json 已直接亲读，BW实际候选为 F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7。

最新 DE clean 输入为 artifacts/de-upload-review/clean-x64-02/com-acceptance-package（157451706字节）及 clean-x86-02/com-acceptance-package（147783111字节），不是旧DD x86的147782599字节。DE smoke-x64-02/task21a05e928a394e2bb7ad4b4bc654539c、smoke-x86-02/taskcce3c41fe7a4417b84f9406b8ae9e70f，各最终父TRX36/36、监督Exit0；actual候选分别F34DEAB0…、7B590883…，与保留生产修复身份一致。仅为入口/边界/BW smoke，不外推 full。

清洁包体积主要是.NET/WPF10.0.13自包含依赖；安装runtime探测不足，不擅自改runtime/框架依赖模式。ReferenceData 的 glyph-arial-original.json、original-wpf.json 仅代码追踪支持静态选择合理；source-clip-intersection.json及original-publicabi.json为同次官方角色生成，不能宣称 full 闭包已实跑证明。

后续 full 只能以全新 clean 包为输入，先审逐文件清单/总字节/架构/身份，默认打包禁止Results、历史回收及无关资产；PublishDir和所有readback/smoke回收根不可再传。可在文档收口后安排有明确清单大小审查的 clean 包 full；当前未发现必须永久禁止 full 的技术阻断。闭包最终由 full 裁决，不以“须full验证闭包”循环拒跑。本轮不提交；保留生产修复及最终四格等未闭验收。

## 授权与边界

以最新纠正为准：先审计上传内容异常膨胀，不以异步启动/轮询绕过超时。仅修改打包及专属验证/本报告；不修改生产、NativeRun、ExitDiagnostics、full 逻辑及共享交接。不新增工程/Host/脚本，不执行命令行。独有产物根为 artifacts/dd-upload-boundary/。仅提交身份/路径与 BW/入口 smoke，不提交 full。

## 进度

以下由 DE 独立审查补录，替代已过时的“尚未上传/运行”进度；原授权边界不变。

已存在最终清洁包及回收：artifacts/dd-upload-boundary/final-x64/com-acceptance-package、final-x86/com-acceptance-package；final-readback-x64、final-readback-x86 是输出回收目录，禁止再次作为上传输入。PublishDir 仍有 alpine、macos、ubuntu、arm64 和 CodeCoverage 资产；当前 self-contained 打包按显式测试平台文件、RuntimePackAsset、Contracts、Native 和指定 ReferenceData 取文件，不再递归 PublishDir。

DE 已亲读 x64 最终 supervision.json 全文：task216843acbe554e38badc6ccbd7c5aba1，filtered/120秒/Exit0；无 timeout/cancel/kill/排空超时，.NET10.0.13/X64，测试SHA7D23BE155F6078938506908DF003D492199F4787A43FC539417AD9950036FA0F、MVID c2229b26-1d90-491d-bdbe-6e11710c4fdb。x86 task174197efec39493d9e070e63a1151c4d 的 stdout 记36/36、失败0、跳过0；本地最终回收未见 TestResults，尚未亲核最终x86 TRX，不能以stdout冒称TRX。

x86输入审计记录TotalBytes147782599、测试SHA1593935A97FD8581A7C24D57515503387A51E537CF09B74D8E3A23B207D6F91A、MVID4aa5cbd9-757f-4025-9017-e27d96d5eeae、候选7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA；x64交付总量157451706待DE完整分布复核。旧DB回收目录混历史Results/TRX/诊断是已知输入边界错误；旧任务当前为0不能推导原上传0，原上传总量未知，不给缩减百分比。

ReferenceData仅glyph-arial-original.json/original-wpf.json的静态闭包及MTP插件/coverage依赖尚待独立核验，不能称full闭包已运行验证。source-clip-intersection.json已追到同次官方角色生成链；PublicAbi原始报告同次生成。最终smoke不等full；本轮不提交full。详见DE replacement-supervised-upload-boundary-review.md持续审查。
