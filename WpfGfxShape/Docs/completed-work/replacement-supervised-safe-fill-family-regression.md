# JT 安全矩形 fill 相关方法族回归

## JU 终态裁决：扩大族有限验收通过，准入下一轮 default COM 双 full
**可以安排同最后源码的 x64/x86 Release default COM 双 full。** 相关族各358/358/0fail/0skip已经实际完成，JU亲核关键原件；未发现尚未解决、会使本次部署或隔离结果丢失的具体阻断。不要求未来所有路径先绿才能跑full，真实失败必须形成新完整清单，不修改阈值/skip，不以文档历史未全回读另设无限门。保留JR/JS矩形、JN resolver、JD及所有已验收修复；本结论不是完整替换或生产全过。
JU本轮仅文件/API核证与文档修改：生产/测试/项目修改0、build0、publish0、沙箱0、full0、重复业务矩阵0、新Host/工程0。JT下列“进行中”及首次错误包均为历史，不代表本终态。

### 原件位置、运行与实际身份
以下相对路径均以WpfGfxShape根为基准；R=artifacts/jt-safe-fill-family/readback。父TRX为R/family-{arch}/Results/jt-family-{arch}/TestResults/child.trx；两份2288–2291行均亲读Completed、total=executed=passed=358、failed=notExecuted=0。两supervision.json全40行已读，BL120、Reason=exited、Exit0、HasExited/OutputCollected=true，无timeout/cancel/kill/drain/SupervisorError；正常退出只证明本次清理，不证明全部异常路径或长期资源收敛。
|项|x64|x86|
|---|---|---|
|task|1a1020b157484f18b7bcde76709390a3|6e1b7fc6bbe44cb2a6e0a3901231ca70|
|监督PID / 业务PID|7120 / 5348|7764 / 6956|
|test SHA256|CB0B19D4D763B3252533E854CE4A5722602D1454BD5F7CD6677C62AD47896B2D|819C8E1CDAB7F1C49989B0D1B71AE1EB62076EB09A19D79A6FBC4E1AA8A81565|
|test MVID|7f77263f-cbe9-404a-b3ff-927640cfaf7d|45c4a505-b29a-474e-8b5a-75668ed8f639|
|actual Native SHA256|1782797EA3271F9426BFEDFC67F30D42834858C778C3ECADF6A40C1AC692CF9A|1DFB33772ACC5FFCD3A5D9D865867ACA9F0EA33A6DD7A8F59113056098E744DF|
Runtime均.NET10.0.13；Native版本1.0.0.0。R/family-{arch}/Results/jt-loaded-identity.json各14行全读，PID与监督业务一致，actual路径均C:\CodingAgentSandbox\Tasks\对应task\Native\wpfgfx_cor3.dll；这是Process.Modules实际枚举，不是仅文件预期。测试SHA/MVID与监督一致，不能将这些条件包身份移绑到未来default full。

### 358分母、精确旧ID与内容边界
监督过滤器是六个精确完整方法OR，不是全量：CompositionTests的WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked（278个已发现DataRow）、WhenDrawingImageVisualMaskIsRenderedThenCompletePbgraSurfaceMatchesOriginalWpf（3）、WhenDrawingViewboxExpandsWithoutChangingMappingThenFloatViewportIsUnchanged（66）、WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements（1），另WorkingAlphaFirstDivergenceTests.WhenPublicDrawingAlphaIsObservedThenFloatContractIsPreserved（4）和JlPresentInvalidArgumentTests.WhenSafeFillRectangleIsClippedThenCompositionPixelsRemainExact（6），合计358。TRX边界8–285/286–288/289–354/355/447–452及后续WorkingAlpha与源码对应；不把fixture内部调用、450校准样本、180控制、像素数或双方角色重复加到父分母。JU核Counters、目标结果/边界与源码，不声称逐条重算358份原始buffer。
两架构TRX135/136行均亲读：原sourceMode7 ID 1bbc964c-b6e5-8d5d-8d59-fba9c3c3ca06、sourceMode8 ID 1308b204-c717-8327-88c1-71486c5f582a，完整参数(0,False,4294901760,4294901760,0,1,0,0,0,True,True,False,0,7或8)均Passed。精确匹配充分，不从358绿减原93推新full通过数。
代表内容源码已核：JL六行保留原两点0xffff0000断言；VisualMask三项(17,0)/(17,3)/(18,3)比较完整64字节；FloatViewport比较source28/29完整Vector4数组；WorkingAlpha source18/15核两点float合同，source17分支只断言16像素且记录观察，不能将其宣称独立精确float golden通过。x86 working-alpha-18-3-0-62cbf707e530497bb4ad0b05e2c972c5.json已读代表非空0.38像素。运行断言与原件支持有限内容结论，JU没有全部buffer逐像素重算，也没有伪造成功TRX未导出的expected/actual像素。

### 官方输入来源与x86校准收口
官方task x64 cdc5147b048f4feeb5084e3ac354976b、x86 324271aabbe8492ba0a8824b5488bc1c；两TRX各43行亲读，TileFixtures与GlyphReference均Passed、Completed、2/2/0skip。两监督全读均Exit0、无timeout/kill/drain；监督/业务PID分别6364/2064、7708/3716。两actual identity全读：官方SHA x64 AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1、x86 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；版本均10,0,1326,50504 @Commit:4099f83977f4d29604a9aeb971fb0a7bc1d5f699，.NET10.0.13，同架构testSHA/MVID与对应族一致。实际路径在各task根wpfgfx_cor3.dll，不经候选转发。
官方TRX附件为original-wpf-014914c1cda3434d87e4fdde8d48e95e.json / original-wpf-689f027ddff34f56a74721e7f55b5780.json；候选ReferenceData/original-wpf.json头部记录对应官方task、时间、版本、SHA、SoftwareOnly。源码ReferenceBehaviorTests:255–280/394–423直接使用WPF DrawingImage/RenderTargetBitmap，Process.Modules核唯一官方路径，保存GUID文件后复制original-wpf.json；生成450样本，source17/18 affine0/3及22–27 affine0 ×五tile×三scaling×fill/stroke/mask。不是候选生成预期。校准记录消费文件SHA分别23C4098E36F616BD70F8471137094FFC47F9AA35F50E6688BCDE3CE516739A2D、6DC448D07319D74E09A8FF179889E38DAD74510DF1AA900BE28D257AA7E33749；JU核来源字段/消费者记录，未额外工具重算全部附件hash或全样本像素。
x86校准R/family-x86/Results/wpf-calibration-cc1ef581a06e46898d8ff2c277e5d61f.json及diag3874–3904亲读：DirectCompared450/DirectMismatches0，solid control180/0，AOT BitmapSource180/0；original bitmap-vs-DrawingImage source differences30、AOT matches bitmap only0，不能把官方不同source内容混作候选失败。跨格式Mismatches57/FormatVerdictDifferences57真实存在，source17 affine3为31、source18 affine3为26；如(2,1)官方[0,0,22,22]、float投影[0,0,21,21]。x64校准db6b87212de3461fb3f6759de6fd4969亦Direct450/0、投影57。
合同裁决：同格式Pbgra32 GUID…c910的完整4x4/64字节独立官方零阈值适用且通过；Prgba128Float GUID…c91a到Pbgra的自定义投影不是同格式官方float oracle。官方文件Limitation与现有源码CrossFormatContract明确此限制；诊断差未删、未调阈值，不能叫“全部校准零差”或float兼容已闭。它不阻断default full准备；若下一full的适用同格式/float断言失败，照常Failed列入新清单，不为准跑full先要求消灭所有非目标投影诊断。

### 误包、默认源码安全与已存在构建
首次x64 c24f00c3c07c4c41a16000d9ef6dd232，JU亲读R/x64/Results/jt-loaded-identity.json：actual ED34A9C615DB44DDCE7519FEBBFC45884A2303D97AE39CAAF3134D0E99778B42，不能计JR修后验收；历史jr-safe-fill/aot误包保留。正确输入fixed/aot目录存在，修后358实际加载1782797…/1DFB337…原件已绑定，不需再编Native。JS报告保留JR fixed-x64先行1/1及family双7/7历史原件路径，此处不重复运行或冒充JU逐份重审其全部记录。
JtSafeFillIdentity.cs全37行读完：整个类型受#if JT_SAFE_FILL_IDENTITY控制，仅AssemblyCleanup枚举模块/读hash、记录identity；不改生产、渲染、断言、环境或resolver。项目/Directory.Build.props未默认定义该符号，默认full无需开启它。条件包的额外记录不是新的业务通过项。新生产矩形修复未变，仍只在原量化失败后证明单figure直线轴对齐矩形、设备±524287交集再既有量化，不推广curve/stroke/viewport通用clip。
已读JN验收报告及当前Rounded CompareAsync/JmResolverIsolation关键源码：仅JM_ROUNDED_ROLE/METHOD判同exe角色，非WPFGFX_RUN_ROOT；错误角色/方法断言失败，helper有界退出/drain并要求真实唯一TRX方法Passed，缺件/0tests/非零exit不会空过。保留JN历史双3/3与有限边界，不伪称本次358包括resolver族，也未证明所有异常路径。未发现会使本轮full结果必然丢失的具体新缺陷。
当前可读最后构建日志63行，JU读1–12及50–63：默认ComAcceptance.csproj Release、无附加guard/压分析器参数，Exit0、129warnings、0errors、3.02秒；原输出101802字符，前81802被工具截断。仅引用这份既有默认构建摘要，不伪造完整未截断日志，不声称JU新build或solution双架构构建；JS/JN各自构建仍为各自历史。

### 缺件范围与下一唯一任务
仅检查指定artifacts/jk-readonly/readback-complete/Results/jj-full-inventory目录不存在；项目JkReadbackInputs明确引用的artifacts/jh-com-full-convergence/readback/full-x64/Results/jh-full-x64/TestResults/child.trx及artifacts/jj-full-closeout/readback/full-x86/Results/jj-full-x86/TestResults/child.trx均精确读取不存在。此范围说明不是全盘无档结论；不无限寻档、不重复旧full、不宣93逐项重核。两mode旧ID当前Passed有原件即可有限验收。
**下一唯一任务：集成执行最后源码的default COM x64/x86 Release双full，按实际发现分母保存父TRX、完整失败清单、actual身份和监督/子隔离结果并回读。** 默认不启JT/JL/JN历史诊断guard，不硬预设旧1802（新增回归可能增加分母），必要产物准备由下一轮承担；不把条件包testSHA移绑default。允许真实失败，子native隔离仍Failed、不skip；危险官方协议风险保持未修，不以隔离称生产全过。旧1802/1709/93仅历史，IO managed4256两格独立；Available/HWloss/mediaresource/final四格仍开放，完整替换未完成。

## JT 阶段历史（双架构方法族已实际通过，当时正在核准入）
正确JR Native下x64 task1a1020b157484f18b7bcde76709390a3、x86 task6e1b7fc6bbe44cb2a6e0a3901231ca70各358/358pass/0fail/0skip，TRX Counters直接读，监督Exit0无timeout/kill/drain，actual分别1782797…/1DFB337…已核。同架构10.0.13官方各2/2生成新参考后才发布消费；450项直接Pbgra32校准为零差，不把内部循环数加父分母。校准JSON仍含57项跨格式投影诊断差，不能将其消失或当同格式差异。后续完成身份/清理与默认构建后裁决。
已接管 JS/JR 有限安全矩形裁剪与 JN 默认 resolver 隔离，禁止改生产及默认断言。先核执行规则、完整替换方案及 JL/JN/JH 历史，再恢复精确失败清单、按 sourceMode 输入合同选方法过滤，双架构实际结果与身份回读后才裁决 full 准入。

阶段历史：接管时尚未构建、发布或运行；

## 首次身份发现
原93 JSON目录及JH/JJ full TRX精确路径不存在，不再找档/重跑full。首x64 task c24f00c3c07c4c41a16000d9ef6dd232正常Exit2无timeout/kill/drain，但亲读actual ED34A9C615DB44DDCE7519FEBBFC45884A2303D97AE39CAAF3134D0E99778B42不是JR1782797…，不作为JR修后回归。输入误选历史jr-safe-fill/aot，正确fixed/aot的包SHA已核1782797…；原件保留，纠正后再跑，不重编Native。仅新增JT_SAFE_FILL_IDENTITY显式guard只读AssemblyCleanup身份报告，无生产/断言/公共设施修改。ReferenceData缺件，参考方法需同版官方先生成，不能冒10.0.13新对照。

历史双架构 full 各 1802/1709/93/0、Exit2 不变，不相减生成新 full。Available/HWloss/mediaresource/长期资源与最终四格仍开放。
