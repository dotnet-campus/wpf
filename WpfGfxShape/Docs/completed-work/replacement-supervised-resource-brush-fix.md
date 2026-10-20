# AP 自产资源 brush x86 修复与 AU 最终证据回填

## 最终状态与证据责任

AP有界修复及回归已交付，本文由AU读取初稿后完成事实回填。**完整替换未完成，最后源码四格full未完成。** AU只使用专用读写搜索，不修改源码/项目、不构建发布运行，不使用终端/shell/直接或间接脚本；主代理只监督。AP执行事实与AU亲核必须分开，下述没有AU新运行。

初稿记录AM Bitmap/Realizer族各191/191、独立各4/4，自产x86 GetState/Ensure NullReferenceException，x64 producer119中103通过16失败；这些是修复前历史，不再作为AP最终状态。原6 RotatedDrawing与3 original-wpf缺reference不在新119过滤中，未复验仍开放。

## 原崩溃、根因与最小修复

- AP复现原x86自产崩溃（用户/AP交接事实）；AU亲读 `artifacts/ap-resource-brush-20260721/pre-x86-repro/Results/aot-identity.json`，实际X86/pointer4，候选SHA `C422361F16B9870A0C8B3B2F4B6544E004CFAD6C55F131975D23B28B248D7A12`，filter为 `WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping`。该identity只证明现场加载/参数，不证明崩溃栈。AU未取得原异常输出/完成TRX，不把缺失记录成通过，也不声称亲核崩溃栈。
- AP修复C++成员函数合同：core/brush/effects分别槽9/10/11采用Thiscall，不能混为Stdcall。AU亲读当前 `Code/WpfGfxShape/Abi/NativeResourceBrushRealization.cs:51-64,115-129`，三个函数表声明和回调均为Thiscall；槽9为无transparent参数的GetBrushCore，槽10带byte参数，槽11写effects。
- 同文件BorrowBrush经immediate槽10按Thiscall借用brush，不把realizer地址误当brush，不额外取得所有权。槽10调用亲见于:113；这是实现核对，不是独立官方行为的全部证明。
- AU亲读 `Code/WpfGfxShape/Core/SoftwareBrushRealization.cs:122-142`：EndPrimitive调用槽8释放realization资源，finally释放owner引用；外层finally复位_realizing并处理延迟释放。这里只证明当前释放路径存在，不证明长期CPU/GPU资源全部收敛。

## AP现成正式结果：AU逐路径亲核

以下路径相对工程根，统一前缀 `artifacts/ap-resource-brush-20260721/`。递归名称枚举曾漏列TestResults；随后专用目录及逐文件读取找到真实顶层TRX，最终以文件内容为准，不以最初搜索空结果判定缺失。

|结果|x86文件|x64文件|亲核裁决|
|---|---|---|---|
|producer|`producer-x86/TestResults/ap-producer-x86.trx:731-734`|`producer-x64/TestResults/ap-producer-x64.trx:731-734`|各Completed，total/executed/passed119，failed及其他非成功计数0|
|realizer定向族|`family-x86/TestResults/ap-family-x86.trx:1148-1151`|`family-x64/TestResults/ap-family-x64.trx:1148-1151`|各Completed，78/78，其他非成功计数0|
|宽Bitmap族|`bitmap-family-x86/TestResults/ap-bitmap-family-x86.trx:2931-2939`|`bitmap-family-x64/TestResults/ap-bitmap-family-x64.trx:2931-2939`|各Failed，251执行/247通过/4失败，退出2；不是全量成功|
|新增托管回归|`managed-readback-x86/TestResults/ap-resource-unit-x86.trx:35-38`|`managed-readback-x64/TestResults/ap-resource-unit-x64.trx:35-38`|各Completed，3/3，其他非成功计数0；不是托管full|

宽族四失败的TRX消息位置各395/414/433/452，均报告缺 `ReferenceData/glyph-arial-original.json`；AU读取x86第一项完整异常与两架构四处搜索命中。属于缺参考、未完成glyph像素验收，不能删失败或写成产品已等价；其余三项栈未逐字复核。新增托管条目亲见重复成员16/128次及保留引用后owner释放仍存活三个用例，不代一般长期资源验收。

## 实际identity与独立内容

AU亲读两个producer现场 `Results/aot-identity.json`：

- x86任务 `be03fb2481c8440db2c5e201a106cda7`，X86/pointer4，Native SHA `AB8EEB23982411FB5F74CB0F65ECB7D1E0804A542B8066FB3309ED4DE89B81BF`。
- x64任务 `6e5c32a5ebba4695b18d2572f41faf10`，X64/pointer8，Native SHA `6F8307E80245F2E84026608613816F5E6962730520833108A8788CA5507AD33F`。
- 两份family identity同SHA，但实际Arguments是source-clip-reference子角色，不能冒充父78族完整参数identity。Native SHA不能用于托管程序集身份；AU未取得AP实际加载托管生产/测试程序集SHA。

AU全文亲读 `bitmap-family-{x86,x64}/Results/independent-bitmap-realizer-*.json` 共八份，文件明确Candidate/Translated、实际模块路径、SHA、版本和像素，不是仅看测试计数。x86四文件后缀为 `False-4e5db0c94ca14ade88636193c7e5f323`、`False-b3f2d3f00bdf4974bcb3f8e315df8e87`、`True-498b0e655c4f4232aba9315e0be7e870`、`True-9805b25d80704be6ad47eadc61afdc11`；x64为 `False-ca54d3548f914f8585592a396c6a897c`、`False-e3f23c5be96f4298ad5394bc113a8c3c`、`True-2e0de2aa02d942d3a32cf751b14cdecc`、`True-3273f09be412426ba5337c03f9fe1fb6`（均.json）。

双方每份16个S_OK，Ensured/Brushes/Effects均16、ContextMatches=true；未平移PixelsBgra为 `[0,0,255,255,0,0,255,255]`，平移为 `[0,0,0,0,0,0,255,255]`，两架构官方/候选分别一致且非全空。x86 matrix/texture48/164、x64 80/208。实际.NET10.0.13，官方FileVersion `10,0,1326,50504`；官方SHA x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`、x64 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，候选为上述AP SHA。

该证据有真实官方非空内容/平移响应，不只是候选自洽；但仅两个独立输入，不覆盖全部宽族、所有自产组合或真实HW。AU未重算hash、解码二进制像素或重新执行比较器，也未核全部生命周期业务JSON；不将当前源码或ProductVersion直接等同最终dirty源码构建身份。

## 保留缺口与下一独立任务

测试线：最后集成源码、正常依赖图的Tests/ComAcceptance × x86/x64 Release四格完整适用基线仍待完成；AR仅AM快照x64 4249/4249，x86无完成TRX，不能拼接AP局部成绩关门。

问题线：原6 RotatedDrawing与3 original-wpf缺reference未复验；新宽族4 glyph参考缺失仍开；软件曲线、通知同步/非零SyncModeStatus/迟到消息，以及真实屏幕HW/duplication帧、loss→Reset→新色HW消费、持续媒体、长期资源和异步关闭继续开放。AQ预量化仅限定实验零差，无生产修复；AS/AT并行结果不在本包裁定。

参考生成另开下一独立纯C#任务：从真实官方同版同架构生成，记录actual identity与原始输出，只放ignored artifacts，不写tracked ReferenceData、不改golden/阈值/skip，不以生成了文件代替相应用例复验。本文范围内不启动生成、构建或执行。
