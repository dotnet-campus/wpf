# AZ 缺失官方参考生成及原失败复验

## 结论

本有界任务完成：Windows x86/x64 Release 最后测试源码发布成功，真实官方独立生成各2/2，原失败13项各13/13通过，新增参考身份验证各1/1，最终parent TRX各14/14，0失败、0跳过、0未执行。不是AP宽族251重新全跑，不是最后生产源码四格full，不声明完整替换。

先创建本报告，再读交接、执行规则、完整方案、resource-brush-fix及bitmap-layout-fix报告。全程专用读写/发布和Windows沙箱执行编译C#，无终端/shell/直接间接脚本。未改共享生产、夹具、生命周期、曲线、通知、上传、捕获、入口或项目，未改golden/skip/阈值/fallback。未清理他人目录。

唯一新增代码：`Tests/WpfGfxShape.ComAcceptance/ReferenceRevalidationEvidenceTests.cs`。记录候选/官方/测试程序集/PresentationCore版本、SHA、MVID、参考消费绝对路径及原始Glyph和三个Mask样本；断言参考官方SHA等于同包官方SHA。不生成像素预期，不改变既有断言。

## 候选与构建

明确复用AP已修复资源Brush归档DLL：`artifacts/ap-resource-brush-20260721/{x86,x64}/native/wpfgfx_cor3.dll`。生产未改，不重发生产。当前测试源码双架构Release自包含发布，net10.0-windows，ComAcceptanceSandbox=true，AotDllPath分别指定AP DLL。仅现有正式ComAcceptance项目，无第三项目。

以下路径相对 `artifacts/az-reference-revalidation-20260721/`。独有WpfGfxShapeArtifactsRoot及发布输出为 `final-{arch}/`，含publish及com-acceptance-package。初轮非final目录的官方各7/7及复验各13/13保留，但最终结果只绑定新增测试后的final包，不累计计数。

## 官方生成与消费

使用现有两个生成测试：`WpfGfxShape.WpfReference.GlyphReferenceTests` 和 `WhenTileFixturesAreRenderedThenOriginalWpfEvidenceIsRecorded`。NativeMethods从PresentationCore同目录加载根目录真实官方wpfgfx_cor3.dll，生成器核实际模块路径；不使用Native子目录候选生成预期。

生成沙箱整个任务回收为 `final-official-{arch}`，含新生成 `ReferenceData/glyph-arial-original.json` 和 `ReferenceData/original-wpf.json`。再把该完整目录作为复验包送入新沙箱，无手工复制假预期，无tracked ReferenceData写入。新进程消费AppContext.BaseDirectory/ReferenceData下这两份原始文件。

| 架构 | 官方生成任务 | 最终消费任务 |
|---|---|---|
| x86 |04429ec5a1ef476ab6d3003accd0fc15|44610a69bb9e491bb764349540c4cf0c|
| x64 |df8452791ab746ceb7aa9e6618e0eead|0ef14cb62d344054b13c30af97f36b12|

绝对消费根：`C:\CodingAgentSandbox\Tasks\<最终消费任务>\ReferenceData\`；候选是同任务 `Native\wpfgfx_cor3.dll`，官方是同任务根 `wpfgfx_cor3.dll`。参考内生成来源路径仍指生成任务，未伪改为消费任务。

## 实际身份（已回读）

实际.NET10.0.13；官方FileVersion `10,0,1326,50504`，ProductVersion `10.0.13-servicing.26504.104`，commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。候选FileVersion1.0.0.0，ProductVersion `1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685`。该字符串不是最终dirty源码凭证，结论绑定AP归档SHA，不追认最终生产源码。

| 身份 | x86 SHA256 | x64 SHA256 |
|---|---|---|
| 候选 |AB8EEB23982411FB5F74CB0F65ECB7D1E0804A542B8066FB3309ED4DE89B81BF|6F8307E80245F2E84026608613816F5E6962730520833108A8788CA5507AD33F|
| 官方 |5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
| 测试程序集 |0303591504F4FC8370FF6ED0EC649A79CA065E9C7D820C86E6AC9D83DECA3BAA|44A593E3F049D20F06A036C73E6FA409DE11390FFB4F006640234BD43CC30138|
| glyph参考 |DE16BA19135234E1D99E7DBC380076FC28A3CBB481F1D2A74324968CC0A7DC52|2C229560BECDA1E5DFF8FCD7CBC61D61F61603887FFC5757A031D91A1CB50273|
| drawing参考 |3B56FAEC09BC0C53F2970F95641C1F81F16BAD8BAF19D9100AC6903458138FBB|F2B1FE085EF90147F4DECDDB6376EEAC8E0166D025DC1AA366BB33DFCB9ADC49|

测试MVID x86 `e3ec87d7-c42f-40d3-9eae-7403da780270`，x64 `f9ac1a94-0a2d-45f2-9bc4-fe9f4e28ca56`。实际X86/pointer4及X64/pointer8、父过滤参数、Native路径见各 `final-revalidate-{arch}/Results/aot-identity.json`，已全文回读。完整身份/消费内容见同目录 `az-reference-consumption.json`，已回读头部和样本段，未声称人工逐字审阅全部450样本。

## 实跑计数与裁决

| 分组 | x86 | x64 | 判定 |
|---|---|---|---|
| 官方生成 |2/2|2/2|glyph非空及450样本生成断言通过|
| 原4 GlyphOwnerCloses |4/4|4/4|原缺glyph参考，现真正执行原像素断言通过|
| 原3 DrawingImageVisualMask |3/3|3/3|17/0、17/3、18/3，完整Pbgra字节零阈值通过|
| 原6 RotatedDrawingHasNoCallback |6/6|6/6|scaling1/2/3 × tiled false/true，原present失败未重现，原内容断言通过|
| 新参考身份验证 |1/1|1/1|参考官方SHA与同版同架构官方匹配|
| 最终parent总计 |14/14|14/14|Completed，所有非成功计数0|

最终生成TRX：`final-official-{arch}/TestResults/az-final-official-{arch}.trx:40-42`；最终复验TRX：`final-revalidate-{arch}/TestResults/az-final-revalidate-{arch}.trx:101-103`，已回读Counters。生成TRX也随包保留于复验目录，不误算新增执行。

原4 Glyph缺文件是AP历史现场，不改写当时通过。原6 RotatedDrawing是present失败而非缺参考；本轮AP候选新实跑消失，不以reference存在推断通过，不把其根因武断归为缺文件。未做单变量归因证明哪项AP改动消除了历史RotatedDrawing失败。

## 原始内容与未闭

完整原始JSON保留在 `final-official-{arch}/ReferenceData/` 和 `final-revalidate-{arch}/ReferenceData/`；消费证据另保存完整GlyphRaw及三份ConsumedMaskSamples，不是候选推导预期。Arial字体SHA `B3658EADAE55E682B5F69EB64C439C1ECC8F196C0BB8D4756D145D13BC86476A`，glyph36、advance10.671875、em16、origin(2,20)、32×24/Pbgra32、Grayscale/Fixed。两架构mask(17,0)64字节原始回读：首/第三行透明，第二/第四行各4个 `[0,0,128,128]`，非双空白。正式测试自动比较完整表面，不以人工节选代替比较。

本轮是软件官方参考及现有COM目标组验证，不覆盖HW/device-loss/媒体/长期资源。AP251宽族未全跑，不能称251/251；两个正式项目×双架构最后生产源码完整适用full仍未完成。其他代理领域缺口不由本包关闭。无需跨域修改，任务有界结束。
